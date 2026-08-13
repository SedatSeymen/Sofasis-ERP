using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.DomainLogics;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Reports;
using System.ComponentModel;

namespace Sofasis.Module
{
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.ModuleBase.
    public sealed class SofasisModule : ModuleBase
    {
        public SofasisModule()
        {
            //
            // SofasisModule
            //
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.ModelDifference));
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.ModelDifferenceAspect));
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.BaseObject));
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.AuditDataItemPersistent));
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.AuditedObjectWeakReference));
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.FileData));
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.FileAttachmentBase));
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.Event));
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.Resource));
            AdditionalExportedTypes.Add(typeof(DevExpress.Persistent.BaseImpl.HCategory));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.SystemModule.SystemModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Security.SecurityModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.AuditTrail.AuditTrailModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.CloneObject.CloneObjectModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ConditionalAppearance.ConditionalAppearanceModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Dashboards.DashboardsModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Notifications.NotificationsModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Office.OfficeModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ReportsV2.ReportsModuleV2));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Scheduler.SchedulerModuleBase));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.StateMachine.StateMachineModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.ValidationModule));
            RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.ViewVariantsModule.ViewVariantsModule));
            // Taşınan domain: dosya ekleri için FileSystemData modülü (FileSystemStoreObject/FileSystemLinkObject)
            RequiredModuleTypes.Add(typeof(FileSystemData.FileSystemDataModule));
        }
        public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB)
        {
            ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
            // Dosya bazlı (kod içi) rapor tasarımları: DB silinip yeniden kurulsa bile
            // her --updateDatabase çalışmasında bu .cs dosyalarından yeniden senkronize edilir.
            PredefinedReportsUpdater predefinedReportsUpdater = new PredefinedReportsUpdater(Application, objectSpace, versionFromDB);
            predefinedReportsUpdater.AddPredefinedReport<KasaBankaHareketMakbuzuReport>("Hareket Makbuzu", typeof(KasaBankaHareketleri), true);
            predefinedReportsUpdater.AddPredefinedReport<CariHesapHareketMakbuzuReport>("Hareket Makbuzu", typeof(CariHesapHareketleri), true);
            return new ModuleUpdater[] { updater, predefinedReportsUpdater };
        }
        public override void Setup(XafApplication application)
        {
            base.Setup(application);
            // Manage various aspects of the application UI and behavior at the module level.

            // Giriş ekranında Kullanıcı Adı alanı varsayılan olarak "Admin" gelsin (geliştirme kolaylığı).
            application.LastLogonParametersRead += Application_LastLogonParametersRead;

            // GenelParametre.MiktarOndalikMaski/TutarOndalikMaski'yi uygulama genelindeki tüm
            // "...Miktar"/"...Tutar" adlı decimal alanlara uygular (bkz. docs/CHANGELOG.md).
            application.SetupComplete += Application_SetupComplete;
        }

        static void Application_LastLogonParametersRead(object sender, LastLogonParametersReadEventArgs e)
        {
            if (e.LogonObject is AuthenticationStandardLogonParameters logonParameters && string.IsNullOrEmpty(logonParameters.UserName))
            {
                logonParameters.UserName = "Admin";
            }
        }

        // ⚠ KRİTİK: Application Model'in bir kısmı (ModelNodeSharedValuesCache) SÜREÇ GENELİNDE
        // PAYLAŞILIR — WinForms'un aksine Blazor Server'da her kullanıcı devresi (circuit) kendi
        // XafApplication örneğini kurar ve SetupComplete HER devrede yeniden tetiklenir. Canlı testte
        // doğrulandı: iki devre (ör. iki sekme/hızlı ardışık istek) SetupComplete'i eşzamanlı
        // tetiklerse ve HER ikisi de BOModel üyelerini mutasyona uğratmaya çalışırsa,
        // "A concurrent update was performed on this collection and corrupted its state" ile
        // uygulama çöküyordu. Düzeltme: mutasyon işlemi `lock` ile korunan bir statik bayrakla
        // SÜREÇ ÖMRÜ BOYUNCA YALNIZCA BİR KEZ çalıştırılır — bu, "GenelParametre değişikliği yeni
        // oturumda otomatik yansır" iddiasını GEÇERSİZ KILAR: artık yalnızca SUNUCU YENİDEN
        // BAŞLATILDIĞINDA etkinleşir (StokParametre/SatinAlmaParametre gibi diğer parametrelerle
        // aynı, zaten bilinen sınırlama).
        static readonly object ondalikMaskiKilit = new();
        static bool ondalikMaskiUygulandi;
        static void Application_SetupComplete(object sender, EventArgs e)
        {
            if (ondalikMaskiUygulandi) return;
            lock (ondalikMaskiKilit)
            {
                if (ondalikMaskiUygulandi) return;
                XafApplication application = (XafApplication)sender;
                OndalikMaskiUygula(application);
                ondalikMaskiUygulandi = true;
            }
        }

        static void OndalikMaskiUygula(XafApplication application)
        {
            string miktarFormat = "{0:N4}", miktarMask = "N4";
            // Tutar burada SEMBOLSÜZ kalır — Tutar alanları farklı para birimlerinde olabilir
            // (DovizTanim seçimine göre), sabit ₺ yanlış olurdu. DetailView'larda gerçek format
            // (dinamik sembol + GÜNCEL hane sayısı), OnActivated'da taze çalışan
            // OndalikFormatController (bkz. Controllers/Process/OndalikFormatController.cs)
            // tarafından üzerine yazılır — bu geçiş yalnızca ListView sütunları gibi
            // OndalikFormatController'ın kapsamadığı yerler için process-ömrü-boyunca-bir-kez'lik
            // bir taban değer sağlar.
            string tutarFormat = "{0:N2}", tutarMask = "N2";
            try
            {
                using IObjectSpace os = application.CreateObjectSpace(typeof(GenelParametre));
                GenelParametre parametre = os.GetObjects<GenelParametre>().FirstOrDefault();
                if (parametre != null)
                {
                    int miktarBasamak = (int)parametre.MiktarOndalikMaski;
                    int tutarBasamak = (int)parametre.TutarOndalikMaski;
                    miktarFormat = $"{{0:N{miktarBasamak}}}";
                    miktarMask = $"N{miktarBasamak}";
                    tutarFormat = $"{{0:N{tutarBasamak}}}";
                    tutarMask = $"N{tutarBasamak}";
                }
            }
            catch (Exception ex)
            {
                // İlk kurulum sırasında (tablo/şema henüz oluşmadan) SetupComplete tetiklenirse
                // varsayılan maskeyle (Miktar N4 / Tutar N2) devam edilir — uygulama başlangıcını
                // asla engellemez.
                Tracing.Tracer.LogError(ex);
            }

            // Maliyet/Değer alanları (BirimMaliyet, ToplamMaliyet, OrtalamaMaliyet, ToplamDeger, ...)
            // gerçek para birimi alanlarıdır (₺ alır) ama birim bazlı/hassas değerler olduğundan
            // Tutar'ın (yuvarlanmış işlem toplamları) değil Miktar'ın hane sayısı parametresini
            // kullanır — kullanıcı onayıyla netleşti (2026-08-13).
            string maliyetFormat = "₺" + miktarFormat;
            string maliyetMask = miktarMask;
            string yerelFormat = "₺" + tutarFormat;
            string yerelMask = tutarMask;

            // ⚠ Sonek-tahmini (EndsWith("Tutar")/EndsWith("Miktar") vb.) TERK EDİLDİ: kullanıcı canlı
            // testte "AlisFiyati" alanının hiçbir sonek kuralına uymadığı için hiç formatlanmadığını
            // yakaladı (bkz. docs/CHANGELOG.md 2026-08-13). Sonek sezgisi her yeni/farklı adlandırılmış
            // alanda tekrar aynı sınıfta hata üretir. Bunun yerine açık, bakımı yapılan bir liste
            // (Resources/Seed/ondalik-alan-listesi.csv, OndalikAlanKatalogu ile okunur) kullanılıyor —
            // kaçırılan bir alan bulunursa CSV'ye tek satır eklenir, bu kod değişmez.
            IReadOnlyDictionary<(string Sinif, string Alan), string> katalog = OndalikAlanKatalogu.Kategoriler;

            foreach (IModelClass modelClass in application.Model.BOModel)
            {
                // IModelClass.Name tam nitelikli tip adıdır (ör. "Sofasis.Module.BusinessObjects.StokTanim")
                // — CSV okunabilirlik için kısa sınıf adı taşıyor, burada karşılaştırma öncesi kısaltılır.
                string kisaSinifAdi = modelClass.Name;
                int noktaIndex = kisaSinifAdi.LastIndexOf('.');
                if (noktaIndex >= 0) kisaSinifAdi = kisaSinifAdi[(noktaIndex + 1)..];

                foreach (IModelMember member in modelClass.OwnMembers)
                {
                    if (member.Type != typeof(decimal) && member.Type != typeof(decimal?))
                        continue;
                    if (!katalog.TryGetValue((kisaSinifAdi, member.Name), out string kategori))
                        continue; // Listede yoksa dokunulmaz (oran/kur alanları vb. kasıtlı olarak dışarıda).

                    switch (kategori)
                    {
                        case "Miktar":
                            member.DisplayFormat = miktarFormat;
                            member.EditMask = miktarMask;
                            break;
                        case "Tutar":
                            // Sembolsüz bırakılır — OndalikFormatController, DetailView açılışında
                            // kaydın kendi (veya Master'ının) DovizTanim.Sembol'üne göre üzerine yazar.
                            member.DisplayFormat = tutarFormat;
                            member.EditMask = tutarMask;
                            break;
                        case "Yerel":
                            // Yerel* alanlar tanımı gereği her zaman TL'dir — dinamik sembole tabi DEĞİL.
                            member.DisplayFormat = yerelFormat;
                            member.EditMask = yerelMask;
                            break;
                        case "Maliyet":
                            member.DisplayFormat = maliyetFormat;
                            member.EditMask = maliyetMask;
                            break;
                    }

                    // ⚠ ListView sütunları (IModelColumn), BOModel üyesindeki bu değeri OTOMATİK
                    // MİRAS ALMAZ — canlı testte StokBakiye_ListView'de yakalandı: sembol (₺) doğru
                    // geliyordu (muhtemelen ayrı bir varsayılan kaynaktan) ama hane sayısı hep 4
                    // kalıyordu, BOModel-seviyesi değişikliğe rağmen. Bu yüzden aynı sınıfın TÜM
                    // ListView'lerindeki (Stok/Hizmet/Masraf gibi aynı sınıfın birden çok filtrelenmiş
                    // görünümü olabilir) eşleşen sütununa da AYNI format burada açıkça yazılır. Tutar
                    // kategorisinde dinamik sembol burada YOK (ListView hücreleri için satır-bazlı
                    // dinamik sembol, ayrı ve daha büyük kapsamlı "özel hücre şablonu" işidir) —
                    // yalnızca doğru hane sayısı garanti edilir.
                    foreach (IModelListView listView in application.Model.Views.OfType<IModelListView>())
                    {
                        if (listView.ModelClass == null) continue;
                        string listViewSinifAdi = listView.ModelClass.Name;
                        int lvNoktaIndex = listViewSinifAdi.LastIndexOf('.');
                        if (lvNoktaIndex >= 0) listViewSinifAdi = listViewSinifAdi[(lvNoktaIndex + 1)..];
                        if (listViewSinifAdi != kisaSinifAdi) continue;

                        IModelColumn column = listView.Columns[member.Name];
                        if (column == null) continue;
                        column.DisplayFormat = member.DisplayFormat;
                        column.EditMask = member.EditMask;
                    }
                }
            }
        }
        public override void CustomizeTypesInfo(ITypesInfo typesInfo)
        {
            base.CustomizeTypesInfo(typesInfo);
            CalculatedPersistentAliasHelper.CustomizeTypesInfo(typesInfo);
        }
    }
}
