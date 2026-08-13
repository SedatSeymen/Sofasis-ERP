using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Sofasis.Module.BusinessObjects;

[DefaultClassOptions]
[XafDisplayName("Satış Sipariş Raporu")]
public class WW_SatisSiparisRapor : XPLiteObject
{
    public WW_SatisSiparisRapor(Session session)
        : base(session)
    {
    }
    public override void AfterConstruction()
    {
        base.AfterConstruction();
    }


    byte[] siparisPhoto;
    string aciklama;
    decimal yerelTutar;
    decimal netTutar;
    decimal kDVTutar;
    decimal kDVHaricTutar;
    KDVTanim kDVTanim;
    decimal ındirimTutar;
    decimal burutTutar;
    decimal birimFiyat;
    BirimTanim birimTanim;
    decimal miktar;
    StokTanim kumasStokAdi;
    StokTanim stokTanim;
    StokGrupTanim stokGrupTanim;
    string siparisText;
    AdresTanim musteriAdresi;
    MateryalTanimD materyalTanimD;
    MateryalTanimM materyalTanimM;
    KDVTipi kDVTipi;
    decimal dovizKuru;
    DovizTanim dovizTanim;
    CariHesapTanim cariHesapTanim;
    string musteriSiparisNo;
    string musteriIsmi;
    DateTime teslimTarihi;
    DateTime terminTarihi;
    DateTime siparisTarihi;
    string siparisKodu;
    SatisSiparisDurumu siparisDurumu;
    SiparisTipi siparisTipi;
    string masterKeyID;
    string detayKeyID;

    [Key, Persistent]
    [Size(13)]
    public string DetayKeyID
    {
        get => detayKeyID;
        set => SetPropertyValue(nameof(DetayKeyID), ref detayKeyID, value);
    }


    [Size(13)]
    public string MasterKeyID
    {
        get => masterKeyID;
        set => SetPropertyValue(nameof(MasterKeyID), ref masterKeyID, value);
    }


    public SiparisTipi SiparisTipi
    {
        get => siparisTipi;
        set => SetPropertyValue(nameof(SiparisTipi), ref siparisTipi, value);
    }


    public SatisSiparisDurumu SiparisDurumu
    {
        get => siparisDurumu;
        set => SetPropertyValue(nameof(SiparisDurumu), ref siparisDurumu, value);
    }


    [Size(32)]
    public string SiparisKodu
    {
        get => siparisKodu;
        set => SetPropertyValue(nameof(SiparisKodu), ref siparisKodu, value);
    }


    public DateTime SiparisTarihi
    {
        get => siparisTarihi;
        set => SetPropertyValue(nameof(SiparisTarihi), ref siparisTarihi, value);
    }


    public DateTime TerminTarihi
    {
        get => terminTarihi;
        set => SetPropertyValue(nameof(TerminTarihi), ref terminTarihi, value);
    }


    public DateTime TeslimTarihi
    {
        get => teslimTarihi;
        set => SetPropertyValue(nameof(TeslimTarihi), ref teslimTarihi, value);
    }


    [Size(50)]
    public string MusteriIsmi
    {
        get => musteriIsmi;
        set => SetPropertyValue(nameof(MusteriIsmi), ref musteriIsmi, value);
    }


    [Size(50)]
    public string MusteriSiparisNo
    {
        get => musteriSiparisNo;
        set => SetPropertyValue(nameof(MusteriSiparisNo), ref musteriSiparisNo, value);
    }


    public CariHesapTanim CariHesapTanim
    {
        get => cariHesapTanim;
        set => SetPropertyValue(nameof(CariHesapTanim), ref cariHesapTanim, value);
    }


    public DovizTanim DovizTanim
    {
        get => dovizTanim;
        set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
    }


    public decimal DovizKuru
    {
        get => dovizKuru;
        set => SetPropertyValue(nameof(DovizKuru), ref dovizKuru, value);
    }


    public KDVTipi KDVTipi
    {
        get => kDVTipi;
        set => SetPropertyValue(nameof(KDVTipi), ref kDVTipi, value);
    }


    public MateryalTanimM MateryalTanimM
    {
        get => materyalTanimM;
        set => SetPropertyValue(nameof(MateryalTanimM), ref materyalTanimM, value);
    }


    public MateryalTanimD MateryalTanimD
    {
        get => materyalTanimD;
        set => SetPropertyValue(nameof(MateryalTanimD), ref materyalTanimD, value);
    }


    public AdresTanim MusteriAdresi
    {
        get => musteriAdresi;
        set => SetPropertyValue(nameof(MusteriAdresi), ref musteriAdresi, value);
    }


    [Size(1000)]
    public string SiparisText
    {
        get => siparisText;
        set => SetPropertyValue(nameof(SiparisText), ref siparisText, value);
    }

    public StokGrupTanim StokGrupTanim
    {
        get => stokGrupTanim;
        set => SetPropertyValue(nameof(StokGrupTanim), ref stokGrupTanim, value);
    }


    public StokTanim StokTanim
    {
        get => stokTanim;
        set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
    }

    public StokTanim KumasStokAdi
    {
        get => kumasStokAdi;
        set => SetPropertyValue(nameof(KumasStokAdi), ref kumasStokAdi, value);
    }


    public decimal Miktar
    {
        get => miktar;
        set => SetPropertyValue(nameof(Miktar), ref miktar, value);
    }

    public BirimTanim BirimTanim
    {
        get => birimTanim;
        set => SetPropertyValue(nameof(BirimTanim), ref birimTanim, value);
    }

    public decimal BirimFiyat
    {
        get => birimFiyat;
        set => SetPropertyValue(nameof(BirimFiyat), ref birimFiyat, value);
    }

    public decimal BurutTutar
    {
        get => burutTutar;
        set => SetPropertyValue(nameof(BurutTutar), ref burutTutar, value);
    }


    public decimal IndirimTutar
    {
        get => ındirimTutar;
        set => SetPropertyValue(nameof(IndirimTutar), ref ındirimTutar, value);
    }


    public KDVTanim KDVTanim
    {
        get => kDVTanim;
        set => SetPropertyValue(nameof(KDVTanim), ref kDVTanim, value);
    }


    public decimal KDVHaricTutar
    {
        get => kDVHaricTutar;
        set => SetPropertyValue(nameof(KDVHaricTutar), ref kDVHaricTutar, value);
    }

    public decimal KDVTutar
    {
        get => kDVTutar;
        set => SetPropertyValue(nameof(KDVTutar), ref kDVTutar, value);
    }

    public decimal NetTutar
    {
        get => netTutar;
        set => SetPropertyValue(nameof(NetTutar), ref netTutar, value);
    }

    public decimal YerelTutar
    {
        get => yerelTutar;
        set => SetPropertyValue(nameof(YerelTutar), ref yerelTutar, value);
    }
    
    [Size(200)]
    public string Aciklama
    {
        get => aciklama;
        set => SetPropertyValue(nameof(Aciklama), ref aciklama, value);
    }

    [VisibleInListView(false)]
    [ImageEditor(DetailViewImageEditorMode = ImageEditorMode.PictureEdit,
    ListViewImageEditorCustomHeight = 70, DetailViewImageEditorFixedWidth = 300)]
    public byte[] SiparisPhoto
    {
        get { return siparisPhoto; }
        set { SetPropertyValue(nameof(SiparisPhoto), ref siparisPhoto, value); }
    }

}