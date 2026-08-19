# Production VPS Hazırlığı (2026-08-19)

Bu belge, SofasisERP'nin **paylaşılan** bir production VPS'te (Sofasis
IoT/Cloud platformuyla aynı sunucu) barındırılması için hazırlanan altyapıyı
belgeler. Asıl detaylı IaC (docker-compose, nginx, backup script'leri)
`D:\SofasisHomeAutomation\Brain\Operations\production\` altında — bu VPS
öncelikle o proje için kuruldu, SofasisERP sonradan aynı sunucuya eklendi.

## Mevcut durum

**Uygulama HENÜZ deploy edilmedi.** SofasisERP kod tabanında aktif
geliştirme sürdüğü için (kullanıcı isteğiyle) deploy bekletiliyor. Aşağıdaki
altyapı parçaları ise hazır ve VPS'te bekliyor:

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
  HTTPS sertifikası **hazır ve aktif** — henüz arkasında çalışan bir uygulama
  olmadığı için şu an 502 dönüyor, bu normal.
- **systemd servisi** (`sofasiserp.service`) tanımlı ama `enable`/`start`
  edilmedi — `ExecStart=/usr/bin/dotnet /opt/sofasiserp/SofasisERP.Blazor.Server.dll --urls http://127.0.0.1:5050`,
  `sofasis-admin` kullanıcısıyla çalışacak şekilde.
- **Bağlantı dizesi secret'ı** `appsettings.json`'a YAZILMAYACAK — VPS'teki
  `/opt/sofasiserp/.env` dosyasında (mode 600) `ConnectionStrings__ConnectionString`
  ortam değişkeni olarak duruyor (ASP.NET Core'un standart çift-alt-çizgi
  config override deseni), systemd `EnvironmentFile` ile enjekte ediliyor.

## Deploy edilecek zaman yapılacaklar (özet)

1. `D:\SofasisERP\src\SofasisERP.Blazor.Server`'da `dotnet publish -c Release`.
2. Çıktıyı VPS'te `/opt/sofasiserp/`'e kopyala (mevcut dosyaların üzerine).
3. `systemctl enable --now sofasiserp` (ilk kurulumda) veya
   `systemctl restart sofasiserp` (sonraki güncellemelerde).
4. `https://erp.sofasis.com` üzerinden smoke test.

Bu adımlar kullanıcının "ERP için deploy'a hazırım" onayı sonrası
uygulanacak — şu an sadece altyapı hazır durumda bekliyor.
