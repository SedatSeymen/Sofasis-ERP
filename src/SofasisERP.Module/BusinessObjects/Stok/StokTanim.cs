/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok kalemi ana tanım sınıfı — hem atomik ürün/bileşen hem
 *                    hammadde seviyesinde var olur (bkz. SOFASIS_ERP_MIMARI_
 *                    TASARIM.md §45.2 Katman 5). StokKodu, StokKoduJeneratoru
 *                    ile OnSaving'de otomatik üretilir.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using SofasisERP.Module.Services;
using System.ComponentModel;
using System.Linq;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[DefaultProperty(nameof(StokAdi))]
[XafDisplayName("Stok Tanımlama")]
public class StokTanim : BaseClassWithAuditAndDescription
{
    public StokTanim(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        // NOT: Session.IsNewObject korumasız idi — XPO, DB'den mevcut kayıt yüklenirken de
        // AfterConstruction'ı çağırdığından, her açılışta DovizTanim güncel varsayılana
        // SESSİZCE ezilebiliyordu (denetim raporunda bulundu, doğrulandı; projedeki diğer
        // tüm sınıflar bu korumayı zaten kullanıyor).
        if (Session.IsNewObject(this))
        {
            // Yeni kayıtta Döviz alanı boş başlamasın — sistemdeki varsayılan para
            // birimi (ör. TRY) otomatik seçili gelsin, kullanıcı isterse değiştirir.
            DovizTanim = Session.FindObject<DovizTanim>(
                CriteriaOperator.FromLambda<DovizTanim>(x => x.IsVarsayilan));
            stokHizmetMasrafTipi = StokHizmetMasrafTipi.Stok;
        }
    }

    StokHizmetMasrafTipi stokHizmetMasrafTipi;

    // StokTipi/StokGrubu/StokAltGrubu hiyerarşisinden BAĞIMSIZ — o hiyerarşi yalnızca
    // fiziksel STOK sınıflandırması içindir. Hizmet ve Masraf kartlarının (eski projeden
    // aynen taşınan ayrım) fiziksel stok hiyerarşisine ihtiyacı yoktur — bkz.
    // Hizmet/Masraf Tanımlama ekranları (StokAltGrubu bu türlerde zorunlu değildir).
    [VisibleInListView(false)]
    [XafDisplayName("Kayıt Türü")]
    public StokHizmetMasrafTipi StokHizmetMasrafTipi
    {
        get => stokHizmetMasrafTipi;
        set => SetPropertyValue(nameof(StokHizmetMasrafTipi), ref stokHizmetMasrafTipi, value);
    }

    FisTuruTanim fisTuruTanim;

    // Kullanıcı tarafından seçilmez — StokTanimYeniKayitVarsayilanlariController
    // yeni kayıt açılan ekrana göre (StokTanim_DetailView/HizmetTanim_DetailView/
    // MasrafTanim_DetailView) otomatik atar. Yalnızca StokKoduUretimYontemi=Otomatik
    // iken (ya da Hizmet/Masraf'ta her zaman) StokKoduJeneratoru.SonrakiNumara'ya
    // önek kaynağı olarak verilir — bkz. NumberSequenceService.SonrakiNumara.
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [XafDisplayName("Fiş Türü")]
    public FisTuruTanim FisTuruTanim
    {
        get => fisTuruTanim;
        set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
    }

    string stokKodu;
    string stokAdi;
    string stokAdiIngilizce;
    string stokBarkodNo;
    MediaDataObject resim;
    bool satilabilirMi;
    StokGrupTanim stokGrubu;
    StokAltGrupTanim stokAltGrubu;
    ModelTanim model;
    BirimTanim birimTanim;
    DovizTanim dovizTanim;
    KDVTanim kdvTanim;
    decimal? alisFiyati;
    decimal? satisFiyati;
    bool kdvDahilMi;
    decimal sonAlisFiyati;
    decimal en;
    decimal boy;
    decimal yukseklik;
    decimal metrekare;
    decimal metreKup;
    decimal agirlik;
    decimal toplamMiktar;
    decimal ortalamaMaliyet;

