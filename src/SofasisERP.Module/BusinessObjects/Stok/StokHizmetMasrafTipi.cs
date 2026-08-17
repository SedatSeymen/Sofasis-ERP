/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokHizmetMasrafTipi.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : StokTanim kaydının temel türü — eski projeden aynen taşınan
 *                    ayrım (bkz. Sofasis.Module StokHizmetMasrafTipi). StokTipi/
 *                    StokGrubu/StokAltGrubu hiyerarşisinden BAĞIMSIZDIR: o hiyerarşi
 *                    yalnızca fiziksel STOK sınıflandırması içindir; Hizmet ve
 *                    Masraf kalemlerinin fiziksel stok hiyerarşisine ihtiyacı yoktur.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum StokHizmetMasrafTipi
{
    [XafDisplayName("Stok")]
    Stok,

    [XafDisplayName("Hizmet")]
    Hizmet,

    [XafDisplayName("Masraf")]
    Masraf
}
