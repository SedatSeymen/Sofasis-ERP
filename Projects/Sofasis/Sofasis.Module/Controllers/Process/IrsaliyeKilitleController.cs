using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Controllers
{
    // StokHareketleriMKayitliFisKilitleController'ın birebir portu — bir İrsaliye KAYDEDİLDİKTEN
    // SONRA tamamen immutable'dır (muhasebe defteri kaydı gibi; düzeltme yeni bir İrsaliye/ters
    // hareket girmekle yapılır, mevcut kaydı değiştirmekle değil).
    public class IrsaliyeKilitleController : ObjectViewController<DetailView, IrsaliyeM>
    {
        const string ActiveKey = "IrsaliyeKilitleController";

        ListPropertyEditor irsaliyeKalemleriEditor;

        protected override void OnActivated()
        {
            base.OnActivated();
            View.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;

            if (View.FindItem(nameof(IrsaliyeM.IrsaliyeDs)) is ListPropertyEditor listEditor)
            {
                irsaliyeKalemleriEditor = listEditor;
                if (listEditor.Frame != null)
                    UygulaSatirKilidi();
                else
                    listEditor.FrameChanged += ListPropertyEditor_FrameChanged;
            }

            UygulaMasterKilidi();
        }

        protected override void OnDeactivated()
        {
            if (irsaliyeKalemleriEditor != null)
                irsaliyeKalemleriEditor.FrameChanged -= ListPropertyEditor_FrameChanged;
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
            if (ViewCurrentObject == null || irsaliyeKalemleriEditor?.Frame == null) return;
            bool kayitli = !ObjectSpace.IsNewObject(ViewCurrentObject);

            NewObjectViewController rowNewController = irsaliyeKalemleriEditor.Frame.GetController<NewObjectViewController>();
            if (rowNewController != null)
                rowNewController.NewObjectAction.Active[ActiveKey] = !kayitli;

            DeleteObjectsViewController rowDeleteController = irsaliyeKalemleriEditor.Frame.GetController<DeleteObjectsViewController>();
            if (rowDeleteController != null)
                rowDeleteController.DeleteAction.Active[ActiveKey] = !kayitli;
        }
    }
}
