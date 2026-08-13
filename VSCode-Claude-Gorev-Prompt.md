# Sofasis ERP — Claude Code (VS Code) Görev ve Bağlam Prompt'u

> Bu metni VS Code içindeki Claude Code oturumuna **ilk mesaj** olarak yapıştır.
> Amaç: Claude Code'un projeyi senin ortamında (derleyebildiği, çalıştırabildiği,
> veritabanına bağlanabildiği ortam) kıdemli bir mühendis gibi inceleyip,
> aşağıdaki açık sorunu **kök nedene inerek** çözmesi ve doğrulaması.

---

## 0) Rolün ve davranış kuralların (İSTİSNASIZ uy)

- 20+ yıl deneyimli, **kıdemli bir .NET/DevExpress XAF mühendisi** gibi düşün ve davran.
- **Varsayma, doğrula.** Kod yazmadan önce ilgili dosyaları, çalışan modeli, veritabanı şemasını ve gerektiğinde çalışan uygulamayı incele. "Muhtemelen şöyledir" ile kod yazma.
- **Onay makinesi olma.** Yanlış ya da riskli gördüğün her şeye gerekçesiyle **karşı çık**. Daha iyi bir yol varsa öner. Ben (kullanıcı) hata yapıyorsam söyle.
- **Dünyaca kabul görmüş kod standartları**, isimlendirme tutarlılığı ve standardizasyona uy. Darboğaz/performans sorunu yaratacak çözümlerden kaçın.
- **Türkçe konuş.** Kod içi isimlendirme ve UI Türkçe (projedeki konvansiyon).
- Her değişiklikten sonra **doğrula**: derle, çalıştır, ekranı/sonucu kontrol et. "Yaptım" deme; "yaptım ve şu şekilde doğruladım" de.

---

## 1) Proje nedir

- **Sofasis** — koltuk (mobilya) üretim yönetim sistemi. Türk **vergi ve muhasebe mevzuatına uygun ön muhasebesi** olan bir ERP.
- Uygulama adı (UI): **"Sofasis Koltuk Üretim Yönetim Sistemi"**.
- **Faz A**: Ön muhasebe + temel tanımlar. **Faz B** (sonra): üretim modülü derinleştirme, Guid PK'ye geçiş vb.

