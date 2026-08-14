using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;

namespace Sofasis.Module.Controllers
{
    // 4-ajan turunda (Endüstri Mühendisi + Mali Müşavir) bağımsız bulunan boşluk: yanlış girilmiş,
    // henüz teslimat almamış bir Siparişin tek düzeltme yolu veritabanına elle müdahaleydi — ADR-018
    // (İrsaliye/Fatura İade) ile aynı sınıf. Mantığın tamamı ISatinAlmaSiparisServisi'nde (ADR-006);
    // bu controller yalnızca "İptal Et" aksiyonunu ekler.
    public class SatinAlmaSiparisiIptalController : ObjectViewController<DetailView, SatinAlmaSiparisiM>
    {
        const string ActiveKey = "SatinAlmaSiparisiIptalController";

        readonly ISatinAlmaSiparisServisi siparisServisi = new SatinAlmaSiparisServisi();

        SimpleAction iptalEtAction;

        public SatinAlmaSiparisiIptalController()
        {
            iptalEtAction = new SimpleAction(this, "SatinAlmaSiparisiIptalEt", PredefinedCategory.View)
            {
                Caption = "İptal Et",
                ImageName = "Action_Delete"
            };
            iptalEtAction.Execute += IptalEtAction_Execute;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            View.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
            UygulaAktiflik();
        }

        protected override void OnDeactivated()
        {
            View.ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
            base.OnDeactivated();
        }

        void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
        {
            if (e.Object == ViewCurrentObject)
                UygulaAktiflik();
        }

        void UygulaAktiflik()
        {
            if (ViewCurrentObject == null) return;
            // v1: yalnızca hiç teslimat/mal kabul almamış (Verildi) Siparişler iptal edilebilir —
            // kısmi/tam teslimat almış bir Siparişin iptali stok/muhasebe etkisini geri almayı
            // gerektirir (YAGNI, kapsam dışı; kısmen teslim alınmış bir sipariş için düzeltme zaten
            // İrsaliye/Fatura İade akışıyla, ADR-018, yapılabiliyor).
            iptalEtAction.Active[ActiveKey] =
                !ObjectSpace.IsNewObject(ViewCurrentObject) &&
                ViewCurrentObject.Durum == SatinAlmaSiparisDurumu.Verildi;
        }

        void IptalEtAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            siparisServisi.IptalEt(ViewCurrentObject);
            ObjectSpace.CommitChanges();
            View.Refresh();
        }
    }
}
