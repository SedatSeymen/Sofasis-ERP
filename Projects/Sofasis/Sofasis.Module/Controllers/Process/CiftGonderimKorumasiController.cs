using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;

namespace Sofasis.Module.Controllers
{
    // 3-ajan değerlendirme turunda (Endüstri Mühendisi + Sistem Analisti + Yazılımcı, 2026-08-13)
    // kanıtlanan proje-geneli boşluk: XAF Blazor, bir Action (Kaydet/Kaydet ve Kapat/Kaydet ve Yeni)
    // sunucu round-trip'i sürerken düğmeyi OTOMATİK devre dışı bırakmaz (DevExpress'in kendi
    // dokümantasyonu: "there may be a delay in client-server interactions. A user may click on the
    // element several times before the panel appears."). Bu proje hiçbir yerde bu koruma
    // uygulamıyordu — canlı testte kanıtlandı: art arda iki "Kaydet" tıklaması, PAYLAŞILAN
    // ObjectSpace'te (Mal Kabul/Fatura popup deseni) sahipsiz bir ikinci taslağın da aynı commit'e
    // sürüklenip mükerrer belge (StokHareketleriM/D) oluşturmasına yol açtı (bkz.
    // docs/CHANGELOG.md 2026-08-13, "çift-Kaydet-tıklama" bulgusu).
    //
    // TÜM DetailView'lar için tek noktadan koruma: Executing'te işlem zaten sürüyorsa yeni
    // çalıştırmayı e.Cancel=true ile durdurur (CommitChanges'e hiç girmeden), Executed'te bayrağı
    // sıfırlar. Bu, Servis katmanındaki (ör. SatinAlmaMalKabulServisi) idempotency güvenlik ağına
    // EK bir katmandır — ikisi birlikte "hem UI hem arka uç" savunması sağlar.
    public class CiftGonderimKorumasiController : ViewController<DetailView>
    {
        ModificationsController modificationsController;
        bool islemSuruyor;

        protected override void OnActivated()
        {
            base.OnActivated();
            modificationsController = Frame.GetController<ModificationsController>();
            if (modificationsController == null) return;

            Abone(modificationsController.SaveAction);
            Abone(modificationsController.SaveAndCloseAction);
            Abone(modificationsController.SaveAndNewAction);
        }

        protected override void OnDeactivated()
        {
            if (modificationsController != null)
            {
                Ayril(modificationsController.SaveAction);
                Ayril(modificationsController.SaveAndCloseAction);
                Ayril(modificationsController.SaveAndNewAction);
            }
            base.OnDeactivated();
        }

        // ActionBase kabul edilir — SaveAndNewAction bu XAF sürümünde SingleChoiceAction (SimpleAction
        // DEĞİL), ama Executing/Executed olayları ActionBase'den miras alındığından ortak tip yeterli.
        void Abone(ActionBase action)
        {
            if (action == null) return;
            action.Executing += Kaydet_Executing;
            action.Executed += Kaydet_Executed;
        }

        void Ayril(ActionBase action)
        {
            if (action == null) return;
            action.Executing -= Kaydet_Executing;
            action.Executed -= Kaydet_Executed;
        }

        void Kaydet_Executing(object sender, CancelEventArgs e)
        {
            if (islemSuruyor)
            {
                e.Cancel = true;
                return;
            }
            islemSuruyor = true;
        }

        void Kaydet_Executed(object sender, ActionBaseEventArgs e) => islemSuruyor = false;
    }
}
