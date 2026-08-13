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
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("SehirAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Şehir Tanımlama")]
    public class SehirTanim : BaseClass
    {
        public SehirTanim(Session session)
            : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        string sehirAdi;
        string plakaKodu;
        private UlkeTanim ulkeTanim;
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Şehir Adı"), ToolTip("Şehir Adını Giriniz")]
        [RuleRequiredField("RuleRequired_SehirTanim_SehirAdi", DefaultContexts.Save, "Lütfen Şehir Adını Giriniz...")]
        [Appearance("ED_SehirTanim_SehirAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]

        public string SehirAdi
        {
            get => sehirAdi;
            set => SetPropertyValue(nameof(SehirAdi), ref sehirAdi, value);
        }

        [XafDisplayName("Plaka Kodu"), ToolTip("Şehir Plaka Kodu (örn. 34)")]
        [Size(3)]
        public string PlakaKodu
        {
            get => plakaKodu;
            set => SetPropertyValue(nameof(PlakaKodu), ref plakaKodu, value);
        }

        [Association("UlkeTanim-SehirTanims")]
        [Appearance("InvisibleUlkeAdi", Visibility = ViewItemVisibility.Hide, Context = "DetailView")]
        [XafDisplayName("Ülke Adı")]
        public UlkeTanim UlkeTanim
        {
            get { return ulkeTanim; }
            set { SetPropertyValue(nameof(UlkeTanim), ref ulkeTanim, value); }
        }

        [XafDisplayName("İlçe Tanımlama")]
        [Association("SehirTanim-IlceTanims"), Aggregated]
        public XPCollection<IlceTanim> IlceTanims
        {
            get
            {
                return GetCollection<IlceTanim>(nameof(IlceTanims));
            }
        }

        protected override void OnDeleting()
        {
            try
            {
                var AdresList = new XPQuery<AdresTanim>(Session).Where(x => x.SehirTanim == this);

                if (AdresList.Count() > 0)
                {
                    throw new UserFriendlyException("Bu Şehir Adres Tanımlarında Kullanılmış !!! Silemezsiniz");
                }

                else
                    base.OnDeleting();

            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Tracing.Tracer.LogError(ex);
                throw;
            }

        }

    }
}
