using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using Sofasis.Module.Extensions;
using Sofasis.Module.Services;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects;

[DefaultClassOptions]
[XafDisplayName("Satış Pazarlama Siparişleri")]
[DefaultProperty("SiparisKodu")]

[Appearance("SatisSiparisMListView_Planlandi", AppearanceItemType = "ViewItem", TargetItems = "*",
        Criteria = "SiparisDurumu = 1", Context = "ListView",
        BackColor = "Aqua",  Priority = 1)]

[Appearance("SatisSiparisMListView_UretimeAlindi", AppearanceItemType = "ViewItem", TargetItems = "*",
        Criteria = "SiparisDurumu = 2", Context = "ListView",
        BackColor = "Blue", FontColor = "White",  Priority = 2)]

[Appearance("SatisSiparisMListView_Uretildi", AppearanceItemType = "ViewItem", TargetItems = "*",
        Criteria = "SiparisDurumu = 3", Context = "ListView",
        BackColor = "Green", FontColor = "White",  Priority = 3)]

[Appearance("SatisSiparisMListView_SevkBekliyor", AppearanceItemType = "ViewItem", TargetItems = "*",
        Criteria = "SiparisDurumu = 4", Context = "ListView",
        BackColor = "Orange", FontColor = "White",  Priority = 3)]

[Appearance("SatisSiparisMListView_SevkEdildi", AppearanceItemType = "ViewItem", TargetItems = "*",
        Criteria = "SiparisDurumu = 5", Context = "ListView",
        BackColor = "Red", FontColor = "White", Priority = 3)]


[ListViewFilter("Siparişi Girilenler", "SiparisDurumu = 0",isCurrentFilter:true,  Index = 0)]
[ListViewFilter("Üretime Alınanlar", "SiparisDurumu = 1", Index = 1)]
[ListViewFilter("Üretilenler", "SiparisDurumu = 2", Index = 2)]
[ListViewFilter("Sevk Bekleyenler", "SiparisDurumu = 3", Index = 3)]
[ListViewFilter("Sevk Edilenler", "SiparisDurumu = 4", Index = 4)]
[ListViewFilter("Tüm Kayıtlar", "", "",Index =5)]


