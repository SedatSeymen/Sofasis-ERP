# SofasisERP — ERP Mimari Tasarım ve Architect Başlangıç Dokümanı

## 1. Belgenin amacı

Bu belge SofasisERP'nin mimari yönünü, temel teknik kararlarını, domain yaklaşımını, stok ve konfigürasyon modelini, ürün/üretim ilişkisini ve geliştirme kurallarını Architect ve Code modlarının ortak referansı olarak tanımlar.

Bu belge bir kod üretme talimatı değildir.

Önce mimari ve business rule'lar netleştirilecek, ardından onaylanan tasarım uygulanacaktır.

---

# 2. Değişmez teknik kararlar

Aşağıdaki kararlar proje için bağlayıcıdır:

- DevExpress XAF sürümü: **26.1.3**
- Persistence: **XPO**
- Veritabanı: **PostgreSQL**
- Lokal makinede bulunan/sağlanan DevExpress paketleri kullanılacaktır.
- Gereksiz yere farklı DevExpress sürümü, EF Core veya alternatif persistence yaklaşımı seçilmeyecektir.
- Resmi DevExpress/XAF tooling ve template yaklaşımı tercih edilecektir.
- XAF'ın Security, Validation, Audit ve Model yetenekleri özel altyapı yazmadan önce değerlendirilecektir.
- Gerçek proje domain kodu, başlangıç teknik doğrulaması tamamlanmadan oluşturulmayacaktır.

---

# 3. Projenin temel amacı

SofasisERP; koltuk ve modüler mobilya üretimi/satışı etrafında şekillenen, stok, konfigürasyon, satış ve üretim süreçlerini birbirinden doğru biçimde ayıran bir ERP olacaktır.

Sistem yalnızca klasik CRUD ekranlarından oluşmayacaktır.

Kullanıcının yaptığı işlemler domain kurallarına göre yönlendirilecek ve veri bütünlüğü UI davranışına bırakılmayacaktır.

---

# 4. Ana domain yaklaşımı

Aşağıdaki kavramlar birbirinden farklı sorumluluklara sahiptir ve aynı kavrammış gibi modellenmeyecektir:

- Model
- Modül
- Kombinasyon
- Geçerli Konfigürasyon
- Varyant
- Satış Ürünü
- Üretim Ürünü
- Stok Kalemi
- Stok Konfigürasyonu
- Üretim Konfigürasyonu

Özellikle:

**Koltuk = tek stok kalemi değildir.**

Bir koltuk; model, modül, varyant ve geçerli konfigürasyonların birleşimi üzerinden anlam kazanabilir.

---

# 5. Modüler ürün yaklaşımı

Sistem modüler ürün mantığını destekleyecektir.

Örneğin bir koltuk:

- farklı modüllerden,
- farklı yönlerden,
- farklı bağlantı noktalarından,
- farklı sıralardan,
- farklı varyantlardan

oluşabilir.

Ancak her modül her modülle otomatik olarak uyumlu değildir.

Bu nedenle konfigürasyon oluşturulurken:

- geometrik uyumluluk,
- yön,
- bağlantı noktası,
- sıra,
- modül tipi,
- varyant,
- model kapsamı

gibi kurallar gerektiğinde domain tarafından doğrulanacaktır.

Bu kurallar yalnızca XAF Validation veya UI kontrollerine bırakılmayacaktır.

---

# 6. Modüller

Modüller bağımsız satılabilir olabilir.

Aynı modül:

- tek başına satılabilir,
- bir modelin parçası olabilir,
- farklı geçerli kombinasyonlarda kullanılabilir,
- farklı satış ve üretim bağlamlarında farklı anlamlar kazanabilir.

Modüller yalnızca belirli tek bir koltuğa fiziksel olarak bağlanmış basit alt kayıtlar olarak tasarlanmayacaktır.

---

# 7. Stock Configuration ve Production Configuration

Bu iki kavram özellikle ayrılacaktır.

## Stock Configuration

Stokta bulunan veya stok açısından yönetilen geçerli ürün/configuration yapısını ifade eder.

Stok kodu, miktar, rezervasyon, giriş/çıkış ve stok hareketleriyle ilişkisi burada değerlendirilir.

## Production Configuration

Üretime aktarılacak ürünün üretim açısından geçerli yapısını ifade eder.

BOM, revizyon, üretim rotası, komponentler ve üretim kuralları burada değerlendirilecektir.

İki konfigüratör ortak uyumluluk kurallarını kullanabilir.

Ancak yaşam döngüleri aynı kabul edilmeyecektir.

---

# 8. Satış ürünü ve üretim ürünü

Satış ürünü ile üretim ürünü aynı kavram olarak zorunlu şekilde modellenmeyecektir.

Aralarında:

- birebir,
- bire-çok,
- çok-bire

gibi ilişkilerin hangisinin geçerli olduğu business rule'lar netleştirilerek kararlaştırılacaktır.

Bu karar verilmeden kalıcı veri modeli oluşturulmayacaktır.

---

# 9. Stok mimarisi

Stok sistemi yalnızca `Urun` ve `Miktar` mantığıyla tasarlanmayacaktır.

Gelecekte aşağıdaki kavramlar ayrı sorumluluklar olarak ele alınabilir:

- Stok Kalemi
- Stok Deposu
- Stok Konumu
- Stok Hareketi
- Stok Rezervasyonu
- Stok Konfigürasyonu
- Stok Kartı
- Seri/Lot gibi takip kavramları
- Ölçü birimi
- Miktar
- Birim dönüşümü

Bunların tamamı ilk aşamada oluşturulmak zorunda değildir.

Önce gerçek business rule'lar belirlenmelidir.

## Kritik kural

Stok hareketi gibi tarihçe oluşturan kayıtların silinmesi, basit CRUD silme davranışı olarak ele alınmayacaktır.

Gerektiğinde:

- iptal,
- ters hareket,
- arşivleme,
- düzeltme hareketi

gibi domain yaklaşımları kullanılacaktır.

Base sınıfında tüm alt kayıtları otomatik silen genel bir `OnDeleting` yaklaşımı kullanılmayacaktır.

---

# 10. Konfigürasyon

