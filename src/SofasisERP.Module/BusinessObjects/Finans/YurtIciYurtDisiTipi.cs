/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : YurtIciYurtDisiTipi.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Cari Hesap kod hiyerarşisinin 2. seviyesinde (CariHesapGrupTanim)
 *                    kullanılır — bkz. CariHesapKoduJeneratoru.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum YurtIciYurtDisiTipi
{
    [XafDisplayName("Yurtiçi")]
    YurtIci,

    [XafDisplayName("Yurtdışı")]
    YurtDisi
}
