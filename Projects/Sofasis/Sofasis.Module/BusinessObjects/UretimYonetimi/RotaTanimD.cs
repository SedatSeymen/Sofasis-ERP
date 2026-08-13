using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    [XafDisplayName("Üretim Rota Tanımlama")]
    public class RotaTanimD : BaseClass
    { 
        public RotaTanimD(Session session)
            : base(session)
        {
        }

        bool takipGerektirmiyor;
        RotaTanimM rotaTanimM;
        int? rotaSiraNo;
        OperasyonTanim operasyonTanim;


        [XafDisplayName("Rota Sıra No"), ToolTip("Rota Sıra No Giriniz")]
        [RuleRequiredField("RuleRequired_RotaTanimD_RotaSiraNo", DefaultContexts.Save, "Lütfen Rota Sıra No Giriniz...")]
        [Appearance("ED_RotaTanimD_RotaSiraNo", Enabled = false, Criteria = "", Context = "DetailView")]
        [ModelDefault("DisplayFormat", "{0:n0}")]
        [ModelDefault("EditMask", "n0")]
        public int? RotaSiraNo
        {
            get => rotaSiraNo;
            set => SetPropertyValue(nameof(RotaSiraNo), ref rotaSiraNo, value);
        }

        [XafDisplayName("Operasyon Adı"), ToolTip("Operasyon Adını Giriniz...")]
        [RuleRequiredField("RuleReguired_RotaTanimD_OperasyonKodu", DefaultContexts.Save, "Lütfen Operasyon Kodunu Giriniz...")]

        public OperasyonTanim OperasyonTanim
        {
            get => operasyonTanim;
            set => SetPropertyValue(nameof(OperasyonTanim), ref operasyonTanim, value);
        }

        [XafDisplayName("Takip Gerektiriyor mu?")]
        public bool TakipGerektirmiyor
        {
            get => takipGerektirmiyor;
            set => SetPropertyValue(nameof(TakipGerektirmiyor), ref takipGerektirmiyor, value);
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [Association("RotaTanimM-RotaTanimDs")]
        public RotaTanimM RotaTanimM
        {
            get => rotaTanimM;
            set => SetPropertyValue(nameof(RotaTanimM), ref rotaTanimM, value);
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            RotaSiraNo = 0;
        }

        protected override void OnSaving()
        {
            base.OnSaving();
        }

    }
}