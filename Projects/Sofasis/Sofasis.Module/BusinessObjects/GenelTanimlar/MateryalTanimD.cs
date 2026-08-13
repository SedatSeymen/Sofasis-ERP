using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("BoyaRengiAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Metaryal Boya Rengi Tanımlama")]
    public class MateryalTanimD : BaseClassWithDescription
    {
        public MateryalTanimD(Session session)
            : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        string boyaRengiAdi;
        private MateryalTanimM materyalTanimM;
        [XafDisplayName("Boya Rengi Adı"), ToolTip("Boya Rengi Adını Giriniz")]
        [RuleRequiredField("RuleRequired_MateryalTanimD_BoyaRengiAdi", DefaultContexts.Save, "Lütfen Boya Rengi Adını Giriniz...")]
        [Appearance("ED_MateryalTanimD_BoyaRengiAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]

        public string BoyaRengiAdi
        {
            get => boyaRengiAdi;
            set => SetPropertyValue(nameof(BoyaRengiAdi), ref boyaRengiAdi, value);
        }

        [Association("MateryalTanimM-MateryalTanimDs")]
        [Appearance("InvisibleUlkeAdi", Visibility = ViewItemVisibility.Hide, Context = "DetailView")]
        [XafDisplayName("Materyal Adı")]
        public MateryalTanimM MateryalTanimM
        {
            get { return materyalTanimM; }
            set { SetPropertyValue(nameof(MateryalTanimM), ref materyalTanimM, value); }
        }


    }
}