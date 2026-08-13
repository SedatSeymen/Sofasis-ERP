---
name: ekran-tasarim
description: SofasisERP için XAF ekran (DetailView/ListView) tasarım ve düzen standartları. Bir XAF formu/görünümü, sekme-grup düzeni, editör seçimi, alan görünürlüğü, durum renkleri veya liste sütunları oluştururken ya da düzenlerken KULLAN. Kural: denetim (audit) alanları her zaman EN SONDA ayrı "Denetim" sekmesinde; etiketler Türkçe; tutarlı ve darboğazsız.
---

# Ekran Tasarımı Standardı (SofasisERP · XAF Blazor)

Bir iş nesnesi için ekran (DetailView/ListView) tasarlarken veya değiştirirken bu kurallara uy. Amaç: tüm uygulamada **tek tip, öngörülebilir, hızlı** ekranlar. Kaynak konvansiyonlar: `docs/00_Kod-Konvansiyonlari.md`. Sapma gerekiyorsa önce o dokümanı güncelle.

## DetailView düzeni

- **Alan sıralaması (üstten alta):** önce kimlik (Kod, Ad), sonra sınıflandırma/ilişkiler (grup, cari, döviz, KDV…), sonra tarih/vade, sonra tutarlar, en sonda açıklama.
- **Mantıksal gruplar/sekmeler**, Türkçe adlarla (ör. "Genel", "Adres", "Tutarlar", "Satırlar"). İlgili alanlar aynı grupta.
- **⚠ Denetim sekmesi zorunlu ve en sonda:** `OlusturanKullanici`, `OlusturmaTarihi`, `DegistirenKullanici`, `DegistirmeTarihi` her DetailView'da ayrı **"Denetim"** sekmesinde ve **tüm sekmelerin en sonunda** gösterilir. Bu, `BaseObjectAudit` taban sınıfındaki `[DetailViewLayoutAttribute("Denetim", LayoutGroupType.TabbedGroup, 1000)]` ile otomatiktir (bkz. `docs/00` §2-3); yeni ekranda bu alanları elle başka yere koyma veya yeniden tanımlama.
- Master-Detail'de satırlar (`XPCollection`) kendi sekmesinde/grubunda; başlık tutar özetleri salt-okunur.

## Editör ve biçim

- Tarih: `DateTimeEdit`, `[ModelDefault("DisplayFormat","{0:dd.MM.yyyy}")]`.
- Para/tutar: `decimal`, `N2`; oran: `N0`/`N2`. Hesaplanan tutarlar (KDV, net, yerel) **salt-okunur** (`Appearance ... Enabled=false`).
- Çok satırlı metin: `[ModelDefault("RowCount","3")]`.
- Kod alanı: yeni kayıtta düzenlenebilir, kayıt sonrası kilitli (`Criteria="!(IsNewObject(this))"`).
- Lookup'lar kademeli ise `DataSourceCriteria` / `DataSourceProperty` ile filtrele.

## Görünürlük ve durum

- Detayda gerekli, listede gereksiz alanları `[VisibleInListView(false)]`.
- Zorunlu alanlar `RuleRequiredField`; benzersizler `RuleUniqueValue` + `[Indexed(Unique=true)]`.
- Durum/önem renklendirmesi `ConditionalAppearance` ile (ör. sipariş durumu, varsayılan kayıt, sistem kaydı). Renkler tutarlı olsun (aynı anlam → aynı renk).

## ListView

- Anlamlı `[DefaultProperty]`; sütunlar öz (kod, ad, tarih, tutar, durum). Ayrıntı alanları listede gizli.
- Toplu işlemler `SimpleAction`/`PopupWindowShowAction` olarak; satır içine iş mantığı gömme.
- Sık kullanılan görünümler için `ListViewFilter`.

## Navigasyon (Model.DesignedDiffs.xafml)

Eski şablon projenin (`D:\2025\ProjectsBackup\Sofasis\Sofasis`) kanıtlanmış deseni: sol navigasyonda her iş modülü kendi klasöründe, altında **"Tanımlar"** (kartlar) ve — ileride hareket doğduğunda — **"Hareketler"** alt grupları. Yeni bir "Tanım" sınıfı eklerken:

1. `Sofasis.ERP.Module/Model.DesignedDiffs.xafml`'de `<NavigationItems>` altında ilgili modül `<Item>`'ının (yoksa oluştur; `Caption`, `ImageName`, sıralı `Index`) `Tanımlar` alt-grubuna `<Item Id="XTanim_ListView" ViewId="XTanim_ListView" ObjectKey="" Index="N" IsNewNode="True" />` ekle.
2. Aynı `Id`'yi `<Item Id="Default" ...>` altında `<Item Id="XTanim_ListView" Removed="True" />` ile bastır — yoksa hem modül grubunda hem "Varsayılan"da mükerrer görünür.
3. Aggregated-only sınıflar (ör. `AdresTanim`) navigasyona hiç eklenmez (zaten `[DefaultClassOptions]` almazlar).

Mevcut modül grupları: **Genel Tanımlar** (`Action_OrganizeDashboard`), **Cari Hesap Yönetimi** (`BO_User`), **Stok Yönetimi** (`BO_Resources`). Yeni modül (Finans, Fatura…) eklerken aynı desenle yeni bir üst `<Item>` aç.

## Standardizasyon ve performans (zorunlu)

- Aynı tür ekran (kart, M/D belge, hareket defteri) **her yerde aynı düzen**. İkinci bir tasarım dili icat etme.
- **Darboğaza sokma:** ListView'da satır başına hesap/DB/dosya erişimi yapma; ağır görselleri (foto/thumbnail) önbellekle veya listede gösterme. `PersistentAlias`/projeksiyonu doğru kullan.
- Aşırı geniş tek-panel formdan kaçın; grupla/sekmele. Blazor'da editör sayısını makul tut.

## Kontrol listesi (bir ekranı "bitti" saymadan önce)
1. Alan sırası ve gruplama mantıklı, etiketler Türkçe.
2. Denetim alanları en sondaki "Denetim" sekmesinde, listede gizli.
3. Zorunlu/benzersiz/salt-okunur kuralları uygulanmış.
4. Kademeli lookup'lar filtreli; durum renkleri tutarlı.
5. ListView sütunları öz; toplu iş action olarak.
6. Satır başına sorgu / döngüde commit yok (darboğaz kontrolü).
