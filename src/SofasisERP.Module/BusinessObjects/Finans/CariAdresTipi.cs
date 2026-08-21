/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariAdresTipi.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : CariAdresTanim.AdresTipi için — kullanıcı isteği (2026-08-18).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum CariAdresTipi
{
    [XafDisplayName("Fatura Adresi")]
    Fatura,

    [XafDisplayName("Ev Adresi")]
    Ev,

    [XafDisplayName("Şirket Adresi")]
    Sirket
}
