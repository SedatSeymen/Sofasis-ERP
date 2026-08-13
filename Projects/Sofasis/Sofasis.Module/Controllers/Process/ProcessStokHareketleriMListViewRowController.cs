using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Controllers
{
    // Genel "Stok Hareketleri" listesi (StokHareketleriM_ListView) tüm fiş türlerini karışık
    // gösterir (AllowNew=False — oluşturma yalnızca 8 özel ekrandan yapılır); çift tıklandığında
    // kaydın hangi özel ekrandan oluşturulduğu FisTuruTanim.ViewName üzerinden bulunup o ekran
    // açılır. Kasa/Cari'nin aynı-amaçlı controller'larından farklı olarak burada çapraz-modül
    // ayna kaydı (IntegrationSourceEntity) yok — stok hareketleri başka bir modüle yansıtılmaz.
    public partial class ProcessStokHareketleriMListViewRowController : ViewController
    {
        ListViewProcessCurrentObjectController stokListProcessController;

        public ProcessStokHareketleriMListViewRowController()
        {
            InitializeComponent();
            TargetViewId = "StokHareketleriM_ListView";
        }

        protected override void OnActivated()
        {
            base.OnActivated();

            stokListProcessController = Frame.GetController<ListViewProcessCurrentObjectController>();
            if (stokListProcessController != null)
            {
                stokListProcessController.CustomProcessSelectedItem += StokListProcessController_CustomProcessSelectedItem;
            }
        }

        void StokListProcessController_CustomProcessSelectedItem(object sender, CustomProcessListViewSelectedItemEventArgs e)
        {
            StokHareketleriM stokObject = (StokHareketleriM)e.InnerArgs.CurrentObject;

            string selectedViewName = stokObject.FisTuruTanim?.ViewName?.Replace("List", "Detail") ?? string.Empty;
            if (string.IsNullOrEmpty(selectedViewName))
                return;

            IObjectSpace objectSpace = Application.CreateObjectSpace(stokObject.GetType());
            e.InnerArgs.ShowViewParameters.CreatedView = Application.CreateDetailView(objectSpace, selectedViewName, true, objectSpace.GetObject(stokObject));
            e.Handled = true;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Frame.GetController<ListViewProcessCurrentObjectController>().CustomProcessSelectedItem -= StokListProcessController_CustomProcessSelectedItem;
            stokListProcessController = null;
        }
    }
}
