# 01 — Mimari ve Kararlar (ADR)

Bu doküman mimarinin genel çerçevesini ve alınan tüm kilit kararların gerekçesini içerir. Yeni mimari karar buraya ADR olarak eklenir; değişen karar "Değiştirildi" işaretlenip yenisi yazılır.

---

## Mimari Çerçeve

Dört mantıksal katman: (1) **Sunum** — Blazor Server host (`SofasisERP.Blazor.Server`): XAF Application Model, controller'lar, DI, güvenlik. (2) **Domain** — `SofasisERP.Module`: XPO iş nesneleri + iş kuralları. (3) **Domain servisleri** — `Services/`: arayüz arkası çapraz mantık (numaralandırma, KDV, maliyet, fiş üretimi, e-Belge). (4) **Entegrasyon** — e-Belge entegratörü (`SofasisERP.EInvoice.*`), muhasebe aktarım yazıcıları.

Bağımlılık tek yönlü: üst modüller (Fatura, Satış, Üretim) alt/ortak modüllere (Base, GenelTanımlar, Cari, Stok) bağımlıdır; tersi olmaz. Domain hiçbir somut entegratörü tanımaz (yalnız `IEInvoiceProvider`).

---

## ADR Kayıtları

### ADR-001 — XAF Blazor Server + XPO + .NET 10 / DevExpress 26.1.3
Kullanıcı tercihi ve mevcut ekip bilgisi. XAF'ın güvenlik/denetim/model altyapısı ERP için hızlı ve sağlam temel. Domain platformdan bağımsız (`SofasisERP.Module`), ileride başka host eklenebilir.

### ADR-002 — Sıfırdan kurulum, mevcut proje yalnız şablon
Mevcut "Sofasis Erp Project" olgun ama teknik borç taşıyor (bkz. `02_Mevcut-Proje-Analizi.md`). Kod taşımak borcu da taşır; bu yüzden **sıfırdan**, ama aynı yapı/konvansiyonla kurulur. Kanıtlanmış desenler (taban sınıflar, M/D, Türkçe isimlendirme) korunur; hatalar tekrarlanmaz.

### ADR-003 — Birincil anahtar: Guid (Oid)
Eski projede PK, `IDGeneratorService` ile üretilen string(13) `KeyID`'di; tek sunucuda güvenli ama çok-instance'ta çakışma riski ve string join maliyeti taşır. **Guid Oid** çok-sunucu/entegrasyon güvenli ve çakışmasız. İş numaraları (FisNo/kod) ayrı, kullanıcıya dönük alanlardır; PK değildir. Çapraz-tablo bağları `[Association]` ile.

### ADR-004 — Numaralandırma: tek üretici, onayda, boşluksuz
Eski projede üç mekanizma vardı (IDGenerator + DistributedIdGeneratorHelper + global-kilitli SequenceGenerator). **Tek** `INumberSequenceService`. Yasal belge numarası **onay/posting anında** atanır → iptal/rollback numara yakmaz (mevzuat boşluksuz seri ister). Global kilit deseni kaldırıldı (darboğaz).

