/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariHesapTanim.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Cari Hesap (Müşteri/Tedarikçi) kartı — eski ERP'den
 *                    kullanıcı kararıyla (2026-08-18) aynen taşındı, iki
 *                    istisna: (1) SatisFiyatListeM alanı YOK (Fiyat Listesi
 *                    Faz 6.5'e kadar yok), (2) sabit Ev/İş Adresi ikilisi
 *                    yerine esnek CariAdresleri + CariYetkiliTanimlari
 *                    Aggregated koleksiyonları (bkz. CariAdresTanim,
 *                    CariYetkiliTanim) — kullanıcı isteği.
 *                    Faz 3 düzeltmesi (2026-08-19): artık Hesap'tan türer —
 *                    CariHesapAdi/DovizTanim alanları kaldırıldı, taban sınıftaki
 *                    HesapAdi/DovizTanim/IsVarsayilan/GuncelBakiye kullanılır.
 *                    Kasa/Banka ile aynı ailede olmak, MuhasebeFisSatiri'nin TEK
 *                    bir Yevmiye Fişinde Kasa/Banka/Cari'yi birlikte
 *                    etkileyebilmesini sağlar (kullanıcı eleştirisi, bkz. Hesap.cs).
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using SofasisERP.Module.Services;
using System;
using System.ComponentModel;
using System.Linq;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(HesapAdi))]
[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Cari Hesap Tanımlama")]
public class CariHesapTanim : Hesap
{
    public CariHesapTanim(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            HesapTuru = HesapTuru.Cari;
            kDVTipi = KDVTipi.KDVHaric;
            DovizTanim = Session.FindObject<DovizTanim>(
                CriteriaOperator.FromLambda<DovizTanim>(x => x.IsVarsayilan));
            FisTuruTanim = Session.FindObject<FisTuruTanim>(
                CriteriaOperator.FromLambda<FisTuruTanim>(x => x.FisTuruKodu == "CARITN"));
        }
    }

    string cariHesapKodu;
    CariHesapGrupTanim cariHesapGrubu;
    CariHesapAltGrupTanim cariHesapAltGrubu;
    KDVTipi kDVTipi;
    CariFirmaHesapTuru cariHesapTuru;
    string tCKimlikVergiNo;
    string vergiDairesi;
    string ePostaAdresi;
    string webAdresi;
    string telefon1;
    string telefon2;
    FisTuruTanim fisTuruTanim;
    bool cariHesapAltGrubuDegisti;

    [Size(32)]
    [Indexed(Unique = true)]
    [Appearance("ED_CariHesapTanim_CariHesapKodu", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Cari Hesap Kodu")]
    public string CariHesapKodu
    {
        get => cariHesapKodu;
        set => SetPropertyValue(nameof(CariHesapKodu), ref cariHesapKodu, value);
    }

    // Kod jeneratörünün 2. seviyesi — kullanıcı BUNU önce seçer (StokTanim.StokGrubu
    // ile aynı kademeli/bağımlı lookup deseni: ImmediatePostData ile seçim yapılır
    // yapılmaz Alt Grubu lookup'ı anında yeniden filtrelenir).
    [ImmediatePostData]
    [XafDisplayName("Cari Hesap Grubu")]
    public CariHesapGrupTanim CariHesapGrubu
    {
        get => cariHesapGrubu;
        set => SetPropertyValue(nameof(CariHesapGrubu), ref cariHesapGrubu, value);
    }

    // Kod jeneratörünün 3. seviyesi — kullanıcı BUNU seçer (ör. "Yurtiçi Toptan
    // Alıcılar"), CariHesapKodu ve CariHesapSinifi buradan türetilir (bkz.
    // CariHesapKoduJeneratoru, CariHesapSinifi salt-okunur özelliği aşağıda).
    // Lookup'ı CariHesapGrubu.CariHesapAltGrupTanims koleksiyonuyla sınırlar —
    // CariHesapGrubu boşken hiçbir seçenek gösterilmez (StokTanim.StokAltGrubu ile
    // aynı desen). CariHesapParametre.CariHesapKoduUretimYontemi=Otomatik iken bu
    // alan KULLANILMAZ, bu yüzden unconditional RuleRequiredField YOK — zorunluluk
    // yalnızca Jenerator modunda CariHesapKoduJeneratoru içinde kontrol edilir
    // (StokTanim.StokAltGrubu'nun TargetCriteria'sından farklı bir çözüm: burada
    // koşul ayrı bir singleton parametre nesnesine bağlı olduğu için model
    // seviyesinde TargetCriteria ile ifade edilemiyor).
    [DataSourceProperty("CariHesapGrubu.CariHesapAltGrupTanims", DataSourcePropertyIsNullMode.SelectNothing)]
    [XafDisplayName("Cari Hesap Alt Grubu")]
    public CariHesapAltGrupTanim CariHesapAltGrubu
    {
        get => cariHesapAltGrubu;
        set => SetPropertyValue(nameof(CariHesapAltGrubu), ref cariHesapAltGrubu, value);
    }

    // CariHesapGrubu.CariHesapTipiTanim'den türetilir; ayrı bir alan olarak
    // SAKLANMAZ, formda en üstte salt-okunur bilgi amaçlı gösterilir (StokTanim.
    // StokTipi ile birebir aynı desen — PersistentAlias + Appearance Enabled=false).
    [PersistentAlias("CariHesapGrubu.CariHesapTipiTanim")]
    [Appearance("ED_CariHesapTanim_CariHesapTipiTanim", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Cari Hesap Tipi")]
    public CariHesapTipiTanim CariHesapTipiTanim => (CariHesapTipiTanim)EvaluateAlias(nameof(CariHesapTipiTanim));

    // Eski "CariHesapTipi" alanının yerini alan salt-okunur kolaylık özelliği —
    // KasaCariBankaHareketleri.cs gibi çağıranlar CariHesapAltGrubu zincirini
    // (AltGrup->Grup->Tipi) elle dolaşmak yerine bunu okur. Tek kaynak: Alt Grup
    // seçimi (StokTanim'in StokAltGrubu'ndan MamulMu okuması ile aynı desen).
    [VisibleInDetailView(false)]
    [XafDisplayName("Cari Hesap Sınıfı")]
    public CariHesapTipi CariHesapSinifi
        => CariHesapAltGrubu?.CariHesapGrupTanim?.CariHesapTipiTanim?.CariHesapSinifi ?? CariHesapTipi.Musteri;

    [XafDisplayName("KDV H/D")]
    public KDVTipi KDVTipi
    {
        get => kDVTipi;
        set => SetPropertyValue(nameof(KDVTipi), ref kDVTipi, value);
    }

    [VisibleInListView(false)]
    [XafDisplayName("Firma Türü")]
    public CariFirmaHesapTuru CariHesapTuru
    {
        get => cariHesapTuru;
        set => SetPropertyValue(nameof(CariHesapTuru), ref cariHesapTuru, value);
    }

    [VisibleInListView(false)]
    [Size(11)]
    [XafDisplayName("TC Kimlik / Vergi No")]
    public string TCKimlikVergiNo
    {
        get => tCKimlikVergiNo;
        set => SetPropertyValue(nameof(TCKimlikVergiNo), ref tCKimlikVergiNo, value);
    }

    [VisibleInListView(false)]
    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [XafDisplayName("Vergi Dairesi")]
    public string VergiDairesi
    {
        get => vergiDairesi;
        set => SetPropertyValue(nameof(VergiDairesi), ref vergiDairesi, value);
    }

    [VisibleInListView(false)]
    [Size(100)]
    [RuleRegularExpression(DefaultContexts.Save, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
        CustomMessageTemplate = "Lütfen Geçerli Bir E Posta Adresi Giriniz...")]
    [XafDisplayName("E Posta Adresi")]
    public string EPostaAdresi
    {
        get => ePostaAdresi;
        set => SetPropertyValue(nameof(EPostaAdresi), ref ePostaAdresi, value);
    }

    [VisibleInListView(false)]
    [Size(100)]
    [XafDisplayName("Web Adresi")]
    public string WebAdresi
    {
        get => webAdresi;
        set => SetPropertyValue(nameof(WebAdresi), ref webAdresi, value);
    }

    [VisibleInListView(false)]
    [Size(16)]
    [XafDisplayName("Telefon 1")]
    public string Telefon1
    {
        get => telefon1;
        set => SetPropertyValue(nameof(Telefon1), ref telefon1, value);
    }

    [VisibleInListView(false)]
    [Size(16)]
    [XafDisplayName("Telefon 2")]
    public string Telefon2
    {
        get => telefon2;
        set => SetPropertyValue(nameof(Telefon2), ref telefon2, value);
    }

    // Kullanıcı tarafından seçilmez — AfterConstruction'da "CARITN" atanır, yalnızca
    // CariHesapParametre.CariHesapKoduUretimYontemi=Otomatik iken CariHesapKodu
    // numaralandırması için kullanılır (bkz. NumberSequenceService.SonrakiNumara).
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [XafDisplayName("Fiş Türü")]
    public FisTuruTanim FisTuruTanim
    {
        get => fisTuruTanim;
        set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
    }

    [XafDisplayName("Adresler")]
    [Association("CariHesapTanim-CariAdresleri"), Aggregated]
    public XPCollection<CariAdresTanim> CariAdresleri
        => GetCollection<CariAdresTanim>(nameof(CariAdresleri));

    [XafDisplayName("Yetkililer")]
    [Association("CariHesapTanim-CariYetkiliTanimlari"), Aggregated]
    public XPCollection<CariYetkiliTanim> CariYetkiliTanimlari
        => GetCollection<CariYetkiliTanim>(nameof(CariYetkiliTanimlari));

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        // Var olan (zaten kaydedilmiş) bir kayıtta Cari Hesap Alt Grubu değişirse, OnSaving()
        // CariHesapKodu'nu yeniden üretir — bkz. OnSaving() üstündeki ayrıntılı açıklama.
        if (propertyName == nameof(CariHesapAltGrubu) && !IsLoading && !Session.IsNewObject(this))
            cariHesapAltGrubuDegisti = true;
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this) && string.IsNullOrEmpty(CariHesapKodu))
        {
            ICariHesapKoduJeneratoru jenerator = new CariHesapKoduJeneratoru();
            CariHesapKodu = jenerator.SonrakiCariHesapKodu(Session, this);
        }
        else if (!Session.IsNewObject(this) && cariHesapAltGrubuDegisti && JenaratorModuAktifMi(Session))
        {
            // 4-ajanlı testte (2026-08-22, kıdemli muhasebeci bulgusu) tespit edildi:
            // kullanıcı yanlış seçilen bir Cari Hesap Alt Grubu'nu sonradan düzeltirse,
            // CariHesapKodu (Jenerator modunda alt gruptan türeyen) ESKİ sınıflandırmayı
            // göstermeye devam ediyordu — yalnızca boşken üretiliyordu. Alt Grup değişince
            // kod da yeniden üretilir (Otomatik modunda AltGrubu kodu belirlemediği için
            // dokunulmaz — CariHesapSinifi zaten AltGrubu'ndan CANLI hesaplandığı için o
            // ayrıca güncellenmeye ihtiyaç duymuyor, bkz. yukarıdaki hesaplanan özellik).
            ICariHesapKoduJeneratoru jenerator = new CariHesapKoduJeneratoru();
            CariHesapKodu = jenerator.SonrakiCariHesapKodu(Session, this);
        }
        cariHesapAltGrubuDegisti = false;
        base.OnSaving();
    }

    static bool JenaratorModuAktifMi(Session session)
    {
        CariHesapParametre parametre = session.GetObjectsToSave().OfType<CariHesapParametre>().FirstOrDefault()
            ?? session.FindObject<CariHesapParametre>(null);
        return (parametre?.CariHesapKoduUretimYontemi ?? CariHesapKoduUretimYontemi.Jenerator) == CariHesapKoduUretimYontemi.Jenerator;
    }
}
