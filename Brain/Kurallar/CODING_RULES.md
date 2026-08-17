# SofasisERP — Kodlama Kuralları

Bu belge, SofasisERP projesinde yazılan tüm kodların uyması gereken standartları tanımlar.

---

## 1. Türkçe İsimlendirme Standardı

### 1.1. Kapsam

Proje tarafından oluşturulan tüm sınıf, Business Object, property, metot, enum, tablo ve kolon isimleri **Türkçe** olmalıdır.

| Öğe | Örnek |
|-----|-------|
| Sınıf / Business Object | `StokTanim`, `CariHesap`, `SatisSiparisi` |
| Property | `StokKodu`, `BirimFiyat`, `AktifMi` |
| Metot | `StokBakiyesiHesapla()`, `SiparisiOnayla()` |
| Enum | `SiparisDurumu.Taslak`, `OdemeSekli.Nakit` |
| Tablo (XPO) | `StokTanim`, `SatisSiparisM` |
| Kolon | `StokKodu`, `BirimFiyat` |

### 1.2. Kurallar

- **Türkçe karakter kullanılmaz**: `ı`, `ğ`, `ü`, `ş`, `ö`, `ç` yerine ASCII karşılıkları kullanılır (`i`, `g`, `u`, `s`, `o`, `c`).
  - Doğru: `StokKodu`, `UrunGrubu`, `AktifMi`
  - Yanlış: `StokKodu`, `ÜrünGrubu`, `AktifMi`
- **PascalCase** kullanılır: `StokKodu`, `BirimFiyat`, `SatisSiparisi`.
- **Anlamlı ve kısa isimler** tercih edilir: `Fiyat` yerine `BirimFiyat`, `Tarih` yerine `SiparisTarihi`.
- **Kısaltmalar** standart ERP terminolojisine uygun olmalıdır: `M` (Master), `D` (Detail), `Tanim` (Tanım).
- **Master-Detail ilişkilerinde** `M` ve `D` ekleri kullanılır: `SatisSiparisM`, `SatisSiparisD`.

### 1.3. Framework İsimleri Korunur

DevExpress, XAF, XPO ve .NET framework API isimleri **değiştirilmez**:

- `XPObject`, `XPCustomObject`, `Session`, `UnitOfWork`
- `DevExpress.ExpressApp`, `DevExpress.Persistent.Base`
- `IObjectSpace`, `ViewController`, `DetailView`
- `Save()`, `Delete()`, `CommitChanges()`

Bu isimler framework tarafından tanımlanır ve Türkçe'ye çevrilmez.

---

## 2. Paket ve Bağımlılık Kuralları

- **DevExpress**: Sürüm `26.1.3` sabittir. Wildcard (`26.1.*`) kullanılmaz. Lokal paket kaynağı önceliklidir.
- **Diğer paketler**: En son stable sürüm kullanılır. Hedef daima **0 uyarı (0 warnings)**.
- **ORM**: Yalnızca DevExpress XPO kullanılır. EF Core kullanılmaz.

---

## 3. Proje Yapısı

- Business domain kodu `SofasisERP.Module` projesinde tutulur.
- UI kodu `SofasisERP.Blazor.Server` projesinde tutulur.
- Business Object'ler `SofasisERP.Module/BusinessObjects/` klasöründe yer alır.

---

## 4. Dokümantasyon

- Tüm dokümantasyon Türkçe yazılır.
- Dokümantasyon gerçek kodla tutarlı tutulur.
- Mimari kararlar `Brain/Mimari/` altında belgelenir. Tüm proje dokümantasyonu (`.md`) `Brain/` klasöründe kategorize edilir (Mimari, Kurallar, Kurulum) — kök dizine veya `docs/`'a yeni `.md` eklenmez.
