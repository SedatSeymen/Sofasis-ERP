using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using Microsoft.Extensions.DependencyInjection;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.DatabaseUpdate
{
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
    public class Updater : ModuleUpdater
    {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
            base(objectSpace, currentDBVersion)
        {
        }
        public override void UpdateDatabaseAfterUpdateSchema()
        {
            base.UpdateDatabaseAfterUpdateSchema();
            //string name = "MyName";
            //DomainObject1 theObject = ObjectSpace.FirstOrDefault<DomainObject1>(u => u.Name == name);
            //if(theObject == null) {
            //    theObject = ObjectSpace.CreateObject<DomainObject1>();
            //    theObject.Name = name;
            //}

            // The code below creates users and roles for testing purposes only.
            // In production code, you can create users and assign roles to them automatically, as described in the following help topic:
            // https://docs.devexpress.com/eXpressAppFramework/119064/data-security-and-safety/security-system/authentication
#if !RELEASE
            // If a role doesn't exist in the database, create this role
            var defaultRole = CreateDefaultRole();
            var adminRole = CreateAdminRole();

            ObjectSpace.CommitChanges(); //This line persists created object(s).

            UserManager userManager = ObjectSpace.ServiceProvider.GetRequiredService<UserManager>();

            // If a user named 'User' doesn't exist in the database, create this user
            if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "User") == null)
            {
                // Set a password if the standard authentication type is used
                string EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "User", EmptyPassword, (user) =>
                {
                    // Add the Users role to the user
                    user.Roles.Add(defaultRole);
                });
            }

            // If a user named 'Admin' doesn't exist in the database, create this user
            if (userManager.FindUserByName<ApplicationUser>(ObjectSpace, "Admin") == null)
            {
                // Set a password if the standard authentication type is used
                string EmptyPassword = "";
                _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "Admin", EmptyPassword, (user) =>
                {
                    // Add the Administrators role to the user
                    user.Roles.Add(adminRole);
                });
            }

            ObjectSpace.CommitChanges(); //This line persists created object(s).
