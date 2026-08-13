# SofasisERP — Fazlı Yol Haritası

**Temel:** Mevcut "Sofasis Erp Project" yapısı ve konvansiyonları (bkz. `Sofasis-Kod-Analizi.md`).
**Hedef:** Koltuk üretimi + Türk vergi/muhasebe mevzuatına uygun ön muhasebe.
**Hedef teknoloji:** .NET 10 · DevExpress XAF 26.1.3+ · XPO · Blazor Server · SQL Server.
**Belge tarihi:** 2026-08-09 · Sürüm v1.0

---

## 0. Yol Haritasının Mantığı

Sıralama iki ilkeye dayanır: (1) **önce zemini sağlamlaştır** — kırık/riskli olanı ve sürüm borcunu temizlemeden yeni modül eklemek borcu büyütür; (2) **ön muhasebe alttan üste kurulur** — stok hareketi ve maliyet motoru olmadan fatura, fatura olmadan muhasebe fişi doğru kurulamaz. Bu yüzden faz sırası: temizlik/yükseltme → kritik düzeltmeler → stok-maliyet → fatura/KDV → e-Belge → muhasebe → finans tamamlama → raporlama.

Her faz sonunda birim testleri + kısa kabul kontrolü yapılır; ilgili doküman ve değişiklik günlüğü güncellenir. Yeni iş nesneleri mevcut konvansiyonla adlandırılır (Türkçe; `Tanim`/`M`/`D`/`Hareketleri`/`Parametre`), `BaseClassWithAudit(AndDescription)`'tan türetilir.

---

## Faz 0 — Zemin: Yükseltme, Hijyen, İskelet

**Amaç:** Temiz, yinelenebilir, test edilebilir bir başlangıç.

> ✅ **Kapsam genişletildi (ADR-011):** Yükseltme/hijyen/test iskelesine ek olarak taban sınıflar (`BaseObject`/`BaseObjectAudit`/`BaseObjectAuditAciklama`), `INumberSequenceService` ve GenelTanımlar/Cari/Stok temel kartları da Faz 0'da kodlandı ve test edildi (bkz. `docs/04`). Kabuk (host wiring) DevExpress Solution Wizard ile oluşturuldu; domain kodu bunun üzerine eklendi.

**Kapsam.**
- DevExpress **26.1.3** / **.NET 10** yükseltmesi; tüm projelerde tek hedef çatı, `Deterministic=true`, sabit `FileVersion`.
- Kod hijyeni: ikiz `BusinessObjects/Base` + `BusinessObjects/BaseClasses` → **tek** `Base` klasörü; ikiz `IdGenerator` (Helper vs Generators) → tek; ölü `SatisYonetimi` klasörü ve `... - Copy.cs` dosyası kaldırılsın.
- Küçük tutarsızlıklar: yanlış `[XafDisplayName]` etiketleri (ör. `SatisSiparisD` = "Satın Alma…") düzeltilsin.
- `SofasisERP.Tests` projesi eklensin (xUnit/NUnit); ilk hedef: `TaxIdValidator` (VKN/TCKN), KDV hesap, ağırlıklı ortalama.
- Ortam kuralı: geliştirme/test **yalnız local**; bağlantı stringi teyitli. Git branch akışı (faz bazlı).

**Bağımlılık:** Yok (ilk faz).
**Definition of Done:** Çözüm 26.1.3/.NET 10 ile derlenir ve çalışır; veritabanı otomatik güncellenir; ikiz/ölü dosyalar yok; test projesi ayağa kalkar ve ilk testler yeşil.

---

## Faz 1 — Kritik Düzeltmeler (Bütünlük & Darboğaz)

**Amaç:** Analizde çıkan veri-bütünlüğü ve performans risklerini kapatmak.

