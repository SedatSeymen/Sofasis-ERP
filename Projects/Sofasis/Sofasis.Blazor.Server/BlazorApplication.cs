using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Xpo;
using Sofasis.Blazor.Server.Services;

namespace Sofasis.Blazor.Server
{
    public class SofasisBlazorApplication : BlazorApplication
    {
        public SofasisBlazorApplication()
        {
            ApplicationName = "Sofasis";
            CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
            DatabaseVersionMismatch += SofasisBlazorApplication_DatabaseVersionMismatch;
            CustomProcessShortcut += SofasisBlazorApplication_CustomProcessShortcut;
        }

        // Tarayıcı adres çubuğunda/sekmesinde silinmiş bir kayda ait eski bir DetailView linki
        // (ör. test verisi temizliği sonrası) kalmışsa, XAF açılışta bu shortcut'ı StartupNavigationItem'dan
        // ÖNCE işlemeye çalışır ve nesne bulunamayınca UserFriendlyException ile kullanıcıyı hata
        // ekranında bırakır — Gösterge Paneli'ne hiç ulaşılamaz. Nesne gerçekten yoksa sessizce
        // Gösterge Paneli'ne düş; nesne varsa (Handled=false) normal akışa karışma.
        void SofasisBlazorApplication_CustomProcessShortcut(object sender, DevExpress.ExpressApp.CustomProcessShortcutEventArgs e)
        {
            ViewShortcut shortcut = e.Shortcut;
            if (string.IsNullOrEmpty(shortcut.ObjectKey) || !Guid.TryParse(shortcut.ObjectKey, out Guid key))
                return;

            // URL route'undan (ViewID/ObjectKey) gelen shortcut'ta ObjectClassName genelde BOŞ olur —
            // gerçek CLR tipi shortcut.ObjectClass'tan değil, ViewId'nin Model node'undaki ModelClass'tan
            // çözümlenmeli (DevExpress dokümantasyonundaki resmi desen).
            Type objectType = shortcut.ObjectClass;
            if (objectType == null && !string.IsNullOrEmpty(shortcut.ViewId))
            {
                objectType = (FindModelView(shortcut.ViewId) as DevExpress.ExpressApp.Model.IModelDetailView)?.ModelClass?.TypeInfo?.Type;
            }
            if (objectType == null)
                return;

            IObjectSpace objectSpace = CreateObjectSpace(objectType);
            if (objectSpace.GetObjectByKey(objectType, key) == null)
            {
                // Bu objectSpace'in sahipliği CreateDashboardView'e devredilir — View kendi
                // Dispose'unda temizler, burada Dispose EDİLMEZ.
                e.View = CreateDashboardView(objectSpace, "AnaDashboard_DashboardView", true);
                e.Handled = true;
            }
            else
            {
                // 4-ajan turunda bulundu: nesne bulunduğunda (asıl yaygın senaryo — her normal
                // derin-link açılışı) bu objectSpace hiç kullanılmadan terk ediliyordu — sızıntı.
                objectSpace.Dispose();
            }
        }
        // ASP.NET Core Blazor uygulamaları varsayılan olarak "son giriş parametreleri" saklama
        // nesnesi oluşturmaz; bu nedenle LastLogonParametersRead hiç tetiklenmez (bkz. DevExpress
        // dokümantasyonu). Boş bir depolama sağlamak, okuma akışını devreye sokup Admin.Module'deki
        // LastLogonParametersRead işleyicisinin çalışmasını sağlar.
        class EmptyLogonParametersStorage : SettingsStorage
        {
            public override string LoadOption(string optionPath, string optionName) => null;
            public override void SaveOption(string optionPath, string optionName, string optionValue) { }
        }
        protected override SettingsStorage CreateLogonParameterStoreCore()
        {
            return new EmptyLogonParametersStorage();
        }
        protected override void OnSetupStarted()
        {
            base.OnSetupStarted();

#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached && CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
                DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
            }
#endif
        }
        void SofasisBlazorApplication_DatabaseVersionMismatch(object sender, DatabaseVersionMismatchEventArgs e)
        {
#if EASYTEST
            e.Updater.Update();
            e.Handled = true;
#else
            if (System.Diagnostics.Debugger.IsAttached)
            {
                e.Updater.Update();
                e.Handled = true;
            }
            else
            {
                string message = "The application cannot connect to the specified database, " +
                    "because the database doesn't exist, its version is older " +
                    "than that of the application or its schema does not match " +
                    "the ORM data model structure. To avoid this error, use one " +
                    "of the solutions from the https://www.devexpress.com/kb=T367835 KB Article.";

                if (e.CompatibilityError != null && e.CompatibilityError.Exception != null)
                {
                    message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
                }
                throw new InvalidOperationException(message);
            }
#endif
        }
    }
}
