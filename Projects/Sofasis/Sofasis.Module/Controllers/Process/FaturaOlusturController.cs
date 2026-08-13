using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;

namespace Sofasis.Module.Controllers
{
    // Mantığın tamamı IFaturaKaydetServisi'nde (ADR-006); bu controller yalnızca "Fatura Oluştur"
    // aksiyonunu ekler ve servisin döndürdüğü (KDV/Tevkifat hesaplı, HENÜZ KAYDEDİLMEMİŞ) taslağı
    // AYNI ObjectSpace'te (ayrı bir ObjectSpace/Session AÇILMAZ) yeni bir DetailView'da gösterir.
    // Kullanıcı KDV/Tevkifat seçimini/fiyatı gözden geçirip kendi "Kaydet"ine basar — commit AYNI
    // transaction'da gerçekleşir (atomik).
    // NOT (2026-08-14): Bu ekran ÖNCEDEN StokHareketleriM üzerinde çalışıyordu — kullanıcı geri
    // dönüp "irsaliye neden yok?" diye sorunca fark edildi ki İrsaliye hiç kurulmamıştı, StokHareketleriM
    // (Mal Kabul) onun yerine geçmişti. Ayrı bir IrsaliyeM iş nesnesi kurulunca (bkz.
    // docs/CHANGELOG.md 2026-08-14) "Fatura Oluştur" da doğru yerine, İrsaliye ekranına taşındı.
    public class FaturaOlusturController : ObjectViewController<DetailView, IrsaliyeM>
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
            // Yalnızca KAYITLI (View.ObjectSpace.IsNewObject == false) bir İrsaliyede aktif — henüz
            // kaydedilmemiş bir taslağın satırları için KaynakStokHareketiD bağlantısı anlamlı
            // değildir (satırlar henüz DB'de yok).
            faturaOlusturAction.Active[ActiveKey] =
                ViewCurrentObject.FisTuruTanim?.FisTuruKodu == "IRALIS" &&
                !ObjectSpace.IsNewObject(ViewCurrentObject);
        }

        // Çift-tıklama koruması: bkz. SatinAlmaIrsaliyeOlusturController — birebir aynı gerekçe
        // (canlı testte kanıtlanan mükerrer belge riski, bkz. docs/CHANGELOG.md 2026-08-13).
        // Servis katmanındaki (FaturaKaydetServisi) idempotency kontrolü asıl güvenlik ağıdır; bu
        // UI katmanı önlemidir.
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
