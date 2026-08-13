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
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    [XafDisplayName("Fiyat Listeleri")]
    [DefaultProperty("FiyatListeAdi")]
    public class SatisFiyatListeM : BaseClassWithAuditAndDescription
    {
        public SatisFiyatListeM(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(This))
            {
                BaslangicTarihi = DateTime.Now.Date;
                BitisTarihi = DateTime.Now.AddMonths(1);
                ListeOrani = 20;
                KarOrani = 20;
                KDVTanim kDV = Session.FindObject<KDVTanim>(
                    new BinaryOperator(nameof(KDVTanim.IsVarsayilan), true));
                if (kDV != null) KDVTanim = kDV;


                DovizTanim Doviz = Session.FindObject<DovizTanim>(
                    new BinaryOperator(nameof(DovizTanim.IsVarsayilan), true));
                if (Doviz != null) DovizTanim = Doviz;

                IsAktif = true;
                FiyatListeAdi = Helper.ConstNewRecordText;
                var Sablon = new XPCollection<FiyatListeSablonM>(Session).FirstOrDefault();
                if (Sablon != null)
                    FiyatListeSablonM = Sablon;
            }
        }

        decimal? karOrani;
        FiyatListeSablonM fiyatListeSablonM;
        bool isAktif;
        bool isVarsayilan;
        KDVTanim kDVTanim;
        decimal? listeOrani;
        DovizTanim dovizTanim;
        DateTime bitisTarihi;
        DateTime baslangicTarihi;
        string fiyatListeAdi;

        [Size(100)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Liste Adı"), ToolTip("Fiyat Liste Adını Giriniz...")]
        [RuleRequiredField("RuleRequired_SatisFiyatListeM_ListeAdi", DefaultContexts.Save, "Lütfen Fiyat Liste Adını Giriniz...")]
        public string FiyatListeAdi
        {
            get => fiyatListeAdi;
            set => SetPropertyValue(nameof(FiyatListeAdi), ref fiyatListeAdi, value);
        }

        [XafDisplayName("Döviz Adı"), ToolTip("Döviz Adını Giriniz")]
        [Appearance("ED_SatisFiyatListeM_DovizTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_SatisFiyatListeM_DovizTanim", DefaultContexts.Save, "Lütfen Döviz Adını Giriniz...")]

        public DovizTanim DovizTanim
        {
            get => dovizTanim;
            set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
        }

        [XafDisplayName("Başlangıç Tarihi"), ToolTip("Başlangıç Tarihini Giriniz")]
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]
        [RuleValueComparison("RuleValueComparison_SatisFiyatListeM_BaslangicTarihi", DefaultContexts.Save, ValueComparisonType.LessThanOrEqual, nameof(BitisTarihi), "Başlangıç Tarihi Bitiş Tarihinden Büyük Olamaz", ParametersMode.Expression)]

        public DateTime BaslangicTarihi
        {
            get => baslangicTarihi;
            set => SetPropertyValue(nameof(BaslangicTarihi), ref baslangicTarihi, value);
        }

        [XafDisplayName("Bitis Tarihi"), ToolTip("Bitiş Tarihini Giriniz")]
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]
        [RuleValueComparison("RuleValueComparison_SatisFiyatListeM_BitisTarihi", DefaultContexts.Save, ValueComparisonType.GreaterThanOrEqual, nameof(BaslangicTarihi), "Bitiş Tarihi Başlangıç Tarihinden Küçük Olamaz", ParametersMode.Expression)]

        public DateTime BitisTarihi
        {
            get => bitisTarihi;
            set => SetPropertyValue(nameof(BitisTarihi), ref bitisTarihi, value);
        }

        [XafDisplayName("Liste İsk Oranı"), ToolTip("Liste İskonto Giriniz")]
        [Appearance("ED_SatisFiyatListeM_ListeOrani", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_SatisFiyatListeM_ListeOrani", DefaultContexts.Save, "Lütfen Liste Oranını Giriniz...")]
        [ModelDefault("EditMask", "N")]
        [ModelDefault("DisplayFormat", "{0:N}")]
        public decimal? ListeOrani
        {
            get => listeOrani;
            set => SetPropertyValue(nameof(ListeOrani), ref listeOrani, value);
        }

        [XafDisplayName("Kar Oranı"), ToolTip("Liste Kar Oranı Giriniz")]
        [ModelDefault("EditMask", "N")]
        [ModelDefault("DisplayFormat", "{0:N}")]
        [Appearance("ED_SatisFiyatListeM_KarOrani", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_SatisFiyatListeM_KarOrani", DefaultContexts.Save, "Lütfen Liste Kar Oranını Giriniz...")]

        public decimal? KarOrani
        {
            get => karOrani;
            set => SetPropertyValue(nameof(KarOrani), ref karOrani, value);
        }

        [XafDisplayName("KDV Oranı")]
        [ModelDefault("EditMask", "N")]
        [ModelDefault("DisplayFormat", "{0:N}")]
        [Appearance("ED_SatisFiyatListeM_KDVOrani", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_SatisFiyatListeM_KDVOrani", DefaultContexts.Save, "Lütfen KDV Oranını Giriniz...")]
        public KDVTanim KDVTanim
        {
            get => kDVTanim;
            set => SetPropertyValue(nameof(KDVTanim), ref kDVTanim, value);
        }

        [XafDisplayName("Fiyat Liste Şablonu")]
        [Appearance("ED_SatisFiyatListeM_FiyatListeSablonM", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_SatisFiyatListeM_FiyatListeSablonM", DefaultContexts.Save, "Lütfen Fiyat Liste Şablonunu Giriniz...")]

        public FiyatListeSablonM FiyatListeSablonM
        {
            get => fiyatListeSablonM;
            set => SetPropertyValue(nameof(FiyatListeSablonM), ref fiyatListeSablonM, value);
        }

        [XafDisplayName("Varsayılan mı ?")]
        public bool IsVarsayilan
        {
            get => isVarsayilan;
            set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
        }

        [XafDisplayName("Liste Aktif mi ?")]
        public bool IsAktif
        {
            get => isAktif;
            set => SetPropertyValue(nameof(IsAktif), ref isAktif, value);
        }

        [Association("SatisFiyatListeM-SatisFiyatListeD"), Aggregated]
        public XPCollection<SatisFiyatListeD> SatisFiyatListeD
        {
            get
            {
                return GetCollection<SatisFiyatListeD>(nameof(SatisFiyatListeD));
            }
        }

        protected override void OnDeleting()
        {
            try
            {
                var Carilist = new XPQuery<CariHesapTanim>(Session).Where(x => x.SatisFiyatListeM == this);
                var Siparislist = new XPQuery<SatisSiparisM>(Session).Where(x => x.SatisFiyatListeM == this);

                if (Carilist.Count() > 0)
                {
                    throw new UserFriendlyException("Bu Liste Cari Hesaplarda Kullanılmış Silemezsiniz !!!");
                }
                else if (Siparislist.Count() > 0)
                {
                    throw new UserFriendlyException("Bu Liste Siparişlerde Kullanılmış Silemezsiniz !!!");
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

            if (IsVarsayilan)
            {
                SatisFiyatListeM entity = Session.FindObject<SatisFiyatListeM>(
                    new BinaryOperator(nameof(SatisFiyatListeM.IsVarsayilan), true));
                if (entity != null)
                {
                    entity.IsVarsayilan = false;
                    entity.Save();
                }
            }
            if(FiyatListeAdi == Helper.ConstNewRecordText)
            {
                FiyatListeAdi = BaslangicTarihi.ToShortDateString() + "-" + BitisTarihi.Date.ToShortDateString() +
                    " " + dovizTanim.DovizKodu + " - " + DovizTanim.DovizAdi + " FİYAT LİSTESİ";
            }


            base.OnSaving();
        }

    }
}