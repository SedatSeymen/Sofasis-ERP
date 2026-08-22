using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.DomainLogics;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.Persistent.BaseImpl;

namespace SofasisERP.Blazor.Server;

[ToolboxItemFilter("Xaf.Platform.Blazor")]
public sealed class SofasisERPBlazorModule : ModuleBase {
    public SofasisERPBlazorModule() {
    }
    public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB) {
        return ModuleUpdater.EmptyModuleUpdaters;
    }
    // Kullanıcı bazlı Model Difference'ları (ör. ListView sütun genişlikleri, sütun sırası) DB'ye
    // kalıcı olarak yazar — eski ERP'den aynen taşındı (bkz. Sofasis.Blazor.Server\BlazorModule.cs).
    // shared:false = her kullanıcı kendi sütun genişliğini görür, ortak Model.xafml'i etkilemez.
    void Application_CreateCustomUserModelDifferenceStore(object sender, CreateCustomModelDifferenceStoreEventArgs e) {
        e.Store = new ModelDifferenceDbStore((XafApplication)sender, typeof(ModelDifference), false, "Blazor");
        e.Handled = true;
    }
    public override void Setup(XafApplication application) {
        base.Setup(application);
        application.CreateCustomUserModelDifferenceStore += Application_CreateCustomUserModelDifferenceStore;
    }
}