Konfigürasyon:

- model,
- modül,
- varyant,
- uyumluluk,
- sıra,
- yön,
- bağlantı

gibi kavramların geçerli bir kombinasyonunu ifade edebilir.

Geçerli konfigürasyonun kim tarafından, hangi yaşam döngüsüyle ve hangi koşullarda onaylandığı henüz açık business rule'dur.

Bu karar verilmeden nihai konfigürasyon veri modeli oluşturulmayacaktır.

---

# 11. Varyant

Varyant ayrı bir kavramdır.

Varyantın hangi boyutlardan oluşacağı henüz tamamen kapatılmış değildir.

Örneğin:

- kumaş,
- renk,
- ölçü,
- malzeme,
- ayak tipi,
- kol tipi

gibi özelliklerin hangi seviyede tutulacağı business analysis ile belirlenecektir.

Varyant kavramı ürün, modül ve stok kavramlarıyla birbirine karıştırılmayacaktır.

---

# 12. Üretim

Üretim tarafında aşağıdaki konular tasarlanacaktır:

- BOM
- BOM revizyonu
- geçerlilik tarihi
- alternatif komponent
- fire
- rota
- operasyon
- üretim emri
- üretim girdileri
- üretim çıktıları

Make-to-stock, make-to-order ve dış tedarik senaryolarının hangilerinin kapsamda olacağı ayrıca kararlaştırılacaktır.

Üretim domaini stok domaininin basit bir uzantısı olarak tasarlanmayacaktır.

---

# 13. Satış

Satış tarafında:

- müşteri,
- teklif,
- sipariş,
- satış ürünü,
- fiyat,
- varyant,
- konfigürasyon,
- teslimat

arasındaki ilişkiler açıkça tanımlanacaktır.

Sipariş değişikliği veya iptalinin:

- üretim,
- satın alma,
- stok,
- rezervasyon,
- sevkiyat

üzerindeki etkileri ayrıca business rule olarak belirlenecektir.

---

# 14. Fiyatlandırma ve mali konular

Aşağıdaki konular kesinleştirilmeden mali hesaplama kodu yazılmayacaktır:

- ölçü birimi,
- fiyatlandırma,
- para birimi,
- vergi,
- yuvarlama,
- iskonto,
- maliyet,
- satış fiyatı,
- üretim maliyeti.

Türk vergi ve muhasebe mevzuatına uygunluk doğrulanmadan sistemin mevzuata tam uyumlu olduğu iddia edilmeyecektir.

---

# 15. Multi-tenancy

Tenant yalnızca ekran filtresi değildir.

Tenant izolasyonu:

- veri erişimi,
- sorgular,
- ilişkiler,
- güvenlik,
- raporlama,
- servisler

dahil tüm veri erişim yollarında güvence altına alınmalıdır.

Aşağıdaki seçenekler değerlendirilerek karar ADR ile kaydedilecektir:

1. Tenant başına veritabanı
2. Tenant başına şema
3. Paylaşımlı şema + tenant anahtarı

Tenantlar arasında hangi ana verilerin paylaşılabileceği ayrıca kararlaştırılacaktır.

Kullanıcının birden fazla tenantta çalışıp çalışamayacağı da business rule'dur.

---

# 16. Edition yaklaşımı

Basic ve Professional ortak çekirdeği kullanacaktır.

Edition farkı yalnızca menü gizleme şeklinde tasarlanmayacaktır.

Edition sınırları:

- yetenek,
- işlev,
- güvenlik,
- veri davranışı

açısından gerçek olmalıdır.

Enterprise özellikleri henüz tanımlı değilse oluşturulmayacaktır.

Basic kapsamındaki ön muhasebenin kesin sınırı ve Professional kapsamındaki basitleştirilmiş üretimin kesin sınırı ayrıca belirlenecektir.

---

# 17. Güvenlik

XAF Security System öncelikli olarak değerlendirilecektir.

Aşağıdaki konular ayrıca tasarlanacaktır:

- kullanıcı
- rol
- izin
- authentication
- parola
- oturum
- kilitleme
- MFA
- kurtarma
- audit
- tenant yetkisi
- edition yetkisi

Edition yetkisi ile rol yetkisi birbirine karıştırılmayacaktır.

---

# 18. Audit

İş kayıtlarında gerekli olması halinde aşağıdaki bilgiler tutulabilir:

- OlusturmaTarihi
- Olusturan
- SonGuncellemeTarihi
- SonGuncelleyen

Ancak XAF Audit Trail ile proje içi audit alanları aynı amaç için gereksiz biçimde iki ayrı mekanizma haline getirilmeyecektir.

Kaynak kod dosyalarının header bilgileri ile veritabanı audit bilgileri birbirinden farklıdır.

---

# 19. Base sınıf standardı

Mevcut Base klasöründeki sınıf isimleri aynen korunacaktır.

Örneğin:

- `BaseClass`
- `BaseClassWithAudit`
- `BaseClassWithDescription`
- `BaseClassWithAuditAndDescription`

Bu sınıf isimleri değiştirilmez.

Base sınıflarındaki yeni alan/property isimleri Türkçe olacaktır.

Base sınıfı tüm domain davranışlarını içine alan dev bir sınıf haline getirilmeyecektir.

Özellikle tüm çocuk kayıtlarını otomatik silen genel silme davranışı kullanılmayacaktır.

---

# 20. Türkçe kodlama standardı

Proje tarafından yazılan kodda:

- sınıf isimleri Türkçe,
- Business Object isimleri Türkçe,
- property isimleri Türkçe,
- metot/fonksiyon isimleri Türkçe,
- enum isimleri Türkçe,
- enum değerleri Türkçe,
- tablo isimleri Türkçe,
- kolon isimleri Türkçe

olacaktır.

Örnek:

```csharp
public class StokHareketi
{
    public DateTime HareketTarihi { get; set; }

    public decimal Miktar { get; set; }

    public void StokDus()
    {
    }
}
```

DevExpress, XAF, XPO ve .NET framework API isimleri değiştirilmez.

Örneğin:

- `XafApplication`
- `ObjectSpace`
- `Controller`
- `ViewController`
- `Validation`
- `Appearance`

