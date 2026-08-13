using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Controllers
{
    // Bir StokHareketleriM (ve StokHareketleriDs satırları) KAYDEDİLDİKTEN SONRA immutable'dır —
    // muhasebe defteri kaydı gibi, hatalı bir girişi düzeltmenin yolu KAYDI DEĞİŞTİRMEK değil ters
    // bir hareket (ör. Sayım Fazlası/Eksiği) girmektir. Bu kısıtlama hem genel "Stok Hareketleri"
    // ekranında hem 8 fiş-türü-özel ekranda GEÇERLİDİR (tutarlılık) — yalnızca YENİ (henüz
    // kaydedilmemiş) bir fişte satır ekleme/kendi silme serbesttir.
    // NOT: Bu yalnızca UI Action'larını (kullanıcının "Sil" düğmesine basmasını) kapatır;
    // PROGRAMATİK silme (StokTransferi.ObjectDeleting içindeki Session.Delete(Master) çağrısı gibi)
    // Action.Active bayrağından tamamen bağımsızdır ve ETKİLENMEZ — transfer silme akışı bozulmaz.
    public class StokHareketleriMKayitliFisKilitleController : ObjectViewController<DetailView, StokHareketleriM>
    {
        const string ActiveKey = "StokHareketleriMKayitliFisKilitle";

        // Nested ListPropertyEditor'ün kendi Frame'i (embedded grid'in Yeni/Sil action'larını taşıyan)
        // OnActivated() anında henüz oluşmamış olabilir — DevExpress'in resmi "Access Nested List
        // View" dokümanının önerdiği gibi, Frame null ise ListPropertyEditor.FrameChanged event'ine
        // abone olup Frame hazır olduğunda kilidi tekrar uygulamak GEREKİYOR (canlı testte doğrulandı:
        // bu abonelik olmadan embedded grid'in "Yeni" butonu hiçbir zaman kapanmıyordu).
        ListPropertyEditor stokKalemleriEditor;

        protected override void OnActivated()
        {
            base.OnActivated();
            View.ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;

            if (View.FindItem(nameof(StokHareketleriM.StokHareketleriDs)) is ListPropertyEditor listEditor)
            {
                stokKalemleriEditor = listEditor;
                if (listEditor.Frame != null)
                    UygulaSatirKilidi();
                else
                    listEditor.FrameChanged += ListPropertyEditor_FrameChanged;
            }

            UygulaMasterKilidi();
        }

        protected override void OnDeactivated()
        {
            if (stokKalemleriEditor != null)
                stokKalemleriEditor.FrameChanged -= ListPropertyEditor_FrameChanged;
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
            if (ViewCurrentObject == null || stokKalemleriEditor?.Frame == null) return;
            bool kayitli = !ObjectSpace.IsNewObject(ViewCurrentObject);

            NewObjectViewController rowNewController = stokKalemleriEditor.Frame.GetController<NewObjectViewController>();
            if (rowNewController != null)
                rowNewController.NewObjectAction.Active[ActiveKey] = !kayitli;

            DeleteObjectsViewController rowDeleteController = stokKalemleriEditor.Frame.GetController<DeleteObjectsViewController>();
            if (rowDeleteController != null)
                rowDeleteController.DeleteAction.Active[ActiveKey] = !kayitli;
        }
    }
}
