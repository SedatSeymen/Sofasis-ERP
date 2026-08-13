using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Controllers
{
    // StokHareketleriMKayitliFisKilitleController'ın birebir portu — bir Fatura KAYDEDİLDİKTEN
    // SONRA tamamen immutable'dır (muhasebe defteri kaydı gibi; düzeltme İade Faturası girmekle
    // yapılır, mevcut kaydı değiştirmekle değil).
    public class FaturaKilitleController : ObjectViewController<DetailView, FaturaM>
    {
        const string ActiveKey = "FaturaKilitleController";

        ListPropertyEditor faturaKalemleriEditor;

        protected override void OnActivated()
        {
            base.OnActivated();
            View.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;

            if (View.FindItem(nameof(FaturaM.FaturaDs)) is ListPropertyEditor listEditor)
            {
                faturaKalemleriEditor = listEditor;
                if (listEditor.Frame != null)
                    UygulaSatirKilidi();
                else
                    listEditor.FrameChanged += ListPropertyEditor_FrameChanged;
            }

            UygulaMasterKilidi();
        }

        protected override void OnDeactivated()
        {
            if (faturaKalemleriEditor != null)
                faturaKalemleriEditor.FrameChanged -= ListPropertyEditor_FrameChanged;
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
            if (ViewCurrentObject == null || faturaKalemleriEditor?.Frame == null) return;
            bool kayitli = !ObjectSpace.IsNewObject(ViewCurrentObject);

            NewObjectViewController rowNewController = faturaKalemleriEditor.Frame.GetController<NewObjectViewController>();
            if (rowNewController != null)
                rowNewController.NewObjectAction.Active[ActiveKey] = !kayitli;

            DeleteObjectsViewController rowDeleteController = faturaKalemleriEditor.Frame.GetController<DeleteObjectsViewController>();
            if (rowDeleteController != null)
                rowDeleteController.DeleteAction.Active[ActiveKey] = !kayitli;
        }
    }
}
