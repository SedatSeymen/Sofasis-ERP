<!-- ****************************************************************************
 * Proje            : Sofasis ERP
 * Dosya Adı        : INITIAL_SETUP_REPORT.md
 * Oluşturma Tarihi : 2026-08-17
 * Oluşturan        : Sofasis Development Team
 * Son Güncelleme   : 2026-08-17
 * Son Güncelleyen  : Sofasis Development Team
 * Açıklama         : İlk teknik kurulum doğrulama raporu (SOFASIS_INITIAL_SETUP.md §9)
 * ****************************************************************************
 -->

# İlk Teknik Kurulum Doğrulama Raporu

**Rapor Tarihi:** 2026-08-17
**Durum:** TAMAMLANDI
**Referans Doküman:** [SOFASIS_INITIAL_SETUP.md](../../SOFASIS_INITIAL_SETUP.md)

---

## 1. Ortam Bilgileri

| Bileşen | Sürüm / Değer | Durum |
|---------|---------------|-------|
| İşletim Sistemi | Microsoft Windows NT 10.0.26200.0 (Windows 11) | Doğrulandı |
| PowerShell | 5.1.26100.9168 | Doğrulandı |
| .NET SDK | 10.0.302 | Doğrulandı |
| .NET Runtime | 10.0.10 (Microsoft.NETCore.App) | Doğrulandı |
| Çalışma Dizini | `d:/SofasisERP` | Doğrulandı |

**Not:** Hedef üretim ortamı Linux VPS'tir; geliştirme ortamı Windows 11'dir. .NET 10 çapraz platform uyumluluğu bu farkı kapatır.

---

## 2. DevExpress 26.1.3 Doğrulaması

Mimari tasarım dokümanı §2 gereği DevExpress sürümü **26.1.3** olarak sabitlenmiştir (wildcard yasak).

### 2.1. NuGet Paket Kaynağı

| Özellik | Değer |
|---------|-------|
| Lokal kaynak | `C:\Program Files\DevExpress 26.1\Components\System\Components\Packages` |
| Uzak kaynak | `nuget.org` (yalnızca DevExpress dışı paketler) |
| Eşleştirme | `packageSourceMapping` ile `DevExpress.*` → lokal kaynak |

### 2.2. Paket Sürümleri

Her iki projede de tüm DevExpress paketleri **26.1.3** olarak doğrulandı:

**SofasisERP.Module:**

| Paket | Sürüm |
|-------|-------|
| DevExpress.ExpressApp | 26.1.3 |
| DevExpress.ExpressApp.CodeAnalysis | 26.1.3 |
| DevExpress.ExpressApp.ConditionalAppearance | 26.1.3 |
| DevExpress.ExpressApp.Validation | 26.1.3 |
| DevExpress.ExpressApp.Security | 26.1.3 *(bu oturumda eklendi)* |
| DevExpress.ExpressApp.Xpo | 26.1.3 |
| DevExpress.Persistent.Base | 26.1.3 |
| DevExpress.Persistent.BaseImpl.Xpo | 26.1.3 |
| Npgsql | 10.0.3 |
| System.IdentityModel.Tokens.Jwt | 8.22.0 |
| System.Security.Cryptography.Xml | 10.0.11 |

**SofasisERP.Blazor.Server:** DevExpress 26.1.3 paketleri doğrulandı.

### 2.3. Çıktı Doğrulaması

Build çıktı dizinindeki tüm DevExpress assembly'leri `v26.1` etiketlidir:
`DevExpress.ExpressApp.v26.1.dll`, `DevExpress.ExpressApp.Blazor.v26.1.dll`, `DevExpress.Xpo.v26.1.dll`, `DevExpress.ExpressApp.Security.v26.1.dll` vb.

XAF log dosyasında assembly çözümlemesi doğrulandı:
```
Resolve the 'DevExpress.Utils.v26.1, Version=26.1.3.0' assembly
```

