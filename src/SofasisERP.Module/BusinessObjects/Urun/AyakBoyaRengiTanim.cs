/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : AyakBoyaRengiTanim.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : AyakTanim'in Aggregated detayı — saf Detail, [DefaultClassOptions]
 *                    BİLEREK yok, yalnızca AyakTanim'in koleksiyonundan düzenlenir
 *                    (IlceTanim/StokAltGrupTanim ile aynı desen).
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

[DefaultProperty(nameof(BoyaRengiAdi))]
[XafDisplayName("Ayak Boya Rengi Tanımlama")]
public class AyakBoyaRengiTanim : BaseClass
{
    public AyakBoyaRengiTanim(Session session) : base(session) { }

    string boyaRengiAdi;
    AyakTanim ayakTanim;

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [Indexed(nameof(AyakTanim), Unique = true)]
    [RuleRequiredField("RuleRequired_AyakBoyaRengiTanim_BoyaRengiAdi", DefaultContexts.Save, "Lütfen Boya Rengi Adını Giriniz...")]
    [XafDisplayName("Boya Rengi Adı")]
    public string BoyaRengiAdi
    {
        get => boyaRengiAdi;
        set => SetPropertyValue(nameof(BoyaRengiAdi), ref boyaRengiAdi, value);
    }

    [Association("AyakTanim-AyakBoyaRenkleri")]
    [Appearance("InvisibleAyakTanim_BoyaRengiDetail", Visibility = ViewItemVisibility.Hide, Context = "DetailView")]
    [XafDisplayName("Ayak Tanımı")]
    public AyakTanim AyakTanim
    {
        get => ayakTanim;
        set => SetPropertyValue(nameof(AyakTanim), ref ayakTanim, value);
    }
}
