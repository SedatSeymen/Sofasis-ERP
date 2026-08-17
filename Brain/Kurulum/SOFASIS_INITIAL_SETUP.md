# SofasisERP — Initial Project Setup

## GÖREV TÜRÜ

Bu bir başlangıç proje kurulumu ve teknik ortam hazırlama görevidir.

Amaç:

1. Geliştirme bilgisayarındaki mevcut ortamı incelemek.
2. Lokal DevExpress paketlerini doğrulamak.
3. DevExpress XAF 26.1.3 ortamını doğrulamak.
4. XPO + PostgreSQL hedefini yapılandırmak.
5. SofasisERP workspace klasör yapısını oluşturmak.
6. Gerekli proje kurallarını ve dokümantasyonu oluşturmak.
7. Resmi XAF tooling/template kullanarak başlangıç XAF Blazor solution'ını oluşturmak.
8. Solution'ın restore/build durumunu doğrulamak.
9. Kurulum sonucunu belgelemek.

Bu görev tamamlandığında gerçek SofasisERP business domain geliştirmesine henüz başlanmayacaktır.

---

# 1. BAĞLAYICI TEKNİK KARARLAR

Aşağıdaki kararlar kesinleşmiştir.

### Framework

DevExpress XAF Blazor

### DevExpress

**26.1.3**

### ORM / Persistence

**DevExpress XPO**

EF Core persistence olarak kullanılmayacaktır.

### Database

**PostgreSQL**

### Paket kaynağı

Öncelikle bilgisayarda mevcut olan **lokal DevExpress paketleri** kullanılacaktır.

DevExpress paketleri için:

- başka sürüm arama
- latest sürüm kullanma
- otomatik upgrade yapma
- farklı DevExpress sürümlerini karıştırma
- kullanıcı onayı olmadan internetten başka DevExpress sürümü indirme

YASAKTIR.

Gerekli 26.1.3 paketleri lokal ortamda bulunamıyorsa DUR ve kullanıcıya bildir.

---

# 2. ÇALIŞMA ALANI

Mevcut workspace:

`SofasisERP`

Bu workspace yeni SofasisERP projesinin başlangıç alanıdır.

Eski/legacy ERP proje veya kaynakları bu görev kapsamında kullanılmayacaktır.

Eski ERP'yi:

- inceleme
- kopyalama
- import etme
- referans alma
- kod taşıma

YAPMA.

Eski ERP ileride gerektiğinde ayrıca referans olarak incelenecektir.

---

# 3. DEĞİŞİKLİK ÖNCESİ KURAL

İlk olarak mevcut workspace'i incele.

Herhangi bir dosya değiştirmeden önce:

- mevcut dosyaları listele
- mevcut solution/project dosyalarını kontrol et
- mevcut git durumunu kontrol et
- mevcut NuGet yapılandırmasını kontrol et
- mevcut DevExpress paket kaynaklarını kontrol et
- mevcut .NET SDK'larını kontrol et
- mevcut PostgreSQL kurulumunu kontrol et

Sonuçları anlamadan mevcut dosyaları silme veya üzerine yazma.

---

# 4. ENVIRONMENT ANALİZİ

Aşağıdakileri gerçek sistemden doğrula.

## .NET

Kontrol:

```text
dotnet --info
dotnet --list-sdks
dotnet --list-runtimes
```

Beklenen:

- XAF 26.1 ile uyumlu, desteklenen en güncel .NET SDK'nın (tercihen .NET 10) kurulu olması.
- Uygun SDK yoksa DUR ve kullanıcıya bildir.

## NuGet

Kontrol:

```text
dotnet nuget list source
```

Doğrula:

- Lokal DevExpress paket kaynağı (klasör veya lokal feed) tanımlı mı?
- Yanlışlıkla farklı DevExpress sürümü içerebilecek kaynaklara otomatik düşme riski var mı?

Not: Lokal paketler önceliklidir. `nuget.org` üzerinden DevExpress indirmek yalnızca kullanıcı onayıyla ve yalnızca 26.1.3 sürümü için yapılabilir.

