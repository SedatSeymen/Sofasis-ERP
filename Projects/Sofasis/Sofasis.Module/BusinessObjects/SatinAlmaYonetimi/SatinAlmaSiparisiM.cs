using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using System.Linq;
using Sofasis.Module.Extensions;
using Sofasis.Module.Services;

namespace Sofasis.Module.BusinessObjects;

// Başlık: Talep/Teklif sürecinden sonra tedarikçiye VERİLEN sipariş. StokHareketleriM'deki
// "muhasebe defteri gibi immutable" felsefesiyle birebir tutarlı: kayıt sonrası (ilk save'den
// itibaren) tamamen değiştirilemez (bkz. SatinAlmaSiparisiKilitleController) — tedarikçiye
// gönderilmiş bir belge olduğundan düzeltme, Durum=İptalEdildi + yeni sipariş oluşturmakla
// yapılır, mevcut kaydı değiştirmekle değil.
[DefaultClassOptions]
[DefaultProperty("SiparisNo")]
[XafDisplayName("Satın Alma Siparişi")]
[RuleCriteria("Rule_SatinAlmaSiparisiM_EnAzBirSatir", DefaultContexts.Save,
    "SatinAlmaSiparisiDs.Count > 0", "Lütfen en az bir sipariş kalemi ekleyiniz.")]
