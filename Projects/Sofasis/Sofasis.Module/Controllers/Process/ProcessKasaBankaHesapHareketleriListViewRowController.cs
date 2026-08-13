using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using Sofasis.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sofasis.Module.Controllers
{
    public partial class ProcessKasaBankaHesapHareketleriListViewRowController : ViewController
    {
        ListViewProcessCurrentObjectController KasaBankalistProcessController;
        public ProcessKasaBankaHesapHareketleriListViewRowController()
        {
            InitializeComponent();
            TargetViewId = "KasaBankaHareketleri_ListView";
        }
        protected override void OnActivated()
        {
            base.OnActivated();

            KasaBankalistProcessController = Frame.GetController<ListViewProcessCurrentObjectController>();
            if (KasaBankalistProcessController != null)
            {
                KasaBankalistProcessController.CustomProcessSelectedItem += ProcessCariHesapHareketleriListViewRowController_CustomProcessSelectedItem;

            }


        }
        void ProcessCariHesapHareketleriListViewRowController_CustomProcessSelectedItem(object sender, CustomProcessListViewSelectedItemEventArgs e)
        {
            KasaBankaHareketleri KasaObject = (KasaBankaHareketleri)e.InnerArgs.CurrentObject;

            IObjectSpace objectSpace = Application.CreateObjectSpace(KasaObject.GetType());

            // Kayıt bir Cari'den gelen AYNA kayıtsa (ör. "Cari Hesap Nakit Tahsilat" girişinde
            // otomatik oluşan Kasa Tahsilat aynası), kendi Kasa/Banka ekranı yerine asıl kaynağı
            // olan Cari ekranı açılır — kullanıcı canlı testte "kendi ekranı yerine kaynağa
            // yönlendirilsin" seçeneğini onayladı (aksi halde tahsilat/ödeme, oluşturulduğu
            // Cari formundan değil, kafa karıştırıcı şekilde Kasa formundan görünüyordu).
            if (KasaObject.IntegrationSourceEntity == typeof(CariHesapHareketleri))
            {
                CariHesapHareketleri kaynakCariHareket = objectSpace.FindObject<CariHesapHareketleri>(
                    CriteriaOperator.Parse(nameof(CariHesapHareketleri.IntegrationCode) + " = ?", KasaObject.IntegrationCode));
                string cariViewName = kaynakCariHareket?.FisTuruTanim?.ViewName?.Replace("List", "Detail") ?? string.Empty;
                if (kaynakCariHareket != null && !string.IsNullOrEmpty(cariViewName))
                {
                    e.InnerArgs.ShowViewParameters.CreatedView = Application.CreateDetailView(objectSpace, cariViewName, true, kaynakCariHareket);
                    e.Handled = true;
                    return;
                }
            }

            // Genel "Kasa Banka Hareketleri" listesi Kasa VE Banka fiş türlerinin tümünü karışık
            // gösterir; çift tıklandığında kaydın hangi özel ekrandan (ör. Banka Gelen Havale)
            // oluşturulduğu FisTuruTanim.ViewName üzerinden bulunup o ekran açılır — aksi halde
            // hep genel/tipsiz KasaBankaHareketleri_DetailView açılırdı (canlı testte fark edildi:
            // Banka fiş türleri hiç eşleşmiyordu, yalnızca "KS" öneki kontrol ediliyordu).
            string kod = KasaObject.FisTuruTanim.FisTuruKodu;
            string selectedViewName = KasaObject.FisTuruTanim.ViewName?.Replace("List", "Detail") ?? string.Empty;
            if (!string.IsNullOrEmpty(kod) && !string.IsNullOrEmpty(selectedViewName) &&
                (kod.StartsWith("KS") || kod.StartsWith("BN")))
            {
                e.InnerArgs.ShowViewParameters.CreatedView = Application.CreateDetailView(objectSpace, selectedViewName, true, objectSpace.GetObject(KasaObject));
                e.Handled = true;
            }
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();

        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Frame.GetController<ListViewProcessCurrentObjectController>().CustomProcessSelectedItem -= ProcessCariHesapHareketleriListViewRowController_CustomProcessSelectedItem;
            KasaBankalistProcessController = null;
        }
    }
}
