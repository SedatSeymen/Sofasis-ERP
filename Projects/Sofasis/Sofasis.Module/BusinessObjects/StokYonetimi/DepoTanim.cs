using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("DepoAdi")]
    [DefaultClassOptions]
    [Appearance("ED_DepoTanim", Enabled = false, TargetItems = "*", Criteria = "IsSystemRecord = true", Context = "DetailView")]

    [Appearance("DepoTanimColor", AppearanceItemType = "ViewItem",
    TargetItems = "*", Criteria = "IsVarsayilan = 1", Context = "ListView",
        FontColor = "Orange", FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]


    [XafDisplayName("Depo Tanımlama")]
    public class DepoTanim : BaseClassWithAudit
    {
        public DepoTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            UlkeTanim ulke = Session.FindObject<UlkeTanim>(
                new BinaryOperator(nameof(UlkeTanim.IsVarsayilan), true));
            if (ulke != null) this.ulkeTanim = ulke;
            base.AfterConstruction();
        }

        bool isVarsayilan;
        SehirTanim sehirTanim;
        UlkeTanim ulkeTanim;
        string depoAdresi;
        string depoAdi;


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Depo Adı"), ToolTip("Depo Adını Giriniz")]
        [RuleRequiredField("DepoAdiRequired", DefaultContexts.Save, "Lütfen Depo Adını Giriniz...")]
        [Appearance("EnableDisableDepoAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]

        public string DepoAdi
        {
            get => depoAdi;
            set => SetPropertyValue(nameof(DepoAdi), ref depoAdi, value);
        }

        [VisibleInListView(false)]
        [Size(200)]
        [ModelDefault("RowCount", "3")]
        [XafDisplayName("Adres")]
        public string DepoAdresi
        {
            get => depoAdresi;
            set => SetPropertyValue(nameof(DepoAdresi), ref depoAdresi, value);
        }
        [VisibleInListView(false)]
        [XafDisplayName("Ülke Adı")]
        public UlkeTanim UlkeTanim
        {
            get { return ulkeTanim; }
            set
            {
                SetPropertyValue(nameof(UlkeTanim), ref ulkeTanim, value);
                if (!IsLoading)
                {
                    SehirTanim = null;
                }
            }
        }
        [VisibleInListView(false)]
        [XafDisplayName("Şehir Adı")]
        [DataSourceProperty("UlkeTanim.SehirTanims", DataSourcePropertyIsNullMode.SelectAll)]

        public SehirTanim SehirTanim
        {
            get => sehirTanim;
            set => SetPropertyValue(nameof(SehirTanim), ref sehirTanim, value);
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

        // Faz 2: StokHareketleriM/StokBakiye/StokTransferi eklenene kadar bu depo hiçbir yerde
        // referans alınmıyordu, bu yüzden şimdiye kadar hiç silme koruması yoktu.
        protected override void OnDeleting()
        {
            try
            {
                if (new XPQuery<StokHareketleriM>(Session).Count(x => x.DepoTanim != null && x.DepoTanim == this) > 0)
                    throw new UserFriendlyException("Bu depo hareket görmüş, silemezsiniz.");
                if (new XPQuery<StokBakiye>(Session).Count(x => x.DepoTanim != null && x.DepoTanim == this) > 0)
                    throw new UserFriendlyException("Bu deponun bakiyesi bulunuyor, silemezsiniz.");
                if (new XPQuery<StokTransferi>(Session).Count(x => x.KaynakDepo == this || x.HedefDepo == this) > 0)
                    throw new UserFriendlyException("Bu depo transferlerde kullanılmış, silemezsiniz.");
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