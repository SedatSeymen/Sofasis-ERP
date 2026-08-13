# 04 — Veri Modeli (Temel + Ön Muhasebe)

Bu doküman kurulacak iş nesnelerinin haritasıdır: modül, sınıf, amaç, öne çıkan alanlar ve ilişkiler. Her faz başında ilgili sınıflar alan-tip-kısıt düzeyinde detaylandırılır. Tüm sınıflar `BaseObject*` taban sınıflarından türer (Guid Oid + audit sekme kuralı; bkz. `00`). İsimlendirme: Türkçe + `Tanim`/`M`/`D`/`Hareketleri`/`Parametre`.

> ✅ **Faz 0'da kodlanmış (bkz. ADR-011):** `BirimTanim`, `DovizTanim`, `KDVTanim`, `UlkeTanim`, `SehirTanim`, `AdresTanim`, `DepoTanim`, `FisTuruTanim`, `CariGrupTanim`, `CariHesapTanim`, `StokGrupTanim`, `StokTanim`. Aşağıdaki listede bu sınıflara ait, Faz 0'da **kasıtlı olarak eklenmeyen** alanlar/ilişkiler (YAGNI — kullanılacakları faz geldiğinde eklenir) ayrıca **"Faz 0'da yok"** notuyla işaretlenmiştir.

---

## GenelTanimlar (temel referans veriler)

