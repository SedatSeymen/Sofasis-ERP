using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;

namespace Sofasis.Module.Controllers
{
    // Mal Kabul mantığının tamamı ISatinAlmaMalKabulServisi'nde (ADR-006) — bu controller yalnızca
    // "Mal Kabul Oluştur" aksiyonunu ekler ve servisin döndürdüğü (HENÜZ KAYDEDİLMEMİŞ) taslağı
    // AYNI ObjectSpace'te (View.ObjectSpace — ayrı bir ObjectSpace/Session AÇILMAZ) yeni bir
    // DetailView'da gösterir. Kullanıcı orada Depo seçer, dilerse satırların Miktar'ını azaltır
    // veya satır siler (kısmi teslimat) ve kendi "Kaydet"ine basar — bu commit, Sipariş'in
    // TeslimEdilenMiktar güncellemesiyle (bkz. SatinAlmaMalKabulKaydetController) AYNI transaction'da
    // gerçekleşir (atomik: ikisi de kaydedilir ya da hiçbiri kaydedilmez).
    public class SatinAlmaMalKabulOlusturController : ObjectViewController<DetailView, SatinAlmaSiparisiM>
    {
        const string ActiveKey = "SatinAlmaMalKabulOlusturController";
        readonly ISatinAlmaMalKabulServisi malKabulServisi = new SatinAlmaMalKabulServisi();

        SimpleAction malKabulOlusturAction;

        public SatinAlmaMalKabulOlusturController()
        {
            malKabulOlusturAction = new SimpleAction(this, "SatinAlmaMalKabulOlustur", PredefinedCategory.View)
            {
                Caption = "Mal Kabul Oluştur",
                ImageName = "Action_Workflow_Activate"
            };
            malKabulOlusturAction.Execute += MalKabulOlusturAction_Execute;
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
            malKabulOlusturAction.Active[ActiveKey] =
                ViewCurrentObject.Durum == SatinAlmaSiparisDurumu.Verildi ||
                ViewCurrentObject.Durum == SatinAlmaSiparisDurumu.KismiTeslimAlindi;
        }

        // Çift-tıklama koruması: "Mal Kabul Oluştur"a art arda iki kez basılması, PAYLAŞILAN
        // ObjectSpace'te (isRoot:false) sahipsiz ikinci bir StokHareketleriM taslağı yaratıp
        // sonradan başka bir commit'e sessizce sürüklenmesine (mükerrer belge) yol açıyordu —
        // canlı testte kanıtlandı, kök neden analizi için bkz. docs/CHANGELOG.md 2026-08-13.
        // Servis katmanındaki (SatinAlmaMalKabulServisi.MalKabulTaslagiOlustur) idempotency
        // kontrolü asıl güvenlik ağıdır; bu, kullanıcıya o hatayı hiç göstermeden ikinci tıklamayı
        // en baştan engelleyen UI katmanı önlemidir.
        bool islemSuruyor;

        void MalKabulOlusturAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (islemSuruyor) return;
            islemSuruyor = true;
            try
            {
                StokHareketleriM malKabulFisi = malKabulServisi.MalKabulTaslagiOlustur(View.ObjectSpace, ViewCurrentObject);

                // isRoot: false ZORUNLU — View.ObjectSpace zaten SatinAlmaSiparisiM_DetailView'ın kök
                // ObjectSpace'i; isRoot:true (2 parametreli overload) "ObjectSpace zaten başka bir kök
                // görünüme atanmış" hatasıyla çöker (DX docs: "Pass false if this Object Space already
                // belongs to another View").
                // "SatinAlmaMalKabul_DetailView" EXPLICIT verilir — varsayılan (tip-bazlı) çözümleme
                // genel StokHareketleriM_DetailView'e düşerdi; o ekranın gömülü grid'i
                // StokHareketleriMGenelKalemler_ListView'e sabit (AllowDelete="False") — kısmi
                // teslimatta kullanıcının taslak satır SİLEBİLMESİ gerektiğinden bu ekran kullanılamaz
                // (bkz. Model.DesignedDiffs.xafml, SatinAlmaMalKabul_DetailView üstündeki yorum).
                DetailView dv = Application.CreateDetailView(View.ObjectSpace, "SatinAlmaMalKabul_DetailView", false, malKabulFisi);
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
