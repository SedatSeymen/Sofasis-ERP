<!-- ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : README.md
 * Oluşturma Tarihi : 2026-08-17
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 2026-08-17
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Brain klasörünün içindekiler dizini — tüm proje
 *                    dokümantasyonu tek yerde, kategorize.
 * ****************************************************************************
-->

# Brain — Proje Dokümantasyonu

SofasisERP'nin tüm `.md` dokümantasyonu bu klasörde, konuya göre kategorize
edilmiş şekilde toplanır. Kod dışında projeyi anlamak için gereken her şey
buradadır.

## Mimari/

Ürün ve teknik mimari kararları.

- [SOFASIS_ERP_MIMARI_TASARIM.md](Mimari/SOFASIS_ERP_MIMARI_TASARIM.md) — Ana mimari tasarım belgesi (domain modeli, edition stratejisi, §45 ilk domain karar kapısı dahil tüm kararlar).
- [PRODUCT_EDITIONS.md](Mimari/PRODUCT_EDITIONS.md) — Basic/Pro/Enterprise sürüm matrisi, multi-tenancy mimarisi.
- [00_DetailView_ve_Servis_Konvansiyonlari.md](Mimari/00_DetailView_ve_Servis_Konvansiyonlari.md) — Eski projeden kanıtlanmış teknik desenler: DetailView sekme deseni (Denetim sekmesi), numaralandırma servisi, CSV seed altyapısı.

## Kurallar/

Kod yazım standartları.

- [CODING_RULES.md](Kurallar/CODING_RULES.md) — Türkçe-ASCII isimlendirme, DevExpress sürüm sabitleme, 0 uyarı/0 hata kuralı, dokümantasyon kuralları.

## Kurulum/

Projenin ilk kurulum süreci ve doğrulama kaydı.

- [SOFASIS_INITIAL_SETUP.md](Kurulum/SOFASIS_INITIAL_SETUP.md) — İlk kurulum görev tanımı/talimatı.
- [INITIAL_SETUP_REPORT.md](Kurulum/INITIAL_SETUP_REPORT.md) — Kurulumun sonunda doldurulan ortam ve doğrulama raporu.

## Bu klasörün dışında kalanlar

- **README.md** (kök dizin) — GitHub/IDE'nin repo açılışında otomatik gösterdiği tanıtım dosyası olduğu için kök dizinde kalır.
- **.roo/rules/sofasis-core.md** — Roo Code AI asistanının sabit yoldan otomatik okuduğu kural dosyası; taşınırsa Roo entegrasyonu kırılır. İçeriği `Kurallar/CODING_RULES.md` ile büyük ölçüde örtüşür, elle senkron tutulur.
