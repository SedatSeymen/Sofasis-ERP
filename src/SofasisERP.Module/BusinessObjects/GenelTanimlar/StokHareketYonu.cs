/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokHareketYonu.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : FisTuruTanim.StokHareketYonu için — FinansBorcAlacakTipi'nin
 *                    Stok modülü karşılığı, eski ERP'den birebir taşındı.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum StokHareketYonu
{
    [XafDisplayName("Yok")]
    Yok,

    [XafDisplayName("Giriş")]
    Giris,

    [XafDisplayName("Çıkış")]
    Cikis
}