#endif

            // "Satınalma Onaycısı" gerçek bir yetki rolüdür (demo Admin/User hesaplarının aksine)
            // — Debug/Release fark etmeksizin oluşturulur ki üretimde gerçek onaycı kullanıcılara
            // atanabilsin. SatinAlmaTalebiOnayController.KullaniciOnaycıMı() bu rol adını arar.
            CreateSatinAlmaOnayciRole();
            ObjectSpace.CommitChanges();

            // Başlangıç (seed) verisi: CSV tabanlı yükleyici. Tüm konfigürasyonlarda
            // (Debug/Release) çalışır; idempotent olduğundan tekrar çalışması güvenlidir.
            new DatabaseSeeder(ObjectSpace).Seed();

            // StokHareketleriD'ye döviz bazlı maliyet desteği eklendi (DovizTanim/DovizKuru +
            // YerelBirimMaliyet/YerelToplamMaliyet) — BirimMaliyet artık HER ZAMAN TL değil, motor
            // (IWeightedAverageCostService) artık YerelBirimMaliyet okuyor. Mevcut kayıtların yeni
            // alanları şema güncellemesi sonrası 0 gelir; backfill yapılmazsa ObjectDeleting'in
            // silme-sonrası YenidenHesapla replay'i geçmiş kayıtları sıfır-maliyetli işler (veri
            // bozulması riski) — bu yüzden (CariHesapHareketleri'ndeki DovizTanim ekleme emsalinin
            // aksine, orada alanın DEĞERİ hiç değişmemişti) burada backfill ZORUNLU. İdempotent:
            // YerelBirimMaliyet==0 olan (henüz taşınmamış) satırlar filtrelenir — geçerli satırlarda
            // BirimMaliyet zaten her zaman >0 zorunlu olduğundan güvenli bir ayraçtır.
            BackfillStokHareketleriDDoviz();
        }

        void BackfillStokHareketleriDDoviz()
        {
            // TRY DovizTanim kaydı DatabaseSeeder.Seed() ile hemen üstte oluşturulmuş olmalı; yine
            // de sessizce atlamak yerine (backfill'in hiç çalışmadığını fark etmeden bırakmamak
            // için) burada açıkça bir istisna fırlatılır.
            DovizTanim tryDoviz = ObjectSpace.FirstOrDefault<DovizTanim>(d => d.DovizKodu == "TRY")
                ?? throw new InvalidOperationException("BackfillStokHareketleriDDoviz: TRY DovizTanim kaydı bulunamadı — DatabaseSeeder.SeedDoviz() önce çalışmış olmalı.");

            bool degisiklikVar = false;
            foreach (StokHareketleriD satir in ObjectSpace.GetObjects<StokHareketleriD>()
                .Where(x => x.YerelBirimMaliyet == 0 && x.BirimMaliyet != 0))
            {
                satir.DovizTanim = tryDoviz;
                satir.DovizKuru = 1;
                satir.YerelBirimMaliyet = satir.BirimMaliyet;
                satir.YerelToplamMaliyet = satir.ToplamMaliyet;
                degisiklikVar = true;
            }

            if (degisiklikVar)
                ObjectSpace.CommitChanges();
        }

        public override void UpdateDatabaseBeforeUpdateSchema()
        {
            base.UpdateDatabaseBeforeUpdateSchema();
            //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
            //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
            //}
        }
        PermissionPolicyRole CreateAdminRole()
        {
            PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
            if (adminRole == null)
            {
                adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                adminRole.Name = "Administrators";
                adminRole.IsAdministrative = true;
            }
            return adminRole;
        }
        PermissionPolicyRole CreateSatinAlmaOnayciRole()
        {
            PermissionPolicyRole role = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Satınalma Onaycısı");
            if (role == null)
            {
                role = ObjectSpace.CreateObject<PermissionPolicyRole>();
                role.Name = "Satınalma Onaycısı";
                // NOT: Tip/nesne izinleri kasıtlı olarak eklenmedi — bu rolün gerçek kullanıcılara
                // atanıp doğru izin setiyle donatılması ayrı bir güvenlik sertleştirme adımıdır
                // (SA-6 kapsamı). Bugünkü canlı doğrulama Administrators rolüyle yapılır (bkz.
                // SatinAlmaTalebiOnayController.KullaniciOnaycıMı() — IsAdministrative kontrolü).
            }
            return role;
        }

        PermissionPolicyRole CreateDefaultRole()
        {
            PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
            if (defaultRole == null)
            {
                defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
                defaultRole.Name = "Default";

                defaultRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
                defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
                defaultRole.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess, "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
                defaultRole.AddObjectPermission<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, "Owner.UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
                defaultRole.AddTypePermission<AuditDataItemPersistent>(SecurityOperations.Read, SecurityPermissionState.Deny);
                defaultRole.AddObjectPermissionFromLambda<AuditDataItemPersistent>(SecurityOperations.Read, a => a.UserId == CurrentUserIdOperator.CurrentUserId().ToString(), SecurityPermissionState.Allow);
                defaultRole.AddTypePermission<AuditedObjectWeakReference>(SecurityOperations.Read, SecurityPermissionState.Allow);

                // Kendi talebini onaylayamama (segregation of duties) korumasını devre dışı
                // bırakabilecek anahtar, Admin dışındaki hiçbir role görünmemeli — aksi halde
                // bu güvenlik kontrolü, kontrolün kendisine tabi olan kullanıcılar tarafından
                // kapatılabilir hale gelirdi. IsAdministrative rolleri (Administrators) bu
                // Deny'den her zaman muaftır (XAF güvenlik sistemi idari rolleri hiç sorgulamaz).
                defaultRole.AddMemberPermission<SatinAlmaParametre>(SecurityOperations.Read, nameof(SatinAlmaParametre.KendiTalebiniOnaylayamaz), null, SecurityPermissionState.Deny);

                // Satın Alma Talebi formu: normal kullanıcılar (Default) süreci başlatabilmek için
                // kendi taleplerini oluşturup düzenleyebilmeli. NOT: "yalnızca kendi talebini görsün"
                // şeklinde obje-bazlı (TalepEdenKullanici = CurrentUserId()) bir kısıtlama denendi,
                // ancak canlı testte güvenilir çalışmadı (satırlar filtrelenmek yerine alan bazında
                // maskeleniyordu — hem başkasının hem kullanıcının kendi kaydında). Bu yüzden bu
                // depoda zaten kanıtlanmış, basit tip-seviyesi Allow deseni tercih edildi (bkz.
                // ModelDifference/Contact örnekleri) — Default rolündeki tüm kullanıcılar (iç
                // kullanım aracı olduğundan) birbirinin talebini görüp düzenleyebilir; Sil zaten
                // yalnızca Taslak durumundayken UI'da açık (SatinAlmaTalebiKilitleController).
                defaultRole.AddTypePermissionsRecursively<SatinAlmaTalebiM>(SecurityOperations.CRUDAccess, SecurityPermissionState.Allow);
                defaultRole.AddTypePermissionsRecursively<SatinAlmaTalebiD>(SecurityOperations.CRUDAccess, SecurityPermissionState.Allow);

                // Talep kalemi satırındaki "Stok / Hizmet / Masraf" seçici için gereken bağımlılıklar —
                // bunlar okunabilir olmadan seçici boş kalır (StokTanim.BirimTanim ise Birim sütununda gösterilir).
                defaultRole.AddTypePermission<StokTanim>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddTypePermission<BirimTanim>(SecurityOperations.Read, SecurityPermissionState.Allow);

                // GenelParametre (miktar/tutar ondalık maskı gibi uygulama genelini etkileyen
                // ayarlar) yalnızca Administrators tarafından değiştirilebilir — Default rolü
                // görebilir (Read açıkça Allow) ama Create/Write/Delete kesin Deny. IsAdministrative
                // rolleri (Administrators) bu Deny'den her zaman muaftır. NOT: Güvenlik sistemi
                // fiilen DenyAllByDefault davranıyor (canlı testte doğrulandı — Default rolü, hiç
                // izin tanımlanmamış bir tipe URL ile bile erişemiyor, "Bu kaynağa erişim yasaktır"),
                // bu yüzden Read burada AÇIKÇA Allow edilmezse User hiçbir şekilde göremezdi.
                defaultRole.AddTypePermission<GenelParametre>(SecurityOperations.Read, SecurityPermissionState.Allow);
                defaultRole.AddTypePermission<GenelParametre>(SecurityOperations.Write, SecurityPermissionState.Deny);
                defaultRole.AddTypePermission<GenelParametre>(SecurityOperations.Create, SecurityPermissionState.Deny);
                defaultRole.AddTypePermission<GenelParametre>(SecurityOperations.Delete, SecurityPermissionState.Deny);
            }
            return defaultRole;
        }
    }
}
