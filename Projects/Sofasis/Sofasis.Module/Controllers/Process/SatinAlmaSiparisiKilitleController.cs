using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Controllers
{
    // StokHareketleriMKayitliFisKilitleController'ın birebir portu — bir Satın Alma Siparişi
    // KAYDEDİLDİKTEN SONRA tamamen immutable'dır (tedarikçiye gönderilmiş bir belge; düzeltme
    // Durum=İptalEdildi + yeni sipariş oluşturmakla yapılır, mevcut kaydı değiştirmekle değil).
    public class SatinAlmaSiparisiKilitleController : ObjectViewController<DetailView, SatinAlmaSiparisiM>
    {
        const string ActiveKey = "SatinAlmaSiparisiKilitleController";

        ListPropertyEditor siparisKalemleriEditor;

        protected override void OnActivated()
        {
            base.OnActivated();
            View.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;

            if (View.FindItem(nameof(SatinAlmaSiparisiM.SatinAlmaSiparisiDs)) is ListPropertyEditor listEditor)
            {
                siparisKalemleriEditor = listEditor;
                if (listEditor.Frame != null)
                    UygulaSatirKilidi();
                else
                    listEditor.FrameChanged += ListPropertyEditor_FrameChanged;
            }

            UygulaMasterKilidi();
        }

        protected override void OnDeactivated()
        {
            if (siparisKalemleriEditor != null)
                siparisKalemleriEditor.FrameChanged -= ListPropertyEditor_FrameChanged;
            View.ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
            base.OnDeactivated();
        }

        void ListPropertyEditor_FrameChanged(object sender, System.EventArgs e) => UygulaSatirKilidi();

        void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
        {
            if (e.Object != ViewCurrentObject) return;
            UygulaMasterKilidi();
            UygulaSatirKilidi();
        }

        void UygulaMasterKilidi()
        {
            if (ViewCurrentObject == null) return;
            bool kayitli = !ObjectSpace.IsNewObject(ViewCurrentObject);

            DeleteObjectsViewController masterDeleteController = Frame.GetController<DeleteObjectsViewController>();
            if (masterDeleteController != null)
                masterDeleteController.DeleteAction.Active[ActiveKey] = !kayitli;
        }

        void UygulaSatirKilidi()
        {
            if (ViewCurrentObject == null || siparisKalemleriEditor?.Frame == null) return;
            bool kayitli = !ObjectSpace.IsNewObject(ViewCurrentObject);

            NewObjectViewController rowNewController = siparisKalemleriEditor.Frame.GetController<NewObjectViewController>();
            if (rowNewController != null)
                rowNewController.NewObjectAction.Active[ActiveKey] = !kayitli;

            DeleteObjectsViewController rowDeleteController = siparisKalemleriEditor.Frame.GetController<DeleteObjectsViewController>();
            if (rowDeleteController != null)
                rowDeleteController.DeleteAction.Active[ActiveKey] = !kayitli;
        }
    }
}
