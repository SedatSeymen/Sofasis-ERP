/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokBakiye.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : (StokTanim, Depo) bileşik anahtarlı depo bazlı bakiye özeti —
 *                    her ekranda tüm StokHareketleriD satırlarını toplamak yerine
 *                    StokHareketleriD.ObjectSaving/ObjectDeleting tarafından
 *                    artırılıp/azaltılan denormalize bir özet tablosu. Eski
 *                    projeden birebir taşındı (Faz 2).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Stok Bakiye")]
public class StokBakiye : BaseClassWithAudit
{
    public StokBakiye(Session session) : base(session) { }

    StokTanim stokTanim;
    DepoTanim depo;
    decimal miktar;

    // Bileşik unique index: IndexedAttribute yalnızca property/field hedefli olduğundan
    // "diğer alan" adı burada semicolon ile belirtilir (DevExpress XPO deseni) — (StokTanim,
    // Depo) çifti benzersiz olur.
    [Indexed("Depo", Unique = true)]
    [Appearance("ED_StokBakiye_StokTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [XafDisplayName("Stok")]
    public StokTanim StokTanim
    {
        get => stokTanim;
        set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
    }

    [Appearance("ED_StokBakiye_Depo", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [XafDisplayName("Depo")]
    public DepoTanim Depo
    {
        get => depo;
        set => SetPropertyValue(nameof(Depo), ref depo, value);
    }

    [Appearance("ED_StokBakiye_Miktar", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Miktar")]
    public decimal Miktar
    {
        get => miktar;
        set => SetPropertyValue(nameof(Miktar), ref miktar, value);
    }
}