> **Düzeltme (Faz 0, doğrulandı):** `DistributedIdGeneratorHelper` diye bir DevExpress API'si 26.1'de yok (dxdocs'ta bulunamadı). Gerçek implementasyon: bir sayaç kaydı (`NumaraSayaci`), **çağıranın kendi Session/UnitOfWork'ü içinde** optimistic locking ile artırılır — ayrı `Session` AÇILMAZ. Bu, "boşluksuz" hedefini DistributedIdGeneratorHelper'dan bile daha iyi karşılar: dış işlem rollback olursa sayaç da rollback olur. Detay: `docs/00_Kod-Konvansiyonlari.md` §4.

### ADR-005 — Maliyet: ağırlıklı ortalama, servis arkası
Stok değerleme hareketli **ağırlıklı ortalama** (stok/varyant bazında tek). `IWeightedAverageCostService` arayüzü arkasında ve birim testli. Depo bazlı maliyet ihtiyacı doğarsa mimari buna açık.

### ADR-006 — İş mantığı servis katmanında
Hesaplama/aktarma (KDV, maliyet, fiş, onay) iş nesnesinin `OnSaving/OnChanged`'ine gömülmez; arayüz arkası servislerde toplanır. Neden: test edilebilirlik ve `OnSaving` içi ağır DB işinin yol açtığı tutarlılık/performans sorunlarından (eski D-1/D-2) kaçınmak.

### ADR-007 — Kapsam sırası: ön muhasebe önce, üretim sonra
Ön muhasebe alttan üste kurulur (stok-maliyet → fatura → fiş). Üretim, bu raylar (stok girişi, maliyet, muhasebe kancaları) kurulduktan sonra devreye alınır; böylece üretim hazır bir maliyet/muhasebe sistemine bağlanır. Satınalma ve Satış-Pazarlama atlanmaz; cari/stok/fatura kesişimleri ön muhasebede, üretime özgü kısım üretim fazında.

### ADR-008 — e-Belge entegratör bağımsız
Tüm e-Belge işlemleri `IEInvoiceProvider` arkasında; UBL-TR üretimi domain'de. Somut entegratör (Uyumsoft/İzibiz/Nes/Sovos…) DI ile host'ta. Entegratör değişimi domain'i etkilemez.

### ADR-009 — Güvenlik ve denetim ilk günden
Integrated Security (ApplicationUser, lockout), AuditTrail açık. Ek olarak kayıt üstü özet denetim alanları taban sınıfta; **DetailView'da ayrı "Denetim" sekmesinde ve en sonda** (bkz. `00_Kod-Konvansiyonlari.md` §3).

### ADR-010 — Muhasebe kapsamı: ön muhasebe + Tekdüzen aktarım
Cari/stok/fatura/kasa-banka + otomatik muhasebe fişi ve Tekdüzen Hesap Planı'na aktarım. Resmi yevmiye/defter-i kebir/e-Defter kapsam dışı (mali müşavir tarafında).

### ADR-011 — Faz 0 kapsamı genişletildi: temel referans kartları da dahil
`docs/03_Yol-Haritasi.md`'nin ilk taslağında Faz 0 yalnızca yükseltme/hijyen/test iskeletiydi. Kullanıcı talimatıyla Faz 0'a taban sınıflar + numaralandırma servisi ile birlikte **GenelTanımlar/Cari/Stok temel kartları** (BirimTanim, DovizTanim, KDVTanim, UlkeTanim, SehirTanim, AdresTanim, DepoTanim, FisTuruTanim, CariGrupTanim, CariHesapTanim, StokGrupTanim, StokTanim) da eklendi. Gerekçe: bu referans verileri her fazın önkoşulu; hareket/fatura olmadan bile bağımsız test edilebilir ve kurulabilirler. Hareket taşıyan/daha karmaşık kısımlar (StokModelTanim+Konfigürasyon, ölçü→m²/m³ otomatik hesap, TevkifatTanim, DovizGunlukKurM/D, FisTuruTanim'in borç/alacak tipi ve hedef view alanları) kasıtlı olarak **dışarıda bırakıldı** (YAGNI) — kullanılacakları faz geldiğinde (Faz 2/3/üretim) eklenecekler. Detay: `docs/04_Veri-Modeli.md`.

### ADR-012 — Denetim sekmesi: runtime ViewController değil, `DetailViewLayoutAttribute`
İlk taslakta "Denetim" sekmesini her DetailView'ın sonuna taşımak için bir `ViewController` + `IModelViewLayout` manipülasyonu öneriliyordu. DevExpress dokümantasyonu bu senaryo için resmi olarak `[DetailViewLayoutAttribute(groupId, LayoutGroupType.TabbedGroup, groupIndex)]` property attribute'unu gösteriyor: taban sınıftaki 4 denetim alanına uygulanınca, tüm alt sınıfların tüm DetailView'larında otomatik olarak tek bir "Denetim" sekmesi oluşur ve yüksek `groupIndex` (1000) sayesinde en sona yerleşir — ayrı bir controller'a gerek kalmadan. Daha az kod, daha az kırılganlık, resmi olarak desteklenen yol. Detay: `docs/00_Kod-Konvansiyonlari.md` §2-3.

### ADR-013 — DevExpress NuGet paketleri daima yerel offline feed'den
DevExpress paketleri (`DevExpress.*`) ticari/lisanslı olduğundan nuget.org'da yoktur veya güvenilir değildir. Çözüm kökünde (`Sofasis.ERP/nuget.config`) `packageSourceMapping` ile `DevExpress*` deseni yerel kurulum dizinindeki offline pakete (`C:\Program Files\DevExpress 26.1\Components\Offline Packages`), `*` deseni nuget.org'a eşlendi. Bu dosya repoya işlenmiştir; yeniden kurulum/klon sonrası tekrar oluşturmaya gerek yoktur.

### ADR-014 — Fatura/e-Belge numaralandırması `INumberSequenceService`'i KULLANMAZ
Faz A/1'de Cari/Kasa/Banka hareketleri için kurulan `FisNo` formatı (`{FisTuruKodu:6}-{yyMMdd}{sıra:D3}`, tarih gömülü, günlük sıfırlanan sayaç — bkz. `docs/00_Kod-Konvansiyonlari.md` §4) yalnızca **iç yardımcı defter** belgeleri içindir. Fatura/e-Belge (e-Fatura/e-Arşiv) numarası GİB'in resmi seri+sıra no kuralına tabidir: sabit 3 harf **belge serisi** + yıl içinde **kesintisiz, boşluksuz, sıfırlanmayan** 9 haneli sıra no (ör. `ABC2026000000001`), İPTAL/rollback numarayı YAKMAZ. Bu, tarih-gömülü/günlük-sıfırlanan mevcut formatla **uyumsuzdur** ve Faz 3/4'te ayrı bir servis (`IEBelgeNumaralandirmaService` veya benzeri, seri+yıl bazlı, `ObjectSaving` değil yalnızca onay/posting anında atanan) olarak kurulacaktır. Mevcut `NumberSequenceService`/`FisNo` deseni Fatura'ya asla uzatılmamalı — bu karar Faz 3 tasarımına başlarken tekrar teyit edilecek.

### ADR-015 — Faz B: `KeyID` (string PK) → native Guid `Oid` migrasyonu

**Bağlam:** Kopyalanan mevcut projede (`Sofasis.Module`) PK, ADR-003'ün kararının aksine hâlâ eski projeden gelen `BaseClass.KeyID` (`string(13)`, `[Key(false)]` ile XPO'nun gerçek anahtar üyesi) idi — CLAUDE.md "GÜNCEL STRATEJİ" bölümü, DX/.NET yükseltmesiyle (Faz A) aynı anda karışıklık yaratmaması için bu geçişi kasıtlı olarak "Faz B"ye ertelemişti. Faz A 2026-08-11 itibarıyla derlenip stabil çalıştığından bu blokaj kalktı; kullanıcı nesne modeli hâlâ küçükken (Faz 2/Stok Hareketi başlamadan) geçişin yapılmasını onayladı.

**Karar:** `BaseClass : XPBaseObject` → `BaseClass : DevExpress.Persistent.BaseImpl.BaseObject` (native Guid `Oid`, `[Key(true)]`, otomatik üretim). Bu depoda zaten kanıtlanmış bir desen (`FilteringCriterion`, `ApplicationUserLoginInfo` bu tabanı sorunsuz kullanıyor). Kendi `KeyID` üyesi ve onu üreten `IDGeneratorService`/`IdGenerator.cs` tamamen kaldırıldı.

**Yan kararlar:**
- `IntegrationCode` (Cari↔Kasa/Banka ayna-kayıt eşleştirme alanı, `docs/00_Kod-Konvansiyonlari.md` §7) `string(13)` → `Guid?` olarak yeniden tiplendi; eşleştirme mekanizmasının kendisi (bağımsız iki satırı gerçek bir `[Association]` yerine paylaşılan bir değerle eşleştirme) değişmedi.
- `FiyatListeSablonD`/`RotaTanimD` detay satırlarının sırası daha önce `KeyID`'nin zaman-monotonik olma özelliğine (örtük, belgelenmemiş bir varsayıma) dayanıyordu; Guid rastgele olduğundan bu kırıldı. Açık bir `int SiraNo` alanıyla değiştirildi.
- `WW_FiyatListeRapor`/`WW_SatisSiparisRapor` (kendi ayrı `string(13)` PK'sını taşıyan, aktif kullanılmayan/boş rapor dataview tabloları) şemasına dokunulmadı — yalnızca 2 controller'daki derleme-kırıcı `.KeyID` çağrısı `.Oid`'e çevrildi; bu iki tablonun `MasterKeyID` join'i artık işlevsiz, ayrı bir görev olarak işaretlendi.
- Taban sınıf isimleri (`BaseClass`/`BaseClassWithAudit`/...) DEĞİŞTİRİLMEDİ — `docs/00_Kod-Konvansiyonlari.md` §2'nin öngördüğü `BaseObject`/`BaseObjectAudit` adlandırması aspirational/stale olarak işaretlendi; isim senkronizasyonu ayrı, bağımsız bir görev.
- Migrasyon sonrası dev veritabanı silinip `--updateDatabase` ile sıfırdan seed edildi (in-place veri taşıma yapılmadı) — prod veri yok, `DatabaseSeeder.cs` zaten PK'dan bağımsız.

### ADR-016 — Faz 2: Stok Hareketleri Master-Detail'e çevrildi + fiş-türü-özel 8 ekran

**Bağlam:** Faz 2'nin ilk taslağı `StokHareketleri`'ni yanlışlıkla düz/tek-satırlı `CariHesapHareketleri`/`KasaBankaHareketleri` şablonundan kopyalamıştı. Kullanıcı durdurdu: bir stok hareketi (alış/satış/sayım fişi), bir kasa işleminin aksine, doğası gereği çok kalemlidir — tek başlık altında birden fazla stok kalemi taşır; ayrıca gelecekteki fatura/irsaliye entegrasyonuna uyumlu olması gerekir.

**Karar:** `StokHareketleri` → `StokHareketleriM` (başlık: FisNo/FisTarihi/FisTuruTanim/DepoTanim) + `StokHareketleriD` (satır: StokTanim/Miktar/BirimMaliyet/ToplamMaliyet/NegatifBakiyeUyarisi/KaynakBelgeTipi+Oid), `SatisSiparisM/D` deseninden esinlenilerek. Motor mantığı (ağırlıklı ortalama, `StokBakiye` bul-veya-oluştur, negatif-stok politikası, silme-sonrası tam geçmiş replay) `StokHareketleriD.ObjectSaving()`/`ObjectDeleting()`'te toplanır — Master yalnızca başlık+numaralandırma. `StokTransferi` yapısal olarak ayrı kalır (kendi FisNo serisi); `ObjectSaving()`'i artık iki tam `StokHareketleriM` (Çıkış+Giriş, her biri tek satırlı) üretir, `ObjectDeleting()`'i ürettiği Master'ları kademeli siler. 8 fiş-türü-özel ekran (Stok Açılış/Alış Girişi/Üretim Girişi/Sayım Fazlası/Satış Çıkışı/Üretim Tüketimi/Sayım Eksiği/Fire-Zayiat) + genel salt-okunur liste, Kasa/Cari'deki kanıtlanmış desenle (`NewRecordDefaultsViewController` merkezi View.Id eşlemesi) kuruldu; `STTRGR`/`STTRCK`/`STTRNS` (transfer-türetilmiş) elle oluşturulmaz, kendi özel ekranları yok.

**Canlı testte bulunan ve düzeltilen kritik hatalar:**
- **Çoklu-flush motor tekrar çalışması:** XAF'ın Master-Detail popup akışı (satır eklenip "Kaydet"/"Kaydet ve Yeni" ile popup kapatılması) her kapanışta TÜM ObjectSpace'i flush ediyor; `Session.IsNewObject(this)` guard'ı bu senaryoda güvenilmez (aynı "yeni" nesne için birden fazla kez true dönebiliyor) — `StokBakiye`'de UNIQUE INDEX ihlaline yol açtı. **Çözüm:** persisted `bool MotorIslendi` bayrağı (motor çalıştıktan sonra true, guard `if (MotorIslendi) return;`) — hem `StokHareketleriD` hem `StokTransferi`'de. Ayrıca `StokTransferi.ObjectSaving()`'teki açık `.Save()` çağrıları (aynı commit döngüsü içinde ObjectSpace'in yeniden işlenmesine yol açıyordu) tamamen kaldırıldı.
- **Silme replay'inde kardeş-satır NullReferenceException:** Bir transfer silinirken üretilmiş iki Master eşzamanlı silinme sürecindeyken, `StokHareketleriD.ObjectDeleting()`'teki global-ortalama replay sorgusu kardeş satırı (henüz DB'den silinmemiş ama nihai durumda var olmayacak) da kapsıyor, `x.StokHareketleriM.FisTuruTanim` erişiminde patlıyordu. **Çözüm:** replay sorgusuna `KaynakBelgeTipi`/`KaynakBelgeOid` eşleşen kardeş satırları hariç tutan bir filtre eklendi.
- **Transfer-kaynaklı satırın doğrudan silinmesine karşı veri-katmanı guard'ı ÇALIŞMADI:** Bayrak tabanlı da, "kaynak StokTransferi hâlâ var mı" sorgusu tabanlı da denendi — `StokTransferi.ObjectDeleting()`'in `Session.Delete(Master)` çağrısının `OnDeleting`/`ObjectDeleting` zincirini XPO'nun tam olarak hangi aşamada tetiklediği güvenilir gözlemlenemedi, guard transferin KENDİ meşru silme sürecini de yanlışlıkla engelledi. **Karar:** kod-seviyesi guard tamamen kaldırıldı (`StokHareketleriD.OnDeleting()` yalnızca `base.OnDeleting()` çağırır); koruma UI seviyesine taşındı — genel "Stok Hareketleri" ekranının embedded satır grid'i için AYRI bir salt-okunur nested `ListView` (`StokHareketleriMGenelKalemler_ListView`, `AllowNew/AllowDelete/AllowUnlink=False`) tanımlanıp `PropertyEditor.View` (Current Object Parameter değil, doğrudan model `View` ataması) ile yalnızca genel `StokHareketleriM_DetailView`'e bağlandı; 8 özel ekran paylaşılan (kısıtlamasız) `StokHareketleriM_StokHareketleriDs_ListView`'i kullanmaya devam ediyor.
- **`ModelDefault("EditFormat", ...)` sessizce etkisizdi:** `IModelCommonMemberViewItem`'da böyle bir property YOK (doğrusu `EditMask`) — bu yanlış anahtar TÜM projede (Stok, Üretim, Satış Pazarlama modülleri) yaygın şekilde kullanılmıştı; sonucu, editör odaklandığında (DevExpress Blazor `DxSpinEdit`'in "focus'ta EditMask, blur'da DisplayFormat" davranışı) decimal alanlar varsayılan Currency mask'e (₺ sembolü) düşüyordu — DisplayFormat doğru olduğundan blur halinde fark edilmiyordu. **Düzeltme:** projedeki tüm `ModelDefault("EditFormat", ...)` → `ModelDefault("EditMask", ...)` olarak toplu değiştirildi (9 dosya, değer aynı kaldı).
- **Depo Transferi'nde Kaynak=Hedef seçimine karşı ek koruma:** Kaydet-anındaki `throw` zaten vardı; kullanıcı isteğiyle `HedefDepo`'ya `[DataSourceCriteria("Oid != '@This.KaynakDepo.Oid'")]` (Current Object Parameter) eklendi — Kaynak Depo seçildiğinde Hedef Depo listesinde artık görünmüyor, seçim anında engelleniyor.

**Doğrulama:** `dotnet test` 10/10 yeşil. Playwright canlı test: çok-satırlı açılış fişi (2 kalem, doğru `StokBakiye`/ortalama), negatif bakiye uyarısı (satır bazında kırmızı/kalın), tek satır silme replay (kardeş satır etkilenmeden), Depo Transferi oluşturma/silme (iki Master üretimi/kademeli silme, bakiye-ortalama doğru geri alınıyor, kardeş veriler bozulmuyor), genel ekrandan embedded grid'de Yeni/Sil aksiyonlarının kaybolduğu, Hedef Depo filtresinin çalıştığı, EditMask düzeltmesi sonrası hiçbir decimal alanda ₺ sembolü kalmadığı doğrulandı.

---

## Açık Kararlar (ilgili faz öncesi netleşecek)
- Ağırlıklı ortalama düzeyi: stok bazında tek mi, depo bazında mı? (Faz 2)
- e-Belge entegratör seçimi + test ortamı. (Faz 4)
- Fiilen kullanılacak tevkifat kodları. (Faz 3)
- Güncel KDV oranları / e-Belge zorunluluk sınırları resmi kaynaktan teyit. (Faz 3–4)
- Proje/namespace adı `SofasisERP.*` mi `Sofasis.*` mi (varsayılan `SofasisERP.*`).
- **Virman fiş türleri model gapı (CAVRMN/KSVRMN/BNVRMN):** Mevcut `CariHesapHareketleri`/`KasaBankaHareketleri` her biri tek bir `KasaBankaTanim`/`CariHesapTanim` alanı taşıyor; gerçek iki-taraflı virman (kaynak hesap → hedef hesap, tek işlemde) için bir "hedef hesap" alanı yok. 2026-08-11'de tespit edildi, ekran kasıtlı olarak yapılmadı — bir "hedef hesap" alanı eklemek mi, yoksa virman'ı iki bağımsız (Çıkış+Giriş) hareket olarak mı modellemek gerektiği kullanıcıyla netleştirilecek.
- **Bilinen kozmetik sınırlama (Blazor, veri bütünlüğünü etkilemiyor):** `CariHesapHareketleri`/`KasaBankaHareketleri` DetailView'ında Döviz Kodu alanı (collapsed link/lookup), Cari/Kasa hesabı değiştirildiğinde veya Döviz Kodu elle değiştirildiğinde ekranda birkaç saniye eski değeri gösterebiliyor — arka plandaki veri (`DovizKuru`, `YerelTutar`) HER ZAMAN doğru hesaplanıyor, yalnızca bu tek alanın görsel yenilenmesi gecikebiliyor (kayıt sonrası veya alana tıklanınca netleşiyor). "Derin düzeltme" (ObjectSpace.ObjectChanged + PropertyEditor.Refresh) denendi, istenmeyen yan etkiler (gereksiz "kaydedilmemiş değişiklik" uyarısı, beklenmedik popup açılması) yarattığı için geri alındı; düşük öncelikli, açık bırakıldı.
