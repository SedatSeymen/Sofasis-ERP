using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ReportsV2;

namespace Sofasis.Module.Controllers;

// DevExpress'in yerleşik "ShowInReport" action'ının Türkçe çevirisi ("Rapora göster") Model.DesignedDiffs.xafml
// üzerinden (ActionDesign/Actions/Action Id="ShowInReport" Caption="...") değiştirilmeye çalışıldı, ancak
// framework'ün bu action için uyguladığı yerleşik lokalizasyon her istek sonrasında modeldeki değeri geri
// eziyor gibi göründü. Kod tarafında Caption'ı doğrudan atamak garanti bir sonuç veriyor.
public class RenameShowInReportActionController : WindowController
{
    protected override void OnActivated()
    {
        base.OnActivated();
        PrintSelectionBaseController printSelectionController = Frame.GetController<PrintSelectionBaseController>();
        if (printSelectionController != null)
        {
            printSelectionController.ShowInReportAction.Caption = "Rapor Çalıştır";
        }
    }
}
