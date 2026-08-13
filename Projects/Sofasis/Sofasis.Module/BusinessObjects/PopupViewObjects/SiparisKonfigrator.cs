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
using Sofasis.Module.Extensions;

namespace Sofasis.Module.BusinessObjects;

[NonPersistent]
[XafDisplayName("Mamül Konfigratör")]
public class SiparisKonfigrator : XPLiteObject
{ 
    public SiparisKonfigrator(Session session)
        : base(session)
    {
    }
    public override void AfterConstruction()
    {
        base.AfterConstruction();
    }


    StokTanim stokTanim;
    StokModelKonfigrasyonM stokModelKonfigrasyonM;


    [XafDisplayName("Konfigrasyon Adı"), ToolTip("Konfigrasyon Adını Giriniz")]
    [RuleRequiredField("RuleRequired_SiparisKalemKonfigrator_StokModelKonfigrasyonM", DefaultContexts.Save, "Lütfen Konfigrasyon Adını Giriniz Giriniz...")]
    [DataSourceProperty("KonfigrasyonList")]
    public StokModelKonfigrasyonM StokModelKonfigrasyonM
    {
        get
        {
            return stokModelKonfigrasyonM;
        }
        set
        {
            SetPropertyValue(nameof(StokModelKonfigrasyonM), ref stokModelKonfigrasyonM, value);
        }
    }

    [XafDisplayName("Kumaş Adı"), ToolTip("Kumaş Adını Giriniz")]
    [RuleRequiredField("RuleRequired_SiparisKalemKonfigrator_StokTanim", DefaultContexts.Save, "Lütfen Kumaş Adını Giriniz Giriniz...")]
    [DataSourceProperty("KumasList")]

    public StokTanim StokTanim
    {
        get
        {
            return stokTanim;
        }
        set
        {
            SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
        }
    }



    [Browsable(false)]
    public List<StokModelKonfigrasyonM> KonfigrasyonList
    {
        get
        {
            return new XPQuery<StokModelKonfigrasyonM>(Session).ToList();
        }
    }

    [Browsable(false)]
    public List<StokTanim> KumasList
    {
        get
        {
            var StokParametre = Session.GetSingleton<StokParametre>();
            return new XPQuery<StokTanim>(Session).Where(x=> x.StokGrupTanim == StokParametre.KumasStokGrubu).ToList();
        }
    }




}