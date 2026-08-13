using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Controllers
{
    public partial class ProcessCariHesapHareketleriListViewRowController : ViewController
    {
        ListViewProcessCurrentObjectController CariHesapListProcessController;
        public ProcessCariHesapHareketleriListViewRowController()
        {
            InitializeComponent();
            TargetViewId = "CariHesapHareketleri_ListView";
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            CariHesapListProcessController = Frame.GetController<ListViewProcessCurrentObjectController>();
            if (CariHesapListProcessController != null)
            {
                CariHesapListProcessController.CustomProcessSelectedItem += ProcessCariHesapHareketleriListViewRowController_CustomProcessSelectedItem;
            }
        }
        void ProcessCariHesapHareketleriListViewRowController_CustomProcessSelectedItem(object sender, CustomProcessListViewSelectedItemEventArgs e)
        {
            CariHesapHareketleri CariObject = (CariHesapHareketleri)e.InnerArgs.CurrentObject;

            IObjectSpace objectSpace = Application.CreateObjectSpace(CariObject.GetType());

            // Kayıt bir Kasa/Banka'dan gelen AYNA kayıtsa (ör. "Kasa Tahsilat Fişi" girişinde
            // otomatik oluşan Cari Nakit Tahsilat aynası), kendi Cari ekranı yerine asıl kaynağı
            // olan Kasa/Banka ekranı açılır — ProcessKasaBankaHesapHareketleriListViewRowController'daki
            // simetrik davranışın aynası (kullanıcı her iki yönde de "kaynağa git" kuralını onayladı;
            // gerçek muhasebe/ERP programlarında muavin/ekstre ekranları salt-okunurdur, çift tık
            // kaydı doğuran belgeye götürür).
            if (CariObject.IntegrationSourceEntity == typeof(KasaBankaHareketleri))
            {
                KasaBankaHareketleri kaynakKasaHareket = objectSpace.FindObject<KasaBankaHareketleri>(
                    CriteriaOperator.Parse(nameof(KasaBankaHareketleri.IntegrationCode) + " = ?", CariObject.IntegrationCode));
                string kasaViewName = kaynakKasaHareket?.FisTuruTanim?.ViewName?.Replace("List", "Detail") ?? string.Empty;
                if (kaynakKasaHareket != null && !string.IsNullOrEmpty(kasaViewName))
                {
                    e.InnerArgs.ShowViewParameters.CreatedView = Application.CreateDetailView(objectSpace, kasaViewName, true, kaynakKasaHareket);
                    e.Handled = true;
                    return;
                }
            }

            // Native Cari kaydı: hangi özel ekrandan (ör. Cari Nakit Tahsilat) oluşturulduğu
            // FisTuruTanim.ViewName üzerinden bulunup o ekran açılır.
            string kod = CariObject.FisTuruTanim.FisTuruKodu;
            string selectedViewName = CariObject.FisTuruTanim.ViewName?.Replace("List", "Detail") ?? string.Empty;
            if (!string.IsNullOrEmpty(kod) && !string.IsNullOrEmpty(selectedViewName) && kod.StartsWith("CA"))
            {
                e.InnerArgs.ShowViewParameters.CreatedView = Application.CreateDetailView(objectSpace, selectedViewName, true, objectSpace.GetObject(CariObject));
                e.Handled = true;
            }
        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Frame.GetController<ListViewProcessCurrentObjectController>().CustomProcessSelectedItem -= ProcessCariHesapHareketleriListViewRowController_CustomProcessSelectedItem;
            CariHesapListProcessController = null;
        }
    }
}
