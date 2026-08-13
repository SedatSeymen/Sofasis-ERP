using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("UlkeAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Ülke Tanımlama")]
    [NavigationItem("Genel Tanımlar")]
    [Appearance("ED_UlkeTanim", Enabled = false, TargetItems = "*", Criteria = "IsSystemRecord = true", Context = "DetailView")]

    [Appearance("UlkeTanimColor", AppearanceItemType = "ViewItem",
    TargetItems = "*", Criteria = "IsVarsayilan = 1", Context = "ListView",
        FontColor = "Orange", FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]
    public class UlkeTanim : BaseClassWithAuditAndDescription
    {
        public UlkeTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        bool isVarsayilan;
        string ulkeKodu;
        string ulkeAdi;
        string ulkeTelefonKodu;

        [XafDisplayName("Ülke Kodu"), ToolTip("ISO 3166-1 alfa-2 ülke kodu (örn. TR)")]
        [RuleRequiredField("ÜlkeKoduRequired", DefaultContexts.Save, "Lütfen Ülke Kodunu Giriniz...")]
        [Appearance("EnableDisableUlkeKodu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [Size(3)]
        [RuleUniqueValue("ÜlkeKoduUnique", DefaultContexts.Save, "Bu Ülke Kodu Zaten Kayıtlı...")]
        [Indexed(Unique = true)]
        public string UlkeKodu
        {
            get => ulkeKodu;
            set => SetPropertyValue(nameof(UlkeKodu), ref ulkeKodu, value);
        }

        [XafDisplayName("Ülke Adı"), ToolTip("Ülke Adını Giriniz")]
        [RuleRequiredField("ÜlkeAdiRequired", DefaultContexts.Save, "Lütfen Ülke Adını Giriniz...")]
        [Appearance("EnableDisableUlkeAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        public string UlkeAdi
        {
            get => ulkeAdi;
            set => SetPropertyValue(nameof(UlkeAdi), ref ulkeAdi, value);
        }

        [XafDisplayName("Telefon Kodu"), ToolTip("Uluslararası telefon kodu (örn. +90)")]
        [Size(6)]
        public string UlkeTelefonKodu
        {
            get => ulkeTelefonKodu;
            set => SetPropertyValue(nameof(UlkeTelefonKodu), ref ulkeTelefonKodu, value);
        }

        [XafDisplayName("Şehir Tanımlama")]
        [Association("UlkeTanim-SehirTanims"), Aggregated]
        public XPCollection<SehirTanim> SehirTanims
        {
            get
            {
                return GetCollection<SehirTanim>(nameof(SehirTanims));
            }
        }

        [XafDisplayName("Varsayılan mı ?")]
        public bool IsVarsayilan
        {
            get => isVarsayilan;
            set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
        }

        protected override void OnDeleting()
        {
            try
            {
                var AdresList = new XPQuery<AdresTanim>(Session).Where(x => x.UlkeTanim == this);

                if (AdresList.Count() > 0)
                {
                    throw new UserFriendlyException("Bu Ülke Adres Tanımlarında Kullanılmış !!! Silemezsiniz");
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
        protected override void OnSaving()
        {
            base.OnSaving();
        }
    }
}
