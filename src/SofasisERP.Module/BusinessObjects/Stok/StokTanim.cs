/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok kalemi ana tanım sınıfı — hem atomik ürün/bileşen hem
 *                    hammadde seviyesinde var olur (bkz. SOFASIS_ERP_MIMARI_
 *                    TASARIM.md §45.2 Katman 5). StokKodu, StokKoduJeneratoru
 *                    ile OnSaving'de otomatik üretilir.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using SofasisERP.Module.Services;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(StokAdi))]
[XafDisplayName("Stok Tanımlama")]
public class StokTanim : BaseClassWithAuditAndDescription
{
    public StokTanim(Session session) : base(session) { }

    string stokKodu;
    string stokAdi;
    string stokAdiIngilizce;
    string stokBarkodNo;
    MediaDataObject resim;
    bool satilabilirMi;
    StokAltGrupTanim stokAltGrubu;
    ModelTanim model;
    BirimTanim birimTanim;
    DovizTanim dovizTanim;
    KDVTanim kdvTanim;
    decimal? alisFiyati;
    decimal sonAlisFiyati;
    decimal en;
    decimal boy;
    decimal yukseklik;
    decimal metrekare;
    decimal metreKup;
    decimal agirlik;

    [Size(32)]
    [Indexed(Unique = true)]
    [Appearance("ED_StokTanim_StokKodu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [XafDisplayName("Stok Kodu")]
    public string StokKodu
    {
        get => stokKodu;
        set => SetPropertyValue(nameof(StokKodu), ref stokKodu, value);
    }

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_StokTanim_StokAdi", DefaultContexts.Save, "Lütfen Stok Adını Giriniz...")]
    [XafDisplayName("Stok Adı")]
    public string StokAdi
    {
        get => stokAdi;
        set => SetPropertyValue(nameof(StokAdi), ref stokAdi, value);
    }

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [VisibleInListView(false)]
    [XafDisplayName("Stok Adı İngilizce")]
    public string StokAdiIngilizce
    {
        get => stokAdiIngilizce;
        set => SetPropertyValue(nameof(StokAdiIngilizce), ref stokAdiIngilizce, value);
    }

    [Size(32)]
    [VisibleInListView(false)]
    [XafDisplayName("Barkod No")]
    public string StokBarkodNo
    {
        get => stokBarkodNo;
        set => SetPropertyValue(nameof(StokBarkodNo), ref stokBarkodNo, value);
    }

    // DB'de saklanır, gecikmeli yüklenir, tarayıcı tarafında önbelleklenir
    // (ApplicationUser.Resim/ModelTanim.Resim ile aynı desen). Resim alanı
    // Sipariş/Model/Stok üçlüsünde zorunlu olarak kararlaştırıldı (§45.2).
    [ImageEditor(ListViewImageEditorMode = ImageEditorMode.PictureEdit, ListViewImageEditorCustomHeight = 32)]
    [RuleRequiredField("RuleRequired_StokTanim_Resim", DefaultContexts.Save, "Lütfen Resim Ekleyiniz...")]
    [XafDisplayName("Resim")]
    public MediaDataObject Resim
    {
        get => resim;
        set => SetPropertyValue(nameof(Resim), ref resim, value);
    }

    // Hammadde/bileşen bile doğrudan satılabilir olabilir (ör. kumaş, ekstra
    // kırlent) — bkz. §45.2 "Satış birimi esnekliği hammaddeye kadar genişliyor".
    [XafDisplayName("Satılabilir mi?")]
    public bool SatilabilirMi
    {
        get => satilabilirMi;
        set => SetPropertyValue(nameof(SatilabilirMi), ref satilabilirMi, value);
    }

    [RuleRequiredField("RuleRequired_StokTanim_StokAltGrubu", DefaultContexts.Save, "Lütfen Stok Alt Grubunu Seçiniz...")]
    [XafDisplayName("Stok Alt Grubu")]
    public StokAltGrupTanim StokAltGrubu
    {
        get => stokAltGrubu;
        set => SetPropertyValue(nameof(StokAltGrubu), ref stokAltGrubu, value);
    }

    // Yalnızca StokAltGrubu'nun bağlı olduğu StokTipiTanim.MamulMu=Evet ise
    // görünür/zorunlu (eski projedeki "StokTipi=Mamul ise Model alanı görünür"
    // deseninin veriye dayalı karşılığı — bkz. StokTipiTanim.MamulMu).
    [Appearance("IsVisible_StokTanim_Model", Visibility = ViewItemVisibility.Hide,
        Criteria = "StokAltGrubu.StokGrupTanim.StokTipiTanim.MamulMu != True", Context = "DetailView")]
    [XafDisplayName("Model")]
    public ModelTanim Model
    {
        get => model;
        set => SetPropertyValue(nameof(Model), ref model, value);
    }

    [RuleRequiredField("RuleRequired_StokTanim_BirimTanim", DefaultContexts.Save, "Lütfen Birimi Seçiniz...")]
    [XafDisplayName("Birim")]
    public BirimTanim BirimTanim
    {
        get => birimTanim;
        set => SetPropertyValue(nameof(BirimTanim), ref birimTanim, value);
    }

    [VisibleInListView(false)]
    [XafDisplayName("Döviz")]
    public DovizTanim DovizTanim
    {
        get => dovizTanim;
        set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
    }

    [VisibleInListView(false)]
    [XafDisplayName("KDV Oranı")]
    public KDVTanim KDVTanim
    {
        get => kdvTanim;
        set => SetPropertyValue(nameof(KDVTanim), ref kdvTanim, value);
    }

    // Manuel/referans "liste" fiyatıdır — Faz 2'de maliyet motorunun hesaplayacağı
    // OrtalamaMaliyet ile KARIŞTIRILMAMALI.
    [VisibleInListView(false)]
    [XafDisplayName("Alış Fiyatı")]
    public decimal? AlisFiyati
    {
        get => alisFiyati;
        set => SetPropertyValue(nameof(AlisFiyati), ref alisFiyati, value);
    }

    // Kullanıcı kararı: bu alan ELLE girilmez — Satınalma Faturası onay akışı
    // (Faz 2/5, henüz bu projede yok) bu alanı otomatik günceller. Şimdilik
    // salt-okunur kalır; ileride ilgili Fatura onay Controller'ı buraya yazacak.
    [VisibleInListView(false)]
    [Appearance("ED_StokTanim_SonAlisFiyati", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Son Alış Fiyatı")]
    public decimal SonAlisFiyati
    {
        get => sonAlisFiyati;
        set => SetPropertyValue(nameof(SonAlisFiyati), ref sonAlisFiyati, value);
    }

    [VisibleInListView(false)]
    [XafDisplayName("En (Cm)")]
    public decimal En
    {
        get => en;
        set { if (SetPropertyValue(nameof(En), ref en, value)) HesaplaOlculer(); }
    }

    [VisibleInListView(false)]
    [XafDisplayName("Boy (Cm)")]
    public decimal Boy
    {
        get => boy;
        set { if (SetPropertyValue(nameof(Boy), ref boy, value)) HesaplaOlculer(); }
    }

    [VisibleInListView(false)]
    [XafDisplayName("Yükseklik (Cm)")]
    public decimal Yukseklik
    {
        get => yukseklik;
        set { if (SetPropertyValue(nameof(Yukseklik), ref yukseklik, value)) HesaplaOlculer(); }
    }

    [VisibleInListView(false)]
    [Appearance("ED_StokTanim_Metrekare", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Metrekare")]
    public decimal Metrekare
    {
        get => metrekare;
        set => SetPropertyValue(nameof(Metrekare), ref metrekare, value);
    }

    [VisibleInListView(false)]
    [Appearance("ED_StokTanim_MetreKup", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Metreküp")]
    public decimal MetreKup
    {
        get => metreKup;
        set => SetPropertyValue(nameof(MetreKup), ref metreKup, value);
    }

    [VisibleInListView(false)]
    [XafDisplayName("Ağırlık")]
    public decimal Agirlik
    {
        get => agirlik;
        set => SetPropertyValue(nameof(Agirlik), ref agirlik, value);
    }

    void HesaplaOlculer()
    {
        if (En != 0 && Boy != 0)
            Metrekare = (En * Boy) / 10000;
        if (En != 0 && Boy != 0 && Yukseklik != 0)
            MetreKup = (En * Boy * Yukseklik) / 1000000;
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this) && string.IsNullOrEmpty(StokKodu) && StokAltGrubu != null)
        {
            IStokKoduJeneratoru jenerator = new StokKoduJeneratoru();
            StokKodu = jenerator.SonrakiStokKodu(Session, StokAltGrubu);
        }
        base.OnSaving();
    }
}