public class SatisSiparisM : BaseClassWithAuditAndDescription
{
    SatisSiparisParametre Parametre = null;
    int Sure = 0;
    public SatisSiparisM(Session session)
        : base(session)
    {

    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.DataLayer != null && Session.IsNewObject(this))
        {
            Parametre = new XPCollection<SatisSiparisParametre>(Session).FirstOrDefault();

            if (string.IsNullOrEmpty(SiparisKodu))
            {

                SiparisKodu = Helper.ConstNewRecordText;
            }

            SiparisTipi = SiparisTipi.YurticiSiparisi;
            if(Parametre != null)
            {
                SiparisTarihi = DateTime.Now.Date;
                Sure = (int)Parametre.SiparisTeslimSuresi;
                TerminTarihi = SiparisTarihi.AddDays(Sure);
                TeslimTarihi = TerminTarihi.AddDays(3);

            }
            else
            {
                SiparisTarihi = DateTime.Now.Date;
                Sure = 15;
                TerminTarihi = SiparisTarihi.AddDays(Sure);
                TeslimTarihi = TerminTarihi.AddDays(3);
            }
            SiparisDurumu = SatisSiparisDurumu.SiparisGirildi;
            KDVTipi = BusinessObjects.KDVTipi.KDVHaric;
            SiparisAktif = true;

            CriteriaOperator fisTuruCriteria =
                CriteriaOperator.FromLambda<FisTuruTanim>(x => x.ViewName == "SatisSiparisM_DetailView");
            var fisTuru = Session.FindObject<FisTuruTanim>(fisTuruCriteria);
            if (fisTuru != null)
                FisTuruTanim = fisTuru;
        }
    }

    bool isTeshirSiparisi;
    MateryalTanimD materyalTanimD;
    MateryalTanimM materyalTanimM;
    SatisFiyatListeM stokFiyatListeM;
    bool siparisAktif;
    KDVTipi? kDVTipi;
    string siparisText;
    string musteriIsmi;
    string musteriSiparisNo;
    decimal? yerelTutar;
    decimal? kDVHaricTutar;
    decimal dovizKuru;
    decimal? netTutar;
    decimal? kDVTutar;
    decimal? indirimTutar;
    decimal? burutTutar;
    DovizTanim dovizTanim;
    CariHesapTanim cariHesapTanim;
    AdresTanim musteriAdresi;
    SatisSiparisDurumu siparisDurumu;
    SiparisTipi siparisTipi;
    DateTime teslimTarihi;
    DateTime terminTarihi;
    DateTime siparisTarihi;
    string siparisKodu;
    FisTuruTanim fisTuruTanim;

    [XafDisplayName("Fiş Türü")]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [VisibleInLookupListView(false)]
    [VisibleInReports(false)]
    [RuleRequiredField("RuleRequired_SatisSiparisM_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
    public FisTuruTanim FisTuruTanim
    {
        get => fisTuruTanim;
        set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
    }

    [XafDisplayName("Sipariş Tipi"), ToolTip("Sipariş Tipi")]
    public SiparisTipi SiparisTipi
    {
        get => siparisTipi;
        set => SetPropertyValue(nameof(SiparisTipi), ref siparisTipi, value);
    }

    [XafDisplayName("Sipariş Durumu"), ToolTip("Sipariş Durumu")]
    [Appearance("ED_SatisSiparisM_SiparisDurumu", Enabled = false, Criteria = "", Context = "DetailView")]

    public SatisSiparisDurumu SiparisDurumu
    {
        get => siparisDurumu;
        set => SetPropertyValue(nameof(SiparisDurumu), ref siparisDurumu, value);
    }


    [Size(32)]
    // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
    // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış pozitif
    // "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
    [RuleUniqueValue(TargetCriteria = "SiparisKodu != '" + Helper.ConstNewRecordText + "'")]
    [Indexed(Unique = true)]
    [XafDisplayName("Sipariş Kodu"), ToolTip("Sipariş Kodunu Giriniz")]
    [Appearance("ED_SatisSiparisM_SiparisKodu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_SatisSiparisM_SiparisKodu", DefaultContexts.Save, "Lütfen Sipariş Kodunu Giriniz...")]
    public string SiparisKodu
    {
        get => siparisKodu;
        set => SetPropertyValue(nameof(SiparisKodu), ref siparisKodu, value);
    }

    [XafDisplayName("Sipariş Tarihi"), ToolTip("Sipariş Tarihi Giriniz")]
    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]
    [RuleValueComparison("RuleValueComparison_SatisSiparisM_SiparisTarihi", DefaultContexts.Save, ValueComparisonType.LessThanOrEqual, nameof(TerminTarihi), "Siparis Tarihi Termin Tarihinden Büyük Olamaz", ParametersMode.Expression)]
    [Appearance("ED_SatisSiparisM_SiparisTarihi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_SatisSiparisM_SiparisTarihi", DefaultContexts.Save, "Lütfen Sipariş Tarihini Giriniz...")]
    public DateTime SiparisTarihi
    {
        get => siparisTarihi;
        set => SetPropertyValue(nameof(SiparisTarihi), ref siparisTarihi, value);
    }

    [XafDisplayName("Termin Tarihi"), ToolTip("Termin Tarihi Giriniz")]
    [RuleValueComparison("RuleValueComparison_SatisSiparisM_TerminTarihi", DefaultContexts.Save, ValueComparisonType.GreaterThanOrEqual, nameof(SiparisTarihi), "Termin Tarihi Sipariş Tarihinden Küçük Olamaz", ParametersMode.Expression)]
    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]
    public DateTime TerminTarihi
    {
        get => terminTarihi;
        set => SetPropertyValue(nameof(TerminTarihi), ref terminTarihi, value);
    }

    [XafDisplayName("Teslim Tarihi"), ToolTip("Teslim Tarihi Giriniz")]
    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]
    public DateTime TeslimTarihi
    {
        get => teslimTarihi;
        set => SetPropertyValue(nameof(TeslimTarihi), ref teslimTarihi, value);
    }

    [Size(50)]
    [XafDisplayName("Müşteri İsmi"), ToolTip("Müşteri İsmi Giriniz")]
    public string MusteriIsmi
    {
        get => musteriIsmi;
        set => SetPropertyValue(nameof(MusteriIsmi), ref musteriIsmi, value);
    }

    [Size(32)]
    [XafDisplayName("Müşteri Sip.No"), ToolTip("Müşteri Sipariş Numarasını Giriniz")]
    public string MusteriSiparisNo
    {
        get => musteriSiparisNo;
        set => SetPropertyValue(nameof(MusteriSiparisNo), ref musteriSiparisNo, value);
    }

    [XafDisplayName("Cari Hesap"), ToolTip("Cari Hesap Adını Giriniz")]
    [RuleRequiredField("RuleRequired_SatisSiparisM_CariHesapTanim", DefaultContexts.Save, "Lütfen Cari Hesap Adını Seçiniz...")]
    public CariHesapTanim CariHesapTanim
    {
        get => cariHesapTanim;
        set
        {
            if (CariHesapTanim != value)
            {
                CariHesapTanim OldValue = cariHesapTanim;
                cariHesapTanim = value;
                OnChanged(nameof(CariHesapTanim), OldValue, cariHesapTanim);
            }
        }
    }

    [XafDisplayName("Döviz Kodu"), ToolTip("Döviz Kodu Giriniz")]
    [RuleRequiredField("RuleRequired_SatisSiparisM_DovizTanim", DefaultContexts.Save, "Lütfen Döviz Kodunu Seçiniz...")]
    public DovizTanim DovizTanim
    {
        get => dovizTanim;
        set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
    }

    [XafDisplayName("Döviz Kuru"), ToolTip("Döviz Kurunu Giriniz")]
    [Appearance("RuleRequired_SatisSiparisM_DovizKuru", Enabled = false, Criteria = "", Context = "DetailView")]
    public decimal DovizKuru
    {
        get => dovizKuru;
        set => SetPropertyValue(nameof(DovizKuru), ref dovizKuru, value);
    }

    [XafDisplayName("KDV H/D")]
    [Appearance("RuleRequired_SatisSiparisM_KDVTipi", Enabled = false, Criteria = "!IsNewObject(this)", Context = "DetailView")]
    [Persistent("KDVTipi"), RuleRequiredField(DefaultContexts.Save)]
    public KDVTipi? KDVTipi
    {
        get => kDVTipi;
        set
        {
            SetPropertyValue(nameof(KDVTipi), ref kDVTipi, value);
            UpdateDetailKDV(KDVTipi);
        }
    }

    [XafDisplayName("Ayak Tipi"), ToolTip("Ayak Tipini Giriniz...")]
    [RuleRequiredField("RuleRequired_SatisSiparisM_MateryalTanimM", DefaultContexts.Save, "Lütfen Ayak Tipini Seçiniz...")]
    public MateryalTanimM MateryalTanimM
    {
        get => materyalTanimM;
        set
        {
            SetPropertyValue(nameof(MateryalTanimM), ref materyalTanimM, value);
            if (!IsLoading)
            {
                MateryalTanimD = null;
            }
        }
    }

    [XafDisplayName("Ayak Boya Rengi"), ToolTip("Ayak Boya Rengini Giriniz...")]
    [RuleRequiredField("RuleRequired_SatisSiparisM_MateryalTanimD", DefaultContexts.Save, "Lütfen Ayak Boya Rengini Seçiniz...")]
    [DataSourceProperty("MateryalTanimM.MateryalTanimDs", DataSourcePropertyIsNullMode.SelectAll)]
    public MateryalTanimD MateryalTanimD
    {
        get => materyalTanimD;
        set => SetPropertyValue(nameof(MateryalTanimD), ref materyalTanimD, value);
    }


    [PersistentAlias("CariHesapTanim.EvAdresi")]
    [XafDisplayName("Cari Ev Adresi"), ToolTip("Cari Ev Adresini Giriniz")]
    public AdresTanim EvAdresi
    {
        get { return (AdresTanim)EvaluateAlias(nameof(EvAdresi)); }
    }

    [PersistentAlias("CariHesapTanim.IsAdresi")]
    [XafDisplayName("Cari İş Adresi"), ToolTip("Cari İş Adresini Giriniz")]

    public AdresTanim IsAdresi
    {
        get { return (AdresTanim)EvaluateAlias(nameof(IsAdresi)); }
    }

    [Aggregated]
    [ExpandObjectMembers(ExpandObjectMembers.Never)]
    [VisibleInListView(false)]
    [XafDisplayName("Müşteri Adresi")]
    public AdresTanim MusteriAdresi
    {
        get => musteriAdresi;
        set => SetPropertyValue(nameof(MusteriAdresi), ref musteriAdresi, value);
    }


    [RuleRequiredField("RuleRequired_SatisSiparisM_SatisFiyatListeM", DefaultContexts.Save, "Lütfen Fiyat Listesini Seçiniz...")]
    [XafDisplayName("Fiyat Listesi")]
    public SatisFiyatListeM SatisFiyatListeM
    {
        get => stokFiyatListeM;
        set => SetPropertyValue(nameof(SatisFiyatListeM), ref stokFiyatListeM, value);
    }

    [XafDisplayName("Brüt Tutar")]
    [Appearance("ED_SatisSiparisM_BurutTutar", Enabled = false, Criteria = "", Context = "DetailView")]

    public decimal? BurutTutar
    {
        get => burutTutar;
        set => SetPropertyValue(nameof(BurutTutar), ref burutTutar, value);
    }

    [XafDisplayName("Indirim Tutar")]
    [Appearance("ED_SatisSiparisM_IndirimTutar", Enabled = false, Criteria = "", Context = "DetailView")]
    public decimal? IndirimTutar
    {
        get => indirimTutar;
        set => SetPropertyValue(nameof(IndirimTutar), ref indirimTutar, value);

    }

    [XafDisplayName("KDV Hariç Tutar")]
    [Appearance("ED_SatisSiparisM_KDVHaricTutar", Enabled = false, Criteria = "", Context = "DetailView")]
    public decimal? KDVHaricTutar
    {
        get => kDVHaricTutar;
        set => SetPropertyValue(nameof(KDVHaricTutar), ref kDVHaricTutar, value);
    }

    [XafDisplayName("KDV Tutar")]
    [Appearance("ED_SatisSiparisM_KDVTutar", Enabled = false, Criteria = "", Context = "DetailView")]
    public decimal? KDVTutar
    {
        get => kDVTutar;
        set => SetPropertyValue(nameof(KDVTutar), ref kDVTutar, value);
    }

    [XafDisplayName("Net Tutar")]
    [Appearance("ED_SatisSiparisM_NetTutar", Enabled = false, Criteria = "", Context = "DetailView")]
    public decimal? NetTutar
    {
        get => netTutar;
        set => SetPropertyValue(nameof(NetTutar), ref netTutar, value);
    }

    [XafDisplayName("Yerel Net Tutar")]
    [Appearance("ED_SatisSiparisM_YerelNetTutar", Enabled = false, Criteria = "", Context = "DetailView")]
    public decimal? YerelTutar
    {
        get => yerelTutar;
        set => SetPropertyValue(nameof(YerelTutar), ref yerelTutar, value);
    }

    byte[] siparisPhoto;
    [ImageEditor(DetailViewImageEditorMode = ImageEditorMode.PictureEdit,
        ListViewImageEditorCustomHeight = 70, DetailViewImageEditorFixedWidth = 400)]
    [VisibleInListView(false)]
    public byte[] SiparisPhoto
    {
        get => siparisPhoto;
        set => SetPropertyValue(nameof(SiparisPhoto), ref siparisPhoto, value);
    }

    [XafDisplayName("Sipariş Aktif ?")]
    public bool SiparisAktif
    {
        get => siparisAktif;
        set => SetPropertyValue(nameof(SiparisAktif), ref siparisAktif, value);
    }


    [Size(1000)]
    [XafDisplayName("Sipariş Açıklama")]
    [ModelDefault("RowCount", "5")]
    [Appearance("ED_SatisSiparisM_SiparisAciklama", Enabled = false, Criteria = "", Context = "DetailView")]

    public string SiparisText
    {
        get => siparisText;
        set => SetPropertyValue(nameof(SiparisText), ref siparisText, value);
    }

    [XafDisplayName("Teşhir Siparişi mi?")]
    public bool IsTeshirSiparisi
    {
        get => isTeshirSiparisi;
        set
        {
            SetPropertyValue(nameof(IsTeshirSiparisi), ref isTeshirSiparisi, value);
            if (!IsLoading && Session.DataLayer != null && Session.IsNewObject(this))
            {
                if (this.isTeshirSiparisi == true)
                {
                    Sure = (int)Parametre.TeshirTeslimSuresi;
                    TerminTarihi = SiparisTarihi.AddDays(Sure);
                    TeslimTarihi = TerminTarihi.AddDays(3);
                }
                else
                {
                    Sure = (int)Parametre.SiparisTeslimSuresi;
                    TerminTarihi = SiparisTarihi.AddDays(Sure);
                    TeslimTarihi = TerminTarihi.AddDays(3);

                }
            }
        }
    }

    [Association("SatisSiparisM-SatisSiparisDs"),Aggregated]
    public XPCollection<SatisSiparisD> SatisSiparisDs
    {
        get
        {
            return GetCollection<SatisSiparisD>(nameof(SatisSiparisDs));
        }
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (propertyName == "CariHesapTanim" && newValue != null)
        {
            DovizTanim = ((CariHesapTanim)newValue).DovizTanim;
            KDVTipi = ((CariHesapTanim)newValue).KDVTipi;
            SatisFiyatListeM = ((CariHesapTanim)newValue).SatisFiyatListeM;
        }
        if (propertyName == "DovizTanim" && newValue != null)
        {
            CriteriaOperator criteria = new BinaryOperator("KurTarihi", SiparisTarihi);
            DovizGunlukKurM entity = Session.FindObject<DovizGunlukKurM>(criteria);
            if(entity!= null)
            {
                DovizGunlukKurD entitydetail = entity.DovizGunlukKurDetails.FirstOrDefault(x => x.DovizTanim == DovizTanim);
                if (entitydetail != null)
                {
                    DovizKuru = entitydetail.DovizSatis;
                }
            }
            else
            {
                if (CariHesapTanim.DovizTanim.DovizKodu == "TRY")
                    DovizKuru = 1;
            }

        }

    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(This) && SiparisKodu == Helper.ConstNewRecordText && FisTuruTanim != null)
        {
            INumberSequenceService numberSequenceService = new NumberSequenceService();
            SiparisKodu = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, SiparisTarihi);
        }
        base.OnSaving();

    }

    void UpdateDetailKDV(KDVTipi? kDVTipi)
    {
        if (KDVTipi == null) return;
        if (SatisSiparisDs != null && SatisSiparisDs.IsLoaded)
        {
            foreach (var item in SatisSiparisDs)
            {
                item.CalculateBurutTutar();
            }
        }

    }

    public void ResetTotals()
    {
        BurutTutar = 0;
        KDVHaricTutar= 0;
        KDVTutar= 0;
        NetTutar= 0;
        IndirimTutar= 0;
        YerelTutar = 0;
        SiparisText = "";
    }
    public void UpdateTotals(SatisSiparisD deletedObject)
    {
        try
        {
            if (SatisSiparisDs == null) return;
            ResetTotals();
            SatisSiparisDs.Remove(deletedObject);
            var parametre = Session.GetSingleton<StokParametre>();
            StringBuilder sb = new StringBuilder();
            BurutTutar = SatisSiparisDs.Sum(x => x.BurutTutar);
            IndirimTutar = SatisSiparisDs.Sum(x => x.IndirimTutar);
            KDVTutar = SatisSiparisDs.Sum(x => x.KDVTutar);
            NetTutar = SatisSiparisDs.Sum(x => x.NetTutar);
            KDVHaricTutar = SatisSiparisDs.Sum(x => x.KDVHaricTutar);
            YerelTutar = SatisSiparisDs.Sum(x => x.YerelTutar);
            foreach (SatisSiparisD item in SatisSiparisDs)
            {
                if (SiparisText == " ; ") SiparisText = "";
                string KumasAdi = "";

                if (item.KumasStokAdi == null)
                    KumasAdi = "Belirtilmemiş";
                else
                    KumasAdi = item.KumasStokAdi.StokAdi;

                if (item.StokGrupTanim == parametre.KoltukStokGrubu || item.StokGrupTanim == parametre.KirlentStokGrubu)
                {
                    sb.AppendLine("(" + item.StokTanim.StokAdi + " - Kumaş : " + KumasAdi + " - Miktar :" + string.Format("{0:0}", item.Miktar) + ")");
                }

            }
            SiparisText = sb.ToString();
            if (SiparisText.Length > 0 && SatisSiparisDs.Count > 0)
            {
                string strvalue = SiparisText.Substring(SiparisText.Length - 1, 1);
                if (strvalue == "\n")
                    SiparisText = SiparisText.Remove(SiparisText.Length - 1, 1);
            }
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