---

## 3. PostgreSQL Doğrulaması

### 3.1. Bağlantı Parametreleri

| Parametre | Değer |
|-----------|-------|
| Host | 127.0.0.1 |
| Port | 5432 |
| Veritabanı | sofasiserp |
| Kullanıcı | sofasiserp_app |
| Provider | XpoProvider=Postgres (Npgsql) |

### 3.2. Doğrulama Sonuçları

| Test | Sonuç |
|------|-------|
| `Test-NetConnection 127.0.0.1 -Port 5432` | **TcpTestSucceeded = True** |
| Uygulama başlatma (XAF/XPO DB erişimi) | **Başarılı — hata yok** |
| `eXpressAppFramework.log` hata taraması | **0 hata / 0 exception** |

**Not:** XAF, veritabanına erişemediğinde başlangıçta `DatabaseVersionMismatch` veya bağlantı hatası fırlatır. Uygulamanın temiz başlaması ve log'da hiçbir DB hatası olmaması, PostgreSQL bağlantısının ve XPO şema eşleşmesinin çalıştığını kanıtlar.

**Açık Konu:** `psql` CLI aracı PATH'te bulunmuyor (PostgreSQL büyük olasılıkla Docker container veya Windows servisi olarak çalışıyor). Bu, uygulama bağlantısını etkilemez; yalnızca manuel SQL sorguları için bir yönetim aracı (pgAdmin, DBeaver vb.) gerekebilir.

---

## 4. Template / Proje Yapısı Doğrulaması

### 4.1. Proje Yapısı

Mimari doküman §22'ye uygun yapı doğrulandı:

```
d:/SofasisERP
├── src/
│   ├── SofasisERP.Module/              (Business domain — BusinessObjects, Controllers, DatabaseUpdate)
│   │   ├── BusinessObjects/
│   │   │   ├── ApplicationUser.cs      (bu oturumda oluşturuldu)
│   │   │   └── Base/
│   │   │       ├── BaseClass.cs
│   │   │       ├── BaseClassWithAudit.cs
│   │   │       ├── BaseClassWithDescription.cs
│   │   │       └── BaseClassWithAuditAndDescription.cs
│   │   ├── Module.cs
│   │   └── DatabaseUpdate/Updater.cs
│   └── SofasisERP.Blazor.Server/       (UI — XAF Blazor Server)
│       ├── BlazorApplication.cs
│       ├── BlazorModule.cs
│       ├── Startup.cs
│       ├── Program.cs
│       └── appsettings.json
├── tests/
├── docs/
│   ├── architecture/
│   └── setup/                          (bu raporun konumu)
├── NuGet.config
├── SOFASIS_ERP_MIMARI_TASARIM.md
└── SOFASIS_INITIAL_SETUP.md
```

### 4.2. Kayıtlı Modüller

[`Module.cs`](../../src/SofasisERP.Module/Module.cs) içinde kayıtlı modüller:
- `SystemModule`
- `ConditionalAppearanceModule`
- `ValidationModule`

---

## 5. Restore ve Build Sonuçları

### 5.1. Restore

Her iki proje için `dotnet restore` başarıyla tamamlandı (`.csproj` yolu belirtilerek — solution dosyası mevcut değil).

### 5.2. Build

| Proje | Komut | Sonuç |
|-------|-------|-------|
| SofasisERP.Module | `dotnet build` | Başarılı |
| SofasisERP.Blazor.Server | `dotnet build --no-restore` | **0 Uyarı, 0 Hata** |

**Hedef "0 uyarı" kuralı (kural §2) sağlandı.**

### 5.3. Çalışma Zamanı Doğrulaması

Uygulama `dotnet run` ile başlatıldı ve başarıyla çalıştı:

```
Now listening on: https://localhost:5001
Now listening on: http://localhost:5000
Application started.
Hosting environment: Development
```

