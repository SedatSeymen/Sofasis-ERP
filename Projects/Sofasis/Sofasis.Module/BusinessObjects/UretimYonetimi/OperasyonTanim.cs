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
    [DefaultProperty("OperasyonAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Üretim Operasyon Tanımlama")]
    public class OperasyonTanim : BaseClassWithAuditAndDescription
    { 
        public OperasyonTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(this))
            {
                OperasyonKodu = Helper.ConstNewRecordText;
                OperasyonSiraNo = int.MinValue;
                CriteriaOperator criteria =
                    CriteriaOperator.FromLambda<FisTuruTanim>(x => x.ViewName == "OperasyonTanim_DetailView");
                var FisTuruTanim = Session.FindObject<FisTuruTanim>(criteria);
                if (FisTuruTanim != null)
                    this.FisTuruTanim = FisTuruTanim;
                FisTuruVarsayilanlariniUygula(this.FisTuruTanim);
            }
        }


        int? operasyonSiraNo;
        string operasyonAdi;
        string operasyonKodu;
        FisTuruTanim fisTuruTanim;



        [Size(16)]
        // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
        // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış
        // pozitif "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
        [RuleUniqueValue(TargetCriteria = "OperasyonKodu != '" + Helper.ConstNewRecordText + "'")]
        [Indexed(Unique = true)]
        [XafDisplayName("Operasyon Kodu"), ToolTip("Operasyon Kodunu Giriniz...")]
        [RuleRequiredField("RuleReguired_OperasyonTanim_OperasyonKodu", DefaultContexts.Save, "Lütfen Operasyon Kodunu Giriniz...")]
        [Appearance("ED_OperasyonTanim_OperasyonKodu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public string OperasyonKodu
        {
            get => operasyonKodu;
            set => SetPropertyValue(nameof(OperasyonKodu), ref operasyonKodu, value);
        }


        [Size(50)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Operasyon Adı"), ToolTip("Operasyon Adı Giriniz...")]
        [RuleRequiredField("RuleReguired_OperasyonTanim_OperasyonAdi", DefaultContexts.Save, "Lütfen Operasyon Adını Giriniz...")]
        [Appearance("ED_OperasyonTanim_OperasyonAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]

        public string OperasyonAdi
        {
            get => operasyonAdi;
            set => SetPropertyValue(nameof(OperasyonAdi), ref operasyonAdi, value);
        }

        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Operasyon Sıra No"), ToolTip("Operasyon Sıra No Giriniz")]
        [RuleRequiredField("", DefaultContexts.Save, "Lütfen Operasyon Sıra No Giriniz...")]
        [Appearance("ED_OperasyonTanim_OperasyonSiraNo", Enabled = false, Criteria = "", Context = "DetailView")]
        [ModelDefault("DisplayFormat", "{0:n0}")]
        [ModelDefault("EditMask", "n0")]
        public int? OperasyonSiraNo
        {
            get => operasyonSiraNo;
            set => SetPropertyValue(nameof(OperasyonSiraNo), ref operasyonSiraNo, value);
        }

        [XafDisplayName("Fiş Türü")]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [RuleRequiredField("RuleRequired_OperasonTanim_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
        public FisTuruTanim FisTuruTanim
        {
            get => fisTuruTanim;
            set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
        }

        protected override void OnSaving()
        {
            if (Session.IsNewObject(This) &&
                OperasyonKodu == Helper.ConstNewRecordText &&
                FisTuruTanim != null)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                OperasyonKodu = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, DateTime.UtcNow);
            }
            if (Session.IsNewObject(This) && OperasyonSiraNo == int.MinValue)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                OperasyonSiraNo = numberSequenceService.SonrakiSiraNo(Session, GetType().FullName);
            }
            base.OnSaving();
        }
        protected override void OnDeleting()
        {
            try
            {
                OperasyonTanim deletedObject = this;
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