gibi framework isimleri aynen kullanılabilir.

---

# 21. Kaynak dosyası header standardı

Her yeni C# sınıf dosyasında aşağıdaki mantıkta header bulunacaktır:

```text
/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : BaseClass.cs
 * Oluşturma Tarihi : ...
 * Oluşturan        : ...
 * Son Güncelleme   : ...
 * Son Güncelleyen  : ...
 * Açıklama         : ...
 * ****************************************************************************
 */
```

Dosya adı, oluşturma tarihi, oluşturan, son güncelleme ve son güncelleyen bilgileri gerçek değerleri yansıtmalıdır.

Elle uydurulmuş tarih veya kullanıcı bilgisi kullanılmayacaktır.

---

# 22. Fiziksel proje yapısı

Resmi XAF template çıktısı görülmeden tahmini katman/proje klasörleri oluşturulmayacaktır.

Mantıksal olarak:

- Domain
- Application
- Infrastructure
- UI

ayrımı korunabilir.

Ancak gereksiz fiziksel proje çoğaltılmayacaktır.

XAF'ın resmi template yapısı temel alınacaktır.

---

# 23. Dokümantasyon yapısı

Önerilen yapı:

```text
.roo/
  rules/
  rules-architect/

docs/
  architecture/
  decisions/
  domain/
  modules/
  ui-ux/

plans/

src/

tests/

ARCHITECTURE.md
DOMAIN_RULES.md
UI_GUIDELINES.md
CODING_RULES.md
DEVELOPMENT_WORKFLOW.md
PRODUCT_EDITIONS.md
```

`src` ve `tests` resmi solution oluşturulmadan önce yalnızca rezerv olabilir.

---

# 24. Architect çalışma biçimi

Architect:

1. Önce mevcut dosyaları ve mevcut durumu inceler.
2. Gerekli teknik ve business context'i toplar.
3. Belirsiz noktalar için kullanıcıya soru sorar.
4. Kararları açık biçimde ayırır.
5. Todo/plan oluşturur.
6. Yeni gereksinimler çıktığında planı günceller.
7. Plan onaylanmadan gerçek implementation'a geçmez.
8. Yeni business rule uydurmaz.
9. Eski ERP'deki davranışı otomatik olarak doğru kabul etmez.
10. Eski projeyi yalnızca gerektiğinde referans olarak kullanır.

---

# 25. Code çalışma biçimi

Code modu:

- onaylanan planı uygular,
- mimari kararları kendiliğinden değiştirmez,
- mevcut kararlarla çelişen bir durum bulursa durur,
- gerçek domain kuralı uydurmaz,
- XAF/XPO kararından sapmaz,
- DevExpress sürümünü değiştirmez,
- lokal paket kararını ihlal etmez,
- eski ERP'den izinsiz kod taşımaz.

---

# 26. İlk teknik kurulum

İlk teknik aşama:

1. .NET SDK doğrula.
2. DevExpress 26.1.3 paketlerini doğrula.
3. Lokal NuGet/paket kaynaklarını doğrula.
4. XAF 26.1.3 template/tooling yöntemini doğrula.
5. XAF Blazor + XPO başlangıç solution'ını resmi yöntemle oluştur.
6. PostgreSQL geliştirme bağlantısını hazırla.
7. Restore çalıştır.
8. Build çalıştır.
9. Uygulamayı başlat.
10. Minimum smoke test yap.
11. Sonuçları dokümante et.

Bu aşamada gerçek SofasisERP domain Business Object'leri oluşturulmaz.

---

# 27. İlk domain aşaması

Teknik altyapı doğrulandıktan sonra domain tasarımı şu sırayla ele alınmalıdır:

1. Ana veri ve organizasyon yapısı
2. Model
3. Modül
4. Varyant
5. Uyumluluk
6. Geçerli konfigürasyon
7. Stok
8. Stok hareketleri
9. Satış ürünü
10. Satış
11. Üretim ürünü
12. BOM
13. Üretim konfigürasyonu
14. Üretim
15. Satın alma ve tedarik
16. Sevkiyat

Bu sıra kesin business rule'lar ortaya çıktıkça güncellenebilir.

---

# 28. Açık business rule listesi

Aşağıdaki konular henüz kesin karar değildir:

- Modüller global katalogda mı, model kapsamında mı, yoksa iki seviyede mi?
- Sol/sağ yön nasıl tanımlanacak?
- Bağlantı noktaları nasıl tanımlanacak?
- Sıra ve geometrik uyumluluk nasıl tanımlanacak?
- Varyant boyutları nelerdir?
- Varyant hangi seviyede tutulacaktır?
- Geçerli konfigürasyonu kim onaylar?
- Tek modül satışında model/varyant bağlamı nasıl korunur?
- Hazır ve siparişe özel konfigürasyonların stok kodları nedir?
- Satış ürünü ile üretim ürünü arasındaki ilişki nedir?
- BOM ve revizyon kuralları nedir?
- Alternatif komponent ve fire nasıl yönetilir?
- Rota ve operasyonlar nasıl yönetilir?
- Make-to-stock kapsamda mı?
- Make-to-order kapsamda mı?
- Dış tedarik kapsamda mı?
- Sipariş değişikliği üretimi nasıl etkiler?
- Sipariş iptali stok ve üretimi nasıl etkiler?
- Ölçü birimi ve dönüşüm kuralları nedir?
- Fiyatlandırma ve yuvarlama kuralları nedir?
- Tenant sınırı nedir?
- Tenantlar arasında hangi ana veriler paylaşılır?
- Kullanıcı birden fazla tenantta çalışabilir mi?
- Basic ön muhasebe kapsamı nedir?
- Professional üretim kapsamı nedir?
- Edition yükseltme ve veri davranışı nasıl olacaktır?
- Authentication ve MFA politikası nedir?
- Audit ve veri saklama politikası nedir?

Bu sorular çözülmeden nihai domain veri modeli oluşturulmayacaktır.

---

# 29. Architect için kritik koruma kuralları

Architect şu varsayımları yapmayacaktır:

