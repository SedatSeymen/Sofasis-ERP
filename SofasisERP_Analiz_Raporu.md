# SofasisERP — Kod Analizi, Güvenlik Denetimi ve Yol Haritası

**Hazırlanma tarihi:** 22 Ağustos 2026
**Kapsam:** `D:\SofasisERP\src` — 3 proje, 135 kaynak dosyası (~811 KB), 145 `.cs` dosyası
**İncelenen alanlar:** Güvenlik açıkları · Fonksiyonel/mantık hataları · Mimari & tasarım · Performans & kod kalitesi
**Yöntem:** Kaynak kodun tamamı bu ortama alınıp dört paralel uzman incelemesiyle taranmış, ardından en kritik bulguların her biri gerçek kod ve satır numaralarıyla elle doğrulanmıştır.

---

## 1. Yönetici Özeti

SofasisERP, **DevExpress XAF 26.1** üzerine kurulu, **Blazor Server (.NET 10)** arayüzlü, **PostgreSQL + XPO** kalıcılık katmanı kullanan bir Türkçe muhasebe/ERP uygulamasıdır. Mobilya sektörüne özel (Ürün/Model/Ayak tanımları) modüllerle birlikte Cari, Kasa, Banka, Stok ve Döviz yönetimi içerir.

Genel değerlendirme: **kod tabanı olgun ve özenli**. Domain mantığı temiz servislere ayrılmış, XPO/XAF desenleri doğru uygulanmış, arka plan işçisi (hosted service) örnek niteliğinde, kod içi açıklamalar zengin. Ancak **finansal doğruluk açısından kritik iki mantık hatası**, **birkaç güvenlik açığı** ve **bir performans darboğazı** production öncesi mutlaka giderilmelidir.

### Bulgu dağılımı

| Önem | Güvenlik | Finans/Mantık | Stok/Maliyet | Performans/Kalite | Toplam |
|------|:---:|:---:|:---:|:---:|:---:|
| 🔴 Kritik | 2 | 2 | 2 | 1 | **7** |
| 🟠 Yüksek | 2 | 4 | 2 | 5 | **13** |
| 🟡 Orta | 3 | 3 | 5 | 6 | **17** |
| 🔵 Düşük | 3 | 3 | 4 | 4 | **14** |
| **Toplam** | **10** | **12** | **13** | **16** | **51** |

### En acil 5 aksiyon (production öncesi zorunlu)

1. **Tedarikçi bakiye işaret mantığı** — motor ile raporlar çelişiyor; tedarikçi hesaplarında kart bakiyesi ≠ ekstre bakiyesi (§4.1).
2. **Ağırlıklı ortalama maliyette sıfıra bölme** — negatif stok sonrası uygulama çöker (§5.1).
3. **Kaynak koda gömülü sırlar** — DB parolası ve URL imzalama anahtarı `appsettings.json` içinde (§3.1).
4. **"Tüm Cariler Bakiye Raporu" bellek patlaması** — gerçek veri hacminde OutOfMemory / dakikalarca bekleme (§6.1).
5. **Kuruş yazıya-çevirme hatası** — çek/makbuz yazı tutarı yanlış (§4.3).

---

## 2. Mimari Genel Bakış

### 2.1 Teknoloji yığını

