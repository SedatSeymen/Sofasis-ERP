using DevExpress.XtraReports.UI;

namespace Sofasis.Module.Reports;

public class KasaBankaHareketMakbuzuReport : XtraReport
{
    public KasaBankaHareketMakbuzuReport()
    {
        HareketMakbuzuReportBuilder.Build(this, "Sofasis.Module.BusinessObjects.KasaBankaHareketleri");
    }
}
