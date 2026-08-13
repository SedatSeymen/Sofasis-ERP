using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;
using Sofasis.Module.Extensions;
using Sofasis.Module.Services;

namespace Sofasis.Module.BusinessObjects
{
    // Satır ("ne/ne kadar"): motor orkestrasyonunun tamamı burada — ağırlıklı ortalama maliyet
    // (IWeightedAverageCostService), StokBakiye bul-veya-oluştur, negatif stok politikası, transfer
    // kaynaklı satırların doğrudan silinmesine karşı koruma ve silme-sonrası tam geçmiş replay'i.
    // BaseClassWithAuditAndDescription'dan türer (SatisSiparisD'nin aksine bare BaseClass DEĞİL) —
    // çünkü silme-sonrası yeniden hesaplama CreatedDate'e göre kayıt sırasını gerektirir.
    [DefaultClassOptions]
    [XafDisplayName("Stok Hareketi Satırı")]
    [Appearance("NegatifBakiyeUyarisiColor", AppearanceItemType = "ViewItem", TargetItems = "*",
        Criteria = "NegatifBakiyeUyarisi = True", Context = "ListView",
        FontColor = "Red", FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]
    public class StokHareketleriD : BaseClassWithAuditAndDescription
    {
        public StokHareketleriD(Session session)
            : base(session)
        {
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [Association("StokHareketleriM-StokHareketleriDs")]
        [RuleRequiredField("RuleRequired_StokHareketleriD_StokHareketleriM", DefaultContexts.Save, "Satır bir fiş başlığına bağlı olmalıdır.")]
        public StokHareketleriM StokHareketleriM { get; set; }

        StokTanim stokTanim;
        decimal miktar;
        decimal birimMaliyet;
        decimal toplamMaliyet;
        DovizTanim dovizTanim;
        decimal? dovizKuru;
        decimal yerelBirimMaliyet;
        decimal yerelToplamMaliyet;
        bool negatifBakiyeUyarisi;
        Type kaynakBelgeTipi;
        Guid? kaynakBelgeOid;
        bool motorIslendi;

        [XafDisplayName("Stok")]
        [Appearance("ED_StokHareketleriD_StokTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "Any")]
        [RuleRequiredField("RuleRequired_StokHareketleriD_StokTanim", DefaultContexts.Save, "Lütfen Stok Seçiniz...")]
        public StokTanim StokTanim
        {
            get => stokTanim;
            set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
        }

        [DbType("decimal(18,4)")]
        [XafDisplayName("Miktar")]
        [Appearance("ED_StokHareketleriD_Miktar", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "Any")]
        [RuleValueComparison("Rule_StokHareketleriD_Miktar_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
            CustomMessageTemplate = "Lütfen miktarı sıfırdan büyük giriniz.")]
        public decimal Miktar
        {
            get => miktar;
            set => SetPropertyValue(nameof(Miktar), ref miktar, value);
        }

        // Giriş satırlarında kullanıcı girer — StokTanim'in kendi DovizTanim'ine göre (aşağıya
        // bkz.) döviz bazlı olabilir; motorun okuduğu gerçek TL değeri YerelBirimMaliyet'tir, bu
        // alan DEĞİL (kullanıcı canlı testte "USD ile girsem sistem TL mi sayacak, belirsiz" diye
        // yakaladı — artık ekranda hangi para biriminde girildiği DovizTanim sembolüyle açık).
        // Çıkış satırlarında motor kaydetme anında StokTanim.OrtalamaMaliyet'e (TL) sabitler —
        // kullanıcı çıkış maliyeti seçmez.
        [DbType("decimal(28,6)")]
        [XafDisplayName("Birim Maliyet")]
        [Appearance("ED_StokHareketleriD_BirimMaliyet", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "Any")]
        public decimal BirimMaliyet
        {
            get => birimMaliyet;
            set
            {
                SetPropertyValue(nameof(BirimMaliyet), ref birimMaliyet, value);
                CalculateYerelMaliyet();
            }
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("Toplam Maliyet")]
        [Appearance("ED_StokHareketleriD_ToplamMaliyet", Enabled = false, Context = "Any")]
        public decimal ToplamMaliyet
        {
            get => toplamMaliyet;
            set => SetPropertyValue(nameof(ToplamMaliyet), ref toplamMaliyet, value);
        }

        [XafDisplayName("Döviz Kodu")]
        [Appearance("ED_StokHareketleriD_DovizTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "Any")]
        public DovizTanim DovizTanim
        {
            get => dovizTanim;
            set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
        }

        [XafDisplayName("Döviz Kuru")]
        [Appearance("ED_StokHareketleriD_DovizKuru", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "Any")]
        [VisibleInListView(false)]
        public decimal? DovizKuru
        {
            get => dovizKuru;
            set
            {
                SetPropertyValue(nameof(DovizKuru), ref dovizKuru, value);
                CalculateYerelMaliyet();
            }
        }

        [DbType("decimal(28,6)")]
        [XafDisplayName("Yerel Birim Maliyet")]
        [Appearance("ED_StokHareketleriD_YerelBirimMaliyet", Enabled = false, Context = "Any")]
        public decimal YerelBirimMaliyet
        {
            get => yerelBirimMaliyet;
            set => SetPropertyValue(nameof(YerelBirimMaliyet), ref yerelBirimMaliyet, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("Yerel Toplam Maliyet")]
        [Appearance("ED_StokHareketleriD_YerelToplamMaliyet", Enabled = false, Context = "Any")]
        [VisibleInListView(false)]
        public decimal YerelToplamMaliyet
        {
            get => yerelToplamMaliyet;
            set => SetPropertyValue(nameof(YerelToplamMaliyet), ref yerelToplamMaliyet, value);
        }

        // Kaydedildiği anda ilgili depoda bakiye negatife düştüyse (NegatifStokPolitikasi.Uyar)
        // işaretlenir; listede/formda bu satırı kırmızı/kalın vurgulamak için kullanılır.
        [XafDisplayName("Negatif Bakiye Uyarısı")]
        [VisibleInDetailView(false)]
        public bool NegatifBakiyeUyarisi
        {
            get => negatifBakiyeUyarisi;
            set => SetPropertyValue(nameof(NegatifBakiyeUyarisi), ref negatifBakiyeUyarisi, value);
        }

        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [HideInUI(HideInUI.All)]
        [Browsable(false)]
        [XafDisplayName("Kaynak Belge Tipi")]
        [ValueConverter(typeof(TypeToStringConverter)), ImmediatePostData]
        [TypeConverter(typeof(LocalizedClassInfoTypeConverter))]
        public Type KaynakBelgeTipi
        {
            get => kaynakBelgeTipi;
            set => SetPropertyValue(nameof(KaynakBelgeTipi), ref kaynakBelgeTipi, value);
        }

        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [HideInUI(HideInUI.All)]
        [Browsable(false)]
        [XafDisplayName("Kaynak Belge Oid")]
        public Guid? KaynakBelgeOid
        {
            get => kaynakBelgeOid;
            set => SetPropertyValue(nameof(KaynakBelgeOid), ref kaynakBelgeOid, value);
        }

        // Motor mantığının EN FAZLA BİR KEZ çalıştığını garanti eder. Master-Detail popup akışında
        // (StokHareketleriM henüz yeniyken StokHareketleriDs'e satır eklenip "Kaydet"/"Kaydet ve
        // Yeni" ile popup kapatılması) XAF her popup kapanışında TÜM ObjectSpace'i flush eder; bu,
        // aynı "yeni" satır için Session.Save()'in — dolayısıyla bu ObjectSaving() override'ının —
        // BaşTA IsNewObject(this) hâlâ true DÖNSE BİLE birden fazla kez tetiklenmesine yol açar
        // (canlı testte doğrulandı: iki satırlı bir fiş kaydedilirken StokBakiye'ye aynı satır için
        // ikinci kez insert denenip UNIQUE INDEX ihlali oluştu). Session.IsNewObject tek başına
        // güvenilir değil — kalıcı (persisted) bir bayrak gerekiyor ki nested/parent ObjectSpace
        // birleşmelerinde de hayatta kalsın.
        [Browsable(false)]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        public bool MotorIslendi
        {
            get => motorIslendi;
            set => SetPropertyValue(nameof(MotorIslendi), ref motorIslendi, value);
        }

        void CalculateYerelMaliyet()
        {
            if (BirimMaliyet > 0)
                YerelBirimMaliyet = DovizKuru.GetValueOrDefault() * BirimMaliyet;
        }

        // CariHesapHareketleri.OnChanged'in StokHareketleriD portu: StokTanim seçilince o stoğun
        // kendi para birimi miras alınır (kullanıcı isterse elle değiştirebilir — aynı stoktan bazen
        // TL bazlı yerel tedarikçiden, bazen döviz bazlı ithalattan girilebilir). DovizTanim'e
        // declarative [RuleRequiredField] KASITLI OLARAK eklenmedi: XAF Validation (Committing),
        // ObjectSaving()'den ÖNCE çalışır — Çıkış satırında kullanıcı hiç döviz seçmez (motor
        // ObjectSaving()'de TRY'ye sabitler), koşulsuz bir RuleRequiredField bu satırın hiç
        // kaydedilememesine yol açardı. Bunun yerine Giriş dalında ObjectSaving() içinde
        // YerelBirimMaliyet<=0 kontrolü yapılır (bkz. aşağı) — hem "döviz seçilmedi" hem "kur
        // bulunamadı" durumunu tek noktada, kullanıcı dostu mesajla yakalar.
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (propertyName == nameof(StokTanim) && newValue != null && !IsLoading)
            {
                DovizTanim = ((StokTanim)newValue).DovizTanim;
                if (DovizTanim != null)
                    DovizKuruGuncelle(DovizTanim);
            }
            if (propertyName == nameof(DovizTanim) && newValue != null && !IsLoading)
            {
                DovizKuruGuncelle((DovizTanim)newValue);
            }
            CalculateYerelMaliyet();
        }

        // CariHesapHareketleri.DovizKuruGuncelle'nin birebir portu — TEK FARK: StokHareketleriD'nin
        // kendi FisTarihi alanı yok, bu yüzden StokHareketleriM.FisTarihi okunur (Master henüz
        // atanmamışsa sessizce çıkılır).
        void DovizKuruGuncelle(DovizTanim doviz)
        {
            if (StokHareketleriM == null) return;

            if (doviz.DovizKodu == "TRY")
            {
                DovizKuru = 1;
            }
            else
            {
                DovizGunlukKurM entity = Session.FindObject<DovizGunlukKurM>(new BinaryOperator("KurTarihi", StokHareketleriM.FisTarihi));
                DovizGunlukKurD entitydetail = entity?.DovizGunlukKurDetails.FirstOrDefault(x => x.DovizTanim == doviz);
                if (entitydetail != null)
                    DovizKuru = entitydetail.DovizSatis;
            }
        }

        public override void ObjectSaving()
        {
            if (MotorIslendi)
                return; // Motor bu satır için zaten çalıştı — tekrar flush edilse de yeniden çalışmaz.

            if (StokHareketleriM == null)
                throw new UserFriendlyException("Stok hareketi bir fiş başlığına bağlı olmalıdır.");
            if (StokHareketleriM.DepoTanim == null)
                throw new UserFriendlyException("Lütfen fiş başlığında Depo seçiniz.");
            if (StokHareketleriM.FisTuruTanim == null)
                throw new UserFriendlyException("Lütfen fiş başlığında Fiş Türü seçiniz.");
            if (StokTanim == null)
                throw new UserFriendlyException("Lütfen Stok seçiniz.");
            if (Miktar <= 0)
                throw new UserFriendlyException("Lütfen miktarı sıfırdan büyük giriniz.");

            StokHareketYonu yon = StokHareketleriM.FisTuruTanim.StokHareketYonu;
            if (yon == StokHareketYonu.Yok)
                throw new UserFriendlyException("Seçilen fiş türü bir stok hareketi üretmek için tanımlanmamış.");

            DepoTanim depo = StokHareketleriM.DepoTanim;
            IWeightedAverageCostService maliyetServisi = new WeightedAverageCostService();

            StokBakiye bakiye = Session.FindObject<StokBakiye>(new GroupOperator(GroupOperatorType.And,
                new BinaryOperator(nameof(StokBakiye.StokTanim), StokTanim),
                new BinaryOperator(nameof(StokBakiye.DepoTanim), depo)));
            if (bakiye == null)
            {
                bakiye = new StokBakiye(Session) { StokTanim = StokTanim, DepoTanim = depo, Miktar = 0 };
            }

            if (yon == StokHareketYonu.Giris)
            {
                if (BirimMaliyet <= 0)
                    throw new UserFriendlyException("Lütfen giriş için birim maliyeti sıfırdan büyük giriniz.");
                if (YerelBirimMaliyet <= 0)
                    throw new UserFriendlyException("Döviz kuru bulunamadı veya birim maliyetin TL karşılığı sıfır — lütfen Döviz Kodu/Kuru bilgisini kontrol ediniz.");

                // Motor HER ZAMAN TL (Yerel) okur — BirimMaliyet döviz bazlı olabilir, ağırlıklı
                // ortalama maliyet farklı dövizlerden giren stoklarda bile tutarlı kalır.
                (decimal yeniMiktar, decimal yeniOrtalama) = maliyetServisi.GirisUygula(
                    StokTanim.ToplamMiktar, StokTanim.OrtalamaMaliyet, Miktar, YerelBirimMaliyet);

                StokTanim.ToplamMiktar = yeniMiktar;
                StokTanim.OrtalamaMaliyet = yeniOrtalama;
                bakiye.Miktar += Miktar;
            }
            else
            {
                // Çıkışta ortalama maliyet ASLA değişmez (ADR-005) — o anki ortalamaya sabitlenir.
                // Döviz alanları da TRY/1'e sabitlenir: motor CikisUygula'da zaten BirimMaliyet
                // okumuyor (yalnızca miktar), bu yalnızca Giriş/Çıkış satırlarının ekranda aynı
                // alan setini anlamlı doldurması içindir — kullanıcı çıkışta hiç döviz seçmez.
                BirimMaliyet = StokTanim.OrtalamaMaliyet;
                DovizTanim = Session.FindObject<DovizTanim>(new BinaryOperator(nameof(BusinessObjects.DovizTanim.DovizKodu), "TRY"));
                DovizKuru = 1;
                YerelBirimMaliyet = BirimMaliyet;

                (decimal yeniMiktar, _) = maliyetServisi.CikisUygula(
                    StokTanim.ToplamMiktar, StokTanim.OrtalamaMaliyet, Miktar);

                StokTanim.ToplamMiktar = yeniMiktar;
                bakiye.Miktar -= Miktar;

                if (bakiye.Miktar < 0)
                {
                    NegatifStokPolitikasi politika = Session.GetSingleton<StokParametre>()?.NegatifStokPolitikasi
                        ?? NegatifStokPolitikasi.Uyar;
                    if (politika == NegatifStokPolitikasi.Engelle)
                        throw new UserFriendlyException($"'{depo.DepoAdi}' deposunda '{StokTanim.StokAdi}' için yeterli bakiye yok.");
                    if (politika == NegatifStokPolitikasi.Uyar)
                        NegatifBakiyeUyarisi = true;
                }
            }

            ToplamMaliyet = Math.Round(Miktar * BirimMaliyet, 2, MidpointRounding.AwayFromZero);
            YerelToplamMaliyet = Math.Round(Miktar * YerelBirimMaliyet, 2, MidpointRounding.AwayFromZero);
            MotorIslendi = true;
        }

        protected override void OnDeleting()
        {
            // NOT: Transfer-kaynaklı satırların DOĞRUDAN silinmesine karşı veri-katmanı guard'ı
            // (bayrak tabanlı da, "kaynak belge hâlâ var mı" sorgusu tabanlı da) canlı testte
            // KARARSIZ çıktı — StokTransferi.ObjectDeleting() içindeki Session.Delete(Master)
            // çağrısının OnDeleting/ObjectDeleting zincirini XPO'nun TAM OLARAK hangi Session/commit
            // aşamasında tetiklediği güvenilir şekilde gözlemlenemedi; bu satırın KENDİ silme
            // sürecini bile (transfer meşru şekilde silinirken) yanlışlıkla engelliyordu. Koruma
            // bunun yerine UI seviyesinde uygulanır: genel "Stok Hareketleri" ekranındaki (tek erişim
            // noktası — STTRCK/STTRGR için özel ekran yok) embedded satır listesi salt-okunurdur
            // (bkz. Model.DesignedDiffs.xafml, StokHareketleriM_StokHareketleriDs_ListView).
            base.OnDeleting();
        }

        public override void ObjectDeleting()
        {
            DepoTanim depo = StokHareketleriM.DepoTanim;
            StokHareketYonu yon = StokHareketleriM.FisTuruTanim.StokHareketYonu;

            StokBakiye bakiye = Session.FindObject<StokBakiye>(new GroupOperator(GroupOperatorType.And,
                new BinaryOperator(nameof(StokBakiye.StokTanim), StokTanim),
                new BinaryOperator(nameof(StokBakiye.DepoTanim), depo)));
            if (bakiye != null)
                bakiye.Miktar += yon == StokHareketYonu.Giris ? -Miktar : Miktar;

            // Global ortalama, bu StokTanim'in TÜM kalan hareketlerinin CreatedDate sırasıyla
            // yeniden oynatılmasıyla (replay) hesaplanır — silinen satır rastgele bir geçmiş kayıt
            // olabilir, yalnızca son kaydı geri almak yanlış sonuç üretir.
            // NOT: Bu satır bir Depo Transferi'nden geliyorsa (KaynakBelgeTipi/Oid dolu), AYNI
            // transferin KARŞI satırı (kardeşi) da StokTransferi.ObjectDeleting() tarafından AYNI
            // anda silinme sürecinde — henüz DB'den silinmemiş olsa da nihai durumda var olmayacak.
            // Kardeş satır replay'e dahil edilirse hem yanlış sonuç üretir hem de o satırın Master'ı
            // paralel silme sürecinde olabileceğinden NullReferenceException'a yol açar (canlı testte
            // doğrulandı). Bu yüzden aynı kaynak belgeye ait diğer satırlar replay dışında tutulur.
            var kalanHareketler = new XPQuery<StokHareketleriD>(Session)
                .Where(x => x.StokTanim == StokTanim && x.Oid != Oid)
                .ToList()
                .Where(x => !(KaynakBelgeTipi != null && x.KaynakBelgeTipi == KaynakBelgeTipi && x.KaynakBelgeOid == KaynakBelgeOid))
                .OrderBy(x => x.CreatedDate)
                .Select(x => (
                    Giris: x.StokHareketleriM.FisTuruTanim.StokHareketYonu == StokHareketYonu.Giris,
                    x.Miktar,
                    BirimMaliyet: x.YerelBirimMaliyet)); // motor her zaman TL (Yerel) okur

            IWeightedAverageCostService maliyetServisi = new WeightedAverageCostService();
            (decimal yeniToplamMiktar, decimal yeniOrtalama) = maliyetServisi.YenidenHesapla(kalanHareketler);

            StokTanim.ToplamMiktar = yeniToplamMiktar;
            StokTanim.OrtalamaMaliyet = yeniOrtalama;
        }
    }
}
