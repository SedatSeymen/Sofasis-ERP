using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{


    [XafDisplayName("Günlük Döviz Kur Girişi")]
    [NavigationItem("Genel Tanımlar")]
    [DefaultClassOptions]
    public class DovizGunlukKurM : BaseClassWithAudit
    {
        public DovizGunlukKurM(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if(Session.IsNewObject(this))
            {
                this.KurTarihi = DateTime.UtcNow.Date;
                KurAciklama = Helper.ConstNewRecordText;

            }
        }



        DateTime kurSaati;
        DateTime kurTarihi;
        string kurAciklama;


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [Appearance("ED_DovizGunlukKurMaster_KurAciklama", Enabled = false, Criteria = "", Context = "DetailView")]

        [XafDisplayName("Kur Açıklama"), ToolTip("Döviz Kuru Açıklamasını Giriniz")]
        public string KurAciklama
        {
            get => kurAciklama;
            set => SetPropertyValue(nameof(KurAciklama), ref kurAciklama, value);
        }

        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Kur Tarihi"), ToolTip("Kur Tarihini Giriniz")]
        [Appearance("ED_DovizGunlukKurMaster_KurTarihi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [ModelDefault("EditMask", "D")]
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]

        public DateTime KurTarihi
        {
            get => kurTarihi;
            set
            {
                if (kurTarihi != value)
                {
                    DateTime OldValue = kurTarihi;
                    kurTarihi = value;
                    OnChanged(nameof(KurTarihi), OldValue, KurTarihi);
                }
            }
        }

        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [XafDisplayName("Kur Saati")]

        public DateTime KurSaati
        {
            get => kurSaati;
            set => SetPropertyValue(nameof(KurSaati), ref kurSaati, value);
        }

        [XafDisplayName("Günlük Kur Tanımlama")]
        [Association("DovizGunlukKurMaster-DovizGunlukKurDetails"),Aggregated]
        public XPCollection<DovizGunlukKurD> DovizGunlukKurDetails
        {
            get
            {
                return GetCollection<DovizGunlukKurD>(nameof(DovizGunlukKurDetails));
            }
        }

        private void Change_KurAciklama()
        {
            this.KurAciklama = KurTarihi.ToShortDateString() + " Tarihli Merkez Bankası Döviz Kurları";
        }

        protected override void OnSaving()
        {
            Change_KurAciklama();
        }

    }
}