using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using Sofasis.Module.Services;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("RotaAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Üretim Rota Tanımlama")]
    public class RotaTanimM : BaseClassWithAuditAndDescription
    { 
        public RotaTanimM(Session session)
            : base(session)
        {
            this.RotaTanimDs.CollectionChanged += RotaTanimDs_CollectionChanged;
        }

        private void RotaTanimDs_CollectionChanged(object sender, XPCollectionChangedEventArgs e)
        {
            if (this != null)
            {
                var list = this.RotaTanimDs.ToList().OrderBy(x => x.RotaSiraNo);

                if (list != null && list.Count() > 0)
                {
                    foreach (var item in list)
                    {
                        item.RotaSiraNo = 0;
                    }
                    foreach (var item in list)
                    {
                        if (item.RotaSiraNo == 0 || item.RotaSiraNo == null)
                        {
                            int? newRotaSiraNo = list.Max(obj => obj.RotaSiraNo);
                            if (newRotaSiraNo == null) newRotaSiraNo = 0;
                            item.RotaSiraNo = newRotaSiraNo + 1;
                        }

                    }
                }


            }
        }

        bool? takipGerektirmiyor;
        string rotaAdi;
        string rotaKodu;
        FisTuruTanim fisTuruTanim;


        [Size(16)]
        // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
        // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış
        // pozitif "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
        [RuleUniqueValue(TargetCriteria = "RotaKodu != '" + Helper.ConstNewRecordText + "'")]
        [Indexed(Unique = true)]
        [XafDisplayName("Rota Kodu"), ToolTip("Rota Kodunu Giriniz...")]
        [RuleRequiredField("RuleReguired_RotaTanimM_RotaKodu", DefaultContexts.Save, "Lütfen Rota Kodunu Giriniz...")]
        [Appearance("ED_RotaTanimM_RotaKodu", Enabled = false, Criteria = "", Context = "DetailView")]
        public string RotaKodu
        {
            get => rotaKodu;
            set => SetPropertyValue(nameof(RotaKodu), ref rotaKodu, value);
        }


        [Size(50)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Rota Adı"), ToolTip("Rota Adını Giriniz...")]
        [RuleRequiredField("RuleReguired_RotaTanimM_RotaAdi", DefaultContexts.Save, "Lütfen Rota Adını Giriniz...")]
        [Appearance("ED_RotaTanimM_RotaAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]

        public string RotaAdi
        {
            get => rotaAdi;
            set => SetPropertyValue(nameof(RotaAdi), ref rotaAdi, value);
        }

        [XafDisplayName("Takip Gerektiriyor mu?")]
        public bool? TakipGerektirmiyor
        {
            get => takipGerektirmiyor;
            set
            {
                SetPropertyValue(nameof(TakipGerektirmiyor), ref takipGerektirmiyor, value);
                TakipGuncelle(TakipGerektirmiyor);
            }
        }

        private void TakipGuncelle(bool? NewValue)
        {
            if (TakipGerektirmiyor == null) return;
            if (RotaTanimDs != null && RotaTanimDs.IsLoaded)
            {
                foreach (var item in RotaTanimDs)
                {
                    item.TakipGerektirmiyor = (bool)NewValue;
                }
            }
        }

        [Association("RotaTanimM-RotaTanimDs"),Aggregated]
        public XPCollection<RotaTanimD> RotaTanimDs
        {
            get
            {
                return GetCollection<RotaTanimD>(nameof(RotaTanimDs));
            }
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(this))
            {
                RotaKodu = Helper.ConstNewRecordText;
                CriteriaOperator criteria =
                    CriteriaOperator.FromLambda<FisTuruTanim>(x => x.ViewName == "RotaTanimM_DetailView");
                var FisTuruTanim = Session.FindObject<FisTuruTanim>(criteria);
                if (FisTuruTanim != null)
                    this.FisTuruTanim = FisTuruTanim;
                FisTuruVarsayilanlariniUygula(this.FisTuruTanim);
            }
        }


        [XafDisplayName("Fiş Türü")]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [RuleRequiredField("RuleRequired_RotaTanimMTanim_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
        public FisTuruTanim FisTuruTanim
        {
            get => fisTuruTanim;
            set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
        }

        protected override void OnSaving()
        {
            if (Session.IsNewObject(This) &&
                RotaKodu == Helper.ConstNewRecordText &&
                FisTuruTanim != null)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                RotaKodu = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, DateTime.UtcNow);
            }
            base.OnSaving();
        }

        [Obsolete]
        protected override void BeforeSave()
        {
            OnSaving();
        }
        protected override void OnDeleting()
        {
            try
            {
                RotaTanimM deletedObject = this;
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