# SofasisERP — Düzeltme Sonuçları

**Branch:** `fix/denetim-duzeltmeleri` (master'dan ayrıldı, **merge edilmedi** — gözden geçirme bekliyor)
**Kaynak:** `DUZELTME_GOREVLERI.md` (aynı klasörde) — 22 Ağustos 2026 tarihli `SofasisERP_Analiz_Raporu.md` denetiminden üretilmiş görev listesi.
**Sonuç:** `dotnet build` (tüm solution) → **0 Hata, 0 Uyarı**. `dotnet test` → **24/24 test başarılı**. Ayrıca G1, G8, G9, G13, G14, G16, G17, G22 canlı dev veritabanına karşı Playwright ile uçtan uca doğrulandı (aşağıda "Canlı doğrulama" bölümüne bakınız).

---

## Faz 0 — Production engelleyicileri

| # | Özet | Commit |
|---|---|---|
| G1 | Tedarikçi bakiye işaret çevirmesi motordan kaldırıldı; tüm Hesap türleri evrensel işaretli kural kullanıyor. Updater'a tek seferlik `GuncelBakiye` yeniden hesaplama (idempotent) eklendi. | `f423671` |
| G2 | Ağırlıklı ortalama maliyette sıfıra bölme koruması: `eskiMiktar<=0 \|\| yeniMiktar<=0` guard'ı. Önce hatayı gösteren 2 test yazıldı, sonra düzeltildi. | `e88b459` |
| G3 | Global `ToplamMiktar` negatif kontrolü + `Engelle` politikası artık mutasyondan ÖNCE çalışıyor (yerel değişkenlerde hesaplanıp kontrol edilip sonra yazılıyor). | `1bd62ca` |
| G4 | `ChangePasswordOnFirstLogon = true` fiilen eklendi (önceden yalnızca yorumda vardı). | `0c4149c` |
| G5 | `SayiyiYaziyaCevirici` decimal aritmetiğiyle yeniden yazıldı: 2 hane yuvarlama, yer-değeri doğru kuruş hesabı, negatif tutar artık istisna değil "Eksi ..." öneki. 8 yeni test. | `75047a1` |
| G6 | `UrlSigningKey` appsettings.json'dan çıkarıldı (sızmış kabul edilen eski GUID bir daha kullanılmadı); `UserSecretsId` eklendi; README'ye "Production Yapılandırması" bölümü. | `b04ee38` |
| G7 | Kimlik doğrulama çerezi: `SecurePolicy=Always`, `HttpOnly=true`, `SameSite=Lax`, `SlidingExpiration` + 8 saatlik `ExpireTimeSpan`. | `13df8d9` |

## Faz 1 — Doğruluk & performans

| # | Özet | Commit |
|---|---|---|
| G8 | Tüm Cariler raporu: tam tablo `.ToList()` yerine 4 sunucu-taraflı GROUP BY sorgusu (Kaynak/Karşı × öncesi/dönem). **Canlı doğrulandı** — rapor sayıları liste görünümüyle birebir eşleşti. | `edf537e` |
| G9 | Ekstre açılış bakiyesi: `.ToList().Sum(...)` yerine iki sunucu-taraflı `Sum`. | `410e496` |
| G10 | `BaseClass.IsDefault`'a `[Indexed]`; `KasaCariBankaHareketleri.FisTarihi`'ne `[Indexed("KaynakHesap;KarsiHesap")]` bileşik indeks. (`IsDefault` sorgusunun zaten yalnızca `IsDefault=true` iken çalıştığı doğrulandı — ek kısaltma gerekmedi.) | `410e496` |
| G11 | `GenelParametreOkuyucu` process-genelinde kilitli statik önbelleğe alındı; `GenelParametre.OnSaved()` önbelleği geçersiz kılıyor. | `624e7a1` |
| G12 | İşlenmiş (`MotorIslendi=true`) stok satırında Miktar/BirimMaliyet değişikliği artık engelleniyor (`UygulananMiktar`/`UygulananBirimMaliyet` + Appearance kilidi + backfill). | `d58c8f1` |
| G13 | Ekstre koşan bakiyesi artık `FisTarihi` (artan) + `CreatedDate` tie-breaker ile sıralanıyor. **Canlı doğrulandı.** | `b9ab2a2` |
| G14 | Yabancı para ekstresi etiketi sabit "TRY" (rapor rakamları zaten hep TL). **Canlı doğrulandı.** | `b9ab2a2` |
| G15 | Döviz kuru bulunamayınca (hafta sonu/tatil) `KurTarihi <= fisTarihi` en son kur fallback — hem Kasa/Cari/Banka hem Stok hareketlerinde. | `1428ba9` |
| G16 | `KaynakBelgeOid` dolu (ör. StokTransferi) Giriş satırlarında BirimMaliyet>0 zorunluluğu kaldırıldı. **Canlı doğrulandı** — iki depo arası sıfır maliyetli transfer hatasız kaydedildi, StokBakiye doğru güncellendi. | `434fef7` |
| G17 | `BorcTutar` setter'ında aynı döviz cinsinde Borç→Alacak kopyalaması artık koşulsuz (tutar düzeltmelerinde bayat kalmıyor). | `434fef7` |

## Faz 2 — Sağlamlaştırma

| # | Özet | Commit |
|---|---|---|
| G18 | **ATLANDI — mimari sınırlama.** Aşağıda ayrıntılı açıklama var. |  |
| G19 | FisNo taşması: sıra `D3`→`D4`; `StokHareketleriM.FisNo` `Size(16)`→`Size(20)`. | `725ebf5` |
| G20 | TCMB servisi `HttpClient` + 15sn timeout + hata loglama; tüm zincir (`IDovizKuruService`→`IDovizKuruGuncellemeServisi`→Worker) async'e çevrildi, hiçbir yerde `.Result` yok. | `8e8d074` |
| G21 | Silme replay sıralamasına `ThenBy(Oid)` tie-breaker eklendi (`FisTarihi`'ne bilerek geçilmedi). | `5cd97b4` |
| G22 | KPI Dashboard `OnInitializedAsync` + try/catch (hata durumunda kart yerine kısa mesaj + log). **Canlı doğrulandı.** | `5cd97b4` |
| G23 | Test kapsamı genişletildi: WeightedAverageCost için 2 yeni `YenidenHesapla` senaryosu; TCMB XML ayrıştırma (`XmlAyristir`) HTTP çağrısından ayrılıp saf/test edilebilir hale getirildi, 4 yeni test. | `f23001b` |

---

## Yapılamayan / atlanan görev: G18

**Görev:** "NumberSequenceService'e sınırlı retry (LockingException/unique-constraint çakışmasında)."

**Neden atlandı:** `NumberSequenceService`'in kendi dosya başı yorumu (bu oturumdan önce yazılmış) mimariyi şöyle açıklıyor: numara üretimi **çağıranla aynı Session/transaction'da** yapılır ve bilerek COMMIT EDİLMEZ — asıl commit, numarayı isteyen iş nesnesinin (ör. `KasaCariBankaHareketleri`) kendi `OnSaving`'ini tetikleyen DIŞARIDAKİ `Session.CommitChanges()` çağrısında olur. Bu, gerçek "boşluksuz" numaralandırmanın ön koşulu: belge kaydı rollback olursa üretilen numara da geri alınır.

Bunun sonucu: `SonrakiNumara`/`SonrakiSiraNo` metotları HİÇBİR ZAMAN kendi başlarına bir `OptimisticLockingException` görmez — bu istisna yalnızca dışarıdaki commit sırasında, bu metotların çoktan dönmüş olduğu bir noktada fırlar. Bu metotların içine bir retry sarmak (a) hiçbir zaman tetiklenmeyecek bir catch bloğu eklemek olurdu (no-op), (b) gerçek anlamda bir retry için numaralandırma mantığının **kendi ayrı transaction'ına** taşınması gerekirdi — ki bu, tam olarak dosyanın kendi yorumunun "bilinçli kabul edilen sınır" dediği ve NumberSequenceService'in şu anki "aynı transaction, gerçek boşluksuz numara" tasarımını terk etmek anlamına gelir.

Bu, görev listesinin öngörmediği bir mimari çelişkiydi; kod olarak yanlış/no-op bir "düzeltme" eklemek yerine görev atlandı ve gerekçesi burada belgelendi. Gerçek bir çözüm istenirse, ayrı bir mimari karar (ör. numaralandırmayı ayrı bir servis çağrısına taşıyıp retry'yi ORADA yapmak, boşluksuzluk garantisinden ödün vermek) gerekir — bu oturumun kapsamı dışında.

---

## Kapsam dışı bırakılanlar (DUZELTME_GOREVLERI.md §"YAPMA" kuralı gereği)

DI kayıt refaktörü, rapor builder/kod jeneratörü birleştirme, merkezi sabitler sınıfı, birim dönüşüm altyapısı, rol seed'leri — hiçbirine dokunulmadı.

## Küçük sapmalar (gerekçeli)

- **G8:** Cari kodu aralığı filtresi SQL kriterine TAŞINMADI (görev bunu önermişti) — bellekte `string.Compare` ile filtreleniyor, aynen önceki gibi. Gerekçe: gerçek performans sorunu (O(Cari×Hareket)) yalnızca hareket tablosundaydı; Cari listesi küçük (mevcut ortamda ~12 kayıt) ve SQL'e taşımak `string.Compare`'in XPO LINQ sağlayıcısında güvenilir çevrilip çevrilmeyeceği belirsizliğini (test edilmemiş risk) gereksiz yere üstlenmek olurdu.
- **G20:** Ağ çağrısı ObjectSpace açılmadan ÖNCEye taşınmadı (görev "mümkünse" demişti) — bunu yapmak Worker'ın ObjectSpace oluşturma sırasını değiştirmeyi gerektirirdi. Kod içinde gerekçe yorumla belgelendi; XPO'da ObjectSpace'in kendisi ağ çağrısı süresince bir DB bağlantısı/transaction tutmadığından pratik bir maliyeti yok.

---

## Canlı doğrulama (Playwright, dev veritabanı)

Aşağıdaki senaryolar gerçek PostgreSQL dev veritabanına karşı, tarayıcı üzerinden uçtan uca test edildi:

1. **G1/G8/G9/G13/G14/G17 — Tedarikçi bakiye tutarlılığı:** "Test Tedarikçi Toptan A.Ş." Cari kartı `-₺1.500,00` gösteriyor; "Cari Hesap Ekstresi" raporu da AYNI hesabı `-1.500,00 TRY` olarak kapatıyor. **Kart ile ekstre artık birebir eşleşiyor** — denetim raporunun en kritik bulgusu (§4.1) doğrulanmış şekilde çözüldü.
2. **G16 — Sıfır maliyetli stok transferi:** Hiç maliyetli girişi olmamış ("Kiev 3 lü Motorlu", OrtalamaMaliyet=0) bir kalem, Merkez Depo→Şube Depo arasında 5 adet transfer edildi — önceden "birim maliyeti sıfırdan büyük giriniz" hatasıyla engelleniyordu, şimdi hatasız kaydedildi. Stok Bakiye ekranında her iki deponun miktarı doğru güncellendi (Merkez: 17→12, Şube: 0→5).
3. **G22 — KPI Dashboard:** Gösterge Paneli hatasız yükleniyor, 4 kart da doğru değerlerle render ediliyor.
4. **Genel:** Uygulama G1-G23'ün tamamı uygulandıktan sonra `--updateDatabase` ile şema güncellemesi sorunsuz tamamlandı, dev sunucu hatasız açılıyor, giriş/navigasyon çalışıyor.

---

## Sıradaki adımlar (İNSAN AKSİYONLARI — DUZELTME_GOREVLERI.md'den, kod ile yapılmadı)

- [ ] PostgreSQL `sofasiserp_app` parolasını değiştir (VPS + yerel).
- [ ] Yeni `UrlSigningKey` üret, production ortam değişkenine koy (VPS `/opt/sofasiserp/.env`).
- [ ] `SOFASIS_ADMIN_INITIAL_PASSWORD`'ü deploy ortamında ayarla.
- [ ] Production `AllowedHosts`'u gerçek alan adıyla sınırla.
- [ ] Git geçmişindeki sızmış `UrlSigningKey` için BFG/filter-repo temizliği değerlendir.
- [ ] **G1 sonrası kritik:** Bu branch VPS'e deploy edildiğinde `--updateDatabase` çalıştığında Updater'daki GuncelBakiye yeniden hesaplama migration'ı otomatik uygulanacak — üretim verisindeki tedarikçi bakiyelerinin yeni değerlerini mali müşavirle/kayıtlarla karşılaştırarak doğrula.
- [ ] Bu branch'i gözden geçirip (Cowork'teki Claude + Sedat) `master`'a merge et.
