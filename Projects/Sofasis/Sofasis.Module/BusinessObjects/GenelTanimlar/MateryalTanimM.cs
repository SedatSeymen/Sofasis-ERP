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
namespace Sofasis.Module.BusinessObjects;

[DefaultProperty("MateryalAdi")]
[DefaultClassOptions]
[XafDisplayName("Materyal Tanımlama")]
[Appearance("ED_MateryalTanimM", Enabled = false, TargetItems = "*", Criteria = "IsSystemRecord = true", Context = "DetailView")]

public class MateryalTanimM : BaseClassWithAudit
{ 
    public MateryalTanimM(Session session)
        : base(session)
    {
    }
    public override void AfterConstruction()
    {
        base.AfterConstruction();
    }

    bool isVarsayilan;
    string materyalAdi;

    [XafDisplayName("Materyal Adı"), ToolTip("Materyal Adını Giriniz")]
    [RuleRequiredField("Reqired_MateryalTanimM_MateryalAdi", DefaultContexts.Save, "Lütfen Materyal Adını Giriniz...")]
    [Appearance("ED_MateryalTanimM_MateryalAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    public string MateryalAdi
    {
        get => materyalAdi;
        set => SetPropertyValue(nameof(MateryalAdi), ref materyalAdi, value);
    }


    [XafDisplayName("Materyal Tanımlama Detayları")]
    [Association("MateryalTanimM-MateryalTanimDs")]
    public XPCollection<MateryalTanimD> MateryalTanimDs
    {
        get
        {
            return GetCollection<MateryalTanimD>(nameof(MateryalTanimDs));
        }
    }

    [XafDisplayName("Varsayılan mı ?")]
    public bool IsVarsayilan
    {
        get => isVarsayilan;
        set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
    }

}