## DevExpress 26.1.3 Lokal Paketleri

Kontrol:

```text
:: Unified Component Installer varsayılan kurulum dizini
dir "C:\Program Files\DevExpress 26.1"
dir "C:\Program Files (x86)\DevExpress 26.1"

:: Yaygın lokal NuGet paket konumları
dir "%USERPROFILE%\.nuget\packages\devexpress.expressapp*" 2>$null
```

Doğrula:

- DevExpress 26.1.3 bileşenleri (XAF, XPO, Blazor) lokal ortamda mevcut mu?
- Gerekli 26.1.3 paketleri lokal ortamda bulunamıyorsa DUR ve kullanıcıya bildir. Başka sürüm arama veya indirme yapma.

## PostgreSQL

Kontrol:

```text
psql --version
```

veya servis durumu:

```text
Get-Service -Name postgresql*
```

Doğrula:

- PostgreSQL sunucusu kurulu ve erişilebilir mi?
- Geliştirme için kullanılacak veritabanı/kullanıcı bilgileri belli mi? (Bilinmiyorsa kurulum aşamasında kullanıcıdan istenecek.)

---

# 5. KLASÖR YAPISI

Aşağıdaki başlangıç klasör yapısını workspace içinde oluştur:

```text
SofasisERP/
├── src/                    # XAF solution ve projeler buraya oluşturulacak
├── docs/                   # Proje dokümantasyonu
│   ├── architecture/       # Mimari kararlar ve diyagramlar
│   └── setup/              # Kurulum kayıtları ve ortam raporları
├── scripts/                # Yardımcı betikler (opsiyonel, gerektiğinde)
├── tests/                  # Test projeleri (ileride)
├── .gitignore              # .NET + Visual Studio + VS Code için standart gitignore
├── NuGet.config            # Lokal DevExpress kaynağını içeren NuGet yapılandırması
└── README.md               # Proje tanıtımı ve geliştirme başlangıç bilgileri
```

Kurallar:

- XAF solution `src/SofasisERP` altında oluşturulacaktır.
- Business domain kodu bu görevde yazılmayacaktır.
- Klasör yapısı dışına çıkma; gereksiz dosya oluşturma.

---

# 6. PROJE KURALLARI VE DOKÜMANTASYON

Oluştur:

1. **README.md** — Proje amacı, teknoloji yığını (XAF Blazor, XPO, PostgreSQL, .NET), geliştirme ön koşulları, çalıştırma talimatları.
2. **.gitignore** — Standart .NET/Visual Studio/VS Code gitignore şablonu.
3. **NuGet.config** — Lokal DevExpress paket kaynağını içeren yapılandırma. Lokal kaynak yolu doğrulanmadan yazma.
4. **Brain/Kurulum/INITIAL_SETUP_REPORT.md** — Bu kurulumun sonunda doldurulacak ortam ve doğrulama raporu.

Kurallar:

- Dokümantasyon gerçek sistem durumuyla tutarlı olmalıdır.
- Doğrulanmamış bilgi "doğrulanmış" gibi yazılmayacaktır.
- Türkçe yaz.

---

# 7. XAF BLAZOR SOLUTION OLUŞTURMA

Resmi DevExpress .NET CLI template'i kullanılacaktır. Elle proje dosyası oluşturma.

## Ön Koşul

XAF proje template'lerinin kurulu olduğunu doğrula:

```text
dotnet new list dx.xaf
```

Kurulu değilse:

```text
dotnet new install DevExpress.XAF.ProjectTemplates::26.1.3
```

Lokal ortamda 26.1.3 template paketi yoksa ve internetten indirilmesi gerekiyorsa kullanıcıdan onay al.

## Solution Oluşturma

`src` klasöründe çalıştır:

```text
dotnet new dx.xaf -n SofasisERP -p Blazor -orm XPO -db PostgreSql
```

Doğrulama (oluşturmadan önce `--dry-run` ile önizleme yapılabilir):

```text
dotnet new dx.xaf -n SofasisERP -p Blazor -orm XPO -db PostgreSql --dry-run
```

