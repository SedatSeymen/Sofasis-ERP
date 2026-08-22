/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : ToptanPerakendeTipi.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Cari Hesap kod hiyerarşisinin 3. seviyesinde (CariHesapAltGrupTanim)
 *                    kullanılır — bkz. CariHesapKoduJeneratoru.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum ToptanPerakendeTipi
{
    [XafDisplayName("Toptan")]
    Toptan,

    [XafDisplayName("Perakende")]
    Perakende
}
