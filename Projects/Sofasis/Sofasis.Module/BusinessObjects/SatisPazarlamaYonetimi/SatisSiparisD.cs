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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Sofasis.Module.Extensions;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects;


[DefaultClassOptions]
[XafDisplayName("Satın Alma Sipariş Detayları")]
public class SatisSiparisD : BaseClass
{
    StokParametre StokParametre;
    public SatisSiparisD(Session session)
        : base(session)
    {

    }
    protected override void OnLoaded()
    {
        base.OnLoaded();
        LoadStokParametre();
    }

    private void LoadStokParametre()
    {
        if (StokParametre != null) return;
        StokParametre = Session.GetSingleton<StokParametre>();
        if (StokParametre != null)
        {
            kumasStokGrupTanim = StokParametre.KumasStokGrubu;
            kirlentStokGrupTanim = StokParametre.KirlentStokGrubu;
            koltukStokGrupTanim = StokParametre.KoltukStokGrubu;
        }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();

        if (Session.IsNewObject(this))
        {
            LoadStokParametre();
            this.StokGrupTanim = koltukStokGrupTanim;

        }


    }
    StokTanim kumasStokAdi;
    StokGrupTanim kumasStokGrupTanim;
    StokGrupTanim kirlentStokGrupTanim;
    StokGrupTanim koltukStokGrupTanim;
    StokGrupTanim stokGrupTanim;
    BirimTanim birimTanim;
    SatisSiparisM satisSiparisM;
    StokTanim stokTanim;
    string aciklama;
    decimal? yerelTutar;
    decimal? netTutar;
    decimal? kDVTutar;
    decimal? kDVHaricTutar;
    KDVTanim kDVTanim;
    decimal? indirimTutar;
    decimal? burutTutar;
    decimal? birimFiyat;
    decimal? miktar;