- Eski ERP'deki model doğrudur.
- Koltuk tek stok kartıdır.
- Her modül tek bir modele aittir.
- Her satış ürünü doğrudan üretim ürünüdür.
- Stock Configuration ve Production Configuration aynıdır.
- Tenant yalnızca bir kullanıcı filtresidir.
- Edition yalnızca menü gizlemektir.
- XAF'ın her özelliği özel servis gerektirir.
- Her Business Object için ayrı servis/interface gerekir.
- Tanımlanmamış Enterprise özellikleri oluşturulmalıdır.

---

# 30. Başarı kriteri

İlk hedef çalışan bir ERP değildir.

İlk hedef:

**Doğru teknik temel + doğru domain modeli + açık business rule'lar + kontrollü implementation.**

SofasisERP'nin uzun vadeli başarısı, hızlı şekilde çok sayıda Business Object oluşturmak yerine doğru kavramların doğru sorumluluklarla modellenmesine bağlıdır.

Architect ve Code bu belgeyi ortak referans kabul eder.

Bu belgede açıkça karar verilmemiş bir konu ortaya çıktığında sistem karar uydurmaz; konuyu açık karar olarak kullanıcıya sunar.


# 31. Dün yapılan tasarım görüşmelerinden korunacak ürün/stok modeli

## 31.1 Koltuk modeli ve üretilebilir modüller

SofasisERP'nin ürün yapısının merkezinde modüler koltuk yaklaşımı vardır.

Bir koltuk ürünü tek parça bir stok kartı olarak ele alınmayacaktır.

Temel mantık:

**Model → Üretilebilir Modüller → Geçerli Kombinasyon/Konfigürasyon → Satış/Üretim bağlamı**

Örnek bir model olarak Kiev düşünülebilir.

Kiev gibi bir modelde örneğin:

- tek kollu üçlü motorlu modül,
- köşe modülü,
- 75 cm sabit modül,
- 75 cm tek kollu recliner modül

gibi farklı üretilebilir modüller bulunabilir.

Bu örnek yalnızca kavramı açıklamak içindir; gerçek ürün kataloğunun tamamı olarak kabul edilmeyecektir.

## 31.2 Modül bağımsız satılabilir

Her modül bağımsız olarak satılabilmelidir.

Dolayısıyla modül yalnızca bir `KoltukModeli`nin altındaki pasif çocuk kayıt değildir.

Aynı modül:

- tek başına satılabilir,
- birden fazla modelde kullanılabilir,
- farklı kombinasyonlarda yer alabilir,
- farklı varyantlarla eşleşebilir.

Bu karar stok ve satış tasarımında korunmalıdır.

## 31.3 Modüllerden alternatif varyantlar

Modüller farklı şekillerde birleştirilerek alternatif geçerli ürünler oluşturabilir.

Örneğin aynı model altında farklı modül dizilimleri farklı konfigürasyonlar oluşturabilir.

Bu nedenle:

**Model ≠ Modül ≠ Kombinasyon ≠ Varyant ≠ Satış Ürünü ≠ Üretim Ürünü**

olarak korunmalıdır.

## 31.4 Uyumluluk

Her modül her modülle birleşemez.

Uyumluluk kuralları ileride aşağıdaki bilgileri kapsayabilir:

- sağ/sol yön,
- bağlantı noktası,
- modül tipi,
- modül sırası,
- geometrik uyumluluk,
- model kapsamı,
- varyant uyumluluğu.

Konfigüratör geçersiz kombinasyonları oluşturmayı engellemelidir.

Uyumluluk kurallarının tek bir domain kaynağından yönetilmesi ve hem stok hem üretim konfigüratörlerinin aynı kuralları kullanması esastır.

---

# 32. Stok ekranı ve kullanıcı deneyimi

Stok tarafında kullanıcı deneyimi ERP'nin temel tasarım kriterlerinden biridir.

## 32.1 Stok kartı

Stok kartı mümkün olduğunca tek ana görünümde anlaşılabilir olmalıdır.

Kritik bilgiler için gereksiz scroll gerektiren ekranlar oluşturulmayacaktır.

Gerekirse sekmeler kullanılabilir; ancak kullanıcının temel stok durumunu görmek için sürekli ekran değiştirmesi veya uzun sayfalarda gezinmesi gerekmemelidir.

## 32.2 Stok özet paneli

Stok kartının sağ tarafında veya uygun sabit bir özet alanında en azından:

- Mevcut
- Rezerve
- kullanılabilir/serbest stok

gibi kritik stok durumu hızlıca görülebilmelidir.

Bu alanın amacı kullanıcının stok kararını tek bakışta verebilmesidir.

## 32.3 Model ve uyumlu modüller

Örneğin Kiev modeli incelenirken aynı çalışma bağlamında mümkün olduğunca:

- modele uygun modüller,
- kumaş grupları,
- ilgili fiyatlar

görülebilmelidir.

Kullanıcı sırf uyumlu modülü bulmak için farklı ekranlar arasında gereksiz gezinmemelidir.

Bu yaklaşım yalnızca görsel bir tercih değildir; domain ilişkilerinin kullanıcıya doğru yansıtılmasıdır.

---

# 33. Konfigüratörlerin kullanıcı davranışı

Sistem kullanıcıya geçersiz kombinasyon seçtirmemeye çalışmalıdır.

Kullanıcı bir modül seçtiğinde sistem mümkün olan sonraki seçenekleri bağlama göre daraltabilir.

Konfigürasyon ekranı:

1. Modeli,
2. seçilebilir modülleri,
3. uyumluluk durumunu,
4. varyant seçeneklerini,
5. fiyat etkisini,
6. oluşan geçerli yapıyı

anlaşılır şekilde göstermelidir.

Ancak UI'nın yaptığı filtreleme tek başına güvenlik veya domain bütünlüğü mekanizması değildir.

Aynı kurallar server/domain seviyesinde de korunmalıdır.

---

# 34. Stok ile üretim arasındaki akış

Stok konfigürasyonu ile üretim konfigürasyonu aynı nesne gibi düşünülmemelidir.

Önerilen düşünsel akış:

**Model**
→ **Modül Seçimi**
→ **Geçerli Kombinasyon**
→ **Stok/Satış Bağlamı**
→ **Üretim Bağlamı**

