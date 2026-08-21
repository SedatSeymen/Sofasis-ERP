/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokHareketleriDPopupBoyutController.cs
 * Oluşturma Tarihi : 08/19/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/19/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : "Stok Hareketi Satırı" popup'ı tek kolona geçirildikten
 *                    sonra kullanıcı isteği: genişliği 100px daralt, yüksekliği
 *                    25px arttır. Ölçülen varsayılan boyut ~1229x560 idi, hedef
 *                    ~1129x585. DevExpress KB'deki resmi desen: WindowController
 *                    + IPopupWindowTemplateSize (bkz. eXpressAppFramework/404014).
 * ****************************************************************************
 */

using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Blazor.Templates;

namespace SofasisERP.Blazor.Server.Controllers;

public class StokHareketleriDPopupBoyutController : WindowController
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
            size.Width = "800px";
            size.Height = "600px";
        }
    }

    protected override void OnDeactivated()
    {
        Window.TemplateChanged -= Window_TemplateChanged;
        base.OnDeactivated();
    }
}
