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

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("StokGrupAdi")]
    [DefaultClassOptions]
    [Appearance("ED_StokGrupTanim", Enabled = false, TargetItems = "*", Criteria = "IsSystemRecord = true", Context = "DetailView")]
    [XafDisplayName("Stok Grup Tanımlama")]
    
    
    [Appearance("StokGrupTanimColor", AppearanceItemType = "ViewItem",
    TargetItems = "*", Criteria = "IsVarsayilan = 1", Context = "ListView",
        FontColor = "Orange", FontStyle = DevExpress.Drawing.DXFontStyle.Bold, Priority = 1)]

    public class StokGrupTanim : BaseClassWithAuditAndDescription
    {
        public StokGrupTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        bool isVarsayilan;
        string stokGrupAdi;



        [Size(50)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Stok Grup Adı"), ToolTip("Grup Adını Giriniz")]
        [RuleRequiredField("StokGrupAdiRequired", DefaultContexts.Save, "Lütfen Stok Grup Adını Giriniz...")]
        public string StokGrupAdi
        {
            get => stokGrupAdi;
            set => SetPropertyValue(nameof(StokGrupAdi), ref stokGrupAdi, value);
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

        protected override void OnDeleting()
        {
            try
            {
                // B1: Silme koruması aktif — kullanımdaki grup silinemez.
                if (new XPQuery<StokTanim>(Session).Count(x => x.StokGrupTanim != null && x.StokGrupTanim == this) > 0)
                    throw new UserFriendlyException("Bu stok grubu stok tanımlarında kullanılmış, silemezsiniz.");
                if (new XPQuery<StokParametre>(Session).Count(x => x.KumasStokGrubu != null && x.KumasStokGrubu == this) > 0)
                    throw new UserFriendlyException("Bu stok grubu, Stok Parametrelerinde Kumaş Stok Grubu olarak tanımlanmış, silemezsiniz.");
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