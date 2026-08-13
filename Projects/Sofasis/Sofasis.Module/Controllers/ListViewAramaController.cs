/* ****************************************************************************
 * Proje     : Sofasis Erp Project
 * Dosya     : ListViewAramaController.cs
 * Açıklama  : ROOT (navigasyondan doğrudan açılan, tam ekran) liste görünümlerinde
 *             metne göre arama kutusunu (Find Panel) otomatik açar. Blazor'da
 *             DevExpress DxGrid'in arama kutusu olarak render edilir.
 *             Master-detail içindeki NESTED ListView'lerde (ör. Talep/Teklif/
 *             Sipariş Kalemleri gibi bir DetailView'a gömülü ListPropertyEditor
 *             ızgaraları) arama kutusu KAPALI tutulur — kullanıcı isteği: az
 *             satırlı bu ızgaralarda arama kutusu gereksiz görsel gürültü.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;

namespace Sofasis.Module.Controllers
{
    public class ListViewAramaController : ViewController<ListView>
    {
        protected override void OnActivated()
        {
            base.OnActivated();

            // Model, Find Panel'i destekliyorsa: yalnızca root ListView'lerde göster.
            if (View?.Model is IModelListViewShowFindPanel model)
            {
                model.ShowFindPanel = View.IsRoot;
            }
        }
    }
}
