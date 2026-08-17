<!-- ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : 00_DetailView_ve_Servis_Konvansiyonlari.md
 * Oluşturma Tarihi : 2026-08-17
 * Oluşturan        : Sofasis Development Team
 * Son Güncelleme   : 2026-08-17
 * Son Güncelleyen  : Sofasis Development Team
 * Açıklama         : Eski SofasisERP projesinden (D:\2025\SofasisERP) ampirik
 *                    olarak doğrulanmış, kanıtlanmış teknik desenler — aynen taşınır.
 * ****************************************************************************
-->

# DetailView ve Servis Konvansiyonları

Bu doküman, eski SofasisERP projesinde (`D:\2025\SofasisERP\docs\00_Kod-Konvansiyonlari.md`) yıllar içinde canlı ortamda test edilerek doğrulanmış üç deseni kayıt altına alır. Bunlar eski projeden **aynen** taşınır — yeniden icat edilmez, yeniden test edilmez.

## 1. Audit Sekme Kuralı (zorunlu, standart)

**Kural:** Denetim alanları (`OlusturanKullanici`, `OlusturmaTarihi`, `DegistirenKullanici`, `DegistirmeTarihi`) her `DetailView`'da **ayrı bir "Denetim" sekmesinde ve tüm sekmelerin EN SONUNDA** gösterilir. `ListView` / `LookupListView` / `Reports` / `Dashboards`'ta gizlidir (taban sınıftaki `VisibleIn*(false)` attribute'ları bunu sağlar).

### ⚠ Kanıtlanmış ÇALIŞMAYAN yaklaşım — denenmeyecek

`[DetailViewLayoutAttribute(groupId, LayoutGroupType.TabbedGroup, groupIndex)]` property attribute'unun, resmi DevExpress dokümantasyonundaki ("NotesAndRemarks" örneği) varsayıma göre aynı `groupId`'yi paylaşan birden fazla property'yi TEK bir sekmede birleştirmesi beklenir. **Bu, DevExpress 26.1.3 Blazor render'ında ÇALIŞMADI** — her property kendi ayrı alt-sekmesi olarak render edildi (ör. "Denetim" altında 4 ayrı alt-sekme). Tarayıcıda adım adım test edilerek doğrulanmış ve terk edilmiştir. Bu attribute grup-birleştirme amacıyla kullanılmaz.

### Kanıtlanmış GERÇEK yöntem

`Model.DesignedDiffs.xafml`'de elle yazılmış `<Layout>` XML'i, her `X_DetailView` için:

```xml
<DetailView Id="XTanim_DetailView">
  <Layout>
    <LayoutGroup Id="Main" RelativeSize="100">
      <LayoutGroup Id="SimpleEditors" RelativeSize="100">
        <!-- Otomatik üretilen varsayılan gruplar mükerrer görünmesin diye kaldırılır -->
        <LayoutGroup Id="XTanim" Removed="True" />                  <!-- sınıfın KENDİ alanları (sınıf adıyla otomatik grup) -->
        <LayoutGroup Id="BaseObject" Removed="True" />
        <LayoutGroup Id="BaseObjectAuditAciklama" Removed="True" />
        <LayoutGroup Id="BaseObjectAudit" Removed="True" />          <!-- 4 denetim alanı -->
        <TabbedGroup Id="Tabs" CaptionLocation="Top" Index="0" RelativeSize="100" IsNewNode="True">
          <LayoutGroup Id="Genel" Caption="Genel" Index="0" RelativeSize="100" IsNewNode="True">
            <LayoutItem Id="XKodu" ViewItem="XKodu" Index="0" IsNewNode="True" />
            <LayoutItem Id="XAdi" ViewItem="XAdi" Index="1" IsNewNode="True" />
          </LayoutGroup>
          <LayoutGroup Id="Denetim" Caption="Denetim" Index="1" RelativeSize="100" IsNewNode="True">
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
    </LayoutGroup>
  </Layout>
</DetailView>
```

**Önemli noktalar:**
- Otomatik üretilen grup ID'leri: sınıfın KENDİ alanları için grup ID'si **sınıf adının kendisidir**; miras alınan alanlar için grup ID'si **tanımlandığı taban sınıfın adıdır** (`BaseClass`, `BaseClassWithAudit`, `BaseClassWithDescription`, `BaseClassWithAuditAndDescription` — bizim proje için tam adlar farklı olabilir, ilk uygulamada gerçek grup ID'leri tarayıcıda/Model Editor'de doğrulanmalı). Kaldırılmazsa alanlar HEM eski otomatik yerinde HEM yeni sekmede mükerrer görünür.
- Master-detail koleksiyonlar sekme İÇİNE alınmaz — XAF onları zaten `Main` altında, sekme grubunun ALTINDA kendi bölümünde otomatik gösterir; dokunulmaz.
- Her yeni "Tanım" sınıfı için bu XML bloğu elle eklenir (mekanik ama kanıtlanmış).

## 2. Numaralandırma Servisi

`INumberSequenceService.SonrakiNo(Session session, string seriKodu)` — sayaç kaydını **çağıranın kendi Session/UnitOfWork'ü içinde** artırır.

- **Neden ayrı Session yok:** Sayaç artışı çağıranın UnitOfWork'ü içinde olduğu için, dış işlem (ör. fatura onayı) rollback olursa sayaç artışı da rollback olur → gerçek "boşluksuz" numaralandırma. DevExpress'in 26.1'de mevcut olmayan `DistributedIdGeneratorHelper`'a dayanmaz, global `lock`+commit deseni kullanılmaz.
- Eşzamanlı iki çağrı aynı `seriKodu`'nu artırmaya çalışırsa commit anında `OptimisticLockException` fırlar; yeniden deneme sorumluluğu çağırana aittir.
- **Kural:** Yasal belge numarası (Fatura, ileride Muhasebe Fişi) ONAY/POSTING anında atanır, taslak kayıtta değil. Dahili belgeler (Sipariş, Fiş, StokKodu sıra numarası) kayıt anında numaralanabilir.

## 3. CSV Tabanlı Seed Verisi

- Gömülü (embedded resource) CSV dosyaları: `Resources/Seed/*.csv`, `;` ayraçlı, UTF-8.
- `SeedCsvReader.Read<T>(csvFileName)` — `CsvHelper` ile genel okuma yardımcısı, tolerant config (eksik alan/hatalı veri/başlık uyuşmazlığını görmezden gelir).
- `DatabaseSeeder` — her CSV için satır tipi POCO + idempotent `Seed()` metodu (`FirstOrDefault` ile var mı kontrolü, yoksa oluştur), bağımlılık sırasına göre çağrılır (ör. Şehir → İlçe), sonunda `CommitChanges()`.
- Seed edilen sistem kayıtları `SistemKaydi`/`IsSystemRecord` flag'i ile korunur (kullanıcı silemez/bazı alanları değiştiremez) — `Appearance` attribute ile `Criteria = "IsSystemRecord = true"` üzerinden salt-okunur yapılır.
- Bağımlılık: `CsvHelper` NuGet paketi (DevExpress dışı, en son stable sürüm).