- **BirimTanim** — Adet/Metre/Kg/M²/Top; `UblBirimKodu` (e-Belge için); `IsDefault`.
- **DovizTanim** — TRY/USD/EUR; `DovizKodu`, `IsVarsayilan`, taban para birimi bayrağı.
- **DovizGunlukKurM / D** — Günlük kur başlığı (`KurTarihi`) + satır (`DovizTanim`, `DovizAlis`, `DovizSatis`).
- **KDVTanim** — `KDVOrani` (benzersiz), `IsVarsayilan`. (Oranlar parametrik; mevzuatla değişir.)
- **TevkifatTanim** — GİB tevkifat kodu, oran (ör. 5/10), açıklama. *(Faz 0'da yok — Faz 3 fatura/KDV ile birlikte eklenir.)*
- **UlkeTanim / SehirTanim** — Adres referansları.
- **AdresTanim** — Açık adres satırı/posta kodu/şehir/ülke. Faz 0'da tek sahibi `CariHesapTanim.Adresler` (aggregated XPCollection); "ev/iş adresi" ayrımı yapılmadı, tek bir liste. İleride başka bir modül (ör. Fatura) adres kullanacaksa, o modül için ayrı bir `[Association]` eklenir — bu, `GenelTanımlar → CariHesapYonetimi` yönünde bilinçli, sınırlı bir bağımlılık yaratır (bkz. `AdresTanim.cs` içi not).
- **FisTuruTanim** — Faz 0'da yalnızca `FisTuruAdi` + `NumaralandirmaOnEki`. Borç/alacak tipi ve hedef view alanları *(Faz 0'da yok — henüz bu alanları kullanacak hiçbir belge/fiş yok; Faz 3/5'te eklenir)*.
- **DepoTanim** — Depo/ambar; tip (Hammadde/YarıMamul/Mamul/Karışık), `IsVarsayilan`. (Çok depo temeli.)

## CariHesapYonetimi

- **CariGrupTanim** — Cari gruplama.
- **CariHesapTanim** — Müşteri/tedarikçi tek kart; `CariHesapKodu` (benzersiz, kullanıcı girer — Faz 0'da `INumberSequenceService`'e bağlanmadı, çünkü ADR-004 bu servisi *yasal belge numaraları* için öngörür, cari kodu bu kapsamda değil), `CariHesapAdi`, `CariHesapTipi` (Musteri/Tedarikci/MusteriVeTedarikci), `CariGrupTanim`, VKN/TCKN (`TaxIdValidator` ile `ObjectSaving`'de doğrulanır), vergi dairesi, e-posta (regex doğrulamalı)/telefon, `DovizTanim`, `Adresler` (aggregated). Fiyat listesi ve e-Fatura mükellef durumu *(Faz 0'da yok — Faz 3/4)*.
- **CariHesapHareketleri** — Cari borç/alacak hareketi; fiş no/tarih, vade, belge no/tarih, döviz + kur + yerel tutar, fiş türü. (Kasa/banka etkisi **servis** ile, aynı UoW'da — eski `new Session` deseni YOK.)

## StokYonetimi

- **StokGrupTanim** — Stok gruplama (Koltuk/Kumaş/Kirlent…); `IsDefault` (taban sınıftan).
- **StokModelTanim** + **StokModelKonfigrasyonM / D** — Koltuk modeli ve varyant/konfigürasyon (kumaş/renk/ayak vb.). *(Faz 0'da yok — üretime özgü, ADR-007 gereği üretim fazında bağlanır.)*
- **StokTanim** — Stok/hizmet/masraf kartı; `StokKodu`/`StokAdi` (benzersiz), `StokTipi` (Ticari/Mamul/YariMamul/Hammadde/Hizmet/Masraf), grup, birim, KDV, döviz, giriş deposu, `MinStokSeviyesi`/`MaxStokSeviyesi` (`decimal(18,4)`). Ölçüler (En/Boy/Yükseklik → m²/m³ otomatik hesap) *(Faz 0'da yok — Faz 2/üretim, gerçekten kullanılacağı yerde eklenir)*.
- **StokHareketleriM / D** — Master-Detail (ADR-016, `docs/01`): Master (`StokHareketleriM`) FisNo/FisTarihi/FisTuruTanim/BelgeNo-Tarihi/`DepoTanim` (bir belge tek depoya karşı işlem yapar); Detail (`StokHareketleriD`) `StokTanim`/Miktar/BirimMaliyet/ToplamMaliyet/NegatifBakiyeUyarisi/kaynak belge (tür+Oid) — motor mantığının (ağırlıklı ortalama, `StokBakiye` bul-veya-oluştur, negatif-stok politikası, silme-sonrası replay) TAMAMI Detail'de. 8 fiş-türü-özel ekran (Açılış/Alış Girişi/Üretim Girişi/Sayım Fazlası/Satış Çıkışı/Üretim Tüketimi/Sayım Eksiği/Fire-Zayiat) + genel salt-okunur liste. Bir belge çok kalemlidir; Satın Alma tarafında `IrsaliyeM/D`+`FaturaM/D` entegrasyonu KURULDU (ADR-017) — `STSAGR` fiş türü `IrsaliyeM`'in bire-bir kaynağıdır.
- **StokTransferi** — Depo-arası transfer; kendi FisNo serisi, `KaynakDepo`/`HedefDepo`/`StokTanim`/Miktar. Kaydederken iki tam `StokHareketleriM` (Çıkış `STTRCK` + Giriş `STTRGR`, her biri tek satırlı) üretir — bunların kendi özel ekranı yoktur, yalnızca genel listede görünür ve genel ekranda salt-okunurdur (doğrudan silme UI seviyesinde engellenir).
- **StokBakiye** — `StokTanim` × `DepoTanim` anlık bakiye (özet). Ağırlıklı ortalama maliyet `IWeightedAverageCostService` ile güncellenir.

## FinansYonetimi

- **KasaBankaTanim** — Kasa/banka hesabı; `DovizTanim`, IBAN vb.
- **KasaBankaHareketleri** — Nakit/banka hareketi; borç/alacak, döviz + kur + yerel, cari/belge ilişkisi.
- **CekSenetTanim / CekSenetHareketleri** — Alınan/verilen çek-senet; tutar, vade, banka, durum (Portföy/Ciro/Tahsilde/Tahsil/Karşılıksız). Durum geçişleri StateMachine ile.
- **TahsilatOdemeM / D** — Tahsilat/ödeme belgesi; nakit/banka/çek karışık satırlar; fatura kapama (mahsup).

## FaturaYonetimi (ön muhasebe çekirdeği)

> ⚠ Gerçekleşen tasarım aşağıdaki gibidir (Alış/Satış için AYRI sınıf DEĞİL — bkz. ADR-017, `docs/01`); bu sayfanın önceki taslağındaki `SatisFaturaM/D`/`AlisFaturaM/D` ayrımı v1.5 kararıyla YERİNE geçmiştir.

- **FaturaM / D** — Tek sınıf, yön (Borç=Alış/Alacak=Satış) `FisTuruTanim.FinansBorcAlacakTipi`'nden okunur (`StokHareketleriM.StokHareketYonu` deseniyle tutarlı). Master: `FaturaNo` (dahili, `INumberSequenceService`), `CariHesap`, `KaynakSiparisTipi`+`KaynakSiparisOid` (polimorfik — bugün yalnız `SatinAlmaSiparisiM`, Satış Faturası geldiğinde `SatisSiparisM`), `KaynakIrsaliye` (direkt `IrsaliyeM` referansı — İrsaliye şimdilik yalnız Satın Alma'da olduğundan polimorfik değil), `DovizTanim`/`DovizKuru`, KDV/Tevkifat/Toplam/Ödenecek/Yerel toplamlar, `Durum` (Taslak/Onaylandı — kayıt anında Onaylandı, ayrı onay akışı yok). Detail: `StokTanim`/Miktar/BirimFiyat/`KaynakStokHareketiD` (`StokHareketleriD`'ye DOĞRUDAN, `[Indexed(Unique=true)]` — bir stok hareket satırı en fazla bir faturaya bağlanabilir, mükerrer faturalama engellenir)/KDV/Tevkifat/Net tutar. `IFaturaKaydetServisi.FaturaTaslagiOlustur` (İrsaliye'den KDV/Tevkifat hesaplı taslak üretir) + kayıt anında `CariAynaKaydiOlustur` (`CariHesapHareketleri`'ne `IntegrationCode` ile ayna) + `KaynakSiparisDurumunuGuncelle` (siparişin TÜM satırları faturalanınca `Durum=Faturalandı`).
- **FaturaParametre** — Fatura ayarları (varsayılan seri, KDV tipi vb.). *(Faz 0'da yok.)*

## SatinAlmaYonetimi / SatisPazarlamaYonetimi

> ⚠ Gerçekleşen Satın Alma zinciri (SA-1…SA-4b, v1.5, ADR-017) aşağıdaki gibidir; önceki taslaktaki tek-adımlı `SASiparisM/D` yerine geçmiştir.

- **SatinAlmaTalebiM / D** — Satınalma talebi; `TalepNo`, `TalepEdenKullanici`, `Gerekce`, `Durum` (Taslak/OnayBekliyor/Onaylandi/Reddedildi/TeklifeCikildi/SiparisEdildi/IptalEdildi), tek-seviyeli rol bazlı onay (`ISatinAlmaOnayServisi`, "Satınalma Onaycısı" rolü, kendi-talebini-onaylayamama guard'ı).
- **SatinAlmaTeklifM / D** — Tedarikçi teklifi; `KaynakTalep` (zorunlu), `Tedarikci`, satır: `KaynakTalepD`/`TeklifBirimFiyat`/`EnDusukFiyatMi` (karşılaştırma servisi tarafından işaretlenir, `Appearance` ile yeşil vurgu).
- **SatinAlmaSiparisiM / D** — Sipariş; `KaynakTalep` (zorunlu)/`KaynakTeklif` (opsiyonel — Teklif Toplama atlanabilir), `Tedarikci`, `DovizTanim`/`DovizKuru` (tam desen — `DovizKuruGuncelle`), `Durum` (Verildi/KismiTeslimAlindi/MalKabulYapildi/Faturalandi/IptalEdildi); satır: `Miktar`/`BirimFiyat`/`TeslimEdilenMiktar`/`KalanMiktar` (kısmi teslimat). Kayıt sonrası tamamen immutable (kilit controller).
- **IrsaliyeM / D** — Sipariş ile stok/maliyet motorunun oturduğu `StokHareketleriM/D` (STSAGR) arasındaki, VUK'un istediği ayrı sevk belgesi (ADR-017). `IrsaliyeNo` (kendi `INumberSequenceService` serisi, `IRALIS` fiş türü), `KaynakSiparis`, `TedarikciIrsaliyeNo/Tarihi` (tedarikçinin KENDİ resmi irsaliyesi — dahili `IrsaliyeNo`'dan ayrı), `StokHareketleriM` (bire-bir, salt-okunur — `ISatinAlmaIrsaliyeServisi.IrsaliyeTaslagiOlustur` popup açılmadan ÖNCE birlikte üretir); satır `IrsaliyeD.StokHareketiD` bire-bir bağlı `StokHareketleriD`'ye işaret eder (asıl ağırlıklı-ortalama-maliyet hâlâ orada). "Fatura Oluştur" buradan başlar (`FaturaM.KaynakIrsaliye`).
- **SatisSiparisM / D** — Sipariş başlık/satır; satışın üretime özgü kısmı (model konfigürasyonu, kumaş, ayak tipi/rengi) **üretim fazında** tam bağlanır. (Satış tarafının İrsaliye/Fatura zinciri henüz kurulmadı — Faz gelecekte Satın Alma'daki simetrik desen kopyalanacak.)
- **SatisFiyatListeM / D**, **FiyatListeSablonM / D** — Fiyat listeleri.

## EBelgeYonetimi (entegratör bağımsız)

- **EBelge** — Giden/gelen e-Belge sarmalayıcı; tür (eFatura/eArşiv/eİrsaliye), UUID/ETTN, UBL-TR XML, durum (Taslak/Gönderildi/Kabul/Ret/İptal), entegratör referansı. İşlemler `IEInvoiceProvider` arkasında.

## MuhasebeYonetimi (Tekdüzen aktarım)

- **HesapPlaniTanim** — Tekdüzen Hesap Planı ağacı (kod/ad/üst/yaprak mı).
- **HesapEslestirmeTanim** — Belge/olay + rol → hesap kodu (parametrik eşleştirme).
- **MuhasebeFisM / D** — Fatura/tahsilat/ödeme/üretim olaylarından otomatik, dengeli fiş; `IJournalPostingService`. Dışa aktarım `IJournalExporter` (Luca/Logo/Mikro).

## UretimYonetimi (son fazda devreye)

- **ReceteTanimM / D**, **ReceteMaliyetD** — Reçete (BOM) + maliyet satırları.
- **RotaTanimM / D**, **OperasyonTanim**, **UretimMerkeziTanim** — Rota/operasyon/iş merkezi.
- **MaliyetParametre** — Maliyet ayarları (genel gider oranı, kur vb.).
- **UretimEmriM / D** — Üretim emri; tamamlanınca mamulü maliyetiyle stoğa basar (StokHareketleriM/D — Üretim Girişi/Üretim Tüketimi fiş türleri) ve muhasebe kancasını tetikler.

---

*Bu harita ADR-007 (ön muhasebe önce) sırasına göre uygulanır. Faz sırası ve öncelik: `03_Yol-Haritasi.md`.*
