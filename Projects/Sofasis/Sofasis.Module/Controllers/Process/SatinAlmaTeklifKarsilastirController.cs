using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;
using System.Linq;

namespace Sofasis.Module.Controllers
{
    // Karşılaştırma mantığının tamamı ISatinAlmaTeklifServisi'nde (ADR-006) — bu controller
    // yalnızca Talep DetailView'ına "Teklifleri Karşılaştır" aksiyonunu ekler.
    public class SatinAlmaTeklifKarsilastirController : ObjectViewController<DetailView, SatinAlmaTalebiM>
    {
        const string ActiveKey = "SatinAlmaTeklifKarsilastirController";

        readonly ISatinAlmaTeklifServisi teklifServisi = new SatinAlmaTeklifServisi();

        SimpleAction karsilastirAction;

        public SatinAlmaTeklifKarsilastirController()
        {
            karsilastirAction = new SimpleAction(this, "SatinAlmaTeklifleriKarsilastir", PredefinedCategory.RecordEdit)
            {
                Caption = "Teklifleri Karşılaştır"
            };
            karsilastirAction.Execute += KarsilastirAction_Execute;
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
            karsilastirAction.Active[ActiveKey] = ViewCurrentObject.SatinAlmaTeklifleri.Any();
        }

        void KarsilastirAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            teklifServisi.Karsilastir(ViewCurrentObject);
            ObjectSpace.CommitChanges();
            View.Refresh();
        }
    }
}
