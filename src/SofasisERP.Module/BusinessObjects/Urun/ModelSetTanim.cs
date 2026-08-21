/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : ModelSetTanim.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Fiilen satılan birim (eski adıyla "Konfigürasyon") — Model'in
 *                    paylaşılan modül havuzundan (Mamul StokTanim kayıtları) belirli
 *                    bir altkümesini bir araya getirir (ör. "Kiev Takım", "Kiev
 *                    Köşe"). Kullanıcı kararı (2026-08-18): ModelTanim'den Model Adı
 *                    seçilir, Set Adı elle girilir. Fiyat alanı BİLEREK YOK (kullanıcı
 *                    kararı) — Set fiyatı her zaman parça StokTanim.SatisFiyati'larının
 *                    toplamından gelir, ayrı bir bundle fiyatı tutulmaz.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[DefaultProperty(nameof(SetAdi))]
[XafDisplayName("Model Set Tanımlama")]
public class ModelSetTanim : BaseClassWithAudit
{
    public ModelSetTanim(Session session) : base(session) { }

    ModelTanim modelTanim;
    string setAdi;

    [RuleRequiredField("RuleRequired_ModelSetTanim_ModelTanim", DefaultContexts.Save, "Lütfen Model Adını Seçiniz...")]
    [XafDisplayName("Model Adı")]
    public ModelTanim ModelTanim
    {
        get => modelTanim;
        set => SetPropertyValue(nameof(ModelTanim), ref modelTanim, value);
    }

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [RuleRequiredField("RuleRequired_ModelSetTanim_SetAdi", DefaultContexts.Save, "Lütfen Set Adını Giriniz...")]
    [RuleUniqueValue("RuleUniqueValue_ModelSetTanim_SetAdi", DefaultContexts.Save, "Bu Set Adı zaten kullanılıyor.")]
    [XafDisplayName("Set Adı")]
    public string SetAdi
    {
        get => setAdi;
        set => SetPropertyValue(nameof(SetAdi), ref setAdi, value);
    }

    [XafDisplayName("Set Detayları")]
    [Association("ModelSetTanim-ModelSetDetaylari"), Aggregated]
    public XPCollection<ModelSetDetay> ModelSetDetaylari
        => GetCollection<ModelSetDetay>(nameof(ModelSetDetaylari));
}
