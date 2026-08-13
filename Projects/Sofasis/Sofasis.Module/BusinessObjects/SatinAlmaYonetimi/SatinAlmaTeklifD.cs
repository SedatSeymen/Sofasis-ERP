using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace Sofasis.Module.BusinessObjects;

// Satır: bir tedarikçinin, Talep'in belirli bir kalemine verdiği fiyat teklifi. EnDusukFiyatMi
// yalnızca ISatinAlmaTeklifServisi.Karsilastir() tarafından yazılır — kullanıcı elle değiştiremez,
// karşılaştırma ListView'indeki [Appearance] rengini tetiklemek için vardır (StokHareketleriD
// üzerindeki MotorIslendi gibi salt-motor amaçlı bir bayrak, bu yüzden Browsable(false)).
[DefaultClassOptions]
[DefaultProperty("StokTanim")]
[XafDisplayName("Satın Alma Teklif Kalemi")]
[Appearance("SatinAlmaTeklifD_EnDusuk", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "EnDusukFiyatMi = True", Context = "ListView", BackColor = "Green", FontColor = "White", Priority = 1)]
// Kullanıcı isteği: satır Talep'ten otomatik ön-dolduğunda (KaynakTalepD doluyken — bu alan zaten
// RuleRequiredField olduğundan pratikte HER ZAMAN dolu), yalnızca "Miktar" (teklif edilen miktar)
// ve "Teklif Birim Fiyatı" (para) serbest kalır; hangi Talep kalemine/hangi Stok'a karşılık geldiği
// artık sabittir, kazayla yanlış Stok'a bağlanmayı önlemek için kilitlenir.
[Appearance("ED_SatinAlmaTeklifD_KaynakTalepD_Kilit", Enabled = false, TargetItems = "KaynakTalepD",
    Criteria = "KaynakTalepD is not null", Context = "DetailView")]
[Appearance("ED_SatinAlmaTeklifD_StokTanim_Kilit", Enabled = false, TargetItems = "StokTanim",
    Criteria = "KaynakTalepD is not null", Context = "DetailView")]
public class SatinAlmaTeklifD : BaseClass
{
    public SatinAlmaTeklifD(Session session)
        : base(session)
    {
    }

    SatinAlmaTeklifM satinAlmaTeklifM;
    SatinAlmaTalebiD kaynakTalepD;
    StokTanim stokTanim;
    decimal miktar;
    decimal teklifBirimFiyat;
    decimal teklifTutar;
    int? teklifTeslimSuresi;
    bool enDusukFiyatMi;

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [Association("SatinAlmaTeklifM-SatinAlmaTeklifDs")]
    [RuleRequiredField("RuleRequired_SatinAlmaTeklifD_SatinAlmaTeklifM", DefaultContexts.Save, "Satır bir teklif başlığına bağlı olmalıdır.")]
    public SatinAlmaTeklifM SatinAlmaTeklifM
    {
        get => satinAlmaTeklifM;
        set => SetPropertyValue(nameof(SatinAlmaTeklifM), ref satinAlmaTeklifM, value);
    }

    // Yalnızca ebeveyn teklifin bağlı olduğu Talebe ait kalemler listelenir — kademeli lookup
    // (StokModelKonfigrasyonD.cs'deki "@This.<Ebeveyn>.<Alan>" deseninin birebir kopyası).
    [XafDisplayName("Kaynak Talep Kalemi")]
    [DataSourceCriteria("SatinAlmaTalebiM = '@This.SatinAlmaTeklifM.KaynakTalep'")]
    [RuleRequiredField("RuleRequired_SatinAlmaTeklifD_KaynakTalepD", DefaultContexts.Save, "Lütfen Kaynak Talep Kalemini Seçiniz...")]
    public SatinAlmaTalebiD KaynakTalepD
    {
        get => kaynakTalepD;
        set
        {
            SetPropertyValue(nameof(KaynakTalepD), ref kaynakTalepD, value);
            // Seçilen talep kaleminden Stok/Hizmet/Masraf ve miktarı öneri olarak devral —
            // kullanıcı gerekirse üzerine yazabilir (SatisSiparisD.StokTanim setter'ıyla aynı desen).
            if (!IsLoading && kaynakTalepD != null)
            {
                if (StokTanim == null) StokTanim = kaynakTalepD.StokTanim;
                if (Miktar == 0) Miktar = kaynakTalepD.Miktar;
            }
        }
    }

