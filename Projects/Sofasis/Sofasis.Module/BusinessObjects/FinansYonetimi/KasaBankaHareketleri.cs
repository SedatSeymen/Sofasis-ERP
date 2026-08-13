using DevExpress.Data.Extensions;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sofasis.Module.Services;
using static System.Net.Mime.MediaTypeNames;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    [XafDisplayName("Kasa Banka Hareketleri")]
    // Bkz. CariHesapHareketleri'ndeki aynı kural açıklaması.
    [RuleCriteria("Rule_KasaBankaHareketleri_KasaDovizUyumu", DefaultContexts.Save,
        "KasaBankaTanim Is Null Or KasaBankaTanim.DovizTanim = DovizTanim",
        "Seçilen Kasa / Banka Hesabının para birimi, Döviz Kodu ile uyuşmuyor.")]
    public class KasaBankaHareketleri : BaseClassWithAuditAndDescription
    {
        public KasaBankaHareketleri(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            FisTarihi = DateTime.UtcNow.Date;
            VadeTarihi = DateTime.UtcNow.Date;
            BelgeTarihi = DateTime.UtcNow.Date;
            FisNo = Helper.ConstNewRecordText;
        }


        CariKasaBankaTipi cariKasaBankaTipi;
        KasaBankaTanim kasaBankaTanim;
        decimal? yerelAlacakTutar;
        decimal? yerelBorcTutar;
        decimal? dovizKuru;
        DovizTanim dovizTanim;
        decimal? alacakTutar;
        decimal? borcTutar;
        CariHesapTanim cariHesapTanim;
        DateTime belgeTarihi;
        string belgeNo;
        FisTuruTanim fisTuruTanim;
        DateTime vadeTarihi;
        DateTime fisTarihi;
        string fisNo;

        bool sonuc = false;
        // Kasa'nın yanı sıra tüm Banka fiş türleri de dahil (kullanıcı kararı): bir Banka hareketi
        // bir Cari'ye bağlıysa (CariHesapTanim seçiliyse) o Cari'nin "Cari Hesap Hareketleri"
        // ekstresinde de görünmesi gerekir — ekonomik olarak bir Kasa Tahsilatı/Ödemesi ile aynı
        // etkiyi yapar, yalnızca ödeme kanalı (nakit/banka) farklıdır. CariHesapTanim seçili
        // DEĞİLSE (ör. tipik bir Banka Açılış/Yatırılan/Çekilen Para işlemi) ObjectSaving()'deki
        // ayrı bir kontrol mirror oluşturmayı zaten atlıyor — bkz. aşağıdaki açıklama.
        string[] strFisTuru = new string[] { "KSTHSL", "KSODME", "BNACLS", "BNGLNH", "BNGDNH", "BNYATP", "BNCEKP" };

        [Size(16)]
        // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
        // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış
        // pozitif "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
        [RuleUniqueValue(TargetCriteria = "FisNo != '" + Helper.ConstNewRecordText + "'")]
        [Indexed(Unique = true)]
        [XafDisplayName("Fiş No")]
        [Appearance("ED_KasaBankaHareketleri_FisNo",
        Enabled = false, TargetItems = "FisNo", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_KasaBankaHareketleri_FisNo", DefaultContexts.Save, "Lütfen Fiş Numarasını Giriniz...")]
        public string FisNo
        {
            get => fisNo;
            set => SetPropertyValue(nameof(FisNo), ref fisNo, value);
        }

        [XafDisplayName("Fiş Tarihi")]
        [Appearance("ED_KasaBankaHareketleri_FisTarihi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_KasaBankaHareketleri_FisTarihi", DefaultContexts.Save, "Lütfen Fiş Tarihini Giriniz...")]

        public DateTime FisTarihi
        {
            get => fisTarihi;
            set => SetPropertyValue(nameof(FisTarihi), ref fisTarihi, value);
        }

        [XafDisplayName("Vade Tarihi")]
        public DateTime VadeTarihi
        {
            get => vadeTarihi;
            set => SetPropertyValue(nameof(VadeTarihi), ref vadeTarihi, value);
        }

        [XafDisplayName("Fiş Türü")]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [RuleRequiredField("RuleRequired_KasaBankaHareketleri_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
        public FisTuruTanim FisTuruTanim
        {
            get => fisTuruTanim;
            set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
        }


        [Size(13)]
        [XafDisplayName("Belge No")]
        public string BelgeNo
        {
            get => belgeNo;
            set => SetPropertyValue(nameof(BelgeNo), ref belgeNo, value);
        }

        [XafDisplayName("Belge Tarihi")]
        public DateTime BelgeTarihi
        {
            get => belgeTarihi;
            set => SetPropertyValue(nameof(BelgeTarihi), ref belgeTarihi, value);
        }

        [XafDisplayName("Cari Hesap Adı")]
        [Appearance("ED_KasaBankaHareketleri_CariHesapTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [Appearance("SH_KasaBankaHareketleri_CariHesapTanim",
            Visibility = ViewItemVisibility.Hide,
            TargetItems = "CariHesapTanim",
            Criteria = "FisTuruTanim.FisTuruKodu in('KSACLS') ", Context = "Any")]
        // Kasa Tahsilat/Ödeme ve Banka Gelen/Giden Havale, doğası gereği bir kişiden/kurumdan
        // tahsilat veya ona ödemedir — Cari'siz kaydedilmesi (kimden tahsil edildiği bilinmeyen
        // bir hareket) muhasebe bütünlüğünü bozar. Kasa/Banka Açılış, Yatırılan/Çekilen Para gibi
        // doğal bir Cari karşılığı olmayan fiş türleri bu kısıtın dışında bırakıldı.
        [RuleRequiredField("RuleReq_KasaBankaHareketleri_CariHesapTanim_TahsilatOdeme",
            DefaultContexts.Save, "Lütfen Cari Hesap Adını Giriniz...",
            TargetCriteria = "FisTuruTanim.FisTuruKodu in('KSTHSL','KSODME','BNGLNH','BNGDNH')")]
        public CariHesapTanim CariHesapTanim
        {
            get => cariHesapTanim;
            set => SetPropertyValue(nameof(CariHesapTanim), ref cariHesapTanim, value);
        }

        [XafDisplayName("Kasa / Banka Tipi")]
        [ImmediatePostData]
        public CariKasaBankaTipi CariKasaBankaTipi
        {
            get => cariKasaBankaTipi;
            set => SetPropertyValue(nameof(CariKasaBankaTipi), ref cariKasaBankaTipi, value);
        }

        [XafDisplayName("Kasa / Banka Hesap")]
        [RuleRequiredField("RuleReq_KasaBankaHareketleri_KasaBankaTanim",
            DefaultContexts.Save, "Lütfen Kasa Banka Hesap Adını Giriniz...")]
        [DataSourceCriteria("CariKasaBankaTipi = '@this.CariKasaBankaTipi' And DovizTanim = '@this.DovizTanim'")]
        public KasaBankaTanim KasaBankaTanim
        {
            get => kasaBankaTanim;
            set => SetPropertyValue(nameof(KasaBankaTanim), ref kasaBankaTanim, value);
        }

        // Muhasebe düzeltmesi: kriterler ters yazılmıştı (bkz. CariHesapHareketleri.cs'deki
        // aynı düzeltme açıklaması). FinansBorcAlacakTipi: Yok=0, Borc=1, Alacak=2, BorcAlacak=3.
        [XafDisplayName("Borç Tutar")]
        [Appearance("SH_KasaBankaHareketleri_BorcTutar",
            Visibility = ViewItemVisibility.Hide, TargetItems = "BorcTutar",
            Criteria = "FisTuruTanim.FinansBorcAlacakTipi = 2 ", Context = "Any")]
        public decimal? BorcTutar
        {
            get => borcTutar;
            set
            {
                SetPropertyValue(nameof(BorcTutar), ref borcTutar, value);
                CalculateTutar();
            }
        }


        [XafDisplayName("Alacak Tutar")]
        [Appearance("SH_KasaBankaHareketleri_AlacakTutar",
            Visibility = ViewItemVisibility.Hide, TargetItems = "AlacakTutar",
            Criteria = "FisTuruTanim.FinansBorcAlacakTipi = 1 ", Context = "Any")]
        public decimal? AlacakTutar
        {
            get => alacakTutar;
            set
            {
                SetPropertyValue(nameof(AlacakTutar), ref alacakTutar, value);
                CalculateTutar();
            }
        }

        [XafDisplayName("Döviz Kodu")]
        [RuleRequiredField("RuleReq_KasaBankaHareketleri_DovizTanim",
            DefaultContexts.Save, "Lütfen Döviz Kodunu Giriniz...")]
        [Appearance("ED_KasaBankaHareketleri_DovizTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public DovizTanim DovizTanim
        {
            get => dovizTanim;
            set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
        }

        [XafDisplayName("Döviz Kuru")]
        [Appearance("ED_KasaBankaHareketleri_DovizKuru",
        Enabled = false, TargetItems = "DovizKuru", 
            Criteria = "!(IsNewObject(this))",Context = "DetailView")]

        [RuleRequiredField("RuleRequired_KasaBankaHareketleri_DovizKuru", DefaultContexts.Save, "Lütfen Bir Döviz Kuru Giriniz...")]
        public decimal? DovizKuru
        {
            get => dovizKuru;
            set => SetPropertyValue(nameof(DovizKuru), ref dovizKuru, value);
        }

        [XafDisplayName("Yerel Borç Tutar")]
        [Appearance("ED_KasaBankaHareketleri_YerelBorcTutar", Enabled = false, Criteria = "", Context = "Any")]
        [Appearance("SH_KasaBankaHareketleri_YerelBorcTutar",
            Visibility = ViewItemVisibility.Hide, TargetItems = "YerelBorcTutar",
            Criteria = "FisTuruTanim.FinansBorcAlacakTipi = 2 ", Context = "Any")]
        public decimal? YerelBorcTutar
        {
            get => yerelBorcTutar;
            set => SetPropertyValue(nameof(YerelBorcTutar), ref yerelBorcTutar, value);
        }

        [XafDisplayName("Yerel Alacak Tutar")]
        [Appearance("ED_KasaBankaHareketleri_YerelAlacakTutar", Enabled = false, Criteria = "", Context = "Any")]
        [Appearance("SH_KasaBankaHareketleri_YerelAlacakTutar",
            Visibility = ViewItemVisibility.Hide, TargetItems = "YerelAlacakTutar",
            Criteria = "FisTuruTanim.FinansBorcAlacakTipi = 1 ", Context = "Any")]
        public decimal? YerelAlacakTutar
        {
            get => yerelAlacakTutar;
            set => SetPropertyValue(nameof(YerelAlacakTutar), ref yerelAlacakTutar, value);
        }
        private void CalculateTutar()
        {
            if (AlacakTutar > 0)
                YerelAlacakTutar = DovizKuru * AlacakTutar;
            if (BorcTutar > 0)
                YerelBorcTutar = DovizKuru * BorcTutar;

        }
       protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (propertyName == nameof(FisTuruTanim) && newValue != null && !IsLoading)
            {
                // Bkz. CariHesapHareketleri.OnChanged()'deki aynı açıklama — yalnızca o Fiş Türü için
                // aktif bir varsayılan tanımlıysa bir şey yapar, aksi halde hiçbir etkisi yoktur.
                FisTuruVarsayilanlariniUygula((FisTuruTanim)newValue);
            }
            if (propertyName == nameof(CariHesapTanim) && newValue != null && !IsLoading)
            {
                DovizTanim = ((CariHesapTanim)newValue).DovizTanim;
                // Bkz. CariHesapHareketleri.OnChanged()'deki aynı açıklama: kur burada da açıkça
                // uygulanır, yalnızca DovizTanim'in kendi (aşağıdaki) OnChanged bloğuna bırakılmaz —
                // iç içe (nested) çağrı sırasında Blazor'un ekranı kaydetmeden önce güncellemeyi
                // kaçırdığı canlı testte tespit edildi (DB'ye yazılan değer doğruydu, yalnızca anlık
                // ekran tutarsızdı).
                if (DovizTanim != null)
                    DovizKuruGuncelle(DovizTanim);
                CriteriaOperator criteria = CriteriaOperator.Parse("DovizTanim = ? And CariKasaBankaTipi = ?",
                    DovizTanim, CariKasaBankaTipi.Kasa);
                KasaBankaTanim entity = Session.FindObject<KasaBankaTanim>(criteria);
                if (entity != null)
                    KasaBankaTanim = entity;
            }

            if (propertyName == nameof(KasaBankaTanim) && newValue != null && CariHesapTanim == null)
            {
                DovizTanim = ((KasaBankaTanim)newValue).DovizTanim;
            }
            if (propertyName == nameof(DovizTanim) && newValue != null && !IsLoading)
            {
                DovizKuruGuncelle((DovizTanim)newValue);
                // Bkz. CariHesapHareketleri.OnChanged()'deki aynı açıklama: Kasa/Banka hesabı, kendi
                // para biriminden farklı bir Döviz Kodu ile uyumsuz kalamaz.
                if (KasaBankaTanim != null && KasaBankaTanim.DovizTanim != (DovizTanim)newValue)
                {
                    KasaBankaTanim = null;
                }
            }
            CalculateTutar();

        }

        // Bkz. CariHesapHareketleri.cs'deki aynı metod açıklaması.
        void DovizKuruGuncelle(DovizTanim doviz)
        {
            if (doviz.DovizKodu == "TRY")
            {
                DovizKuru = 1;
            }
            else
            {
                DovizGunlukKurM entity = Session.FindObject<DovizGunlukKurM>(new BinaryOperator("KurTarihi", FisTarihi));
                DovizGunlukKurD entitydetail = entity?.DovizGunlukKurDetails.FirstOrDefault(x => x.DovizTanim == doviz);
                if (entitydetail != null)
                    DovizKuru = entitydetail.DovizSatis;
            }
        }
        protected override void OnSaving()
        {
            if (Session.IsNewObject(This) &&
                FisNo == Helper.ConstNewRecordText &&
                FisTuruTanim != null)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                FisNo = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, FisTarihi);
            }
            // D-2/D-6: kendi eşleştirme kodumuz burada (ObjectSaving'den ÖNCE) sabitlenir — bkz.
            // CariHesapHareketleri.OnSaving()'deki aynı açıklama (mükerrer kayıt bug'ı düzeltmesi).
            // NOT: IntegrationSourceEntity BURADA (native/kaynak kayıtta) KASITLI OLARAK set
            // EDİLMEZ — bkz. CariHesapHareketleri.OnSaving()'deki aynı gerekçe (native/ayna ayrımı
            // belirsizleşiyordu). Yalnızca gerçek ayna kayıtlarda ObjectSaving() tarafından set
            // edilir.
            if (IntegrationCode == null &&
                FisTuruTanim != null &&
                Array.Exists(strFisTuru, element => element == FisTuruTanim.FisTuruKodu))
            {
                IntegrationCode = Oid;
            }
            base.OnSaving();
        }



        public override void ObjectSaving()
        {
            sonuc = Array.Exists(strFisTuru, element => element == FisTuruTanim.FisTuruKodu);

            // CariHesapTanim boşsa ayna oluşturulmaz: Banka Açılış/Yatırılan Para/Çekilen Para gibi
            // fiş türleri tipik olarak bir Cari'ye bağlı DEĞİLDİR (iç nakit yönetimi işlemleridir) —
            // bu alan boşken mirror oluşturmaya çalışmak, CariHesapHareketleri.CariHesapTanim'in
            // [RuleRequiredField] kuralına (aynı commit/UnitOfWork içinde) çarpıp kaydı tamamen
            // engellerdi; oysa kaynak ekranda bu alan hiç zorunlu değildir. Cari seçiliyse davranış
            // değişmez (KSTHSL/KSODME için önceden de olduğu gibi mirror oluşur).
            if (sonuc == true && CariHesapTanim != null)
            {
                // D-2/D-6: aynı commit içinde ObjectSaving() birden fazla kez tetiklenirse bile
                // mükerrer kayıt oluşmasın diye önce henüz commit edilmemiş adaylara bakılır.
                CariHesapHareketleri CariHareket =
                    Session.GetObjectsToSave().OfType<CariHesapHareketleri>()
                        .FirstOrDefault(x => x.IntegrationCode == this.IntegrationCode)
                    ?? Session.FindObject<CariHesapHareketleri>(
                        new BinaryOperator(nameof(IntegrationCode), this.IntegrationCode));

                if (CariHareket == null)
                {
                    CariHareket = new CariHesapHareketleri(Session);

                    CariHareket.IntegrationCode = this.IntegrationCode;
                    CariHareket.IntegrationSourceEntity = typeof(KasaBankaHareketleri);

                    CariHareket.FisNo = this.FisNo;
                    CariHareket.FisTarihi = this.FisTarihi;
                    CariHareket.VadeTarihi = this.VadeTarihi;
                    // Cari tarafının kendi Fiş Türü'ne eşlenir (ör. KSODME -> CAODME) — bkz.
                    // CariHesapHareketleri.ObjectSaving()'deki aynı açıklama.
                    CariHareket.FisTuruTanim = KarsiFisTuru(this.FisTuruTanim.FisTuruKodu);
                    CariHareket.CariKasaBankaTipi = this.CariKasaBankaTipi;
                }

                CariHareket.KasaBankaTanim = this.KasaBankaTanim;
                CariHareket.BelgeNo = this.BelgeNo;
                CariHareket.BelgeTarihi = this.BelgeTarihi;
                CariHareket.CariHesapTanim = this.CariHesapTanim;
                // Muhasebe düzeltmesi: bkz. CariHesapHareketleri.ObjectSaving()'deki aynı açıklama —
                // Borç/Alacak TAKAS EDİLEREK kopyalanır (Kasa'nın Borç'u Cari'nin Alacak'ına,
                // Kasa'nın Alacak'ı Cari'nin Borç'una — aynı işlem iki defterde ters yönlüdür).
                CariHareket.BorcTutar = this.AlacakTutar;
                CariHareket.AlacakTutar = this.BorcTutar;
                CariHareket.DovizTanim = this.DovizTanim;
                CariHareket.DovizKuru = this.DovizKuru;
                CariHareket.YerelBorcTutar = this.YerelAlacakTutar;
                CariHareket.YerelAlacakTutar = this.YerelBorcTutar;

                if (Session.IsObjectToSave(this))
                {
                    CariHareket.Save();
                }
            }
        }
        // Bkz. CariHesapHareketleri.cs'deki aynı bayrağın açıklaması.
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [Browsable(false)]
        public bool AynaKaydiSilmeSurecinde { get; private set; }

        public override void ObjectDeleting()
        {
            if (AynaKaydiSilmeSurecinde)
                return;
            AynaKaydiSilmeSurecinde = true;

            sonuc = Array.Exists(strFisTuru, element => element == FisTuruTanim.FisTuruKodu);
            if (sonuc == true && IntegrationCode != null)
            {
                CariHesapHareketleri CariHareket = Session.FindObject<CariHesapHareketleri>(
                    new BinaryOperator(nameof(IntegrationCode), IntegrationCode));
                if (CariHareket != null && !CariHareket.AynaKaydiSilmeSurecinde)
                    Session.Delete(CariHareket);
            }
        }

        // Kasa/Banka fiş türünü Cari tarafındaki kendi eşdeğerine eşler. Eşleme yön bazlıdır (Cari'nin
        // kendi ekstresi için nakit/banka kanalı önemsizdir, yalnızca tahsilat mı ödeme mi olduğu
        // önemlidir): Borç yönlü kaynaklar (KSTHSL, BNGLNH, BNYATP) -> CATHSL (Cari Alacaklanır),
        // Alacak yönlü kaynaklar (KSODME, BNGDNH, BNCEKP) -> CAODME (Cari Borçlanır), Açılış (BNACLS)
        // -> CAACLS (Cari Hesap Açılış, aynı BorcAlacak-karma tip). Eşdeğer bulunamazsa (beklenmeyen
        // durum) kaynağın türü aynen kullanılır.
        FisTuruTanim KarsiFisTuru(string kaynakKodu)
        {
            string karsiKodu = kaynakKodu switch
            {
                "KSTHSL" => "CATHSL",
                "KSODME" => "CAODME",
                "BNACLS" => "CAACLS",
                "BNGLNH" => "CATHSL",
                "BNGDNH" => "CAODME",
                "BNYATP" => "CATHSL",
                "BNCEKP" => "CAODME",
                _ => kaynakKodu
            };
            return Session.FindObject<FisTuruTanim>(new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), karsiKodu))
                ?? this.FisTuruTanim;
        }
    }

}