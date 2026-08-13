using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("IlceAdi")]
    [XafDisplayName("İlçe Tanımlama")]
    public class IlceTanim : BaseClass
    {
        public IlceTanim(Session session)
            : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        string ilceAdi;
        private SehirTanim sehirTanim;

        [XafDisplayName("İlçe Adı"), ToolTip("İlçe Adını Giriniz")]
        [RuleRequiredField("RuleRequired_IlceTanim_IlceAdi", DefaultContexts.Save, "Lütfen İlçe Adını Giriniz...")]
        [Appearance("ED_IlceTanim_IlceAdi", Enabled = false, Criteria = "IsSystemRecord = true", Context = "DetailView")]
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string IlceAdi
        {
            get => ilceAdi;
            set => SetPropertyValue(nameof(IlceAdi), ref ilceAdi, value);
        }

        [Association("SehirTanim-IlceTanims")]
        [Appearance("InvisibleSehirAdi_IlceDetail", Visibility = ViewItemVisibility.Hide, Context = "DetailView")]
        [XafDisplayName("Şehir Adı")]
        public SehirTanim SehirTanim
        {
            get { return sehirTanim; }
            set { SetPropertyValue(nameof(SehirTanim), ref sehirTanim, value); }
        }
    }
}
