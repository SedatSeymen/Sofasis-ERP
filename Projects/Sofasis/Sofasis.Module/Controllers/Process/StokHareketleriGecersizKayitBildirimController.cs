using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Controllers
{
    // Var olmayan bir Oid ile bir StokHareketleriM/StokTransferi DetailView URL'sine doğrudan
    // gidildiğinde (ör. silinmiş bir kayda eski bir bağlantı), View.CurrentObject null kalır ve
    // XAF sayfayı sessizce boş render eder — kullanıcı hiçbir geri bildirim almaz (canlı testte
    // bulundu). Bu, en azından anlaşılır bir uyarı gösterir.
    public class StokHareketleriGecersizKayitBildirimController : ViewController<DetailView>
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            if (View.CurrentObject == null &&
                (typeof(StokHareketleriM).IsAssignableFrom(View.ObjectTypeInfo.Type) ||
                 typeof(StokTransferi).IsAssignableFrom(View.ObjectTypeInfo.Type)))
            {
                Application.ShowViewStrategy.ShowMessage(
                    "Aradığınız kayıt bulunamadı — silinmiş olabilir veya bağlantı hatalı.",
                    InformationType.Warning);
            }
        }
    }
}
