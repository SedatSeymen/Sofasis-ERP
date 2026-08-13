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

    [XafDisplayName("Satış Sipariş Parametreleri")]
    public class SatisSiparisParametre : BaseClassWithAudit
    { 
        public SatisSiparisParametre(Session session)
            : base(session)
        {

        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }


        int? siparisTeslimSuresi;
        int? teshirTeslimSuresi;


        [XafDisplayName("Teşhir Teslim Süresi")]
        [RuleRequiredField("", DefaultContexts.Save, "Lütfen Teşhir Teslim Süresini Giriniz...")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "N0")]
        public int? TeshirTeslimSuresi
        {
            get => teshirTeslimSuresi;
            set => SetPropertyValue(nameof(TeshirTeslimSuresi), ref teshirTeslimSuresi, value);
        }

        [XafDisplayName("Sipariş Teslim Süresi")]
        [RuleRequiredField("", DefaultContexts.Save, "Lütfen Sipariş Teslim Süresini Giriniz...")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [ModelDefault("EditMask", "N0")]
        public int? SiparisTeslimSuresi
        {
            get => siparisTeslimSuresi;
            set => SetPropertyValue(nameof(SiparisTeslimSuresi), ref siparisTeslimSuresi, value);
        }

    }
}