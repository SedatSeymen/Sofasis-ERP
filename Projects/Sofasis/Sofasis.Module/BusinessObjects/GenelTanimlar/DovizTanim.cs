using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace Sofasis.Module.BusinessObjects
{

    [DefaultProperty("DovizKodu")]
    [DefaultClassOptions]
    [XafDisplayName("Döviz Tanımlama")]
    [NavigationItem("Genel Tanımlar")]
    [Appearance("ED_DovizTanim", Enabled = false, TargetItems = "*", Criteria = "IsSystemRecord = true", Context = "DetailView")]

    [Appearance("DovizTanimColor", AppearanceItemType = "ViewItem",
    TargetItems = "*", Criteria = "IsVarsayilan = 1", Context = "ListView",
        FontColor = "Orange", FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]
    public class DovizTanim : BaseClassWithAudit, IFisTuruVarsayilanHedefi
    { 
        public DovizTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();

        }


        bool isVarsayilan;
        string dovizKodu;
        string dovizAdi;
        string sembol;


        [Size(5)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Döviz Kodu"), ToolTip("Döviz Kodu Giriniz")]
        [RuleRequiredField("RuleRequired_DovizTanim_DovizKodu", DefaultContexts.Save, "Lütfen Döviz Kodunu Giriniz...")]
        [Appearance("ED_DovizTanim_DovizKodu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]

        public string DovizKodu
        {
            get => dovizKodu;
            set => SetPropertyValue(nameof(DovizKodu), ref dovizKodu, value);
        }

        [Size(50)]
        [XafDisplayName("Döviz Adı"), ToolTip("Döviz Adını Giriniz")]
        [RuleRequiredField("RuleRequired_DovizTanim_DovizAdi", DefaultContexts.Save,"Lütfen Döviz Adını Giriniz...")]
        public string DovizAdi
        {
            get => dovizAdi;
            set => SetPropertyValue(nameof(DovizAdi), ref dovizAdi, value);
        }

        // Tutar alanlarının ondalık maskında (bkz. Module.cs OndalikMaskiUygula) dinamik olarak
        // kullanılır — her para birimi kendi sembolüyle gösterilir, sabit ₺ değil.
        [Size(5)]
        [XafDisplayName("Sembol"), ToolTip("Para Birimi Sembolü (₺, $, € vb.)")]
        public string Sembol
        {
            get => sembol;
            set => SetPropertyValue(nameof(Sembol), ref sembol, value);
        }

        [XafDisplayName("Varsayılan mı ?")]
        public bool IsVarsayilan
        {
            get => isVarsayilan;
            set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
        }

        protected override void OnSaving()
        {
            if (IsVarsayilan)
            {
                DovizTanim entity = Session.FindObject<DovizTanim>(
                    CriteriaOperator.FromLambda<DovizTanim>(x => x.IsVarsayilan && x.Oid != this.Oid));
                if (entity != null)
                {
                    entity.IsVarsayilan = false;
                    entity.Save();
                }
            }
            base.OnSaving();
        }
    }
}