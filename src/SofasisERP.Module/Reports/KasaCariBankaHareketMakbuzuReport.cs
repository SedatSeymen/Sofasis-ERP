/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : KasaCariBankaHareketMakbuzuReport.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Tek bir hareket fişinin (Açılış/Tahsilat/Ödeme/Virman) yazdırılabilir
 *                    makbuz çıktısı — isInplaceReport=true ile kayıtlı (bkz. Module.cs),
 *                    DevExpress ReportsV2'nin yerleşik "Rapora Göster" (ShowInReport)
 *                    aksiyonu üzerinden her Cari/Kasa/Banka Hareketleri List/DetailView'da
 *                    otomatik sunulur — ayrı bir Controller/Action yazmaya gerek yok.
 * ****************************************************************************
 */

using DevExpress.XtraReports.UI;

namespace SofasisERP.Module.Reports;

public class KasaCariBankaHareketMakbuzuReport : XtraReport
{
    public KasaCariBankaHareketMakbuzuReport()
    {
        HareketMakbuzuReportBuilder.Build(this, "SofasisERP.Module.BusinessObjects.KasaCariBankaHareketleri");
    }
}
