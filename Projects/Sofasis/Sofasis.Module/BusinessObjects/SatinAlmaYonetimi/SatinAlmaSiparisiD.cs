using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Sofasis.Module.BusinessObjects;

// Satır: tedarikçiye sipariş edilen kalem. BirimFiyat, SA-4 Mal Kabul'de
// IWeightedAverageCostService.GirisUygula'ya girecek gerçek maliyettir — bu yüzden Teklif'in
// aksine burada fiyat KİLİTLİ (kayıt sonrası immutable, bkz. SatinAlmaSiparisiKilitleController).
[DefaultClassOptions]
[XafDisplayName("Satın Alma Sipariş Kalemi")]
public class SatinAlmaSiparisiD : BaseClass
{
    public SatinAlmaSiparisiD(Session session)
        : base(session)
    {
    }

    SatinAlmaSiparisiM satinAlmaSiparisiM;
    StokTanim stokTanim;
    decimal miktar;
    decimal birimFiyat;
    decimal toplamTutar;
    SatinAlmaTalebiD kaynakTalepD;
    SatinAlmaTeklifD kaynakTeklifD;

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [Association("SatinAlmaSiparisiM-SatinAlmaSiparisiDs")]
    [RuleRequiredField("RuleRequired_SatinAlmaSiparisiD_SatinAlmaSiparisiM", DefaultContexts.Save, "Satır bir sipariş başlığına bağlı olmalıdır.")]
    public SatinAlmaSiparisiM SatinAlmaSiparisiM
    {
        get => satinAlmaSiparisiM;
        set => SetPropertyValue(nameof(SatinAlmaSiparisiM), ref satinAlmaSiparisiM, value);
    }

    [XafDisplayName("Stok / Hizmet / Masraf")]
    [RuleRequiredField("RuleRequired_SatinAlmaSiparisiD_StokTanim", DefaultContexts.Save, "Lütfen Stok/Hizmet/Masraf Seçiniz...")]
    public StokTanim StokTanim
    {
        get => stokTanim;
        set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
    }

    // Kalıcı bir kolon DEĞİL — StokTanim zaten kendi BirimTanim'ini taşıyor (SatinAlmaTalebiD/
    // SatinAlmaTeklifD'deki Birim property'siyle birebir aynı desen).
    [XafDisplayName("Birim")]
    public BirimTanim Birim => StokTanim?.BirimTanim;

    [DbType("decimal(18,4)")]
    [XafDisplayName("Miktar")]
    [RuleValueComparison("Rule_SatinAlmaSiparisiD_Miktar_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Lütfen miktarı sıfırdan büyük giriniz.")]
    public decimal Miktar
    {
        get => miktar;
        set
        {
            decimal oldValue = miktar;
            SetPropertyValue(nameof(Miktar), ref miktar, value);
            if (oldValue != value) HesaplaToplamTutar();
        }
    }

    [DbType("decimal(28,6)")]
    [XafDisplayName("Birim Fiyat")]
    [RuleValueComparison("Rule_SatinAlmaSiparisiD_BirimFiyat_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Lütfen birim fiyatı sıfırdan büyük giriniz.")]
    public decimal BirimFiyat
    {
        get => birimFiyat;
        set
        {
            decimal oldValue = birimFiyat;
            SetPropertyValue(nameof(BirimFiyat), ref birimFiyat, value);
            if (oldValue != value) HesaplaToplamTutar();
        }
    }

    [DbType("decimal(18,2)")]
    [XafDisplayName("Toplam Tutar")]
    [Appearance("ED_SatinAlmaSiparisiD_ToplamTutar", Enabled = false, Context = "Any")]
    public decimal ToplamTutar
    {
        get => toplamTutar;
        set => SetPropertyValue(nameof(ToplamTutar), ref toplamTutar, value);
    }

    // İzlenebilirlik: bu satırın hangi Talep/Teklif kaleminden geldiği (ISatinAlmaSiparisServisi
    // tarafından doldurulur; Talep'ten doğrudan sipariş açıldığında ikisi de boş kalabilir).
    [XafDisplayName("Kaynak Talep Kalemi")]
    public SatinAlmaTalebiD KaynakTalepD
    {
        get => kaynakTalepD;
        set => SetPropertyValue(nameof(KaynakTalepD), ref kaynakTalepD, value);
    }

    [XafDisplayName("Kaynak Teklif Kalemi")]
    public SatinAlmaTeklifD KaynakTeklifD
    {
        get => kaynakTeklifD;
        set => SetPropertyValue(nameof(KaynakTeklifD), ref kaynakTeklifD, value);
    }

    void HesaplaToplamTutar()
    {
        ToplamTutar = System.Math.Round(Miktar * BirimFiyat, 2, System.MidpointRounding.AwayFromZero);
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        if (SatinAlmaSiparisiM != null)
            SatinAlmaSiparisiM.UpdateTotals(null);
    }

    protected override void OnDeleting()
    {
        base.OnDeleting();
        if (SatinAlmaSiparisiM != null)
            SatinAlmaSiparisiM.UpdateTotals(this);
    }
}
