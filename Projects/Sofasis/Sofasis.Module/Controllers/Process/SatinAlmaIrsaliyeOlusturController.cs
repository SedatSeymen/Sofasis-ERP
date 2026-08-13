using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;

namespace Sofasis.Module.Controllers
{
    // SatinAlmaMalKabulOlusturController'ın (2026-08-13, kullanıcı geri dönüp "irsaliye neden yok?"
    // diye sorana kadar İrsaliye'nin YERİNE geçmişti) yerini alır — bkz. docs/CHANGELOG.md
    // 2026-08-14. Mantığın tamamı ISatinAlmaIrsaliyeServisi'nde (ADR-006); bu controller yalnızca
    // "İrsaliye Oluştur" aksiyonunu ekler ve servisin döndürdüğü (HENÜZ KAYDEDİLMEMİŞ) taslağı AYNI
    // ObjectSpace'te (View.ObjectSpace — ayrı bir ObjectSpace/Session AÇILMAZ) yeni bir DetailView'da
    // gösterir.
    public class SatinAlmaIrsaliyeOlusturController : ObjectViewController<DetailView, SatinAlmaSiparisiM>
    {
        const string ActiveKey = "SatinAlmaIrsaliyeOlusturController";
        readonly ISatinAlmaIrsaliyeServisi irsaliyeServisi = new SatinAlmaIrsaliyeServisi();

        SimpleAction irsaliyeOlusturAction;

        public SatinAlmaIrsaliyeOlusturController()
        {
            irsaliyeOlusturAction = new SimpleAction(this, "SatinAlmaIrsaliyeOlustur", PredefinedCategory.View)
            {
                Caption = "İrsaliye Oluştur",
                ImageName = "Action_Workflow_Activate"
            };
            irsaliyeOlusturAction.Execute += IrsaliyeOlusturAction_Execute;
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
            irsaliyeOlusturAction.Active[ActiveKey] =
                ViewCurrentObject.Durum == SatinAlmaSiparisDurumu.Verildi ||
                ViewCurrentObject.Durum == SatinAlmaSiparisDurumu.KismiTeslimAlindi;
        }

        // Çift-tıklama koruması: bkz. docs/CHANGELOG.md 2026-08-13 ("çift-Kaydet-tıklama" bulgusu)
        // — art arda iki tetikleme, paylaşılan ObjectSpace'te sahipsiz ikinci bir taslak yaratıp
        // sonradan başka bir commit'e sürüklenebilirdi. Servis katmanındaki
        // (SatinAlmaIrsaliyeServisi.IrsaliyeTaslagiOlustur) idempotency kontrolü asıl güvenlik ağı,
        // bu UI katmanı önlemidir.
        bool islemSuruyor;

        void IrsaliyeOlusturAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (islemSuruyor) return;
            islemSuruyor = true;
            try
            {
                IrsaliyeM irsaliye = irsaliyeServisi.IrsaliyeTaslagiOlustur(View.ObjectSpace, ViewCurrentObject);

                // isRoot: false ZORUNLU — View.ObjectSpace zaten SatinAlmaSiparisiM_DetailView'ın kök
                // ObjectSpace'i. "IrsaliyeM_DetailView" EXPLICIT verilir — varsayılan (tip-bazlı)
                // çözümleme genel ekrana düşerdi; o ekranın gömülü grid'i kısıtlanmamış
                // IrsaliyeM_IrsaliyeDs_ListView'e sabit olmalı (kısmi teslimatta kullanıcının taslak
                // satır SİLEBİLMESİ gerektiğinden — bkz. Model.DesignedDiffs.xafml).
                DetailView dv = Application.CreateDetailView(View.ObjectSpace, "IrsaliyeM_DetailView", false, irsaliye);
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