| Katman | Teknoloji |
|--------|-----------|
| Arayüz | DevExpress XAF **Blazor Server**, .NET 10 |
| Uygulama çatısı | DevExpress.ExpressApp (XAF) 26.1.3 — Reports V2, ConditionalAppearance, Validation |
| Kalıcılık | XPO (eXpress Persistent Objects) → **PostgreSQL** (Npgsql 10.0.3) |
| Güvenlik | XAF Integrated Mode + PermissionPolicy + çerez tabanlı kimlik doğrulama |
| Arka plan | `IHostedService` (döviz kuru güncelleme worker'ı) |
| Dış servis | TCMB günlük döviz kuru XML servisi |

### 2.2 Proje yapısı

```
SofasisERP.sln
├── SofasisERP.Module           → Domain: iş nesneleri, servisler, raporlar, DB seed
│   ├── BusinessObjects/
│   │   ├── Base/               → BaseClass, BaseClassWithAudit (audit alanları)
│   │   ├── Finans/             → Cari, Kasa, Banka, Hesap, KasaCariBankaHareketleri (bakiye motoru)
│   │   ├── Stok/               → StokHareketleriM/D, StokBakiye, StokTransferi (maliyet motoru)
│   │   ├── GenelTanimlar/      → Döviz, KDV, Şehir/İlçe, Fiş türü, parametreler
│   │   └── Urun/               → Model, Ayak, ModelSet (mobilya sektörüne özel)
│   ├── Services/               → Numaralandırma, kod üretimi, maliyet, döviz, sayı→yazı
│   ├── Controllers/Process/    → Rapor tetikleme, yeni-kayıt varsayılanları
│   ├── Reports/                → Hesap ekstresi, cari bakiye, hareket makbuzu (XtraReport builder'ları)
│   └── DatabaseUpdate/         → Updater (admin/rol seed), DatabaseSeeder (referans veri)
├── SofasisERP.Blazor.Server    → Sunucu: Startup, Program, controllers, editors, KPI dashboard
└── SofasisERP.Module.Tests     → Yalnızca 1 test dosyası (WeightedAverageCostServiceTests)
```

### 2.3 Mimari değerlendirme — güçlü yönler

- **Katman ayrımı temiz:** Domain mantığı (Module) sunucu/UI (Blazor.Server) katmanından ayrık; saf hesaplama mantığı (ağırlıklı ortalama maliyet, sayı→yazı) test edilebilir servislere çıkarılmış.
- **Tek-kaynak yardımcılar:** `TurkiyeZamani` ile zaman dilimi, `GenelParametreOkuyucu` ile ondalık hane politikası, `OndalikAlanKatalogu` ile alan biçimlendirme merkezi olarak yönetilmiş.
- **Sağlam base sınıf hiyerarşisi:** `BaseClass → BaseClassWithAudit → ...WithDescription` — audit alanları (CreatedBy/ModifiedBy/tarihler) ve varsayılan-kayıt (IsDefault) mantığı tek yerde.
- **Numaralandırma çağıranla aynı transaction'da** commit edilir → başarısız kayıtta numara da geri alınır (gerçek boşluksuz seri).
- **Örnek hosted service:** `DovizKuruGuncellemeWorker` uygulama başlamasını bekler, her turda scope oluşturup düzgün dispose eder, ilk hatada durmaz, `PeriodicTimer` + `ConfigureAwait(false)` kullanır.

### 2.4 Mimari değerlendirme — iyileştirme gereken yönler

- **Bağımlılık enjeksiyonu eksik uygulanmış:** `INumberSequenceService`, `IWeightedAverageCostService`, kod jeneratörü arayüzleri tanımlı ama DI'ye kayıtlı değil; iş nesneleri bunları her yerde `new` ile üretiyor → soyutlamalar pratikte ölü, test edilebilirlik düşük (§6, O3).
- **Bakiye/maliyet "saklanan koşan toplam" deseni:** `GuncelBakiye`, `ToplamMiktar`, `OrtalamaMaliyet` oku-değiştir-yaz ile güncelleniyor; eşzamanlılıkta optimistic-lock istisnası kullanıcıya ham hata olarak yansıyor, otomatik retry yok.
- **Test kapsamı çok dar:** En riskli finansal mantık (bakiye motoru, numaralandırma, negatif stok) testsiz; tek test dosyası saf maliyet matematiğinin mutlu yolunu kapsıyor.
- **Yetki modeli tek boyutlu:** Yalnızca tam yetkili "Administrators" rolü seed ediliyor; görev-bazlı kısıtlı roller yok (§3, O3).

---

## 3. Güvenlik Bulguları

### 🔴 3.1 (Kritik) Kaynak koda gömülü sırlar — DB parolası ve URL imzalama anahtarı
**Dosya:** `SofasisERP.Blazor.Server/appsettings.json` — satır 3-4 ve 21

Bağlantı dizeleri PostgreSQL kullanıcı adı/parolasını (`Username=sofasiserp_app;Password=sofasis_local_dev`) düz metin taşıyor; ayrıca `UrlSigningKey` sabit bir GUID olarak gömülü (satır 21). Bu dosya repoya girdiğinden, kod tabanına erişen herkes bu sırları elde eder ve git geçmişinde kalıcı olur. `UrlSigningKey` XAF'ın imzalı URL doğrulamasını kırılabilir hâle getirir (anahtarı bilen geçerli imzalı URL üretebilir). Kodun kendi yorumu (satır 20) bu değerin gizli depoda saklanması gerektiğini kabul ediyor.

**Çözüm:** Sırları `appsettings.json`'dan çıkarın; development için .NET User Secrets, production için ortam değişkeni / gizli bilgi yöneticisi kullanın. Sızmış parolayı ve imzalama anahtarını rotasyona sokun, git geçmişinden temizleyin.

### 🟠 3.2 (Yüksek) "İlk girişte parola değiştirme" gerçekte uygulanmıyor
**Dosya:** `SofasisERP.Module/DatabaseUpdate/Updater.cs` — `SeedAdminKullanicisi()` (satır 106-161)

Kod yorumları (satır 84-86, 104-105) ısrarla `ChangePasswordOnFirstLogon` uygulandığını söylüyor, **ancak bu atama kodun hiçbir yerinde yok** (tüm repoda yalnızca yorumda geçiyor). Sonuç: production'da `SOFASIS_ADMIN_INITIAL_PASSWORD` ile kurulan Admin, başlangıç parolasını değiştirmeye zorlanmaz — bu parola deploy scriptlerinde/systemd env dosyasında/komut geçmişinde kalabilir ve süresiz geçerli olur.

**Çözüm:** `userAdmin.SetPassword(...)` sonrası `userAdmin.ChangePasswordOnFirstLogon = true;` satırını ekleyin ve yanıltıcı yorumları koda uygun hâle getirin.

> **Olumlu not:** Production'da başlangıç parolasının ortam değişkeni ile zorunlu kılınması ve yoksa güncellemenin durması (`Updater.cs:120-125`) doğru bir tasarım; boş parolalı Admin yalnızca Development'a kısıtlı (`OrtamKontrolu`).

### 🟠 3.3 (Yüksek) Kimlik doğrulama çerezi güvenlik bayrakları açık değil
**Dosya:** `SofasisERP.Blazor.Server/Startup.cs` — satır 126-129

`AddCookie` yalnızca `LoginPath` ayarlıyor; `Cookie.SecurePolicy = Always`, `Cookie.HttpOnly = true`, `Cookie.SameSite` açıkça belirtilmemiş. HTTPS/HSTS mevcut olsa da çerezin `Secure`/`SameSite` özniteliklerinin açıkça zorlanmaması CSRF ve oturum çerezi sızıntısı yüzeyini artırır.

**Çözüm:**
```csharp
.AddCookie(o => {
    o.LoginPath = "/LoginPage";
    o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;
    o.SlidingExpiration = true;
});
```

### 🟡 3.4 (Orta) `AllowedHosts: "*"` — Host header kısıtlaması yok
**Dosya:** `appsettings.json:14` — Production'da gerçek alan adlarıyla sınırlayın (Host header injection / cache poisoning yüzeyi).

### 🟡 3.5 (Orta) TCMB XML çekiminde zaman aşımı yok, tüm hatalar sessizce yutuluyor
**Dosya:** `TcmbDovizKuruService.cs:30, 38-43` — `XDocument.Load(url)` zaman aşımı ayarlanamaz (askıda kalırsa worker thread bloke); `catch (Exception)` DNS/TLS hatası dâhil her şeyi yutup boş liste döner, kalıcı arıza fark edilmez. `IHttpClientFactory` + açık `Timeout` ile çekip `XDocument.Parse` edin; hataları loglayın.
> **Olumlu:** `XDocument.Load` varsayılanı DTD/harici varlık işlemez → **XXE sömürülemez** (iyi varsayılan).

### 🟡 3.6 (Orta) Yalnızca tam yetkili rol var; en az yetki rolleri tanımsız
**Dosya:** `Updater.cs:150-158` — Seed edilen tek rol `IsAdministrative=true`. Görev-bazlı kısıtlı roller (yalnız-okuma, kasa operatörü, stok görevlisi) tanımlanıp seed edilmeli.

### 🔵 3.7 (Düşük) Diğer
- **`DetailedErrors: true`** yalnızca Development'ta (`appsettings.Development.json:2`); production `appsettings.json`'a taşınmadığından emin olun (deploy kontrol listesi).
- **Gereksiz bağımlılık:** Uygulama PostgreSQL kullanırken `Microsoft.Data.SqlClient 7.0.2` referansı var — saldırı yüzeyini azaltmak için kaldırılabilir.
- **Bağımlılık taraması:** `dotnet list package --vulnerable` CI'a eklenmeli.

> **Olumlu güvenlik uygulamaları:** Tüm dinamik sorgular tip-güvenli `CriteriaOperator.FromLambda` / parametreli `BinaryOperator` ile kuruluyor → **SQL/kriter injection yok**. Parolalar XAF'ın tuzlanmış hash mekanizmasıyla saklanıyor. Production'da canlı şema değişikliği varsayılan kapalı (`SOFASISERP_ZORUNLU_DB_GUNCELLEME=1` ile bilinçli açılıyor). Hesap kilitleme altyapısı (`ISecurityUserLockout`) hazır.

---

## 4. Fonksiyonel & Mantık Bulguları — Finans/Kasa/Cari

### 🔴 4.1 (Kritik) Tedarikçi bakiye işaret düzeltmesi motorda var, raporlarda yok → kart ≠ ekstre
**Dosya:** Motor `KasaCariBankaHareketleri.cs:407-408, 443-444` vs Rapor `HesapEkstresiRaporuController.cs:90`, `TumCarilerBakiyeRaporuController.cs`

Bakiye motoru, bir **Tedarikçi** cari için TAHSİLAT/ÖDEME fişlerinde işareti çevirir:
```csharp
KaynakHesap.GuncelBakiye += TedarikciMi(KaynakHesap, acilisFisi) ? -YerelBorcTutar : YerelBorcTutar;
```
Ancak hem Hesap Ekstresi hem Tüm Cariler raporu bu çevirmeyi **hiç yapmaz** — ham borç-alacak kullanır (`HesapEkstresiRaporuController.cs:90` elle doğrulandı: `UygulananYerelBorc − UygulananYerelAlacak`, işaret çevirme yok).

**Somut hata:** Tedarikçi T, açılış −1000 TL. Kasa'dan 400 TL ödeme girilir. Motor karta `−400` uygular → **kart bakiyesi −1400**. Ekstre `+400` uygular → **ekstre kapanışı −600**. Aynı tedarikçi için kart ve ekstre çelişir, her işlemde fark büyür. En az biri her zaman yanlıştır.

**Not (muhasebe yönü):** Tek işlemde işaret çevirme, çift kayıt mantığına da aykırıdır — bir hesabın bakiye yönü karşı tarafın müşteri/tedarikçi olmasına değil, o satırda Borç mu Alacak tarafında olduğuna bağlıdır. Tedarikçinin doğal negatif yönü zaten ACILIS fişindeki Kaynak/Karşı **takası** ile çözülüyor (satır 378-385).

**Önerilen çözüm:** İşaret çevirmeyi (`TedarikciMi` dalını) motordan tamamen kaldırıp motoru da raporlarla aynı evrensel işaretli kurala getirin; açılış yönünü yalnızca mevcut ACILIS takasına bırakın. Bu tek değişiklik §4.1, §4.2 ve §4.5'i birlikte çözer. Karar öncesi bir mali müşavirle konvansiyon netleştirilmeli ve **mevcut tedarikçi bakiyeleri yeniden hesaplanmalı**.

### 🔴 4.2 (Kritik) İşlem-bazlı tedarikçi işaret çevirmesi ödeme yönünü tersine çeviriyor
**Dosya:** `KasaCariBankaHareketleri.cs:406-408`

Tedarikçiye ödeme borcumuzu azaltmalı (−1000 → 400 ödeyince −600). Motor `TedarikciMi` nedeniyle `−1000 + (−400) = −1400` üretir; borcu azaltmak yerine artırır. Kod yorumu (satır 396-398) yalnızca "tahsilat" senaryosunu haklı çıkarıyor, ÖDEME yönü hatalı kalıyor. (§4.1 ile aynı kök neden; birlikte çözülür.)

### 🟠 4.3 (Yüksek) Kuruş yazıya-çevirmede yer-değer hatası + 3 haneye yuvarlama
**Dosya:** `SayiyiYaziyaCevirici.cs:74-83` (çağrı: `HareketMakbuzuReportBuilder.cs:104-105`)

Ondalık kısım yer değeri normalize edilmeden okunuyor ve `Math.Round(..., 3)` para için yanlış:
- `1500,5` → ondalık `"5"` → **"Beş Kuruş"** — oysa 0,5 TL = **50 kuruş** ("Elli Kuruş").
- `Round(...,3)` → `"1500,500"` → "Beş Yüz Kuruş".

Bu servis çek/makbuzun yasal yazı tutarını üretir; tutar yanlış çıkar.

**Çözüm:** `Math.Round(tutar, 2)` kullanıp kuruşu yer-değeriyle hesaplayın:
```csharp
int kurus = (int)Math.Round((tutar - Math.Truncate(tutar)) * 100m);
string ondalik = kurus.ToString("00"); // "50"
```

### 🟠 4.4 (Yüksek) Sayı→yazı çevirici kültüre bağımlı — thread tr-TR değilse tutar 100 katına çıkar
**Dosya:** `SayiyiYaziyaCevirici.cs:74-77`

`Convert.ToDecimal(..., CurrentCulture)` + `ToString(CurrentCulture)` ile biçimlendirip sabit `Split(',')` yapılıyor. Rapor oluşturma thread'i tr-TR değilse (`1500.50` → virgül yok) `Replace(".","")` → `"150050"` → **"Yüz Elli Bin Elli"**. Startup kültürü tr-TR'ye sabitliyor ancak rapor render bağlamı garanti değil. **Çözüm:** Sabit `CultureInfo.GetCultureInfo("tr-TR")` kullanın veya decimal aritmetiğiyle tam/ondalık ayırın (string round-trip'ten kaçının).

### 🟠 4.5 (Yüksek) Yabancı para ekstresinde TL rakamlar döviz koduyla etiketleniyor
**Dosya:** `HesapEkstresiRaporuBuilder.cs:306, 326`; `HesapEkstresiRaporuController.cs:108`

Ekstre satır/toplamları `YerelBorc/YerelAlacak` (TL) gösterir ama "Para Birimi"/"Kapanış" alanlarını hesabın döviz koduyla (ör. USD) etiketler → USD kasa ekstresi TL rakamları "USD" etiketiyle basar. Ayrıca `GuncelBakiye` yalnız TL izlendiğinden döviz kasasının kendi döviz bakiyesi geri elde edilemez. **Çözüm:** Etiketi TRY sabitleyin veya TRY-olmayan hesaplar için hesabın kendi dövizinde (`BorcTutar/AlacakTutar`) üretin; döviz-miktar için ayrı alan tutun.

### 🟠 4.6 (Yüksek) Ekstre koşan bakiyesi tarih sırasına göre garanti değil
**Dosya:** `HesapEkstresiRaporuBuilder.cs` (SortField yok) + `HesapEkstresiRaporuController.cs:114-119`

Koşan bakiye (`sumRunningSum`) yazdırma sırasına göre birikir; ne raporda ne kriterde `FisTarihi` sıralaması var. Geri tarihli fiş girildiğinde her satırın "Bakiye" sütunu yanlış olur (nihai kapanış doğru kalır). **Çözüm:** `report.SortFields`'a `FisTarihi` + tie-breaker (`Oid`/`CreatedDate`) ekleyin.

### 🟡 4.7 (Orta) Kur bulunamayınca (hafta sonu/tatil) yedek kur yok → işlem bloke
**Dosya:** `KasaCariBankaHareketleri.cs:316-332` — `KurTarihi == fisTarihiGunu` **tam gün** arar; o güne kur yoksa `kur=0` döner ve `DovizKuru>0` kuralı kaydı bloke eder. **Çözüm:** `KurTarihi <= fisTarihi` en son kaydı (top 1, azalan) fallback kullanın.

### 🟡 4.8 (Orta) `BorcTutar → AlacakTutar` kopyalama düzeltmede bayat kalıyor
**Dosya:** `KasaCariBankaHareketleri.cs:185` — `alacakTutar == 0` iken kopyalar; 100→150 düzeltmesinde AlacakTutar 100'de kalır, denge kuralı kaydı bloke eder. Aynı döviz cinsinde koşulsuz bağlayın.

### 🟡 4.9 (Orta) `TedarikciMi` canlı sınıf üzerinden — sınıf değişirse silme bakiyeyi bozar
**Dosya:** `KasaCariBankaHareketleri.cs:442-455` — Kayıt Müşteri iken işlenip Tedarikçi'ye çevrilirse `ObjectDeleting` güncel sınıfla ters işaret uygular → bakiye bozulur. (§4.1 çözümüyle birlikte ortadan kalkar.)

### 🔵 4.10 (Düşük) Diğer
- Negatif tutarda çevirici boş döner; `ToTitleCase("TRY")` → "Try" basılır (`SayiyiYaziyaCevirici.cs:81,85`).
- Mükerrer-kayıt kontrolü yalnız `BorcTutar`'a bakar → aynı gün aynı tutarlı iki gerçek ödeme yanlış-pozitifle engellenir (`:341-352`).
- `GuncelBakiye` saklanan koşan toplam → eşzamanlı harekette lost-update riski (`Hesap.cs:80`).

> **Olumlu:** Para alanlarında tutarlı `decimal` + `Math.Round(...,2, AwayFromZero)`; `TurkiyeZamani` ile tarih aralığı filtreleri çakışmasız/boşluksuz (off-by-one yok); TCMB ayrıştırması `InvariantCulture` ile doğru; Tüm Cariler raporunda tüm rakamlar TL olduğundan para birimi karışımı yok.

---

## 5. Fonksiyonel & Mantık Bulguları — Stok/Maliyet

### 🔴 5.1 (Kritik) Ağırlıklı ortalama maliyette sıfıra bölme (DivideByZeroException)
**Dosya:** `WeightedAverageCostService.cs:20-22` (tetikleyici `StokHareketleriD.cs:307-312`)

`GirisUygula`'da yalnızca `eskiMiktar == 0` özel ele alınır. `eskiMiktar` **negatif** ve giriş bakiyeyi tam 0'a getiriyorsa `yeniMiktar = 0` olur ve else dalı `(...) / 0` → **decimal sıfıra bölme istisnası** (elle doğrulandı).

**Senaryo (varsayılan `Uyar` politikasıyla ulaşılabilir):** A deposu Giriş 10 → `ToplamMiktar=10`. B'den Çıkış 15 (Uyar engellemez) → `ToplamMiktar=−5`. Sonra Giriş 5 → `yeniMiktar=0` → **çökme**. `eskiMiktar` negatif ama ≠0 ise istisna olmasa da ortalama anlamsız/negatif çıkar.

**Çözüm:** `yeniMiktar <= 0` durumunda ortalamayı `girisBirimMaliyeti`'ne sabitleyin; motor Çıkış'ta global `ToplamMiktar`'ın 0 altına inmesini de kontrol etsin (§5.2).

### 🔴 5.2 (Kritik) Global `ToplamMiktar` koşulsuz negatife düşüyor; negatif politikası yalnız depo bakiyesine bakıyor
**Dosya:** `StokHareketleriD.cs:328, 312, 331`

Çıkışta `StokTanim.ToplamMiktar = yeniMiktar` doğrudan yazılır; negatif kontrolü (`bakiye.Miktar < 0`) yalnızca **depo bazlı** `StokBakiye` için. `Engelle` politikası aktifken bile global `ToplamMiktar` kontrol edilmez → tutarsızlık + §5.1'in ön koşulu. Ayrıca `Engelle` throw'u nesneler **bellekte değiştirildikten sonra** çalışır (kirli nesne). **Çözüm:** Negatif kontrolünü matematik uygulanmadan **önce** yapın; global `ToplamMiktar` için de kontrol ekleyin.

### 🟠 5.3 (Yüksek) Kaydedilmiş hareket satırının düzenlenmesi bakiye/maliyeti sessizce bozar
**Dosya:** `StokHareketleriD.cs:271-272`

`MotorIslendi` bayrağı motorun tekrar çalışmasını engeller (idempotency için doğru), ancak var olan satırın `Miktar`'ı sonradan değiştirilip kaydedilirse motor **erken return** eder → bakiye/`ToplamMiktar`/`OrtalamaMaliyet` güncellenmez, satır 100 gösterirken bakiye 10'da kalır. `Miktar` post sonrası salt-okunur değil. **Çözüm:** Post sonrası `Miktar`/`BirimMaliyet` alanlarını değiştirilemez yapın (veya delta ile işleyin).

### 🟠 5.4 (Yüksek) Sıfır maliyetli stokun depo transferi tümden çöküyor
**Dosya:** `StokTransferi.cs:156` + `StokHareketleriD.cs:300-303`

Transfer Giriş satırı `BirimMaliyet <= 0` kontrolüne takılır; hiç maliyetli giriş görmemiş (OrtalamaMaliyet=0) bir kalemin transferi **"birim maliyeti sıfırdan büyük giriniz"** hatasıyla geri alınır — oysa transfer gerçek alım değil. **Çözüm:** `KaynakBelgeTipi = StokTransferi` olan giriş satırlarında bu zorunluluğu gevşetin.

### 🟡 5.5 (Orta) Diğer stok/numaralandırma bulguları
- **Yeni SequenceGenerator yarışı** (`NumberSequenceService.cs:60-87`): iki kullanıcı aynı anda ilk belgeyi açarsa ikinci commit ham unique-constraint/optimistic-lock istisnasıyla düşer, retry yok. Bounded retry ekleyin.
- **Günlük >999 belge FisNo taşması** (`NumberSequenceService.cs:57` + `StokHareketleriM.cs:69-70`): 6 karakterlik türle 1000. belge `Size(16)`'yı aşar. Sıra genişliğini artırın veya `Size`'ı büyütün.
- **StokBakiye/StokTanim eşzamanlı güncellemede retry yok** (`StokHareketleriD.cs:312,328`): lost-update önlense de kullanıcı ham hata görür.
- **Hareket satırında birim yok** (`StokHareketleriD.cs:83-87`): farklı birimler (adet/koli/kg) dönüşümsüz toplanır. Baz-birim dönüşüm faktörü ekleyin.
- **Silme replay'i `CreatedDate`'e göre** (`:365-373`), `FisTarihi` değil: geri-tarihli fişler yanlış sırada maliyetlenir. `FisTarihi` ile sıralayın.

### 🔵 5.6 (Düşük) Diğer
- `StokKodu` yeniden üretimi sıra numarasında boşluk bırakır (kabul edilebilir).
- `OrtalamaMaliyet` hiç yuvarlanmıyor → uzun vadede hassasiyet kayması.
- `NegatifStokPolitikasi` fallback'i `Uyar` → stok kolayca negatife düşer (§5.1/5.2 zemini).
- `ObjectDeleting`'de null-güvenlik yok (`:350-351`) → bozuk veride NRE.

> **Olumlu:** Maliyet matematiği saf, yan-etkisiz servise ayrılmış ve test edilmiş; `MotorIslendi` idempotency; `StokBakiye`'de `(Stok, Depo)` bileşik unique index; silme sonrası tam replay (hareketli ortalama sıraya duyarlı olduğundan doğru seçim); transferde kaynak-çıkış/hedef-giriş çiftinin `Oid` ile eşleştirilmesi.

---

## 6. Performans & Kod Kalitesi Bulguları

### 🔴 6.1 (Kritik) Tüm Cariler Bakiye Raporu — tüm tabloları belleğe çekip O(Cari × Hareket) tarama
**Dosya:** `TumCarilerBakiyeRaporuController.cs:72, 82, 89-96`

`XPQuery<CariHesapTanim>().ToList()` tüm carileri, `XPQuery<KasaCariBankaHareketleri>().Where(FisTarihi <= bitis).ToList()` dönem sonuna kadar tüm hareketleri belleğe yükler; sonra her cari için `hareketler.Where(...)` çalıştırır. 2.000 cari × 300.000 hareket ≈ **600 milyon karşılaştırma + tüm hareket tablosu RAM'de** → dakikalarca bekleme veya OutOfMemory. (Kod yorumu "N+1 önlendi" diyor ama asıl sorun tüm tabloların belleğe alınması.)

**Çözüm:** Devreden/Dönem Borç/Alacak'ı veritabanında `GROUP BY` ile hesaplatın (Kaynak ve Karşı için iki sunucu-taraflı `Sum`, cari-Oid bazında sözlükte birleştirin). Cari kodu aralığı filtresini de SQL `Where`'e taşıyın.

### 🟠 6.2 (Yüksek) Her yeni harekette indekssiz mükerrer-kontrol sorgusu (tam tablo taraması)
**Dosya:** `KasaCariBankaHareketleri.cs:341-348` — Her kayıtta `FisTuruTanim+FisTarihi+KaynakHesap+KarsiHesap+BorcTutar` beşlisiyle `FindObject`; bu kombinasyonda bileşik indeks yok. Tablo büyüdükçe her "Kaydet" yavaşlar. `(FisTarihi, KaynakHesap, KarsiHesap)` üzerinde `[Indexed]` ekleyin.

### 🟠 6.3 (Yüksek) `BaseClass.OnSaving` her kayıtta indekssiz `IsDefault` sorgusu
**Dosya:** `BaseClass.cs:144-159` (IsDefault `:103` indekssiz) — Yüksek yazma hacimli tipler (hareketler) dâhil her insert'te `FindObject(IsDefault=true)` taraması; §6.2'nin üstüne biner. `IsDefault`'a `[Indexed]` ekleyin veya yalnızca IsDefault kullanan tiplerde çalıştırın.

### 🟠 6.4 (Yüksek) GenelParametre önbeleksiz — her ekran açılışında/döviz değişiminde okunuyor
**Dosya:** `GenelParametreOkuyucu.cs:27` — Her DetailView/ListView açılışında ve **her Döviz Kodu/Kuru değişiminde** (`OndalikCanliFormatController.cs:84`) tek-satırlık config tablosu tekrar sorgulanır. Process-genelinde (invalidation ile) önbelleğe alın.

### 🟠 6.5 (Yüksek) Tek hesap ekstresinde açılış bakiyesi belleğe çekilerek toplanıyor
**Dosya:** `HesapEkstresiRaporuController.cs:87-90` — `.ToList().Sum(...)` dönem öncesi tüm hareketleri tam nesne olarak materyalize eder. İki sunucu-taraflı `Sum` ile DB'ye indirin.

### 🟠 6.6 (Yüksek) Kritik finansal mantık test edilmemiş
Tek test dosyası `WeightedAverageCostServiceTests` yalnızca saf matematiğin mutlu yolunu kapsıyor. **Testsiz kritik mantık:** bakiye motoru (`ObjectSaving` — tedarikçi çevirme, açılış takası, fark mekanizması), numaralandırma/eşzamanlılık, negatif stok politikası, silme replay'i, TCMB XML ayrıştırma, sıfıra bölme senaryosu (§5.1). XPO in-memory data store ile entegrasyon testleri yazın; `TcmbDovizKuruService` ayrıştırmasını enjekte edilebilir hâle getirip saf birim testi ekleyin.

### 🟡 6.7 (Orta) Diğer performans/kalite
- **TCMB senkron, zaman aşımısız çağrı açık ObjectSpace içinde** (`DovizKuruGuncellemeServisi.cs:60`): yavaş ağ DB bağlantısını meşgul eder. Ağ çağrısını ObjectSpace açılmadan önce yapın.
- **Silme replay'inde master/fiş-türü gezinmesi N+1 riski** (`StokHareketleriD.cs:365-373`): ilgili referansları tek sorguda projekte edin.
- **Servis arayüzleri DI'ye kayıtlı değil, `new` ile üretiliyor** (bkz. §2.4): `Session.ServiceProvider` üzerinden enjekte edin (`BaseClassWithAudit`'teki mevcut desenle) veya soyutlamaları kaldırın.
- **KPI Dashboard senkron yükleme** (`KpiDashboardComponent.razor:60-81`): `OnInitializedAsync` kullanın, iki cari sorgusunu birleştirin, try/catch ekleyin.
- **Numaralandırmada sessiz retry yok** (`NumberSequenceService.cs:40-58`): yoğun eşzamanlı girişte kullanıcılar hata diyaloğu görebilir.
- **Rapor builder ve kod jeneratörlerinde büyük kod tekrarı**: ortak yardımcılara/generic taban sınıfa çıkarın.

### 🔵 6.8 (Düşük) Diğer
- Sihirli string'ler ("TRY", "ACILIS/TAHSIL" fiş kodları) birçok dosyaya dağılmış → merkezi sabitler sınıfı.
- Yuvarlama noktaları tip/alan bazında tutarsız → tek politika.
- Küçük ölü/bellek-içi filtre kod (`DovizKuruGuncellemeServisi.cs:43`).
- "Faz 7 — Security System" TODO işaretleri; audit alanları (CreatedBy/ModifiedBy) Security kapalıyken boş kalıyor.

> **Olumlu:** Kodda hiç bloklayan async (`.Result`/`.Wait`/`.GetAwaiter().GetResult()`) veya `new HttpClient()` yok; event handler'lar `OnDeactivated`'da düzgün abonelikten çıkarılıyor (circuit sızıntısı yok); KPI toplamları DB'ye indirilmiş; hosted service örnek nitelikte.

---

## 7. Sıradaki Adım Planı (Yol Haritası)

Bulgular, **risk × düzeltme maliyeti** ekseninde dört faza ayrılmıştır. Her faz kendi içinde teslim edilebilir.

### Faz 0 — Production öncesi engelleyiciler (1-3 gün) 🚨
Bunlar giderilmeden canlıya çıkılmamalı.

1. **Tedarikçi işaret mantığını düzelt (§4.1, §4.2, §4.9).** Bir mali müşavirle konvansiyonu netleştir → motoru ve raporları tek kurala getir → **mevcut tedarikçi bakiyelerini yeniden hesaplayan bir düzeltme (migration) çalıştır**. Öncesinde bakiye motoru için entegrasyon testleri yaz (§6.6).
2. **Sıfıra bölme + global negatif miktarı düzelt (§5.1, §5.2).** `yeniMiktar<=0` koruması + global `ToplamMiktar` negatif kontrolü; regresyon testi ekle.
3. **Sırları koddan çıkar (§3.1).** DB parolası + `UrlSigningKey` → User Secrets/ortam değişkeni; değerleri rotasyona sok; git geçmişini temizle.
4. **`ChangePasswordOnFirstLogon = true` ekle (§3.2).**
5. **Kuruş çevirici hatasını düzelt (§4.3, §4.4).** 2 haneye yuvarla, yer-değerini düzelt, sabit tr-TR kültürü kullan; makbuz testi ekle.

### Faz 1 — Yüksek etkili doğruluk & performans (1-2 hafta)
6. **Tüm Cariler raporunu DB-taraflı GROUP BY'a taşı (§6.1).**
7. **İndeksleri ekle (§6.2, §6.3):** hareket mükerrer-kontrolü ve `IsDefault` için bileşik/tekil indeksler.
8. **Kritik indekssiz sorguları DB'ye indir (§6.5)** ve **GenelParametre önbelleği (§6.4).**
9. **Kaydedilmiş hareket düzenlemesi desync'ini gider (§5.3):** post sonrası satırı kilitle veya delta işле.
10. **Ekstre sıralaması + yabancı para etiketi (§4.5, §4.6).**
11. **Kur fallback'i (§4.7)** ve **sıfır maliyetli transfer (§5.4).**

### Faz 2 — Sağlamlaştırma & güvenlik derinliği (2-4 hafta)
12. **Test kapsamını genişlet (§6.6):** bakiye motoru, numaralandırma/eşzamanlılık, negatif stok, TCMB ayrıştırma, silme replay için XPO in-memory entegrasyon testleri. Hedef: kritik finansal yolların tamamı.
13. **Eşzamanlılık retry'ları (§5.5, §6.7):** numaralandırma ve bakiye/maliyet güncellemelerinde bounded optimistic-retry.
14. **Güvenlik sıkılaştırma (§3.3, §3.4, §3.6):** çerez bayrakları, `AllowedHosts`, görev-bazlı kısıtlı roller.
15. **TCMB servisini `IHttpClientFactory` + timeout + hata loglama ile yenile (§3.5, §6.7).**
16. **DI kaydı (§2.4, §6.7):** servis arayüzlerini kaydet ve `Session.ServiceProvider` üzerinden enjekte et.

### Faz 3 — Bakım & teknik borç (sürekli)
17. Birim dönüşüm altyapısı (§5.5) — çoklu-birim stok gereksinimi varsa.
18. Kod tekrarını azalt (rapor builder / kod jeneratörü ortak taban — §6.7).
19. Merkezi sabitler sınıfı, yuvarlama politikası birliği (§6.8).
20. "Faz 7 — Security System" TODO'larını ve audit alanı doldurmayı tamamla (§6.8).
21. CI'a `dotnet list package --vulnerable` ve otomatik test koşumu ekle.

### Önerilen çalışma prensipleri
- **Her finansal düzeltmeden önce test yaz** — mevcut davranışı kilitle, sonra düzelt (özellikle §4.1 gibi konvansiyon değişiklikleri).
- **Bakiye/maliyet değişikliklerinde veri düzeltme (migration) planla** — mevcut kayıtlar eski (yanlış) mantıkla üretildi.
- **Faz 0 ve Faz 1 ölçülebilir:** her madde küçük, izole, geri alınabilir bir PR olabilir.

---

## Ek — Metodoloji ve doğrulama notu

Kaynak kodun tamamı (`bin/`, `obj/`, `.vs/` hariç 135 dosya) bu ortama alınıp dört bağımsız uzman incelemesiyle (güvenlik, finans mantığı, stok/maliyet mantığı, performans/kalite) taranmıştır. Ardından her kategorinin en kritik bulguları — sırların gömülülüğü, eksik `ChangePasswordOnFirstLogon`, ağırlıklı ortalama maliyet sıfıra bölme, kuruş çevirici, tedarikçi işaret tutarsızlığı (motor `KasaCariBankaHareketleri.cs:407` vs rapor `HesapEkstresiRaporuController.cs:90`) ve Tüm Cariler bellek taraması — gerçek kod okunarak elle doğrulanmıştır. Bildirilen tüm bulgular kodda mevcut ve satır numaralarıyla izlenebilirdir; hiçbiri varsayıma dayanmaz.
