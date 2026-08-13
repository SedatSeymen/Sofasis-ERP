using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using System.Collections.Generic;

namespace Sofasis.Module.Controllers
{
    // Kök neden: SatinAlmaMalKabulOlusturController / FaturaOlusturController popup'ları,
    // isRoot:false ile View.ObjectSpace'İ PAYLAŞARAK (ayrı/nested bir ObjectSpace AÇMADAN) yeni
    // bir StokHareketleriM/FaturaM nesnesi oluşturur (2 adımlı commit deseni gereği — bkz. o
    // controller'lardaki yorumlar). Bu popup'ın kendi "İptal" düğmesi (ModificationsController.
    // CancelAction), PAYLAŞILAN ObjectSpace'i geri almaz (Rollback ETMEZ) — çünkü bunu yapmak dış
    // (Sipariş/Mal Kabul) ekranın KENDİ bekleyen değişikliklerini de silme riski taşırdı. Sonuç:
    // "İptal" tıklandığında oluşturulan nesne ObjectSpace'te "yeni" olarak asılı KALIR ve
    // kullanıcı SONRADAN başka bir işlemi başarıyla kaydettiğinde (örn. yeniden deneyip doğru
    // değerle kaydettiğinde) o eski/iptal edilmiş nesne de SESSİZCE aynı commit'e sürüklenir —
    // hayalet (istenmeyen) Mal Kabul/Fatura kaydı oluşur (canlı testte kanıtlandı, bkz.
    // docs/CHANGELOG.md). Bu controller, PAYLAŞILAN ObjectSpace kullanan popup ekranlarında
    // "İptal"e basıldığında o ekranın kök nesnesini (hâlâ yeniyse) ObjectSpace'ten AÇIKÇA siler —
    // [Aggregated] ilişkili satırlar (StokHareketleriDs/FaturaDs) XPO'nun kendi cascade-delete
    // davranışıyla otomatik silinir, ek kod gerekmez.
    public class PaylasimliObjectSpacePopupIptalTemizlikController : ViewController<DetailView>
    {
        static readonly HashSet<string> HedefViewIdleri = new HashSet<string>
        {
            "SatinAlmaMalKabul_DetailView",
            "FaturaM_DetailView",
        };

        ModificationsController modificationsController;

        protected override void OnActivated()
        {
            base.OnActivated();
            if (!HedefViewIdleri.Contains(View.Id)) return;

            modificationsController = Frame.GetController<ModificationsController>();
            if (modificationsController != null)
                modificationsController.CancelAction.Execute += CancelAction_Execute;
        }

        protected override void OnDeactivated()
        {
            if (modificationsController != null)
                modificationsController.CancelAction.Execute -= CancelAction_Execute;
            base.OnDeactivated();
        }

        void CancelAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            object currentObject = View.CurrentObject;
            if (currentObject != null && View.ObjectSpace.IsNewObject(currentObject))
                View.ObjectSpace.Delete(currentObject);
        }
    }
}
