using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
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

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    [XafDisplayName("Cari Hesap Hareketleri")]
    // Savunma katmanı: DataSourceCriteria yalnızca UI seçimini kısıtlar; bu kural, API/import gibi
    // UI dışı yollarla da bir Kasa/Banka hesabına kendi para biriminden farklı bir Döviz Kodu ile
    // hareket kaydedilmesini engeller (bkz. KasaBankaTanim DataSourceCriteria ve OnChanged'deki
    // otomatik temizleme).
    [RuleCriteria("Rule_CariHesapHareketleri_KasaDovizUyumu", DefaultContexts.Save,
        "KasaBankaTanim Is Null Or KasaBankaTanim.DovizTanim = DovizTanim",
        "Seçilen Kasa / Banka Hesabının para birimi, Döviz Kodu ile uyuşmuyor.")]
    public class CariHesapHareketleri : BaseClassWithAuditAndDescription
    { 
        public CariHesapHareketleri(Session session)
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
        string[] strFisTuru = new string[] { "CATHSL", "CAODME" };

        [Size(16)]
        // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
        // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış
        // pozitif "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
        [RuleUniqueValue(TargetCriteria = "FisNo != '" + Helper.ConstNewRecordText + "'")]
        [Indexed(Unique = true)]
        [XafDisplayName("Fiş No")]
        [Appearance("ED_CariHesapHareketleri_FisNo",
        Enabled = false, TargetItems = "FisNo", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_CariHesapHareketleri_FisNo", DefaultContexts.Save, "Lütfen Fiş Numarasını Giriniz...")]
        public string FisNo
        {
            get => fisNo;
            set => SetPropertyValue(nameof(FisNo), ref fisNo, value);
        }

        [XafDisplayName("Fiş Tarihi")]
        [Appearance("ED_CariHesapHareketleri_FisTarihi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_CariHesapHareketleri_FisTarihi", DefaultContexts.Save, "Lütfen Fiş Tarihini Giriniz...")]

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
        [RuleRequiredField("RuleRequired_CariHesapHareketleri_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
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
        [RuleRequiredField("RuleReq_CariHesapHareketleri_CariHesapTanim",
            DefaultContexts.Save, "Lütfen Cari Hesap Adını Giriniz...")]
        [Appearance("ED_CariHesapHareketleri_CariHesapTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]

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
        [Appearance("SH_CariHesapHareketleri_KasaBankaTanim", 
            Visibility = ViewItemVisibility.Hide, 
            TargetItems = "KasaBankaTanim",
            Criteria = "FisTuruTanim.FisTuruKodu in('CAACLS','CABDKN','CAADKN') ", Context = "Any")]
        [DataSourceCriteria("CariKasaBankaTipi = '@this.CariKasaBankaTipi' And DovizTanim = '@this.DovizTanim'")]
        public KasaBankaTanim KasaBankaTanim
        {
            get => kasaBankaTanim;
            set => SetPropertyValue(nameof(KasaBankaTanim), ref kasaBankaTanim, value);
        }

        // Muhasebe düzeltmesi: kriterler ters yazılmıştı (Borç tipi fişte Alacak alanı,
        // Alacak tipi fişte HER İKİ alan birden görünüyordu — Tahsilat fişinde kullanıcı
        // yanlışlıkla Borç Tutar'a veri girebiliyordu). FinansBorcAlacakTipi: Yok=0, Borc=1,
        // Alacak=2, BorcAlacak=3. Artık alan adı, fiş türünün kendi tipiyle eşleşiyor:
        // Borç tipi fişte yalnızca Borç Tutar, Alacak tipi fişte yalnızca Alacak Tutar,
        // BorcAlacak tipinde (ör. açılış fişi) ikisi de görünür.
        [XafDisplayName("Borç Tutar")]
        [Appearance("SH_CariHesapHareketleri_BorcTutar",
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
        [Appearance("SH_CariHesapHareketleri_AlacakTutar",
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
        [RuleRequiredField("RuleReq_CariHesapHareketleri_DovizTanim",
            DefaultContexts.Save, "Lütfen Döviz Kodunu Giriniz...")]
        [Appearance("ED_CariHesapHareketleri_DovizTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public DovizTanim DovizTanim
        {
            get => dovizTanim;
            set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
        }

        [XafDisplayName("Döviz Kuru")]
        [Appearance("ED_CariHesapHareketleri_DovizKuru",
        Enabled = false, TargetItems = "DovizKuru", 
            Criteria = "!(IsNewObject(this))",Context = "DetailView")]

        [RuleRequiredField("RuleRequired_CariHesapHareketleri_DovizKuru", DefaultContexts.Save, "Lütfen Bir Döviz Kuru Giriniz...")]
        public decimal? DovizKuru
        {
            get => dovizKuru;
            set => SetPropertyValue(nameof(DovizKuru), ref dovizKuru, value);
        }

        [XafDisplayName("Yerel Borç Tutar")]
        [Appearance("ED_CariHesapHareketleri_YerelBorcTutar", Enabled = false, Criteria = "", Context = "Any")]
        [Appearance("SH_CariHesapHareketleri_YerelBorcTutar",
            Visibility = ViewItemVisibility.Hide, TargetItems = "YerelBorcTutar",
            Criteria = "FisTuruTanim.FinansBorcAlacakTipi = 2 ", Context = "Any")]
        public decimal? YerelBorcTutar
        {
            get => yerelBorcTutar;
            set => SetPropertyValue(nameof(YerelBorcTutar), ref yerelBorcTutar, value);
        }

        [XafDisplayName("Yerel Alacak Tutar")]
        [Appearance("ED_CariHesapHareketleri_YerelAlacakTutar", Enabled = false, Criteria = "", Context = "Any")]
        [Appearance("SH_CariHesapHareketleri_YerelAlacakTutar",
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
                // Fiş Türü'ne tanımlı varsayılan değerler (FisTuruVarsayilanDegeri, Faz 3) varsa
                // uygulanır. Kullanıcı sonradan Cari Hesap seçerse aşağıdaki cascade bunun üzerine
                // yazar — daha spesifik bir seçim, genel Fiş Türü varsayılanına göre önceliklidir.
                FisTuruVarsayilanlariniUygula((FisTuruTanim)newValue);
            }
            if (propertyName == nameof(CariHesapTanim) && newValue != null)
            {
                DovizTanim = ((CariHesapTanim)newValue).DovizTanim;
                if (DovizTanim != null && ! IsLoading)
                {
                    // Kur, DovizTanim'in kendi OnChanged bloğuna (aşağıda) bırakılmadan burada da
                    // açıkça uygulanır: DovizTanim ataması reentrant bir OnChanged("DovizTanim",...)
                    // tetiklese de, Blazor bazı senaryolarda iç içe (nested) çağrı sırasında güncellenen
                    // bir referans alanının UI'da kayıttan önce görünmesini kaçırabiliyordu (canlı testte
                    // yakalandı: Cari TRY hesap seçilince ekranda Döviz Kodu bir adım geriden "USD"
                    // gösterip Kuru "1,00" gösteriyordu — DB'ye yazılan değer doğruydu, yalnızca anlık
                    // ekran tutarsızdı). Açık çağrı, kullanıcının kaydetmeden önce doğru veriyi görmesini
                    // garanti eder.
                    DovizKuruGuncelle(DovizTanim);
                    CriteriaOperator criteria = new BinaryOperator("DovizTanim", DovizTanim);
                    KasaBankaTanim = Session.FindObject<KasaBankaTanim>(criteria);
                }

            }
            if (propertyName == nameof(DovizTanim) && newValue != null && !IsLoading)
            {
                DovizKuruGuncelle((DovizTanim)newValue);
                // Kasa/Banka Hesabı, kendi para birimiyle farklı bir Döviz Kodu taşıyamaz (bir TRY
                // kasasına USD tahsilat/ödeme kaydı gibi tutar-doğruluğu bozan bir durum, canlı
                // testte yakalandı). Döviz Kodu elle değiştirildiğinde, artık uyumsuz olan Kasa/Banka
                // seçimi temizlenir; kullanıcı yeni para birimine uygun bir hesap seçmek zorunda kalır.
                if (KasaBankaTanim != null && KasaBankaTanim.DovizTanim != (DovizTanim)newValue)
                {
                    KasaBankaTanim = null;
                }
            }
            CalculateTutar();

        }

        // Kur otomatik güncellenir: varsayılandan gelsin ya da sonradan elle seçilsin, Döviz Kodu
        // her değiştiğinde o günün (Fiş Tarihi) güncel kuru çekilir. TRY için her zaman 1 — eskiden
        // bu yalnızca "o gün için hiç TCMB kaydı yoksa" durumunda çalışıyordu; ama TCMB tablosu
        // TRY'yi hiç listelemediğinden (yalnızca döviz kurlarını TRY karşılığında verir)
        // DovizGunlukKurM kaydı bulunsa bile içinde TRY detayı olmadığı için kur hiç atanmıyordu —
        // gerçek bug, canlı testte yakalandı.
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
            // D-2/D-6: kendi eşleştirme kodumuz burada (ObjectSaving'den ÖNCE) sabitlenir.
            // ObjectSaving() içinde this'in kendi alanını değiştirmek, XPO'nun aynı commit
            // içinde ObjectSaving()'i bu nesne için ikinci kez tetiklemesine yol açıyordu —
            // ikinci geçişte henüz commit edilmemiş mirror bulunamayınca AYNI FisNo ile İKİNCİ
            // bir KasaBankaHareketleri daha oluşuyor ve unique index'te çakışıyordu (gerçek bug,
            // canlı testte yakalandı). Artık ObjectSaving() this'e hiç dokunmuyor.
            // NOT: IntegrationSourceEntity BURADA (native/kaynak kayıtta) KASITLI OLARAK set
            // EDİLMEZ — yalnızca IntegrationCode "zaten işlendi" bayrağı olarak yeterli. Önceden
            // burada IntegrationSourceEntity = typeof(KasaBankaHareketleri) da set ediliyordu; bu,
            // ayna kaydın KENDİ ObjectSaving()'inin karşı tarafa yazdığı AYNI değerle çakışıyordu
            // (bkz. KasaBankaHareketleri.ObjectSaving()'de "CariHareket.IntegrationSourceEntity =
            // typeof(KasaBankaHareketleri)") — sonuç: native bir kayıt İLE onun ürettiği gerçek
            // ayna kaydı, IntegrationSourceEntity'ye bakarak birbirinden AYIRT EDİLEMİYORDU (genel
            // "Cari Hesap Hareketleri" listesinden native bir CATHSL kaydına çift tıklandığında
            // yanlışlıkla Kasa ekranına yönlendiriliyordu — canlı testte yakalandı). Artık bu alan
            // SADECE gerçek ayna kayıtlarda (ObjectSaving() tarafından karşı nesneye yazılırken)
            // set ediliyor; native kayıtta null kalır.
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

            if (sonuc == true)
            {
                // D-2/D-6: ayrı bir Session açıp gerçek PK'yı (KeyID) paylaştırma kaldırıldı.
                // Karşı kayıt, bunun için var olan IntegrationCode/IntegrationSourceEntity
                // (BaseClass) çifti ile, aynı Session (outer UnitOfWork) içinde bulunur/oluşturulur —
                // commit/rollback outer UnitOfWork ile birlikte olur. Önce henüz commit edilmemiş
                // (bu transaction içinde yeni oluşturulmuş) adaylara bakılır, sonra DB'ye sorulur —
                // aynı commit içinde bu metod birden fazla kez tetiklenirse bile mükerrer kayıt oluşmaz.
                KasaBankaHareketleri KasaBankaHareket =
                    Session.GetObjectsToSave().OfType<KasaBankaHareketleri>()
                        .FirstOrDefault(x => x.IntegrationCode == this.IntegrationCode)
                    ?? Session.FindObject<KasaBankaHareketleri>(
                        new BinaryOperator(nameof(IntegrationCode), this.IntegrationCode));

                if (KasaBankaHareket == null)
                {
                    KasaBankaHareket = new KasaBankaHareketleri(Session);

                    KasaBankaHareket.IntegrationCode = this.IntegrationCode;
                    KasaBankaHareket.IntegrationSourceEntity = typeof(CariHesapHareketleri);

                    KasaBankaHareket.FisNo = this.FisNo;
                    KasaBankaHareket.FisTarihi = this.FisTarihi;
                    KasaBankaHareket.VadeTarihi = this.VadeTarihi;
                    // Kasa tarafının kendi Fiş Türü'ne eşlenir (ör. CAODME -> KSODME) — kaynağın
                    // Fiş Türü'nü olduğu gibi kopyalamak, "Kasa Ödeme Fişi" gibi türe-özel
                    // filtrelenmiş görünümlerde mirror kaydının hiç görünmemesine yol açıyordu.
                    KasaBankaHareket.FisTuruTanim = KarsiFisTuru(this.FisTuruTanim.FisTuruKodu);
                    KasaBankaHareket.CariKasaBankaTipi = this.CariKasaBankaTipi;
                }

                KasaBankaHareket.KasaBankaTanim = this.KasaBankaTanim;
                KasaBankaHareket.BelgeNo = this.BelgeNo;
                KasaBankaHareket.BelgeTarihi = this.BelgeTarihi;
                KasaBankaHareket.CariHesapTanim = this.CariHesapTanim;
                // Muhasebe düzeltmesi: Borç/Alacak TAKAS EDİLEREK kopyalanır (önceki "artık ters
                // çevrilmiyor" kararı yanlıştı — bkz. docs/CHANGELOG.md). Çift-taraflı muhasebede bir
                // Cari tahsilatı, Cari hesabını ALACAK, Kasa/Banka hesabını BORÇ yönünde etkiler —
                // aynı işlem iki defterde TERS yönlerdedir. fis-turleri.csv artık bunu yansıtır
                // (CATHSL=Alacak / KSTHSL=Borç, CAODME=Borç / KSODME=Alacak), bu yüzden kaynağın
                // Alacak tutarı ayna'nın Borç alanına, Borç tutarı ayna'nın Alacak alanına yazılır —
                // aksi halde tutar, ayna'nın kendi Fiş Türü yönüne göre GİZLİ bir alanda kalırdı
                // (Appearance kriteri "Bug 2" ile aynı hata sınıfı).
                KasaBankaHareket.BorcTutar = this.AlacakTutar;
                KasaBankaHareket.AlacakTutar = this.BorcTutar;
                KasaBankaHareket.DovizTanim = this.DovizTanim;
                KasaBankaHareket.DovizKuru = this.DovizKuru;
                KasaBankaHareket.YerelBorcTutar = this.YerelAlacakTutar;
                KasaBankaHareket.YerelAlacakTutar = this.YerelBorcTutar;

                if (Session.IsObjectToSave(this))
                {
                    KasaBankaHareket.Save();
                }
            }

        }
        // Ayna kaydı silme zincirinde çapraz-tetiklemeyi (bkz. ObjectDeleting()) kesin olarak
        // engeller. XPO'nun Session.IsObjectMarkedDeleted/IsObjectToDelete metodları bu callback
        // sırasında henüz "silinecek" olarak işaretlenmemiş objeler için false dönüyor (immediate
        // deletion'da "pending delete" durumu ObjectDeleting hook'larından SONRA işleniyor gibi
        // görünüyor) — ikisi de canlı testte StackOverflowException/sunucu çökmesiyle sonuçlanan
        // sonsuz karşılıklı silmeyi engelleyemedi. Instance-level bu bayrak, XPO'nun iç zamanlamasına
        // bağımlı olmadığından güvenilir.
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
                KasaBankaHareketleri KasaBankaHareket = Session.FindObject<KasaBankaHareketleri>(
                    new BinaryOperator(nameof(IntegrationCode), IntegrationCode));
                if (KasaBankaHareket != null && !KasaBankaHareket.AynaKaydiSilmeSurecinde)
                    Session.Delete(KasaBankaHareket);
            }

        }

        // Cari fiş türünü Kasa tarafındaki kendi eşdeğerine eşler (CATHSL->KSTHSL, CAODME->KSODME).
        // Eşdeğer bulunamazsa (beklenmeyen durum) kaynağın türü aynen kullanılır.
        FisTuruTanim KarsiFisTuru(string kaynakKodu)
        {
            string karsiKodu = kaynakKodu switch
            {
                "CATHSL" => "KSTHSL",
                "CAODME" => "KSODME",
                _ => kaynakKodu
            };
            return Session.FindObject<FisTuruTanim>(new BinaryOperator(nameof(FisTuruTanim.FisTuruKodu), karsiKodu))
                ?? this.FisTuruTanim;
        }

    }
}