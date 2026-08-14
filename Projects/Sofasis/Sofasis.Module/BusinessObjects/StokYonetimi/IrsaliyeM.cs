using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using Sofasis.Module.Extensions;
using Sofasis.Module.Services;
using System;
using System.ComponentModel;
using System.Linq;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{
    // Başlık: Sipariş ile Fatura arasındaki FİZİKSEL mal hareketini temsil eden gerçek bir belge.
    // Önceki tasarımda bu adım StokHareketleriM (STSAGR) ile örtük/sessizce birleştirilmişti —
    // kullanıcı geri döndüğünde "irsaliye neden yok?" diye sorunca fark edildi (bkz.
    // docs/CHANGELOG.md 2026-08-14) ve kullanıcı isteğiyle AYRI bir iş nesnesi olarak kuruldu.
    // Kaydedildiği anda (ISatinAlmaIrsaliyeServisi.IrsaliyeTaslagiOlustur, popup açılmadan ÖNCE)
    // bire-bir bağlı bir StokHareketleriM/D (STSAGR) çifti de birlikte oluşturulur — asıl stok/
    // ağırlıklı-ortalama-maliyet motoru HÂLÂ StokHareketleriD'de, İrsaliye yalnızca "hangi sevkiyat"
    // bağlamını ve VUK'un istediği irsaliye bilgilerini taşır.
    [DefaultClassOptions]
    [DefaultProperty("IrsaliyeNo")]
    [XafDisplayName("İrsaliye")]
    [RuleCriteria("Rule_IrsaliyeM_EnAzBirSatir", DefaultContexts.Save,
        "IrsaliyeDs.Count > 0", "Lütfen en az bir irsaliye kalemi ekleyiniz.")]
    public class IrsaliyeM : BaseClassWithAuditAndDescription
    {
        public IrsaliyeM(Session session)
            : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            IrsaliyeTarihi = DateTime.UtcNow.Date;
            IrsaliyeNo = Helper.ConstNewRecordText;

            DepoTanim varsayilanDepo = Session.GetVarsayilan<DepoTanim>();
            if (varsayilanDepo != null) DepoTanim = varsayilanDepo;
        }

        string irsaliyeNo;
        DateTime irsaliyeTarihi;
        FisTuruTanim fisTuruTanim;
        CariHesapTanim tedarikci;
        SatinAlmaSiparisiM kaynakSiparis;
        string tedarikciIrsaliyeNo;
        DateTime? tedarikciIrsaliyeTarihi;
        IrsaliyeM kaynakIrsaliye;
        DepoTanim depoTanim;
        DovizTanim dovizTanim;
        decimal? dovizKuru;
        decimal toplamTutar;
        StokHareketleriM stokHareketleriM;

        [Size(16)]
        // TargetCriteria: bkz. StokHareketleriM.FisNo — aynı çoklu-yeni-kayıt/RuleUniqueValue
        // zamanlama düzeltmesi.
        [RuleUniqueValue(TargetCriteria = "IrsaliyeNo != '" + Helper.ConstNewRecordText + "'")]
        [Indexed(Unique = true)]
        [XafDisplayName("İrsaliye No")]
        [Appearance("ED_IrsaliyeM_IrsaliyeNo", Enabled = false, TargetItems = "IrsaliyeNo", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_IrsaliyeM_IrsaliyeNo", DefaultContexts.Save, "Lütfen İrsaliye Numarasını Giriniz...")]
        public string IrsaliyeNo
        {
            get => irsaliyeNo;
            set => SetPropertyValue(nameof(IrsaliyeNo), ref irsaliyeNo, value);
        }

        [XafDisplayName("İrsaliye Tarihi")]
        [Appearance("ED_IrsaliyeM_IrsaliyeTarihi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_IrsaliyeM_IrsaliyeTarihi", DefaultContexts.Save, "Lütfen İrsaliye Tarihini Giriniz...")]
        public DateTime IrsaliyeTarihi
        {
            get => irsaliyeTarihi;
            set => SetPropertyValue(nameof(IrsaliyeTarihi), ref irsaliyeTarihi, value);
        }

        // Genel ekranda seçilebilir; fiş-türü-özel ekranda (IrsaliyeM_DetailView) Layout'a hiç
        // yerleştirilmez, NewRecordDefaultsViewController tarafından IRALIS'e sabitlenir.
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [XafDisplayName("Fiş Türü")]
        [DataSourceCriteria("FinansModulTipi = 'Irsaliye'")]
        [RuleRequiredField("RuleRequired_IrsaliyeM_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
        public FisTuruTanim FisTuruTanim
        {
            get => fisTuruTanim;
            set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
        }

        [XafDisplayName("Tedarikçi")]
        [Appearance("ED_IrsaliyeM_Tedarikci", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_IrsaliyeM_Tedarikci", DefaultContexts.Save, "Lütfen Tedarikçiyi Seçiniz...")]
        public CariHesapTanim Tedarikci
        {
            get => tedarikci;
            set => SetPropertyValue(nameof(Tedarikci), ref tedarikci, value);
        }

        [XafDisplayName("Kaynak Sipariş")]
        [Appearance("ED_IrsaliyeM_KaynakSiparis", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_IrsaliyeM_KaynakSiparis", DefaultContexts.Save, "Lütfen Kaynak Siparişi Seçiniz...")]
        public SatinAlmaSiparisiM KaynakSiparis
        {
            get => kaynakSiparis;
            set => SetPropertyValue(nameof(KaynakSiparis), ref kaynakSiparis, value);
        }

        // VUK gereği saklanması gereken, TEDARİKÇİNİN KENDİ irsaliyesinin (fiziksel sevkiyatı
        // taşıyan resmi belge) numarası/tarihi — bizim IrsaliyeNo'muzdan (dahili takip no, ADR-014
        // teyidiyle aynı kategori: gerçek resmi/e-Belge numarası DEĞİL) AYRIDIR, karıştırılmamalıdır.
        [Size(32)]
        [XafDisplayName("Tedarikçi İrsaliye No")]
        public string TedarikciIrsaliyeNo
        {
            get => tedarikciIrsaliyeNo;
            set => SetPropertyValue(nameof(TedarikciIrsaliyeNo), ref tedarikciIrsaliyeNo, value);
        }

        [XafDisplayName("Tedarikçi İrsaliye Tarihi")]
        public DateTime? TedarikciIrsaliyeTarihi
        {
            get => tedarikciIrsaliyeTarihi;
            set => SetPropertyValue(nameof(TedarikciIrsaliyeTarihi), ref tedarikciIrsaliyeTarihi, value);
        }

        // Yalnızca İADE türünde (IRALID) dolu — orijinal (iade edilen) İrsaliye'ye referans. Normal
        // İrsaliye'de (IRALIS) hep null. [Indexed(Unique=true)]: bir İrsaliye yalnızca BİR KEZ iade
        // edilebilir (DB seviyesi garanti — FaturaD.KaynakStokHareketiD ile aynı desen; bu turda
        // öğrenilen ders gereği şema güncellemesi sonrası is_unique=1 olduğu SQL ile ayrıca
        // doğrulanmalı, XPO var olan bir index'i sonradan UNIQUE'e çevirmiyor).
        [Indexed(Unique = true)]
        [XafDisplayName("Kaynak İrsaliye (İade)")]
        [Appearance("ED_IrsaliyeM_KaynakIrsaliye", Enabled = false, Context = "Any")]
        // Kendine-referans veren (IrsaliyeM->IrsaliyeM) bir lookup, DevExpress XAF Blazor'da NULL
        // iken editör kontrolünü hiç render ETMİYOR (yalnızca başlık/caption kalıyor, canlı testte
        // kanıtlandı — "birçok detay öğesi ufak ve görünmüyor" kullanıcı bulgusu). Normal (İade
        // olmayan) İrsaliye'de bu alan HER ZAMAN null olduğundan, en pragmatik/güvenli çözüm: null
        // iken alanı tamamen gizlemek (yalnızca gerçek İade belgesinde, dolu haldeyken görünür).
        [Appearance("Hide_IrsaliyeM_KaynakIrsaliye", Visibility = ViewItemVisibility.Hide,
            TargetItems = "KaynakIrsaliye", Criteria = "KaynakIrsaliye is null", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_IrsaliyeM_KaynakIrsaliye", DefaultContexts.Save,
            "İade İrsaliyesinde Kaynak İrsaliye zorunludur.", TargetCriteria = "FisTuruTanim.FisTuruKodu = 'IRALID'")]
        public IrsaliyeM KaynakIrsaliye
        {
            get => kaynakIrsaliye;
            set => SetPropertyValue(nameof(KaynakIrsaliye), ref kaynakIrsaliye, value);
        }

        [XafDisplayName("Depo")]
        [Appearance("ED_IrsaliyeM_DepoTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_IrsaliyeM_DepoTanim", DefaultContexts.Save, "Lütfen Depo Seçiniz...")]
        public DepoTanim DepoTanim
        {
            get => depoTanim;
            set => SetPropertyValue(nameof(DepoTanim), ref depoTanim, value);
        }

        [XafDisplayName("Döviz Kodu")]
        [Appearance("ED_IrsaliyeM_DovizTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_IrsaliyeM_DovizTanim", DefaultContexts.Save, "Lütfen Döviz Kodunu Giriniz...")]
        public DovizTanim DovizTanim
        {
            get => dovizTanim;
            set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
        }

        [XafDisplayName("Döviz Kuru")]
        [Appearance("ED_IrsaliyeM_DovizKuru", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_IrsaliyeM_DovizKuru", DefaultContexts.Save, "Lütfen Bir Döviz Kuru Giriniz...")]
        public decimal? DovizKuru
        {
            get => dovizKuru;
            set => SetPropertyValue(nameof(DovizKuru), ref dovizKuru, value);
        }

        [DbType("decimal(18,2)")]
        [XafDisplayName("Toplam Tutar")]
        [Appearance("ED_IrsaliyeM_ToplamTutar", Enabled = false, Context = "Any")]
        public decimal ToplamTutar
        {
            get => toplamTutar;
            set => SetPropertyValue(nameof(ToplamTutar), ref toplamTutar, value);
        }

        // İrsaliye taslağı oluşturulurken (ISatinAlmaIrsaliyeServisi.IrsaliyeTaslagiOlustur) BİRLİKTE
        // yaratılan, asıl stok/ağırlıklı-ortalama-maliyet etkisini taşıyan STSAGR fişi. Salt-okunur —
        // kullanıcı elle bağlayamaz/değiştiremez.
        [XafDisplayName("Stok Hareket Fişi")]
        [Appearance("ED_IrsaliyeM_StokHareketleriM", Enabled = false, Context = "Any")]
        public StokHareketleriM StokHareketleriM
        {
            get => stokHareketleriM;
            set => SetPropertyValue(nameof(StokHareketleriM), ref stokHareketleriM, value);
        }

        [Association("IrsaliyeM-IrsaliyeDs"), Aggregated]
        [XafDisplayName("İrsaliye Kalemleri")]
        public XPCollection<IrsaliyeD> IrsaliyeDs
        {
            get { return GetCollection<IrsaliyeD>(nameof(IrsaliyeDs)); }
        }

        // CariHesapHareketleri/SatinAlmaSiparisiM'deki OnChanged(DovizTanim) deseninin birebir portu.
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (propertyName == nameof(DovizTanim) && newValue != null && !IsLoading)
                DovizKuruGuncelle((DovizTanim)newValue);
        }

        void DovizKuruGuncelle(DovizTanim doviz)
        {
            if (doviz.DovizKodu == "TRY")
            {
                DovizKuru = 1;
            }
            else
            {
                DovizGunlukKurM entity = Session.FindObject<DovizGunlukKurM>(new BinaryOperator("KurTarihi", IrsaliyeTarihi));
                DovizGunlukKurD entitydetail = entity?.DovizGunlukKurDetails.FirstOrDefault(x => x.DovizTanim == doviz);
                if (entitydetail != null)
                    DovizKuru = entitydetail.DovizSatis;
            }
        }

        protected override void OnSaving()
        {
            if (Session.IsNewObject(This) &&
                IrsaliyeNo == Helper.ConstNewRecordText &&
                FisTuruTanim != null)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                IrsaliyeNo = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, IrsaliyeTarihi);
            }
            base.OnSaving();
        }

        public override void ObjectSaving()
        {
            ToplamTutar = IrsaliyeDs.Sum(x => x.ToplamTutar);
        }
    }
}
