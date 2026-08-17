/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : UlkeTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Ülke tanımı — eski projeden uyarlandı. AdresTanim'e bağlı
 *                    silme koruması, bu projede AdresTanim henüz olmadığından
 *                    çıkarıldı.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(UlkeAdi))]
[DefaultClassOptions]
[XafDisplayName("Ülke Tanımlama")]
public class UlkeTanim : BaseClassWithAuditAndDescription
{
    public UlkeTanim(Session session) : base(session) { }

    bool isVarsayilan;
    string ulkeKodu;
    string ulkeAdi;
    string ulkeTelefonKodu;

    [XafDisplayName("Ülke Kodu"), ToolTip("ISO 3166-1 alfa-2 ülke kodu (örn. TR)")]
    [RuleRequiredField("RuleRequired_UlkeTanim_UlkeKodu", DefaultContexts.Save, "Lütfen Ülke Kodunu Giriniz...")]
    [Appearance("ED_UlkeTanim_UlkeKodu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [Size(3)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    public string UlkeKodu
    {
        get => ulkeKodu;
        set => SetPropertyValue(nameof(UlkeKodu), ref ulkeKodu, value);
    }

    [XafDisplayName("Ülke Adı"), ToolTip("Ülke Adını Giriniz")]
    [RuleRequiredField("RuleRequired_UlkeTanim_UlkeAdi", DefaultContexts.Save, "Lütfen Ülke Adını Giriniz...")]
    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    public string UlkeAdi
    {
        get => ulkeAdi;
        set => SetPropertyValue(nameof(UlkeAdi), ref ulkeAdi, value);
    }

    [XafDisplayName("Telefon Kodu"), ToolTip("Uluslararası telefon kodu (örn. +90)")]
    [Size(6)]
    public string UlkeTelefonKodu
    {
        get => ulkeTelefonKodu;
        set => SetPropertyValue(nameof(UlkeTelefonKodu), ref ulkeTelefonKodu, value);
    }

    [XafDisplayName("Şehir Tanımlama")]
    [Association("UlkeTanim-SehirTanims"), Aggregated]
    public XPCollection<SehirTanim> SehirTanims
    {
        get { return GetCollection<SehirTanim>(nameof(SehirTanims)); }
    }

    [XafDisplayName("Varsayılan mı ?")]
    public bool IsVarsayilan
    {
        get => isVarsayilan;
        set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
    }
}