### Teknoloji
- **.NET 10**, **DevExpress XAF 26.1.3** (paketler 26.1.3'e **sabitli**, lokal beslemeli), **DevExpress XPO**, **Blazor Server** UI, **SQL Server**.
- Çözüm: `D:\SofasisERP\Projects\Sofasis.slnx`
- Projeler (`D:\SofasisERP\Projects\Sofasis\` altında): `Sofasis.Blazor.Server`, `Sofasis.Module`, `FileSystemData`.

---

## 2) Önce bunları OKU (bağlamın tamamı repoda)

Kod yazmadan önce şunları oku ve içselleştir:

- `D:\SofasisERP\CLAUDE.md` — proje kuralları (ana rehber).
- `D:\SofasisERP\.github\copilot-instructions.md` — aynısının aynası.
- `D:\SofasisERP\docs\` klasörü:
  - `00_Kod-Konvansiyonlari` — isimlendirme, base sınıflar, audit sekmesi kuralı vb.
  - `01_Mimari-ve-Kararlar` — kilitli kararlar.
  - `02_Mevcut-Proje-Analizi` — güçlü/zayıf yönler, darboğazlar.
  - `03_Yol-Haritasi` — fazlar/adımlar.
  - `04_Veri-Modeli`
  - `05_Darbogaz-Cozum-Plani` — B1..B10 darboğaz maddeleri.
  - `CHANGELOG`
- `D:\SofasisERP\.claude\skills\` — **DevExpress XAF/Blazor skill'leri** (39 adet: `ekran-tasarim`, `xpo-is-nesnesi` + 37 DevExpress). XAF/DevExpress ile ilgili her işte önce **ilgili skill'i oku**, tahminle davranma. (Örn. liste görünümü/arama için `devexpress-xaf-filtering`, `devexpress-blazor-grid`; görünüm layout için `devexpress-xaf-views`.)

---

## 3) Mimari kararlar ve konvansiyonlar (özet — ayrıntı docs'ta)

- **İş nesnesi base sınıfları** (`Sofasis.Module/BusinessObjects/Base/`):
  - `BaseClass` — `XPBaseObject`; **PK = KeyID (string, 13 hane)**, `IDGeneratorService` ile üretilir. `IsSystemRecord` alanı vardır.
  - `BaseClassWithAudit` — `BaseClass` + denetim: `CreatedBy` / `ModifiedBy` (`ApplicationUser` tipinde).
  - `BaseClassWithAuditAndDescription` — üstüne açıklama.
- **Audit (denetim) bilgileri UI'da DAİMA ayrı bir sekmede ve EN SONDA** gösterilir.
- **İsimlendirme (Türkçe):** `...Tanim` (kart), `...M` (master), `...D` (detay), `...Hareketleri`, `...Parametre`.
- **Numaralandırma:** İki üreteç var — (1) KeyID üreteci (PK), (2) fiş no üreteci. Fiş no, `FisTuruTanim` kayıtlarından gelir; kod üretimi `DistributedIdGeneratorHelper.Generate(...)` ile yapılır (26.1'de kaldırılan DevExpress sınıfının yerine yazılmış drop-in; `Sofasis.Module/Generators/`). İlgili `FisTuruTanim`, çoğu yerde **`ViewName`** ile bulunur.
- **KeyID string(13)** Faz A'da kalıyor; **Guid'e geçiş Faz B'ye ertelendi** (KeyID kaldırılmadı).
- **Master-Detail:** `[Association]` + `[Aggregated]` + `XPCollection`.
- **Doğrulama:** `RuleRequiredField`, `RuleUniqueValue`.
- **UI:** SDI (Single Document Interface). Türkçe dil paketi (satellite DLL'ler `Localization/tr`), `appsettings.json` → `DevExpress:ExpressApp:Languages = "tr-TR;"`, kültür tr-TR'ye zorlanmış.
- **Seed verisi:** `Sofasis.Module/Resources/Seed/*.csv` (`;` ayraçlı, UTF-8, gömülü kaynak) → `SeedCsvReader` (CsvHelper) → `DatabaseUpdate/DatabaseSeeder.cs` (idempotent). Eski `DatabaseSeed.cs` derleme dışı.

---

## 4) KISITLAR (ihlal etme)

- **Veritabanı: yalnızca LOKAL SQL Server** üzerinde geliştirme/test. **Asla production veritabanını hedefleme.** Bağlantı dizesini kontrol et; localhost/(local)/.\SQLEXPRESS gibi lokal bir instance olmalı.
- DevExpress paketleri **26.1.3'e sabit**, lokalden beslenir; sürüm yükseltme yapma.
- Yıkıcı işlemlerde (DB drop, tablo silme, toplu DELETE) önce **bana söyle ve onay al**; ne yapacağını ve nedenini açıkla.
- Değişiklikleri küçük, izlenebilir ve gerekçeli tut.

---

## 5) Bu oturuma kadar YAPILAN işler (senin ortamına eşitli)

Son çalışmalar (hepsi diske yazıldı, derlendi):

1. **B10 — CSV tabanlı seed:** `Resources/Seed/*.csv` + `CsvHelper` + `DatabaseUpdate/SeedCsvReader.cs` + `DatabaseUpdate/DatabaseSeeder.cs`; `DatabaseUpdate/Updater.cs` içinde `UpdateDatabaseAfterUpdateSchema` sonunda `new DatabaseSeeder(ObjectSpace).Seed();` çağrısı.
2. **UlkeTanim:** `UlkeKodu` (ISO 3166-1 alfa-2, zorunlu+benzersiz, oluşturulduktan sonra kilitli) ve `UlkeTelefonKodu` (örn. +90) eklendi. `ulkeler.csv` **249 ISO ülkesi** (Türkiye varsayılan). DetailView sırası: Ülke Kodu, Ülke Adı, Telefon Kodu.
3. **SehirTanim:** `PlakaKodu` eklendi. `sehirler.csv` 81 il, **plaka sırasına göre** doğru Türkçe adlarla.
4. **IlceTanim (YENİ nesne):** `BusinessObjects/GenelTanimlar/IlceTanim.cs`. Şehir→İlçe **master-detail**: `SehirTanim` içinde `[Association("SehirTanim-IlceTanims"), Aggregated] XPCollection<IlceTanim> IlceTanims`; `IlceTanim` içinde karşı taraf `SehirTanim`.
5. **Tüm ListView'larda metne göre arama:** `Controllers/ListViewAramaController.cs` — `ViewController<ListView>` içinde `((IModelListViewShowFindPanel)View.Model).ShowFindPanel = true;`.
6. **Arama kutusu sola hizalama:** `Sofasis.Blazor.Server/wwwroot/css/site.css` sonuna `.dxbl-grid-search-box { order:-1; margin-inline-start:0; margin-inline-end:auto; }` benzeri kural.
7. **Model (`Sofasis.Module/Model.DesignedDiffs.xafml`):**
   - `SehirTanim_DetailView` açıkça tanımlandı (üstte Şehir Adı + Plaka Kodu, altta **İlçeler** sekmesi).
   - "Genel Tanımlar" navigasyonuna Ülke'nin yanına görünür **"Şehir Tanımlama"** menü öğesi eklendi (`SehirTanim_ListView`, IsNewNode).

---

## 6) ÇÖZÜLECEK ASIL SORUN

**Belirti:** Şehir tanımının altında **İlçeler görünmüyor.** Yukarıdaki 4, 5, 7 numaralı değişiklikler yapılıp **Rebuild** edildiği hâlde İlçeler sekmesi ekrana gelmiyor.

Ek küçük konu: Liste görünümlerindeki **arama kutusu** geldi ama başta sağa hizalıydı; sola alma CSS'i eklendi — bunu da çalışan uygulamada doğrula.

### Şimdiye kadar denenenler (tekrar etme, üstüne git)
- `IlceTanim` nesnesi eklendi ve **derlenmiş DLL'de mevcut** (doğrulandı). Yani tip kayıtlı.
- `SehirTanim` içinde `IlceTanims` koleksiyonu + association eklendi (isimler iki tarafta eşleşiyor).
- `SehirTanim_DetailView` layout'u modele açıkça eklendi (İlçeler sekmesiyle).
- "Şehir Tanımlama" menü öğesi eklendi (standalone erişim için).

### Açık kalan hipotezler (SEN doğrula/çürüt)
1. **Veritabanı şeması güncellenmedi:** `IlceTanim` tablosu oluşmamış olabilir → koleksiyon yüklenemiyor. Kontrol et: DB'de `IlceTanim` tablosu var mı? Association FK kolonu (`SehirTanim` OID/KeyID) var mı?
2. **Eski model farkı (ModelDifference) baskın:** XAF görünüm düzenini `ModelDifference` / `ModelDifferenceAspect` tablolarında saklar. `SehirTanim_DetailView`'in eski (İlçeler'siz) düzeni kayıtlıysa modül xafml'i ezip İlçeler'i gizliyor olabilir. Kontrol et: bu tablolarda `SehirTanim_DetailView` içeren satır var mı? (Geliştirmede güvenli reset: bu iki tablonun içeriğini temizlemek — ama **önce bana söyle/onay al**.)
3. **Nested-only görüntüleme:** Şehir yalnızca Ülke'nin alt-listesinden açılıyorsa ve detay ekranı hiç açılmıyorsa İlçeler sekmesi görünmez. Yeni eklenen "Şehir Tanımlama" menüsünden standalone açıldığında görünüyor mu?
4. **Association/aggregation yanlış kurulmuş olabilir:** İki taraftaki `[Association("SehirTanim-IlceTanims")]` adının birebir eşleştiğini, koleksiyon tarafında `[Aggregated]` olduğunu, `IlceTanim.SehirTanim` referansının doğru tipte olduğunu XPO açısından doğrula.

### Senin AVANTAJLARIN (bende yoktu, sende var — kullan)
- Uygulamayı **derleyip çalıştırabilirsin** (Clean + Rebuild + Run). Embedded `Model.DesignedDiffs.xafml` değişikliği için **Clean Solution** sonrası **Rebuild** öneririm.
- **Lokal SQL Server'a bağlanabilirsin** — şemayı ve `ModelDifference` tablolarını doğrudan sorgula. (Bağlantı dizesi `appsettings.json`'da; **sadece lokal DB**.)
- Çalışan uygulamada ekranı/DOM'u ve **DevExpress model editörünü** inceleyebilirsin — arama kutusunun gerçek CSS sınıfını canlı DOM'dan teyit et (CSS kuralı tutmuyorsa doğru selektörü bul ve düzelt).
- Uygulama loglarını/exception'ları görebilirsin.

---

## 7) Senden beklenen çıktı

1. Yukarıdaki hipotezleri **sırayla, kanıtla** doğrula/çürüt (şema sorgusu, ModelDifference sorgusu, çalışan model, standalone vs nested).
2. **Kök nedeni** net söyle.
3. Projenin konvansiyonlarına uygun, minimal ve gerekçeli **düzeltmeyi** uygula.
4. Uygulamayı çalıştırıp **doğrula**: bir şehri aç → İlçeler sekmesi görünüyor, ilçe eklenip kaydedilebiliyor; liste görünümlerinde arama kutusu **solda** ve çalışıyor.
5. Yaptıklarını ve nasıl doğruladığını Türkçe özetle. Riskli/yıkıcı bir adım gerekiyorsa **önce sor**.

> Not: Değişiklik yaptığın dosyaları ve gerekçelerini açıkça belirt; `CHANGELOG`'a da işle. Bir şeye katılmıyorsan kıdemli mühendis olarak itiraz et.
