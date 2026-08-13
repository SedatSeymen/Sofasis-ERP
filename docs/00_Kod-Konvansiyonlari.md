# 00 — Kod Konvansiyonları

Bu doküman, SofasisERP'de tüm XPO iş nesnelerinin ve servislerin uyacağı standartları tanımlar. `CLAUDE.md` bunun özetidir; burada uygulama detayı ve C# taslakları var. Kod bu dokümandan saparsa önce doküman güncellenir.

---

## 1. İsimlendirme

- **Namespace:** `SofasisERP.Module.BusinessObjects` (klasörler yalnız gruplama; tip adları benzersiz).
- **Sınıf ve property adları Türkçe.** Kullanıcı etiketleri `[XafDisplayName("...")]` ile Türkçe.
- **Son ekler:** `Tanim` = kart/master tanım (ör. `CariHesapTanim`), `M` = başlık (master), `D` = satır (detay), `Hareketleri` = hareket defteri, `Parametre` = modül ayarı (tekil kayıt).
- **Servisler İngilizce arayüz adıyla:** `INumberSequenceService`, `IVatCalculator`, `IWeightedAverageCostService`, `IJournalPostingService`, `IEInvoiceProvider`.
- Kopyala-yapıştır etiket bırakma; her `[XafDisplayName]` sınıfın gerçek anlamını yansıtsın.

## 2. Taban Sınıf Hiyerarşisi (Guid PK)

