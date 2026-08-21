/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : KDVTipi.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Eski ERP'den birebir taşındı — CariHesapTanim'e özel.
 *                    StokTanim'deki KDVDahilMi (bool) alanından bilerek farklı;
 *                    kullanıcı kararı (2026-08-18): Cari Hesap kartı eski ERP'den
 *                    aynen taşınıyor.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum KDVTipi
{
    [XafDisplayName("KDV Hariç")]
    KDVHaric,

    [XafDisplayName("KDV Dahil")]
    KDVDahil
}
