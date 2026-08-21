# Production VPS Hazırlığı (2026-08-19, ilk deploy 2026-08-22)

Bu belge, SofasisERP'nin **paylaşılan** bir production VPS'te (Sofasis
IoT/Cloud platformuyla aynı sunucu) barındırılması için hazırlanan altyapıyı
belgeler. Asıl detaylı IaC (docker-compose, nginx, backup script'leri)
`D:\SofasisHomeAutomation\Brain\Operations\production\` altında — bu VPS
öncelikle o proje için kuruldu, SofasisERP sonradan aynı sunucuya eklendi.

## Mevcut durum (2026-08-22 itibarıyla)

**Uygulama CANLI ve çalışıyor** — ilk deploy 2026-08-22'de yapıldı,
`https://erp.sofasis.com` üzerinden erişilebilir. Yeniden deploy için
`D:\SofasisERP\deploy-vps.ps1` scripti kullanılır (tek komut: `dotnet publish`
→ zip → VPS'e kopyala → izin düzelt → `--updateDatabase` → servis restart →
smoke test).

**Deploy sırasında öğrenilen kritik notlar:**
- SSH portu **22 değil, 22667** (bkz. `ufw-rules.txt`), IP kısıtlaması yok.
- Windows'ta `Compress-Archive` ile oluşturulan zip, bazı klasörlerin execute
  (`x`) bitini kaybediyor (`drw-r--r--` yerine `drwxr-xr-x` olmalı) — extract
  sonrası `chmod` şart, yoksa `wwwroot/_content` altındaki DevExpress JS/CSS
  dosyaları 404 döner ve rapor görüntüleyici/tasarımcı çalışmaz.
- `.env`'deki `ConnectionStrings__ConnectionString` noktalı virgül (`;`)
  içerdiği için düz `source .env` bash'te komut ayırıcı olarak yorumlanıp
  değeri kesiyor — değer tırnaklanarak source edilmeli (script bunu yapıyor).
- Aşağıdaki altyapı parçaları hazır ve kullanımda:

- **VPS**: `178.210.161.162`, Ubuntu 24.04 LTS, SSH sadece key ile (root
  girişi kapalı, `sofasis-admin` sudo kullanıcısı üzerinden erişim), firewall
  (`ufw`) + `fail2ban` aktif.
- **Veritabanı**: PostgreSQL 18 (Docker container `sofasis-postgres`,
  `127.0.0.1:5432`'e bind, dışarıya kapalı). `sofasiserp` adında ayrı bir
  veritabanı ve `sofasiserp_app` adında ayrı bir kullanıcı zaten oluşturuldu
  (Sofasis IoT'nin `SofasisAutomation` veritabanından tamamen izole).
  **Şema henüz yüklenmedi** — veritabanı boş, ilk deploy'da XPO'nun kendi
  `updateDatabase`/migration mekanizması veya elle bir schema-only dump ile
  doldurulacak.
- **.NET 10 ASP.NET Core runtime** kurulu (`aspnetcore-runtime-10.0`,
  `10.0.11`) — DevExpress paketleri bu makinede lisanslı OLMADIĞI için
  **build/publish mutlaka geliştirme makinesinde (`D:\SofasisERP`) yapılıp
  çıktı VPS'e kopyalanacak**, VPS'te `dotnet build` çalıştırılmayacak.
- **Domain**: `erp.sofasis.com` DNS'te tanımlı, VPS'e işaret ediyor. Nginx
  reverse proxy (`127.0.0.1:5050` → `erp.sofasis.com`) ve Let's Encrypt
  HTTPS sertifikası **aktif**, arkasında uygulama çalışıyor (200 dönüyor).
- **systemd servisi** (`sofasiserp.service`) `enable`/`active` — `ExecStart=/usr/bin/dotnet /opt/sofasiserp/SofasisERP.Blazor.Server.dll --urls http://127.0.0.1:5050`,
  `sofasis-admin` kullanıcısıyla çalışıyor.
- **Bağlantı dizesi secret'ı** `appsettings.json`'a YAZILMADI — VPS'teki
  `/opt/sofasiserp/.env` dosyasında (mode 600) `ConnectionStrings__ConnectionString`
  ortam değişkeni olarak duruyor (ASP.NET Core'un standart çift-alt-çizgi
  config override deseni), systemd `EnvironmentFile` ile enjekte ediliyor.

## Yeniden deploy (güncelleme) — `deploy-vps.ps1` ile tek komut

```powershell
D:\SofasisERP> .\deploy-vps.ps1
```

Script sırasıyla: `dotnet publish -c Release` → zip → servis durdur →
VPS'e kopyala → izinleri düzelt (`chmod 755` dizinler / `644` dosyalar) →
`--updateDatabase --forceUpdate --silent` → servis başlat → smoke test.
