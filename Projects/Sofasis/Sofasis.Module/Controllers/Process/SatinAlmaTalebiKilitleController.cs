using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Controllers
{
    // StokHareketleriMKayitliFisKilitleController'ın birebir portu (ListPropertyEditor.FrameChanged
    // inceliği dahil) — tek fark kilit koşulu: Stok'ta "artık yeni değil" yeterliyken burada Talep
    // Taslak durumundan çıktığı andan (Onaya Gönder) itibaren kilitlenir, salt "kaydedilmiş olmak"
    // değil — bir Taslak birden fazla kez kaydedilip düzenlenebilmeli.
    public class SatinAlmaTalebiKilitleController : ObjectViewController<DetailView, SatinAlmaTalebiM>
    {
        const string ActiveKey = "SatinAlmaTalebiKilitleController";

        ListPropertyEditor talepKalemleriEditor;

        protected override void OnActivated()
        {
            base.OnActivated();
            View.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;

            if (View.FindItem(nameof(SatinAlmaTalebiM.SatinAlmaTalebiDs)) is ListPropertyEditor listEditor)
            {
                talepKalemleriEditor = listEditor;
                if (listEditor.Frame != null)
                    UygulaSatirKilidi();
                else
                    listEditor.FrameChanged += ListPropertyEditor_FrameChanged;
            }

            UygulaMasterKilidi();
        }

        protected override void OnDeactivated()
        {
            if (talepKalemleriEditor != null)
                talepKalemleriEditor.FrameChanged -= ListPropertyEditor_FrameChanged;
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
            bool kilitli = ViewCurrentObject.Durum != SatinAlmaTalepDurumu.Taslak;

            DeleteObjectsViewController masterDeleteController = Frame.GetController<DeleteObjectsViewController>();
            if (masterDeleteController != null)
                masterDeleteController.DeleteAction.Active[ActiveKey] = !kilitli;
        }

        void UygulaSatirKilidi()
        {
            if (ViewCurrentObject == null || talepKalemleriEditor?.Frame == null) return;
            bool kilitli = ViewCurrentObject.Durum != SatinAlmaTalepDurumu.Taslak;

            NewObjectViewController rowNewController = talepKalemleriEditor.Frame.GetController<NewObjectViewController>();
            if (rowNewController != null)
                rowNewController.NewObjectAction.Active[ActiveKey] = !kilitli;

            DeleteObjectsViewController rowDeleteController = talepKalemleriEditor.Frame.GetController<DeleteObjectsViewController>();
            if (rowDeleteController != null)
                rowDeleteController.DeleteAction.Active[ActiveKey] = !kilitli;
        }
    }
}
