/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : FinansBorcAlacakTipi.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : FisTuruTanim.FinansBorcAlacakTipi için — eski ERP'den
 *                    birebir taşındı.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum FinansBorcAlacakTipi
{
    [XafDisplayName("Yok")]
    Yok,

    [XafDisplayName("Borç")]
    Borc,

    [XafDisplayName("Alacak")]
    Alacak,

    [XafDisplayName("Borç/Alacak")]
    BorcAlacak
}
