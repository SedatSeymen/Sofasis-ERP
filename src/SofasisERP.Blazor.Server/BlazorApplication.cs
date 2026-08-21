using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using SofasisERP.Blazor.Server.Services;

namespace SofasisERP.Blazor.Server;

public class SofasisERPBlazorApplication : BlazorApplication {
    public SofasisERPBlazorApplication() {
        ApplicationName = "SofasisERP";
        CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
        DatabaseVersionMismatch += SofasisERPBlazorApplication_DatabaseVersionMismatch;

        // Giriş ekranında Kullanıcı Adı varsayılan "Admin" gelmesi artık burada DEĞİL,
        // Startup.cs'de options.LogonParametersType = typeof(SofasisERPLogonParameters)
        // ile çözülüyor — bkz. SofasisERPLogonParameters.cs (LastLogonParametersRead
        // event'i yeni Blazor /LoginPage akışında güvenilmez tetikleniyordu).
    }
    protected override void OnSetupStarted() {
        base.OnSetupStarted();

#if DEBUG
        if(System.Diagnostics.Debugger.IsAttached && CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
            DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
        }
#endif
    }
    void SofasisERPBlazorApplication_DatabaseVersionMismatch(object sender, DatabaseVersionMismatchEventArgs e) {
#if EASYTEST
        e.Updater.Update();
        e.Handled = true;
#else
        if(System.Diagnostics.Debugger.IsAttached) {
            e.Updater.Update();
            e.Handled = true;
        }
        else {
            string message = "The application cannot connect to the specified database, " +
                "because the database doesn't exist, its version is older " +
                "than that of the application or its schema does not match " +
                "the ORM data model structure. To avoid this error, use one " +
                "of the solutions from the https://www.devexpress.com/kb=T367835 KB Article.";

            if(e.CompatibilityError != null && e.CompatibilityError.Exception != null) {
                message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
            }
            throw new InvalidOperationException(message);
        }
#endif
    }
}
