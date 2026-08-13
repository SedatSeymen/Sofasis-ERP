using DevExpress.XtraReports.UI;

namespace Sofasis.Module.Reports;

public class CariHesapHareketMakbuzuReport : XtraReport
{
    public CariHesapHareketMakbuzuReport()
    {
        HareketMakbuzuReportBuilder.Build(this, "Sofasis.Module.BusinessObjects.CariHesapHareketleri");
    }
}
