# SofasisERP — Düzeltme Görevleri (Uygulayıcı: Claude Code)

> **Bu dosya bir yapay zekâ kodlama ajanı için hazırlanmış görev listesidir.**
> Kaynak: `SofasisERP_Analiz_Raporu.md` (aynı klasörde) — 22 Ağustos 2026 tarihli kod denetimi.
> Tüm satır numaraları o günkü koda göredir; küçük kaymalar olabilir, kodu okuyarak doğrula.

---

## ÇALIŞMA KURALLARI (önce oku, sonra başla)

1. **Başlamadan önce:** `git status` temiz değilse mevcut değişiklikleri commit'le. Sonra `git checkout -b fix/denetim-duzeltmeleri` ile yeni branch aç.
2. **Her görev = ayrı commit.** Commit mesajı: `fix(G##): kısa açıklama` formatında (## = görev numarası).
3. **Her görevden sonra `dotnet build`** çalıştır; derleme hatası varsa düzeltmeden sonraki göreve geçme.
4. **Davranış değiştiren finansal görevlerde (G1, G2, G3) önce test yaz:** mevcut hatalı davranışı gösteren test → düzeltme → testin geçmesi. Testler `SofasisERP.Module.Tests` projesine.
5. **Kapsam dışı (YAPMA):** DI kayıt refaktörü, rapor builder / kod jeneratörü birleştirme, merkezi sabitler sınıfı, birim dönüşüm altyapısı, rol seed'leri. Bunlar ayrı bir iş olarak planlandı.
6. Mevcut kod stili korunacak: Türkçe alan/yorum adları, dosya başı açıklama blokları, mevcut `Math.Round(..., 2, MidpointRounding.AwayFromZero)` deseni.
7. Görev sırası önemlidir — sırayla uygula.

---

## FAZ 0 — Production engelleyicileri

### G1 — Tedarikçi bakiye işaret çevirmesini kaldır + bakiyeleri yeniden hesapla 🔴
**Dosya:** `src/SofasisERP.Module/BusinessObjects/Finans/KasaCariBankaHareketleri.cs`

**Sorun:** `ObjectSaving` (satır ~406-408) ve `ObjectDeleting` (satır ~442-444) tedarikçi cari için işareti çeviriyor (`TedarikciMi(...) ? -YerelBorcTutar : YerelBorcTutar`). Raporlar (`HesapEkstresiRaporuController.cs:90`, `TumCarilerBakiyeRaporuController.cs`) bu çevirmeyi yapmıyor → tedarikçi hesaplarında kart bakiyesi ile ekstre çelişiyor. Ayrıca çevirme muhasebe olarak da yanlış: tedarikçiye ödeme borcu azaltacağına artırıyor (−1000 iken 400 ödeme → −1400 oluyor, −600 olmalı).

**Yapılacak:**
1. `ObjectSaving` içindeki iki satırı evrensel işaretli kurala çevir:
   ```csharp
   KaynakHesap.GuncelBakiye += YerelBorcTutar;
   KarsiHesap.GuncelBakiye  -= YerelAlacakTutar;
   ```
2. `ObjectDeleting` içindeki geri alma da aynı kurala:
   ```csharp
   KaynakHesap.GuncelBakiye -= UygulananYerelBorcTutar;
   KarsiHesap.GuncelBakiye  += UygulananYerelAlacakTutar;
   ```
3. `TedarikciMi` statik metodunu (satır ~454-455) ve ona atıf yapan uzun yorum bloklarını sil; yerine kısa bir yorum: açılış yönü ACILIS fişindeki Kaynak/Karşı takasıyla çözülür (satır ~378-385 — **o takas KALIYOR, dokunma**), işlem bazında tip'e göre işaret çevirme muhasebe kuralına aykırıdır.
4. **Migration:** `src/SofasisERP.Module/DatabaseUpdate/Updater.cs` → `UpdateDatabaseAfterUpdateSchema()` içine tek seferlik, idempotent bakiye yeniden hesaplama ekle (mevcut backfill bloklarının desenini takip et):
   - Tüm `Hesap` nesneleri için `GuncelBakiye` = Σ(KaynakHesap==hesap → +UygulananYerelBorcTutar) + Σ(KarsiHesap==hesap → −UygulananYerelAlacakTutar), yalnızca `MotorIslendi == true` hareketler üzerinden.
   - İdempotentlik: hesaplanan değer mevcut değerden farklıysa yaz (her çalıştırmada aynı sonucu üretir, zarar vermez).
5. `ObjectDeleting`'deki `bool acilisFisi` ve `ObjectSaving`'deki `bool acilisFisi` değişkenleri artık kullanılmıyorsa temizle.

**Kabul kriteri (test yaz):** Saf mantık test edilemiyorsa en azından şu senaryoyu yorum + elle doğrulama ile belgele: tedarikçi açılış −1000, kasadan 400 ödeme → kart bakiyesi −600 VE ekstre kapanışı −600 (ikisi eşit). Silinince ikisi de −1000'e döner.

### G2 — Ağırlıklı ortalama maliyette sıfıra bölme koruması 🔴
**Dosya:** `src/SofasisERP.Module/Services/WeightedAverageCostService.cs` (satır 16-24)

**Sorun:** `GirisUygula`'da `eskiMiktar` negatif ve `yeniMiktar == 0` olursa `/0` → DivideByZeroException. `eskiMiktar` negatifken (ör. −10 + 5) ortalama anlamsız çıkıyor.

**Yapılacak:** Guard'ı genişlet:
```csharp
decimal yeniMiktar = eskiMiktar + girisMiktari;
decimal yeniOrtalama = (eskiMiktar <= 0 || yeniMiktar <= 0)
    ? girisBirimMaliyeti
    : ((eskiMiktar * eskiOrtalama) + (girisMiktari * girisBirimMaliyeti)) / yeniMiktar;
```
Mantık: stok negatif/sıfırdan girişle toparlanıyorsa geçmiş (bozuk) ortalama anlamını yitirmiştir; yeni girişin maliyeti taban alınır.

**Kabul kriteri:** `WeightedAverageCostServiceTests`'e ekle: `GirisUygula(-5, 20, 5, 30)` exception atmaz, ortalama 30 döner; `GirisUygula(-10, 20, 5, 30)` → miktar −5, ortalama 30; mevcut testler bozulmaz.

### G3 — Global ToplamMiktar negatif kontrolü + Engelle'yi mutasyondan önce çalıştır 🔴
**Dosya:** `src/SofasisERP.Module/BusinessObjects/Stok/StokHareketleriD.cs` (satır ~300-340, `ObjectSaving` motor bloğu)

**Sorun:** (a) Çıkışta `StokTanim.ToplamMiktar` koşulsuz negatife düşürülüyor; negatif stok politikası yalnızca depo bazlı `StokBakiye.Miktar`'a bakıyor. (b) `NegatifStokPolitikasi.Engelle` istisnası, nesneler bellekte değiştirildikten SONRA fırlatılıyor → aynı ObjectSpace'te kirli nesne kalıyor.

**Yapılacak:** Çıkış dalında sırayı değiştir:
1. Önce yeni değerleri **yerel değişkenlerde** hesapla (`yeniDepoBakiye`, `yeniToplamMiktar`).
2. Politika kontrolünü bu yerel değerlerle yap — hem depo bakiyesi hem global `ToplamMiktar` için: `Engelle` ise ikisinden biri < 0 olduğunda hiçbir nesneye yazmadan `UserFriendlyException` fırlat; `Uyar` ise mevcut uyarı davranışını koru.
3. Kontrolden geçerse nesnelere yaz.

**Kabul kriteri:** `Engelle` politikasında depo bakiyesi yeterli ama global toplam negatife düşecekse kayıt engellenir; engellenen kayıtta `StokTanim.ToplamMiktar` ve `StokBakiye.Miktar` değişmemiş kalır.

### G4 — `ChangePasswordOnFirstLogon = true` ekle 🟠
**Dosya:** `src/SofasisERP.Module/DatabaseUpdate/Updater.cs` — `SeedAdminKullanicisi()` (satır ~106-133)

**Sorun:** Yorumlar (satır 84-86, 104-105) bu özelliğin uygulandığını söylüyor ama atama kodda YOK.

**Yapılacak:** Yeni kullanıcı oluşturulan dalda, `SetPassword(...)` çağrılarından sonra (hem dev hem prod yolu için, if/else dışında):
```csharp
userAdmin.ChangePasswordOnFirstLogon = true;
```
Yorumları koda uygun hâle getir.

### G5 — SayiyiYaziyaCevirici'yi decimal aritmetiğiyle yeniden yaz 🟠
**Dosya:** `src/SofasisERP.Module/Services/SayiyiYaziyaCevirici.cs`

**Sorunlar:** (a) `Math.Round(...,3)` — para 2 hane olmalı. (b) Ondalık kısım yer-değersiz okunuyor: `1500,5` → "Beş Kuruş" (doğrusu 50 kuruş = "Elli Kuruş"). (c) `CurrentCulture` + sabit `Split(',')` — kültür tr-TR değilse `1500.50` → "Yüz Elli Bin Elli". (d) Negatif tutar `catch` ile boş string dönüyor. (e) `ToTitleCase("TRY")` → "Try" basılıyor.

**Yapılacak:** `SayiyiYaziyaCevirVirgullu`'yu string round-trip olmadan yeniden yaz:
```csharp
decimal deger = Math.Round(Convert.ToDecimal(tutar, CultureInfo.CurrentCulture), 2, MidpointRounding.AwayFromZero);
bool negatif = deger < 0; deger = Math.Abs(deger);
long tamKisim = (long)Math.Truncate(deger);
int kurus = (int)Math.Round((deger - tamKisim) * 100m);
```
- `SayiyiYaziyaCevir(tamKisim.ToString(CultureInfo.InvariantCulture))` ile tam kısmı, `kurus > 0` ise `SayiyiYaziyaCevir(kurus.ToString(...))` ile kuruşu yazıya çevir.
- Negatifse sonucun başına `"Eksi "` ekle.
- Para birimi görünen adı: `"TRY" → "Türk Lirası"`, `"USD" → "Amerikan Doları"`, `"EUR" → "Euro"`, diğerleri → kodun kendisi (ToTitleCase KULLANMA).
- Girdi string ayrıştırması başarısızsa (mevcut davranış) boş string dönmeye devam et; ama negatif artık istisna DEĞİL, normal yol.
- Metodun mevcut imzasını koru (çağıran: `HareketMakbuzuReportBuilder.cs:104-105`).

**Kabul kriteri (yeni test dosyası `SayiyiYaziyaCeviriciTests.cs`):** `1500,50` → "Bin Beş Yüz Türk Lirası Elli Kuruş"; `1500,5` girdisi de aynı sonucu verir; `0,05` → kuruş "Beş Kuruş"; `1000` → "Bin Türk Lirası"; `-250,75` → "Eksi İki Yüz Elli Türk Lirası Yetmiş Beş Kuruş"; `123456,78` doğru; kültür Invariant iken tr-TR biçimli girdiyle sonuç değişmez.

### G6 — Sırları koddan çıkarma hazırlığı 🔴 (kod kısmı)
**Dosyalar:** `src/SofasisERP.Blazor.Server/SofasisERP.Blazor.Server.csproj`, `appsettings.json`, repo kökü `README.md`

**Yapılacak:**
1. csproj'a `<UserSecretsId>sofasiserp-blazor-server</UserSecretsId>` ekle.
2. `appsettings.json`'daki `UrlSigningKey` değerini kaldır; anahtar `DevExpress:ExpressApp:Security:UrlSigningKey` yolundan ortam değişkeni/user-secrets ile gelecek. Development kolaylığı için `appsettings.Development.json`'a YENİ rastgele bir GUID koy (eski `87A0CCC3-...` değerini KULLANMA — sızmış kabul ediliyor).
3. ConnectionString localhost dev değeri şimdilik kalabilir (yalnız yerel), ancak `README.md`'ye "Production yapılandırması" bölümü ekle: `ConnectionStrings__ConnectionString`, `DevExpress__ExpressApp__Security__UrlSigningKey`, `SOFASIS_ADMIN_INITIAL_PASSWORD`, `AllowedHosts` ortam değişkenleri zorunlu; `deploy-vps.ps1` bunlarla uyumlu olmalı (kontrol et, gerekiyorsa parametre ekle).

> **İNSAN AKSİYONU (Sedat):** VPS'te DB parolasını ve UrlSigningKey'i fiilen değiştir/rotasyona sok; git geçmişi temizliği (BFG/filter-repo) ayrıca değerlendirilecek. Bu adımları KODLA yapma, sadece README'de belgele.

### G7 — Kimlik çerezi bayrakları 🟠
**Dosya:** `src/SofasisERP.Blazor.Server/Startup.cs` (satır ~126-129)

**Yapılacak:**
```csharp
services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => {
        options.LoginPath = "/LoginPage";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
```

---

## FAZ 1 — Doğruluk & performans

### G8 — Tüm Cariler raporunu DB-taraflı toplamaya çevir 🔴
**Dosya:** `src/SofasisERP.Module/Controllers/Process/TumCarilerBakiyeRaporuController.cs` (satır ~72-96)

**Sorun:** Tüm cariler + dönem sonuna kadar TÜM hareketler `.ToList()` ile belleğe alınıp her cari için bellek-içi `Where` çalıştırılıyor → O(Cari × Hareket), gerçek veride OutOfMemory.

**Yapılacak:** Bellek-içi taramayı kaldır; hareketleri veritabanında grupla. XPO ile önerilen desen (LINQ `GroupBy`+`Sum` XPO'da güvenilir çevrilmeyebilir — bunun yerine `Session.SelectData` kullan):
- Kaynak tarafı: `SelectData(typeof(KasaCariBankaHareketleri))`, criteria: `MotorIslendi && FisTarihi < baslangic` (devreden) / `FisTarihi >= baslangic && <= bitis` (dönem), properties: `Sum(UygulananYerelBorcTutar)`, groupProperties: `KaynakHesap.Oid` → `Dictionary<Guid, decimal>`.
- Karşı tarafı: aynısı `KarsiHesap.Oid` + `Sum(UygulananYerelAlacakTutar)`.
- Cari listesi: cari kodu aralığı filtresini de SQL kriterine taşı (`CariHesapKodu >= x && <= y` benzeri, mevcut bellek-içi filtre neyse onun kriter karşılığı).
- Sonra cari başına sözlüklerden: `devreden = borcSozluk_öncesi[cari] − alacakSozluk_öncesi[cari]`, dönem borç/alacak aynı şekilde. Rapor satırlarını (CariBakiyeSatiri) mevcut alan adlarıyla aynen doldur — çıktı birebir aynı kalmalı.

**Kabul kriteri:** Küçük veriyle mevcut çıktıyla birebir aynı sonuç; hiçbir yerde hareket listesi `.ToList()` ile materialize edilmiyor.

### G9 — Ekstre açılış bakiyesini sunucu tarafında topla 🟠
**Dosya:** `src/SofasisERP.Module/Controllers/Process/HesapEkstresiRaporuController.cs` (satır ~87-90)

**Yapılacak:** `.ToList().Sum(...)` yerine iki sunucu-taraflı toplam:
```csharp
var sorgu = new XPQuery<KasaCariBankaHareketleri>(session);
decimal borc   = sorgu.Where(x => x.KaynakHesap == hesap && x.FisTarihi < baslangic && x.MotorIslendi)
                      .Sum(x => (decimal?)x.UygulananYerelBorcTutar) ?? 0m;
decimal alacak = sorgu.Where(x => x.KarsiHesap == hesap && x.FisTarihi < baslangic && x.MotorIslendi)
                      .Sum(x => (decimal?)x.UygulananYerelAlacakTutar) ?? 0m;
decimal acilisBakiyesi = borc - alacak;
```
(`(decimal?)` + `?? 0m`: boş kümede Sum'ın istisna atmaması için.)

### G10 — Eksik indeksler 🟠
**Dosyalar:** `src/SofasisERP.Module/BusinessObjects/Base/BaseClass.cs`, `Finans/KasaCariBankaHareketleri.cs`

**Yapılacak:**
1. `BaseClass.IsDefault` persistent alanına `[Indexed]` ekle (her kayıtta `FindObject(IsDefault=true)` sorgusu var — `OnSaving` satır ~144-159).
2. `KasaCariBankaHareketleri.FisTarihi` alanına `[Indexed("KaynakHesap;KarsiHesap")]` ekle (mükerrer kontrol + rapor sorguları için bileşik indeks).
3. Ek iyileştirme: `BaseClass.OnSaving`'deki IsDefault sorgusunu yalnızca nesnenin `IsDefault` değeri `true` olduğunda çalıştır (false olan kayıt başka kaydın varsayılanlığını etkileyemez — kontrol et: mevcut mantık 'başka varsayılan varsa kaldır' ise bu kısaltma güvenlidir; değilse dokunma).

### G11 — GenelParametre önbelleği 🟠
**Dosya:** `src/SofasisERP.Module/BusinessObjects/Helper/GenelParametreOkuyucu.cs` (+ `GenelTanimlar/GenelParametre.cs`)

**Yapılacak:** Okunan değerleri process-genelinde statik, thread-safe önbelleğe al (`static volatile` snapshot veya `Lazy` + invalidation). `GenelParametre.OnSaving` (ve `StokParametre` benzer kullanılıyorsa o da) sonunda önbelleği geçersiz kıl (`GenelParametreOkuyucu.OnbellegiTemizle()`). Önbellek boşken ilk okuma mevcut sorguyu yapar.

**Kabul kriteri:** Parametre ekranında değişiklik kaydedilince yeni değer bir sonraki okumada görünür; art arda okumalar DB'ye gitmez.

### G12 — İşlenmiş stok satırında Miktar/Maliyet düzenlemesini engelle 🟠
**Dosya:** `src/SofasisERP.Module/BusinessObjects/Stok/StokHareketleriD.cs`

**Sorun:** `MotorIslendi=true` satır tekrar kaydedilirse motor erken return eder; kullanıcı `Miktar`'ı değiştirirse bakiye/maliyet sessizce bayat kalır.

**Yapılacak:**
1. Persistent `UygulananMiktar` (decimal) alanı ekle; motor bloğunun sonunda (`MotorIslendi = true` yapılan yerde) `UygulananMiktar = Miktar;` ata.
2. `ObjectSaving`'de erken-return dalına koruma ekle: `MotorIslendi && Miktar != UygulananMiktar` → `UserFriendlyException("İşlenmiş stok hareketi satırında miktar değiştirilemez. Satırı silip yeniden giriniz.")`. `BirimMaliyet` için de aynı yaklaşım (ikinci alan `UygulananBirimMaliyet` veya tek istisna mesajı altında birleşik kontrol — tercih et, tutarlı ol).
3. UI tarafı: sınıfa `[Appearance]` kuralı ekle — `MotorIslendi = true` iken `Miktar`, `BirimMaliyet`, `StokTanim` alanları `Enabled = false` (mevcut Appearance kullanımlarını örnek al, `ConditionalAppearance` modülü zaten yüklü).
4. `Updater.cs`'e backfill: mevcut `MotorIslendi=true` satırlarda `UygulananMiktar == 0 && Miktar != 0` ise `UygulananMiktar = Miktar` (mevcut Uygulanan* backfill deseniyle aynı).

### G13 — Ekstre koşan bakiyesini tarihe göre sırala 🟠
**Dosya:** `src/SofasisERP.Module/Reports/HesapEkstresiRaporuBuilder.cs`

**Yapılacak:** Detay bandına sıralama ekle: önce `FisTarihi` artan, tie-breaker `CreatedDate` (yoksa `Oid`). XtraReport'ta: `detailBand.SortFields.Add(new GroupField("FisTarihi", XRColumnSortOrder.Ascending)); detailBand.SortFields.Add(new GroupField("CreatedDate", XRColumnSortOrder.Ascending));`

### G14 — Yabancı para ekstresi etiketini düzelt 🟠
**Dosyalar:** `Reports/HesapEkstresiRaporuBuilder.cs` (satır ~306, ~326), `Controllers/Process/HesapEkstresiRaporuController.cs` (satır ~108)

**Sorun:** Rakamlar TL (`YerelBorc/YerelAlacak`) ama başlık/kapanış hesabın döviz koduyla (USD vb.) etiketleniyor.

**Yapılacak:** Minimal doğru çözüm: `DovizKodu` parametresine hesabın kodu yerine sabit `"TRY"` gönder (satır 108) — rapor zaten tamamen TL. Builder'da bu parametrenin kullanıldığı etiketleri kontrol et; "Para Birimi: TRY" tutarlı görünsün. Kod yorumuna not düş: hesabın kendi dövizinde ekstre ayrı bir geliştirme.

### G15 — Döviz kuru fallback'i (hafta sonu/tatil) 🟡
**Dosya:** `src/SofasisERP.Module/BusinessObjects/Finans/KasaCariBankaHareketleri.cs` — `DovizKuruGuncelle` (satır ~316-332)

**Yapılacak:** Tam gün eşleşmesi bulunamazsa `KurTarihi <= fisTarihiGunu` olan en yeni kaydı kullan:
```csharp
var kurKaydi = new XPQuery<DovizGunlukKurD>(Session)
    .Where(x => x.DovizGunlukKurM.KurTarihi <= fisTarihiGunu && x.DovizTanim == seciliDoviz)
    .OrderByDescending(x => x.DovizGunlukKurM.KurTarihi)
    .FirstOrDefault();
```
(Gerçek nesne modeline göre uyarla — `DovizGunlukKurM/D` ilişkisini oku.) Hâlâ yoksa mevcut davranış (0 → validasyon engeli) kalsın. `StokHareketleriD`'de benzer tam-gün kur araması varsa (satır ~250-260 civarı, `TRY` kontrolü yakınında) aynı fallback'i oraya da uygula.

### G16 — Sıfır maliyetli stok transferine izin ver 🟠
**Dosyalar:** `Stok/StokTransferi.cs` (satır ~156), `Stok/StokHareketleriD.cs` (satır ~300-303)

**Yapılacak:** `BirimMaliyet > 0` zorunluluğunu transfer kaynaklı giriş satırları için gevşet: satırın transferden geldiğini gösteren mevcut alanı kullan (`KaynakBelgeTipi == ...StokTransferi` — gerçek enum/alan adını koddan doğrula). Koşul: `if (girisMi && BirimMaliyet <= 0 && KaynakBelgeTipi != KaynakBelgeTipi.StokTransferi) throw ...`. Transferde maliyet 0 ise motor 0 maliyetle işlesin (ortalama G2 guard'ı sayesinde güvenli).

### G17 — Aynı dövizde Borç→Alacak tutarını koşulsuz eşitle 🟡
**Dosya:** `Finans/KasaCariBankaHareketleri.cs` — `BorcTutar` setter (satır ~185)

**Sorun:** `alacakTutar == 0` koşulu, tutar düzeltmelerinde (100→150) AlacakTutar'ı bayat bırakıyor; denge kuralı kaydı bloke ediyor.

**Yapılacak:** Kaynak ve karşı döviz cinsi aynıysa kopyalamayı koşulsuz yap: `if (!IsLoading && value > 0 && KaynakDovizAyniMi()) AlacakTutar = value;` (döviz eşitliği kontrolünü nesne modelinden doğru alanlarla kur; farklı dövizlerde mevcut davranışı koru). Kenar durum: kullanıcı Alacak'ı bilinçli farklı girdiyse ve dövizler aynıysa denge kuralı zaten eşitliği şart koşuyor — kopyalama bu yüzden güvenli.

---

## FAZ 2 — Sağlamlaştırma (kolay kazanımlar)

### G18 — NumberSequenceService'e sınırlı retry 🟡
**Dosya:** `src/SofasisERP.Module/Services/NumberSequenceService.cs`

**Yapılacak:** Generator oluşturma/artırma çevresine en fazla 3 denemelik retry sar: `DevExpress.Xpo.Exceptions.LockingException` ve unique-constraint ihlali (Postgres `23505` — `ConstraintViolationException` / provider istisnasını yakala) durumlarında nesneyi yeniden yükleyip (`Session.Reload` veya yeniden `FindObject`) tekrar dene. 3. denemede orijinal istisnayı fırlat. Dosya başındaki "bilinçli sınır" yorumunu güncelle.

### G19 — FisNo taşması: sıra 4 hane + alan genişletme 🟡
**Dosyalar:** `Services/NumberSequenceService.cs` (satır ~57, `:D3`), `Stok/StokHareketleriM.cs` (satır ~69-70) ve `Finans/KasaCariBankaHareketleri.cs`'de FisNo `Size` attribute'ları

**Yapılacak:** Format `D3` → `D4`; `FisNo` alanlarının `[Size(16)]` → `[Size(20)]`. Mevcut kayıtlar etkilenmez (eski format sıralamada sorun çıkarmaz çünkü tarih bölümü sabit genişlik; yine de commit mesajında not düş).

### G20 — TCMB servisini HttpClient + timeout'a taşı 🟡
**Dosyalar:** `Services/TcmbDovizKuruService.cs`, `Services/DovizKuruGuncellemeServisi.cs`, `Blazor.Server/Startup.cs`

**Yapılacak:**
1. `TcmbDovizKuruService`'e `HttpClient` enjekte et; `Startup.ConfigureServices`'e `services.AddHttpClient<IDovizKuruService, TcmbDovizKuruService>(c => c.Timeout = TimeSpan.FromSeconds(15));` ekle (mevcut `AddScoped<IDovizKuruService, TcmbDovizKuruService>` kaydını bununla DEĞİŞTİR).
2. `XDocument.Load(url)` → `var xml = await httpClient.GetStringAsync(url)` + `XDocument.Parse(xml)`. Metod senkronsa imzayı async'e çevir ve çağıran zinciri (`DovizKuruGuncellemeServisi`, `DovizKuruGuncellemeWorker`) uyumla — worker zaten async, `.Result` KULLANMA.
3. `catch (Exception)` sessiz yutmayı bırak: istisnayı logla (worker'daki mevcut loglama desenini kullan) ve boş liste dön.
4. `DovizKuruGuncellemeServisi`'nde mümkünse ağ çağrısını ObjectSpace açılmadan önce yap (kur listesini çek → sonra kısa ömürlü ObjectSpace ile yaz). Yapı buna izin vermiyorsa dokunma, yorumla belgele.

### G21 — Silme replay sıralamasına tie-breaker 🔵
**Dosya:** `Stok/StokHareketleriD.cs` (satır ~365-373)

**Yapılacak:** Minimal ve güvenli değişiklik: `OrderBy(x => x.CreatedDate)` → `OrderBy(x => x.CreatedDate).ThenBy(x => x.Oid)` (aynı saniye belirsizliğini giderir). **`FisTarihi`'ne geçme** — geriye-tarihli fişlerde tam kronolojik yeniden maliyetlendirme ayrı bir ürün kararı (canlı işleme sırası ile replay sırasının tutarlılığı bozulur); kodda yorumla belgele.

### G22 — KPI Dashboard async + hata yönetimi 🟡
**Dosya:** `src/SofasisERP.Blazor.Server/Editors/KpiDashboardComponent.razor` (satır ~60-81)

**Yapılacak:** `OnInitialized` → `OnInitializedAsync`; veri yüklemeyi try/catch'e al (hata durumunda kart yerine kısa hata metni + logla); `MusterilerdenAlacak`/`TedarikcilereBorc` iki ayrı sorguysa tek sorguya indirmek pratikse birleştir, değilse bırak.

### G23 — Testleri genişlet 🟠
**Proje:** `SofasisERP.Module.Tests`

**Yapılacak (saf mantık, XPO'suz test edilebilenler):**
- `SayiyiYaziyaCeviriciTests` (G5'te yazıldı — kapsamı kontrol et).
- `WeightedAverageCostServiceTests`'e: G2 senaryoları + `YenidenHesapla` ile karışık giriş/çıkış (çıkışla sıfıra inip yeni girişle ortalamanın resetlenmesi; negatife düşüp toparlanma).
- `TcmbDovizKuruService` ayrıştırmasını test edilebilir yap: XML string alan bir iç metodu (`KurlariAyristir(XDocument)` gibi) public/internal yapıp örnek TCMB XML'iyle test et (`InternalsVisibleTo` gerekirse ekle).
- Bakiye motoru/numaralandırma entegrasyon testleri (XPO in-memory) İSTEĞE BAĞLI — kur ve süre maliyetli; bu turda zorunlu değil, altyapı kurabiliyorsan G1 senaryosunu otomatikleştir.

---

## BİTİRİNCE

1. `dotnet build` + `dotnet test` temiz olmalı; sonucu özetle.
2. Yapamadığın / atladığın görev varsa nedenini yaz.
3. Tüm commit'ler `fix/denetim-duzeltmeleri` branch'inde kalsın — **main'e merge ETME**, gözden geçirme (Cowork'taki Claude + Sedat) sonrası birleştirilecek.
4. Değişen dosyaların listesini ve her görev için 1 satırlık özeti içeren `DUZELTME_SONUC.md` dosyası oluştur.

## İNSAN AKSİYONLARI (Sedat — kod dışı)
- [ ] PostgreSQL `sofasiserp_app` parolasını değiştir (VPS + yerel).
- [ ] Yeni `UrlSigningKey` üret, production ortam değişkenine koy.
- [ ] `SOFASIS_ADMIN_INITIAL_PASSWORD`'ü deploy ortamında ayarla; ilk girişten sonra Admin parolasını değiştir.
- [ ] Production `AllowedHosts`'u gerçek alan adıyla sınırla.
- [ ] Git geçmişindeki sırlar için BFG/filter-repo temizliği değerlendir.
- [ ] G1 sonrası: tedarikçi bakiyelerinin yeni değerlerini mali müşavirle/kayıtlarla karşılaştırarak doğrula.