Satışta oluşturulan bir yapı üretime aktarılabilir; ancak üretim tarafının kendi kuralları, revizyonu ve üretilebilirlik koşulları olabilir.

Bu nedenle satışta geçerli olan her kombinasyonun otomatik olarak üretilebilir olduğu varsayılmayacaktır.

---

# 35. Satış açısından modüler yapı

Bir müşteri yalnızca komple koltuk değil, tek bir modül de satın alabilir.

Bu nedenle satış sistemi:

- tek modül satışı,
- birden fazla modülden oluşan konfigürasyon satışı,
- hazır/geçerli ürün,
- siparişe özel konfigürasyon

senaryolarını birbirine karıştırmadan desteklemelidir.

Tek modül satışında model ve varyant bağlamının nasıl korunacağı henüz açık business rule'dur; Architect bunu varsayarak kapatmamalıdır.

---

# 36. Hazır ve siparişe özel konfigürasyon

İki farklı kullanım senaryosu ayrıca düşünülmelidir:

### Hazır konfigürasyon

Önceden tanımlanmış, geçerli ve stoklanabilir bir yapı.

### Siparişe özel konfigürasyon

Müşteri siparişi sırasında oluşturulan ve üretim/satış sürecine bağlanan yapı.

Bu iki yapının stok kodu, yaşam döngüsü ve rezervasyon davranışı aynı olmak zorunda değildir.

---

# 37. Fiyatın konfigürasyonla ilişkisi

Modül ve varyant seçiminin satış fiyatına etkisi olabilecektir.

Örneğin:

- modül,
- motor,
- recliner,
- kumaş grubu,
- diğer varyantlar

fiyatı değiştirebilir.

Fiyat hesaplama mekanizması henüz kesin business rule değildir.

Ancak fiyatın yalnızca UI üzerinde hesaplanan geçici bir değer olarak bırakılmaması gerektiği mimari olarak korunmalıdır.

---

# 38. Stok miktarı ile konfigürasyon ilişkisi

Konfigürasyonlu ürünlerde stok miktarı yalnızca ana model seviyesinde tutulmamalıdır.

Gerçek stok biriminin hangi seviyede olduğu açıkça belirlenmelidir.

Özellikle:

- modül stoğu,
- hazır konfigürasyon stoğu,
- siparişe özel konfigürasyon,
- üretimden çıkacak ürün

arasındaki fark korunmalıdır.

Bu ayrım netleşmeden tek bir `StokKalemi` tablosuna bütün kavramların doldurulması önerilmez.

---

# 39. Ürün yaşam döngüsü

Bir ürün kavramının:

**Tanımlandı → Geçerli → Satılabilir → Stoklanabilir → Üretilebilir → Pasif**

gibi yaşam döngülerinden hangilerini destekleyeceği ayrıca belirlenecektir.

Bir kaydın `AktifMi` olması, onun otomatik olarak:

- satılabilir,
- üretilebilir,
- stoklanabilir

olduğu anlamına gelmeyecektir.

---

# 40. Mimari tasarımın ana prensibi

SofasisERP'de amaç en fazla sayıda tablo veya sınıf üretmek değildir.

Amaç:

**Gerçek iş kavramlarını doğru ayırmak, modüler ürün yapısını korumak, stok ve üretim yaşam döngülerini ayırmak ve kullanıcıya bu karmaşıklığı mümkün olduğunca sade bir deneyimle sunmaktır.**

Architect herhangi bir noktada teknik olarak kolay olduğu için business kavramlarını birleştirmemelidir.

Özellikle şu yaklaşım reddedilir:

> Her şeyi tek Product sınıfında tut, varyantları alan olarak ekle, stok miktarını Product üzerinde tut ve üretimi aynı kayıt üzerinden yürüt.

Bunun yerine kavramların gerçek yaşam döngüleri ve sorumlulukları analiz edilmelidir.

---

# 41. Eski ERP'nin kullanım biçimi

Eski ERP gerektiğinde referans olarak kullanılabilir.

Ancak eski kod veya veri modeli:

- otomatik olarak doğru kabul edilmez,
- yeni projeye birebir taşınmaz,
- yeni domain kararlarının yerine geçmez.

Eski sistem özellikle:

- geçmişteki iş kurallarını anlamak,
- kullanıcı alışkanlıklarını görmek,
- eksik business rule'ları keşfetmek

için kullanılabilir.

Yeni SofasisERP'nin mimarisi yeni proje kararlarıyla oluşturulacaktır.

---

# 42. İlk gerçek domain karar kapısı

Teknik XAF/XPO altyapısı doğrulandıktan sonra ilk büyük domain karar kapısı **ürün + modül + uyumluluk + konfigürasyon + stok ilişkisi** olacaktır.

Bu kapı kapanmadan:

- nihai stok tabloları,
- stok hareketleri,
- satış ürünü,
- üretim ürünü,
- BOM

için kalıcı model oluşturulmayacaktır.

Architect önce kavramları ve ilişkileri görselleştirecek, açık business rule'ları listeleyecek ve kullanıcı onayını alacaktır.

---

# 43. Kullanıcı deneyimi için bağlayıcı prensip

SofasisERP'nin karmaşık domain modeli kullanıcıya karmaşık bir ekran olarak yansıtılmamalıdır.

Öncelik:

- az tıklama,
- gereksiz ekran geçişlerinin azaltılması,
- kritik bilgilerin aynı bağlamda gösterilmesi,
- geçersiz seçeneklerin mümkün olduğunca erken elenmesi,
- stok durumunun hızlı anlaşılması,
- model/modül/varyant/fiyat ilişkisinin tek çalışma bağlamında görülebilmesi

olacaktır.

Özellikle stok ve konfigürasyon ekranlarında bu prensip tasarım kriteridir.

---

# 44. Son karar

Bu dokümandaki kesin teknik kararlar uygulanabilir.

Açık business rule'lar ise Architect tarafından kapatılmadan gerçek Business Object tasarımına dönüştürülmeyecektir.

Her yeni karar:

