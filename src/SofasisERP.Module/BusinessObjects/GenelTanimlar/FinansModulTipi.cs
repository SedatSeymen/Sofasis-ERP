/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : FinansModulTipi.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : FisTuruTanim.FinansModulTipi için — eski ERP'den birebir
 *                    taşındı (tüm değerler, ileride eklenecek modüller için
 *                    şimdiden eklendi, StokHareketYonu Faz 2b'de eklenecek).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;

namespace SofasisERP.Module.BusinessObjects;

public enum FinansModulTipi
{
    [XafDisplayName("Yok")]
    Yok,

    [XafDisplayName("Stok")]
    Stok,

    [XafDisplayName("Cari")]
    Cari,

    // Faz 3 düzeltmesi (2026-08-19) — MuhasebeFisi'nin (birleşik Kasa/Banka/Cari
    // Yevmiye Fişi) FisTuruTanim seçiminde kullandığı tek modül değeri. Modül-özel
    // Kasa/Banka fiş türleri (KSTHSL/BNGLNH vb.) artık kullanılmıyor.
    [XafDisplayName("Yevmiye")]
    Yevmiye,

    [XafDisplayName("Fatura")]
    Fatura,

    [XafDisplayName("İrsaliye")]
    Irsaliye,

    [XafDisplayName("Sipariş")]
    Siparis,

    [XafDisplayName("Kasa")]
    Kasa,

    [XafDisplayName("Banka")]
    Banka,

    [XafDisplayName("Çek")]
    Cek,

    [XafDisplayName("Senet")]
    Senet,

    [XafDisplayName("Üretim")]
    Uretim,

    [XafDisplayName("Satın Alma")]
    SatinAlma,

    [XafDisplayName("Tahsilat/Ödeme")]
    TahsilatOdeme
}