XAF başlangıç döngüsü log'da doğrulandı: `SetupModules` → `Customize TypesInfo` → `Generate default Model` — tümü hatasız. Kullanıcı dili `tr-TR` olarak algılandı.

---

## 6. Bu Oturumda Yapılan Düzeltmeler

Kurulum kapanışı sırasında aşağıdaki sorunlar tespit edilip giderildi:

| # | Sorun | Çözüm | Dosya |
|---|-------|-------|-------|
| 1 | `ApplicationUser` tipi tanımsız (CS0246 × 6) | `DevExpress.ExpressApp.Security 26.1.3` paketi eklendi; [`ApplicationUser.cs`](../../src/SofasisERP.Module/BusinessObjects/ApplicationUser.cs) oluşturuldu (`PermissionPolicyUser` + `ISecurityUserWithLoginInfo`) | `SofasisERP.Module.csproj`, `ApplicationUser.cs` |
| 2 | `FisTuruTanim` tanımsız (CS0246) — eski ERP bağımlılığı | `FisTuruVarsayilanlariniUygula` metodu kaldırıldı (mimari doküman §41: eski ERP bağımlılığı taşınmaz) | [`BaseClass.cs`](../../src/SofasisERP.Module/BusinessObjects/Base/BaseClass.cs) |
| 3 | `Children` tanımsız (CS0103) — eski ERP yardımcısı | `Children.GetChildrenEntity` cascade-delete bloğu `OnDeleting`'den kaldırıldı (mimari doküman §9/§19: base sınıfta genel cascade delete yok) | [`BaseClass.cs`](../../src/SofasisERP.Module/BusinessObjects/Base/BaseClass.cs) |
| 4 | `using DevExpress.Persistent.BaseImpl;` eksik | Eklendi | [`BaseClassWithAudit.cs`](../../src/SofasisERP.Module/BusinessObjects/Base/BaseClassWithAudit.cs) |

Tüm düzeltmeler mimari doküman kurallarına uygundur:
- §2: DevExpress 26.1.3 sabit sürüm
- §19: Base sınıf standardı (cascade delete yok)
- §41: Eski ERP bağımlılıklarının taşınmaması

---

## 7. Açık Konular ve Sınırlamalar

| # | Konu | Etki | Önerilen Aksiyon |
|---|------|------|------------------|
| 1 | `psql` CLI PATH'te yok | Manuel SQL sorgusu çalıştırılamıyor | pgAdmin/DBeaver kurulumu veya Docker exec kullanımı (opsiyonel) |
| 2 | Solution dosyası (`.sln`) mevcut değil | `dotnet restore/build` komutları `.csproj` yolu ile çalıştırılmalı | İleride `.sln` oluşturulabilir (opsiyonel) |
| 3 | [`ApplicationUser`](../../src/SofasisERP.Module/BusinessObjects/ApplicationUser.cs) `UserLogins`/`CreateUserLoginInfo` stub | OAuth login bilgisi henüz persist edilmiyor | Güvenlik aşamasında (mimari §17) `ApplicationUserLoginInfo` sınıfı ile tamamlanacak |
| 4 | Henüz hiçbir domain Business Object'i yok | Beklenen durum | Mimari §42: İlk domain karar kapısı kapanmadan kalıcı domain modeli oluşturulmayacak |

---

## 8. Sonuç

İlk teknik kurulum **başarıyla tamamlanmıştır**:

- Derleme: **0 uyarı, 0 hata**
- DevExpress: **tüm paketler 26.1.3** (sabit sürüm kuralı sağlandı)
- PostgreSQL: **bağlantı ve şema eşleşmesi doğrulandı**
- Uygulama: **başarıyla başlatıldı** (https://localhost:5001)
- Eski ERP bağımlılıkları: **temizlendi**

Teknik altyapı, mimari tasarım dokümanı §42'de tanımlanan **ilk domain karar kapısı** (ürün + modül + uyumluluk + konfigürasyon + stok) için hazırdır.
