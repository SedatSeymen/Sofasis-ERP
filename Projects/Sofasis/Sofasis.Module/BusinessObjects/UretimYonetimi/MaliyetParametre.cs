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

    [XafDisplayName("Üretim Parametreleri")]
    public class MaliyetParametre : BaseClassWithAudit
    { 
        public MaliyetParametre(Session session)
            : base(session)
        {

        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }


        int? siparisTeslimSuresi;
        int? teshirTeslimSuresi;
        decimal? euroSatisKuru;
        decimal? euroAlisKuru;
        decimal? uSDSatisKuru;
        decimal? uSDAlisKuru;
        int? uretilecekTakimSayisi;

        [XafDisplayName("Üretilecek Takım Sayısı")]
        [RuleRequiredField("", DefaultContexts.Save, "Lütfen Üretilecek Takım Sayısını Giriniz...")]
        [ModelDefault("DisplayFormat", "{0:n0}")]
        [ModelDefault("EditMask", "n0")]
        public int? UretilecekTakimSayisi
        {
            get => uretilecekTakimSayisi;
            set => SetPropertyValue(nameof(UretilecekTakimSayisi), ref uretilecekTakimSayisi, value);
        }

        [XafDisplayName("USD Alış Kuru")]
        [RuleRequiredField("", DefaultContexts.Save, "Lütfen USD Alış Kurunu Giriniz...")]
        [ModelDefault("DisplayFormat", "{0:N2}")]
        [ModelDefault("EditMask", "N2")]
        public decimal? USDAlisKuru
        {
            get => uSDAlisKuru;
            set => SetPropertyValue(nameof(USDAlisKuru), ref uSDAlisKuru, value);
        }

        [XafDisplayName("USD Satis Kuru")]
        [RuleRequiredField("", DefaultContexts.Save, "Lütfen USD Satış Kurunu Giriniz...")]
        [ModelDefault("DisplayFormat", "{0:N2}")]
        [ModelDefault("EditMask", "N2")]
        public decimal? USDSatisKuru
        {
            get => uSDSatisKuru;
            set => SetPropertyValue(nameof(USDSatisKuru), ref uSDSatisKuru, value);
        }

        [XafDisplayName("EURO Alış Kuru")]
        [RuleRequiredField("", DefaultContexts.Save, "Lütfen EURO Alış Kurunu Giriniz...")]
        [ModelDefault("DisplayFormat", "{0:N2}")]
        [ModelDefault("EditMask", "N2")]
        public decimal? EuroAlisKuru
        {
            get => euroAlisKuru;
            set => SetPropertyValue(nameof(EuroAlisKuru), ref euroAlisKuru, value);
        }

        [XafDisplayName("EURO Satis Kuru")]
        [RuleRequiredField("", DefaultContexts.Save, "Lütfen EURO Satış Kurunu Giriniz...")]
        [ModelDefault("DisplayFormat", "{0:N2}")]
        [ModelDefault("EditMask", "N2")]
        public decimal? EuroSatisKuru
        {
            get => euroSatisKuru;
            set => SetPropertyValue(nameof(EuroSatisKuru), ref euroSatisKuru, value);
        }

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