/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : SehirTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Şehir tanımı — eski projeden uyarlandı. AdresTanim'e bağlı
 *                    silme koruması çıkarıldı (AdresTanim bu projede henüz yok).
 *                    Kullanıcı kararı (2026-08-17): Genel Tanımlar menüsünde ayrı
 *                    bir "Şehir Tanımlama" ekranı YOK — Ülke Tanımlama'nın
 *                    Aggregated koleksiyonundan (UlkeTanim.SehirTanims) düzenlenir,
 *                    bu yüzden [DefaultClassOptions] KALDIRILDI (IlceTanim'in
 *                    SehirTanim'e göre zaten sahip olduğu desenle aynı).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(SehirAdi))]
[XafDisplayName("Şehir Tanımlama")]
public class SehirTanim : BaseClass
{
    public SehirTanim(Session session) : base(session) { }

    string sehirAdi;
    string plakaKodu;
    UlkeTanim ulkeTanim;

    [RuleUniqueValue]
    [Indexed(Unique = true)]
    [XafDisplayName("Şehir Adı"), ToolTip("Şehir Adını Giriniz")]
    [RuleRequiredField("RuleRequired_SehirTanim_SehirAdi", DefaultContexts.Save, "Lütfen Şehir Adını Giriniz...")]
    [Appearance("ED_SehirTanim_SehirAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    public string SehirAdi
    {
        get => sehirAdi;
        set => SetPropertyValue(nameof(SehirAdi), ref sehirAdi, value);
    }

    [XafDisplayName("Plaka Kodu"), ToolTip("Şehir Plaka Kodu (örn. 34)")]
    [Size(3)]
    public string PlakaKodu
    {
        get => plakaKodu;
        set => SetPropertyValue(nameof(PlakaKodu), ref plakaKodu, value);
    }

    [Association("UlkeTanim-SehirTanims")]
    [Appearance("InvisibleUlkeAdi", Visibility = ViewItemVisibility.Hide, Context = "DetailView")]
    [XafDisplayName("Ülke Adı")]
    public UlkeTanim UlkeTanim
    {
        get => ulkeTanim;
        set => SetPropertyValue(nameof(UlkeTanim), ref ulkeTanim, value);
    }

    [XafDisplayName("İlçe Tanımlama")]
    [Association("SehirTanim-IlceTanims"), Aggregated]
    public XPCollection<IlceTanim> IlceTanims
    {
        get { return GetCollection<IlceTanim>(nameof(IlceTanims)); }
    }
}
