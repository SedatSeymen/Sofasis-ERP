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
    [DefaultProperty("KDVOrani")]
    [DefaultClassOptions]
    [XafDisplayName("KDV Tanımlama")]
    [NavigationItem("Genel Tanımlar")]
    [Appearance("ED_KDVTanim", Enabled = false, TargetItems = "*", Criteria = "IsSystemRecord = true", Context = "DetailView")]
    
    [Appearance("KDVTanimColor", AppearanceItemType = "ViewItem",
    TargetItems = "*", Criteria = "IsVarsayilan = 1", Context = "ListView",
        FontColor = "Orange", FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]
    public class KDVTanim : BaseClassWithAudit, IFisTuruVarsayilanHedefi
    { 
        public KDVTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        bool isVarsayilan;
        decimal kDVOrani;


        [XafDisplayName("KDV Oranı"), ToolTip("KDV Oranını Giriniz")]
        [ModelDefault("EditMask", "N")]
        [ModelDefault("DisplayFormat", "{0:N0}")]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        public decimal KDVOrani
        {
            get => kDVOrani;
            set => SetPropertyValue(nameof(KDVOrani), ref kDVOrani, value);
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
                KDVTanim entity = Session.FindObject<KDVTanim>(
                    CriteriaOperator.FromLambda<KDVTanim>(x => x.IsVarsayilan && x.Oid != this.Oid));
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