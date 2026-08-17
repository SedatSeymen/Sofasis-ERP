/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : OndalikBasamakSayisi.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : GenelParametre'de seçilen ondalık basamak sayısı (eski projeden aynen).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum OndalikBasamakSayisi
{
    [XafDisplayName("0 (#,##0)")]
    Basamak0 = 0,
    [XafDisplayName("1 (#,##0.0)")]
    Basamak1 = 1,
    [XafDisplayName("2 (#,##0.00)")]
    Basamak2 = 2,
    [XafDisplayName("3 (#,##0.000)")]
    Basamak3 = 3,
    [XafDisplayName("4 (#,##0.0000)")]
    Basamak4 = 4,
    [XafDisplayName("5 (#,##0.00000)")]
    Basamak5 = 5,
    [XafDisplayName("6 (#,##0.000000)")]
    Basamak6 = 6,
}