**Kapsam.**
- **Silme korumaları** (referans bütünlüğü) yorumdan çıkarılıp merkezi hale getirilsin: `StokTanim`, `ReceteTanimM`, `StokModelTanim` kullanılıyorken silinemesin. Ortak bir "kullanımda mı?" yardımcı deseni.
- **Kasa/banka aynalama (D-2):** `CariHesapHareketleri.ObjectSaving` içindeki `new Session(...)` kaldırılsın; aynalama parent transaction'ı içinde `IPostingService` veya gerçek `[Association]` ile yapılsın; `KeyID` string bağı yerine ilişki.
- **Numaralandırma tek sistem (D-3):** Belge kodları `DistributedIdGeneratorHelper`; `SequenceGeneratorHelper`'daki global `lock`+commit kaldırılsın veya kilitsiz retry desenine indirgensin.
- **Parametre/varsayılan okumaları (D-1):** `StokParametre`/varsayılan kartlar oturum bazında önbelleğe alınsın; `SatisSiparisD.OnLoaded`'daki satır-başı sorgu kaldırılsın.
- **Toplu maliyet (D-4):** `TopluReceteMaliyetViewController`'da `CommitChanges`/`RefreshDataSource` döngü dışına.

**Bağımlılık:** Faz 0.
**Definition of Done:** Kullanımdaki ana veri silinemez (testli); kasa/banka aynası tek transaction'da tutarlı; belge no üretimi tek sistemden ve yük altında boşluksuz; büyük sipariş/gridde satır-başı sorgu yok (ölçümlü).

---

## Faz 2 — Stok Hareketi + Ambar Bakiyesi + Ağırlıklı Ortalama Maliyet

**Amaç:** Ön muhasebenin ve maliyetin temeli. (Şu an eksik.)

**Kapsam (yeni iş nesneleri, mevcut konvansiyonla).**
- `StokHareketleri` — giriş/çıkış/transfer/sayım; depo + stok + miktar + birim maliyet + kaynak belge izi (tip + KeyID).
- `AmbarBakiye` (veya `StokBakiye`) — stok × depo anlık bakiye (özet).
- `StokMaliyetHareketleri` / motor — her girişte hareketli **ağırlıklı ortalama** güncellemesi; çıkışlar o anki ortalama ile değerlenir. `IWeightedAverageCostService` arayüz arkasında, testli.
- Depo transferi belgesi; negatif stok politikası (parametrik).

**Bağımlılık:** Faz 1 (numaralandırma, parametre cache).
**Definition of Done:** Alış/üretim girişleri bakiyeyi ve ortalama maliyeti doğru günceller (birim testli senaryolar); çıkış maliyeti ortalamadan gelir; depo bazında bakiye raporlanır.

---

## Faz 3 — Fatura (Satış/Alış) + KDV / İskonto / Tevkifat

**Amaç:** Ticari belge → cari/stok/maliyet etkisi.

**Kapsam.**
- `SatisFaturaM/D`, `AlisFaturaM/D`, `FaturaParametre`. Sipariş→irsaliye→fatura veya doğrudan fatura akışı; mevcut `SatisSiparisM/D` ile bağ.
- `IVatCalculator`: satır bazında KDV (yuvarlama), Dahil/Hariç, iskonto zinciri; `TevkifatTanim` (GİB kodları) satır düzeyinde.
- Fatura onayında: cari hareket + stok çıkış/giriş + (satışta) satılan mal maliyeti; idempotent onay (`IsPosted`).

**Bağımlılık:** Faz 2 (stok/maliyet), mevcut cari & KDVTanim.
**Definition of Done:** Fatura kesildiğinde cari borç/alacak, stok hareketi ve maliyet doğru oluşur; KDV/tevkifat hesapları testli; mükerrer onay mükerrer kayıt üretmez.

---

## Faz 4 — e-Belge (Entegratör Bağımsız)

**Amaç:** e-Fatura / e-Arşiv / e-İrsaliye.

**Kapsam.**
- `IEFaturaProvider` arayüzü (Gönder/DurumSorgu/Gelen/MükellefSorgu/İptal); somut entegratör (Uyumsoft/İzibiz/Nes/Sovos…) DI ile, ayrı sınıf kütüphanesinde.
- UBL-TR 1.2 üretimi domain'de; `EBelge` kaydı (tip, ETTN/UUID, XML, durum, entegratör ref).
- Cari kartında mükellef durumu alanı (GİB sorgusundan); fatura anında e-Fatura/e-Arşiv kararı.

**Bağımlılık:** Faz 3 (fatura).
**Definition of Done:** Test ortamında bir entegratörle e-Fatura/e-Arşiv gönderilir, durum güncellenir; e-İrsaliye sevkte üretilir; entegratör değişimi domain'i etkilemez (arayüz testli).

