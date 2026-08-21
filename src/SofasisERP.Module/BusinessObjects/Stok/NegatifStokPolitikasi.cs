/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : NegatifStokPolitikasi.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : StokParametre.NegatifStokPolitikasi için — bir çıkış
 *                    hareketi bakiyeyi negatife düşürdüğünde sistemin tepkisi.
 *                    Eski ERP'den birebir taşındı.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum NegatifStokPolitikasi
{
    [XafDisplayName("İzin Ver")]
    Izinver,

    [XafDisplayName("Uyar")]
    Uyar,

    [XafDisplayName("Engelle")]
    Engelle
}
