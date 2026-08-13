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
    [DefaultProperty("ReceteAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Üretim Reçete Tanımlama")]
    public class ReceteTanimM : BaseClassWithAuditAndDescription
    { 
        public ReceteTanimM(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(this))
            {
                Miktar = 1;
                CriteriaOperator criteria = CriteriaOperator.Parse(nameof(BirimTanim.IsVarsayilan), true);
                BirimTanim = Session.FindObject<BirimTanim>(criteria);

                criteria = CriteriaOperator.FromLambda<RotaTanimM>(x => x.RotaAdi.Contains("STANDART"));
                RotaTanimM = Session.FindObject<RotaTanimM>(criteria);
                this.ReceteKodu = Helper.ConstNewRecordText;

                CriteriaOperator rcriteria =
                    CriteriaOperator.FromLambda<FisTuruTanim>(x => x.ViewName == "ReceteTanimM_DetailView");
                var FisTuruTanim = Session.FindObject<FisTuruTanim>(rcriteria);
                if (FisTuruTanim != null)
                    this.FisTuruTanim = FisTuruTanim;
                FisTuruVarsayilanlariniUygula(this.FisTuruTanim);
            }
        }

        string receteKodu;
        decimal? urunMaliyeti;
        decimal? genelGiderOrani;
        RotaTanimM rotaTanimM;
        BirimTanim birimTanim;
        decimal? miktar;
        StokTanim stokTanim;
        FisTuruTanim fisTuruTanim;

        [Size(16)]
        // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
        // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış
        // pozitif "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
        [RuleUniqueValue(TargetCriteria = "ReceteKodu != '" + Helper.ConstNewRecordText + "'")]
        [Indexed(Unique = true)]
        [RuleRequiredField("RuleRequired_ReceteTanimM_ReceteKodu", DefaultContexts.Save, "Lütfen Miktar Giriniz...")]
        [Appearance("ED_ReceteTanimM_ReceteKodu", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [XafDisplayName("Reçete Kodu")]
        public string ReceteKodu
        {
            get => receteKodu;
            set => SetPropertyValue(nameof(ReceteKodu), ref receteKodu, value);
        }

        [XafDisplayName("Stok Adı")]
        [DataSourceCriteria("(StokTipi = 0 OR StokTipi = 1) and StokHizmetMasrafTipi = 0")]
        [Appearance("ED_ReceteTanimM_StokTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public StokTanim StokTanim
        {
            get => stokTanim;
            set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
        }

        [XafDisplayName("Miktar")]
        [RuleRequiredField("RuleRequired_ReceteTanimM_Miktar", DefaultContexts.Save, "Lütfen Miktar Giriniz...")]
        public decimal? Miktar
        {
            get => miktar;
            set => SetPropertyValue(nameof(Miktar), ref miktar, value);
        }

        [XafDisplayName("Birim")]
        [RuleRequiredField("RuleRequired_ReceteTanimM_BirimTanim", DefaultContexts.Save, "Lütfen Miktar Giriniz...")]
        public BirimTanim BirimTanim
        {
            get => birimTanim;
            set => SetPropertyValue(nameof(BirimTanim), ref birimTanim, value);
        }

        [XafDisplayName("Rota Adı")]
        [RuleRequiredField("RuleRequired_ReceteTanimM_RotaTanimM", DefaultContexts.Save, "Lütfen Rotayı Seçiniz...")]
        public RotaTanimM RotaTanimM
        {
            get => rotaTanimM;
            set => SetPropertyValue(nameof(RotaTanimM), ref rotaTanimM, value);
        }

        [XafDisplayName("Genel Gider Oranı")]
        [RuleRequiredField("RuleRequired_ReceteTanimM_GenelGiderOrani", DefaultContexts.Save, "Lütfen Genel Gider Oranını Seçiniz...")]
        public decimal? GenelGiderOrani
        {
            get => genelGiderOrani;
            set => SetPropertyValue(nameof(GenelGiderOrani), ref genelGiderOrani, value);
        }

        [XafDisplayName("Ürün Maliyeti")]
        [Appearance("ED_ReceteTanimM_UrunMaliyeti", Enabled = false, Criteria = "", Context = "DetailView")]

        public decimal? UrunMaliyeti
        {
            get => urunMaliyeti;
            set => SetPropertyValue(nameof(UrunMaliyeti), ref urunMaliyeti, value);
        }

        [Association("ReceteTanimM-ReceteTanimDs"), Aggregated]
        public XPCollection<ReceteTanimD> ReceteTanimDs
        {
            get
            {
                return GetCollection<ReceteTanimD>(nameof(ReceteTanimDs));
            }
        }

        [Association("ReceteTanimM-ReceteMaliyetDs"),Aggregated]
        public XPCollection<ReceteMaliyetD> ReceteMaliyetDs
        {
            get
            {
                return GetCollection<ReceteMaliyetD>(nameof(ReceteMaliyetDs));
            }
        }

        [XafDisplayName("Fiş Türü")]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [RuleRequiredField("RuleRequired_ReceteTanimMTanim_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
        public FisTuruTanim FisTuruTanim
        {
            get => fisTuruTanim;
            set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
        }

        protected override void OnSaving()
        {
            if (Session.IsNewObject(This) &&
                ReceteKodu == Helper.ConstNewRecordText &&
                FisTuruTanim != null)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                ReceteKodu = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, DateTime.UtcNow);
            }
            base.OnSaving();
        }

        protected override void OnDeleting()
        {
            try
            {
                // B1: Silme koruması aktif — reçetenin ürünü satılmışsa silinemez.
                if (this.StokTanim != null &&
                    new XPQuery<SatisSiparisD>(Session).Count(x => x.StokTanim == this.StokTanim) > 0)
                    throw new UserFriendlyException("Bu ürün reçetesiyle satış yapılmış, silemezsiniz.");
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