using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace Sofasis.Module.BusinessObjects;

[DefaultProperty("CariGrupAdi")]
[DefaultClassOptions]
[XafDisplayName("Cari Grup Tanımlama")]

[Appearance("ED_CariGrupTanim",
        Enabled = false, TargetItems = "*",
        Criteria = "IsSystemRecord = true", Context = "DetailView")]

[Appearance("CariGrupTanimColor", AppearanceItemType = "ViewItem",
    TargetItems = "*", Criteria = "IsVarsayilan = 1", Context = "ListView",
        FontColor = "Orange", FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]


public class CariGrupTanim : BaseClassWithAuditAndDescription
{ 
    public CariGrupTanim(Session session)
        : base(session)
    {
    }
    public override void AfterConstruction()
    {
        base.AfterConstruction();
    }


    bool isVarsayilan;
    string cariGrupAdi;


    [Size(50)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    [XafDisplayName("Cari Grup Adı"), ToolTip("Grup Adını Giriniz")]
    [RuleRequiredField("RuleRequired_CariGrupTanim_CariGrupAdi", DefaultContexts.Save, "Lütfen Cari Grup Adını Giriniz...")]
    public string CariGrupAdi
    {
        get => cariGrupAdi;
        set => SetPropertyValue(nameof(CariGrupAdi), ref cariGrupAdi, value);
    }

    [XafDisplayName("Varsayılan mı ?")]
    public bool IsVarsayilan
    {
        get => isVarsayilan;
        set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
    }


    protected override void OnSaving()
    {
        base.OnSaving();
    }
}