/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariHesapTipi.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Eski ERP'den birebir taşındı.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum CariHesapTipi
{
    [XafDisplayName("Müşteri")]
    Musteri,

    [XafDisplayName("Tedarikçi")]
    Tedarikci,

    [XafDisplayName("Müşteri + Tedarikçi")]
    MusteriTedarikci
}
