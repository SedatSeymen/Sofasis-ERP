# Sofasis ERP — Mevcut Kod Analizi (Güçlü / Eksik / Darboğaz)

**İncelenen kaynak:** `D:\2025\ProjectsBackup\Sofasis\Sofasis` (Sofasis Erp Project)
**Kapsam:** XAF Blazor Server solution — `Sofasis.Module` (domain/XPO), `Sofasis.Blazor.Server` (host), `FileSystemData` (dosya deposu). Toplam ~111 `.cs` (bin/obj hariç).
**İncelenen dosyalar (örnek):** BaseClass / BaseClassWithAudit / …AndDescription, IDGeneratorService, SequenceGenerator(+Helper), Helper, Module.cs, Startup.cs, CariHesapTanim, CariHesapHareketleri, StokTanim, StokModelTanim, StokModelKonfigrasyonM, SatisSiparisM, SatisSiparisD, ReceteTanimM, KDVTanim, TopluReceteMaliyetViewController.
**Belge tarihi:** 2026-08-09

---

## 1. Genel Değerlendirme

Bu, prototip değil **olgun ve tutarlı** bir XAF ERP'si. Katman ayrımı doğru, taban sınıf hiyerarşisi düşünülmüş, isimlendirme disiplinli ve koltuk üretimine özel iş mantığı (model/konfigürasyon, reçete, rota, maliyet, sipariş durum akışı) zaten yerleşik. Yapı, SofasisERP için sağlam bir şablon olmaya kesinlikle uygun. Aşağıda güçlü yönleri, eksikleri ve performans/tutarlılık darboğazlarını dosya bazında çıkardım; sonda düzeltme ve devam önerisi var.

---

## 2. Güçlü Yönler

**Katman ve proje düzeni.** `Sofasis.Module` (platformdan bağımsız domain) + `Sofasis.Blazor.Server` (host/DI/güvenlik) + `FileSystemData` (dosya saklama) ayrımı standart ve doğru XAF yaklaşımı. Domain klasörleri Türkçe iş alanlarına göre bölünmüş (CariHesapYonetimi, StokYonetimi, UretimYonetimi, FinansYonetimi, GenelTanimlar, SatinAlmaYonetimi, SatisPazarlamaYonetimi).

**Taban sınıf hiyerarşisi (DRY).** `BaseClass` → `BaseClassWithAudit` → `BaseClassWithAuditAndDescription`. Ortak kaygılar tek yerde: `KeyID` (otomatik), `IsDefault`/`IsSystemRecord`, tek-varsayılan zorlaması (`OnSaving`), sistem kaydı silme koruması, child cascade, `ObjectSaving/ObjectDeleting` sanal kancaları; audit alanları ve `CustomCode1/2`+`Description`.

**Disiplinli isimlendirme.** Türkçe sınıf/property + net son ekler: `Tanim` (kart), `M`/`D` (başlık/satır), `Hareketleri` (hareket), `Parametre` (ayar). Kod kendi kendini belgeliyor; Türk ekip için okunabilirliği yüksek.

**Zengin, hazır domain.** Stok kartında ölçü→m²/m³ otomatik hesap; StokModelTanim + StokModelKonfigrasyon (varyant/konfigürasyon); sipariş durum akışı (Girildi→Üretime Alındı→Üretildi→Sevk) renk ve `ListViewFilter` ile; Reçete (M/D) + Rota + maliyet; fiyat listeleri; döviz günlük kur (M/D); kasa/banka; Fiş Türü'ne dayalı belge kodlama.

**İyi XAF idiomları.** `DefaultClassOptions`, `ConditionalAppearance` (durum renkleri/enable-disable), `DataSourceCriteria`/`DataSourceProperty` (kademeli lookup), `[Association]+[Aggregated]` master-detail, `RuleRequiredField/RuleUniqueValue/RuleValueComparison`, `ImmediatePostData`, `PersistentAlias`.

**Güvenlik ve altyapı.** Integrated Security (ApplicationUser, OAuth uyumlu, lockout), AuditTrail, ReportsV2, StateMachine, Dashboards, Validation, Office modülleri host'ta düzgün kurulmuş (`Startup.cs`). `ThreadSafe = true`, `UseSharedDataStoreProvider = true`.

**Numaralandırma bilinci.** Üç ayrı mekanizma: `IDGeneratorService` (KeyID — tick tabanlı base-32, `Interlocked`+lock), `SequenceGeneratorHelper` (UnitOfWork + optimistic-lock retry), ve belge kodları için DevExpress `DistributedIdGeneratorHelper`. Eşzamanlılık düşünülmüş.

---

## 3. Eksik Yönler

