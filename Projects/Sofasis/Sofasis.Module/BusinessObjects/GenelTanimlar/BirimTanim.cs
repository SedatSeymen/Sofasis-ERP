using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("BirimAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Birim Tanımlama")]
    [NavigationItem("Genel Tanımlar")]
    [Appearance("ED_BirimTanim",
        Enabled = false, TargetItems = "*",
        Criteria = "IsSystemRecord = true", Context = "DetailView")]

    [Appearance("BirimTanimColor", AppearanceItemType = "ViewItem",
    TargetItems = "*", Criteria = "IsVarsayilan = 1", Context = "ListView",
        FontColor = "Orange",FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]
    public class BirimTanim : BaseClassWithAudit, IFisTuruVarsayilanHedefi
    { 
        public BirimTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        bool isVarsayilan;
        string birimAdi;


        [Size(50)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Birim Adı"), ToolTip("Birim Adını Giriniz")]
        [RuleRequiredField("RuleReguired_BirimTanim_BirimAdi",DefaultContexts.Save,"Lütfen Birim Adını Giriniz...")]
        public string BirimAdi
        {
            get => birimAdi;
            set => SetPropertyValue(nameof(BirimAdi), ref birimAdi, value);
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
}