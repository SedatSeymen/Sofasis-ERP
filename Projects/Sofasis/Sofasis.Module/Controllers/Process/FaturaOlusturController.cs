using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;

namespace Sofasis.Module.Controllers
{
    // SatinAlmaMalKabulOlusturController'ın birebir portu — Mal Kabul mantığının tamamı
    // IFaturaKaydetServisi'nde (ADR-006); bu controller yalnızca "Fatura Oluştur" aksiyonunu ekler
    // ve servisin döndürdüğü (KDV/Tevkifat hesaplı, HENÜZ KAYDEDİLMEMİŞ) taslağı AYNI ObjectSpace'te
    // (ayrı bir ObjectSpace/Session AÇILMAZ) yeni bir DetailView'da gösterir. Kullanıcı KDV/Tevkifat
    // seçimini/fiyatı gözden geçirip kendi "Kaydet"ine basar — commit AYNI transaction'da gerçekleşir
    // (atomik).
    public class FaturaOlusturController : ObjectViewController<DetailView, StokHareketleriM>
    {
        const string ActiveKey = "FaturaOlusturController";
        readonly IFaturaKaydetServisi faturaServisi = new FaturaKaydetServisi();

        SimpleAction faturaOlusturAction;

        public FaturaOlusturController()
        {
            faturaOlusturAction = new SimpleAction(this, "FaturaOlustur", PredefinedCategory.View)
            {
                Caption = "Fatura Oluştur",
                ImageName = "Action_Workflow_Activate"
            };
            faturaOlusturAction.Execute += FaturaOlusturAction_Execute;
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
            if (ViewCurrentObject == null) { faturaOlusturAction.Active[ActiveKey] = false; return; }
            // Yalnızca KAYITLI (View.ObjectSpace.IsNewObject == false) bir STSAGR fişinde aktif —
            // henüz kaydedilmemiş bir Mal Kabul taslağının satırları için KaynakStokHareketiD
            // bağlantısı anlamlı değildir (satırlar henüz DB'de yok).
            faturaOlusturAction.Active[ActiveKey] =
                ViewCurrentObject.FisTuruTanim?.FisTuruKodu == "STSAGR" &&
                !ObjectSpace.IsNewObject(ViewCurrentObject);
        }

        // Çift-tıklama koruması: bkz. SatinAlmaMalKabulOlusturController.MalKabulOlusturAction_Execute
        // — birebir aynı gerekçe (canlı testte kanıtlanan mükerrer belge riski, bkz.
        // docs/CHANGELOG.md 2026-08-13). Servis katmanındaki (FaturaKaydetServisi) idempotency
        // kontrolü asıl güvenlik ağıdır; bu UI katmanı önlemidir.
        bool islemSuruyor;

        void FaturaOlusturAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (islemSuruyor) return;
            islemSuruyor = true;
            try
            {
                FaturaM fatura = faturaServisi.FaturaTaslagiOlustur(View.ObjectSpace, ViewCurrentObject);

                DetailView dv = Application.CreateDetailView(View.ObjectSpace, "FaturaM_DetailView", false, fatura);
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