    [XafDisplayName("Stok Grubu"), ToolTip("Stok Grubunu Giriniz...")]
    [RuleRequiredField("RuleRequired_SatisSiparisD_StokGrupTanim", DefaultContexts.Save, "Lütfen Stok Grubunu Giriniz...")]
    [Appearance("ED_SatisSiparisD_StokGrupTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    public StokGrupTanim StokGrupTanim
    {
        get => stokGrupTanim;
        set
        {
            SetPropertyValue(nameof(StokGrupTanim), ref stokGrupTanim, value);
            if (!IsLoading)
            {
                this.StokTanim = null;
            }
        }
    }


    [XafDisplayName("Stok Adı"), ToolTip("S/H/M Adını Giriniz")]
    [RuleRequiredField("RuleRequired_SatisSiparisD_StokTanim", DefaultContexts.Save, "Lütfen Stok Adını Giriniz...")]
    [Appearance("ED_SatisSiparisD_StokTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [DataSourceCriteria("StokGrupTanim = '@This.StokGrupTanim'")]
    public StokTanim StokTanim
    {
        get => stokTanim;
        set
        {
            SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
            if (!IsLoading && Session.IsNewObject(this))
            {
                if (StokTanim != null)
                {
                    if (satisSiparisM == null) return;
                    var fiyatlisteDetay = SatisSiparisM.SatisFiyatListeM.SatisFiyatListeD;
                    var stokFiyat = fiyatlisteDetay.FirstOrDefault<SatisFiyatListeD>(x => x.StokTanim == StokTanim);
                    if(stokFiyat != null)
                    {
                        this.BirimFiyat = stokFiyat.BirimFiyat;
                    }
                }

            }
        }
    }

    [XafDisplayName("Kumaş Adı"), ToolTip("Lütfen Kumaş Adını Giriniz")]
    [DataSourceCriteria("StokGrupTanim = '@This.kumasStokGrupTanim'")]

    public StokTanim KumasStokAdi
    {
        get => kumasStokAdi;
        set => SetPropertyValue(nameof(KumasStokAdi), ref kumasStokAdi, value);
    }

    [XafDisplayName("Miktar")]
    [RuleRequiredField("RuleRequired_SatisSiparisD_Miktar", DefaultContexts.Save, "Lütfen Miktar Giriniz...")]
    public decimal? Miktar
    {
        get => miktar;
        set
        {
            if (Miktar != value)
            {
                decimal? OldValue = miktar;
                miktar = value;
                OnChanged(nameof(Miktar), OldValue, miktar);
            }
        }
    }

    [XafDisplayName("Birim")]
    [RuleRequiredField("RuleRequired_SatisSiparisD_BirimTanim", DefaultContexts.Save, "Lütfen Miktar Giriniz...")]
    public BirimTanim BirimTanim
    {
        get => birimTanim;
        set => SetPropertyValue(nameof(BirimTanim), ref birimTanim, value);
    }

    [XafDisplayName("Birim Fiyat")]
    [RuleRequiredField("RuleRequired_SatisSiparisD_BirimFiyat", DefaultContexts.Save, "Lütfen Miktar Giriniz...")]
    public decimal? BirimFiyat
    {
        get => birimFiyat;
        set
        {
            if (BirimFiyat != value)
            {
                decimal? OldValue = birimFiyat;
                birimFiyat = value;
                OnChanged(nameof(BirimFiyat), OldValue, birimFiyat);
            }
        }
    }

    [XafDisplayName("Bürüt Fiyat")]
    [Appearance("ED_SatisSiparisD_BurutFiyat", Enabled = false, Criteria = "", Context = "DetailView")]
    public decimal? BurutTutar
    {
        get => burutTutar;
        set
        {
            if (BurutTutar != value)
            {
                decimal? OldValue = burutTutar;
                burutTutar = value;
                OnChanged(nameof(BurutTutar), OldValue, burutTutar);
            }
        }
    }

    [XafDisplayName("İndirim Tutar")]
    public decimal? IndirimTutar
    {
        get => indirimTutar;
        set
        {
            if (IndirimTutar != value)
            {
                decimal? OldValue = indirimTutar;
                indirimTutar = value;
                OnChanged(nameof(IndirimTutar), OldValue, indirimTutar);
            }
        }
    }

    [XafDisplayName("KDV Oranı")]
    public KDVTanim KDVTanim
    {
        get => kDVTanim;
        set
        {
            if (kDVTanim != value)
            {
                KDVTanim OldValue = kDVTanim;
                kDVTanim = value;
                OnChanged(nameof(KDVTanim), OldValue, kDVTanim);
            }
        }
    }

    [XafDisplayName("KDV Hariç Tutar")]
    [Appearance("ED_SatisSiparisD_KDVHaricTutar", Enabled = false, Criteria = "", Context = "DetailView")]
    public decimal? KDVHaricTutar
    {
        get => kDVHaricTutar;
        set
        {
            if (KDVHaricTutar != value)
            {
                decimal? OldValue = kDVHaricTutar;
                kDVHaricTutar = value;
                OnChanged(nameof(KDVHaricTutar), OldValue, kDVHaricTutar);
            }
        }
    }

    [XafDisplayName("KDV Tutar")]
    [Appearance("ED_SatisSiparisD_KDVTutar", Enabled = false, Criteria = "", Context = "DetailView")]
    public decimal? KDVTutar
    {
        get => kDVTutar;
        set
        {
            if (KDVTutar != value)
            {
                decimal? OldValue = kDVTutar;
                kDVTutar = value;
                OnChanged(nameof(KDVTutar), OldValue, kDVTutar);
            }
        }
    }

    [XafDisplayName("Net Tutar")]
    [Appearance("ED_SatisSiparisD_NetTutar", Enabled = false, Criteria = "", Context = "DetailView")]
    public decimal? NetTutar
    {
        get => netTutar;
        set
        {
            if (NetTutar != value)
            {
                decimal? OldValue = netTutar;
                netTutar = value;
                OnChanged(nameof(NetTutar), OldValue, netTutar);
            }
        }
    }

    [XafDisplayName("Yerel Tutar")]
    [Appearance("ED_SatisSiparisD_YerelNetTutar", Enabled = false, Criteria = "", Context = "DetailView")]
    public decimal? YerelTutar
    {
        get => yerelTutar;
        set
        {
            if (YerelTutar != value)
            {
                decimal? OldValue = yerelTutar;
                yerelTutar = value;
                OnChanged(nameof(YerelTutar), OldValue, yerelTutar);
            }
        }
    }

    [Size(200)]
    [VisibleInListView(false)]
    [ModelDefault("RowCount", "2")]
    [XafDisplayName("Sipariş Açıklama")]
    public string Aciklama
    {
        get => aciklama;
        set => SetPropertyValue(nameof(Aciklama), ref aciklama, value);
    }


    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [Association("SatisSiparisM-SatisSiparisDs")]
    public SatisSiparisM SatisSiparisM
    {
        get => satisSiparisM;
        set => SetPropertyValue(nameof(SatisSiparisM), ref satisSiparisM, value);
    }

    public void CalculateBurutTutar()
    {
        decimal? Miktar = 0;
        Miktar = this.Miktar;
        if (SatisSiparisM == null) return;

        if (IndirimTutar == null || IndirimTutar == 0)
        {
            BurutTutar = (Miktar * BirimFiyat);
            IndirimTutar = 0;
        }
        else
        {
            BurutTutar = (Miktar * BirimFiyat) - IndirimTutar;
        }
        if (KDVTanim != null)
            CalculateKDVTutar(BurutTutar, KDVTanim.KDVOrani);

    }

    void CalculateKDVTutar(decimal? PBurutFiyat, decimal PKDVOrani)
    {
        decimal? _BurutFiyat = 0;
        decimal? _KDVOrani = 0;
        decimal? _KDVTutar = 0;
        if (this.SatisSiparisM != null && BurutTutar > 0)
        {
            if(SatisSiparisM.KDVTipi == KDVTipi.KDVHaric) 
            {
                _BurutFiyat = PBurutFiyat;
                _KDVOrani = PKDVOrani / 100;
                _KDVOrani = 1 + _KDVOrani;
                _KDVTutar = (PBurutFiyat * _KDVOrani) - BurutTutar;

                KDVTutar =  _KDVTutar;
                KDVHaricTutar = BurutTutar;
                NetTutar = KDVHaricTutar + KDVTutar;
                if (SatisSiparisM != null && (SatisSiparisM.DovizKuru > 0))
                    YerelTutar = NetTutar * SatisSiparisM.DovizKuru;
            }

            if (SatisSiparisM.KDVTipi == KDVTipi.KDVDahil)
            {
                _BurutFiyat = PBurutFiyat;
                _KDVOrani = PKDVOrani / 100;
                _KDVOrani = 1 + _KDVOrani;
                _KDVTutar = BurutTutar - (PBurutFiyat / _KDVOrani) ;

                KDVTutar = _KDVTutar;
                KDVHaricTutar = (PBurutFiyat / _KDVOrani);
                NetTutar = KDVHaricTutar + KDVTutar;
                if (SatisSiparisM != null && (SatisSiparisM.DovizKuru > 0))
                    YerelTutar = NetTutar * SatisSiparisM.DovizKuru;
            }

        }
    }

    protected override void OnSaving()
    {
        base.OnSaving();
        if(SatisSiparisM != null)
            SatisSiparisM.UpdateTotals(null);
    }

    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (propertyName== nameof(Miktar) && newValue != oldValue) 
        {
            CalculateBurutTutar();
        }
        if (propertyName == nameof(BirimFiyat))
        {
            CalculateBurutTutar();
        }
        if (propertyName == nameof(IndirimTutar))
        {
            CalculateBurutTutar();
        }
        if (propertyName == nameof(KDVTanim))
        {
            CalculateBurutTutar();
        }
        if (propertyName == nameof(StokTanim))
        {
            if(stokTanim !=null)
            {
                this.BirimTanim = StokTanim.BirimTanim;
                this.KDVTanim = StokTanim.KDVTanim;
            }
        }

    }

    protected override void OnDeleting()
    {
        base.OnDeleting();
        SatisSiparisD deletedObject = this;
        if (SatisSiparisM != null)
            SatisSiparisM.UpdateTotals(deletedObject);
    }
}