---

## Faz 5 — Muhasebe: Tekdüzen Hesap Planı + Fiş + Aktarım

**Amaç:** Ön muhasebeden mali müşavire köprü.

**Kapsam.**
- `HesapPlaniTanim` (Tekdüzen Hesap Planı ağacı), `HesapEslestirmeTanim` (belge/olay → hesap; parametrik).
- `MuhasebeFisM/D` — fatura/tahsilat/ödeme/üretim olaylarından otomatik, dengeli fiş; `IJournalPostingService`.
- Dışa aktarım: Luca/Logo/Mikro (`IJournalExporter`). KDV listeleri, BA/BS altyapısı.

**Bağımlılık:** Faz 3–4.
**Definition of Done:** Satış/alış/tahsilat/ödeme/üretim için dengeli (borç=alacak) fiş üretilir (testli); eşleştirme tablosundan yönetilir; en az bir dış format export'u çalışır.

---

## Faz 6 — Finans Tamamlama

**Amaç:** Nakit döngüsünün kapanması.

**Kapsam.**
- `CekSenetTanim`/`CekSenetHareketleri` (alınan/verilen, portföy durumları), `TahsilatOdemeM/D` (fatura kapama/mahsup), cari mutabakat.
- Mevcut `KasaBankaTanim`/`KasaBankaHareketleri` ile entegrasyon; çek/senet durum geçişleri (StateMachine ile).

**Bağımlılık:** Faz 3, 5.
**Definition of Done:** Çek/senet portföy ve durum takibi; tahsilat/ödeme fatura kapatır ve fiş üretir; cari ekstre/mutabakat doğru.

---

## Faz 7 — Raporlama, Pano, İnce Ayar

**Amaç:** Görünürlük ve olgunluk.

**Kapsam.**
- Cari ekstre/yaşlandırma, stok durum/değer (ağırlıklı ortalama), üretim maliyet analizi, KDV/tevkifat listeleri, çek-senet vade, kasa/banka gün sonu, muhasebe fiş dökümü.
- Blazor panolar; lokalizasyon (etiketlerin resx'e taşınması, EN altyapısı); performans profilleme (D-5 thumbnail cache, D-7 izin cache).

**Bağımlılık:** Önceki tüm fazlar.
**Definition of Done:** Temel raporlar ve panolar çalışır; kritik ekranlarda performans ölçümü kabul sınırında.

---

## Özet Sıralama ve Öncelik

| Faz | Başlık | Öncelik | Ön koşul |
|---|---|---|---|
| 0 | Yükseltme + Hijyen + Test iskeleti | Yüksek | — |
| 1 | Kritik düzeltmeler (bütünlük/darboğaz) | Yüksek | 0 |
| 2 | Stok hareket + ambar + ağırlıklı ortalama | Yüksek | 1 |
| 3 | Fatura + KDV/tevkifat | Yüksek | 2 |
| 4 | e-Belge (entegratör bağımsız) | Orta | 3 |
| 5 | Tekdüzen + muhasebe fişi + aktarım | Yüksek | 3–4 |
| 6 | Çek/senet + tahsilat-ödeme + mutabakat | Orta | 3,5 |
| 7 | Raporlama + pano + ince ayar | Orta | Tümü |

## Açık Kararlar (fazlar başlamadan netleşmeli)

- Çalışılacak kopya: bu yedek mi, başka bir canlı kopya mı, yoksa temiz yeni klasör mü? (Faz 0 öncesi)
- Ağırlıklı ortalama düzeyi: stok bazında tek mi, depo bazında mı? (Faz 2)
- e-Belge entegratör seçimi ve test ortamı. (Faz 4)
- Fiilen kullanılacak tevkifat kodları. (Faz 3)
- Güncel KDV oranları ve e-Belge zorunluluk sınırları resmi kaynaktan teyit. (Faz 3–4)

---

*Bu plan mevcut yapıyı ve `Sofasis-Kod-Analizi.md` bulgularını temel alır. Her faz başında ilgili detaylı teknik tasarım (varlık-alan-ilişki) yapılır; plan ilerledikçe güncellenir.*
