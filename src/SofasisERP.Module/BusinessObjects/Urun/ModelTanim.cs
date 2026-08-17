/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : ModelTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Paylaşılan modül havuzunun sahibi olan üst çatı (ör. "Kiev").
 *                    Kendisi doğrudan satılmaz — bkz. SOFASIS_ERP_MIMARI_TASARIM.md
 *                    §45.2 Katman 1. Minimal başlangıç hâli: sadece StokTanim'in
 *                    Model referansını karşılamak için — Modül/Konfigürasyon
 *                    ilişkileri ayrı bir oturumda genişletilecek.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(ModelAdi))]
[XafDisplayName("Model Tanımlama")]
public class ModelTanim : BaseClassWithAuditAndDescription
{
    public ModelTanim(Session session) : base(session) { }

    string modelKodu;
    string modelAdi;
    MediaDataObject resim;

    [Size(20)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_ModelTanim_ModelKodu", DefaultContexts.Save, "Lütfen Model Kodunu Giriniz...")]
    [XafDisplayName("Model Kodu")]
    public string ModelKodu
    {
        get => modelKodu;
        set => SetPropertyValue(nameof(ModelKodu), ref modelKodu, value);
    }

    [Size(50)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_ModelTanim_ModelAdi", DefaultContexts.Save, "Lütfen Model Adını Giriniz...")]
    [XafDisplayName("Model Adı")]
    public string ModelAdi
    {
        get => modelAdi;
        set => SetPropertyValue(nameof(ModelAdi), ref modelAdi, value);
    }

    // DB'de saklanır, gecikmeli yüklenir, tarayıcı tarafında önbelleklenir
    // (ApplicationUser.Resim ile aynı desen).
    [ImageEditor(ListViewImageEditorMode = ImageEditorMode.PictureEdit, ListViewImageEditorCustomHeight = 32)]
    [XafDisplayName("Resim")]
    public MediaDataObject Resim
    {
        get => resim;
        set => SetPropertyValue(nameof(Resim), ref resim, value);
    }
}