1. problem,
2. seçenekler,
3. tercih edilen yaklaşım,
4. gerekçe,
5. etkilediği domain,
6. etkilediği veri modeli,
7. kullanıcı etkisi

şeklinde değerlendirilip gerektiğinde ADR olarak kaydedilecektir.

---

# 45. İlk Domain Karar Kapısının Kapanışı (2026-08-17)

Bu bölüm, §42'de tanımlanan "ilk gerçek domain karar kapısı"nın (ürün + modül + uyumluluk + konfigürasyon + stok ilişkisi) 2026-08-17'de kullanıcıyla yapılan kapsamlı bir tasarım oturumu sonucunda nasıl kapandığını kayıt altına alır. Edition stratejisi için bkz. `Brain/Mimari/PRODUCT_EDITIONS.md`; DetailView/servis konvansiyonları için bkz. `Brain/Mimari/00_DetailView_ve_Servis_Konvansiyonlari.md`.

## 45.1 Hedef sektör

Uygulama tamamen **koltuk üreten firmalar** için tasarlanır (genel mobilya değil, spesifik olarak koltuk üreticileri).

## 45.2 Kavramsal domain modeli

Nedensel sıralama: Model tanımlanır → üretilir → reçetesi çıkarılır → maliyet hesaplanır → satış fiyatı belirlenir → satışa açılır → sipariş/satınalma döngüsü bu temel üzerinde işler.

1. **Model** — paylaşılan modül havuzunun sahibi olan üst çatı (ör. "Kiev"). **Kendisi doğrudan satılmaz.** `ModelAdi`, `ModelKodu`, `Resim`.
2. **Modül** — atomik üretilebilir/satılabilir birim, bağımsız satılabilir (§6). `ModulTipi`: TekKollu(Sol/Sağ), Köşe, Sabit, Kutu (ses/LED içerebilir), Üçlü, Berjer/Tekli; Ölçü (75/85cm).
3. **Mekanizma** — ayrı bir "Tanım" varlığı DEĞİL; fiziksel bir bileşen/hammadde olarak `StokTanim` + üç seviyeli Stok Tipi/Grup/Alt Grup hiyerarşisiyle çözülür (bkz. §45.4). Üretim/Reçete katmanında (Enterprise) bir StokKalemi; Basic/Pro'da (Reçete yok) mekanizma farkı zaten farklı StokKalemi/Konfigürasyon kaydı (farklı stok kodu/fiyat) olarak var olur.
4. **Konfigürasyon** — **fiilen satılan birim**. Model'in paylaşılan modül havuzundan belirli bir dizilim/altküme kullanır, ama katalogda kendi adı/fiyatı/stok koduyla ayrı bir ürün gibi görünür (ör. "Kiev Takım", "Kiev Köşe"). `FormTipi`: Köşe/Takım — Konfigürasyon'un bir alanıdır, ayrı bir varlık DEĞİLDİR (bkz. §45.5 karar geçmişi). Uyumluluk kuralları (§31.4) burada uygulanır; StokKonfigürasyonu ≠ ÜretimKonfigürasyonu (§7).
5. **Stok Kalemi** — hem atomik Modül hem Konfigürasyon seviyesinde var olur; üç seviyeli hiyerarşiyle sınıflanır (bkz. §45.4). `Resim` alanı **opsiyoneldir** (kullanıcı kararı, 2026-08-17 — `StokTanim.cs` içindeki `Resim` alanı zorunlu tutulmadı, bkz. kod yorumu). `StokTipi=Mamul` ise `Model` referansı zorunludur (Sipariş ekranında modele göre filtreleme/arama için).
6. **Reçete (BOM)** — Model/Modül **üretildikten SONRA** çıkarılır; bu keyfi bir tercih değil zorunluluktur: model ilk üretilmeye başladığında hangi malzemeden ne kadar kullanılacağı henüz bilinmez. İlk üretim reçetesiz/provizyon olarak başlar → gerçek malzeme tüketimi kayıt altına alınır → bu kayıtlardan resmi Reçete türetilir. Satır türü: Jenerik hammadde (ör. standart tabaka sünger, dansiteye göre) / Modele özel bileşen (ör. özel kesim sünger) — §12 "alternatif komponent". **Açık nokta:** ilk (provizyon) üretimdeki tüketimin teknik kayıt mekanizması henüz netleşmedi (Faz 6/Üretim öncesi kapatılacak).
7. **Maliyet & Satış Fiyatı** — maliyet reçeteden hesaplanır, satış fiyatı maliyetten türetilir.
8. **Sipariş** — Kaynak: Yurtiçi/Yurtdışı; Amaç: Stok/Tamir (tamir siparişinin Model→Reçete→Maliyet zincirine bağlanışı henüz netleşmedi); Kanal: WhatsApp/Mail/Mağaza. Yurtdışı siparişte kapsamlı bir proforma süreci devreye girer. `Resim` alanı zorunlu (özellikle köşede özel ölçü isteyen müşteri için) + özel ölçü girişi desteklenir.

**Satış birimi esnekliği:** Her modül tek başına stok kalemi olarak satılabilir, ya da bir konfigürasyon/set içinde satılabilir (§6, §35) — hammaddeye kadar genişler (ör. satın alınan kumaş doğrudan satılabilir; reçetede standart 4 kırlent varken müşteri ekstra kırlent isteyebilir).

**Ayak** — koltuk siparişinde önemli bir belirteç: `AyakTipi` (Ahşap/Metal), `KasaliMi` (oturum altında saklama kasası var/yok), `AyakRengi`. Mekanizma'nın aksine kendi `AyakTanim` varlığı olarak kalır (kullanıcı kararı).

## 45.3 Satınalma / Malzeme Tedarik Akışı

Malzemeler genelde bir **fiş** (mal kabul belgesi) ile gelir. Ay sonunda tedarikçi bu fişlerle ilgili toplu fatura keser; bazen doğrudan fatura ya da irsaliye gönderir — üç senaryo: (1) fiş → sonradan aylık toplu fatura, (2) doğrudan fatura, (3) doğrudan irsaliye. **Stok girişi FATURAYA değil, FİŞ'in sisteme işlenmesine bağlıdır.** `StokHareketi`'nin kaynak belgesi Fatura değil, Fiş/İrsaliyedir; fatura ayrı zamanlı, ayrı bir mali belgedir (bir fatura birden fazla fişi kapsayabilir).

