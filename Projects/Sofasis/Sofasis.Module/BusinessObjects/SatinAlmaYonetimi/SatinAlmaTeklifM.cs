using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.ComponentModel;
using Sofasis.Module.Extensions;
using Sofasis.Module.Services;

namespace Sofasis.Module.BusinessObjects;

// Bir Talebe karşılık bir tedarikçiden alınan teklif. Talep'in aksine kendi onay/kilit akışı yok
// (plan: Kilit yalnızca Talep/Sipariş/Fatura'da) — teklif serbestçe düzenlenebilir, karşılaştırma
// ISatinAlmaTeklifServisi üzerinden çalışır (ADR-006).
[DefaultClassOptions]
[DefaultProperty("TeklifNo")]
[XafDisplayName("Satın Alma Teklifi")]
[RuleCriteria("Rule_SatinAlmaTeklifM_EnAzBirSatir", DefaultContexts.Save,
    "SatinAlmaTeklifDs.Count > 0", "Lütfen en az bir teklif kalemi ekleyiniz.")]
[Appearance("SatinAlmaTeklifM_Secildi", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 2", Context = "ListView", BackColor = "Green", FontColor = "White", Priority = 1)]
[Appearance("SatinAlmaTeklifM_Reddedildi", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 3", Context = "ListView", BackColor = "Red", FontColor = "White", Priority = 1)]
[ListViewFilter("Beklemede/Alındı", "Durum = 0 Or Durum = 1", isCurrentFilter: true, Index = 0)]
[ListViewFilter("Seçilenler", "Durum = 2", Index = 1)]
[ListViewFilter("Reddedilenler", "Durum = 3", Index = 2)]
[ListViewFilter("Tüm Kayıtlar", "", "", Index = 3)]
public class SatinAlmaTeklifM : BaseClassWithAuditAndDescription
{
    public SatinAlmaTeklifM(Session session)
        : base(session)
    {
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        TeklifTarihi = DateTime.UtcNow.Date;
        TeklifNo = Helper.ConstNewRecordText;
        Durum = SatinAlmaTeklifDurumu.Beklemede;

        CriteriaOperator fisTuruCriteria =
            CriteriaOperator.FromLambda<FisTuruTanim>(x => x.ViewName == "SatinAlmaTeklifM_DetailView");
        FisTuruTanim = Session.FindObject<FisTuruTanim>(fisTuruCriteria);
    }

    string teklifNo;
    FisTuruTanim fisTuruTanim;
    SatinAlmaTalebiM kaynakTalep;
    CariHesapTanim tedarikci;
    DateTime teklifTarihi;
    DateTime? gecerlilikTarihi;
    SatinAlmaTeklifDurumu durum;

    [Size(16)]
    // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
    // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış pozitif
    // "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
    [RuleUniqueValue(TargetCriteria = "TeklifNo != '" + Helper.ConstNewRecordText + "'")]
    [Indexed(Unique = true)]
    [XafDisplayName("Teklif No")]
    [Appearance("ED_SatinAlmaTeklifM_TeklifNo", Enabled = false, TargetItems = "TeklifNo", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_SatinAlmaTeklifM_TeklifNo", DefaultContexts.Save, "Lütfen Teklif Numarasını Giriniz...")]
    public string TeklifNo
    {
        get => teklifNo;
        set => SetPropertyValue(nameof(TeklifNo), ref teklifNo, value);
    }

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [XafDisplayName("Fiş Türü")]
    [DataSourceCriteria("FinansModulTipi = 'SatinAlma'")]
    [RuleRequiredField("RuleRequired_SatinAlmaTeklifM_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
    public FisTuruTanim FisTuruTanim
    {
        get => fisTuruTanim;
        set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
    }

    // Yalnızca onaylanmış (veya zaten teklife çıkılmış — ikinci/üçüncü teklif eklenirken) talepler
    // teklif kaynağı olabilir; Taslak/Onay Bekliyor/Reddedildi/İptal Edildi/Sipariş Edildi bir
    // teklife temel oluşturamaz (onay süreci bu şekilde atlanamaz).
    [XafDisplayName("Kaynak Talep")]
    [Association("SatinAlmaTalebiM-SatinAlmaTeklifleri")]
    [DataSourceCriteria("Durum = 'Onaylandi' Or Durum = 'TeklifeCikildi'")]
    [RuleRequiredField("RuleRequired_SatinAlmaTeklifM_KaynakTalep", DefaultContexts.Save, "Lütfen Kaynak Talebi Seçiniz...")]
    public SatinAlmaTalebiM KaynakTalep
    {
        get => kaynakTalep;
        set
        {
            SetPropertyValue(nameof(KaynakTalep), ref kaynakTalep, value);
            // Kullanıcı isteği: Kaynak Talep seçilince o Talebin tüm kalemleri otomatik teklif
            // kalemi olarak ön-doldurulur (tedarikçiye her kalem için ayrı ayrı teklif isteme
            // zahmetini kaldırır); yalnızca kalem listesi hâlâ boşken çalışır — kullanıcı zaten
            // kalem eklemişken Talep değiştirilirse mevcut girdiler SESSİZCE silinmez.
            // SatinAlmaTeklifD.KaynakTalepD setter'ı StokTanim/Miktar'ı talep kaleminden devralır;
            // Miktar burada yalnızca BAŞLANGIÇ önerisidir — tedarikçi talep edilenden farklı
            // (ör. 10 istenirken 20 veya 5) teklif edebilir, alan serbestçe düzenlenebilir kalır.
            if (!IsLoading && kaynakTalep != null && SatinAlmaTeklifDs.Count == 0)
            {
                foreach (SatinAlmaTalebiD talepKalemi in kaynakTalep.SatinAlmaTalebiDs)
                {
                    _ = new SatinAlmaTeklifD(Session)
                    {
                        SatinAlmaTeklifM = this,
                        KaynakTalepD = talepKalemi
                    };
                }
            }
        }
    }

    // İsimli enum değeri (ordinal karşılaştırma YOK) — CariHesapTipi'nin sadece Tedarikçi veya
    // Müşteri+Tedarikçi olanları teklif verebilir.
    [XafDisplayName("Tedarikçi")]
    [DataSourceCriteria("CariHesapTipi = 'Tedarikci' Or CariHesapTipi = 'MüşteriTedarikci'")]
    [RuleRequiredField("RuleRequired_SatinAlmaTeklifM_Tedarikci", DefaultContexts.Save, "Lütfen Tedarikçiyi Seçiniz...")]
    public CariHesapTanim Tedarikci
    {
        get => tedarikci;
        set => SetPropertyValue(nameof(Tedarikci), ref tedarikci, value);
    }

    [XafDisplayName("Teklif Tarihi")]
    [RuleRequiredField("RuleRequired_SatinAlmaTeklifM_TeklifTarihi", DefaultContexts.Save, "Lütfen Teklif Tarihini Giriniz...")]
    public DateTime TeklifTarihi
    {
        get => teklifTarihi;
        set => SetPropertyValue(nameof(TeklifTarihi), ref teklifTarihi, value);
    }

    [XafDisplayName("Geçerlilik Tarihi")]
    public DateTime? GecerlilikTarihi
    {
        get => gecerlilikTarihi;
        set => SetPropertyValue(nameof(GecerlilikTarihi), ref gecerlilikTarihi, value);
    }

    [XafDisplayName("Durum")]
    public SatinAlmaTeklifDurumu Durum
    {
        get => durum;
        set => SetPropertyValue(nameof(Durum), ref durum, value);
    }

    [Association("SatinAlmaTeklifM-SatinAlmaTeklifDs"), DevExpress.Xpo.Aggregated]
    [XafDisplayName("Teklif Kalemleri")]
    public XPCollection<SatinAlmaTeklifD> SatinAlmaTeklifDs
    {
        get
        {
            return GetCollection<SatinAlmaTeklifD>(nameof(SatinAlmaTeklifDs));
        }
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(This) &&
            TeklifNo == Helper.ConstNewRecordText &&
            FisTuruTanim != null)
        {
            INumberSequenceService numberSequenceService = new NumberSequenceService();
            TeklifNo = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, TeklifTarihi);
        }

        // Mekanik, her zaman doğru bir yan etki (SatinAlmaSiparisiM.OnSaving'in KaynakTalep.Durum'u
        // SiparisEdildi'ye çevirmesiyle aynı desen): bir Talep'ten oluşturulan İLK teklif, Talebi
        // Onaylandı'dan TeklifeÇıkıldı'ya geçirir — "bu talep için teklif toplama süreci başladı"
        // sinyali. İkinci/üçüncü rakip teklif (aynı veya farklı tedarikçiden) zaten TeklifeÇıkıldı
        // durumundaki bir Talebe eklenebildiğinden (KaynakTalep DataSourceCriteria'sı buna izin
        // verir) burada koşul yalnızca Onaylandı iken tetiklenir — başka bir durumu asla ezmez.
        if (Session.IsNewObject(This) && KaynakTalep != null && KaynakTalep.Durum == SatinAlmaTalepDurumu.Onaylandi)
            KaynakTalep.Durum = SatinAlmaTalepDurumu.TeklifeCikildi;

        base.OnSaving();
    }
}
