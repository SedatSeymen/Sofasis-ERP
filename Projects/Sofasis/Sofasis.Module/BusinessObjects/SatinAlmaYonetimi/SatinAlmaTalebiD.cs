using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace Sofasis.Module.BusinessObjects;

// Satır ("ne/ne kadar"): StokTanim zaten Stok/Hizmet/Masraf'ın hepsini StokHizmetMasrafTipi ile
// kapsadığından (bkz. HizmetTanim/MasrafTanim ekranları da StokTanim üzerinden çalışıyor) ayrı bir
// kalem-tipi ayrımı gerekmiyor. Bare BaseClass'tan türer (StokHareketleriD'nin aksine) — burada
// CreatedDate'e göre sıralanan bir silme-sonrası replay algoritması yok, SatisSiparisD ile aynı
// gerekçeyle bare BaseClass yeterli.
[DefaultClassOptions]
// DefaultProperty StokTanim'e devredilir (StokTanim'in kendi DefaultProperty'si StokAdi) — aksi
// halde SatinAlmaTeklifD.KaynakTalepD gibi bu satırı referans alan lookup'larda ham Oid görünür.
[DefaultProperty(nameof(StokTanim))]
[XafDisplayName("Satın Alma Talep Kalemi")]
public class SatinAlmaTalebiD : BaseClass
{
    public SatinAlmaTalebiD(Session session)
        : base(session)
    {
    }

    SatinAlmaTalebiM satinAlmaTalebiM;
    StokTanim stokTanim;
    decimal miktar;
    decimal? tahminiBirimFiyat;
    decimal tahminiTutar;
    string aciklamaSatir;

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [Association("SatinAlmaTalebiM-SatinAlmaTalebiDs")]
    [RuleRequiredField("RuleRequired_SatinAlmaTalebiD_SatinAlmaTalebiM", DefaultContexts.Save, "Satır bir talep başlığına bağlı olmalıdır.")]
    public SatinAlmaTalebiM SatinAlmaTalebiM
    {
        get => satinAlmaTalebiM;
        set => SetPropertyValue(nameof(SatinAlmaTalebiM), ref satinAlmaTalebiM, value);
    }

    [XafDisplayName("Stok / Hizmet / Masraf")]
    [RuleRequiredField("RuleRequired_SatinAlmaTalebiD_StokTanim", DefaultContexts.Save, "Lütfen Stok/Hizmet/Masraf Seçiniz...")]
    public StokTanim StokTanim
    {
        get => stokTanim;
        set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
    }

    // Kalıcı bir kolon DEĞİL — StokTanim zaten kendi BirimTanim'ini taşıyor (Adet/Metre/Kg...),
    // burada tekrar saklamak iki ayrı kaynaktan çelişebilecek bir birim bilgisine yol açardı.
    // Salt-okunur, seçilen StokTanim'e göre otomatik gelen bir gösterim alanı yeterli.
    [XafDisplayName("Birim")]
    public BirimTanim Birim => StokTanim?.BirimTanim;

    [DbType("decimal(18,4)")]
    [XafDisplayName("Miktar")]
    [RuleValueComparison("Rule_SatinAlmaTalebiD_Miktar_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Lütfen miktarı sıfırdan büyük giriniz.")]
    public decimal Miktar
    {
        get => miktar;
        set
        {
            decimal oldValue = miktar;
            SetPropertyValue(nameof(Miktar), ref miktar, value);
            if (oldValue != value) HesaplaTahminiTutar();
        }
    }

    [DbType("decimal(28,6)")]
    [XafDisplayName("Tahmini Birim Fiyat")]
    [RuleValueComparison("Rule_SatinAlmaTalebiD_TahminiBirimFiyat_NegatifOlamaz", DefaultContexts.Save, ValueComparisonType.GreaterThanOrEqual, 0,
        CustomMessageTemplate = "Tahmini birim fiyat negatif olamaz.")]
    public decimal? TahminiBirimFiyat
    {
        get => tahminiBirimFiyat;
        set
        {
            decimal? oldValue = tahminiBirimFiyat;
            SetPropertyValue(nameof(TahminiBirimFiyat), ref tahminiBirimFiyat, value);
            if (oldValue != value) HesaplaTahminiTutar();
        }
    }

    [DbType("decimal(18,2)")]
    [XafDisplayName("Tahmini Tutar")]
    [Appearance("ED_SatinAlmaTalebiD_TahminiTutar", Enabled = false, Context = "Any")]
    public decimal TahminiTutar
    {
        get => tahminiTutar;
        set => SetPropertyValue(nameof(TahminiTutar), ref tahminiTutar, value);
    }

    [Size(200)]
    [XafDisplayName("Açıklama")]
    public string AciklamaSatir
    {
        get => aciklamaSatir;
        set => SetPropertyValue(nameof(AciklamaSatir), ref aciklamaSatir, value);
    }

    void HesaplaTahminiTutar()
    {
        TahminiTutar = System.Math.Round(Miktar * (TahminiBirimFiyat ?? 0), 2, System.MidpointRounding.AwayFromZero);
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        if (SatinAlmaTalebiM != null)
            SatinAlmaTalebiM.UpdateTotals(null);
    }

    protected override void OnDeleting()
    {
        base.OnDeleting();
        if (SatinAlmaTalebiM != null)
            SatinAlmaTalebiM.UpdateTotals(this);
    }
}
