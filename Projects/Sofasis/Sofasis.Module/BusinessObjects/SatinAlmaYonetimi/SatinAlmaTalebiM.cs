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
using System.Linq;
using Sofasis.Module.Extensions;
using Sofasis.Module.Services;

namespace Sofasis.Module.BusinessObjects;

// Başlık ("kim/ne zaman/neden"): Satın Alma sürecinin ilk adımı — bir departman/kullanıcı ihtiyaç
// duyduğu stok/hizmet/masraf kalemlerini talep eder. StokHareketleriM/D master-detail deseninin
// (ADR-016) birebir portu: Master yalnızca başlık + numaralandırma, tüm onay durumu geçişleri
// ISatinAlmaOnayServisi üzerinden yapılır (ADR-006 — iş mantığı servis katmanında).
[DefaultClassOptions]
[DefaultProperty("TalepNo")]
[XafDisplayName("Satın Alma Talebi")]
[RuleCriteria("Rule_SatinAlmaTalebiM_EnAzBirSatir", DefaultContexts.Save,
    "SatinAlmaTalebiDs.Count > 0", "Lütfen en az bir talep kalemi ekleyiniz.")]
[Appearance("SatinAlmaTalebiM_OnayBekliyor", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 1", Context = "ListView", BackColor = "Orange", FontColor = "White", Priority = 1)]
[Appearance("SatinAlmaTalebiM_Onaylandi", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 2", Context = "ListView", BackColor = "Green", FontColor = "White", Priority = 1)]
[Appearance("SatinAlmaTalebiM_Reddedildi", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 3", Context = "ListView", BackColor = "Red", FontColor = "White", Priority = 1)]
[Appearance("SatinAlmaTalebiM_IptalEdildi", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 6", Context = "ListView", BackColor = "Gray", FontColor = "White", Priority = 1)]
[Appearance("SatinAlmaTalebiM_TeklifeCikildi", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 4", Context = "ListView", BackColor = "Blue", FontColor = "White", Priority = 1)]
[Appearance("SatinAlmaTalebiM_SiparisEdildi", AppearanceItemType = "ViewItem", TargetItems = "*",
    Criteria = "Durum = 5", Context = "ListView", BackColor = "Purple", FontColor = "White", Priority = 1)]
[ListViewFilter("Taslaklar", "Durum = 0", isCurrentFilter: true, Index = 0)]
[ListViewFilter("Onay Bekleyenler", "Durum = 1", Index = 1)]
[ListViewFilter("Onaylananlar", "Durum = 2", Index = 2)]
[ListViewFilter("Reddedilenler", "Durum = 3", Index = 3)]
[ListViewFilter("Teklife Çıkılanlar", "Durum = 4", Index = 4)]
[ListViewFilter("Sipariş Edilenler", "Durum = 5", Index = 5)]
[ListViewFilter("İptal Edilenler", "Durum = 6", Index = 6)]
[ListViewFilter("Tüm Kayıtlar", "", "", Index = 7)]
public class SatinAlmaTalebiM : BaseClassWithAuditAndDescription
{
    public SatinAlmaTalebiM(Session session)
        : base(session)
    {
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        TalepTarihi = DateTime.UtcNow.Date;
        IhtiyacTarihi = DateTime.UtcNow.Date;
        TalepNo = Helper.ConstNewRecordText;
        Durum = SatinAlmaTalepDurumu.Taslak;

        TalepEdenKullanici = Session.FindObject<ApplicationUser>(
            CriteriaOperator.Parse("Oid=CurrentUserId()"));

        CriteriaOperator fisTuruCriteria =
            CriteriaOperator.FromLambda<FisTuruTanim>(x => x.ViewName == "SatinAlmaTalebiM_DetailView");
        FisTuruTanim = Session.FindObject<FisTuruTanim>(fisTuruCriteria);
    }

    string talepNo;
    DateTime talepTarihi;
    FisTuruTanim fisTuruTanim;
    ApplicationUser talepEdenKullanici;
    DateTime ihtiyacTarihi;
    string gerekce;
    decimal tahminiToplamTutar;
    SatinAlmaTalepDurumu durum;
    ApplicationUser onaylayanKullanici;
    DateTime? onayTarihi;
    ApplicationUser reddedenKullanici;
    DateTime? redTarihi;
    string redNedeni;

    [Size(16)]
    // TargetCriteria: aynı commit'te birden fazla yeni kayıt oluşursa hepsi hâlâ placeholder
    // metnini taşırken RuleUniqueValue (OnSaving'den ÖNCE, Committing'de) çalışır — yanlış pozitif
    // "benzersiz değil" hatasını önler (bkz. CariHesapTanim.CariHesapKodu).
    [RuleUniqueValue(TargetCriteria = "TalepNo != '" + Helper.ConstNewRecordText + "'")]
    [Indexed(Unique = true)]
    [XafDisplayName("Talep No")]
    [Appearance("ED_SatinAlmaTalebiM_TalepNo", Enabled = false, TargetItems = "TalepNo", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_SatinAlmaTalebiM_TalepNo", DefaultContexts.Save, "Lütfen Talep Numarasını Giriniz...")]
    public string TalepNo
    {
        get => talepNo;
        set => SetPropertyValue(nameof(TalepNo), ref talepNo, value);
    }

    [XafDisplayName("Talep Tarihi")]
    [Appearance("ED_SatinAlmaTalebiM_TalepTarihi", Enabled = false, Criteria = "Durum != 0", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_SatinAlmaTalebiM_TalepTarihi", DefaultContexts.Save, "Lütfen Talep Tarihini Giriniz...")]
    public DateTime TalepTarihi
    {
        get => talepTarihi;
        set => SetPropertyValue(nameof(TalepTarihi), ref talepTarihi, value);
    }

    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    [XafDisplayName("Fiş Türü")]
    [DataSourceCriteria("FinansModulTipi = 'SatinAlma'")]
    [RuleRequiredField("RuleRequired_SatinAlmaTalebiM_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Giriniz...")]
    public FisTuruTanim FisTuruTanim
    {
        get => fisTuruTanim;
        set => SetPropertyValue(nameof(FisTuruTanim), ref fisTuruTanim, value);
    }

    [XafDisplayName("Talep Eden Kullanıcı")]
    [Appearance("ED_SatinAlmaTalebiM_TalepEdenKullanici", Enabled = false, Criteria = "Durum != 0", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_SatinAlmaTalebiM_TalepEdenKullanici", DefaultContexts.Save, "Lütfen Talep Eden Kullanıcıyı Seçiniz...")]
    public ApplicationUser TalepEdenKullanici
    {
        get => talepEdenKullanici;
        set => SetPropertyValue(nameof(TalepEdenKullanici), ref talepEdenKullanici, value);
    }

    [XafDisplayName("İhtiyaç Tarihi")]
    [Appearance("ED_SatinAlmaTalebiM_IhtiyacTarihi", Enabled = false, Criteria = "Durum != 0", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_SatinAlmaTalebiM_IhtiyacTarihi", DefaultContexts.Save, "Lütfen İhtiyaç Tarihini Giriniz...")]
    public DateTime IhtiyacTarihi
    {
        get => ihtiyacTarihi;
        set => SetPropertyValue(nameof(IhtiyacTarihi), ref ihtiyacTarihi, value);
    }

    [Size(500)]
    [XafDisplayName("Gerekçe")]
    [ModelDefault("RowCount", "3")]
    [Appearance("ED_SatinAlmaTalebiM_Gerekce", Enabled = false, Criteria = "Durum != 0", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_SatinAlmaTalebiM_Gerekce", DefaultContexts.Save, "Lütfen Gerekçe Giriniz...")]
    public string Gerekce
    {
        get => gerekce;
        set => SetPropertyValue(nameof(Gerekce), ref gerekce, value);
    }

    [DbType("decimal(18,2)")]
    [XafDisplayName("Tahmini Toplam Tutar")]
    [Appearance("ED_SatinAlmaTalebiM_TahminiToplamTutar", Enabled = false, Context = "Any")]
    public decimal TahminiToplamTutar
    {
        get => tahminiToplamTutar;
        set => SetPropertyValue(nameof(TahminiToplamTutar), ref tahminiToplamTutar, value);
    }

    // Yalnızca ISatinAlmaOnayServisi tarafından değiştirilir — kullanıcı elle düzenleyemez
    // (ADR-006: onay iş mantığı servis katmanında, business object'te değil).
    [XafDisplayName("Durum")]
    [Appearance("ED_SatinAlmaTalebiM_Durum", Enabled = false, Context = "DetailView")]
    public SatinAlmaTalepDurumu Durum
    {
        get => durum;
        set => SetPropertyValue(nameof(Durum), ref durum, value);
    }

    [XafDisplayName("Onaylayan Kullanıcı")]
    [Appearance("ED_SatinAlmaTalebiM_OnaylayanKullanici", Enabled = false, Context = "DetailView")]
    [VisibleInListView(false)]
    public ApplicationUser OnaylayanKullanici
    {
        get => onaylayanKullanici;
        set => SetPropertyValue(nameof(OnaylayanKullanici), ref onaylayanKullanici, value);
    }

    [XafDisplayName("Onay Tarihi")]
    [Appearance("ED_SatinAlmaTalebiM_OnayTarihi", Enabled = false, Context = "DetailView")]
    [VisibleInListView(false)]
    public DateTime? OnayTarihi
    {
        get => onayTarihi;
        set => SetPropertyValue(nameof(OnayTarihi), ref onayTarihi, value);
    }

    [XafDisplayName("Reddeden Kullanıcı")]
    [Appearance("ED_SatinAlmaTalebiM_ReddedenKullanici", Enabled = false, Context = "DetailView")]
    [VisibleInListView(false)]
    public ApplicationUser ReddedenKullanici
    {
        get => reddedenKullanici;
        set => SetPropertyValue(nameof(ReddedenKullanici), ref reddedenKullanici, value);
    }

    [XafDisplayName("Red Tarihi")]
    [Appearance("ED_SatinAlmaTalebiM_RedTarihi", Enabled = false, Context = "DetailView")]
    [VisibleInListView(false)]
    public DateTime? RedTarihi
    {
        get => redTarihi;
        set => SetPropertyValue(nameof(RedTarihi), ref redTarihi, value);
    }

    [Size(500)]
    [XafDisplayName("Red Nedeni")]
    [ModelDefault("RowCount", "3")]
    [Appearance("ED_SatinAlmaTalebiM_RedNedeni", Enabled = false, Context = "DetailView")]
    [VisibleInListView(false)]
    public string RedNedeni
    {
        get => redNedeni;
        set => SetPropertyValue(nameof(RedNedeni), ref redNedeni, value);
    }

    [Association("SatinAlmaTalebiM-SatinAlmaTalebiDs"), DevExpress.Xpo.Aggregated]
    [XafDisplayName("Talep Kalemleri")]
    public XPCollection<SatinAlmaTalebiD> SatinAlmaTalebiDs
    {
        get
        {
            return GetCollection<SatinAlmaTalebiD>(nameof(SatinAlmaTalebiDs));
        }
    }

    // Teklifler, Talep'in aksine bağımsız yönetilen kayıtlardır (kendi List/DetailView'i, kendi Sil
    // kontrolü var) — bu yüzden [Aggregated] DEĞİL: Talep silinirken teklifler cascade silinmez
    // (zaten Durum != Taslak iken Talep silinemiyor, teklif varken pratikte bu senaryo oluşmaz).
    // VisibleInDetailView(false): auto-layout üreticisi bu koleksiyon için KENDİ ayrı sekmesini
    // oluşturmasın diye — Model.DesignedDiffs.xafml'deki açık "Teklifler" sekmesi (Genel'in yanında)
    // bu ayardan bağımsız olarak zaten gösteriyor; aksi halde iki ayrı "Teklifler" sekmesi render olur.
    [VisibleInDetailView(false)]
    [Association("SatinAlmaTalebiM-SatinAlmaTeklifleri")]
    [XafDisplayName("Teklifler")]
    public XPCollection<SatinAlmaTeklifM> SatinAlmaTeklifleri
    {
        get
        {
            return GetCollection<SatinAlmaTeklifM>(nameof(SatinAlmaTeklifleri));
        }
    }

    // SatisSiparisM'deki "satır → master toplam push" deseninin birebir kopyası — satırlar kendi
    // OnSaving/OnDeleting'inden bunu çağırır. deletedObject, silinme sürecindeki satırı (OnDeleting
    // anında koleksiyonda hâlâ görünebildiğinden) toplamdan hariç tutmak için kullanılır.
    public void UpdateTotals(SatinAlmaTalebiD deletedObject)
    {
        TahminiToplamTutar = SatinAlmaTalebiDs
            .Where(x => x != deletedObject)
            .Sum(x => x.TahminiTutar);
    }

    // [Size(500)] yalnızca DB kolon genişliğini ve istemci tarafı HTML maxlength'i belirler,
    // sunucu tarafında bir uzunluk kısıtı UYGULAMAZ — istemci-taraflı sınırı atlayan (yapıştırma,
    // otomasyon) bir istek doğrudan DB katmanına ulaşıp ham SqlException fırlatabilir. Bu yüzden
    // burada açık bir sunucu-taraflı uzunluk kontrolü gerekli (StokTanim.OnDeleting'deki manuel
    // UserFriendlyException deseniyle birebir tutarlı).
    const int GerekceMaxUzunluk = 500;
    const int RedNedeniMaxUzunluk = 500;

    protected override void OnSaving()
    {
        if (Gerekce != null && Gerekce.Length > GerekceMaxUzunluk)
            throw new UserFriendlyException($"Gerekçe en fazla {GerekceMaxUzunluk} karakter olabilir.");
        if (RedNedeni != null && RedNedeni.Length > RedNedeniMaxUzunluk)
            throw new UserFriendlyException($"Red Nedeni en fazla {RedNedeniMaxUzunluk} karakter olabilir.");

        // Boş talep kontrolü class-level [RuleCriteria] ile declarative olarak yapılıyor (bkz. sınıf
        // tanımı) — StokHareketleriM'deki aynı gerekçeyle burada tekrar throw YAPILMAZ.
        if (Session.IsNewObject(This) &&
            TalepNo == Helper.ConstNewRecordText &&
            FisTuruTanim != null)
        {
            INumberSequenceService numberSequenceService = new NumberSequenceService();
            TalepNo = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, TalepTarihi);
        }
        base.OnSaving();
    }
}
