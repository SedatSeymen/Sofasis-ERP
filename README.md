# SofasisERP

Kanepe ve mobilya üretimine özel, bulut tabanlı ERP sistemi.

## Teknoloji Yığını

| Bileşen | Sürüm / Seçim |
|---------|---------------|
| Framework | .NET 10 |
| UI Platformu | DevExpress XAF Blazor Server |
| ORM | DevExpress XPO |
| Veritabanı | PostgreSQL 16 |
| DevExpress | 26.1.3 |
| Bulut | Linux VPS |

## Geliştirme Ön Koşulları

- .NET 10 SDK
- PostgreSQL 16 (lokal Docker veya kurulum)
- DevExpress 26.1.3 lokal paket kaynağı (`C:\Program Files\DevExpress 26.1\Components\System\Components\Packages`)
- Docker Desktop (PostgreSQL konteyneri için)

## Başlangıç

### 1. PostgreSQL Bağlantısı

Geliştirme ortamında PostgreSQL `127.0.0.1:5432` adresinde çalışmalıdır.

Bağlantı bilgileri:
- Host: `127.0.0.1`
- Port: `5432`
- Database: `sofasiserp`
- Username: `sofasiserp_app`
- Password: `sofasis_local_dev` (lokal geliştirme)

### 2. Bağımlılıkları Yükle

```powershell
dotnet restore
```

### 3. Uygulamayı Çalıştır

```powershell
dotnet run --project src/SofasisERP/SofasisERP.Blazor.Server
```

Tarayıcıda `https://localhost:5001` veya `http://localhost:5000` adresini açın.

## Proje Yapısı

```
SofasisERP/
├── src/
│   └── SofasisERP/
│       ├── SofasisERP.Module/          # Platformdan bağımsız XAF modülü (XPO Business Objects)
│       └── SofasisERP.Blazor.Server/   # ASP.NET Core Blazor Server uygulaması
├── docs/
│   ├── architecture/                   # Mimari kararlar
│   └── setup/                          # Kurulum kayıtları
├── scripts/                            # Yardımcı betikler
├── tests/                              # Test projeleri (ileride)
├── .gitignore
├── NuGet.config
└── README.md
```

## Kurallar

- **ORM**: Yalnızca DevExpress XPO kullanılır. EF Core kullanılmaz.
- **DevExpress**: Sürüm 26.1.3'ten sapılmaz. Lokal paketler önceliklidir. Paket referanslarında wildcard (`26.1.*`) kullanılmaz, tam sürüm `26.1.3` yazılır.
- **Paket Güncelleme**: DevExpress paketleri 26.1.3'te sabittir. Diğer tüm paketler (Microsoft, Npgsql vb.) en son stable sürüme güncellenir. Hedef daima **0 uyarı (0 warnings)**.
- **Veritabanı**: PostgreSQL. Bağlantı bilgileri `appsettings.json` veya `dotnet user-secrets` ile yönetilir.
- **Kod**: Business domain kodu `SofasisERP.Module` projesinde, UI kodu `SofasisERP.Blazor.Server` projesinde tutulur.
- **Dokümantasyon**: Türkçe yazılır ve gerçek kodla tutarlı tutulur.

## Lisans

Özel (Proprietary) — Sofasis.