**Sürüm/çatı kayması.** `Sofasis.Module.csproj` → `net8.0` + DevExpress **24.1.7**; ancak bin klasörlerinde net10.0/net8.0 karışık. Hedef **26.1.3 / .NET 10**. Temiz bir yükseltme adımı gerekli. Ayrıca `AssemblyVersion 1.0.*` + `Deterministic=false` (yinelenebilir derleme değil).

**Kod hijyeni / tekrar.**
- İkiz taban-sınıf klasörü: hem `BusinessObjects/Base/` hem `BusinessObjects/BaseClasses/` aynı 4 sınıfı içeriyor → kafa karışıklığı ve sürüklenme (divergence) riski.
- İkiz `IdGenerator`: `Helper/IdGenerator.cs` ve `Generators/IdGenerator.cs`.
- Ölü kod: csproj `BusinessObjects/SatisYonetimi/**`'i derlemeden çıkarmış (yerini `SatisPazarlamaYonetimi` almış) ama klasör duruyor.
- Depoya girmiş kopya dosya: `ProcessCariHesapHareketleriListViewRowController - Copy.cs`.

**Devre dışı bırakılmış referans-bütünlük kontrolleri.** `StokTanim.OnDeleting`, `ReceteTanimM.OnDeleting`, `StokModelTanim.OnDeleting` içindeki "kullanılmış mı?" koruma blokları **tamamen yorum satırı**. Yani şu an bir stok/model/reçete, sipariş veya fiyat listesinde kullanılıyorken silinebilir → veri bütünlüğü riski.

**Ön muhasebe / vergi tarafı eksik.** Üretim + satış + cari/kasa-banka hareketi var; ancak Türk mevzuatına uygun ön muhasebe için gerekenler görünmüyor: **satış/alış faturası** nesneleri, **e-Fatura/e-Arşiv/e-İrsaliye**, **tevkifat/istisna**, **Tekdüzen Hesap Planı + muhasebe fişi/aktarım**, **stok hareket defteri + ambar bakiyesi + ağırlıklı ortalama maliyet**, çek/senet. KDV yalnızca oran (KDVTanim) ve Dahil/Hariç hesabı düzeyinde.

**Test ve servis katmanı yok.** İş mantığı iş nesnelerinin `OnChanged/OnSaving/OnLoaded` metotlarında; birim test projesi yok. Hesaplama (KDV, maliyet, toplamlar) domain'e gömülü, izole test edilemez.

**Lokalizasyon.** Etiketler `[XafDisplayName("...")]` ile koda gömülü Türkçe. `StokAdiIngilizce`/`StokModelAdiIngilizce` alanları çok dillilik niyetini gösteriyor ama UI etiketleri kaynak dosyaya (resx) taşınmamış → ileride EN eklemek zor.

**Küçük tutarlılık hataları.** `SatisSiparisD` sınıfının başlığı `[XafDisplayName("Satın Alma Sipariş Detayları")]` — aslında **satış** detayı (kopyala-yapıştır kalıntısı). Benzer etiket/криterya kalıntıları başka yerlerde de olabilir.

---

## 4. Darboğazlar (Performans / Tutarlılık)

> **Durum (2026-08-11):** D-1 → D-6 kapatıldı (ayrıntı: `docs/CHANGELOG.md`, "Faz A Kapanışı" kaydı). D-7 gerçek değil çıktı — aşağıda not edildi.

**D-1 · Kayıt başına DB okuması (chatty access). ✅ Çözüldü.** `StokTanim.AfterConstruction` her yeni kayıtta 5 `FindObject` (varsayılan grup/birim/KDV/döviz/depo) yapıyordu. `SatisSiparisD.OnLoaded` **her satır yüklendiğinde** `StokParametre` çekiyordu (asıl N+1 kaynağı — `SatisSiparisM.UpdateTotals` tarafındaki sorgu incelemede zaten döngü DIŞINDA tek sefer çalıştığı görüldü, ilk tespit bu noktada hatalıydı). Çözüm: `Session.GetVarsayilan<T>()`/`GetSingleton<T>()` (`Sofasis.Module/Extensions/SessionCacheExtensions.cs`) — Session ömrü boyunca geçerli bir önbellek.

**D-2 · `ObjectSaving` içinde ayrı `new Session`. ✅ Çözüldü.** `CariHesapHareketleri.ObjectSaving`, kasa/banka aynasını yazmak için `new Session(Session.DataLayer)` açıp orada `Save()` yapıyordu (dispose edilmiyordu, outer transaction'dan bağımsızdı → rollback durumunda yarım/tutarsız veri riski). Çözüm: aynı `Session` (outer UnitOfWork) içinde çalışacak şekilde yeniden yazıldı; karşı kayıt artık `IntegrationCode`/`IntegrationSourceEntity` (`BaseClass`) çiftiyle bulunuyor. Ayrıca inceleme sırasında **gerçek bir bug** bulundu: `CariHesapHareketleri.ObjectDeleting()` karşı taraf yerine kendi tipini arayıp siliyordu (silme cascade'i hiç çalışmıyordu) — bu da düzeltildi.

