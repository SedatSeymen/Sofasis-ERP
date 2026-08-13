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

namespace Sofasis.Module.BusinessObjects
{
    [NonPersistent]
    public class CariFiyatGuncelleme : XPLiteObject
    { 
        public CariFiyatGuncelleme(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }


        SatisFiyatListeM yeniFiyatListesi;
        SatisFiyatListeM eskiFiyatListesi;

        [DataSourceProperty("EskiFiyatList")]
        public SatisFiyatListeM EskiFiyatListesi
        {
            get => eskiFiyatListesi;
            set =>SetPropertyValue(nameof(EskiFiyatListesi), ref eskiFiyatListesi, value);
        }

        [DataSourceProperty("YeniFiyatList")]
        [DataSourceCriteriaProperty(nameof(ListCriteria))]
        public SatisFiyatListeM YeniFiyatListesi
        {
            get => yeniFiyatListesi;
            set => SetPropertyValue(nameof(YeniFiyatListesi), ref yeniFiyatListesi, value);
        }


        [Browsable(false)]
        public List<SatisFiyatListeM> EskiFiyatList
        {
            get
            {
                return new XPQuery<SatisFiyatListeM>(Session).ToList();

            }
        }

        [VisibleInDetailView(false)]
        [Browsable(false)]
        public CriteriaOperator ListCriteria
        {
            get { return CriteriaOperator.FromLambda<SatisFiyatListeM>(
                x => x.DovizTanim == EskiFiyatListesi.DovizTanim &&
                x.Oid != EskiFiyatListesi.Oid); }
        }

        [Browsable(false)]
        public List<SatisFiyatListeM> YeniFiyatList
        {
            get
            {
                return new XPQuery<SatisFiyatListeM>(Session).ToList();

            }
        }

    }
}