using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;

namespace Sofasis.Module.BusinessObjects
{
    // Satır: irsaliye edilen kalem. Asıl stok/maliyet motoru burada DEĞİL — bkz. StokHareketiD
    // (bire-bir bağlı StokHareketleriD satırı, ISatinAlmaIrsaliyeServisi tarafından İrsaliye
    // satırıyla BİRLİKTE oluşturulur). Bu satır yalnızca "hangi sevkiyatta, hangi sipariş kaleminden,
    // ne kadar" bilgisini taşıyan bir izlenebilirlik/görüntüleme katmanıdır.
    [DefaultClassOptions]
    [XafDisplayName("İrsaliye Kalemi")]
    public class IrsaliyeD : BaseClass
    {
        public IrsaliyeD(Session session)
            : base(session)
        {
        }

        IrsaliyeM irsaliyeM;
        StokTanim stokTanim;
        decimal miktar;
        decimal birimFiyat;
        decimal toplamTutar;
        SatinAlmaSiparisiD kaynakSiparisD;
        StokHareketleriD stokHareketiD;

        // KRİTİK: SetPropertyValue KULLANILIR — bkz. StokHareketleriD.StokHareketleriM'deki ayrıntılı
        // yorum (bu proje genelinde TÜM ilişki alanları için zorunlu desen).
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [Association("IrsaliyeM-IrsaliyeDs")]
        [RuleRequiredField("RuleRequired_IrsaliyeD_IrsaliyeM", DefaultContexts.Save, "Satır bir irsaliye başlığına bağlı olmalıdır.")]
        public IrsaliyeM IrsaliyeM
        {
            get => irsaliyeM;
            set => SetPropertyValue(nameof(IrsaliyeM), ref irsaliyeM, value);
        }

        // NOT: "!(IsNewObject(this))" DEĞİL — servis satırları programatik olarak (kullanıcı "Yeni"
        // butonuna basmadan) ÖNCEDEN oluşturup popup'a öyle taşır; StokHareketleriD/FaturaD'de
        // canlı testte kanıtlanan nested-ObjectSpace/IsNewObject sorunu (bkz. docs/CHANGELOG.md
        // 2026-08-13) burada da geçerli olurdu. BAŞLIK irsaliyenin gerçekten numaralanıp
        // numaralanmadığına bakılır — düz alan okuması, bağlama duyarlı değil.
        [XafDisplayName("Stok / Hizmet / Masraf")]
        [Appearance("ED_IrsaliyeD_StokTanim", Enabled = false, Criteria = "IrsaliyeM.IrsaliyeNo != '" + Helper.ConstNewRecordText + "'", Context = "Any")]
        [RuleRequiredField("RuleRequired_IrsaliyeD_StokTanim", DefaultContexts.Save, "Lütfen Stok/Hizmet/Masraf Seçiniz...")]
        public StokTanim StokTanim
        {
            get => stokTanim;
            set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
        }

        [DbType("decimal(18,4)")]
        [XafDisplayName("Miktar")]
        [Appearance("ED_IrsaliyeD_Miktar", Enabled = false, Criteria = "IrsaliyeM.IrsaliyeNo != '" + Helper.ConstNewRecordText + "'", Context = "Any")]
        [RuleValueComparison("Rule_IrsaliyeD_Miktar_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
            CustomMessageTemplate = "Lütfen miktarı sıfırdan büyük giriniz.")]
        public decimal Miktar
        {
            get => miktar;
            set => SetPropertyValue(nameof(Miktar), ref miktar, value);
        }

        [DbType("decimal(28,6)")]
        [XafDisplayName("Birim Fiyat")]
        [Appearance("ED_IrsaliyeD_BirimFiyat", Enabled = false, Criteria = "IrsaliyeM.IrsaliyeNo != '" + Helper.ConstNewRecordText + "'", Context = "Any")]
        [RuleValueComparison("Rule_IrsaliyeD_BirimFiyat_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
            CustomMessageTemplate = "Lütfen birim fiyatı sıfırdan büyük giriniz.")]
        public decimal BirimFiyat
        {
            get => birimFiyat;
            set => SetPropertyValue(nameof(BirimFiyat), ref birimFiyat, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("Toplam Tutar")]
        [Appearance("ED_IrsaliyeD_ToplamTutar", Enabled = false, Context = "Any")]
        public decimal ToplamTutar
        {
            get => toplamTutar;
            set => SetPropertyValue(nameof(ToplamTutar), ref toplamTutar, value);
        }

        [Browsable(false)]
        public SatinAlmaSiparisiD KaynakSiparisD
        {
            get => kaynakSiparisD;
            set => SetPropertyValue(nameof(KaynakSiparisD), ref kaynakSiparisD, value);
        }

        // Bire-bir bağlı StokHareketleriD satırı — asıl stok/maliyet etkisi ve Fatura'nın izlediği
        // KaynakStokHareketiD zinciri buradan geçer (bkz. FaturaKaydetServisi). FaturaD.KaynakStokHareketiD
        // ile simetrik: bir StokHareketleriD satırı en fazla BİR IrsaliyeD'ye bağlanabilir (DB seviyesi
        // garanti — 3-ajan incelemesinde bu simetrinin eksik olduğu bulundu, bkz. docs/CHANGELOG.md).
        [Indexed(Unique = true)]
        [XafDisplayName("Stok Hareket Satırı")]
        [Appearance("ED_IrsaliyeD_StokHareketiD", Enabled = false, Context = "Any")]
        public StokHareketleriD StokHareketiD
        {
            get => stokHareketiD;
            set => SetPropertyValue(nameof(StokHareketiD), ref stokHareketiD, value);
        }

        public override void ObjectSaving()
        {
            if (IrsaliyeM == null)
                throw new UserFriendlyException("İrsaliye kalemi bir irsaliye başlığına bağlı olmalıdır.");
            if (StokTanim == null)
                throw new UserFriendlyException("Lütfen Stok/Hizmet/Masraf seçiniz.");

            ToplamTutar = Math.Round(Miktar * BirimFiyat, 2, MidpointRounding.AwayFromZero);
        }
    }
}
