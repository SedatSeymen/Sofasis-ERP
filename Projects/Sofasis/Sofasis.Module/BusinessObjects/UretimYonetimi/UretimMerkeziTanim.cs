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
using Sofasis.Module.Services;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("UretimMerkeziAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Üretim Merkezi Tanımlama")]
    public class UretimMerkeziTanim : BaseClassWithAuditAndDescription
    { 
        public UretimMerkeziTanim(Session session)
            : base(session)
        {
        }

        string uretimMerkeziAdi;
        string uretimMerkeziKodu;
        FisTuruTanim fisTuruTanim;

        [Size(16)]
        // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
        // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış
        // pozitif "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
        [RuleUniqueValue(TargetCriteria = "UretimMerkeziKodu != '" + Helper.ConstNewRecordText + "'")]
        [Indexed(Unique = true)]
        [XafDisplayName("Üretim Merkezi Kodu"), ToolTip("Üretim Merkezi Kodunu Giriniz...")]
        [RuleRequiredField("RuleReguired_UretimMerkeziTanim_UretimMerkeziKodu", DefaultContexts.Save, "Lütfen Üretim Merkezi Kodunu Giriniz...")]
        [Appearance("ED_UretimMerkeziTanim_UretimMerkeziKodu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public string UretimMerkeziKodu
        {
            get => uretimMerkeziKodu;
            set => SetPropertyValue(nameof(UretimMerkeziKodu), ref uretimMerkeziKodu, value);
        }


        [Size(13)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Üretim Merkezi Adı"), ToolTip("Üretim Merkezi Adını Giriniz...")]
        [RuleRequiredField("RuleReguired_UretimMerkeziTanim_UretimMerkeziAdi", DefaultContexts.Save, "Lütfen Üretim Merkezi Adını Giriniz...")]
        [Appearance("ED_UretimMerkeziTanim_UretimMerkeziAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]

        public string UretimMerkeziAdi
        {
            get => uretimMerkeziAdi;
            set => SetPropertyValue(nameof(UretimMerkeziAdi), ref uretimMerkeziAdi, value);
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(this))
            {
                this.UretimMerkeziKodu = Helper.ConstNewRecordText;
                CriteriaOperator criteria =
                    CriteriaOperator.FromLambda<FisTuruTanim>(x => x.ViewName == "UretimMerkeziTanim_DetailView");
                var FisTuruTanim = Session.FindObject<FisTuruTanim>(criteria);
                if(FisTuruTanim != null)
                    this.FisTuruTanim = FisTuruTanim;
                FisTuruVarsayilanlariniUygula(this.FisTuruTanim);
            }

        }

        [XafDisplayName("Fiş Türü")]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [RuleRequiredField("RuleRequired_UretimMerkeziTanim_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
        public FisTuruTanim FisTuruTanim
        {
            get => fisTuruTanim;
            set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
        }

        protected override void OnSaving()
        {
            if (Session.IsNewObject(This) &&
                UretimMerkeziKodu == Helper.ConstNewRecordText &&
                FisTuruTanim != null)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                UretimMerkeziKodu = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, DateTime.UtcNow);
            }
            base.OnSaving();
        }
        protected override void OnDeleting()
        {
            try
            {
                UretimMerkeziTanim deletedObject = this;
                if (deletedObject == null) return;

                if (deletedObject.IsSystemRecord == true)
                {
                    throw new UserFriendlyException("Bu Bir Sistem kaydıdır !!! Silemezsiniz");
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