**D-3 · Global kilit + DB commit (sequence). ✅ Çözüldü (önceki fazda).** `INumberSequenceService`/`NumberSequenceService` ile merkezi, global kilitsiz, aynı transaction'da çalışan numaralandırmaya geçildi.

**D-4 · Döngü içinde commit + UI refresh. ✅ Çözüldü.** `TopluReceteMaliyetViewController`'da `os.CommitChanges()`/`View.RefreshDataSource()` döngü dışına taşındı (N commit yerine 1 commit).

**D-5 · Thumbnail her erişimde diskten. ✅ Çözüldü.** `Helper.GetImage` artık `IMemoryCache` (`Sofasis.Blazor.Server/Program.cs` + `Startup.cs`'de kayıtlı) kullanıyor; cache anahtarı dosyanın son değişiklik zamanını içerdiği için resim değiştiğinde otomatik geçersiz olur.

**D-6 · String `KeyID` ile çapraz-tablo bağı. ✅ Çözüldü.** `CariHesapHareketleri`↔`KasaBankaHareketleri` artık gerçek PK'yı (`KeyID`) paylaşmıyor — bunun için var olan `IntegrationCode`/`IntegrationSourceEntity` (`BaseClass`) alan çifti kullanılıyor, `IntegrationCode`'a indeks eklendi. (Not: XPO'nun `[Association]`'ı bire-bir ilişkide iki tarafta da tekil referansı desteklemiyor — `AssociationInvalidException` ile denendi ve elendi; `IntegrationCode` tabanlı çözüm hem bu kısıtı aşıyor hem de gerçek PK'yı serbest bırakıyor.)

**D-7 · `PermissionsReloadMode.NoCache`. ⚠ Bu tespit geçerli değil (belge/kod tutarsızlığı).** Kod incelemesinde mevcut projenin zaten `PermissionsReloadMode.CacheOnFirstAccess` + `UseXpoPermissionsCaching()` kullandığı doğrulandı — `NoCache` değil. Bu analiz raporunun başlığında belirtilen "İncelenen kaynak" (`D:\2025\ProjectsBackup\Sofasis\Sofasis`) muhtemelen farklı/eski bir yedek kopyaydı. Kodda düzeltilecek bir şey yok.

---

## 5. Öneri

**Yapıyı benimse — evet.** Katman düzeni, taban sınıflar, Türkçe isimlendirme ve modül klasörleri SofasisERP için doğrudan şablon alınmalı; ekip bu idioma alışık ve XAF açısından sağlam. Ancak yeni kurulumda aşağıdakiler baştan düzeltilmeli:

1. **Tek taban-sınıf klasörü** (`Base/`), ikiz `IdGenerator`/ölü `SatisYonetimi`/"- Copy" dosyaları temizlensin.
2. **Referans-bütünlük kontrolleri** (silme koruması) yorumdan çıkarılıp merkezi ve tutarlı hale getirilsin.
3. **Numaralandırma tek sisteme** indirgensin (tercihen `DistributedIdGeneratorHelper`; `SequenceGeneratorHelper`'daki global kilit kaldırılsın).
4. **Kasa/banka aynalama** ayrı session yerine gerçek `[Association]` veya bir `IPostingService` ile parent transaction'ı içinde yapılsın (D-2).
5. **Parametre/varsayılan okumaları** önbelleğe alınsın; `OnLoaded`'da satır başına sorgu kaldırılsın (D-1).
6. **Toplu işlemlerde** commit/refresh döngü dışına alınsın (D-4).
7. **Servis + test katmanı**: KDV, maliyet (ağırlıklı ortalama), fiş üretimi arayüz arkasına alınıp birim testleri yazılsın.
8. **Ön muhasebe modülleri** eklensin: fatura, e-Belge (entegratör bağımsız), tevkifat, Tekdüzen fiş/aktarım, stok hareket + ambar bakiye + maliyet motoru.
9. **Yükseltme**: DevExpress 26.1.3 / .NET 10; `Deterministic=true`, sabit `FileVersion`.

**Sonraki adım önerim:** Bu mevcut projeyi temel alıp (a) hijyen + yükseltme temizliği yaparak, (b) ön muhasebe modüllerini aynı konvansiyonlarla ekleyerek ilerlemek. İstersen bunun için fazlı bir plan çıkarırım.

---

*Bu rapor statik kod okumasına dayanır; çalışma zamanı profillemesi (SQL trace, gerçek veri hacmi) darboğaz sıralamasını değiştirebilir. Derleme/çalıştırma ile doğrulanması önerilir.*