    [Size(32)]
    [Indexed(Unique = true)]
    [Appearance("ED_StokTanim_StokKodu", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Stok Kodu")]
    public string StokKodu
    {
        get => stokKodu;
        set => SetPropertyValue(nameof(StokKodu), ref stokKodu, value);
    }

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_StokTanim_StokAdi", DefaultContexts.Save, "Lütfen Stok Adını Giriniz...")]
    [XafDisplayName("Stok Adı")]
    public string StokAdi
    {
        get => stokAdi;
        set => SetPropertyValue(nameof(StokAdi), ref stokAdi, value);
    }

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [VisibleInListView(false)]
    [XafDisplayName("Stok Adı İngilizce")]
    public string StokAdiIngilizce
    {
        get => stokAdiIngilizce;
        set => SetPropertyValue(nameof(StokAdiIngilizce), ref stokAdiIngilizce, value);
    }

    [Size(32)]
    [VisibleInListView(false)]
    [XafDisplayName("Barkod No")]
    public string StokBarkodNo
    {
        get => stokBarkodNo;
        set => SetPropertyValue(nameof(StokBarkodNo), ref stokBarkodNo, value);
    }

    // DB'de saklanır, gecikmeli yüklenir, tarayıcı tarafında önbelleklenir
    // (ApplicationUser.Resim/ModelTanim.Resim ile aynı desen). Kullanıcı kararı
    // (2026-08-17): Resim Stok kartında da ASLA zorunlu değil — opsiyonel kalır.
    [ImageEditor(ListViewImageEditorMode = ImageEditorMode.PictureEdit, ListViewImageEditorCustomHeight = 32)]
    [XafDisplayName("Resim")]
    public MediaDataObject Resim
    {
        get => resim;
        set => SetPropertyValue(nameof(Resim), ref resim, value);
    }

    // Hammadde/bileşen bile doğrudan satılabilir olabilir (ör. kumaş, ekstra
    // kırlent) — bkz. §45.2 "Satış birimi esnekliği hammaddeye kadar genişliyor".
    [XafDisplayName("Satılabilir mi?")]
    public bool SatilabilirMi
    {
        get => satilabilirMi;
        set => SetPropertyValue(nameof(SatilabilirMi), ref satilabilirMi, value);
    }

    // Kullanıcı kararı (2026-08-17): Stok Grubu gerçekten seçilebilir/aktif bir alan —
    // önce bu seçilir, Stok Alt Grubu listesi buna göre filtrelenir (kademeli/bağımlı
    // lookup, bkz. StokAltGrubu'ndaki DataSourceProperty). ImmediatePostData ile seçim
    // yapılır yapılmaz Alt Grubu lookup'ı anında yeniden filtrelenir.
    [ImmediatePostData]
    [RuleRequiredField("RuleRequired_StokTanim_StokGrubu", DefaultContexts.Save, "Lütfen Stok Grubunu Seçiniz...",
        TargetCriteria = "[StokHizmetMasrafTipi] = 'Stok'")]
    [XafDisplayName("Stok Grubu")]
    public StokGrupTanim StokGrubu
    {
        get => stokGrubu;
        set => SetPropertyValue(nameof(StokGrubu), ref stokGrubu, value);
    }

    // Lookup'ı StokGrubu.StokAltGrupTanims koleksiyonuyla sınırlar — StokGrubu boşken
    // hiçbir seçenek gösterilmez (SelectNothing), kullanıcı önce Stok Grubunu seçmek
    // zorunda kalır. Bkz. DevExpress "Implement Dependent Reference Properties".
    [DataSourceProperty("StokGrubu.StokAltGrupTanims", DataSourcePropertyIsNullMode.SelectNothing)]
    [RuleRequiredField("RuleRequired_StokTanim_StokAltGrubu", DefaultContexts.Save, "Lütfen Stok Alt Grubunu Seçiniz...",
        TargetCriteria = "[StokHizmetMasrafTipi] = 'Stok'")]
    [XafDisplayName("Stok Alt Grubu")]
    public StokAltGrupTanim StokAltGrubu
    {
        get => stokAltGrubu;
        set => SetPropertyValue(nameof(StokAltGrubu), ref stokAltGrubu, value);
    }

    // StokGrubu.StokTipiTanim'den türetilir; ayrı bir alan olarak SAKLANMAZ, yalnızca
    // listede/formda hızlı görünürlük için hesaplanan salt-okunur bir gezinme
    // (navigation) alanıdır. PersistentAlias sayesinde ListView'da gerçek,
    // filtrelenebilir/sıralanabilir bir kolon olarak çalışır. Setter YOK — StokGrubu
    // seçilince otomatik hesaplanır; seçilebilir bir lookup gibi görünüp de tıklamaya
    // tepki vermemesi kafa karıştırdığı için (kullanıcı geri bildirimi) Appearance ile
    // görsel olarak da salt-okunur/devre dışı yapılır.
    [PersistentAlias("StokGrubu.StokTipiTanim")]
    [Appearance("ED_StokTanim_StokTipi", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Stok Tipi")]
    public StokTipiTanim StokTipi => (StokTipiTanim)EvaluateAlias(nameof(StokTipi));

    // Yalnızca StokGrubu'nun bağlı olduğu StokTipiTanim.MamulMu=Evet ise
    // görünür/zorunlu (eski projedeki "StokTipi=Mamul ise Model alanı görünür"
    // deseninin veriye dayalı karşılığı — bkz. StokTipiTanim.MamulMu).
    [Association("ModelTanim-StokTanims")]
    [Appearance("IsVisible_StokTanim_Model", Visibility = ViewItemVisibility.Hide,
        Criteria = "StokGrubu.StokTipiTanim.MamulMu != True", Context = "DetailView")]
    // NOT: yorum "zorunlu" diyordu ama yalnızca görünürlük vardı, gerçek zorunluluk kuralı
    // eksikti — Mamul tipi stok Model'siz kaydedilebiliyordu (denetim raporunda bulundu,
    // doğrulandı). §45.2 Katman 5'teki "StokTipi=Mamul ise Model referansı zorunludur"
    // kararıyla artık tutarlı.
    [RuleRequiredField("RuleRequired_StokTanim_Model", DefaultContexts.Save, "Mamul tipi stok için Lütfen Modeli Seçiniz...",
        TargetCriteria = "StokGrubu.StokTipiTanim.MamulMu = True")]
    [XafDisplayName("Model")]
    public ModelTanim Model
    {
        get => model;
        set => SetPropertyValue(nameof(Model), ref model, value);
    }

    [RuleRequiredField("RuleRequired_StokTanim_BirimTanim", DefaultContexts.Save, "Lütfen Birimi Seçiniz...")]
    [XafDisplayName("Birim")]
    public BirimTanim BirimTanim
    {
        get => birimTanim;
        set => SetPropertyValue(nameof(BirimTanim), ref birimTanim, value);
    }

    [VisibleInListView(false)]
    [XafDisplayName("Döviz")]
    public DovizTanim DovizTanim
    {
        get => dovizTanim;
        set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
    }

    [VisibleInListView(false)]
    [XafDisplayName("KDV Oranı")]
    public KDVTanim KDVTanim
    {
        get => kdvTanim;
        set => SetPropertyValue(nameof(KDVTanim), ref kdvTanim, value);
    }

    // Manuel/referans "liste" fiyatıdır — Faz 2'de maliyet motorunun hesaplayacağı
    // OrtalamaMaliyet ile KARIŞTIRILMAMALI. Hizmet/Masraf'ta ve fiziksel Hammadde'de
    // (Mamul olmayan Stok) kullanılır. Mamul'de GİZLİ — kullanıcı kararı (2026-08-18):
    // Mamul'ün alış fiyatı anlamlı değil (satın alınmaz, üretilir) ve satış fiyatı
    // ileride yalnızca Fiyat Listesi'nden gelecek.
    [VisibleInListView(false)]
    [Appearance("IsVisible_StokTanim_AlisFiyati", Visibility = ViewItemVisibility.Hide,
        Criteria = "StokGrubu.StokTipiTanim.MamulMu = True", Context = "DetailView")]
    [XafDisplayName("Alış Fiyatı")]
    public decimal? AlisFiyati
    {
        get => alisFiyati;
        set => SetPropertyValue(nameof(AlisFiyati), ref alisFiyati, value);
    }

    // Manuel/referans liste satış fiyatıdır — Faz 6'da maliyet motorunun hesaplayacağı
    // önerilen satış fiyatı ile KARIŞTIRILMAMALI (o zaman bu alan varsayılan/override olarak kalır).
    // Mamul'de GİZLİ — bkz. AlisFiyati açıklaması.
    [VisibleInListView(false)]
    [Appearance("IsVisible_StokTanim_SatisFiyati", Visibility = ViewItemVisibility.Hide,
        Criteria = "StokGrubu.StokTipiTanim.MamulMu = True", Context = "DetailView")]
    [XafDisplayName("Satış Fiyatı")]
    public decimal? SatisFiyati
    {
        get => satisFiyati;
        set => SetPropertyValue(nameof(SatisFiyati), ref satisFiyati, value);
    }

    // Alış/Satış Fiyatı alanlarına girilen tutarın KDV dahil mi hariç mi olduğunu
    // belirtir — varsayılan Hariç (Türkiye'de liste/maliyet fiyatları genelde
    // KDV hariç girilir). Basic kullanıcı bir "*" ile karışıklık yaşamasın diye
    // ayrı bir onay/uyarı alanı DEĞİL, basit bir işaret kutusudur.
    [VisibleInListView(false)]
    [XafDisplayName("KDV Dahil mi?")]
    public bool KDVDahilMi
    {
        get => kdvDahilMi;
        set => SetPropertyValue(nameof(KDVDahilMi), ref kdvDahilMi, value);
    }

    // Kullanıcı kararı: bu alan ELLE girilmez — Satınalma Faturası onay akışı
    // (Faz 2/5, henüz bu projede yok) bu alanı otomatik günceller. Şimdilik
    // salt-okunur kalır; ileride ilgili Fatura onay Controller'ı buraya yazacak.
    [VisibleInListView(false)]
    [Appearance("ED_StokTanim_SonAlisFiyati", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Son Alış Fiyatı")]
    public decimal SonAlisFiyati
    {
        get => sonAlisFiyati;
        set => SetPropertyValue(nameof(SonAlisFiyati), ref sonAlisFiyati, value);
    }

    [VisibleInListView(false)]
    [XafDisplayName("En (Cm)")]
    public decimal En
    {
        get => en;
        set { if (SetPropertyValue(nameof(En), ref en, value)) HesaplaOlculer(); }
    }

    [VisibleInListView(false)]
    [XafDisplayName("Boy (Cm)")]
    public decimal Boy
    {
        get => boy;
        set { if (SetPropertyValue(nameof(Boy), ref boy, value)) HesaplaOlculer(); }
    }

    [VisibleInListView(false)]
    [XafDisplayName("Yükseklik (Cm)")]
    public decimal Yukseklik
    {
        get => yukseklik;
        set { if (SetPropertyValue(nameof(Yukseklik), ref yukseklik, value)) HesaplaOlculer(); }
    }

    [VisibleInListView(false)]
    [Appearance("ED_StokTanim_Metrekare", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Metrekare")]
    public decimal Metrekare
    {
        get => metrekare;
        set => SetPropertyValue(nameof(Metrekare), ref metrekare, value);
    }

    [VisibleInListView(false)]
    [Appearance("ED_StokTanim_MetreKup", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Metreküp")]
    public decimal MetreKup
    {
        get => metreKup;
        set => SetPropertyValue(nameof(MetreKup), ref metreKup, value);
    }

    [VisibleInListView(false)]
    [XafDisplayName("Ağırlık")]
    public decimal Agirlik
    {
        get => agirlik;
        set => SetPropertyValue(nameof(Agirlik), ref agirlik, value);
    }

    // Faz 2 (StokHareketleriM/D) Ağırlıklı Ortalama Maliyet motoru tarafından günceller —
    // tüm depoların toplamıdır (depo bazlı bakiye için bkz. StokBakiye). Kullanıcı elle
    // değiştirmez.
    [VisibleInListView(false)]
    [Appearance("ED_StokTanim_ToplamMiktar", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Toplam Miktar")]
    public decimal ToplamMiktar
    {
        get => toplamMiktar;
        set => SetPropertyValue(nameof(ToplamMiktar), ref toplamMiktar, value);
    }

    // Hareketli ağırlıklı ortalama, TL (Yerel) bazında — yalnızca Giriş hareketlerinde
    // yeniden hesaplanır, Çıkış hareketlerinde DEĞİŞMEZ (bkz. IWeightedAverageCostService).
    [VisibleInListView(false)]
    [Appearance("ED_StokTanim_OrtalamaMaliyet", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Ortalama Maliyet")]
    public decimal OrtalamaMaliyet
    {
        get => ortalamaMaliyet;
        set => SetPropertyValue(nameof(OrtalamaMaliyet), ref ortalamaMaliyet, value);
    }

    // NOT: önceden yalnızca En/Boy/Yükseklik sıfırdan farklıyken hesaplanıyordu — ölçü
    // sıfırlanınca Metrekare/MetreKüp ESKİ DEĞERDE DONUK KALIYORDU (denetim raporunda
    // bulundu, doğrulandı). Artık koşulsuz her zaman yeniden hesaplanıyor.
    void HesaplaOlculer()
    {
        Metrekare = (En * Boy) / 10000;
        MetreKup = (En * Boy * Yukseklik) / 1000000;
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this) && string.IsNullOrEmpty(StokKodu))
        {
            IStokKoduJeneratoru jenerator = new StokKoduJeneratoru();
            StokKodu = jenerator.SonrakiStokKodu(Session, this);
        }
        base.OnSaving();
    }

    // CariHesapTanim.OnDeleting ile aynı üslup (Faz 2): bu stoğa ait hareket kaydı varsa
    // silme reddedilir — aksi halde StokHareketleriD orphan FK/çiğ Postgres hatası riski taşır.
    protected override void OnDeleting()
    {
        try
        {
            if (new XPQuery<StokHareketleriD>(Session).Count(x => x.StokTanim == this) > 0)
                throw new UserFriendlyException("Bu stoğa ait hareket kaydı var, silemezsiniz.");
            base.OnDeleting();
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Tracing.Tracer.LogError(ex);
            throw;
        }
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        // StokTanimYeniKayitVarsayilanlariController yeni kayıtta Fiş Türü'nü atadığında,
        // o Fiş Türü'ne tanımlı aktif varsayılan değerler (ör. HZMTTN için varsayılan Döviz/
        // KDV) varsa otomatik uygulanır. Kullanıcı sonradan Döviz/KDV'yi elle değiştirirse
        // bu değişiklik onun üzerine yazar (varsayılan yalnızca bir kez, atama anında uygulanır).
        if (propertyName == nameof(FisTuruTanim) && newValue != null && !IsLoading)
            FisTuruVarsayilanlariniUygula((FisTuruTanim)newValue);
    }
}
