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
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("BankaAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Banka Tanımlama")]
    public class BankaTanim : BaseClassWithAudit
    {
        public BankaTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        string bankaAdi;
        string swiftKodu;

        [Size(50)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Banka Adı"), ToolTip("Banka Adını Giriniz")]
        [RuleRequiredField("RuleReguired_BankaTanim_BankaAdi", DefaultContexts.Save, "Lütfen Banka Adını Giriniz...")]
        public string BankaAdi
        {
            get => bankaAdi;
            set => SetPropertyValue(nameof(BankaAdi), ref bankaAdi, value);
        }

        [Size(11)]
        [XafDisplayName("Swift Kodu"), ToolTip("Bankanın SWIFT/BIC kodunu giriniz (örn. TCZBTR2A)")]
        [RuleRegularExpression(DefaultContexts.Save, "^[A-Z]{6}[A-Z0-9]{2}([A-Z0-9]{3})?$",
            SkipNullOrEmptyValues = true, CustomMessageTemplate = "Lütfen Geçerli Bir SWIFT/BIC Kodu Giriniz...")]
        public string SwiftKodu
        {
            get => swiftKodu;
            set => SetPropertyValue(nameof(SwiftKodu), ref swiftKodu, value);
        }

    }
}