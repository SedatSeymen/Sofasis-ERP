using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;

namespace Sofasis.Module.Controllers
{
    // İrsaliye/Fatura için düzeltme/iade akışı — bkz. docs/01_Mimari-ve-Kararlar.md ADR-017 Açık
    // Kararlar (Endüstri Mühendisi 3-ajan turunda KRİTİK işaretledi: kaydedilip kilitlenen bir
    // İrsaliye'yi düzeltmenin tek yolu şu ana kadar veritabanına elle müdahaleydi). Mantığın tamamı
    // ISatinAlmaIrsaliyeServisi'nde (ADR-006); bu controller yalnızca "İade Oluştur" aksiyonunu ekler
    // ve servisin döndürdüğü (HENÜZ KAYDEDİLMEMİŞ) İade taslağını AYNI ObjectSpace'te gösterir.
    public class SatinAlmaIrsaliyeIadeOlusturController : ObjectViewController<DetailView, IrsaliyeM>
    {
        const string ActiveKey = "SatinAlmaIrsaliyeIadeOlusturController";
        readonly ISatinAlmaIrsaliyeServisi irsaliyeServisi = new SatinAlmaIrsaliyeServisi();

        SimpleAction iadeOlusturAction;

        public SatinAlmaIrsaliyeIadeOlusturController()
        {
            iadeOlusturAction = new SimpleAction(this, "SatinAlmaIrsaliyeIadeOlustur", PredefinedCategory.View)
            {
                Caption = "İade Oluştur",
                ImageName = "Action_Workflow_Discard"
            };
            iadeOlusturAction.Execute += IadeOlusturAction_Execute;
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
            if (ViewCurrentObject == null) { iadeOlusturAction.Active[ActiveKey] = false; return; }

            // Yalnızca KAYITLI, normal (İade olmayan) ve HENÜZ iade edilmemiş bir İrsaliyede aktif.
            bool uygunTur = ViewCurrentObject.FisTuruTanim?.FisTuruKodu == "IRALIS" &&
                !ObjectSpace.IsNewObject(ViewCurrentObject);
            bool zatenIadeEdilmis = uygunTur && ObjectSpace.FindObject<IrsaliyeM>(
                new BinaryOperator(nameof(IrsaliyeM.KaynakIrsaliye), ViewCurrentObject)) != null;

            iadeOlusturAction.Active[ActiveKey] = uygunTur && !zatenIadeEdilmis;
        }

        // Çift-tıklama koruması: bkz. SatinAlmaIrsaliyeOlusturController — birebir aynı gerekçe.
        bool islemSuruyor;

        void IadeOlusturAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (islemSuruyor) return;
            islemSuruyor = true;
            try
            {
                IrsaliyeM iade = irsaliyeServisi.IadeTaslagiOlustur(View.ObjectSpace, ViewCurrentObject);

                DetailView dv = Application.CreateDetailView(View.ObjectSpace, "IrsaliyeM_DetailView", false, iade);
                dv.ViewEditMode = ViewEditMode.Edit;
                e.ShowViewParameters.CreatedView = dv;
                e.ShowViewParameters.TargetWindow = TargetWindow.NewModalWindow;
            }
            finally
            {
                islemSuruyor = false;
            }
        }
    }
}
