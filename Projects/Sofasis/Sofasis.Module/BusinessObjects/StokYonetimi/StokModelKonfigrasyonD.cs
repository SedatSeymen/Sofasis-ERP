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

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    [XafDisplayName("Stok Model Konfigrasyon Detay Tanımlama")]
    public class StokModelKonfigrasyonD : BaseClass
    { 
        public StokModelKonfigrasyonD(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(this))
            {
                Miktar = 1;
            }

        }


        StokModelKonfigrasyonM stokModelKonfigrasyonM;
        decimal? miktar;
        StokTanim stokTanim;


        [XafDisplayName("Stok Adı"), ToolTip("Stok Adını Giriniz...")]
        [RuleRequiredField("StokAdiRequired", DefaultContexts.Save, "Stok Adını Giriniz...")]
        [Appearance("EnableDisableStokAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [DataSourceCriteria("StokTipi = 0 && StokModelTanim = '@This.StokModelKonfigrasyonM.StokModelTanim'")]
        public StokTanim StokTanim
        {
            get => stokTanim;
            set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
        }

        [XafDisplayName("Miktar"), ToolTip("Miktar Giriniz...")]
        [RuleRequiredField("MiktarRequired", DefaultContexts.Save, "Miktar Giriniz...")]
        [Appearance("EnableDisableMiktar", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "Any")]
        public decimal? Miktar
        {
            get => miktar;
            set => SetPropertyValue(nameof(Miktar), ref miktar, value);
        }

        
        [Association("StokModelKonfigrasyonM-StokModelKonfigrasyonDs")]
        public StokModelKonfigrasyonM StokModelKonfigrasyonM
        {
            get => stokModelKonfigrasyonM;
            set => SetPropertyValue(nameof(StokModelKonfigrasyonM), ref stokModelKonfigrasyonM, value);
        }
    }
}