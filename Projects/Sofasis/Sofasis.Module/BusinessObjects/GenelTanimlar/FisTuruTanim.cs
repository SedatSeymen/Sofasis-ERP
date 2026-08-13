using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultProperty("FisTuruAdi")]
    [DefaultClassOptions]
    [XafDisplayName("Fiş Türü Tanımlama")]

    // TargetItems bilinçli olarak "*" değil: Fiş Türü'nün kimlik alanları sistem kaydıyken
    // kilitli kalır, ama "Varsayılan Değerler" alt tablosu (FisTuruVarsayilanDegerleri)
    // KİLİTLENMEZ — aksi halde tüm Fiş Türleri seed'den IsSystemRecord=true geldiği için
    // hiçbir kullanıcı hiçbir zaman UI'dan varsayılan değer ekleyemezdi.
    [Appearance("ED_FisTuruTanim",
        Enabled = false,
        TargetItems = "FisTuruKodu,FisTuruAdi,FinansModulTipi,FinansBorcAlacakTipi",
        Criteria = "IsSystemRecord = true", Context = "DetailView")]
    public class FisTuruTanim : BaseClassWithAudit
    {
        public FisTuruTanim(Session session)
            : base(session)
        {
        }

        string viewName;
        FinansBorcAlacakTipi finansBorcAlacakTipi;
        StokHareketYonu stokHareketYonu;
        FinansModulTipi finansModulTipi;
        string fisTuruAdi;
        string fisTuruKodu;



        [Size(6)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Fiş Türü Kodu")]
        [RuleRequiredField("RuleReguired_FisTuruTanim_FisTuruKodu", DefaultContexts.Save, "Fiş Türü Kodu Giriniz...")]
        public string FisTuruKodu
        {
            get => fisTuruKodu;
            set => SetPropertyValue(nameof(FisTuruKodu), ref fisTuruKodu, value);
        }


        [Size(50)]
        [RuleUniqueValue]
        [Indexed(Unique = true)]
        [XafDisplayName("Fiş Türü Adı")]
        [RuleRequiredField("RuleReguired_FisTuruTanim_FisTuruAdi", DefaultContexts.Save, "Fiş Türü Adını Giriniz...")]

        public string FisTuruAdi
        {
            get => fisTuruAdi;
            set => SetPropertyValue(nameof(FisTuruAdi), ref fisTuruAdi, value);
        }

        [XafDisplayName("Finans Modül Tipi")]
        public FinansModulTipi FinansModulTipi
        {
            get => finansModulTipi;
            set => SetPropertyValue(nameof(FinansModulTipi), ref finansModulTipi, value);
        }

        [XafDisplayName("Borç Alacak Tipi")]
        public FinansBorcAlacakTipi FinansBorcAlacakTipi
        {
            get => finansBorcAlacakTipi;
            set => SetPropertyValue(nameof(FinansBorcAlacakTipi), ref finansBorcAlacakTipi, value);
        }

        // Stok modülü fiş türleri için Borç/Alacak'ın karşılığı: bir hareketin StokBakiye/
        // OrtalamaMaliyet üzerindeki etkisi bu alandan okunur (StokHareketleriD.Miktar her zaman
        // pozitiftir, yön ikinci bir kaynaktan değil buradan gelir).
        [XafDisplayName("Stok Hareket Yönü")]
        public StokHareketYonu StokHareketYonu
        {
            get => stokHareketYonu;
            set => SetPropertyValue(nameof(StokHareketYonu), ref stokHareketYonu, value);
        }

        [Size(150)]
        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [XafDisplayName("Kullanan Form Adı")]
        public string ViewName
        {
            get => viewName;
            set => SetPropertyValue(nameof(ViewName), ref viewName, value);
        }

        [Association("FisTuruTanim-FisTuruVarsayilanDegerleri"), Aggregated]
        [XafDisplayName("Varsayılan Değerler")]
        public XPCollection<FisTuruVarsayilanDegeri> FisTuruVarsayilanDegerleri
        {
            get
            {
                return GetCollection<FisTuruVarsayilanDegeri>(nameof(FisTuruVarsayilanDegerleri));
            }
        }

        protected override void OnSaving()
        {
            base.OnSaving();
        }
    }
}