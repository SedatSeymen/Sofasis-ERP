/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : HesapTuru.cs
 * Oluşturma Tarihi : 08/20/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/20/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Kasa/Banka/Cari'nin kendi ayrı Hareketler ekranlarının hem
 *                    lookup filtrelemesi (KaynakHesap/KarsiHesap DataSourceCriteria)
 *                    hem de ListView üyeliği (bkz. Model.DesignedDiffs.xafml) BU
 *                    alana dayanır. Taban Hesap tablosunda tutulur, her alt sınıf
 *                    kendi AfterConstruction'ında SABİT değerini atar.
 * ****************************************************************************
 */

namespace SofasisERP.Module.BusinessObjects;

public enum HesapTuru
{
    Kasa,
    Banka,
    Cari
}
