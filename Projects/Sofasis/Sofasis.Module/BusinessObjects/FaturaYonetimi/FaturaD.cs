using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sofasis.Module.Services;
using System.Linq;

namespace Sofasis.Module.BusinessObjects
{
    // Satır: fatura edilen kalem. StokHareketleriD'nin aksine burada bir "motor" (ağırlıklı
    // ortalama gibi kalıcı/kümülatif durum değiştiren bir hesap) YOK — KDV/Tevkifat hesaplaması
    // saf/idempotent (IVatCalculator), bu yüzden StokHareketleriD.MotorIslendi türü bir bayrağa
    // gerek duyulmadı.
    [DefaultClassOptions]
    [XafDisplayName("Fatura Kalemi")]
    public class FaturaD : BaseClass
    {
        public FaturaD(Session session)
            : base(session)
        {
        }

        FaturaM faturaM;
        StokTanim stokTanim;
        decimal miktar;
        decimal birimFiyat;
        StokHareketleriD kaynakStokHareketiD;
        KDVTanim kDVTanim;
        TevkifatTanim tevkifatTanim;
        decimal kDVHaricTutar;
        decimal kDVTutar;
        decimal tevkifatTutar;
        decimal netTutar;

        // KRİTİK: SetPropertyValue KULLANILIR — StokHareketleriD.StokHareketleriM'de bu
        // atlanmıştı ve XPO'nun [Association] çift-yönlü senkronizasyonu (FaturaM.FaturaDs
        // koleksiyonunun otomatik güncellenmesi) tamamen bozuluyordu (canlı testte kanıtlandı,
        // bkz. o dosyadaki ayrıntılı yorum). Bu proje genelinde TÜM ilişki alanları için zorunlu
        // desen.
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [Association("FaturaM-FaturaDs")]
        [RuleRequiredField("RuleRequired_FaturaD_FaturaM", DefaultContexts.Save, "Satır bir fatura başlığına bağlı olmalıdır.")]
        public FaturaM FaturaM
        {
            get => faturaM;
            set => SetPropertyValue(nameof(FaturaM), ref faturaM, value);
        }

        [XafDisplayName("Stok / Hizmet / Masraf")]
        // NOT: "!(IsNewObject(this))" DEĞİL — FaturaKaydetServisi satırları programatik olarak
        // (kullanıcı "Yeni" butonuna basmadan) ÖNCEDEN oluşturup popup'a öyle taşıyor; StokHareketleriD'de
        // canlı testte kanıtlanan aynı nested-ObjectSpace/IsNewObject sorunu burada da geçerli
        // (bkz. StokHareketleriD.StokTanim'deki ayrıntılı yorum). BAŞLIK faturasının gerçekten
        // numaralanıp numaralanmadığına bakılır — düz alan okuması, IsNewObject() gibi bağlama
        // duyarlı değil. Kayıtlı bir faturaya sonradan satır eklenmesi FaturaKilitleController
        // tarafından zaten engellendiğinden regresyon YARATMAZ.
        [Appearance("ED_FaturaD_StokTanim", Enabled = false, Criteria = "FaturaM.FaturaNo != '" + Helper.ConstNewRecordText + "'", Context = "Any")]
        [RuleRequiredField("RuleRequired_FaturaD_StokTanim", DefaultContexts.Save, "Lütfen Stok/Hizmet/Masraf Seçiniz...")]
        public StokTanim StokTanim
        {
            get => stokTanim;
            set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
        }

        [DbType("decimal(18,4)")]
        [XafDisplayName("Miktar")]
        // Kriter gerekçesi: bkz. StokTanim property'sindeki yorum (nested ObjectSpace/IsNewObject sorunu).
        [Appearance("ED_FaturaD_Miktar", Enabled = false, Criteria = "FaturaM.FaturaNo != '" + Helper.ConstNewRecordText + "'", Context = "Any")]
        [RuleValueComparison("Rule_FaturaD_Miktar_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
            CustomMessageTemplate = "Lütfen miktarı sıfırdan büyük giriniz.")]
        public decimal Miktar
        {
            get => miktar;
            set => SetPropertyValue(nameof(Miktar), ref miktar, value);
        }

        [DbType("decimal(28,6)")]
        [XafDisplayName("Birim Fiyat")]
        // Kriter gerekçesi: bkz. StokTanim property'sindeki yorum (nested ObjectSpace/IsNewObject sorunu).
        [Appearance("ED_FaturaD_BirimFiyat", Enabled = false, Criteria = "FaturaM.FaturaNo != '" + Helper.ConstNewRecordText + "'", Context = "Any")]
        [RuleValueComparison("Rule_FaturaD_BirimFiyat_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
            CustomMessageTemplate = "Lütfen birim fiyatı sıfırdan büyük giriniz.")]
        public decimal BirimFiyat
        {
            get => birimFiyat;
            set => SetPropertyValue(nameof(BirimFiyat), ref birimFiyat, value);
        }

