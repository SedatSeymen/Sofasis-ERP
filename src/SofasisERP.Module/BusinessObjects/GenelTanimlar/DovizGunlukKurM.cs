/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : DovizGunlukKurM.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Günlük döviz kuru başlığı (tarih başına tek kayıt) — eski
 *                    projeden uyarlandı; Helper.ConstNewRecordText yerine
 *                    yerel bir sabit metin kullanıldı (Helper sınıfı bu
 *                    projede henüz yok).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace SofasisERP.Module.BusinessObjects;

[XafDisplayName("Günlük Döviz Kur Girişi")]
[DefaultClassOptions]
public class DovizGunlukKurM : BaseClassWithAudit
{
    public DovizGunlukKurM(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            KurTarihi = DateTime.UtcNow.Date;
            KurAciklama = "[Yeni Kayıt]";
        }
    }

    DateTime kurSaati;
    DateTime kurTarihi;
    string kurAciklama;

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [RuleUniqueValue]
    [Indexed(Unique = true)]
    [Appearance("ED_DovizGunlukKurMaster_KurAciklama", Enabled = false, Context = "DetailView")]
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
        set => SetPropertyValue(nameof(KurTarihi), ref kurTarihi, value);
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
    [Association("DovizGunlukKurMaster-DovizGunlukKurDetails"), Aggregated]
    public XPCollection<DovizGunlukKurD> DovizGunlukKurDetails
    {
        get { return GetCollection<DovizGunlukKurD>(nameof(DovizGunlukKurDetails)); }
    }

    protected override void OnSaving()
    {
        KurAciklama = KurTarihi.ToShortDateString() + " Tarihli Merkez Bankası Döviz Kurları";
        base.OnSaving();
    }
}
