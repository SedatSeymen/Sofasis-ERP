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
    // Kararlar. Mantığın tamamı IFaturaKaydetServisi'nde (ADR-006); bu controller yalnızca "İade
    // Faturası Oluştur" aksiyonunu ekler ve servisin döndürdüğü (HENÜZ KAYDEDİLMEMİŞ, KDV/Tevkifat
    // hesaplı) taslağı AYNI ObjectSpace'te gösterir. FaturaOlusturController'ın aksine bir
    // Committing-abonelik gerekmiyor: İade Faturasının Cari-ayna etkisi FaturaM.ObjectSaving()
    // içindeki CariAynaKaydiOlustur'da SENKRON gerçekleşir (FisTuruTanim.FinansBorcAlacakTipi
    // FAALID için orijinal FAALIS'in TERSİ olduğundan otomatik ters yönde kayıt üretir — 4-ajan
    // turunda Cari Borç/Alacak yönü Netsis/Logo/Mikro standardına çevrildi, bkz. fis-turleri.csv)
    // — TeslimatiIsle gibi ayrı bir post-commit adım yok.
    public class FaturaIadeOlusturController : ObjectViewController<DetailView, FaturaM>
    {
        const string ActiveKey = "FaturaIadeOlusturController";
        readonly IFaturaKaydetServisi faturaServisi = new FaturaKaydetServisi();

        SimpleAction iadeOlusturAction;

        public FaturaIadeOlusturController()
        {
            iadeOlusturAction = new SimpleAction(this, "FaturaIadeOlustur", PredefinedCategory.View)
            {
                Caption = "İade Faturası Oluştur",
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

            // Yalnızca KAYITLI, normal (İade olmayan) ve HENÜZ iade edilmemiş bir Faturada aktif.
            bool uygunTur = ViewCurrentObject.FisTuruTanim?.FisTuruKodu == "FAALIS" &&
                !ObjectSpace.IsNewObject(ViewCurrentObject);
            bool zatenIadeEdilmis = uygunTur && ObjectSpace.FindObject<FaturaM>(
                new BinaryOperator(nameof(FaturaM.KaynakFatura), ViewCurrentObject)) != null;

            iadeOlusturAction.Active[ActiveKey] = uygunTur && !zatenIadeEdilmis;
        }

        // Çift-tıklama koruması: bkz. SatinAlmaIrsaliyeIadeOlusturController — birebir aynı gerekçe.
        bool islemSuruyor;

        void IadeOlusturAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (islemSuruyor) return;
            islemSuruyor = true;
            try
            {
                FaturaM iade = faturaServisi.IadeTaslagiOlustur(View.ObjectSpace, ViewCurrentObject);

                DetailView dv = Application.CreateDetailView(View.ObjectSpace, "FaturaM_DetailView", false, iade);
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
