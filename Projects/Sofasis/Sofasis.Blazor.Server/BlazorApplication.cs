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