Tek `BusinessObjects/Base/` klasörü. PK = `Oid` (Guid, XAF `BaseObject`'ten). Eski projedeki string(13) `KeyID`-as-PK KULLANILMAZ.

> ⚠ **26.1.3'te namespace değişikliği (doğrulandı, derlenerek kontrol edildi):** `XafDisplayNameAttribute` artık `DevExpress.ExpressApp.DC` içinde; `ModelDefaultAttribute`, `DetailViewLayoutAttribute`, `LayoutGroupType` ise `DevExpress.ExpressApp.Model` içinde — `DevExpress.Persistent.Base`'de DEĞİL (bazı eski örnek/döküman parçaları bu üçünü de `DevExpress.Persistent.Base`'de gösterir, 26.1.3 için doğru değil). `RuleRequiredField`/`RuleUniqueValue`/`RuleValueComparison`/`RuleRegularExpression` ise `DevExpress.Persistent.Validation` içinde. `VisibleInListView`/`VisibleInDetailView`/`VisibleInLookupListView`/`VisibleInReports`/`VisibleInDashboards`/`DefaultClassOptions` hâlâ `DevExpress.Persistent.Base`'de.

Gerçek sınıf adı **`BaseObject`**'tir (DevExpress'in kendi `DevExpress.Persistent.BaseImpl.BaseObject`'inden türer; bizimki `Sofasis.ERP.Module.BusinessObjects.Base` namespace'inde olduğu için ada çakışma yok, ama türetme satırında tam nitelenmiş ad kullanılır).

```csharp
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.DC;      // XafDisplayNameAttribute
using DevExpress.ExpressApp.Model;   // ModelDefaultAttribute
using DevExpress.Persistent.Base;    // VisibleIn*Attribute
using DevExpress.Xpo;

namespace Sofasis.ERP.Module.BusinessObjects.Base;

// Tüm iş nesnelerinin tabanı: Guid Oid + IsDefault/IsSystemRecord + kancalar
[NonPersistent]
public abstract class BaseObject : DevExpress.Persistent.BaseImpl.BaseObject
{
    protected BaseObject(Session session) : base(session) { }

    bool isDefault;
    bool isSystemRecord;

    [XafDisplayName("Varsayılan mı?")]
    [VisibleInListView(false)]
    public bool IsDefault { get => isDefault; set => SetPropertyValue(nameof(IsDefault), ref isDefault, value); }

    [XafDisplayName("Sistem Kaydı mı?")]
    [VisibleInListView(false), VisibleInDetailView(false)]
    [ModelDefault("AllowEdit", "False")]
    public bool IsSystemRecord { get => isSystemRecord; set => SetPropertyValue(nameof(IsSystemRecord), ref isSystemRecord, value); }

    // Alt sınıflar için kancalar (ağır DB işi BURADA değil; servis katmanında)
    protected internal virtual void ObjectSaving() { }
    protected internal virtual void ObjectDeleting() { }

    protected override void OnSaving()
    {
        // Tek-varsayılan zorlaması: IsDefault=true ise aynı tipteki diğerini false yap (aynı Session içinde)
        if (IsDefault)
        {
            var deger = Session.FindObject(GetType(), new BinaryOperator(nameof(IsDefault), true));
            if (deger != null && !ReferenceEquals(deger, this))
                ((BaseObject)deger).SetMemberValue(nameof(IsDefault), false);
        }
        ObjectSaving();
        base.OnSaving();
    }

    protected override void OnDeleting()
    {
        if (IsSystemRecord)
            throw new DevExpress.ExpressApp.UserFriendlyException("Bu bir sistem kaydıdır, silinemez.");
        ObjectDeleting();
        base.OnDeleting();
    }
}
```

```csharp
// Denetim alanları — sekme yerleşimi Model.DesignedDiffs.xafml'de (bkz. §3), burada DEĞİL
[NonPersistent]
public abstract class BaseObjectAudit : BaseObject
{
    protected BaseObjectAudit(Session session) : base(session) { }

    ApplicationUser? olusturanKullanici, degistirenKullanici;
    DateTime olusturmaTarihi, degistirmeTarihi;

    [XafDisplayName("Oluşturan Kullanıcı"), ModelDefault("AllowEdit", "False")]
    [VisibleInListView(false), VisibleInLookupListView(false), VisibleInReports(false), VisibleInDashboards(false)]
    public ApplicationUser? OlusturanKullanici { get => olusturanKullanici; set => SetPropertyValue(nameof(OlusturanKullanici), ref olusturanKullanici, value); }

    [XafDisplayName("Oluşturma Tarihi"), ModelDefault("AllowEdit", "False"), ModelDefault("DisplayFormat", "G")]
    [VisibleInListView(false), VisibleInLookupListView(false), VisibleInReports(false), VisibleInDashboards(false)]
    public DateTime OlusturmaTarihi { get => olusturmaTarihi; set => SetPropertyValue(nameof(OlusturmaTarihi), ref olusturmaTarihi, value); }

    [XafDisplayName("Değiştiren Kullanıcı"), ModelDefault("AllowEdit", "False")]
    [VisibleInListView(false), VisibleInLookupListView(false), VisibleInReports(false), VisibleInDashboards(false)]
    public ApplicationUser? DegistirenKullanici { get => degistirenKullanici; set => SetPropertyValue(nameof(DegistirenKullanici), ref degistirenKullanici, value); }

    [XafDisplayName("Değiştirme Tarihi"), ModelDefault("AllowEdit", "False"), ModelDefault("DisplayFormat", "G")]
    [VisibleInListView(false), VisibleInLookupListView(false), VisibleInReports(false), VisibleInDashboards(false)]
    public DateTime DegistirmeTarihi { get => degistirmeTarihi; set => SetPropertyValue(nameof(DegistirmeTarihi), ref degistirmeTarihi, value); }

    protected internal override void ObjectSaving()
    {
        base.ObjectSaving();
        var kullanici = GetCurrentUser();   // DI yoksa (test/tasarım-zamanı) null döner, patlamaz
        if (Session.IsNewObject(this)) { OlusturmaTarihi = DateTime.Now; if (kullanici != null) OlusturanKullanici = kullanici; }
        else { DegistirmeTarihi = DateTime.Now; if (kullanici != null) DegistirenKullanici = kullanici; }
    }

    // SecuritySystem.CurrentUserId KULLANILMAZ (Blazor'da ValueManager context'i garanti değil,
    // bkz. XAF0035 analyzer kuralı). Bunun yerine Session.ServiceProvider + ISecurityStrategyBase.
    ApplicationUser? GetCurrentUser()
    {
        var security = Session.ServiceProvider?.GetService<DevExpress.ExpressApp.Security.ISecurityStrategyBase>();
        if (security?.UserId == null) return null;
        return Session.GetObjectByKey<ApplicationUser>(security.UserId);
    }
}
```

```csharp
[NonPersistent]
public abstract class BaseObjectAuditAciklama : BaseObjectAudit
{
    protected BaseObjectAuditAciklama(Session session) : base(session) { }
    string? aciklama, ozelKod1, ozelKod2;

    [Size(32), XafDisplayName("Özel Kod 1"), VisibleInListView(false)]
    public string? OzelKod1 { get => ozelKod1; set => SetPropertyValue(nameof(OzelKod1), ref ozelKod1, value); }

    [Size(32), XafDisplayName("Özel Kod 2"), VisibleInListView(false)]
    public string? OzelKod2 { get => ozelKod2; set => SetPropertyValue(nameof(OzelKod2), ref ozelKod2, value); }

    [Size(200), ModelDefault("RowCount", "3"), XafDisplayName("Açıklama"), VisibleInListView(false)]
    public string? Aciklama { get => aciklama; set => SetPropertyValue(nameof(Aciklama), ref aciklama, value); }
}
```

> Not: Denetim için XAF'ın hazır `AuditTrail` modülü de açıktır (kim neyi ne zaman değiştirdi geçmişi). Yukarıdaki alanlar kayıt üstündeki özet denetim bilgisidir; ikisi birlikte kullanılır.

## 3. ⚠ AUDIT SEKME KURALI (zorunlu ve standart) — ve genel DetailView sekme deseni

**Kural:** Denetim alanları (`OlusturanKullanici`, `OlusturmaTarihi`, `DegistirenKullanici`, `DegistirmeTarihi`) her `DetailView`'da **ayrı bir "Denetim" sekmesinde ve tüm sekmelerin EN SONUNDA** gösterilir. `ListView` / `LookupListView` / `Reports` / `Dashboards`'ta gizlidir (taban sınıftaki `VisibleIn*(false)` attribute'ları bunu sağlar).

> ⚠ **Düzeltme (Faz 0, canlı ortamda ampirik olarak doğrulandı):** İki taslak önce `[DetailViewLayoutAttribute(groupId, LayoutGroupType.TabbedGroup, groupIndex)]` property attribute'unun, aynı `groupId`'yi paylaşan BİRDEN FAZLA property'yi TEK bir sekmede birleştireceği varsayılmıştı (resmi dokümandaki "NotesAndRemarks" örneğine dayanarak). **Bu, 26.1.3 Blazor render'ında ÇALIŞMADI** — her property kendi ayrı alt-sekmesi olarak render edildi (ör. "Denetim" sekmesi altında 4 ayrı sekme: her denetim alanı kendi başına). Tarayıcıda adım adım test edilerek doğrulandı ve **terk edildi**.

**Gerçek, kanıtlanmış yöntem: `Sofasis.ERP.Module/Model.DesignedDiffs.xafml`'de elle yazılmış `<Layout>` XML'i** — eski şablon projenin (Sofasis Erp Project) yıllardır kullandığı, tamamen kanıtlanmış desen. Her `X_DetailView` için:

```xml
<DetailView Id="XTanim_DetailView">
  <Layout>
    <LayoutGroup Id="Main" RelativeSize="100">
      <LayoutGroup Id="SimpleEditors" RelativeSize="100">
        <!-- Otomatik üretilen varsayılan gruplar mükerrer görünmesin diye kaldırılır -->
        <LayoutGroup Id="XTanim" Removed="True" />                  <!-- sınıfın KENDİ alanları (sınıf adıyla otomatik grup) -->
        <LayoutGroup Id="BaseObject" Removed="True" />               <!-- IsDefault -->
        <LayoutGroup Id="BaseObjectAuditAciklama" Removed="True" />  <!-- OzelKod1/2, Aciklama -->
        <LayoutGroup Id="BaseObjectAudit" Removed="True" />          <!-- 4 denetim alanı -->
        <TabbedGroup Id="Tabs" CaptionLocation="Top" Index="0" RelativeSize="100" IsNewNode="True">
          <LayoutGroup Id="Genel" Caption="Genel" Index="0" RelativeSize="100" IsNewNode="True">
            <LayoutItem Id="XKodu" ViewItem="XKodu" Index="0" IsNewNode="True" />
            <LayoutItem Id="XAdi" ViewItem="XAdi" Index="1" IsNewNode="True" />
            <LayoutItem Id="IsDefault" ViewItem="IsDefault" Index="2" IsNewNode="True" />
          </LayoutGroup>
          <LayoutGroup Id="OzelKodlarVeAciklama" Caption="Özel Kodlar &amp; Açıklama" Index="1" RelativeSize="100" IsNewNode="True">
            <LayoutItem Id="OzelKod1" ViewItem="OzelKod1" Index="0" IsNewNode="True" />
            <LayoutItem Id="OzelKod2" ViewItem="OzelKod2" Index="1" IsNewNode="True" />
            <LayoutItem Id="Aciklama" ViewItem="Aciklama" Index="2" IsNewNode="True" />
          </LayoutGroup>
          <LayoutGroup Id="Denetim" Caption="Denetim" Index="2" RelativeSize="100" IsNewNode="True">
            <!-- 2 sütun: satır başına Direction="Horizontal" alt-grup -->
            <LayoutGroup Id="Denetim_Row1" ShowCaption="False" Direction="Horizontal" Index="0" RelativeSize="50" IsNewNode="True">
              <LayoutItem Id="OlusturanKullanici" ViewItem="OlusturanKullanici" Index="0" RelativeSize="50" IsNewNode="True" />
              <LayoutItem Id="OlusturmaTarihi" ViewItem="OlusturmaTarihi" Index="1" RelativeSize="50" IsNewNode="True" />
            </LayoutGroup>
            <LayoutGroup Id="Denetim_Row2" ShowCaption="False" Direction="Horizontal" Index="1" RelativeSize="50" IsNewNode="True">
              <LayoutItem Id="DegistirenKullanici" ViewItem="DegistirenKullanici" Index="0" RelativeSize="50" IsNewNode="True" />
              <LayoutItem Id="DegistirmeTarihi" ViewItem="DegistirmeTarihi" Index="1" RelativeSize="50" IsNewNode="True" />
            </LayoutGroup>
          </LayoutGroup>
        </TabbedGroup>
      </LayoutGroup>
      <!-- Aciklama, RowCount=3 nedeniyle "Sizeable" sınıflanır ve ayrıca SizeableEditors grubuna da otomatik düşer — o da kaldırılmalı -->
      <LayoutGroup Id="SizeableEditors" Removed="True" />
      <LayoutItem Id="Aciklama" Removed="True" />
    </LayoutGroup>
  </Layout>
</DetailView>
```

**Önemli noktalar:**
- Otomatik üretilen grup ID'leri: sınıfın KENDİ alanları için grup ID'si **sınıf adının kendisidir** (ör. `UlkeTanim`); miras alınan alanlar için grup ID'si **tanımlandığı taban sınıfın adıdır** (`BaseObject`, `BaseObjectAuditAciklama`, `BaseObjectAudit`). Bunları `Removed="True"` ile kaldırmazsan alanlar HEM eski otomatik yerinde HEM yeni sekmende mükerrer görünür.
- Master-detail koleksiyonlar (ör. `UlkeTanim.Sehirler`) sekme İÇİNE alınmaz — XAF onları zaten `Main` altında, sekme grubunun ALTINDA kendi bölümünde otomatik gösterir; dokunma.
- Her yeni "Tanım" sınıfı için bu XML bloğu elle eklenir (mekanik ama kanıtlanmış); `[DetailViewLayoutAttribute]` grup-birleştirme amacıyla KULLANILMAZ (yalnızca tek-property/tek-yeni-sekme senaryosunda güvenilir olabilir, ama biz hiç kullanmıyoruz — tutarlılık için tamamen XML deseni tercih edildi).

## 4. Numaralandırma (tek sistem, onayda, boşluksuz)

> ⚠ **Düzeltme (doğrulandı):** İlk taslakta anılan `DevExpress.Xpo.DistributedIdGeneratorHelper` DevExpress 26.1 dokümantasyonunda/derlenen paketlerde **mevcut değil** (dxdocs araması sonuçsuz). Gerçek, resmi olarak belgelenen DB-güvenli desen — DevExpress'in "generate a sequential number within a database transaction" örneği — bir sayaç kaydını optimistic locking ile artırmaktır. Aşağıdaki tasarım bu deseni, ADR-004'ün "boşluksuz" hedefine daha uygun şekilde (ayrı Session açmadan) uygular.

- Tek servis: `INumberSequenceService.SonrakiNo(Session session, string seriKodu)` — `Sofasis.ERP.Module/Services/`. Implementasyon `Sofasis.ERP.Module/BusinessObjects/Base/NumaraSayaci.cs` adlı iç (kullanıcıya görünmez, `[DefaultClassOptions]` yok) bir sayaç kaydını **çağıranın kendi Session'ı içinde** artırır — DevExpress `DistributedIdGeneratorHelper` DEĞİL, ayrı `Session` de AÇMAZ.
- **Neden ayrı Session yok:** Sayaç artışı çağıranın UnitOfWork'ü içinde olduğu için, dış işlem (ör. fatura onayı) rollback olursa sayaç artışı da rollback olur → numara boşa harcanmaz (gerçek "boşluksuz"). Eşzamanlı iki çağrı aynı `seriKodu`'nu artırmaya çalışırsa commit anında `OptimisticLockException` fırlar; bunu yakalayıp **tüm dış işlemi** yeniden deneme sorumluluğu çağırana (gelecekteki `IJournalPostingService`/fatura onay akışı, Faz 3) aittir.
- **Yasal belge numarası (fatura, muhasebe fişi vb.) ONAY/POSTING anında** atanır, taslak kayıtta değil.
- Ara/dahili belgeler (sipariş vb.) kayıt anında numaralanabilir.
- Global `lock` + DB commit deseni (eski `SequenceGeneratorHelper`) KULLANILMAZ.

## 5. Master-Detail

```csharp
// Başlık (M)
[Association("FaturaM-FaturaDs"), Aggregated]
public XPCollection<SatisFaturaD> Satirlar => GetCollection<SatisFaturaD>(nameof(Satirlar));

// Satır (D)
[Association("FaturaM-FaturaDs")]
public SatisFaturaM Baslik { get => baslik; set => SetPropertyValue(nameof(Baslik), ref baslik, value); }
```

## 6. Doğrulama ve Görünüm

- Doğrulama: `RuleRequiredField`, `RuleUniqueValue` (+ `[Indexed(Unique=true)]`), `RuleValueComparison` — hepsi `DevExpress.Persistent.Validation` namespace'inde (26.1.3, doğrulandı; `DevExpress.Persistent.Base`'de DEĞİL). VKN/TCKN için ortak `Sofasis.ERP.Module.Helpers.TaxIdValidator` statik sınıfı (`VknGecerliMi`/`TcknGecerliMi`/`VknVeyaTcknGecerliMi`); IBAN/e-posta gibi diğer alanlar için de aynı `Helpers/` klasöründe benzer statik doğrulayıcılar eklenir — dağınık ad-hoc regex yerine.
- Durum renkleri, enable/disable: `ConditionalAppearance` (`[Appearance(...)]`).
- Kademeli lookup: `DataSourceCriteria` / `DataSourceProperty`.
- `[DefaultProperty]` ve anlamlı `[XafDisplayName]` her kartta.

## 7. Referans Bütünlüğü (silme koruması)

Kullanımdaki ana veri silinemez. `ObjectDeleting`/`OnDeleting` içinde ilgili hareket/belge var mı kontrol edilir ve varsa `UserFriendlyException` fırlatılır. **Bu kontroller yorumda bırakılmaz** (eski projedeki hata). Ortak bir "kullanımda mı?" yardımcı deseni kullanılır.

## 8. Tipler ve Kesin Kurallar

> ⚠ **Kritik XPO tuzağı (doğrulandı):** C# `decimal` özelliği, XPO'nun SQL Server eşlemesinde **varsayılan olarak `money` tipine** gider (`decimal(18,2)`'ye DEĞİL) — bkz. [XPO SQL Server Data Types Mapping]. Bu yüzden **her decimal alanda açıkça `[DbType("decimal(p,s)")]` ZORUNLUDUR**; yoksa aşağıdaki hassasiyet kuralı sessizce ihlal edilir (kod derlenir, çalışır, ama kolon tipi yanlıştır). Örnek: `[DbType("decimal(9,4)")] public decimal KDVOrani { ... }`.

- Para/oran: tutar `decimal(18,2)`, birim maliyet `decimal(28,6)`, oran(%) `decimal(9,4)`, kur `decimal(18,6)` — her biri yukarıdaki gibi `[DbType(...)]` ile.
- Metinlerde `[Size(n)]` zorunlu; uzun metin `SizeAttribute.Unlimited`.
- İş mantığı (KDV, maliyet, fiş üretimi, aktarım) arayüz arkası servislerde; iş nesnesinin `OnSaving/OnChanged`'ine ağır DB/hesap işi gömülmez.
- `new Session(...)` ile ObjectSaving içinde yan-yazma YOK; aynı UnitOfWork + `[Association]`/servis.
- Parametre/varsayılan kartları oturum bazında önbelleğe al; `OnLoaded`'da satır başına sorgu yok. **Kanonik yol:** `Sofasis.Module/Extensions/SessionCacheExtensions.cs` — `Session.GetVarsayilan<T>()` (`IsVarsayilan=true` işaretli tekil kayıt için, ör. `KDVTanim`/`DovizTanim`/`BirimTanim`) ve `Session.GetSingleton<T>()` (tablodaki tek satır için, ör. `StokParametre`). Yeni bir varsayılan/tekil-parametre okuması gerektiğinde bu iki metottan biri kullanılır, ayrı bir önbellekleme icat edilmez.
- Toplu işlemlerde commit/refresh döngü dışında.
- **Çapraz-tablo bağı, gerçek PK (`KeyID`) paylaşılarak kurulmaz.** İki bağımsız tabloyu (ör. bir "ayna kayıt" senaryosunda) ilişkilendirmek için `BaseClass`'taki `IntegrationCode`/`IntegrationSourceEntity` çifti kullanılır: her iki tarafa aynı `IntegrationCode` değeri (örn. kaynak nesnenin kendi `KeyID`'si) ve karşı tarafın tipini `IntegrationSourceEntity`'ye yazılır, arama `Session.FindObject<T>(new BinaryOperator(nameof(IntegrationCode), kod))` ile yapılır. Not: XPO'nun `[Association]`'ı bire-bir ilişkide iki tarafta da tekil referansı desteklemiyor (`AssociationInvalidException`); gerçek bire-bir association gerekiyorsa DevExpress'in resmi deseni (düz referans property + karşı tarafı elle senkronize eden setter) kullanılır, `IntegrationCode` genel amaçlı gevşek bağ için tercih edilir.
