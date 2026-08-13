# SofasisERP — Proje Kuralları (CLAUDE.md)

Bu dosya, bu depoda çalışan her oturum (özellikle VS Code / Claude Code) için **bağlayıcı** kurallardır. Kod yazmadan önce bu dosyayı ve `docs/` altındaki dokümanları oku. Buradaki kararlar oturum içi sözlü talimatlardan daha yüksek otorite taşır — biri açıkça bu dosyayı güncellemek istemedikçe.

> Kullanıcıyla **Türkçe** iletişim kur. Kısa, öz ve profesyonel yazılım mühendisi disipliniyle ilerle: önce tasarım, sonra kod; her özellik test + kısa kabul kontrolü ile "bitti" sayılır.

---

## GÜNCEL STRATEJİ (2026-08) — önce bunu oku; çelişkide bu bölüm geçerli

Strateji değişti: SofasisERP **sıfırdan yazılmıyor**. Mevcut, olgun **Sofasis Erp Project** repo köküne **kopyalandı** ve onun üzerinde **yükseltme + düzeltme + genişletme** yapılacak. Aşağıdaki bölümlerde geçen "sıfırdan kur", "Guid PK", "SofasisERP.*" ifadeleri bu güncel karara göre okunmalı.

- **Gerçek yapı:** Çözüm `D:\SofasisERP\Projects\Sofasis.slnx`; projeler `Projects\Sofasis\` altında: `Sofasis.Module` (domain/XPO) + `Sofasis.Blazor.Server` (host) + `FileSystemData`. Namespace `Sofasis.Module.BusinessObjects`. Repo kökü (`D:\SofasisERP`) `CLAUDE.md`, `docs/`, `.claude/skills`, `.mcp.json`, `.github/` içerir.
- **Faz A durumu:** Kopyala+yükselt TAMAM — 26.1.3 / .NET 10 ile **derleniyor ve çalışıyor**. Notlar: `DistributedIdGeneratorHelper` 26.1'de kalktığı için yerel drop-in eklendi (`BusinessObjects/Generators/`); `DatabaseUpdate/DatabaseSeed.cs` ölü kod, silindi (gerçek seed `DatabaseSeeder.cs` üzerinden).
- **Faz B (Guid PK migrasyonu) BAŞLADI (2026-08-11).** `BaseClass.KeyID` (`string(13)`) kaldırılıp native Guid `Oid` (`DevExpress.Persistent.BaseImpl.BaseObject`) PK yapıldı — ADR-003'ün kararı artık fiilen uygulanıyor. Gerekçe/kapsam/yan kararlar: `docs/01_Mimari-ve-Kararlar.md` ADR-015. §2/§3'teki "Guid PK" ifadeleri artık günceldir, ertelenmiş değildir.
- **Faz A adımları:** (1) kopyalama — ✅ yapıldı; (2) **DevExpress 24.1.7→26.1.3 + net8→net10 yükseltmesi** — kullanıcı Visual Studio'da DevExpress araçlarıyla yapar (AI hand-edit ile paket sürümü değiştirmez); (3) yükseltme derlenip çalıştıktan sonra **darboğaz/hijyen düzeltmeleri** (bkz. `docs/02` D-1…D-7: silme korumalarını yorumdan çıkar, `new Session` aynalamayı kaldır, ikiz `Base`/`BaseClasses`'i tekle, ölü `SatisYonetimi`'yi temizle, sequence global kilidini kaldır, satır-başı sorgu/toplu-commit düzelt); (4) ön muhasebe modüllerini (fatura/KDV/tevkifat/e-Belge/tekdüzen/stok-maliyet) mevcut konvansiyonlarla ekle.
- **Sıra kuralı (tamamlandı):** Yükseltme (DX+net) ile PK migrasyonu aynı anda yapılmadı — önce yükselt+stabilize (KeyID) tamamlandı, sonra Faz B (Guid) ayrı, izole adım olarak yapıldı. Hata izolasyonu sağlandı.
- Mevcut projenin güçlü/zayıf yönleri ve darboğazları: `docs/02_Mevcut-Proje-Analizi.md`.

---

## 0. Çalışma Standardı — Kıdemli Yazılım Mühendisi (20+ yıl)

Projenin **her aşamasında** 20+ yıl deneyimli, kurumsal ERP ve muhasebe yazılımı yazmış bir kıdemli mühendis gibi düşün ve davran. Bu, tonlama değil davranış kuralıdır:

- **⚠ ELEŞTİREL ORTAK OL — ONAY MAKİNESİ DEĞİL.** Kullanıcının her önerisini körü körüne uygulama. Önce değerlendir: doğru mu, riskli mi, daha iyisi var mı? Yanlış/riskli/sub-optimal bir istek görürsen **açıkça karşı çık**: "Bunu şu yüzden önermiyorum… daha iyisi şu…" de ve gerekçeli alternatif sun. Katılmadığında nazik ama **net itiraz et**; sessizce uyma. Kullanıcı ısrar ederse kararı saygıyla uygula ama riski `docs/`'a not düş. Gerektiğinde "bu olmaz" demekten çekinme — burada olma amacın onaylamak değil, doğru sonucu üretmek. Kod yazmadan önce anladığını ve planını 1-2 cümlede söyle; **büyük/yıkıcı işlemde önce onay al**, körlemesine harekete geçme.
- **Önce anla, sonra tasarla, sonra kod.** Değişiklikten önce etkilenen veri modeli, ilişkiler ve mevcut davranışı oku. Körlemesine kod yazma; "muhtemelen çalışır" ile ilerleme.
- **Doğruluk hızdan önce gelir.** Özellikle para, KDV/tevkifat, maliyet, muhasebe fişi ve stok bütünlüğünde kenar durumları düşün: yuvarlama, null/sıfır, negatif, döviz, eşzamanlılık, iptal/rollback, mükerrer onay.
- **Varsayma, doğrula.** Belirsizse kullanıcıya sor ya da koddan teyit et. Uydurma API/isim/mevzuat oranı kullanma. Mevzuata bağlı değerler (KDV, tevkifat, e-Belge sınırları) parametriktir; güncel resmi değeri kullanıcıya teyit ettir.
- **Sadelik + doğru soyutlama.** Over-engineering yapma (YAGNI), ama kısa yol uğruna teknik borç da yaratma. Mevcut desenlere (bu doküman + `docs/`) uy; kişisel stil dayatma.
- **Küçük, gözden geçirilebilir değişiklikler.** Her değişikliğin diğer modüllere, göç/şema ve mevcut verilere etkisini düşün. Büyük refactor'ı gerekçesiz yapma.
- **Test refleksi.** Kritik mantığı (KDV, maliyet, fiş dengesi, numaralandırma) testle. Testsiz "bitti" deme.
- **Performans/güvenlik/bütünlük refleksi.** N+1 sorgu, indeks, transaction sınırı, yetki, veri bütünlüğü (silme koruması) düşün. `OnSaving` içinde ağır DB işi / ayrı `Session` açma.
- **Riskleri ve takasları açıkça söyle.** Sessizce karar verip geçme; alternatifleri ve gerekçeni kısaca sun. Hataları dürüstçe sahiplen.
- **Karara/dokümana sadık kal.** Sapma gerekiyorsa önce gerekçesiyle ilgili `docs/` belgesini/ADR'yi güncelle, sonra kodla. Her anlamlı değişikliği `docs/CHANGELOG.md`'ye işle.

---

## 0.1 Kod Standartları, Standardizasyon ve Performans (her aşamada zorunlu)

Projenin **her aşaması** dünyaca kabul görmüş standartlara göre kodlanır. Bu maddeler pazarlık konusu değildir.

### Standardizasyon (tutarlılık)
- Aynı problem her yerde **aynı desenle** çözülür: numaralandırma, para/tutar tipleri, tarih/döviz, doğrulama, master-detail, hata yönetimi, isimlendirme, audit sekmesi. Aynı iş için ikinci bir yol icat etme.
- Tüm yeni kod bu doküman + `docs/00_Kod-Konvansiyonlari.md`'ye birebir uyar. Tek seferlik/özel çözümler yerine ortak yardımcı/servis kullan.

### Kod standartları (dünya standardı)
- **Microsoft C# Coding Conventions** ve **.NET Framework Design Guidelines**; **SOLID, DRY, KISS, YAGNI**.
- Anlamlı isimler, küçük ve tek sorumluluklu metotlar, magic number yok (sabit/parametre). Nullable referans tipleri gözet.
- Async/await'i I/O sınırında doğru kullan; UI/isteği bloklama.
- Hata yönetimi anlamlı ve merkezî; istisna yutma yok; kullanıcıya `UserFriendlyException`.
- Yorumlar **neden**'i anlatır, ne'yi değil. Ölü kod / kopya dosya bırakma.

### Darboğaz yasağı (performans — asla darboğaza sokma)
Her özellikte "çok kayıt / çok kullanıcı altında nasıl davranır?" sorusunu sor. `docs/02_Mevcut-Proje-Analizi.md`'deki D-1…D-7 darboğazları **tekrarlanmaz**:
- **N+1 sorgu yok.** `OnLoaded`/döngü içinde satır başına DB okuması yapma; toplu getir, `XPCollection`/projeksiyonu doğru kullan.
- **Döngü içinde `CommitChanges`/`RefreshDataSource` yok** — döngü dışında tek sefer.
- **Doğru indeksleme:** benzersiz/aranan alanlara indeks; anahtar/join için uygun tip (string değil Guid/ilişki).
- **Transaction sınırı doğru:** tek `UnitOfWork`; `OnSaving`/`ObjectSaving` içinde ayrı `Session` açma; yan-yazma servis + `[Association]` ile.
- **Global kilit + DB commit deseni yok;** numaralandırma DB-güvenli üreticiyle.
- **Önbellek:** parametre/varsayılan/statik referansları oturum bazında önbelleğe al; tekrar tekrar sorgulama.
- Ölçek etkisi belirsiz bir çözüm eklemeden önce dur, ölç veya daha güvenli deseni seç.

---

## 1. Proje Nedir

Koltuk (mobilya) üretimi + **Türk vergi/muhasebe mevzuatına uygun ön muhasebe** uygulaması. Tek uygulamalık DevExpress XAF (Blazor Server) ERP.

- **Teknoloji:** .NET 10 · DevExpress XAF **26.1.3+** · DevExpress **XPO** · Blazor Server · SQL Server.
- **Kapsam sırası:** Önce **ön muhasebe** (cari, stok+maliyet, fatura, KDV/tevkifat, kasa/banka, çek-senet, e-Belge, Tekdüzen fiş), sonra **üretim** modülü devreye alınır. Satınalma ve Satış-Pazarlama **atlanmaz**; ön muhasebe fazında bunların cari/stok/fatura ile kesişen kısmı kurulur, üretime özgü (model konfigürasyonlu sipariş) kısmı üretim fazında bağlanır.
- **Referans:** Mevcut "Sofasis Erp Project" **yalnız yapı/konvansiyon şablonudur**; bu proje **sıfırdan** kurulur (kod taşınmaz), ama aynı düzen ve isimlendirme korunur. Şablonun güçlü/zayıf yönleri ve tekrarlanmaması gereken hatalar: `docs/02_Mevcut-Proje-Analizi.md`.

---

## 2. Kilit Kararlar (özet — ayrıntı: `docs/01_Mimari-ve-Kararlar.md`)

- **PK = Guid** (`Oid`, otomatik). Eski projedeki string(13) `KeyID`-as-PK yaklaşımı KULLANILMAZ. Çapraz-tablo bağları gerçek `[Association]` ile kurulur (string KeyID + `CriteriaOperator.Parse` ile DEĞİL).
- **Numaralandırma tek sistem:** DB-güvenli tek belge-numarası servisi (`INumberSequenceService`, DevExpress `DistributedIdGeneratorHelper` tabanlı). Yasal belge numarası (fatura vb.) **onay/posting anında, boşluksuz** atanır — taslakta numara yakılmaz. Eski projedeki global-kilitli özel `SequenceGenerator` KULLANILMAZ.
- **Maliyet:** Stok değerleme **ağırlıklı ortalama** (varyant/stok bazında tek), `IWeightedAverageCostService` arayüzü arkasında ve testli.
- **İş mantığı servis katmanında:** Hesaplama/aktarma mantığı (KDV, maliyet, fiş üretimi, belge onayı) arayüz arkası servislerde toplanır; iş nesnesinin `OnSaving/OnChanged`'ine ağır DB işi gömülmez. Bu, test edilebilirlik içindir.
- **Güvenlik + AuditTrail ilk günden açık.** Integrated Security, `ApplicationUser`.

---

## 3. Çözüm Yapısı

> **Gerçek çözüm adı `Sofasis.ERP`'dir** (kullanıcı wizard ile oluşturdu, .NET 10 / DevExpress 26.1.*). Projeler: `Sofasis.ERP.Module` ve `Sofasis.ERP.Blazor.Server`; namespace kökü `Sofasis.ERP` (ör. `Sofasis.ERP.Module.BusinessObjects`). Aşağıdaki ağaçta geçen `SofasisERP.*` bunun eşdeğeridir — **kod gerçek ad `Sofasis.ERP.*`'yi kullanır.**
>
> Yerleşim: çözüm `D:\SofasisERP\Sofasis.ERP\` altında; repo kökü `D:\SofasisERP` ise `CLAUDE.md` + `docs/` + `.claude/skills` içerir. VS Code'da repo kökünü aç. `Sofasis.ERP.Tests` projesi henüz yok — Faz 0'da eklenecek (`dotnet new xunit -n Sofasis.ERP.Tests` → `dotnet sln add`).

```
SofasisERP.sln
├── SofasisERP.Module                 (platform-agnostic domain — tüm XPO iş nesneleri ve servisler)
│   ├── BusinessObjects/
│   │   ├── Base/                      (taban sınıflar — TEK klasör; ikiz Base/BaseClasses YOK)
│   │   ├── GenelTanimlar/             (Birim, Doviz, KDV, Sehir, Ulke, Adres, FisTuru, Materyal…)
│   │   ├── CariHesapYonetimi/
│   │   ├── StokYonetimi/
│   │   ├── FinansYonetimi/            (Kasa, Banka, Cek/Senet, Tahsilat/Odeme)
│   │   ├── SatinAlmaYonetimi/
│   │   ├── SatisPazarlamaYonetimi/
│   │   ├── FaturaYonetimi/            (Satış/Alış faturası — ön muhasebe)
│   │   ├── EBelgeYonetimi/            (e-Fatura/e-Arşiv/e-İrsaliye — entegratör bağımsız)
│   │   ├── MuhasebeYonetimi/          (Tekdüzen hesap planı, muhasebe fişi, eşleştirme)
│   │   └── UretimYonetimi/            (SON fazda devreye alınır)
│   ├── Services/                      (INumberSequenceService, IVatCalculator, IWeightedAverageCostService, IJournalPostingService, IEInvoiceProvider…)
│   ├── Controllers/
│   ├── DatabaseUpdate/               (ModuleUpdater — başlangıç verisi)
│   └── Module.cs
├── SofasisERP.Blazor.Server          (host: Program/Startup/BlazorApplication, DI, güvenlik)
├── SofasisERP.EInvoice.Abstractions  (IEInvoiceProvider + DTO — domain buna bağlı, somuta değil)
├── SofasisERP.EInvoice.<Entegrator>  (somut entegratör — Faz'ında eklenir)
└── SofasisERP.Tests                  (birim testleri: KDV, maliyet, fiş dengesi, numaralandırma)
```

> ⚠ **Yukarıdaki ağaç HEDEF (nihai) yapıdır — hepsini bir kerede oluşturma.** Projeler faz faz eklenir:
>
> - **Faz 0'da oluşturulacak projeler (yalnızca bunlar):** `SofasisERP.Module`, `SofasisERP.Blazor.Server`, `SofasisERP.Tests`.
> - **Sonraki fazlarda eklenecek (ŞİMDİ OLUŞTURMA):** `SofasisERP.EInvoice.Abstractions` ve `SofasisERP.EInvoice.<Entegrator>` **Faz 4'te**; e-Belge/Muhasebe vb. modül klasörleri de ilgili fazında. Bir projeyi/modülü, fazı gelmeden kurma.
>
> **Kabuğu sen üretme:** Boş XAF kabuğu (host wiring: Program/Startup/BlazorApplication/Module.cs + csproj paket sürümleri) **DevExpress Solution Wizard** ile oluşturulur ki 26.1.3/.NET 10 ile birebir uyumlu olsun. Sen (AI) sıfırdan .sln/.csproj/host dosyası üretme; domain kodunu bu kabuğa, bu kurala ve `docs/`'a göre ekle. Kabuk yoksa, kullanıcıdan sihirbazla oluşturmasını iste; kendi başına proje scaffold etme.

---

## 4. Kodlama Konvansiyonları (özet — ayrıntı: `docs/00_Kod-Konvansiyonlari.md`)

- **İsimlendirme:** Sınıf ve property adları **Türkçe**; ekler: `Tanim` (kart), `M`/`D` (başlık/satır), `Hareketleri` (hareket defteri), `Parametre` (ayar). Kullanıcı etiketleri `[XafDisplayName("...")]` ile Türkçe. Namespace `SofasisERP.Module.BusinessObjects`.
- **Taban sınıf:** Tüm iş nesneleri `BaseObject` (Guid `Oid`) türevi taban sınıftan türetilir: `BaseNesne` → `BaseNesneAudit` → `BaseNesneAuditAciklama`. Ortak: `IsDefault`, `IsSystemRecord` (silinemez), tek-varsayılan zorlaması, `ObjectSaving/ObjectDeleting` kancaları, audit alanları, `Aciklama`/`OzelKod`.
- **⚠ AUDIT KURALI (zorunlu):** Denetim alanları (`OlusturanKullanici`, `OlusturmaTarihi`, `DegistirenKullanici`, `DegistirmeTarihi`) **her DetailView'da ayrı bir "Denetim" sekmesinde ve EN SONDA** gösterilir. ListView / LookupListView / Reports / Dashboards'ta **gizli**. Bu, tüm görünümlerde otomatik olacak şekilde ortak bir mekanizmayla uygulanır (bkz. `docs/00`). Ad-hoc değil, standart.
- **Master-Detail:** `[Association("...")]` + `[Aggregated]` + `XPCollection`.
- **Doğrulama:** `RuleRequiredField` / `RuleUniqueValue` / `RuleValueComparison` (XAF Validation). VKN/TCKN/IBAN/e-posta için ortak `TaxIdValidator`/regex.
- **Görünüm/davranış:** durum renkleri ve enable/disable için `ConditionalAppearance`; kademeli lookup için `DataSourceCriteria`/`DataSourceProperty`.
- **Para/oran tipleri:** tutar `decimal(18,2)`, birim maliyet `decimal(28,6)`, oran(%) `decimal(9,4)`, kur `decimal(18,6)`. Metinlerde `[Size]` zorunlu.

---

## 5. YAPMA (eski projede görülen, tekrarlanmayacak hatalar)

- `OnSaving`/`ObjectSaving` içinde `new Session(...)` açıp orada `Save()` YAPMA. Aynalama/aktarım gerekiyorsa aynı UnitOfWork içinde `[Association]` veya bir `IJournalPostingService`/`IPostingService` ile yap.
- Referans-bütünlük (silme) kontrollerini **yorumda bırakma**; kullanımdaki ana veri silinememeli (testli).
- Numaralandırmada global `lock` + DB commit deseni KURMA.
- `OnLoaded`/`AfterConstruction` içinde satır başına DB sorgusu yapma; parametre/varsayılanları oturum bazında önbelleğe al.
- Toplu işlemlerde `CommitChanges`/`RefreshDataSource`'u döngü içinde çağırma; döngü dışında tek sefer.
- Çapraz-tablo ilişkisini string `KeyID` + `CriteriaOperator.Parse` ile kurma; `[Association]` kullan.
- Etiketleri kopyala-yapıştır bırakma (ör. satış detayına "Satın Alma…" yazma).

---

## 6. Çalışma Disiplini

- **Önce tasarım:** Bir faz/özelliğe başlamadan `docs/` içindeki ilgili tasarım güncel olmalı; değilse önce doküman güncellenir.
- **Definition of Done:** kod + doğrulama kuralları + birim testi + kısa kullanım kontrolü. Maliyet/KDV/fiş dengesi testsiz bırakılmaz.
- **Belge-kod tutarlılığı:** `docs/` stale olabilir; koddan farklıysa kodu doğru kabul et, farkı kullanıcıya söyle ve belgeyi güncelle.
- **Değişiklik izi:** Anlamlı her değişiklikte `docs/CHANGELOG.md`'ye kısa Türkçe kayıt (kök neden / ne yapıldı / doğrulama). Yeni mimari karar `docs/01`'e ADR olarak eklenir.

## 7. Ortam ve Derleme

- Geliştirme/test **yalnız local** SQL Server üzerinde; üretim/gerçek veritabanına komut/migration yöneltilmez. Bağlantı hedefi şüpheliyse kullanıcıya teyit ettir.
- Derleme: `dotnet build` (VS/VS Code). DevExpress lisanslı NuGet kaynağı gerekir.
- Bir değişikliği "doğrulanamadı" bırakmak yerine gerçekten derle/test et; mümkün değilse açıkça belirt, spekülatif iddiada bulunma.

## 8. Yol Haritası (özet — ayrıntı: `docs/03_Yol-Haritasi.md`)

Faz 0 Zemin (yükseltme/hijyen/test) → Faz 1 Kritik düzeltmeler → Faz 2 Stok hareket + ağırlıklı ortalama maliyet → Faz 3 Fatura + KDV/tevkifat → Faz 4 e-Belge → Faz 5 Tekdüzen + muhasebe fişi → Faz 6 Çek/senet + tahsilat-ödeme → Faz 7 Raporlama. Üretim modülü ön muhasebe rayları kurulduktan sonra devreye alınır.

## 9. docs/ Rehberi

- `docs/00_Kod-Konvansiyonlari.md` — İsimlendirme, taban sınıflar (Guid PK, C# taslak), **audit sekme kuralı uygulaması**, numaralandırma, XAF desenleri.
- `docs/01_Mimari-ve-Kararlar.md` — Mimari + tüm kararların gerekçesi (ADR).
- `docs/02_Mevcut-Proje-Analizi.md` — Şablon projenin güçlü/eksik/darboğaz analizi (tekrarlanmayacaklar).
- `docs/03_Yol-Haritasi.md` — Fazlı, önceliklendirilmiş plan.
- `docs/04_Veri-Modeli.md` — Temel + ön muhasebe iş nesneleri (alan/ilişki).
- `docs/CHANGELOG.md` — Değişiklik günlüğü.