## 45.4 Stok Kodlama (üç seviyeli, Tekdüzen STİLİNDE — gerçek hesap planına bağlı değil)

`StokTipiTanim` → `StokGrupTanim` → `StokAltGrupTanim`, üçü de TABLO (enum değil — muhasebe tarafından yönetilebilir olması için). Her seviyenin `Kod` alanı **kümülatif/tam kodu** taşır:

- `StokTipiTanim.Kod` = "150" (ör. Hammadde)
- `StokGrupTanim.GrupKodu` = "150.01" (ör. Sünger)
- `StokAltGrupTanim.AltGrupKodu` = "150.01.01" (ör. "30 Dansite Sünger")
- `StokKodu` = `AltGrupKodu + "." + SiraNumarasi` (4 haneli) → ör. **"150.01.01.0001"**

Bu, **Tekdüzen Hesap Planı'nın noktalı-hiyerarşik numaralandırma STİLİNDEN** esinlenir ama gerçek muhasebe hesap planına BAĞLANMAZ (Tekdüzen çok kaba taneli — SKU seviyesinde ayrım gücü yok; muhasebe/Tekdüzen entegrasyonu kapsamı [[project_faz5_muhasebe_kapsam_karari]] ayrıca, mali müşavire sorularak netleştirilecek).

`StokTipiTanim`/`StokGrupTanim`/`StokAltGrupTanim` tanımlama ekranları **yalnızca muhasebe rolüne açıktır** — genel "basit kullanıcı" arayüz kısıtının istisnasıdır.

**Ertelenen fikir:** Her stok grubu için dinamik "Stok Özellik" seti + konfigüratörle kod üretme fikri değerlendirildi, ilk sürüm için ertelendi (Faz 1'i hızlı bitirmek, henüz kanıtlanmış ihtiyaç olmaması) — sabit hiyerarşinin üzerine ileride opsiyonel bir katman olarak eklenebilir, geriye dönük kırılma riski düşük.

## 45.5 Form (Köşe/Takım) — karar geçmişi

İlk tasarımda Form, Model'in altında ayrı bir varlık (`ModelFormu`) olarak önerildi. Kullanıcının somut Kiev örneği (aynı 85cm oturum modülünün hem Takım'da hem sağ kolu çıkarıp köşeye eklenen dizilimde kullanılması — modüller formlar arası paylaşılıyor, her form kendi modül listesini sahiplenmiyor) üzerine bu karar geri alındı. **Nihai karar: Form ayrı bir varlık DEĞİL — Konfigürasyon'un bir alanı (`FormTipi`: Köşe/Takım)** (bkz. §45.2 madde 4).

## 45.6 Finans / Ön Muhasebe

- **Ödeme/tahsilat seçenekleri:** Nakit, Kredi Kartı, Kredi Kartına Taksit, Çek/Senet (`OdemeSekli` enum).
- **Cari hesap ekstre/tahsilat döngüsü (B2B):** Ayın 1-30'u arasında ürün alan firmaya ayın 2-4'ü arasında ekstre gönderilir, 7-10'u arasında anlaşılan şartlara göre ödeme istenir — periyodik bir süreç olarak modellenir.
- **E-posta entegrasyonu (sistem geneli zorunlu):** Sistemde tanımlı müşteri/tedarikçilere otomatik mail atılabilmesi gerekir (ekstre, proforma, sipariş onayı gibi süreçlerin altyapısı) — merkezi bir `IEPostaGonderimServisi`, ilgili modüller bunu çağırır.

## 45.7 Ekran ve Menü Standardizasyonu

En temel kural: standart ekran tip ve şablonları — bir ekranı/menüyü anlayan tümünü anlamalı (hedef kullanıcı profili ilkokul/ortaokul/lise mezunu). Ekranlar DevExpress XAF Blazor'un native ListView/DetailView/Popup/Report yapıları temel alınarak standardize edilir; genel/soyut UI kuralları XAF'ın kendi bileşen davranışının üstüne gereksiz özel katman olarak eklenmez. Navigasyon: her iş alanı yalnızca standart "Tanımlar" ve "Hareketler" alt-gruplarını kullanır. DetailView sekme deseni ve numaralandırma servisi için bkz. `Brain/Mimari/00_DetailView_ve_Servis_Konvansiyonlari.md`.

İstisna (2026-08-20, kullanıcı kararı): Kasa Yönetimi, Banka Yönetimi ve Finans (Cari Hesap Yönetimi) HER BİRİ kendi üst-seviye navigasyon grubuna sahiptir (aynı "Tanımlar"/"Hareketler" alt-grup deseni her birinin İÇİNDE tekrar eder) — bu, "yeni ekran kendi üst-grubunu açmasın" genel kuralının bilinçli bir istisnasıdır. **Güncelleme (2026-08-21):** Çek Senet Yönetimi kararı geri alınmıştır — Çek/Senet modülü (tanım+hareket) 2026-08-20'de kullanıcı kararıyla TAMAMEN kaldırılmış, kod tabanında hiç yoktur (bkz. §45.8, güncel mimari) ve şu an aktif bir plan/faz kapsamında değildir.

## 45.8 Kasa/Banka/Cari — Tek Düz Hareket Mimarisi (`KasaCariBankaHareketleri`) [2026-08-21 GÜNCEL]

> **Not:** Bu bölüm daha önce (§45.8, eski hali) Master-Detail bir `CariKasaBankaHareketM`/`D` çifti, ayrı `CekSenetHesabi`/`AcilisDengeHesabi` sınıfları, `CariKasaBankaEkSatir` ("Diğer Cariler") ve DevExpress ReportsV2 tabanlı raporları anlatıyordu. Bunların HİÇBİRİ artık kod tabanında yoktur — 2026-08-20'de kullanıcı kararıyla tamamen kaldırılıp yerine burada anlatılan, daha basit tasarım getirilmiştir. Aşağıdaki metin GÜNCEL ve GEÇERLİDİR.

