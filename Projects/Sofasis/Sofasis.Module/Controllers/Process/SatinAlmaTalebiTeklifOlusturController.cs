using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;

namespace Sofasis.Module.Controllers
{
    // Doğru satın alma akışı: Talep → Teklif (RFQ/teklif toplama) → Sipariş. Talep'ten DOĞRUDAN
    // Sipariş oluşturan bir kısayol KASITLI OLARAK yok — bu, rekabetçi fiyat toplamanın (Teklif
    // Toplama/Karşılaştırma) tüm amacını atlardı. Onaylı (veya zaten teklife çıkılmış — ikinci/
    // üçüncü rakip teklif eklenirken) bir Talep, bu buton ile bir Teklif TASLAĞI olarak açılır;
    // servis (ISatinAlmaTeklifServisi.TalepdenTeklifTaslagiOlustur) KAYDETMEZ — kullanıcı
    // Tedarikçiyi seçip fiyatları girdikten sonra kendi "Kaydet"iyle onaylar.
    public class SatinAlmaTalebiTeklifOlusturController : ObjectViewController<DetailView, SatinAlmaTalebiM>
    {
        const string ActiveKey = "SatinAlmaTalebiTeklifOlusturController";

        readonly ISatinAlmaTeklifServisi teklifServisi = new SatinAlmaTeklifServisi();

        SimpleAction teklifOlusturAction;

        public SatinAlmaTalebiTeklifOlusturController()
        {
            teklifOlusturAction = new SimpleAction(this, "SatinAlmaTalebiTeklifOlustur", PredefinedCategory.View)
            {
                Caption = "Teklif Oluştur",
                ImageName = "Action_Workflow_Activate"
            };
            teklifOlusturAction.Execute += TeklifOlusturAction_Execute;
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
            teklifOlusturAction.Active[ActiveKey] =
                ViewCurrentObject.Durum == SatinAlmaTalepDurumu.Onaylandi ||
                ViewCurrentObject.Durum == SatinAlmaTalepDurumu.TeklifeCikildi;
        }

        void TeklifOlusturAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            IObjectSpace yeniObjectSpace = Application.CreateObjectSpace(typeof(SatinAlmaTeklifM));
            SatinAlmaTalebiM talepYeniUzayda = yeniObjectSpace.GetObject(ViewCurrentObject);

            SatinAlmaTeklifM teklifTaslagi = teklifServisi.TalepdenTeklifTaslagiOlustur(talepYeniUzayda);

            DetailView teklifView = Application.CreateDetailView(yeniObjectSpace, teklifTaslagi);
            teklifView.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            e.ShowViewParameters.CreatedView = teklifView;
            e.ShowViewParameters.TargetWindow = TargetWindow.NewWindow;
        }
    }
}
