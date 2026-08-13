using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using System;
using System.Linq;

namespace Sofasis.Module.Controllers
{
    // "Uyar" politikasında (StokParametre.NegatifStokPolitikasi) bir satır bakiyeyi negatife
    // düşürdüğünde, satır kırmızı/kalın işaretlenip NegatifBakiyeUyarisi=true olarak kaydediliyor
    // ama bu kolayca gözden kaçabiliyordu (canlı testte bulundu). Kayıt başarıyla commit
    // edildikten SONRA, fişte böyle bir satır varsa kullanıcıya görünür bir bilgi mesajı gösterir.
    public class StokHareketleriMNegatifBakiyeBildirimController : ObjectViewController<DetailView, StokHareketleriM>
    {
        protected override void OnActivated()
        {
            base.OnActivated();
            View.ObjectSpace.Committed += ObjectSpace_Committed;
        }

        protected override void OnDeactivated()
        {
            View.ObjectSpace.Committed -= ObjectSpace_Committed;
            base.OnDeactivated();
        }

        void ObjectSpace_Committed(object sender, EventArgs e)
        {
            if (ViewCurrentObject == null) return;
            if (ViewCurrentObject.StokHareketleriDs.Any(d => d.NegatifBakiyeUyarisi))
            {
                Application.ShowViewStrategy.ShowMessage(
                    "Dikkat: Bu fişteki bir veya daha fazla satır, ilgili depodaki stok bakiyesini negatife düşürdü.",
                    InformationType.Warning);
            }
        }
    }
}
