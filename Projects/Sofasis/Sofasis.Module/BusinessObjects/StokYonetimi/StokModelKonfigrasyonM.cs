using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
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
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("KonfigrasyonAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Stok Model Konfigrasyon Tanımlama")]
    public class StokModelKonfigrasyonM : BaseClassWithAuditAndDescription
    {
        public StokModelKonfigrasyonM(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }


        StokModelTanim stokModelTanim;
        string konfigrasyonAdi;


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Konfigrasyon Adı"), ToolTip("Stok Model Konfigrasyon Adını Giriniz...")]
        [RuleRequiredField("KonfigrasyonAdiRequired", DefaultContexts.Save, "Stok Model Konfigrasyon Adını Giriniz...")]
        [Appearance("EnableDisableKonfigrasyonAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]

        public string KonfigrasyonAdi
        {
            get => konfigrasyonAdi;
            set => SetPropertyValue(nameof(KonfigrasyonAdi), ref konfigrasyonAdi, value);
        }

        [XafDisplayName("Model Adı"), ToolTip("Stok Model Adını Giriniz...")]
        [RuleRequiredField("ModelAdiRequired", DefaultContexts.Save, "Stok Model Adını Giriniz...")]
        [Appearance("EnableDisableModelAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public StokModelTanim StokModelTanim
        {
            get => stokModelTanim;
            set=>  SetPropertyValue(nameof(StokModelTanim), ref stokModelTanim, value);

        }

    [Association("StokModelKonfigrasyonM-StokModelKonfigrasyonDs"), Aggregated]
        public XPCollection<StokModelKonfigrasyonD> StokModelKonfigrasyonDs
        {
            get
            {
                return GetCollection<StokModelKonfigrasyonD>(nameof(StokModelKonfigrasyonDs));
            }
        }
    }
}