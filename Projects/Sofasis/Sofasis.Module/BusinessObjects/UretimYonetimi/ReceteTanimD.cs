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
    [XafDisplayName("Üretim Reçete Kalemleri")]

    public class ReceteTanimD : BaseClassWithAuditAndDescription
    { 
        public ReceteTanimD(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(this))
            {
                ReceteMalzemeTipi = ReceteMalzemeTipi.Sabit;
                CriteriaOperator criteria = CriteriaOperator.Parse(nameof(BirimTanim.IsVarsayilan), true);
                BirimTanim = Session.FindObject<BirimTanim>(criteria);
                FireOrani = 0;
            }
        }


        ReceteTanimM receteTanimM;
        ReceteMalzemeTipi receteMalzemeTipi;
        decimal? fireOrani;
        BirimTanim birimTanim;
        decimal? miktar;
        StokTanim stokTanim;



        [XafDisplayName("Malzeme Tipi")]
        public ReceteMalzemeTipi ReceteMalzemeTipi
        {
            get => receteMalzemeTipi;
            set => SetPropertyValue(nameof(ReceteMalzemeTipi), ref receteMalzemeTipi, value);
        }

        [XafDisplayName("Stok Adı")]
        [DataSourceCriteria("(StokTipi != 0) and StokHizmetMasrafTipi = 0")]
        public StokTanim StokTanim
        {
            get => stokTanim;
            set
            {
                SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
                if(StokTanim != null)
                    BirimTanim = StokTanim.BirimTanim;
            }
        }

        [XafDisplayName("Miktar")]
        [RuleRequiredField("RuleRequired_ReceteTanimD_Miktar", DefaultContexts.Save, "Lütfen Miktar Giriniz...")]
        public decimal? Miktar
        {
            get => miktar;
            set => SetPropertyValue(nameof(Miktar), ref miktar, value);
        }

        [XafDisplayName("Birim")]
        [RuleRequiredField("RuleRequired_ReceteTanimD_BirimTanim", DefaultContexts.Save, "Lütfen Miktar Giriniz...")]
        public BirimTanim BirimTanim
        {
            get => birimTanim;
            set => SetPropertyValue(nameof(BirimTanim), ref birimTanim, value);
        }

        [XafDisplayName("Fire Oranı")]
        [RuleRequiredField("RuleRequired_ReceteTanimD_FireOrani", DefaultContexts.Save, "Lütfen Fire Oranını Giriniz...")]
        public decimal? FireOrani
        {
            get => fireOrani;
            set => SetPropertyValue(nameof(FireOrani), ref fireOrani, value);
        }

        [Association("ReceteTanimM-ReceteTanimDs")]
        public ReceteTanimM ReceteTanimM
        {
            get => receteTanimM;
            set => SetPropertyValue(nameof(ReceteTanimM), ref receteTanimM, value);
        }

    }
}