        // İzlenebilirlik: bu fatura satırının hangi Mal Kabul (StokHareketleriD) satırından
        // geldiği. DOĞRUDAN referans — StokHareketleriD zaten hem Giriş hem Çıkış yönünü tek
        // sınıfta kapsadığından polimorfik bir Tip/Oid çiftine GEREK YOK (StokHareketleriD.
        // KaynakBelgeTipi/Oid'nin aksine). Bir StokHareketleriD satırı en fazla BİR FaturaD'ye
        // bağlanabilir — bkz. ObjectSaving() içindeki mükerrer kullanım kontrolü.
        // DB-seviyesi benzersizlik: ObjectSaving() içindeki uygulama-seviyesi mükerrer kontrolü
        // yarış durumuna (iki eşzamanlı istek) karşı yeterli DEĞİL — CariHesapTanim.CariHesapKodu/
        // FaturaM.FaturaNo'daki aynı [Indexed(Unique=true)] deseniyle kapatılır. NULL'a izin verir
        // (SQL Server unique index birden çok NULL'a izin verir), bu yüzden elle oluşturulmuş
        // (Mal Kabul kaynağı olmayan) fatura satırlarını etkilemez.
        [Indexed(Unique = true)]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        public StokHareketleriD KaynakStokHareketiD
        {
            get => kaynakStokHareketiD;
            set => SetPropertyValue(nameof(KaynakStokHareketiD), ref kaynakStokHareketiD, value);
        }

        [XafDisplayName("KDV")]
        [RuleRequiredField("RuleRequired_FaturaD_KDVTanim", DefaultContexts.Save, "Lütfen KDV Oranı Seçiniz...")]
        public KDVTanim KDVTanim
        {
            get => kDVTanim;
            set => SetPropertyValue(nameof(KDVTanim), ref kDVTanim, value);
        }

        [XafDisplayName("Tevkifat")]
        public TevkifatTanim TevkifatTanim
        {
            get => tevkifatTanim;
            set => SetPropertyValue(nameof(TevkifatTanim), ref tevkifatTanim, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("KDV Hariç Tutar")]
        [Appearance("ED_FaturaD_KDVHaricTutar", Enabled = false, Context = "Any")]
        public decimal KDVHaricTutar
        {
            get => kDVHaricTutar;
            set => SetPropertyValue(nameof(KDVHaricTutar), ref kDVHaricTutar, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("KDV Tutar")]
        [Appearance("ED_FaturaD_KDVTutar", Enabled = false, Context = "Any")]
        public decimal KDVTutar
        {
            get => kDVTutar;
            set => SetPropertyValue(nameof(KDVTutar), ref kDVTutar, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("Tevkifat Tutar")]
        [Appearance("ED_FaturaD_TevkifatTutar", Enabled = false, Context = "Any")]
        public decimal TevkifatTutar
        {
            get => tevkifatTutar;
            set => SetPropertyValue(nameof(TevkifatTutar), ref tevkifatTutar, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("Net Tutar")]
        [Appearance("ED_FaturaD_NetTutar", Enabled = false, Context = "Any")]
        public decimal NetTutar
        {
            get => netTutar;
            set => SetPropertyValue(nameof(NetTutar), ref netTutar, value);
        }

        public override void ObjectSaving()
        {
            if (FaturaM == null)
                throw new UserFriendlyException("Fatura kalemi bir fatura başlığına bağlı olmalıdır.");
            if (StokTanim == null)
                throw new UserFriendlyException("Lütfen Stok/Hizmet/Masraf seçiniz.");
            if (KDVTanim == null)
                throw new UserFriendlyException("Lütfen KDV oranı seçiniz.");

            // Bir Mal Kabul satırı en fazla bir kez faturalanabilir — aynı KaynakStokHareketiD'ye
            // bağlı BAŞKA bir FaturaD (bu satır hariç) varsa reddedilir (SA-4b'deki KalanMiktar
            // aşımı kontrolüyle aynı disiplin: taslak üzerinde serbestçe düzenlenebilen bir alan,
            // sunucu tarafında ayrıca doğrulanmalı).
            if (KaynakStokHareketiD != null)
            {
                bool mukerrer = Session.GetObjectsToSave().OfType<FaturaD>()
                    .Any(x => x != this && x.KaynakStokHareketiD == KaynakStokHareketiD)
                    || (Session.FindObject<FaturaD>(
                            DevExpress.Data.Filtering.CriteriaOperator.Parse(
                                "KaynakStokHareketiD = ? And Oid != ?", KaynakStokHareketiD.Oid, Oid)) != null);
                if (mukerrer)
                    throw new UserFriendlyException(
                        $"'{KaynakStokHareketiD.StokTanim?.StokAdi}' Mal Kabul satırı zaten başka bir faturada kullanılmış.");
            }

            IVatCalculator hesaplayici = new VatCalculator();
            FaturaSatirTutarlari tutarlar = hesaplayici.Hesapla(Miktar, BirimFiyat, KDVTanim.KDVOrani, TevkifatTanim?.TevkifatOrani);
            KDVHaricTutar = tutarlar.KDVHaricTutar;
            KDVTutar = tutarlar.KDVTutar;
            TevkifatTutar = tutarlar.TevkifatTutar;
            NetTutar = tutarlar.NetTutar;
        }
    }
}
