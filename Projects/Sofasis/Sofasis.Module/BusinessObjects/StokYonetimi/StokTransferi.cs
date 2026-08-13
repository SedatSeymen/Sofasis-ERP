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
using Sofasis.Module.Services;

namespace Sofasis.Module.BusinessObjects
{
    [DefaultClassOptions]
    [XafDisplayName("Depo Transferi")]
    public class StokTransferi : BaseClassWithAuditAndDescription
    {
        public StokTransferi(Session session)
            : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            FisTarihi = DateTime.UtcNow.Date;
            BelgeTarihi = DateTime.UtcNow.Date;
            FisNo = Helper.ConstNewRecordText;
            // Tek olası fiş türü — kullanıcıya seçtirilmez, StokHareketleriM.FisTuruTanim'in
            // (StokTanim.FisTuruTanim) numaralandırma-amaçlı-gizli deseniyle aynı.
            FisTuruTanim = Session.FindObject<FisTuruTanim>(
                CriteriaOperator.Parse("FisTuruKodu = ?", "STTRNS"));
        }

        string fisNo;
        DateTime fisTarihi;
        FisTuruTanim fisTuruTanim;
        string belgeNo;
        DateTime belgeTarihi;
        StokTanim stokTanim;
        DepoTanim kaynakDepo;
        DepoTanim hedefDepo;
        decimal miktar;
        bool motorIslendi;

