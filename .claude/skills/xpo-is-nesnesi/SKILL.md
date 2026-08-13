---
name: xpo-is-nesnesi
description: SofasisERP için yeni bir XPO iş nesnesi (Tanim/M/D/Hareketleri/Parametre) oluştururken veya değiştirirken uygulanacak standart. Guid PK'lı taban sınıflar, Türkçe isimlendirme, doğrulama, master-detail, silme koruması, numaralandırma ve servis katmanı kurallarını kapsar. Bir XPO business object eklerken KULLAN.
---

# XPO İş Nesnesi Standardı (SofasisERP)

Yeni bir iş nesnesi eklerken bu kontrol listesine uy. Tek doğru kaynak: `docs/00_Kod-Konvansiyonlari.md` ve `docs/04_Veri-Modeli.md`. Sapma gerekiyorsa önce dokümanı güncelle.

## Zorunlu kurallar
1. **Taban sınıf:** `BaseNesne` / `BaseNesneAudit` / `BaseNesneAuditAciklama` türet (Guid `Oid`). Ham `XPObject`/`XPBaseObject` kullanma. Denetim gereken her kartta `...Audit(Aciklama)`.
2. **İsimlendirme:** Türkçe sınıf + property; ekler `Tanim`/`M`/`D`/`Hareketleri`/`Parametre`. `[XafDisplayName]` Türkçe, `[DefaultProperty]` anlamlı. Namespace `SofasisERP.Module.BusinessObjects`.
3. **Tipler:** tutar `decimal(18,2)`, birim maliyet `decimal(28,6)`, oran `decimal(9,4)`, kur `decimal(18,6)`. Metinlerde `[Size]` zorunlu.
4. **Doğrulama:** `RuleRequiredField`; benzersiz alan `RuleUniqueValue` + `[Indexed(Unique=true)]`; karşılaştırma `RuleValueComparison`. VKN/TCKN/IBAN/e-posta için ortak yardımcı.
5. **Master-Detail:** `[Association("...")]` + `[Aggregated]` + `XPCollection`. String `KeyID` ile bağ kurma.
6. **Silme koruması:** Kullanımdaki ana veri silinemez — `OnDeleting`/`ObjectDeleting`'de kontrol et, `UserFriendlyException` fırlat. **Kontrolü yorumda bırakma.**
7. **Numaralandırma:** Belge no tek servisten (`INumberSequenceService`); yasal belgede **onay/posting anında**, boşluksuz. Kayıt anında yakma.
8. **İş mantığı servis katmanında:** Hesap/aktarma (KDV, maliyet, fiş) arayüz arkası serviste. `OnSaving`/`OnChanged`'e ağır DB işi gömme; **ayrı `new Session` açma** — aynı `UnitOfWork` + `[Association]`/servis.
9. **Audit sekmesi:** Denetim alanları otomatik olarak en sondaki "Denetim" sekmesinde (bkz. `ekran-tasarim` skill'i / `docs/00` §3).

## Performans (darboğaz yasağı)
- `AfterConstruction`/`OnLoaded`'da satır başına DB sorgusu yok; parametre/varsayılanları önbelleğe al.
- Toplu işlemde `CommitChanges`/`RefreshDataSource` döngü dışında.
- Doğru indeks; N+1 yok.

## Test (DoD)
- Kritik mantık (KDV, maliyet, fiş dengesi, numaralandırma) için `SofasisERP.Tests`'te birim testi. Testsiz "bitti" deme.

## Faz sınırı
- Yalnızca fazı gelmiş modüllerde nesne oluştur. `EInvoice`/e-Belge = Faz 4, Muhasebe = Faz 5 — fazı gelmeden kurma. Kabuğu (`.sln`/host) AI oluşturmaz; DevExpress wizard üretir.
