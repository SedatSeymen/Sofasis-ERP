using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using Sofasis.Module.Extensions;
using Sofasis.Module.Services;

namespace Sofasis.Module.BusinessObjects
{
    // Başlık ("nerede/ne zaman"): Fiş No/Tarihi/Türü + Depo + belge satırlarının (StokHareketleriD)
    // koleksiyonu. SatisSiparisM/D deseninden esinlenildi (Faz 2'nin ilk taslağı yanlışlıkla
    // CariHesapHareketleri/KasaBankaHareketleri'nin DÜZ tek-satır şablonunu kopyalamıştı — bir
    // stok hareketi, bir kasa tahsilatının aksine, doğası gereği çok kalemlidir: bir alış
    // faturası/irsaliyesi tek başlık altında birden fazla stok kalemi taşır). Motor mantığı
    // (ağırlıklı ortalama, bakiye güncelleme) burada DEĞİL, StokHareketleriD'de — bkz. o dosya.
    [DefaultClassOptions]
    [XafDisplayName("Stok Hareketleri")]
    [RuleCriteria("Rule_StokHareketleriM_EnAzBirSatir", DefaultContexts.Save,
        "StokHareketleriDs.Count > 0", "Lütfen en az bir stok kalemi ekleyiniz.")]
    public class StokHareketleriM : BaseClassWithAuditAndDescription
    {
        public StokHareketleriM(Session session)
            : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            FisTarihi = DateTime.UtcNow.Date;
            BelgeTarihi = DateTime.UtcNow.Date;
            FisNo = Helper.ConstNewRecordText;

            DepoTanim varsayilanDepo = Session.GetVarsayilan<DepoTanim>();
            if (varsayilanDepo != null) DepoTanim = varsayilanDepo;
        }

        string fisNo;
        DateTime fisTarihi;
        FisTuruTanim fisTuruTanim;
        string belgeNo;
        DateTime belgeTarihi;
        DepoTanim depoTanim;

        [Size(16)]
        // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
        // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış
        // pozitif "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
        [RuleUniqueValue(TargetCriteria = "FisNo != '" + Helper.ConstNewRecordText + "'")]
        [Indexed(Unique = true)]
        [XafDisplayName("Fiş No")]
        [Appearance("ED_StokHareketleriM_FisNo", Enabled = false, TargetItems = "FisNo", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_StokHareketleriM_FisNo", DefaultContexts.Save, "Lütfen Fiş Numarasını Giriniz...")]
        public string FisNo
        {
            get => fisNo;
            set => SetPropertyValue(nameof(FisNo), ref fisNo, value);
        }

        [XafDisplayName("Fiş Tarihi")]
        [Appearance("ED_StokHareketleriM_FisTarihi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_StokHareketleriM_FisTarihi", DefaultContexts.Save, "Lütfen Fiş Tarihini Giriniz...")]
        public DateTime FisTarihi
        {
            get => fisTarihi;
            set => SetPropertyValue(nameof(FisTarihi), ref fisTarihi, value);
        }

        // Genel ekranda (StokHareketleriM_ListView/_DetailView) görünür ve seçilebilir; fiş-türüne
        // özel 8 ekranda (StokAcilisFisi_DetailView vb.) Layout'a hiç yerleştirilmez ve
        // NewRecordDefaultsViewController tarafından sabitlenir — Kasa/Cari'deki aynı desen.
        [XafDisplayName("Fiş Türü")]
        [DataSourceCriteria("FinansModulTipi = 'Stok' And StokHareketYonu != 'Yok'")]
        [Appearance("ED_StokHareketleriM_FisTuruTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_StokHareketleriM_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
        public FisTuruTanim FisTuruTanim
        {
            get => fisTuruTanim;
            set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
        }

        [Size(13)]
        [XafDisplayName("Belge No")]
        public string BelgeNo
        {
            get => belgeNo;
            set => SetPropertyValue(nameof(BelgeNo), ref belgeNo, value);
        }

        [XafDisplayName("Belge Tarihi")]
        public DateTime BelgeTarihi
        {
            get => belgeTarihi;
            set => SetPropertyValue(nameof(BelgeTarihi), ref belgeTarihi, value);
        }

        // Bir belge tek bir depoya karşı işlem yapar (alış/satış/üretim/sayım hepsi doğal olarak
        // "bu depo için") — Depo Transferi'nin Kaynak/Hedef çifti bu yüzden StokTransferi'nde ayrı
        // kalıyor, bu alanı KULLANMIYOR.
        [XafDisplayName("Depo")]
        [Appearance("ED_StokHareketleriM_DepoTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_StokHareketleriM_DepoTanim", DefaultContexts.Save, "Lütfen Depo Seçiniz...")]
        public DepoTanim DepoTanim
        {
            get => depoTanim;
            set => SetPropertyValue(nameof(DepoTanim), ref depoTanim, value);
        }

        [Association("StokHareketleriM-StokHareketleriDs"), DevExpress.Xpo.Aggregated]
        [XafDisplayName("Stok Kalemleri")]
        public XPCollection<StokHareketleriD> StokHareketleriDs
        {
            get
            {
                return GetCollection<StokHareketleriD>(nameof(StokHareketleriDs));
            }
        }

        protected override void OnSaving()
        {
            // Boş fiş kontrolü artık class-level [RuleCriteria] ile declarative olarak yapılıyor
            // (bkz. sınıf tanımı) — XAF'ın Validation modülü bunu CommitChanges'ten (ve dolayısıyla
            // bu OnSaving()'den, FisNo üretiminden) ÖNCE kontrol eder; kural ihlal edilirse numara
            // hiç üretilmez. Burada tekrar throw ETMEK, declarative kuralın kullanıcıya gösterdiği
            // kırmızı hata banner'ını atlayıp yerine sessiz/görünmez bir reddi tetikliyordu (canlı
            // testte doğrulandı) — bu yüzden burada YİNE throw YAPILMAZ.
            if (Session.IsNewObject(This) &&
                FisNo == Helper.ConstNewRecordText &&
                FisTuruTanim != null)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                FisNo = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, FisTarihi);
            }
            base.OnSaving();
        }
    }
}