        [Size(16)]
        // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
        // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış
        // pozitif "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
        [RuleUniqueValue(TargetCriteria = "FisNo != '" + Helper.ConstNewRecordText + "'")]
        [Indexed(Unique = true)]
        [XafDisplayName("Fiş No")]
        [Appearance("ED_StokTransferi_FisNo", Enabled = false, TargetItems = "FisNo", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_StokTransferi_FisNo", DefaultContexts.Save, "Lütfen Fiş Numarasını Giriniz...")]
        public string FisNo
        {
            get => fisNo;
            set => SetPropertyValue(nameof(FisNo), ref fisNo, value);
        }

        [XafDisplayName("Fiş Tarihi")]
        [Appearance("ED_StokTransferi_FisTarihi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_StokTransferi_FisTarihi", DefaultContexts.Save, "Lütfen Fiş Tarihini Giriniz...")]
        public DateTime FisTarihi
        {
            get => fisTarihi;
            set => SetPropertyValue(nameof(FisTarihi), ref fisTarihi, value);
        }

        [XafDisplayName("Fiş Türü")]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        [VisibleInLookupListView(false)]
        [VisibleInReports(false)]
        [RuleRequiredField("RuleRequired_StokTransferi_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
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

        [XafDisplayName("Stok")]
        [Appearance("ED_StokTransferi_StokTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_StokTransferi_StokTanim", DefaultContexts.Save, "Lütfen Stok Seçiniz...")]
        public StokTanim StokTanim
        {
            get => stokTanim;
            set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
        }

        [XafDisplayName("Kaynak Depo")]
        [Appearance("ED_StokTransferi_KaynakDepo", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_StokTransferi_KaynakDepo", DefaultContexts.Save, "Lütfen Kaynak Depo Seçiniz...")]
        public DepoTanim KaynakDepo
        {
            get => kaynakDepo;
            set => SetPropertyValue(nameof(KaynakDepo), ref kaynakDepo, value);
        }

        // Kaynak Depo olarak seçilen depo, seçim aşamasında burada listelenmez (Current Object
        // Parameter — @This). Kaydet anındaki KaynakDepo==HedefDepo kontrolü (ObjectSaving) yine de
        // kalır: kullanıcı önce Hedef'i seçip sonra Kaynak'ı aynı depo yaparsa bu filtre onu geriye
        // dönük temizlemez.
        [XafDisplayName("Hedef Depo")]
        [DataSourceCriteria("Oid != '@This.KaynakDepo.Oid'")]
        [Appearance("ED_StokTransferi_HedefDepo", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        [RuleRequiredField("RuleRequired_StokTransferi_HedefDepo", DefaultContexts.Save, "Lütfen Hedef Depo Seçiniz...")]
        [RuleValueComparison("Rule_StokTransferi_KaynakHedefFarkli", DefaultContexts.Save, ValueComparisonType.NotEquals,
            "KaynakDepo", ParametersMode.Expression, CustomMessageTemplate = "Kaynak ve hedef depo aynı olamaz.")]
        public DepoTanim HedefDepo
        {
            get => hedefDepo;
            set => SetPropertyValue(nameof(HedefDepo), ref hedefDepo, value);
        }

        [DbType("decimal(18,4)")]
        [XafDisplayName("Miktar")]
        [Appearance("ED_StokTransferi_Miktar", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
        public decimal Miktar
        {
            get => miktar;
            set => SetPropertyValue(nameof(Miktar), ref miktar, value);
        }

        // bkz. StokHareketleriD.MotorIslendi — aynı gerekçe: bu belge yeni-master-detail popup
        // akışında birden fazla kez flush edilebilir; Session.IsNewObject(this) tek başına
        // yeterli değil (canlı testte doğrulandı: iki StokHareketleriM üretimi ikinci kez
        // tetiklenip StokBakiye'de UNIQUE INDEX ihlaline yol açtı).
        [Browsable(false)]
        [VisibleInDetailView(false)]
        [VisibleInListView(false)]
        public bool MotorIslendi
        {
            get => motorIslendi;
            set => SetPropertyValue(nameof(MotorIslendi), ref motorIslendi, value);
        }

        protected override void OnSaving()
        {
            if (Session.IsNewObject(This) &&
                FisNo == Helper.ConstNewRecordText &&
                FisTuruTanim != null)
            {
                INumberSequenceService numberSequenceService = new NumberSequenceService();
                FisNo = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, FisTarihi);
            }
            base.OnSaving();
        }

        public override void ObjectSaving()
        {
            if (MotorIslendi) return;

            if (KaynakDepo == HedefDepo)
                throw new UserFriendlyException("Kaynak ve hedef depo aynı olamaz.");
            if (Miktar <= 0)
                throw new UserFriendlyException("Lütfen miktarı sıfırdan büyük giriniz.");

            FisTuruTanim cikisFisTuru = Session.FindObject<FisTuruTanim>(CriteriaOperator.Parse("FisTuruKodu = ?", "STTRCK"));
            FisTuruTanim girisFisTuru = Session.FindObject<FisTuruTanim>(CriteriaOperator.Parse("FisTuruKodu = ?", "STTRGR"));

            // Her yön için tam bir StokHareketleriM (tek StokHareketleriD çocuklu) üretilir — transfer
            // çoklu-kalem değildir (YAGNI), o yüzden ayrı bir StokTransferiD eklenmedi. Çıkış önce
            // işlenir (kendi ObjectSaving'i BirimMaliyet'i o anki StokTanim.OrtalamaMaliyet'e zaten
            // sabitler); Giriş için aynı ortalama açıkça geçilir — ortalama Çıkış'ta değişmediğinden
            // (ADR-005) transfer maliyet-nötrdür, hangisinin önce işlendiği sonucu etkilemez.
            // NOT: Master'lar üzerinde bilinçli olarak .Save() ÇAĞRILMAZ — aynı commit döngüsü
            // içinde (Session zaten bu StokTransferi'yi kaydetmek üzere çalışıyorken) ek bir .Save()
            // çağrısı XPO'nun tüm ObjectSpace'i yeniden işlemesine, dolayısıyla bu ObjectSaving()'in
            // (ve çocuklarının motorunun) birden fazla kez tetiklenmesine yol açıyordu — canlı testte
            // StokBakiye'de UNIQUE INDEX ihlali olarak ortaya çıktı. Yeni nesneler zaten aynı Session
            // içinde oldukları için sürüp giden commit onları otomatik olarak kapsar.
            StokHareketleriM cikisBasligi = new StokHareketleriM(Session)
            {
                FisTuruTanim = cikisFisTuru,
                FisTarihi = FisTarihi,
                BelgeNo = BelgeNo,
                BelgeTarihi = BelgeTarihi,
                DepoTanim = KaynakDepo
            };
            new StokHareketleriD(Session)
            {
                StokHareketleriM = cikisBasligi,
                StokTanim = StokTanim,
                Miktar = Miktar,
                KaynakBelgeTipi = typeof(StokTransferi),
                KaynakBelgeOid = Oid
            };

            StokHareketleriM girisBasligi = new StokHareketleriM(Session)
            {
                FisTuruTanim = girisFisTuru,
                FisTarihi = FisTarihi,
                BelgeNo = BelgeNo,
                BelgeTarihi = BelgeTarihi,
                DepoTanim = HedefDepo
            };
            new StokHareketleriD(Session)
            {
                StokHareketleriM = girisBasligi,
                StokTanim = StokTanim,
                Miktar = Miktar,
                BirimMaliyet = StokTanim.OrtalamaMaliyet,
                KaynakBelgeTipi = typeof(StokTransferi),
                KaynakBelgeOid = Oid
            };

            MotorIslendi = true;
        }

        public override void ObjectDeleting()
        {
            var cocukSatirlar = new XPQuery<StokHareketleriD>(Session)
                .Where(x => x.KaynakBelgeTipi == typeof(StokTransferi) && x.KaynakBelgeOid == Oid)
                .ToList();
            foreach (StokHareketleriD satir in cocukSatirlar)
            {
                // Aggregated tek-çocuklu Master silinir — çocuk satır kademeli (cascade) silinir ve
                // kendi OnDeleting/ObjectDeleting'i (replay dahil) bu sırada tetiklenir. Doğrudan
                // silmeye karşı koruma (StokHareketleriD.OnDeleting) bir bayrağa değil, kaynak
                // StokTransferi'nin (biz zaten silinme sürecindeyken) sorgudan kaybolmasına dayanır.
                Session.Delete(satir.StokHareketleriM);
            }
        }
    }
}
