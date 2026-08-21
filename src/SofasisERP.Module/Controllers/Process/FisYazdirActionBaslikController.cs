/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : FisYazdirActionBaslikController.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : DevExpress ReportsV2'nin yerleşik "ShowInReport" aksiyonunun
 *                    başlığını "Fişi Yazdır" yapar. Model.DesignedDiffs.xafml üzerinden
 *                    (ActionDesign/Actions/Action Id="ShowInReport" Caption="...") caption
 *                    değiştirmek denenmedi — eski ERP'de bu yaklaşımın framework'ün yerleşik
 *                    lokalizasyonu tarafından her istekte ezildiği (bkz. eski ERP
 *                    RenameShowInReportActionController.cs) zaten kanıtlanmıştı; koddan
 *                    doğrudan atamak garanti sonuç veriyor.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ReportsV2;

namespace SofasisERP.Module.Controllers.Process;

public class FisYazdirActionBaslikController : WindowController
{
    protected override void OnActivated()
    {
        base.OnActivated();
        PrintSelectionBaseController printSelectionController = Frame.GetController<PrintSelectionBaseController>();
        if (printSelectionController != null)
        {
            printSelectionController.ShowInReportAction.Caption = "Fişi Yazdır";
        }
    }
}