    [XafDisplayName("Stok / Hizmet / Masraf")]
    [RuleRequiredField("RuleRequired_SatinAlmaTeklifD_StokTanim", DefaultContexts.Save, "Lütfen Stok/Hizmet/Masraf Seçiniz...")]
    public StokTanim StokTanim
    {
        get => stokTanim;
        set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
    }

    // Kalıcı bir kolon DEĞİL — Talebin ne kadar istediğinin salt-okunur referansı. "Miktar"
    // (aşağıda) tedarikçinin GERÇEKTEN teklif ettiği miktardır ve bundan farklı olabilir (Talep 10
    // isterken tedarikçi 20 veya 5 teklif edebilir); ikisini yan yana göstermek karşılaştırmayı
    // kolaylaştırır.
    [XafDisplayName("Talep Miktarı")]
    public decimal? TalepMiktari => KaynakTalepD?.Miktar;

    // Kalıcı bir kolon DEĞİL — StokTanim zaten kendi BirimTanim'ini taşıyor (Adet/Metre/Kg...),
    // burada tekrar saklamak iki ayrı kaynaktan çelişebilecek bir birim bilgisine yol açardı.
    // Salt-okunur, seçilen StokTanim'e göre otomatik gelen bir gösterim alanı yeterli.
    [XafDisplayName("Birim")]
    public BirimTanim Birim => StokTanim?.BirimTanim;

    [DbType("decimal(18,4)")]
    [XafDisplayName("Miktar")]
    [RuleValueComparison("Rule_SatinAlmaTeklifD_Miktar_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Lütfen miktarı sıfırdan büyük giriniz.")]
    public decimal Miktar
    {
        get => miktar;
        set
        {
            decimal oldValue = miktar;
            SetPropertyValue(nameof(Miktar), ref miktar, value);
            if (oldValue != value) HesaplaTeklifTutar();
        }
    }

    [DbType("decimal(28,6)")]
    [XafDisplayName("Teklif Birim Fiyatı")]
    [RuleValueComparison("Rule_SatinAlmaTeklifD_TeklifBirimFiyat_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Lütfen teklif birim fiyatını sıfırdan büyük giriniz.")]
    public decimal TeklifBirimFiyat
    {
        get => teklifBirimFiyat;
        set
        {
            decimal oldValue = teklifBirimFiyat;
            SetPropertyValue(nameof(TeklifBirimFiyat), ref teklifBirimFiyat, value);
            if (oldValue != value) HesaplaTeklifTutar();
        }
    }

    [DbType("decimal(18,2)")]
    [XafDisplayName("Teklif Tutarı")]
    [Appearance("ED_SatinAlmaTeklifD_TeklifTutar", Enabled = false, Context = "Any")]
    public decimal TeklifTutar
    {
        get => teklifTutar;
        set => SetPropertyValue(nameof(TeklifTutar), ref teklifTutar, value);
    }

    [XafDisplayName("Teklif Teslim Süresi (Gün)")]
    [ModelDefault("DisplayFormat", "{0:N0}")]
    [ModelDefault("EditMask", "N0")]
    public int? TeklifTeslimSuresi
    {
        get => teklifTeslimSuresi;
        set => SetPropertyValue(nameof(TeklifTeslimSuresi), ref teklifTeslimSuresi, value);
    }

    [Browsable(false)]
    public bool EnDusukFiyatMi
    {
        get => enDusukFiyatMi;
        set => SetPropertyValue(nameof(EnDusukFiyatMi), ref enDusukFiyatMi, value);
    }

    void HesaplaTeklifTutar()
    {
        TeklifTutar = System.Math.Round(Miktar * TeklifBirimFiyat, 2, System.MidpointRounding.AwayFromZero);
    }
}
