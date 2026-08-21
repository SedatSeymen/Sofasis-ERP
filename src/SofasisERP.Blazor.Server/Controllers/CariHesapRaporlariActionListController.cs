/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariHesapRaporlariActionListController.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Kullanıcı isteği (2026-08-22) — CariHesapTanim_ListView'daki
 *                    "Raporları" (PredefinedCategory.Reports) araç çubuğu grubunu
 *                    tek bir açılır menüye (ActionList/drop-down) dönüştürür.
 *                    DevExpress resmi deseni: BlazorRibbonController'ın
 *                    RibbonActionContainerCreating event'i ile ContainerId="Reports"
 *                    olan container'ı IsDropDown=true yapılır (bkz. DevExpress
 *                    dokümantasyonu "Group Actions in a Drop-Down Menu (ASP.NET
 *                    Core Blazor)"). Sadece CariHesapTanim_ListView için
 *                    uygulanır (Frame.View.Id kontrolü) — diğer ekranlardaki
 *                    (Kasa/Banka/Cari Hareketleri) "Raporları" grubu (Fişi
 *                    Yazdır/Cari Hesap Ekstresi) etkilenmez, kullanıcı kararı.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.SystemModule;
using DevExpress.Persistent.Base;

namespace SofasisERP.Blazor.Server.Controllers;

public class CariHesapRaporlariActionListController : WindowController
{
    protected override void OnActivated()
    {
        base.OnActivated();
        BlazorRibbonController controller = Frame.GetController<BlazorRibbonController>();
        if (controller != null)
        {
            controller.RibbonActionContainerCreating += Controller_RibbonActionContainerCreating;
        }
    }

    void Controller_RibbonActionContainerCreating(object sender, RibbonActionContainerCreatingEventArgs e)
    {
        if (Frame.View?.Id != "CariHesapTanim_ListView") return;
        if (e.ActionContainer.ContainerId != nameof(PredefinedCategory.Reports)) return;

        e.ActionContainer.IsDropDown = true;
        e.ActionContainer.Caption = "Cari Raporları";
    }

    protected override void OnDeactivated()
    {
        BlazorRibbonController controller = Frame.GetController<BlazorRibbonController>();
        if (controller != null)
        {
            controller.RibbonActionContainerCreating -= Controller_RibbonActionContainerCreating;
        }
        base.OnDeactivated();
    }
}
