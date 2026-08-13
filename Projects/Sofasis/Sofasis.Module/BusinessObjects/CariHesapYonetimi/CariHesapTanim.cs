using DevExpress.Data.Filtering;
using DevExpress.DataAccess.Excel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using Sofasis.Module.Services;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects;

[DefaultProperty("CariHesapAdi")]
[DefaultClassOptions]
[XafDisplayName("Cari Hesap Tanımlama")]
public class CariHesapTanim : BaseClassWithAuditAndDescription
{
    public CariHesapTanim(Session session)
        : base(session)
    {
    }
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            CariHesapKodu = Helper.ConstNewRecordText;
            this.EvAdresi = new AdresTanim(Session);
            this.IsAdresi = new AdresTanim(Session);
            this.cariHesapTipi = CariHesapTipi.Musteri;
            this.kDVTipi = BusinessObjects.KDVTipi.KDVDahil;

            DovizTanim DovizEntity = Session.FindObject<DovizTanim>(
                new BinaryOperator(nameof(DovizTanim.IsVarsayilan), true));
            if (DovizEntity != null)
            {
                this.DovizTanim = DovizEntity;
            }
        }

    }

    KDVTipi? kDVTipi;
    SatisFiyatListeM satisFiyatListeM;
    string telefon2;
    string telefon1;
    string webAdresi;
    string ePostaAdresi;
    string vergiDairesi;
    string tCKimlikVergiNo;
    DovizTanim dovizTanim;
    CariFirmaHesapTuru cariHesapTuru;
    CariGrupTanim cariGrupTanim;
    AdresTanim ısAdresi;
    AdresTanim evAdresi;
    string cariHesapAdi;
    string cariHesapKodu;
    CariHesapTipi cariHesapTipi;
    FisTuruTanim fisTuruTanim;

    [XafDisplayName("Cari Hesap Tipi")]
    public CariHesapTipi CariHesapTipi
    {
        get => cariHesapTipi;
        set => SetPropertyValue(nameof(CariHesapTipi), ref cariHesapTipi, value);
    }

    [Size(32)]
    // TargetCriteria: XAF'ın RuleUniqueValue doğrulaması (Committing) XPO'nun OnSaving'inde
    // (numaralandırmanın gerçekten atandığı an) ÖNCE çalışır — bu yüzden aynı anda birden fazla
    // yeni kayıt tek commit'te kaydedilirse hepsi hâlâ placeholder metnini taşır ve "benzersiz
    // değil" diye yanlış pozitif verir. Placeholder'ı doğrulama dışı bırakmak asıl (gerçek
    // numaralara uygulanan) benzersizlik kontrolünü zayıflatmaz.
    [RuleUniqueValue(TargetCriteria = "CariHesapKodu != '" + Helper.ConstNewRecordText + "'")]
    [Indexed(Unique = true)]
    [XafDisplayName("Cari Hesap Kodu")]
    [Persistent("CariHesapKodu"), RuleRequiredField(DefaultContexts.Save)]
    //[Appearance("ED_CariHesapCariHesapKodu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    public string CariHesapKodu
    {
        get => cariHesapKodu;
        set => SetPropertyValue(nameof(CariHesapKodu), ref cariHesapKodu, value);
    }

    [Size(200)]
    [XafDisplayName("Cari Adı")]
    [Indexed(Unique = true)]
    [Persistent("CariHesapAdi"), RuleRequiredField(DefaultContexts.Save)]
    public string CariHesapAdi
    {
        get => cariHesapAdi;
        set => SetPropertyValue(nameof(CariHesapAdi), ref cariHesapAdi, value);
    }

    [XafDisplayName("KDV H/D")]
    [Persistent("KDVTipi"), RuleRequiredField(DefaultContexts.Save)]
    public KDVTipi? KDVTipi
    {
        get => kDVTipi;
        set => SetPropertyValue(nameof(KDVTipi), ref kDVTipi, value);
    }


    [Aggregated]
    [ExpandObjectMembers(ExpandObjectMembers.Never)]
    [VisibleInListView(false)]
    [XafDisplayName("Ev Adresi")]
    public AdresTanim EvAdresi
    {
        get => evAdresi;
        set => SetPropertyValue(nameof(EvAdresi), ref evAdresi, value);
    }

    [Aggregated]
    [ExpandObjectMembers(ExpandObjectMembers.Never)]
    [VisibleInListView(false)]
    [XafDisplayName("İş Adresi")]
    public AdresTanim IsAdresi
    {
        get => ısAdresi;
        set => SetPropertyValue(nameof(IsAdresi), ref ısAdresi, value);
    }

    public CariFirmaHesapTuru CariHesapTuru
    {
        get => cariHesapTuru;
        set => SetPropertyValue(nameof(CariHesapTuru), ref cariHesapTuru, value);
    }

    [XafDisplayName("Cari Hesap Grubu")]
    public CariGrupTanim CariGrupTanim
    {
        get => cariGrupTanim;
        set => SetPropertyValue(nameof(CariGrupTanim), ref cariGrupTanim, value);
    }

    [XafDisplayName("Döviz Kuru")]
    [Persistent("DovizTanim"), RuleRequiredField(DefaultContexts.Save)]
    public DovizTanim DovizTanim
    {
        get => dovizTanim;
        set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
    }

    [XafDisplayName("Fiyat Listesi")]
    [Appearance("IsVisible_CariHesapTanim_StokFiyatListeM", Visibility = ViewItemVisibility.Hide, Criteria = "(!CariHesapTipi==0)", Context = "DetailView")]
    public SatisFiyatListeM SatisFiyatListeM
    {
        get => satisFiyatListeM;
        set
        {
            SetPropertyValue(nameof(SatisFiyatListeM), ref satisFiyatListeM, value);
            if (!IsLoading)
            {
                if (SatisFiyatListeM != null)
                {
                    dovizTanim = null;
                    var fiyatlistedoviz = SatisFiyatListeM.DovizTanim;
                    if (fiyatlistedoviz != null)
                        DovizTanim = fiyatlistedoviz;
                }

            }
        }
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
    [RuleRegularExpression(DefaultContexts.Save, Helper.EmailTypeRegEx,
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

    [XafDisplayName("Fiş Türü")]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [VisibleInLookupListView(false)]
    [VisibleInReports(false)]
    [RuleRequiredField("RuleRequired_CariHesapTanim_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
    public FisTuruTanim FisTuruTanim
    {
        get => fisTuruTanim;
        set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
    }
    protected override void OnSaving()
    {
        if (Session.IsNewObject(This) &&
            CariHesapKodu == Helper.ConstNewRecordText &&
            FisTuruTanim != null)
        {
            INumberSequenceService numberSequenceService = new NumberSequenceService();
            CariHesapKodu = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, DateTime.UtcNow);
        }
        base.OnSaving();
    }

    protected override void OnDeleting()
    {
        try
        {
            var Siparislist = new XPQuery<SatisSiparisM>(Session).Where(x => x.CariHesapTanim == this);
            var CariHareketlist = new XPQuery<CariHesapHareketleri>(Session).Where(x => x.CariHesapTanim == this);
            var SatinAlmaSiparisleri = new XPQuery<SatinAlmaSiparisiM>(Session).Where(x => x.Tedarikci == this);
            var SatinAlmaTeklifleri = new XPQuery<SatinAlmaTeklifM>(Session).Where(x => x.Tedarikci == this);
            var Faturalar = new XPQuery<FaturaM>(Session).Where(x => x.CariHesap == this);

            if (Siparislist.Count() > 0)
            {
                throw new UserFriendlyException("Bu Cari Siparişlerde Kullanılmış Silemezsiniz !!!");
            }
            else if (SatinAlmaSiparisleri.Count() > 0)
            {
                throw new UserFriendlyException("Bu Cari, Satın Alma Siparişlerinde Kullanılmış — Silemezsiniz !!!");
            }
            else if (SatinAlmaTeklifleri.Count() > 0)
            {
                throw new UserFriendlyException("Bu Cari, Satın Alma Tekliflerinde Kullanılmış — Silemezsiniz !!!");
            }
            else if (Faturalar.Count() > 0)
            {
                throw new UserFriendlyException("Bu Cari, Faturalarda Kullanılmış — Silemezsiniz !!!");
            }
            else if (CariHareketlist.Count() > 0)
            {
                throw new UserFriendlyException("Bu Cari,Hesap Hareketlerinde Kullanılmış Silemezsiniz !!!");
            }
            else
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
}