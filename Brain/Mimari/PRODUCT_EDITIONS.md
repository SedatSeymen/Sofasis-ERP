<!-- ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : PRODUCT_EDITIONS.md
 * Oluşturma Tarihi : 2026-08-17
 * Oluşturan        : Sofasis Development Team
 * Son Güncelleme   : 2026-08-17
 * Son Güncelleyen  : Sofasis Development Team
 * Açıklama         : Basic/Pro/Enterprise sürüm stratejisi ve teknik parametrizasyon mekanizması.
 * ****************************************************************************
-->

# SofasisERP Ürün Seviyeleri (Edition Stratejisi)

## 1. Kapsam Matrisi

Kullanıcının kendi tanımıyla, koltuk üreten firmaların gerçek kullanım gözlemine dayanır:

| Modül | Basic | Pro | Enterprise |
|---|:---:|:---:|:---:|
| Stok Yönetimi | ✓ | ✓ | ✓ |
| Sipariş | ✓ | ✓ | ✓ |
| Sevkiyat | ✓ | ✓ | ✓ |
| Finans / Cari Hesap (ön muhasebe, basit) | ✓ | ✓ | ✓ |
| Satınalma | — | ✓ | ✓ |
| Üretim + Reçete/BOM + Maliyet | — | — | ✓ |
| Üretim Planlama | — | — | ✓ |

- **Basic** = koltuk firmalarının çoğunluğunun gerçekte kullandığı temel kapsam: basit ön muhasebe + sipariş + sevkiyat.
- **Pro** = Basic + Satınalma (fiş/fatura ayrık ilişkisi, tedarikçi yönetimi).
- **Enterprise** = Pro + Üretim + Üretim Planlama = Full ERP.

## 2. Dağıtım/Mimari Prensibi

**Tek kod tabanı, ayrı ürünler değil.** Basic→Pro→Enterprise bir yükseltme (upgrade) yoludur, farklı sistemler/derlemeler değil. Modüller **parametre/lisans bayrağı** ile açılıp kapanır.

## 3. Multi-Tenancy Mimarisi (DevExpress XAF resmi dokümantasyonu ile doğrulandı)

- XAF'ın yerleşik `AddMultiTenancy()` desteği **"tenant başına ayrı veritabanı"** modeli üzerine kuruludur — paylaşımlı şema+TenantId XAF'ta native desteklenmez ("The same database should not store data that belongs to several separate tenants"). Host Database (tenant listesi, Super Admin, paylaşılan/ortak veriler) + her tenant için ayrı Tenant Database.
- **Kritik kısıt:** Her tenant DB'si AYNI şemayı/tüm Business Object'leri içerir — tenant bazlı özel şema desteklenmez. Bu yüzden **Basic/Pro/Enterprise farkı şema seviyesinde değil, Controller ile modül gizleme/kapatma seviyesinde** uygulanır — framework'ün resmi önerisi budur.
- **Tenant ≠ Şirket:** `Tenant` sınıfı `CustomTenant : Tenant` olarak genişletilip üzerine `EditionSeviyesi` (Basic/Pro/Enterprise) alanı eklenir. **Tenant = 1 müşteri hesabı = 1 fiziksel veritabanı.** **Şirket** ise yalnızca Enterprise'daki holding ihtiyacı için, tenant'ın KENDİ veritabanı içinde bir iş nesnesi olarak modellenir — ayrı fiziksel DB değil. Login yönlendirmesi `TenantByEmailResolver` ile.
- **Bilinen sınırlama:** Multi-tenant modda persistent validation rules (`IRuleSource`) desteklenmez — implementasyon aşamasında doğrulanacak. Host UI'da edition/lisans yönetim ekranı hazır gelmez, `EditionSeviyesi` yönetim ekranı ayrıca kurulacaktır.

## 4. Uygulama Sırası (neden multi-tenancy en sona bırakıldı)

Multi-tenancy dönüşümü ve Enterprise'ın üretim/BOM derinliği bilinçli olarak **en sona** bırakılmıştır:

1. Koltuk firmalarının çoğunluğu Basic seviyesinde kalıyor — en çok değeri en hızlı üreten yol önce Basic'i uçtan uca çalışır hale getirmektir.
2. DevExpress'in resmi yol haritası "önce tek-tenant uygulama, sonra dönüşüm" akışını destekler (`Convert an Existing Application into a Multi-Tenant Application`) — multi-tenancy'yi en başta kurmak, hiçbir domain nesnesi yokken doğrulanması zor bir risk üstlenmek anlamına gelir.

Detaylı faz planı için bkz. proje kök dizinindeki güncel implementasyon planı.

## 5. Yetkilendirme Notu

Stok kategorisi tanımlama ekranları (`StokTipiTanim`/`StokGrupTanim`/`StokAltGrupTanim`) yalnızca muhasebe rolüne açıktır — genel "basit kullanıcı" arayüz kısıtının istisnasıdır, çünkü bu ekranları yalnızca muhasebe kullanır.
