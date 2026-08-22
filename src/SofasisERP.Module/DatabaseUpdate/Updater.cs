using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Updating;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using Microsoft.Extensions.DependencyInjection;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion) {
    }
    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();

        // GenelParametre/StokParametre.TekKayitAnahtari (tek-satır garantisi, unique
        // index) yeni eklenen bir sütun — şema güncellemesi ile var olan satırlarda
        // DB seviyesinde NULL kalır (ALTER TABLE ADD COLUMN mevcut satırları
        // doldurmaz). Postgres'te unique index NULL'ları birbirinden ayrı sayar, bu
        // yüzden geriye dönük doldurma yapılmazsa indeks var olan satırı KORUMAZ.
        // Tek seferlik, idempotent backfill (denetim raporu O4).
        foreach (GenelParametre parametre in ObjectSpace.GetObjects<GenelParametre>())
        {
            if (parametre.TekKayitAnahtari != 1)
                parametre.TekKayitAnahtari = 1;

            // Ondalık format kuralı netleşti: Miktar ve Kur alanları da Tutar gibi
            // daima 2 hane olmalı (önceden Miktar=4, Kur=6 idi). C# alan varsayılanı
            // yalnızca YENİ nesneleri kapsar; DB'de zaten var olan tek satır burada
            // geriye dönük düzeltilir.
            if (parametre.MiktarOndalikMaski != OndalikBasamakSayisi.Basamak2)
                parametre.MiktarOndalikMaski = OndalikBasamakSayisi.Basamak2;
            if (parametre.KurOndalikMaski != OndalikBasamakSayisi.Basamak2)
                parametre.KurOndalikMaski = OndalikBasamakSayisi.Basamak2;
        }
        foreach (StokParametre parametre in ObjectSpace.GetObjects<StokParametre>())
        {
            if (parametre.TekKayitAnahtari != 1)
                parametre.TekKayitAnahtari = 1;
        }

        // HesapTuru (2026-08-20) yeni eklenen bir sütun — şema güncellemesi mevcut
        // Kasa/Banka/Cari satırlarını geriye dönük doldurmaz, aksi halde Kasa/Cari/
        // Banka Hareketleri ekranlarının KaynakHesap/KarsiHesap filtrelemesi bu eski
        // kayıtları hiç göremez. Tek seferlik, idempotent backfill.
        foreach (KasaTanim kasa in ObjectSpace.GetObjects<KasaTanim>())
            if (kasa.HesapTuru != HesapTuru.Kasa) kasa.HesapTuru = HesapTuru.Kasa;
        foreach (BankaHesabiTanim banka in ObjectSpace.GetObjects<BankaHesabiTanim>())
            if (banka.HesapTuru != HesapTuru.Banka) banka.HesapTuru = HesapTuru.Banka;
        foreach (CariHesapTanim cari in ObjectSpace.GetObjects<CariHesapTanim>())
            if (cari.HesapTuru != HesapTuru.Cari) cari.HesapTuru = HesapTuru.Cari;

        // UygulananYerelBorcTutar/UygulananYerelAlacakTutar (2026-08-21) yeni eklenen
        // sütunlar — Açılış Fişi'nde düzenleme sonrası bakiye motorunun doğru "fark"
        // hesaplayabilmesi için gerekli (bkz. KasaCariBankaHareketleri.ObjectSaving).
        // Şema güncellemesi mevcut satırlarda bu alanları 0 bırakır; zaten işlenmiş
        // (MotorIslendi=true) satırlar için GERÇEKTE uygulanmış tutar hâlâ
        // YerelBorcTutar/YerelAlacakTutar'da duruyor (henüz hiç düzenlenmediler) —
        // bu yüzden oradan geriye dönük doldurulur. Doldurulmazsa, var olan bir Açılış
        // kaydı ilk kez düzenlendiğinde fark 0'a göre hesaplanır ve bakiye ÇİFT SAYILIR.
        foreach (KasaCariBankaHareketleri hareket in ObjectSpace.GetObjects<KasaCariBankaHareketleri>())
        {
            if (hareket.MotorIslendi && hareket.UygulananYerelBorcTutar == 0 && hareket.UygulananYerelAlacakTutar == 0
                && (hareket.YerelBorcTutar != 0 || hareket.YerelAlacakTutar != 0))
            {
                hareket.UygulananYerelBorcTutar = hareket.YerelBorcTutar;
                hareket.UygulananYerelAlacakTutar = hareket.YerelAlacakTutar;
            }
        }
        ObjectSpace.CommitChanges();

        // Kullanıcı isteği (2026-08-21): standart parola güvenliği (Security System +
        // kullanıcı adı/şifre girişi). DatabaseSeeder.userAdmin zaten "Admin" adlı bir
        // ApplicationUser bekliyordu (CreatedBy geriye dönük doldurma için) — bu yüzden
        // Admin kullanıcısı/rolü DatabaseSeeder.Seed()'den ÖNCE oluşturulur. Resmi
        // DevExpress örneği boş parola (SetPassword("")) kullanıyor — GÜVENLİK isteği
        // olduğu için burada BİLEREK gerçek bir başlangıç parolası + ilk girişte parola
        // değiştirme zorunluluğu (ChangePasswordOnFirstLogon) uygulanıyor; parola hash'i
        // XAF'ın kendi tuzlanmış hash mekanizmasıyla saklanır, düz metin DB'ye yazılmaz.
        SeedAdminKullanicisi();

        new DatabaseSeeder(ObjectSpace).Seed();
    }
    public override void UpdateDatabaseBeforeUpdateSchema() {
        base.UpdateDatabaseBeforeUpdateSchema();
        //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
        //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
        //}
    }

    // İlk kurulum: "Administrators" rolü (tam yetkili) + "Admin" kullanıcısı. Yalnızca hiç
    // kullanıcı yokken çalışır (idempotent) — Security System'in kendi ilk-kurulum deseni
    // (bkz. DevExpress "Use the Security System" belgesi), resmi DevExpress demosuyla aynı
    // şekilde boş parola ile (kullanıcı kararı 2026-08-21: geliştirme kolaylığı için) —
    // AMA yalnızca Development ortamında. 4-ajanlı testte (2026-08-22, yazılım mühendisi
    // bulgusu) tespit edildi: bu boş parola hiçbir ortam kontrolü olmadan production'a da
    // uygulanabiliyordu. Production/Staging'de SOFASIS_ADMIN_INITIAL_PASSWORD ortam
    // değişkeni ZORUNLU — yoksa güncelleme durur (sessizce boş parolaya düşmez).
    void SeedAdminKullanicisi()
    {
        ApplicationUser userAdmin = ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == "Admin");
        if (userAdmin == null)
        {
            userAdmin = ObjectSpace.CreateObject<ApplicationUser>();
            userAdmin.UserName = "Admin";

            if (OrtamKontrolu.GelistirmeOrtamiMi())
            {
                userAdmin.SetPassword("");
            }
            else
            {
                string ilkParola = Environment.GetEnvironmentVariable("SOFASIS_ADMIN_INITIAL_PASSWORD");
                if (string.IsNullOrEmpty(ilkParola))
                    throw new InvalidOperationException(
                        "Production/Staging ortamında Admin kullanıcısı boş parola ile oluşturulamaz. " +
                        "Lütfen SOFASIS_ADMIN_INITIAL_PASSWORD ortam değişkenini ayarlayıp veritabanı " +
                        "güncellemesini tekrar çalıştırın (ilk girişten sonra Admin kendi parolasını değiştirebilir).");
                userAdmin.SetPassword(ilkParola);
            }

            // UserLoginInfo nesnesi kullanıcının Oid'ine ihtiyaç duyar — önce commit edilir.
            ObjectSpace.CommitChanges();
            ((ISecurityUserWithLoginInfo)userAdmin).CreateUserLoginInfo(
                SecurityDefaults.PasswordAuthentication, ObjectSpace.GetKeyValueAsString(userAdmin));
        }
        else
        {
            // Savunma amaçlı onarım (kalıcı): "Admin" kaydı var ama karşılık gelen
            // ISecurityUserLoginInfo satırı yoksa (ör. elle DB temizliği, kısmi rollback)
            // standart kimlik doğrulama "Cannot login with Standard authentication type"
            // hatasıyla başarısız olur (DevExpress destek kaydı T1165436 ile aynı teşhis).
            // Eksikse sessizce yeniden oluşturulur.
            bool loginInfoVarMi = ObjectSpace.GetObjects<ApplicationUserLoginInfo>()
                .Any(li => li.User == userAdmin && li.LoginProviderName == SecurityDefaults.PasswordAuthentication);
            if (!loginInfoVarMi)
            {
                ((ISecurityUserWithLoginInfo)userAdmin).CreateUserLoginInfo(
                    SecurityDefaults.PasswordAuthentication, ObjectSpace.GetKeyValueAsString(userAdmin));
            }
        }

        PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
        if (adminRole == null)
        {
            adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            adminRole.Name = "Administrators";
        }
        adminRole.IsAdministrative = true;
        if (!userAdmin.Roles.Contains(adminRole))
            userAdmin.Roles.Add(adminRole);

        ObjectSpace.CommitChanges();
    }
}