Kasa, Banka ve Cari hesap hareketleri TEK bir düz sınıfta (`KasaCariBankaHareketleri`) tutulur — Master/Detay (Fiş/Satır) AYRIMI YOKTUR, her kayıt kendi başına tam bir harekettir. Gerçek Tekdüzen Hesap Planı mantığında Kasa/Banka/Alıcılar-Satıcılar hepsi birer "Hesap"tır; `KasaTanim`, `BankaHesabiTanim`, `CariHesapTanim` ortak bir taban sınıftan (`Hesap`, XPO Class Table Inheritance) türer.

**Hesap taban sınıfı ve `HesapTuru` ayırt edicisi.** Üç somut alt sınıf (`KasaTanim`/`BankaHesabiTanim`/`CariHesapTanim`) `Hesap`'tan türer; her biri kendi `AfterConstruction()`'ında sabit bir `HesapTuru` (enum: yalnızca Kasa/Banka/Cari — Çek/Senet kaldırıldığından o değerler yok) atar. Bu alan iki işi birden görür: (a) `KaynakHesap`/`KarsiHesap` lookup'larının `DataSourceCriteria`'sı, (b) 12 modül×işlem-türü ListView'inin çapraz-görünürlük kriteri: `[KaynakHesap.HesapTuru] = 'Kasa' Or [KarsiHesap.HesapTuru] = 'Kasa'` gibi — HANGİ EKRANDAN girildiğine değil, satırın GERÇEKTEN hangi Hesap türünü etkilediğine bakar. Kasa'dan Banka'ya bir Virman, hangi taraftan girilirse girilsin TEK kayıt olarak hem "Kasa Virman" hem "Banka Virman" listesinde görünür — ayna/ikinci fiziksel satır YOK (eski ERP'nin ihtiyaç duyduğu karmaşıklık, ortak `Hesap` tabanı sayesinde gereksiz).

**Sade giriş / iş-dili sözleşmesi.** `KaynakHesap`/`KarsiHesap`/`BorcTutar` (TEK görünen tutar alanı — `AlacakTutar` her zaman gizli/otomatik) sade alanlardır; Kasa/Banka/Cari × Açılış/Tahsilat/Ödeme/Virman = 12 ekranın her biri bunlardan yalnızca ilgilisini, iş-dilinde başlıklarla (ör. "Tahsil Edilen Tutar", "Kimden Tahsil Edildi") gösterir. Sabit konvansiyon: `KaynakHesap` HER ZAMAN Borç tarafı, `KarsiHesap` HER ZAMAN Alacak tarafı — hangi FİZİKSEL hesabın bu rolü oynadığı ekrana/işlem tipine göre değişir (ör. Tahsilat'ta Kasa=Kaynak, Ödeme'de Kasa=Karşı). **İstisna:** Cari Açılış Fişi'nde seçilen Cari `Tedarikçi` ise (`CariHesapTipi.Tedarikci`), motor Kaynak/Karşı'yı otomatik ters çevirir (`KasaCariBankaHareketleri.ObjectSaving()`) — aksi halde Tedarikçi'nin (alacaklı) açılış bakiyesi yanlış yönde işlenir.

**Kayıt sonrası düzenleme kilidi.** `FisTarihi`, `FisTuruTanim`, `KaynakHesap`, `KarsiHesap`, `BorcTutar` kaydedildikten sonra kilitlenir (`Appearance Criteria="!(IsNewObject(this))"`, StokHareketleriM/StokTransferi ile aynı kanıtlanmış desen) — bir hareketin tutarı/hesabı SONRADAN değiştirilemez, yalnızca SİLİNİP yeniden girilebilir. Bu, bakiye motorunun (`ObjectSaving`/`ObjectDeleting`, `MotorIslendi` bayrağı ile tek-seferlik) her zaman doğru/simetrik çalışmasını garanti eder (4-ajanlı testte bulunan "düzenlemede bakiye bozuluyor" hatasının kökten çözümü).

**Açılış Dengesi sistem hesabı.** Açılış Fişi ekranlarında Karşı Hesap serbest DEĞİLDİR — sabit, tek bir sistem hesabına (`CariHesapTanim`, `CustomCode1 = "ACILIS_DENGE"`, `DatabaseSeeder.SeedAcilisDengeHesabi()`) filtrelenir. Böylece açılış bakiyeleri gerçek ticari Cari hesaplarına karışmaz.

**Mükerrer kayıt kontrolü.** Aynı gün, aynı Fiş Türü, aynı iki hesap, aynı tutarla başka bir hareket zaten varsa `OnSaving()` kaydı reddeder (`UserFriendlyException`) — aynı işlemin yanlışlıkla iki farklı ekrandan (ör. hem Kasa Tahsilat hem Cari Tahsilat) girilmesine karşı bir güvenlik ağı.

**Ödeme Şekli.** Yalnızca Cari ekranlarında (Nakit/Kredi Kartı/Havale-EFT) gösterilir — Kasa/Banka ekranlarında zaten hangi hesabın kullanıldığı belli olduğundan gizlenir.

**Raporlama — açık nokta.** Hareket Fişi Makbuzu / Cari Hesap Ekstresi gibi raporlar bu yeni mimaride HENÜZ YOKTUR (eski ReportsV2 tabanlı raporlar Çek/Senet'le birlikte kaldırıldı) — ayrı bir iş kalemi olarak planlanmalı. Tutar/para birimi her zaman `KaynakHesap`/ilgili `Hesap`'ın KENDİ döviz cinsindendir (ham `BorcTutar`/`AlacakTutar`); `Hesap.GuncelBakiye` ise her zaman TL karşılığıdır (`YerelBorcTutar`/`YerelAlacakTutar`) — bu ikisi FARKLI birimlerdir, karıştırılmamalıdır.

**Bilinçli kapsam dışı bırakılanlar:** Farklı döviz cinsli hesaplar arası hareket için otomatik kur farkı satırı üretimi (kullanıcı tutarları manuel eşitler); Çek/Senet entegrasyonu; KDV/stopaj; "Diğer Cariler" (tek Kasa/Banka + çoklu Cari satırı) deseni.
