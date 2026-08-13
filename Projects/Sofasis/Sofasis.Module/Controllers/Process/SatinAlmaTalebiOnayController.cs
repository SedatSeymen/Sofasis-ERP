using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;
using System.Linq;

namespace Sofasis.Module.Controllers
{
    // Onay iş mantığının tamamı ISatinAlmaOnayServisi'nde (ADR-006) — bu controller yalnızca
    // UI aksiyonlarını (görünürlük/aktiflik + kullanıcıdan Red Nedeni alma) yönetir.
    public class SatinAlmaTalebiOnayController : ObjectViewController<DetailView, SatinAlmaTalebiM>
    {
        const string ActiveKey = "SatinAlmaTalebiOnayController";
        const string OnayciRolAdi = "Satınalma Onaycısı";

        readonly ISatinAlmaOnayServisi onayServisi = new SatinAlmaOnayServisi();

        SimpleAction onayaGonderAction;
        SimpleAction onaylaAction;
        PopupWindowShowAction reddetAction;
        SimpleAction taslagaDondurAction;

        public SatinAlmaTalebiOnayController()
        {
            // Onay iş akışı aksiyonları kasıtlı olarak PredefinedCategory.View kategorisinde —
            // Sil'in bulunduğu RecordEdit/Edit grubuyla aynı toolbar bölümünde yan yana durup
            // yanlış tıklama riski yaratmasınlar diye ayrı bir gruba alındı (ReceteMaliyetViewController/
            // TopluReceteMaliyetViewController'daki özel iş-aksiyonu deseniyle aynı).
            onayaGonderAction = new SimpleAction(this, "SatinAlmaTalebiOnayaGonder", PredefinedCategory.View)
            {
                Caption = "Onaya Gönder",
                ImageName = "Action_Workflow_Activate"
            };
            onayaGonderAction.Execute += OnayaGonderAction_Execute;

            onaylaAction = new SimpleAction(this, "SatinAlmaTalebiOnayla", PredefinedCategory.View)
            {
                Caption = "Onayla",
                ImageName = "Action_Grant"
            };
            onaylaAction.Execute += OnaylaAction_Execute;

            reddetAction = new PopupWindowShowAction(this, "SatinAlmaTalebiReddet", PredefinedCategory.View)
            {
                Caption = "Reddet",
                ImageName = "Action_Deny"
            };
            reddetAction.CustomizePopupWindowParams += ReddetAction_CustomizePopupWindowParams;
            reddetAction.Execute += ReddetAction_Execute;

            // Reddedilen bir talep, düzeltilip yeniden gönderilebilmesi için Taslak durumuna
            // döndürülebilmeli — aksi halde reddedilme, talep eden için kalıcı bir çıkmaz olurdu.
            taslagaDondurAction = new SimpleAction(this, "SatinAlmaTalebiTaslagaDondur", PredefinedCategory.View)
            {
                Caption = "Taslağa Döndür",
                ImageName = "Action_Navigation_History_Back"
            };
            taslagaDondurAction.Execute += TaslagaDondurAction_Execute;
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

            bool onaycıMı = KullaniciOnaycıMı();

            onayaGonderAction.Active[ActiveKey] = ViewCurrentObject.Durum == SatinAlmaTalepDurumu.Taslak;
            onaylaAction.Active[ActiveKey] = onaycıMı && ViewCurrentObject.Durum == SatinAlmaTalepDurumu.OnayBekliyor;
            reddetAction.Active[ActiveKey] = onaycıMı && ViewCurrentObject.Durum == SatinAlmaTalepDurumu.OnayBekliyor;
            taslagaDondurAction.Active[ActiveKey] = ViewCurrentObject.Durum == SatinAlmaTalepDurumu.Reddedildi;
        }

        // "Satınalma Onaycısı" rolündeki ya da idari (Administrators) rolündeki kullanıcılar
        // onaylayabilir/reddedebilir — ikinci koşul olmadan varsayılan Admin hesabıyla canlı test
        // imkansız hale gelirdi (Admin'e özel rol otomatik atanmaz). SecuritySystem.CurrentUser
        // KULLANILMAZ (XAF0035 — Blazor'da statik erişim güvenli değil); BaseClassWithAudit'in
        // GetCurrentUser()'ıyla aynı CurrentUserId() kriter fonksiyonu deseni kullanılır.
        bool KullaniciOnaycıMı()
        {
            ApplicationUser mevcutKullanici = GuncelKullaniciyiGetir();
            if (mevcutKullanici == null) return false;
            return mevcutKullanici.Roles.Any(r => r.Name == OnayciRolAdi || r.IsAdministrative);
        }

        ApplicationUser GuncelKullaniciyiGetir()
        {
            return ObjectSpace.FindObject<ApplicationUser>(CriteriaOperator.Parse("Oid=CurrentUserId()"));
        }

        void OnayaGonderAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            onayServisi.Gonder(ViewCurrentObject);
            ObjectSpace.CommitChanges();
            View.Refresh();
        }

        void OnaylaAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            onayServisi.Onayla(ViewCurrentObject, GuncelKullaniciyiGetir());
            ObjectSpace.CommitChanges();
            View.Refresh();
        }

        void ReddetAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            IObjectSpace popupObjectSpace = Application.CreateObjectSpace(typeof(RedNedeniPopup));
            RedNedeniPopup popup = popupObjectSpace.CreateObject<RedNedeniPopup>();
            popupObjectSpace.CommitChanges();
            e.View = Application.CreateDetailView(popupObjectSpace, popup);
            e.DialogController.SaveOnAccept = false;
        }

        void ReddetAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            RedNedeniPopup popup = (RedNedeniPopup)e.PopupWindow.View.CurrentObject;
            onayServisi.Reddet(ViewCurrentObject, GuncelKullaniciyiGetir(), popup.Neden);
            ObjectSpace.CommitChanges();
            View.Refresh();
        }

        void TaslagaDondurAction_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            onayServisi.TaslagaDondur(ViewCurrentObject);
            ObjectSpace.CommitChanges();
            View.Refresh();
        }
    }
}
