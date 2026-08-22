/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokHareketleriD.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok hareket fişi satırı (Faz 2) — motor orkestrasyonunun
 *                    tamamı burada: Ağırlıklı Ortalama Maliyet (IWeightedAverage-
 *                    CostService), StokBakiye bul-veya-oluştur, negatif stok
 *                    politikası, silme-sonrası tam geçmiş replay. Eski projeden
 *                    birebir port.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using SofasisERP.Module.Services;
using System.ComponentModel;
using System.Linq;

namespace SofasisERP.Module.BusinessObjects;

// Bakiye negatife düştüyse (NegatifStokPolitikasi.Uyar) kırmızı/kalın vurgulanır.
[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Stok Hareketi Satırı")]
[Appearance("NegatifBakiyeUyarisiColor", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "NegatifBakiyeUyarisi = True", Context = "ListView",
    FontColor = "Red", FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]
public class StokHareketleriD : BaseClassWithAuditAndDescription
{
    public StokHareketleriD(Session session) : base(session) { }

    StokHareketleriM stokHareketleriM;

    // KRİTİK: SetPropertyValue KULLANILIR (auto-property DEĞİL) — XPO'nun [Association]
    // çift-yönlü senkronizasyonu (bu satır set edildiğinde StokHareketleriM.StokHareketleriDs
    // koleksiyonuna otomatik eklenmesi) SetPropertyValue'nün içindeki mekanizmaya bağlıdır.
    // Auto-property kullanılırsa programatik oluşturulan satırlar StokHareketleriDs.Count'ta
    // hiç görünmez (eski projede canlı testte kanıtlanmış bir hata).
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [Association("StokHareketleriM-StokHareketleriDs")]
    [RuleRequiredField("RuleRequired_StokHareketleriD_StokHareketleriM", DefaultContexts.Save, "Satır bir fiş başlığına bağlı olmalıdır.")]
    public StokHareketleriM StokHareketleriM
    {
        get => stokHareketleriM;
        set => SetPropertyValue(nameof(StokHareketleriM), ref stokHareketleriM, value);
    }

    StokTanim stokTanim;
    decimal miktar;
    decimal birimMaliyet;
    decimal toplamMaliyet;
    DovizTanim dovizTanim;
    decimal? dovizKuru;
    decimal yerelBirimMaliyet;
    decimal yerelToplamMaliyet;
    bool negatifBakiyeUyarisi;
    Type kaynakBelgeTipi;
    Guid? kaynakBelgeOid;
    bool motorIslendi;

    [DataSourceCriteria("StokHizmetMasrafTipi = 'Stok'")]
    [XafDisplayName("Stok")]
    [RuleRequiredField("RuleRequired_StokHareketleriD_StokTanim", DefaultContexts.Save, "Lütfen Stok Seçiniz...")]
    public StokTanim StokTanim
    {
        get => stokTanim;
        set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
    }

    [XafDisplayName("Miktar")]
    [RuleValueComparison("Rule_StokHareketleriD_Miktar_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Lütfen miktarı sıfırdan büyük giriniz.")]
    public decimal Miktar
    {
        get => miktar;
        set => SetPropertyValue(nameof(Miktar), ref miktar, value);
    }

    // Giriş satırlarında kullanıcı girer (StokTanim'in kendi DovizTanim'ine göre dövizli
    // olabilir — motorun okuduğu gerçek TL değeri YerelBirimMaliyet'tir, bu alan DEĞİL).
    // Çıkış satırlarında motor kaydetme anında StokTanim.OrtalamaMaliyet'e (TL) sabitler —
    // kullanıcı çıkış maliyeti seçmez.
    //
    // NOT: >0 kontrolü BURADA declarative olarak da tanımlanır (TargetCriteria ile yalnızca
    // Giriş yönünde) — Miktar'daki RuleValueComparison ile aynı desen. Önceden bu kontrol
    // yalnızca ObjectSaving()'de imperative throw ile yapılıyordu; kullanıcı raporu: bu,
    // kaydetme anında alan üzerinde kırmızı satır içi hata yerine genel bir exception
    // diyaloğu (debugger altında "unhandled exception" gibi görünen) üretiyordu. Declarative
    // kural artık kullanıcıya save denemesinden ÖNCE, doğru alanda gösteriyor.
    // ObjectSaving()'deki kontrol KALDIRILMADI — motor matematiği (ağırlıklı ortalama) için
    // son bir savunma hattı olarak kalıyor (programatik/gelecekteki İrsaliye entegrasyonu gibi
    // bu declarative kuralı atlayabilecek yollara karşı).
    [RuleValueComparison("Rule_StokHareketleriD_BirimMaliyet_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        TargetCriteria = "StokHareketleriM.FisTuruTanim.StokHareketYonu = 'Giris'",
        CustomMessageTemplate = "Lütfen giriş için birim maliyeti sıfırdan büyük giriniz.")]
    [Appearance("ED_StokHareketleriD_BirimMaliyet", Enabled = false,
        Criteria = "StokHareketleriM.FisTuruTanim.StokHareketYonu = 'Cikis'", Context = "DetailView")]
    [XafDisplayName("Birim Maliyet")]
    public decimal BirimMaliyet
    {
        get => birimMaliyet;
        set
        {
            SetPropertyValue(nameof(BirimMaliyet), ref birimMaliyet, value);
            YerelMaliyetiHesapla();
        }
    }

    [Appearance("ED_StokHareketleriD_ToplamMaliyet", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Toplam Maliyet")]
    public decimal ToplamMaliyet
    {
        get => toplamMaliyet;
        set => SetPropertyValue(nameof(ToplamMaliyet), ref toplamMaliyet, value);
    }

    [Appearance("ED_StokHareketleriD_DovizTanim", Enabled = false,
        Criteria = "StokHareketleriM.FisTuruTanim.StokHareketYonu = 'Cikis'", Context = "DetailView")]
    [XafDisplayName("Döviz Kodu")]
    public DovizTanim DovizTanim
    {
        get => dovizTanim;
        set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
    }

    [Appearance("ED_StokHareketleriD_DovizKuru", Enabled = false,
        Criteria = "StokHareketleriM.FisTuruTanim.StokHareketYonu = 'Cikis'", Context = "DetailView")]
    [XafDisplayName("Döviz Kuru")]
    public decimal? DovizKuru
    {
        get => dovizKuru;
        set
        {
            SetPropertyValue(nameof(DovizKuru), ref dovizKuru, value);
            YerelMaliyetiHesapla();
        }
    }

    // BirimMaliyet'teki desenin aynısı: döviz kuru bulunamayıp bu alan sıfırda kalırsa
    // (ör. o gün için DovizGunlukKurM/D kaydı yok) kullanıcı artık save denemesinden önce
    // alan üzerinde net bir mesaj görür — ObjectSaving()'deki eşdeğer kontrol savunma
    // hattı olarak kalır.
    [RuleValueComparison("Rule_StokHareketleriD_YerelBirimMaliyet_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        TargetCriteria = "StokHareketleriM.FisTuruTanim.StokHareketYonu = 'Giris'",
        CustomMessageTemplate = "Döviz kuru bulunamadı veya birim maliyetin TL karşılığı sıfır — lütfen Döviz Kodu/Kuru bilgisini kontrol ediniz.")]
    [Appearance("ED_StokHareketleriD_YerelBirimMaliyet", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Yerel Birim Maliyet")]
    public decimal YerelBirimMaliyet
    {
        get => yerelBirimMaliyet;
        set => SetPropertyValue(nameof(YerelBirimMaliyet), ref yerelBirimMaliyet, value);
    }

    [VisibleInListView(false)]
    [Appearance("ED_StokHareketleriD_YerelToplamMaliyet", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Yerel Toplam Maliyet")]
    public decimal YerelToplamMaliyet
    {
        get => yerelToplamMaliyet;
        set => SetPropertyValue(nameof(YerelToplamMaliyet), ref yerelToplamMaliyet, value);
    }

    [VisibleInDetailView(false)]
    [XafDisplayName("Negatif Bakiye Uyarısı")]
    public bool NegatifBakiyeUyarisi
    {
        get => negatifBakiyeUyarisi;
        set => SetPropertyValue(nameof(NegatifBakiyeUyarisi), ref negatifBakiyeUyarisi, value);
    }

    // İrsaliye/Fatura (bu projede henüz yok) ileride bu satıra bağlanacağı genişletme kancası —
    // bu turda yalnızca StokTransferi'nin kendi iki (Çıkış+Giriş) satırını eşleştirmek için
    // kullanılır. ValueConverter ZORUNLU: System.Type XPO'nun native desteklediği bir tip
    // DEĞİLDİR, converter olmadan bu alan sessizce non-persistent kalır.
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    [VisibleInReports(false)]
    [HideInUI(HideInUI.All)]
    [Browsable(false)]
    [XafDisplayName("Kaynak Belge Tipi")]
    [ValueConverter(typeof(TypeToStringConverter)), ImmediatePostData]
    [TypeConverter(typeof(LocalizedClassInfoTypeConverter))]
    public Type KaynakBelgeTipi
    {
        get => kaynakBelgeTipi;
        set => SetPropertyValue(nameof(KaynakBelgeTipi), ref kaynakBelgeTipi, value);
    }

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    [VisibleInLookupListView(false)]
    [VisibleInReports(false)]
    [HideInUI(HideInUI.All)]
    [Browsable(false)]
    [XafDisplayName("Kaynak Belge Oid")]
    public Guid? KaynakBelgeOid
    {
        get => kaynakBelgeOid;
        set => SetPropertyValue(nameof(KaynakBelgeOid), ref kaynakBelgeOid, value);
    }

    // Motor mantığının EN FAZLA BİR KEZ çalıştığını garanti eder. Master-Detail popup akışında
    // (satır eklenip "Kaydet") XAF her popup kapanışında ObjectSpace'i flush edebilir; bu,
    // aynı "yeni" satır için ObjectSaving()'in Session.IsNewObject hâlâ true dönse bile birden
    // fazla kez tetiklenmesine yol açabilir — bayraksız motor aynı satırı iki kez işler,
    // StokBakiye unique index ihlaline yol açar (eski projede canlı testte kanıtlanmış).
    [Browsable(false)]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public bool MotorIslendi
    {
        get => motorIslendi;
        set => SetPropertyValue(nameof(MotorIslendi), ref motorIslendi, value);
    }

    void YerelMaliyetiHesapla()
    {
        if (BirimMaliyet > 0)
            YerelBirimMaliyet = DovizKuru.GetValueOrDefault() * BirimMaliyet;
    }

    // CariHesapHareketleri.OnChanged'in portu: StokTanim seçilince o stoğun kendi para birimi
    // miras alınır (kullanıcı isterse değiştirebilir). DovizTanim'e declarative RuleRequiredField
    // BİLİNÇLİ OLARAK eklenmez: Çıkış satırında kullanıcı hiç döviz seçmez (motor ObjectSaving'de
    // TRY'ye sabitler) — koşulsuz bir RuleRequiredField bu satırın hiç kaydedilememesine yol açardı.
    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (propertyName == nameof(StokTanim) && newValue != null && !IsLoading)
        {
            DovizTanim = ((StokTanim)newValue).DovizTanim;
            if (DovizTanim != null)
                DovizKuruGuncelle(DovizTanim);
        }
        if (propertyName == nameof(DovizTanim) && newValue != null && !IsLoading)
            DovizKuruGuncelle((DovizTanim)newValue);

        YerelMaliyetiHesapla();
    }

    // CariHesapHareketleri.DovizKuruGuncelle'nin portu — TEK FARK: StokHareketleriD'nin kendi
    // FisTarihi alanı yok, StokHareketleriM.FisTarihi okunur (Master henüz atanmamışsa çıkılır).
    void DovizKuruGuncelle(DovizTanim doviz)
    {
        if (StokHareketleriM == null) return;

        if (doviz.DovizKodu == "TRY")
        {
            DovizKuru = 1;
            return;
        }
        DovizGunlukKurM kurMaster = Session.FindObject<DovizGunlukKurM>(
            DevExpress.Data.Filtering.CriteriaOperator.FromLambda<DovizGunlukKurM>(x => x.KurTarihi == StokHareketleriM.FisTarihi));
        DovizGunlukKurD kurDetay = kurMaster?.DovizGunlukKurDetails.FirstOrDefault(x => x.DovizTanim == doviz);
        DovizKuru = kurDetay?.DovizSatis;
    }

    public override void ObjectSaving()
    {
        if (MotorIslendi)
            return; // Motor bu satır için zaten çalıştı — tekrar flush edilse de yeniden çalışmaz.

        if (StokHareketleriM == null)
            throw new UserFriendlyException("Stok hareketi bir fiş başlığına bağlı olmalıdır.");
        if (StokHareketleriM.Depo == null)
            throw new UserFriendlyException("Lütfen fiş başlığında Depo seçiniz.");
        if (StokHareketleriM.FisTuruTanim == null)
            throw new UserFriendlyException("Lütfen fiş başlığında Fiş Türünü seçiniz.");
        if (StokTanim == null)
            throw new UserFriendlyException("Lütfen Stok seçiniz.");
        if (Miktar <= 0)
            throw new UserFriendlyException("Lütfen miktarı sıfırdan büyük giriniz.");

        StokHareketYonu yon = StokHareketleriM.FisTuruTanim.StokHareketYonu;
        if (yon == StokHareketYonu.Yok)
            throw new UserFriendlyException("Seçilen fiş türü bir stok hareketi üretmek için tanımlanmamış.");

        DepoTanim depo = StokHareketleriM.Depo;
        IWeightedAverageCostService maliyetServisi = new WeightedAverageCostService();

        StokBakiye bakiye = Session.FindObject<StokBakiye>(
            new DevExpress.Data.Filtering.GroupOperator(DevExpress.Data.Filtering.GroupOperatorType.And,
                new DevExpress.Data.Filtering.BinaryOperator(nameof(StokBakiye.StokTanim), StokTanim),
                new DevExpress.Data.Filtering.BinaryOperator(nameof(StokBakiye.Depo), depo)));
        bakiye ??= new StokBakiye(Session) { StokTanim = StokTanim, Depo = depo, Miktar = 0 };

        if (yon == StokHareketYonu.Giris)
        {
            if (BirimMaliyet <= 0)
                throw new UserFriendlyException("Lütfen giriş için birim maliyeti sıfırdan büyük giriniz.");
            if (YerelBirimMaliyet <= 0)
                throw new UserFriendlyException("Döviz kuru bulunamadı veya birim maliyetin TL karşılığı sıfır — lütfen Döviz Kodu/Kuru bilgisini kontrol ediniz.");

            // Motor HER ZAMAN TL (Yerel) okur — BirimMaliyet döviz bazlı olabilir, ağırlıklı
            // ortalama maliyet farklı dövizlerden giren stoklarda bile tutarlı kalır.
            (decimal yeniMiktar, decimal yeniOrtalama) = maliyetServisi.GirisUygula(
                StokTanim.ToplamMiktar, StokTanim.OrtalamaMaliyet, Miktar, YerelBirimMaliyet);

            StokTanim.ToplamMiktar = yeniMiktar;
            StokTanim.OrtalamaMaliyet = yeniOrtalama;
            bakiye.Miktar += Miktar;
        }
        else
        {
            // Çıkışta ortalama maliyet ASLA değişmez — o anki ortalamaya sabitlenir. Döviz
            // alanları da TRY/1'e sabitlenir: motor yalnızca miktar okur, bu yalnızca ekranda
            // Giriş/Çıkış satırlarının aynı alan setini anlamlı doldurması içindir.
            BirimMaliyet = StokTanim.OrtalamaMaliyet;
            DovizTanim = Session.FindObject<DovizTanim>(
                DevExpress.Data.Filtering.CriteriaOperator.FromLambda<DovizTanim>(x => x.DovizKodu == "TRY"));
            DovizKuru = 1;
            YerelBirimMaliyet = BirimMaliyet;

            (decimal yeniToplamMiktar, _) = maliyetServisi.CikisUygula(
                StokTanim.ToplamMiktar, StokTanim.OrtalamaMaliyet, Miktar);
            decimal yeniDepoBakiye = bakiye.Miktar - Miktar;

            // Negatif stok politikası, nesnelere YAZILMADAN ÖNCE hem depo-bazlı hem
            // GLOBAL toplam için kontrol edilir (22.08.2026 denetimi G3) — önceki halde
            // yalnızca depo bakiyesi kontrol ediliyordu ve kontrol mutasyondan SONRA
            // çalışıyordu (Engelle durumunda bile nesneler bellekte kirleniyordu).
            if ((yeniDepoBakiye < 0 || yeniToplamMiktar < 0))
            {
                StokParametre parametre = Session.GetObjectsToSave().OfType<StokParametre>().FirstOrDefault()
                    ?? new XPQuery<StokParametre>(Session).FirstOrDefault();
                NegatifStokPolitikasi politika = parametre?.NegatifStokPolitikasi ?? NegatifStokPolitikasi.Uyar;
                if (politika == NegatifStokPolitikasi.Engelle)
                    throw new UserFriendlyException($"'{depo.DepoAdi}' deposunda '{StokTanim.StokAdi}' için yeterli bakiye yok.");
                if (politika == NegatifStokPolitikasi.Uyar)
                    NegatifBakiyeUyarisi = true;
            }

            StokTanim.ToplamMiktar = yeniToplamMiktar;
            bakiye.Miktar = yeniDepoBakiye;
        }

        ToplamMaliyet = Math.Round(Miktar * BirimMaliyet, 2, MidpointRounding.AwayFromZero);
        YerelToplamMaliyet = Math.Round(Miktar * YerelBirimMaliyet, 2, MidpointRounding.AwayFromZero);
        MotorIslendi = true;
    }

    public override void ObjectDeleting()
    {
        DepoTanim depo = StokHareketleriM.Depo;
        StokHareketYonu yon = StokHareketleriM.FisTuruTanim.StokHareketYonu;

        StokBakiye bakiye = Session.FindObject<StokBakiye>(
            new DevExpress.Data.Filtering.GroupOperator(DevExpress.Data.Filtering.GroupOperatorType.And,
                new DevExpress.Data.Filtering.BinaryOperator(nameof(StokBakiye.StokTanim), StokTanim),
                new DevExpress.Data.Filtering.BinaryOperator(nameof(StokBakiye.Depo), depo)));
        if (bakiye != null)
            bakiye.Miktar += yon == StokHareketYonu.Giris ? -Miktar : Miktar;

        // Global ortalama, bu StokTanim'in TÜM kalan hareketlerinin CreatedDate sırasıyla
        // yeniden oynatılmasıyla (replay) hesaplanır — silinen satır rastgele bir geçmiş kayıt
        // olabilir, yalnızca son kaydı geri almak yanlış sonuç üretir. Aynı kaynak belgeye
        // (StokTransferi'nin kardeş satırı) ait diğer satırlar replay dışında tutulur — o satır
        // da AYNI anda silinme sürecinde olabilir, dahil edilirse yanlış sonuç/NullReference riski.
        var kalanHareketler = new XPQuery<StokHareketleriD>(Session)
            .Where(x => x.StokTanim == StokTanim && x.Oid != Oid)
            .ToList()
            .Where(x => !(KaynakBelgeTipi != null && x.KaynakBelgeTipi == KaynakBelgeTipi && x.KaynakBelgeOid == KaynakBelgeOid))
            .OrderBy(x => x.CreatedDate)
            .Select(x => (
                Giris: x.StokHareketleriM.FisTuruTanim.StokHareketYonu == StokHareketYonu.Giris,
                x.Miktar,
                BirimMaliyet: x.YerelBirimMaliyet));

        IWeightedAverageCostService maliyetServisi = new WeightedAverageCostService();
        (decimal yeniToplamMiktar, decimal yeniOrtalama) = maliyetServisi.YenidenHesapla(kalanHareketler);

        StokTanim.ToplamMiktar = yeniToplamMiktar;
        StokTanim.OrtalamaMaliyet = yeniOrtalama;
    }
}
