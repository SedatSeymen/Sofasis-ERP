/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : AyakTipi.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : AyakTanim.AyakTipi için sabit iki değerli enum.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum AyakTipi
{
    [XafDisplayName("Ahşap")]
    Ahsap,

    [XafDisplayName("Metal")]
    Metal
}
