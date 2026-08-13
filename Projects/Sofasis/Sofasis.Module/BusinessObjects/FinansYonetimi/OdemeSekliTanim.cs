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
    [DefaultProperty("OdemeSekliAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Ödeme Şekli Tanımlama")]

    [Appearance("OdemeSekliTanimColor", AppearanceItemType = "ViewItem",
    TargetItems = "*", Criteria = "IsVarsayilan = 1", Context = "ListView",
        FontColor = "Orange", FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]
    public class OdemeSekliTanim : BaseClassWithAudit
    {
        public OdemeSekliTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        string odemeSekliAdi;
        bool isVarsayilan;

        [Size(50)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Ödeme Şekli"), ToolTip("Ödeme Şeklini Giriniz")]
        [RuleRequiredField("RuleReguired_OdemeSekliTanim_OdemeSekliAdi", DefaultContexts.Save, "Lütfen Ödeme Şeklini Giriniz...")]
        public string OdemeSekliAdi
        {
            get => odemeSekliAdi;
            set => SetPropertyValue(nameof(OdemeSekliAdi), ref odemeSekliAdi, value);
        }

        [XafDisplayName("Varsayılan mı ?")]
        public bool IsVarsayilan
        {
            get => isVarsayilan;
            set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
        }

    }
}