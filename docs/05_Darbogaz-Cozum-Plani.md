# 05 — Darboğaz Analizi ve Çözüm Planı (Faz A sonrası)

**Kapsam:** Taşınmış ve derlenen çözüm — `Projects/Sofasis` (Sofasis.Module, net10 / DevExpress 26.1.3).
**Yöntem:** Statik kod incelemesi + kod tabanı taraması (grep ile konum/sayı doğrulaması). Çalışma zamanı profillemesi (SQL trace, gerçek veri hacmi) sıralamayı değiştirebilir; devreye alma öncesi ölçüm önerilir.
**İlgili:** `docs/02_Mevcut-Proje-Analizi.md` (ilk analiz, D-1…D-7), `docs/03_Yol-Haritasi.md`.
**Belge tarihi:** 2026-08-10 · Sürüm v1.0

---

## 1. Özet

Proje derleniyor ve çalışıyor; ancak taşıma sırasında **mevcut teknik borç bilinçli olarak korundu** (Faz A hedefi: önce çalışır hale getir). Aşağıdaki 10 madde, veri bütünlüğü → doğruluk → performans → yapısal sırasıyla önceliklendirildi. Yüksek öncelikliler düşük eforlu ve düşük risklidir; hızlıca kapatılmalı.

| # | Darboğaz | Önem | Efor | Konum (doğrulanan) |
|---|---|---|---|---|
| B1 | Silme korumaları yorumda | **Yüksek** (bütünlük) | Düşük | 4 dosya (StokTanim, StokGrupTanim, StokModelTanim, ReceteTanimM) |
| B2 | `OnSaving`/`ObjectSaving` içinde `new Session` | **Yüksek** (tutarlılık/sızıntı) | Orta | 3 dosya (CariHesapHareketleri, KasaBankaHareketleri, SASiparisM) |
| B3 | Belge no kayıtta üretiliyor + gap riski | **Yüksek** (mevzuat — faturada) | Orta | Tüm `*M`/`Hareketleri` OnSaving |
| B4 | `AfterConstruction`/`OnLoaded` satır-başı DB okuması | Orta (perf) | Orta | OnLoaded 2 dosya; FindObject 22 dosya / AfterConstruction 52 dosya |
| B5 | Numaralandırmada global `lock` + DB commit; 3 ayrı mekanizma | Orta (ölçek) | Orta | Generators: DistributedIdGeneratorHelper, IdGenerator, SequenceGeneratorHelper |
| B6 | Controller döngü-içi `CommitChanges`/`RefreshDataSource` | Orta (perf) | Düşük | 5 controller (TopluReceteMaliyet, ReceteMaliyet, FiyatListeOlusturma…) |
| B7 | Thumbnail her erişimde diskten | Düşük-Orta (perf) | Düşük | Helper.GetImage → StokModelTanim, WW_FiyatListeRapor |
| B8 | String `KeyID` ile çapraz-tablo bağı + string(13) PK | Orta (perf/yapı) | Yüksek | ~10 dosyada `CriteriaOperator.Parse`, 12 KeyID kriteri |
| B9 | `PermissionsReloadMode.NoCache` | Düşük (ölçek) | Çok düşük | Blazor.Server/Startup.cs |
| B10 | `DatabaseSeed` derleme dışı | Fonksiyonel eksik | Orta | Module.csproj Compile Remove |

---

## 2. Bulgular (detay)

### B1 — Silme korumaları yorum satırında (veri bütünlüğü)
`StokTanim`, `StokGrupTanim`, `StokModelTanim`, `ReceteTanimM` içindeki `OnDeleting` koruma blokları **yorumlu**. Yani kullanımdaki (siparişte/reçetede/fiyat listesinde geçen) ana veri **silinebiliyor** → yetim kayıt / tutarsızlık.
**Çözüm:** Blokları yorumdan çıkar; ortak bir `IReferansKontrol.KullanimdaMi(nesne)` yardımcı deseniyle merkezileştir; kullanımda ise `UserFriendlyException`. **Test:** kullanımdaki kayıt silinememeli.

