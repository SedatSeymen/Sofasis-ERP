/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokHareketleriM.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok hareket fişi başlığı (Faz 2) — eski projeden birebir
 *                    port. Bir stok hareketi, bir Cari hareketinin aksine doğası
 *                    gereği çok kalemlidir (bir mal kabul/sayım/transfer tek
 *                    başlık altında birden fazla stok kalemi taşır) — bu yüzden
 *                    CariHesapHareketleri'nin düz tek-satır şablonu DEĞİL,
 *                    DovizGunlukKurM/D'nin Master/Detail (Aggregated) deseni
 *                    kullanılır. Motor mantığı (Ağırlıklı Ortalama Maliyet,
 *                    bakiye güncelleme) burada DEĞİL, StokHareketleriD'de.
 *
 *                    Cari ayna kaydı BİLEREK YOK — eski projede bu, Fatura
 *                    katmanının işidir (StokHareketleriM saf miktar/maliyet
 *                    defteridir; bir sayım/transfer/fire fişinin Cari'yle hiçbir
 *                    ilgisi yoktur). İrsaliye/Fatura bu projede henüz yok;
 *                    eklendiğinde StokHareketleriD.KaynakBelgeTipi/Oid üzerinden
 *                    (eski projedeki gibi) bu fişe bağlanacaklar.
 * ****************************************************************************
 */

using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using SofasisERP.Module.Services;
using System.ComponentModel;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[DefaultProperty(nameof(FisNo))]
[XafDisplayName("Stok Hareketleri")]
[RuleCriteria("Rule_StokHareketleriM_EnAzBirSatir", DefaultContexts.Save,
    "StokHareketleriDs.Count > 0", "Lütfen en az bir stok kalemi ekleyiniz.")]
public class StokHareketleriM : BaseClassWithAuditAndDescription
{
    public StokHareketleriM(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
        {
            FisTarihi = TurkiyeZamani.Bugun;
            BelgeTarihi = TurkiyeZamani.Bugun;

            DepoTanim varsayilanDepo = Session.FindObject<DepoTanim>(
                DevExpress.Data.Filtering.CriteriaOperator.FromLambda<DepoTanim>(x => x.IsVarsayilan));
            if (varsayilanDepo != null)
                Depo = varsayilanDepo;
        }
    }

    string fisNo;
    DateTime fisTarihi;
    FisTuruTanim fisTuruTanim;
    string belgeNo;
    DateTime belgeTarihi;
    DepoTanim depo;

    [Size(20)]
    [Indexed(Unique = true)]
    [Appearance("ED_StokHareketleriM_FisNo", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Fiş No")]
    public string FisNo
    {
        get => fisNo;
        set => SetPropertyValue(nameof(FisNo), ref fisNo, value);
    }

    [Appearance("ED_StokHareketleriM_FisTarihi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_StokHareketleriM_FisTarihi", DefaultContexts.Save, "Lütfen Fiş Tarihini Giriniz...")]
    [XafDisplayName("Fiş Tarihi")]
    public DateTime FisTarihi
    {
        get => fisTarihi;
        set => SetPropertyValue(nameof(FisTarihi), ref fisTarihi, value);
    }

    // Genel ekranda seçilebilir; fiş-türüne özel varyant ekranlarda Layout'a hiç
    // yerleştirilmez, Controller tarafından arka planda atanır — CariHesapHareketleri
    // ile aynı desen. StokTN/HZMTTN/MSRFTN (kod üretimi) ve STTRNS (StokTransferi'nin
    // kendi fiş türü) burada seçilemez.
    [ImmediatePostData]
    [DataSourceCriteria("FinansModulTipi = 'Stok' And StokHareketYonu != 'Yok'")]
    [RuleRequiredField("RuleRequired_StokHareketleriM_FisTuruTanim", DefaultContexts.Save, "Lütfen Fiş Türünü Seçiniz...")]
    [XafDisplayName("Fiş Türü")]
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

    // Bir fiş TEK depoya karşı işlem yapar (alış/satış/sayım/fire hepsi doğal olarak
    // "bu depo için") — Depo Transferi'nin Kaynak/Hedef çifti bu yüzden ayrı bir BO
    // (StokTransferi) olarak kalır, bu alanı kullanmaz.
    [Appearance("ED_StokHareketleriM_Depo", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_StokHareketleriM_Depo", DefaultContexts.Save, "Lütfen Depo Seçiniz...")]
    [XafDisplayName("Depo")]
    public DepoTanim Depo
    {
        get => depo;
        set => SetPropertyValue(nameof(Depo), ref depo, value);
    }

    [Association("StokHareketleriM-StokHareketleriDs"), Aggregated]
    [XafDisplayName("Stok Kalemleri")]
    public XPCollection<StokHareketleriD> StokHareketleriDs
        => GetCollection<StokHareketleriD>(nameof(StokHareketleriDs));

    protected override void OnSaving()
    {
        // Boş fiş kontrolü class-level [RuleCriteria] ile declarative yapılır (XAF Validation,
        // ObjectSaving/FisNo üretiminden ÖNCE çalışır) — burada tekrar throw EDİLMEZ, aksi halde
        // kullanıcıya declarative kuralın kırmızı hata banner'ı yerine sessiz bir red gösterilir.
        if (Session.IsNewObject(this) && string.IsNullOrEmpty(FisNo) && FisTuruTanim != null)
        {
            INumberSequenceService numberSequenceService = new NumberSequenceService();
            FisNo = numberSequenceService.SonrakiNumara(Session, GetType().FullName, FisTuruTanim, FisTarihi);
        }
        base.OnSaving();
    }
}
