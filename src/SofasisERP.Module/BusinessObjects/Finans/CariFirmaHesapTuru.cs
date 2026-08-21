/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariFirmaHesapTuru.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Eski ERP'den birebir taşındı.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum CariFirmaHesapTuru
{
    [XafDisplayName("Şahıs Firması")]
    Sahis,

    [XafDisplayName("Limited Şirket")]
    LimitedSirket,

    [XafDisplayName("Anonim Şirket")]
    AnonimSirket
}
