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
            cariHesapTipi = CariHesapTipi.Musteri;
            kDVTipi = KDVTipi.KDVHaric;
            DovizTanim = Session.FindObject<DovizTanim>(
                CriteriaOperator.FromLambda<DovizTanim>(x => x.IsVarsayilan));
            FisTuruTanim = Session.FindObject<FisTuruTanim>(
                CriteriaOperator.FromLambda<FisTuruTanim>(x => x.FisTuruKodu == "CARITN"));
        }
    }

    string cariHesapKodu;
    CariHesapTipi cariHesapTipi;
    KDVTipi kDVTipi;
    CariGrupTanim cariGrupTanim;
    CariFirmaHesapTuru cariHesapTuru;
    string tCKimlikVergiNo;
    string vergiDairesi;
    string ePostaAdresi;
    string webAdresi;
    string telefon1;
    string telefon2;
    FisTuruTanim fisTuruTanim;

    [Size(32)]
    [Indexed(Unique = true)]
    [Appearance("ED_CariHesapTanim_CariHesapKodu", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Cari Hesap Kodu")]
    public string CariHesapKodu
    {
        get => cariHesapKodu;
        set => SetPropertyValue(nameof(CariHesapKodu), ref cariHesapKodu, value);
    }

    [XafDisplayName("Cari Hesap Tipi")]
    public CariHesapTipi CariHesapTipi
    {
        get => cariHesapTipi;
        set => SetPropertyValue(nameof(CariHesapTipi), ref cariHesapTipi, value);
    }

    [XafDisplayName("KDV H/D")]
    public KDVTipi KDVTipi
    {
        get => kDVTipi;
        set => SetPropertyValue(nameof(KDVTipi), ref kDVTipi, value);
    }

    [XafDisplayName("Cari Hesap Grubu")]
    public CariGrupTanim CariGrupTanim
    {
        get => cariGrupTanim;
        set => SetPropertyValue(nameof(CariGrupTanim), ref cariGrupTanim, value);
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
    // CariHesapKodu numaralandırması için kullanılır (bkz. NumberSequenceService.SonrakiNumara).
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

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this) && string.IsNullOrEmpty(CariHesapKodu) && FisTuruTanim != null)
        {
            INumberSequenceService numberSequenceService = new NumberSequenceService();
            CariHesapKodu = numberSequenceService.SonrakiNumara(Session, "CariHesapTanim", FisTuruTanim, TurkiyeZamani.Bugun);
        }
        base.OnSaving();
    }
}
