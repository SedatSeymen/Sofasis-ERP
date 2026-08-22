using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.DomainLogics;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using SofasisERP.Module.BusinessObjects;
using SofasisERP.Module.Reports;

namespace SofasisERP.Module;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.ModuleBase.
public sealed class SofasisERPModule : ModuleBase {
    public SofasisERPModule() {
        //
        // SofasisERPModule
        //
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.SystemModule.SystemModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ConditionalAppearance.ConditionalAppearanceModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.ValidationModule));
        RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Dashboards.DashboardsModule));
        RequiredModuleTypes.Add(typeof(ReportsModuleV2));
        // Kullanıcı bazlı Model Difference deposu (ör. ListView sütun genişlikleri) için gerekli —
        // eski ERP'den taşındı, bkz. SofasisERPBlazorModule.Application_CreateCustomUserModelDifferenceStore.
        AdditionalExportedTypes.Add(typeof(ModelDifference));
    }
    public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB) {
        ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
        // Kod-tabanlı (dosya) rapor tasarımları — her --updateDatabase'de DB'deki
        // ReportDataV2 kayıtlarıyla senkronize edilir (eski ERP'den taşınan desen,
        // bkz. D:\2025\SofasisERP\...\Sofasis.Module\Module.cs). "n sayıda rapor"
        // eklemek için buraya yeni bir AddPredefinedReport satırı yeterli — Kasa/Banka/Cari
        // Hesap Ekstresi TEK bir HesapEkstresiRaporu sınıfını paylaşıyor (unified Hesap
        // modelimiz sayesinde eski ERP'nin ayrı Cari/Kasa-Banka rapor sınıfı ikilisine
        // gerek yok), isInplaceReport=false — özel Controller'lar (Controllers/Process/
        // HesapEkstresiRaporuController*.cs) üzerinden PopupWindowShowAction ile tetiklenir.
        PredefinedReportsUpdater predefinedReportsUpdater = new PredefinedReportsUpdater(Application, objectSpace, versionFromDB);
        predefinedReportsUpdater.AddPredefinedReport<HesapEkstresiRaporu>("Cari Hesap Ekstresi", typeof(BusinessObjects.KasaCariBankaHareketleri), false);
        predefinedReportsUpdater.AddPredefinedReport<HesapEkstresiRaporu>("Kasa Ekstresi", typeof(BusinessObjects.KasaCariBankaHareketleri), false);
        predefinedReportsUpdater.AddPredefinedReport<HesapEkstresiRaporu>("Banka Ekstresi", typeof(BusinessObjects.KasaCariBankaHareketleri), false);
        // Tek fiş makbuzu (Açılış/Tahsilat/Ödeme/Virman) — isInplaceReport=true, DevExpress
        // ReportsV2'nin yerleşik "Rapora Göster" aksiyonu her List/DetailView'da otomatik
        // sunar (eski ERP'deki HareketMakbuzuReportBuilder deseninden uyarlandı).
        predefinedReportsUpdater.AddPredefinedReport<KasaCariBankaHareketMakbuzuReport>("Hareket Makbuzu", typeof(BusinessObjects.KasaCariBankaHareketleri), true);
        // Tüm Cariler Borç/Alacak özet raporu — canlı XPO CollectionDataSource yerine
        // Controller'ın önceden hesapladığı List<CariBakiyeSatiri>'ye bağlanır, bu yüzden
        // isInplaceReport=false + özel Controller (TumCarilerBakiyeRaporuController).
        predefinedReportsUpdater.AddPredefinedReport<TumCarilerBakiyeRaporu>("Tüm Cariler Borç/Alacak Raporu", typeof(BusinessObjects.CariHesapTanim), false);
        return new ModuleUpdater[] { updater, predefinedReportsUpdater };
    }
    public override void Setup(XafApplication application) {
        base.Setup(application);
    }
    public override void CustomizeTypesInfo(ITypesInfo typesInfo) {
        base.CustomizeTypesInfo(typesInfo);
        CalculatedPersistentAliasHelper.CustomizeTypesInfo(typesInfo);
    }
}