### B2 — `ObjectSaving` içinde `new Session(...)` (tutarlılık + kaynak sızıntısı)
`CariHesapHareketleri.ObjectSaving`, kasa/banka aynasını yazmak için ayrı `new Session` açıp orada `Save()` yapıyor (KasaBankaHareketleri ve SASiparisM'de benzer). Parent'ın transaction'ından ayrı → yarım-kayıt riski; `using`/dispose yok → session sızıntısı; bağ string `KeyID` ile.
**Çözüm:** Aynalama aynı `UnitOfWork` içinde ve gerçek `[Association]` ile; veya bir `IPostingService.Post(...)` altında toplanıp tek işlemde. `new Session` tümüyle kalkmalı. **Test:** cari hareket + kasa/banka aynası tek transaction'da tutarlı; hata durumunda ikisi de geri alınır.

### B3 — Belge numarası kayıtta üretiliyor; boşluk (gap) riski
`OnSaving` içinde ilk kayıtta numara üretiliyor. Yasal belgelerde (fatura, muhasebe fişi — ileride) iptal/rollback numara yakar → **boşluklu seri**, mevzuata aykırı.
**Çözüm:** Yasal belgede numarayı **onay/posting anında** ata; taslakta değil. Dahili belgeler (sipariş) kayıtta kalabilir. **Test:** iptal edilen faturada numara yanmaz; seri boşluksuz.

### B4 — `AfterConstruction`/`OnLoaded` içinde satır-başı DB okuması (N+1)
`SASiparisD`/`SatisSiparisD.OnLoaded` her satır yüklendiğinde `StokParametre` çekiyor → çok satırlı gridde N sorgu. Ayrıca 52 dosyada `AfterConstruction`, 22 dosyada `FindObject` (çoğu varsayılan grup/birim/KDV/döviz/depo lookup'ı) — yeni kayıt açılışında çoklu sorgu.
**Çözüm:** Oturum bazlı **parametre/varsayılan önbelleği** (`IDefaultsCache` — StokParametre, varsayılan kartlar bir kez okunur). `OnLoaded`'daki satır sorgusu kaldırılır. **Kabul:** büyük sipariş/gridde satır-başı sorgu yok (SQL trace ile ölçüm).

### B5 — Numaralandırmada global kilit + DB commit; üç mekanizma
`DistributedIdGeneratorHelper` (Faz A drop-in), `IDGeneratorService` (KeyID), `SequenceGeneratorHelper` bir arada; belge no üretimi süreç genelinde `lock` içinde UoW commit ediyor → yoğun kullanımda serileşme.
**Çözüm:** Belge numarasını **tek** `INumberSequenceService` altında topla; global `lock`'u kaldır, XPO'nun transaction/optimistic-lock + retry desenine bırak. `IDGeneratorService` yalnız teknik anahtar üretimi olarak kalır (PK KeyID iken). **Kabul:** eşzamanlı üretimde boşluksuz ve kilitsiz.

### B6 — Controller'da döngü-içi commit/refresh
`TopluReceteMaliyet`, `ReceteMaliyet`, `FiyatListeOlusturma` gibi controller'lar döngü **içinde** `CommitChanges()`/`RefreshDataSource()` çağırıyor → O(n) commit + n UI yenileme.
**Çözüm:** Commit ve refresh döngü **dışına**, tek sefer. **Kabul:** toplu işlem tek commit; süre ölçülebilir biçimde düşer.

### B7 — Thumbnail her erişimde diskten
`Helper.GetImage`, `FileSystemStoreObject.RealFileName`'den her `Thumbnail` get'inde dosya okuyor; önbellek yok. Küçük resimli liste görünümlerinde ağır.
**Çözüm:** Bellekte hafif önbellek (KeyID→byte[]) veya listede thumbnail gösterme; yalnız DetailView'da yükle. **Kabul:** resimli listede tekrar tekrar disk erişimi yok.

### B8 — String `KeyID` çapraz-tablo bağı + string(13) PK (yapısal)
~10 dosyada `CriteriaOperator.Parse` ile `KeyID` üzerinden tablolar bağlanıyor; PK string(13) → geniş indeks, yavaş join, hataya açık.
**Çözüm (Faz B):** Bağları gerçek `[Association]`'a çevir; ardından **Guid PK migration** (yol haritası ADR-003, Faz B). Büyük/riskli — izole ve testli yapılmalı. **Kabul:** çapraz-tablo string kriteri kalmaz; PK Guid.

### B9 — `PermissionsReloadMode.NoCache`
Her Session'da ilk güvenli erişimde izinler DB'den yeniden yükleniyor → ek DB yükü.
**Çözüm:** Tazelik gereksinimi düşükse `CacheOnFirstAccess`. **Kabul:** yetki sorgu sayısı düşer; davranış doğrulanır.

### B10 — `DatabaseSeed` derleme dışı (fonksiyonel eksik)
Şu an seed yok → varsayılan kartlar (birim/KDV/döviz/depo), fiş türleri, roller oluşmuyor; birçok kartın "varsayılan bul" mantığı null dönüyor.
**Çözüm:** Seed'i mevcut base'e (CreatedBy/ModifiedBy) uyarla, `Updater`'a bağla. **Kabul:** ilk açılışta varsayılan veri gelir; yeni kart varsayılanları dolar.

---

## 3. Öncelikli Çözüm Planı (fazlı)

### Adım 1 — Hızlı kazanımlar + bütünlük (düşük risk, yüksek değer)
- **B1** silme korumaları (uncomment + merkezî helper)
- **B6** döngü-içi commit/refresh düzeltmesi
- **B7** thumbnail önbellek/erteleme
- **B9** `CacheOnFirstAccess`
- **B10** DatabaseSeed'i geri kazan + `Updater`'a bağla
→ Her biri bağımsız, küçük; test + kısa kabulle kapatılır.

### Adım 2 — Doğruluk ve tutarlılık (orta risk)
- **B2** `new Session` aynalamayı `[Association]`/`IPostingService`'e taşı
- **B5** numaralandırmayı tek `INumberSequenceService`'te topla, global kilidi kaldır
- **B4** `IDefaultsCache` + `OnLoaded` satır sorgusunu kaldır
→ Birim testleri: aynalama tutarlılığı, numaralandırma eşzamanlılık, gridde sorgu sayısı.

### Adım 3 — Yasal belge numaralandırma
- **B3** fatura/muhasebe fişinde numarayı **onayda** ata (ön muhasebe modülleriyle birlikte, Faz 3/5).

### Faz B — Yapısal
- **B8** çapraz-tablo bağlarını association'a çevir → **Guid PK migration** (izole, testli).

---

## 4. Notlar
- Her düzeltme sonrası `docs/CHANGELOG.md` güncellenir; mimari etkisi olan değişiklik `docs/01`'e ADR olarak eklenir.
- Adım 1 ve 2, ön muhasebe geliştirmesine (fatura/e-Belge/tekdüzen) **başlamadan önce** kapatılmalı — çünkü yeni modüller bu desenleri (numaralandırma, posting, defaults) tekrar kullanacak; borç önce temizlenirse yayılmaz.
- Sıralama önerisi net: **Adım 1 → Adım 2 → (ön muhasebe) → Adım 3 → Faz B.**