Beklenen çıktı:

- `SofasisERP.Module` — platformdan bağımsız modül projesi (XPO business object'ler burada tanımlanacak).
- `SofasisERP.Blazor.Server` — ASP.NET Core Blazor Server uygulama projesi.

Kurallar:

- Yalnızca Blazor platformu oluşturulacaktır. WinForms, Web API, Middle Tier, e2e test projesi ekleme.
- Varsayılan güvenlik (Password authentication) template ne üretiyorsa o kalacaktır; bu görevde güvenlik mimarisi değiştirilmeyecektir.
- `-dbu` (database update) parametresi template varsayılanında bırakılacaktır; production davranışı ileride ayrıca kararlaştırılacaktır.
- Template parametrelerinden emin olunamazsa önce `dotnet new dx.xaf -h` ile kurulu template'in gerçek parametre listesi doğrulanacaktır.

## Bağlantı Dizesi

Oluşturulan `appsettings.json` içindeki PostgreSQL bağlantı dizesini gerçek geliştirme ortamına göre düzenle:

```text
Host=localhost;Port=5432;Database=SofasisERP_Dev;Username=...;Password=...
```

Kurallar:

- Gerçek parolayı depoya işleme; kullanıcı bazlı gizli değerler için `dotnet user-secrets` veya ortam değişkeni kullan.
- Bağlantı bilgileri bilinmiyorsa DUR ve kullanıcıdan iste.

---

# 8. RESTORE VE BUILD DOĞRULAMASI

Solution oluşturulduktan sonra:

```text
dotnet restore
dotnet build --no-restore
```

Doğrula:

- Restore hatasız tamamlanıyor ve tüm DevExpress paketleri 26.1.3 sürümünden çözümleniyor.
- Build hatasız tamamlanıyor.
- Paket sürümlerini doğrula: `dotnet list package` çıktısında DevExpress paketlerinin tamamı 26.1.3 olmalıdır. Karışık sürüm varsa DUR ve bildir.

Kurallar:

- Build hatası durumunda rastgele düzeltme yapma; hatanın kök nedenini raporla.
- Template'in ürettiği kodu bu aşamada değiştirme.

---

# 9. KURULUM RAPORU

`Brain/Kurulum/INITIAL_SETUP_REPORT.md` dosyasına aşağıdakileri kaydet:

1. Tarih ve ortam bilgileri (işletim sistemi, .NET SDK sürümleri).
2. DevExpress 26.1.3 doğrulama sonucu (kurulum dizini, paket kaynağı).
3. PostgreSQL doğrulama sonucu.
4. Kullanılan tam template komutu ve üretilen proje listesi.
5. Restore/build sonuçları ve paket sürüm doğrulaması.
6. Kalan açık konular ve kullanıcıdan beklenen bilgiler (varsa).

---

# 10. KAPSAM DIŞI

Bu görevde aşağıdakiler YAPILMAYACAKTIR:

- Sofasis business domain'i (model, modül, konfigüratör, sipariş, üretim vb.) geliştirmek.
- EF Core'a geçiş veya XPO dışı persistence denemesi.
- DevExpress sürüm yükseltme/değiştirme.
- Eski ERP'den kod/taşıma/inceleme.
- Enterprise ürün stratejisi kapsamındaki gelişmiş özellikler.
- Üretim (production) dağıtım yapılandırması.

---

# 11. DURMA KOŞULLARI

Aşağıdaki durumlarda ilerlemeyi DURDUR ve kullanıcıya bildir:

- Gerekli DevExpress 26.1.3 paketleri veya template'i lokal ortamda yoksa.
- .NET SDK uyumsuzsa veya eksikse.
- PostgreSQL erişimi sağlanamıyorsa ve bağlantı bilgileri alınamıyorsa.
- Restore sırasında farklı DevExpress sürümleri çözümlenirse.
- Build kök nedeni anlaşılamayan bir hatayla başarısız olursa.
- Belirsiz bir iş kuralı veya mimari karar gerekirse.