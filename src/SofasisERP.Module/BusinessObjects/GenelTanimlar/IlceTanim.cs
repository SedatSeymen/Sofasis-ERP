/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : IlceTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : İlçe tanımı — eski projeden aynen. [DefaultClassOptions]
 *                    BİLEREK yok — saf Detail (Şehir'in Aggregated koleksiyonu
 *                    içinden düzenlenir), menüde ayrı görünmemeli.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(IlceAdi))]
[XafDisplayName("İlçe Tanımlama")]
public class IlceTanim : BaseClass
{
    public IlceTanim(Session session) : base(session) { }

    string ilceAdi;
    SehirTanim sehirTanim;

    [XafDisplayName("İlçe Adı"), ToolTip("İlçe Adını Giriniz")]
    [RuleRequiredField("RuleRequired_IlceTanim_IlceAdi", DefaultContexts.Save, "Lütfen İlçe Adını Giriniz...")]
    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    public string IlceAdi
    {
        get => ilceAdi;
        set => SetPropertyValue(nameof(IlceAdi), ref ilceAdi, value);
    }

    [Association("SehirTanim-IlceTanims")]
    [Appearance("InvisibleSehirAdi_IlceDetail", Visibility = ViewItemVisibility.Hide, Context = "DetailView")]
    [XafDisplayName("Şehir Adı")]
    public SehirTanim SehirTanim
    {
        get => sehirTanim;
        set => SetPropertyValue(nameof(SehirTanim), ref sehirTanim, value);
    }
}
