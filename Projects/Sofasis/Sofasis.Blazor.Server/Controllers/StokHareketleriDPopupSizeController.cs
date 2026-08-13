using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Templates;

namespace Sofasis.Blazor.Server.Controllers;

// Stok Hareketi Satırı popup'ı (StokHareketleriD_DetailView) tek kolonlu/dar bir form olarak
// tasarlandı (bkz. Model.DesignedDiffs.xafml) — varsayılan popup genişliği bu dar içerik için
// gereksiz boşluk bırakıyordu, bu yüzden yalnızca bu View için daraltılır.
public class StokHareketleriDPopupSizeController : WindowController
{
    protected override void OnActivated()
    {
        base.OnActivated();
        Window.TemplateChanged += Window_TemplateChanged;
    }

    void Window_TemplateChanged(object sender, EventArgs e)
    {
        if (Window.Template is IPopupWindowTemplateSize size && Window.View?.Id == "StokHareketleriD_DetailView")
        {
            size.Width = "640px";
            size.MaxWidth = "90vw";
            size.Height = "480px";
            size.MaxHeight = "90vh";
        }
    }

    protected override void OnDeactivated()
    {
        Window.TemplateChanged -= Window_TemplateChanged;
        base.OnDeactivated();
    }
}