[Appearance("SatinAlmaSiparisiM_MalKabulYapildi", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 1", Context = "ListView", BackColor = "Green", FontColor = "White", Priority = 1)]
[Appearance("SatinAlmaSiparisiM_Faturalandi", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 2", Context = "ListView", BackColor = "Blue", FontColor = "White", Priority = 1)]
[Appearance("SatinAlmaSiparisiM_IptalEdildi", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 3", Context = "ListView", BackColor = "Gray", FontColor = "White", Priority = 1)]
// KismiTeslimAlindi (ordinal=4) enum'un SONUNA eklendiği için burada da 4 kullanılır (bkz.
// EnumHelper.cs'deki yorum — ordinal kaymasını önlemek için araya eklenmedi).
[Appearance("SatinAlmaSiparisiM_KismiTeslimAlindi", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 4", Context = "ListView", BackColor = "Orange", FontColor = "White", Priority = 1)]
[ListViewFilter("Verilenler", "Durum = 0", isCurrentFilter: true, Index = 0)]
[ListViewFilter("Kısmi Teslim Alınanlar", "Durum = 4", Index = 1)]
[ListViewFilter("Mal Kabul Yapılanlar", "Durum = 1", Index = 2)]
[ListViewFilter("Faturalananlar", "Durum = 2", Index = 3)]
[ListViewFilter("İptal Edilenler", "Durum = 3", Index = 4)]
[ListViewFilter("Tüm Kayıtlar", "", "", Index = 5)]
public class SatinAlmaSiparisiM : BaseClassWithAuditAndDescription
{
    public SatinAlmaSiparisiM(Session session)
        : base(session)
    {
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        SiparisTarihi = DateTime.UtcNow.Date;
        TerminTarihi = SiparisTarihi.AddDays(7);
        SiparisNo = Helper.ConstNewRecordText;
        Durum = SatinAlmaSiparisDurumu.Verildi;
        DovizKuru = 1;

        CriteriaOperator fisTuruCriteria =
            CriteriaOperator.FromLambda<FisTuruTanim>(x => x.ViewName == "SatinAlmaSiparisiM_DetailView");
        FisTuruTanim = Session.FindObject<FisTuruTanim>(fisTuruCriteria);
    }

    string siparisNo;
    FisTuruTanim fisTuruTanim;
    CariHesapTanim tedarikci;
    SatinAlmaTeklifM kaynakTeklif;
    SatinAlmaTalebiM kaynakTalep;
    string tedarikciSiparisNo;
    DateTime siparisTarihi;
    DateTime terminTarihi;
    SatinAlmaSiparisDurumu durum;
    DovizTanim dovizTanim;
    decimal dovizKuru;
    decimal toplamTutar;

    [Size(16)]
    // TargetCriteria: XAF'ın RuleUniqueValue doğrulaması (Committing) XPO'nun OnSaving'inden
    // (numaralandırmanın gerçekten atandığı an) ÖNCE çalışır — bu yüzden aynı commit'te (ör.
    // ISatinAlmaSiparisServisi'nin bir seferde birden fazla SatinAlmaSiparisiM oluşturması) birden
    // fazla yeni kayıt hâlâ ortak placeholder metnini taşırken "benzersiz değil" diye yanlış
    // pozitif verir (canlı testte doğrulandı). Placeholder'ı doğrulama dışı bırakmak gerçek
    // numaralara uygulanan asıl benzersizlik kontrolünü zayıflatmaz.
    [RuleUniqueValue(TargetCriteria = "SiparisNo != '" + Helper.ConstNewRecordText + "'")]
    [Indexed(Unique = true)]
    [XafDisplayName("Sipariş No")]
    [Appearance("ED_SatinAlmaSiparisiM_SiparisNo", Enabled = false, TargetItems = "SiparisNo", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_SatinAlmaSiparisiM_SiparisNo", DefaultContexts.Save, "Lütfen Sipariş Numarasını Giriniz...")]
    public string SiparisNo
    {
        get => siparisNo;
        set => SetPropertyValue(nameof(SiparisNo), ref siparisNo, value);
    }

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [XafDisplayName("Fiş Türü")]
    [DataSourceCriteria("FinansModulTipi = 'SatinAlma'")]
    [RuleRequiredField("RuleRequired_SatinAlmaSiparisiM_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
    public FisTuruTanim FisTuruTanim
    {
        get => fisTuruTanim;
        set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
    }

    // İsimli enum değeri (ordinal karşılaştırma YOK) — SatinAlmaTeklifM.Tedarikci ile aynı filtre.
    [XafDisplayName("Tedarikçi")]
    [DataSourceCriteria("CariHesapTipi = 'Tedarikci' Or CariHesapTipi = 'MüşteriTedarikci'")]
    [RuleRequiredField("RuleRequired_SatinAlmaSiparisiM_Tedarikci", DefaultContexts.Save, "Lütfen Tedarikçiyi Seçiniz...")]
    public CariHesapTanim Tedarikci
    {
        get => tedarikci;
        set
        {
            SetPropertyValue(nameof(Tedarikci), ref tedarikci, value);
            // SatisSiparisM.CariHesapTanim'daki OnChanged deseninin sadeleştirilmiş hali — Sipariş'te
            // ayrıca KDVTipi/FiyatListesi taşınmaz (Satın Alma tarafında henüz kullanılmıyor).
            if (!IsLoading && tedarikci != null)
            {
                DovizTanim = tedarikci.DovizTanim;
                // Standardizasyon (bu turda kararlaştırıldı): CariHesapHareketleri/StokHareketleriD
                // ile AYNI tam desen — kur artık elle zorunlu değil, güncel kur otomatik çekilir.
                if (DovizTanim != null)
                    DovizKuruGuncelle(DovizTanim);
            }
        }
    }

    // Teklif Toplama atlanıp doğrudan Talep'ten de sipariş oluşturulabildiğinden opsiyonel —
    // yalnızca ISatinAlmaSiparisServisi.TekliflerdenSiparisOlustur() ile dönüştürüldüğünde dolar.
    [XafDisplayName("Kaynak Teklif")]
    public SatinAlmaTeklifM KaynakTeklif
    {
        get => kaynakTeklif;
        set => SetPropertyValue(nameof(KaynakTeklif), ref kaynakTeklif, value);
    }

    // Onay sistemi bu şekilde atlanamaz: yalnızca onaylanmış veya zaten teklife çıkılmış talepler
    // sipariş kaynağı olabilir. "SiparisEdildi" durumundaki bir talep kasıtlı olarak DIŞARIDA
    // bırakılır — aksi halde aynı talepten birden fazla sipariş oluşturulabilir hale gelirdi.
    [XafDisplayName("Kaynak Talep")]
    [DataSourceCriteria("Durum = 'Onaylandi' Or Durum = 'TeklifeCikildi'")]
    [RuleRequiredField("RuleRequired_SatinAlmaSiparisiM_KaynakTalep", DefaultContexts.Save, "Lütfen Kaynak Talebi Seçiniz...")]
    public SatinAlmaTalebiM KaynakTalep
    {
        get => kaynakTalep;
        set => SetPropertyValue(nameof(KaynakTalep), ref kaynakTalep, value);
    }

    [Size(32)]
    [XafDisplayName("Tedarikçi Sipariş No")]
    public string TedarikciSiparisNo
    {
        get => tedarikciSiparisNo;
        set => SetPropertyValue(nameof(TedarikciSiparisNo), ref tedarikciSiparisNo, value);
    }

    [XafDisplayName("Sipariş Tarihi")]
    [RuleRequiredField("RuleRequired_SatinAlmaSiparisiM_SiparisTarihi", DefaultContexts.Save, "Lütfen Sipariş Tarihini Giriniz...")]
    public DateTime SiparisTarihi
    {
        get => siparisTarihi;
        set => SetPropertyValue(nameof(SiparisTarihi), ref siparisTarihi, value);
    }

    [XafDisplayName("Termin Tarihi")]
    [RuleValueComparison("Rule_SatinAlmaSiparisiM_TerminTarihi", DefaultContexts.Save, ValueComparisonType.GreaterThanOrEqual, nameof(SiparisTarihi), "Termin Tarihi, Sipariş Tarihinden küçük olamaz.", ParametersMode.Expression)]
    [RuleRequiredField("RuleRequired_SatinAlmaSiparisiM_TerminTarihi", DefaultContexts.Save, "Lütfen Termin Tarihini Giriniz...")]
    public DateTime TerminTarihi
    {
        get => terminTarihi;
        set => SetPropertyValue(nameof(TerminTarihi), ref terminTarihi, value);
    }

    // Yalnızca ISatinAlmaSiparisServisi / SA-4 Mal Kabul akışı tarafından değiştirilir (ADR-006).
    [XafDisplayName("Durum")]
    [Appearance("ED_SatinAlmaSiparisiM_Durum", Enabled = false, Context = "DetailView")]
    public SatinAlmaSiparisDurumu Durum
    {
        get => durum;
        set => SetPropertyValue(nameof(Durum), ref durum, value);
    }

    [XafDisplayName("Döviz Kodu")]
    [RuleRequiredField("RuleRequired_SatinAlmaSiparisiM_DovizTanim", DefaultContexts.Save, "Lütfen Döviz Kodunu Seçiniz...")]
    public DovizTanim DovizTanim
    {
        get => dovizTanim;
        set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
    }

    [DbType("decimal(18,6)")]
    [XafDisplayName("Döviz Kuru")]
    [ModelDefault("DisplayFormat", "{0:n6}")]
    [ModelDefault("EditMask", "n6")]
    [Appearance("ED_SatinAlmaSiparisiM_DovizKuru", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [RuleValueComparison("Rule_SatinAlmaSiparisiM_DovizKuru_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Lütfen döviz kurunu sıfırdan büyük giriniz.")]
    public decimal DovizKuru
    {
        get => dovizKuru;
        set => SetPropertyValue(nameof(DovizKuru), ref dovizKuru, value);
    }

    [DbType("decimal(18,2)")]
    [XafDisplayName("Toplam Tutar")]
    [Appearance("ED_SatinAlmaSiparisiM_ToplamTutar", Enabled = false, Context = "Any")]
    [ModelDefault("DisplayFormat", "{0:n2}")]
    [ModelDefault("EditMask", "n2")]
    public decimal ToplamTutar
    {
        get => toplamTutar;
        set => SetPropertyValue(nameof(ToplamTutar), ref toplamTutar, value);
    }

    [Association("SatinAlmaSiparisiM-SatinAlmaSiparisiDs"), DevExpress.Xpo.Aggregated]
    [XafDisplayName("Sipariş Kalemleri")]
    public XPCollection<SatinAlmaSiparisiD> SatinAlmaSiparisiDs
    {
        get
        {
            return GetCollection<SatinAlmaSiparisiD>(nameof(SatinAlmaSiparisiDs));
        }
    }

    // SatisSiparisM/SatinAlmaTalebiM'deki "satır → master toplam push" deseninin birebir kopyası.
    public void UpdateTotals(SatinAlmaSiparisiD deletedObject)
    {
        ToplamTutar = SatinAlmaSiparisiDs
            .Where(x => x != deletedObject)
            .Sum(x => x.ToplamTutar);
    }

    // CariHesapHareketleri/StokHareketleriD.OnChanged(DovizTanim)'in birebir portu — Döviz Kodu
    // elle değiştirilirse de (Tedarikçi'den miras alınan varsayılanın kullanıcı tarafından
    // override edilmesi) kur otomatik güncellenir.
    protected override void OnChanged(string propertyName, object oldValue, object newValue)
    {
        base.OnChanged(propertyName, oldValue, newValue);
        if (propertyName == nameof(DovizTanim) && newValue != null && !IsLoading)
            DovizKuruGuncelle((DovizTanim)newValue);
    }

    // CariHesapHareketleri.DovizKuruGuncelle'nin birebir portu — SatinAlmaSiparisiM zaten kendi
    // Master'ı olduğundan (StokHareketleriD'deki gibi ayrı bir Master'a bağımlılık yok) doğrudan
    // kendi SiparisTarihi'ni kullanır.
    void DovizKuruGuncelle(DovizTanim doviz)
    {
        if (doviz.DovizKodu == "TRY")
        {
            DovizKuru = 1;
        }
        else
        {
            DovizGunlukKurM entity = Session.FindObject<DovizGunlukKurM>(new BinaryOperator("KurTarihi", SiparisTarihi));
            DovizGunlukKurD entitydetail = entity?.DovizGunlukKurDetails.FirstOrDefault(x => x.DovizTanim == doviz);
            if (entitydetail != null)
                DovizKuru = entitydetail.DovizSatis;
        }
    }

    protected override void OnSaving()
    {
        bool ilkKayit = Session.IsNewObject(This);

        // Sunucu-taraflı tekrar-sipariş koruması: KaynakTalep alanındaki DataSourceCriteria
        // ("Durum = 'Onaylandi' Or 'TeklifeCikildi'") yalnızca UI lookup'ını filtreler —
        // ISatinAlmaSiparisServisi.TekliflerdenSiparisOlustur() bu lookup'ı hiç kullanmadan,
        // önceden var olan Teklif satırlarından doğrudan Sipariş üretir ve DataSourceCriteria'yı
        // BAYPAS EDER. Canlı testte doğrulandı: aynı Talebin İKİ farklı tedarikçiden gelen rakip
        // teklifi seçilip TEK "Siparişe Dönüştür" çağrısında birlikte dönüştürülünce, aynı Talep'ten
        // 2 ayrı Sipariş oluştu (v1'in "bir Talep → en fazla bir Sipariş" tasarım kararını ihlal
        // ediyor — kısmi teslimat/çoklu-tedarikçi bölüştürme v1 kapsamında YOK).
        //
        // ÖNEMLİ (canlı testte ikinci bir bug olarak bulundu): İlk sürüm bu kontrolü
        // "KaynakTalep.Durum == SiparisEdildi" şeklinde yapıyordu — ama DevExpress'in kendi
        // dokümantasyonunun açıkça uyardığı gibi OnSaving() AYNI nesne için birden fazla kez
        // çağrılabilir. İlk çağrıda mekanik yan etki (aşağıda) KaynakTalep.Durum'u SiparisEdildi'ye
        // çevirdikten SONRA OnSaving ikinci kez tetiklenirse, kontrol KENDİ yaptığı değişikliği
        // "başka bir Sipariş zaten var" sanıp İLK (ve tek) meşru dönüşümü bile reddediyordu
        // (self-match yanlış pozitifi — canlı testte doğrulandı, "Teklif Karşılaştırma → Siparişe
        // Dönüştür" akışını tamamen kullanılamaz hale getirmişti). Düzeltme: Durum bayrağına değil,
        // gerçekten BAŞKA bir Sipariş nesnesinin var olup olmadığına bakılır (`s != this` ile kendisi
        // hariç tutulur) — hem aynı commit içindeki kardeş nesneler (Session.GetObjectsToSave, henüz
        // DB'de değil) hem önceki commit'lerden kalıcı kayıtlar (XPQuery) kontrol edilir.
        if (ilkKayit && KaynakTalep != null)
        {
            bool ayniTalepIcinBaskaSiparisVarMi =
                Session.GetObjectsToSave().OfType<SatinAlmaSiparisiM>().Any(s => s != this && s.KaynakTalep == KaynakTalep) ||
                new XPQuery<SatinAlmaSiparisiM>(Session).Any(s => s.Oid != Oid && s.KaynakTalep == KaynakTalep);

            if (ayniTalepIcinBaskaSiparisVarMi)
                throw new UserFriendlyException($"'{KaynakTalep.TalepNo}' talebinden zaten bir Satın Alma Siparişi oluşturulmuş — aynı talepten ikinci bir sipariş açılamaz.");
        }

        if (ilkKayit && SiparisNo == Helper.ConstNewRecordText && FisTuruTanim != null)
        {
            INumberSequenceService numberSequenceService = new NumberSequenceService();
            SiparisNo = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, SiparisTarihi);
        }

        // Mekanik, her zaman doğru bir yan etki (SatinAlmaTalebiD.OnSaving'in UpdateTotals çağırması
        // gibi) — hangi yoldan oluşturulursa oluşturulsun (doğrudan "Yeni" ile veya
        // ISatinAlmaSiparisServisi ile) bir Sipariş'in Kaynak Talebi otomatik SiparisEdildi'ye geçer;
        // aynı talepten ikinci bir sipariş oluşturulması artık YUKARIDAKİ kontrolle sunucu
        // tarafında da engelleniyor (DataSourceCriteria yalnızca UI'daki ilk savunma katmanı).
        if (ilkKayit && KaynakTalep != null && KaynakTalep.Durum != SatinAlmaTalepDurumu.SiparisEdildi)
            KaynakTalep.Durum = SatinAlmaTalepDurumu.SiparisEdildi;

        base.OnSaving();
    }
}
