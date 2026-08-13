using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sofasis.Module.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{
    // Mikro/Netsis emsali: Alış ve Satış faturaları AYRI SINIF DEĞİL — yön (Borç=Alış/Alacak=Satış)
    // FisTuruTanim.FinansBorcAlacakTipi'nden okunur (StokHareketleriM'in kendi StokHareketYonu
    // deseniyle birebir tutarlı). v1.5 kapsamı yalnızca Alış (Borç) yönünü BESLER (Mal Kabul'den
    // otomatik taslak) — Satış Faturası (gelecek faz) AYNI şema/motoru (IVatCalculator, Cari ayna,
    // kilit deseni) hiç değişiklik gerektirmeden kullanacak.
    [DefaultClassOptions]
    [XafDisplayName("Fatura")]
    [RuleCriteria("Rule_FaturaM_EnAzBirSatir", DefaultContexts.Save,
        "FaturaDs.Count > 0", "Lütfen en az bir fatura kalemi ekleyiniz.")]
    // Cari'nin Döviz Kodu ile fatura Döviz Kodu uyuşmalı — CariHesapHareketleri/KasaBankaHareketleri'ndeki
    // aynı kuralın Fatura karşılığı (kullanıcı yanlışlıkla farklı bir döviz seçip Cari ekstresini
    // karıştırmasın diye).
    // Yön uyumu: DataSourceCriteria CariHesap seçimini BİLEREK geniş tutuyor (Tedarikçi/Müşteri/
    // MüşteriTedarikci hepsi seçilebilir — TEK sınıf hem Alış hem Satış yönünü kapsadığından).
    // Ama Borç (Alış) faturasında Müşteri, Alacak (Satış) faturasında Tedarikçi seçilmesi muhasebe
    // hatasıdır (CariHesapHareketleri.Rule_CariHesapHareketleri_KasaDovizUyumu ile birebir aynı
    // save-time doğrulama deseni) — CariHesapTanim.OnDeleting'in aksine burada UI'da değil, kayıt
    // anında engellenir.
    [RuleCriteria("Rule_FaturaM_CariYonUyumu", DefaultContexts.Save,
        "(FisTuruTanim Is Null Or FisTuruTanim.FinansBorcAlacakTipi <> 1 Or CariHesap Is Null Or CariHesap.CariHesapTipi = 'Tedarikci' Or CariHesap.CariHesapTipi = 'MüşteriTedarikci') " +
        "And (FisTuruTanim Is Null Or FisTuruTanim.FinansBorcAlacakTipi <> 2 Or CariHesap Is Null Or CariHesap.CariHesapTipi = 'Musteri' Or CariHesap.CariHesapTipi = 'MüşteriTedarikci')",
        "Alış (Borç) faturasında Tedarikçi/Müşteri-Tedarikçi, Satış (Alacak) faturasında Müşteri/Müşteri-Tedarikçi tipinde bir Cari Hesap seçilmelidir.")]
    public class FaturaM : BaseClassWithAuditAndDescription
    {
        public FaturaM(Session session)
            : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            FaturaTarihi = DateTime.UtcNow.Date;
            FaturaNo = Helper.ConstNewRecordText;
            Durum = FaturaDurumu.Taslak;
        }

        string faturaNo;
        DateTime faturaTarihi;
        FisTuruTanim fisTuruTanim;
        CariHesapTanim cariHesap;
        Type kaynakSiparisTipi;
        Guid? kaynakSiparisOid;
        string tedarikciFaturaNo;
        DateTime? tedarikciFaturaTarihi;
        DovizTanim dovizTanim;
        decimal? dovizKuru;
        decimal kDVHaricToplam;
        decimal kDVToplam;
        decimal tevkifatToplam;
        decimal toplamTutar;
        decimal odenecekTutar;
        decimal yerelToplamTutar;
        decimal yerelOdenecekTutar;
        FaturaDurumu durum;

        [Size(16)]
        // TargetCriteria: bkz. StokHareketleriM.FisNo — aynı çoklu-yeni-kayıt/RuleUniqueValue
        // zamanlama düzeltmesi.
        [RuleUniqueValue(TargetCriteria = "FaturaNo != '" + Helper.ConstNewRecordText + "'")]
        [Indexed(Unique = true)]
        [XafDisplayName("Fatura No")]
        [Appearance("ED_FaturaM_FaturaNo", Enabled = false, TargetItems = "FaturaNo", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_FaturaM_FaturaNo", DefaultContexts.Save, "Lütfen Fatura Numarasını Giriniz...")]
        public string FaturaNo
        {
            get => faturaNo;
            set => SetPropertyValue(nameof(FaturaNo), ref faturaNo, value);
        }

        [XafDisplayName("Fatura Tarihi")]
        [Appearance("ED_FaturaM_FaturaTarihi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_FaturaM_FaturaTarihi", DefaultContexts.Save, "Lütfen Fatura Tarihini Giriniz...")]
        public DateTime FaturaTarihi
        {
            get => faturaTarihi;
            set => SetPropertyValue(nameof(FaturaTarihi), ref faturaTarihi, value);
        }

        // Genel ekranda seçilebilir; fiş-türü-özel ekranlarda (FAALIS/FASTIS) Layout'a hiç
        // yerleştirilmez, NewRecordDefaultsViewController tarafından sabitlenir (Kasa/Stok/SatınAlma
        // ile birebir aynı desen).
        [XafDisplayName("Fiş Türü")]
        [DataSourceCriteria("FinansModulTipi = 'Fatura'")]
        [Appearance("ED_FaturaM_FisTuruTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_FaturaM_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
        public FisTuruTanim FisTuruTanim
        {
            get => fisTuruTanim;
            set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
        }

        [XafDisplayName("Cari Hesap")]
        [DataSourceCriteria("CariHesapTipi = 'Tedarikci' Or CariHesapTipi = 'MüşteriTedarikci' Or CariHesapTipi = 'Musteri'")]
        [Appearance("ED_FaturaM_CariHesap", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_FaturaM_CariHesap", DefaultContexts.Save, "Lütfen Cari Hesap Seçiniz...")]
        public CariHesapTanim CariHesap
        {
            get => cariHesap;
            set => SetPropertyValue(nameof(CariHesap), ref cariHesap, value);
        }

        // StokHareketleriD.KaynakBelgeTipi/Oid'nin birebir portu — polimorfik izlenebilirlik.
        // Bugün yalnızca typeof(SatinAlmaSiparisiM) kullanılır; Satış Faturası geldiğinde
        // typeof(SatisSiparisM) kullanacak, ŞEMA DEĞİŞMEZ.
        [Browsable(false)]
        public Type KaynakSiparisTipi
        {
            get => kaynakSiparisTipi;
            set => SetPropertyValue(nameof(KaynakSiparisTipi), ref kaynakSiparisTipi, value);
        }

        [Browsable(false)]
        public Guid? KaynakSiparisOid
        {
            get => kaynakSiparisOid;
            set => SetPropertyValue(nameof(KaynakSiparisOid), ref kaynakSiparisOid, value);
        }

        // Yalnız Borç (Alış) yönünde anlamlı — CariHesapHareketleri'ndeki Borç/Alacak alan gizleme
        // deseniyle birebir (FinansBorcAlacakTipi: Yok=0,Borc=1,Alacak=2,BorcAlacak=3).
        [Size(32)]
        [XafDisplayName("Tedarikçi Fatura No")]
        [Appearance("SH_FaturaM_TedarikciFaturaNo", Visibility = ViewItemVisibility.Hide,
            TargetItems = "TedarikciFaturaNo", Criteria = "FisTuruTanim.FinansBorcAlacakTipi != 1", Context = "Any")]
        public string TedarikciFaturaNo
        {
            get => tedarikciFaturaNo;
            set => SetPropertyValue(nameof(TedarikciFaturaNo), ref tedarikciFaturaNo, value);
        }

        [XafDisplayName("Tedarikçi Fatura Tarihi")]
        [Appearance("SH_FaturaM_TedarikciFaturaTarihi", Visibility = ViewItemVisibility.Hide,
            TargetItems = "TedarikciFaturaTarihi", Criteria = "FisTuruTanim.FinansBorcAlacakTipi != 1", Context = "Any")]
        public DateTime? TedarikciFaturaTarihi
        {
            get => tedarikciFaturaTarihi;
            set => SetPropertyValue(nameof(TedarikciFaturaTarihi), ref tedarikciFaturaTarihi, value);
        }

        [XafDisplayName("Döviz Kodu")]
        [RuleRequiredField("RuleRequired_FaturaM_DovizTanim", DefaultContexts.Save, "Lütfen Döviz Kodunu Giriniz...")]
        [Appearance("ED_FaturaM_DovizTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public DovizTanim DovizTanim
        {
            get => dovizTanim;
            set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
        }

        [XafDisplayName("Döviz Kuru")]
        [Appearance("ED_FaturaM_DovizKuru", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_FaturaM_DovizKuru", DefaultContexts.Save, "Lütfen Bir Döviz Kuru Giriniz...")]
        public decimal? DovizKuru
        {
            get => dovizKuru;
            set => SetPropertyValue(nameof(DovizKuru), ref dovizKuru, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("KDV Hariç Toplam")]
        [Appearance("ED_FaturaM_KDVHaricToplam", Enabled = false, Context = "Any")]
        public decimal KDVHaricToplam
        {
            get => kDVHaricToplam;
            set => SetPropertyValue(nameof(KDVHaricToplam), ref kDVHaricToplam, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("KDV Toplam")]
        [Appearance("ED_FaturaM_KDVToplam", Enabled = false, Context = "Any")]
        public decimal KDVToplam
        {
            get => kDVToplam;
            set => SetPropertyValue(nameof(KDVToplam), ref kDVToplam, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("Tevkifat Toplam")]
        [Appearance("ED_FaturaM_TevkifatToplam", Enabled = false, Context = "Any")]
        public decimal TevkifatToplam
        {
            get => tevkifatToplam;
            set => SetPropertyValue(nameof(TevkifatToplam), ref tevkifatToplam, value);
        }

        // Faturanın RESMİ toplamı (KDVHaricToplam + KDVToplam) — tedarikçinin kestiği kağıt/e-Fatura
        // üzerinde yazan tutar, tevkifat düşülmeden ÖNCEKİ hâli.
        [DbType("decimal(18,2)")]
        [XafDisplayName("Toplam Tutar")]
        [Appearance("ED_FaturaM_ToplamTutar", Enabled = false, Context = "Any")]
        public decimal ToplamTutar
        {
            get => toplamTutar;
            set => SetPropertyValue(nameof(ToplamTutar), ref toplamTutar, value);
        }

        // Tevkifat kesilen KDV payı tedarikçiye ÖDENMEZ (alıcı sorumlu sıfatıyla beyan eder) — bu
        // yüzden Cari ayna kaydına giden GERÇEK borç ToplamTutar DEĞİL, bu alandır
        // (= ToplamTutar - TevkifatToplam).
        [DbType("decimal(18,2)")]
        [XafDisplayName("Ödenecek Tutar")]
        [Appearance("ED_FaturaM_OdenecekTutar", Enabled = false, Context = "Any")]
        public decimal OdenecekTutar
        {
            get => odenecekTutar;
            set => SetPropertyValue(nameof(OdenecekTutar), ref odenecekTutar, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("Yerel Toplam Tutar")]
        [Appearance("ED_FaturaM_YerelToplamTutar", Enabled = false, Context = "Any")]
        public decimal YerelToplamTutar
        {
            get => yerelToplamTutar;
            set => SetPropertyValue(nameof(YerelToplamTutar), ref yerelToplamTutar, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("Yerel Ödenecek Tutar")]
        [Appearance("ED_FaturaM_YerelOdenecekTutar", Enabled = false, Context = "Any")]
        public decimal YerelOdenecekTutar
        {
            get => yerelOdenecekTutar;
            set => SetPropertyValue(nameof(YerelOdenecekTutar), ref yerelOdenecekTutar, value);
        }

        [XafDisplayName("Durum")]
        [Appearance("ED_FaturaM_Durum", Enabled = false, Context = "Any")]
        public FaturaDurumu Durum
        {
            get => durum;
            set => SetPropertyValue(nameof(Durum), ref durum, value);
        }

        [Association("FaturaM-FaturaDs"), Aggregated]
        [XafDisplayName("Fatura Kalemleri")]
        public XPCollection<FaturaD> FaturaDs
        {
            get { return GetCollection<FaturaD>(nameof(FaturaDs)); }
        }

        // CariHesapHareketleri/KasaBankaHareketleri'nin OnChanged desenindeki birebir port — Cari
        // seçilince döviz miras alınır, kur otomatik güncellenir.
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (propertyName == nameof(CariHesap) && newValue != null && !IsLoading)
            {
                DovizTanim = ((CariHesapTanim)newValue).DovizTanim;
                if (DovizTanim != null)
                    DovizKuruGuncelle(DovizTanim);
            }
            if (propertyName == nameof(DovizTanim) && newValue != null && !IsLoading)
            {
                DovizKuruGuncelle((DovizTanim)newValue);
            }
        }

        void DovizKuruGuncelle(DovizTanim doviz)
        {
            if (doviz.DovizKodu == "TRY")
            {
                DovizKuru = 1;
            }
            else
            {
                DovizGunlukKurM entity = Session.FindObject<DovizGunlukKurM>(new BinaryOperator("KurTarihi", FaturaTarihi));
                DovizGunlukKurD entitydetail = entity?.DovizGunlukKurDetails.FirstOrDefault(x => x.DovizTanim == doviz);
                if (entitydetail != null)
                    DovizKuru = entitydetail.DovizSatis;
            }
        }

        protected override void OnSaving()
        {
            if (Session.IsNewObject(This) &&
                FaturaNo == Helper.ConstNewRecordText &&
                FisTuruTanim != null)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                FaturaNo = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, FaturaTarihi);
            }

            // FaturaKilitleController zaten kayıttan SONRA faturayı tamamen immutable yapıyor
            // (StokHareketleriM ile aynı "muhasebe defteri" felsefesi) — Durum bu kilitle
            // senkron tutulur: ilk kayıt anı = Onaylandı. Ayrı bir onay ekranı/iş akışı YOK
            // (YAGNI — Satın Alma Talebi'ndeki gibi çok adımlı onay burada gerekmiyor, kilit
            // zaten fiilen onay yerine geçiyor); bu satır olmadan Durum hep Taslak'ta kalıp
            // yanıltıcı bir alan olurdu.
            if (Session.IsNewObject(This))
            {
                Durum = FaturaDurumu.Onaylandi;
            }

            base.OnSaving();
        }

        public override void ObjectSaving()
        {
            // Satırların (FaturaD.ObjectSaving zaten çalışmış — XPO alt-nesneleri üstten önce
            // kaydeder) tutarlarını topla.
            KDVHaricToplam = FaturaDs.Sum(x => x.KDVHaricTutar);
            KDVToplam = FaturaDs.Sum(x => x.KDVTutar);
            TevkifatToplam = FaturaDs.Sum(x => x.TevkifatTutar);
            ToplamTutar = KDVHaricToplam + KDVToplam;
            OdenecekTutar = ToplamTutar - TevkifatToplam;
            YerelToplamTutar = System.Math.Round(DovizKuru.GetValueOrDefault() * ToplamTutar, 2, System.MidpointRounding.AwayFromZero);
            YerelOdenecekTutar = System.Math.Round(DovizKuru.GetValueOrDefault() * OdenecekTutar, 2, System.MidpointRounding.AwayFromZero);

            CariAynaKaydiOlustur();
            KaynakSiparisDurumunuGuncelle();
        }

        // KasaBankaHareketleri.ObjectSaving()'in birebir portu: Cari tarafına doğrudan alan
        // kopyalama ile ayna kaydı oluşturulur (CariHareket'in KENDİ hesaplamasına güvenilmez).
        void CariAynaKaydiOlustur()
        {
            if (CariHesap == null || FisTuruTanim == null) return;

            CariHesapHareketleri CariHareket =
                Session.GetObjectsToSave().OfType<CariHesapHareketleri>()
                    .FirstOrDefault(x => x.IntegrationCode == this.Oid)
                ?? Session.FindObject<CariHesapHareketleri>(
                    new BinaryOperator(nameof(CariHesapHareketleri.IntegrationCode), this.Oid));

            bool yeniKayit = CariHareket == null;
            if (yeniKayit)
            {
                CariHareket = new CariHesapHareketleri(Session)
                {
                    IntegrationCode = this.Oid,
                    IntegrationSourceEntity = typeof(FaturaM)
                };
            }

            CariHareket.FisNo = this.FaturaNo;
            CariHareket.FisTarihi = this.FaturaTarihi;
            CariHareket.VadeTarihi = this.FaturaTarihi;
            CariHareket.FisTuruTanim = KarsiFisTuru(this.FisTuruTanim.FisTuruKodu);
            CariHareket.BelgeNo = this.TedarikciFaturaNo;
            CariHareket.BelgeTarihi = this.TedarikciFaturaTarihi ?? this.FaturaTarihi;
            CariHareket.CariHesapTanim = this.CariHesap;
            CariHareket.DovizTanim = this.DovizTanim;
            CariHareket.DovizKuru = this.DovizKuru;

            // Yön: Borç (Alış) -> Cari'de Alacaklanır BİZ DEĞİL, biz tedarikçiye BORÇLANIRIZ, yani
            // Cari kaydında BorcTutar. Alacak (Satış) -> müşteri bize borçlanır, Cari'de AlacakTutar.
            // (CariHesapHareketleri "Borç Tutar" alanı, o Cari'nin bize olan borcunu değil, BİZİM o
            // Cari'ye olan borcumuzu/alacağımızı tuttuğu için — bkz. CariHesapHareketleri.cs Appearance
            // kriterleri; burada doğrudan kaynağın kendi yönü kopyalanır, KasaBankaHareketleri'ndeki
            // "takas" YAPILMAZ çünkü Fatura zaten Cari'nin karşı tarafı değil, Cari'nin KENDİSİDİR.)
            if (this.FisTuruTanim.FinansBorcAlacakTipi == FinansBorcAlacakTipi.Borc)
            {
                CariHareket.BorcTutar = this.OdenecekTutar;
                CariHareket.AlacakTutar = null;
                CariHareket.YerelBorcTutar = this.YerelOdenecekTutar;
                CariHareket.YerelAlacakTutar = null;
            }
            else
            {
                CariHareket.AlacakTutar = this.OdenecekTutar;
                CariHareket.BorcTutar = null;
                CariHareket.YerelAlacakTutar = this.YerelOdenecekTutar;
                CariHareket.YerelBorcTutar = null;
            }

            if (Session.IsObjectToSave(this))
                CariHareket.Save();
        }

        // Fatura fiş türünü Cari tarafındaki karşılığına eşler (StokHareketleriM'in fiş-türü şeması
        // gibi — bkz. Resources/Seed/fis-turleri.csv: FAALIS->CAALIS, FASTIS->CASTIS).
        FisTuruTanim KarsiFisTuru(string kaynakKodu)
        {
            string karsiKodu = kaynakKodu switch
            {
                "FAALIS" => "CAALIS",
                "FAALID" => "CAALIS",
                "FASTIS" => "CASTIS",
                "FASTID" => "CASTIS",
                _ => kaynakKodu
            };
            return Session.FindObject<FisTuruTanim>(new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), karsiKodu))
                ?? this.FisTuruTanim;
        }

        // Yalnız Alış (Borç) yönünde ve yalnız SatinAlmaSiparisiM kaynaklıyken çalışır — kaynağa
        // bağlı TÜM Mal Kabul satırları (bu fatura dahil) artık bir FaturaD'ye sahipse Durum
        // Faturalandı'ya geçer (kısmi teslimatta N Mal Kabul'ün HEPSİ faturalanana kadar geçmez).
        void KaynakSiparisDurumunuGuncelle()
        {
            if (KaynakSiparisTipi != typeof(SatinAlmaSiparisiM) || KaynakSiparisOid == null) return;

            SatinAlmaSiparisiM siparis = Session.GetObjectByKey<SatinAlmaSiparisiM>(KaynakSiparisOid.Value);
            if (siparis == null) return;

            // Performans: StokHareketleriD/FaturaD tablolarının TAMAMINI (tüm siparişler/faturalar)
            // belleğe çekmek yerine, bu siparişin satır Oid'leriyle sınırlı bir IN filtresi doğrudan
            // SQL'e itilir — sonuç kümesi bu siparişin/mal kabulünün satır sayısıyla sınırlı kalır.
            List<Guid> siparisSatirOidleri = siparis.SatinAlmaSiparisiDs.Select(x => x.Oid).ToList();
            if (siparisSatirOidleri.Count == 0) return;

            var malKabulSatirlari = new XPQuery<StokHareketleriD>(Session)
                .Where(x => x.KaynakBelgeTipi == typeof(SatinAlmaSiparisiD)
                    && x.KaynakBelgeOid != null
                    && siparisSatirOidleri.Contains(x.KaynakBelgeOid.Value))
                .ToList();
            if (malKabulSatirlari.Count == 0) return;

            List<Guid> malKabulSatirOidleri = malKabulSatirlari.Select(x => x.Oid).ToList();
            var faturalananOidler = new XPQuery<FaturaD>(Session)
                .Where(x => x.KaynakStokHareketiD != null && malKabulSatirOidleri.Contains(x.KaynakStokHareketiD.Oid))
                .Select(x => x.KaynakStokHareketiD.Oid)
                .ToHashSet();
            foreach (FaturaD f in Session.GetObjectsToSave().OfType<FaturaD>())
                if (f.KaynakStokHareketiD != null)
                    faturalananOidler.Add(f.KaynakStokHareketiD.Oid);

            if (malKabulSatirlari.All(x => faturalananOidler.Contains(x.Oid)))
                siparis.Durum = SatinAlmaSiparisDurumu.Faturalandi;
        }
    }
}
