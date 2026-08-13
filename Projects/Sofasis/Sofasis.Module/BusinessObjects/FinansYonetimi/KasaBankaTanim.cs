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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Sofasis.Module.Services;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{


    [DefaultProperty("HesapAdi")]
    [DefaultClassOptions]
    // Genel (kriterisiz) KasaBankaTanim_DetailView hem Kasa hem Banka kayıtları için aynı popup'ı
    // kullanıyor (lookup alanlarındaki "Düzenle" navigasyonu her zaman sınıfın genel DetailView'ini
    // açar, kayıt Kasa mı Banka mı olduğuna bakmaksızın) — bu yüzden Banka'ya özgü alanlar (Banka
    // Adı, Banka Şubesi, Iban Numarası) Kasa tipi kayıtlarda görünürlükten gizlenir; canlı testte
    // yakalandı (Kasa kaydı açılınca Banka alanları da anlamsızca görünüyordu).
    [Appearance("SH_KasaBankaTanim_BankaAlanlari", Visibility = ViewItemVisibility.Hide,
        TargetItems = "BankaTanim,BankaSubesi,IbanNo",
        Criteria = "CariKasaBankaTipi = 1", Context = "DetailView")]
    public class KasaBankaTanim : BaseClassWithAuditAndDescription
    { 
        public KasaBankaTanim(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            if (Session.IsNewObject(this))
            {
                DovizTanim doviz = Session.FindObject<DovizTanim>(
                    new BinaryOperator(nameof(DovizTanim.IsVarsayilan), true));
                if (doviz != null) this.dovizTanim = doviz;

                this.AcilisBakiyesiTarihi = DateTime.UtcNow.Date;
                if(cariKasaBankaTipi== CariKasaBankaTipi.Kasa)
                    HesapNo = GlobalUsing.ConstNewRecordText;
            }

        }


        DateTime acilisBakiyesiTarihi;
        decimal? acilisBakiyesi;
        DovizTanim dovizTanim;
        string ıbanNo;
        string hesapNo;
        string bankaSubesi;
        BankaTanim bankaTanim;
        CariKasaBankaTipi cariKasaBankaTipi;
        string hesapAdi;
        FisTuruTanim fisTuruTanim;


        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        [ModelDefault("AllowEdit", "False")]
        public CariKasaBankaTipi CariKasaBankaTipi
        {
            get => cariKasaBankaTipi;
            set => SetPropertyValue(nameof(CariKasaBankaTipi), ref cariKasaBankaTipi, value);
        }


        [XafDisplayName("Hesap Adı")]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [Appearance("ED_KasaBankaTanim_HesapAdi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleReguired_KasaBankaTanim_HesapAdi", DefaultContexts.Save, "Lütfen Hesap Adını Giriniz...")]
        [Size(120)]
        public string HesapAdi
        {
            get => hesapAdi;
            set => SetPropertyValue(nameof(HesapAdi), ref hesapAdi, value);
        }

        [XafDisplayName("Banka Adı")]
        public BankaTanim BankaTanim
        {
            get => bankaTanim;
            set => SetPropertyValue(nameof(BankaTanim), ref bankaTanim, value);
        }


        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        [XafDisplayName("Banka Şubesi")]
        public string BankaSubesi
        {
            get => bankaSubesi;
            set => SetPropertyValue(nameof(BankaSubesi), ref bankaSubesi, value);
        }


        [Appearance("ED_KasaBankaTanim_HesapNo",
        Enabled = false, TargetItems = "HesapNo",
            Criteria = "CariKasaBankaTipi = 0", Context = "DetailView")]
        [Size(32)]
        [XafDisplayName("Hesap No")]
        public string HesapNo
        {
            get => hesapNo;
            set => SetPropertyValue(nameof(HesapNo), ref hesapNo, value);
        }


        [Size(26)]
        [XafDisplayName("Iban Numarası"), ToolTip("Personel Iban Nomarasını Giriniz...")]
        [RuleRegularExpression(DefaultContexts.Save, Helper.IbanNoTypeRegEx,
            CustomMessageTemplate = "Lütfen Geçerli Bir İban Numarası Giriniz...")]
        public string IbanNo
        {
            get => ıbanNo;
            set => SetPropertyValue(nameof(IbanNo), ref ıbanNo, value);
        }

        [XafDisplayName("Döviz Kodu")]
        [RuleRequiredField("RuleReguired_KasaBankaTanim_DovizKodu", DefaultContexts.Save, "Lütfen Döviz Kodunu Giriniz...")]
        public DovizTanim DovizTanim
        {
            get => dovizTanim;
            set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
        }

        [XafDisplayName("Açılış Bakiyesi")]
        [RuleRequiredField("RuleReguired_KasaBankaTanim_AcilisBakiyesi", DefaultContexts.Save, "Lütfen Açılış Bakiyesini Giriniz...")]
        [VisibleInListView(false)]
        public decimal? AcilisBakiyesi
        {
            get => acilisBakiyesi;
            set => SetPropertyValue(nameof(AcilisBakiyesi), ref acilisBakiyesi, value);
        }

        [XafDisplayName("Açılış Bakiyesi Tarihi")]
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]
        [RuleRequiredField("RuleReguired_KasaBankaTanim_AcilisBakiyesiTarihi", DefaultContexts.Save, "Lütfen Açılış Bakiyesi Tarihini Giriniz...")]
        [VisibleInListView(false)]
        public DateTime AcilisBakiyesiTarihi
        {
            get => acilisBakiyesiTarihi;
            set => SetPropertyValue(nameof(AcilisBakiyesiTarihi), ref acilisBakiyesiTarihi, value);
        }

        [XafDisplayName("Fiş Türü")]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [RuleRequiredField("RuleRequired_KasaBankaTanim_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
        public FisTuruTanim FisTuruTanim
        {
            get => fisTuruTanim;
            set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
        }

        protected override void OnSaving()
        {
            if (Session.IsNewObject(This) &&
                HesapNo == Helper.ConstNewRecordText &&
                FisTuruTanim != null && CariKasaBankaTipi== CariKasaBankaTipi.Kasa)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                HesapNo = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, AcilisBakiyesiTarihi);
            }
            base.OnSaving();
        }

    }
}