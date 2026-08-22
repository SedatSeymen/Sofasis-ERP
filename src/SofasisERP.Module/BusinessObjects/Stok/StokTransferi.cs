/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokTransferi.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Depo-depo transfer wrapper'ı (Faz 2) — StokHareketleriM'e
 *                    gömülmez, kaydedildiğinde otomatik olarak bir STTRCK (Kaynak
 *                    Depo, Çıkış) ve bir STTRGR (Hedef Depo, Giriş) StokHareketleriM/D
 *                    çifti üretir; ikisi de KaynakBelgeTipi/Oid ile bu nesneye
 *                    eşitlenir (kardeşleri eşleştirmek ve silme-replay'de birbirini
 *                    dışlamak için — bkz. StokHareketleriD.ObjectDeleting). Eski
 *                    projeden birebir port.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using SofasisERP.Module.Services;
using System.Linq;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem(false)]
[XafDisplayName("Stok Transferi")]
public class StokTransferi : BaseClassWithAuditAndDescription
{
    public StokTransferi(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        if (Session.IsNewObject(this))
            TransferTarihi = TurkiyeZamani.Bugun;
    }

    DateTime transferTarihi;
    DepoTanim kaynakDepo;
    DepoTanim hedefDepo;
    StokTanim stokTanim;
    decimal miktar;

    [Appearance("ED_StokTransferi_TransferTarihi", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_StokTransferi_TransferTarihi", DefaultContexts.Save, "Lütfen Transfer Tarihini Giriniz...")]
    [XafDisplayName("Transfer Tarihi")]
    public DateTime TransferTarihi
    {
        get => transferTarihi;
        set => SetPropertyValue(nameof(TransferTarihi), ref transferTarihi, value);
    }

    [Appearance("ED_StokTransferi_KaynakDepo", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_StokTransferi_KaynakDepo", DefaultContexts.Save, "Lütfen Kaynak Depoyu Seçiniz...")]
    [XafDisplayName("Kaynak Depo")]
    public DepoTanim KaynakDepo
    {
        get => kaynakDepo;
        set => SetPropertyValue(nameof(KaynakDepo), ref kaynakDepo, value);
    }

    [Appearance("ED_StokTransferi_HedefDepo", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [RuleRequiredField("RuleRequired_StokTransferi_HedefDepo", DefaultContexts.Save, "Lütfen Hedef Depoyu Seçiniz...")]
    [XafDisplayName("Hedef Depo")]
    public DepoTanim HedefDepo
    {
        get => hedefDepo;
        set => SetPropertyValue(nameof(HedefDepo), ref hedefDepo, value);
    }

    [Appearance("ED_StokTransferi_StokTanim", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [DataSourceCriteria("StokHizmetMasrafTipi = 'Stok'")]
    [RuleRequiredField("RuleRequired_StokTransferi_StokTanim", DefaultContexts.Save, "Lütfen Stok Seçiniz...")]
    [XafDisplayName("Stok")]
    public StokTanim StokTanim
    {
        get => stokTanim;
        set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
    }

    [Appearance("ED_StokTransferi_Miktar", Enabled = false, Criteria = "!(IsNewObject(this))", Context = "DetailView")]
    [RuleValueComparison("Rule_StokTransferi_Miktar_Pozitif", DefaultContexts.Save, ValueComparisonType.GreaterThan, 0,
        CustomMessageTemplate = "Lütfen miktarı sıfırdan büyük giriniz.")]
    [XafDisplayName("Miktar")]
    public decimal Miktar
    {
        get => miktar;
        set => SetPropertyValue(nameof(Miktar), ref miktar, value);
    }

    public override void ObjectSaving()
    {
        if (!Session.IsNewObject(this)) return;

        if (KaynakDepo == null || HedefDepo == null)
            throw new UserFriendlyException("Lütfen Kaynak ve Hedef Depoyu seçiniz.");
        if (KaynakDepo == HedefDepo)
            throw new UserFriendlyException("Kaynak Depo ile Hedef Depo aynı olamaz.");
        if (StokTanim == null)
            throw new UserFriendlyException("Lütfen Stok seçiniz.");
        if (Miktar <= 0)
            throw new UserFriendlyException("Lütfen miktarı sıfırdan büyük giriniz.");

        FisTuruTanim cikisFisTuru = Session.FindObject<FisTuruTanim>(
            CriteriaOperator.FromLambda<FisTuruTanim>(x => x.FisTuruKodu == "STTRCK"));
        FisTuruTanim girisFisTuru = Session.FindObject<FisTuruTanim>(
            CriteriaOperator.FromLambda<FisTuruTanim>(x => x.FisTuruKodu == "STTRGR"));
        if (cikisFisTuru == null || girisFisTuru == null)
            throw new UserFriendlyException("STTRCK/STTRGR fiş türleri tanımlı değil — sistem yöneticinizle irtibata geçin.");

        StokHareketleriM cikisFisi = new StokHareketleriM(Session)
        {
            FisTuruTanim = cikisFisTuru,
            FisTarihi = TransferTarihi,
            Depo = KaynakDepo
        };
        StokHareketleriD cikisSatiri = new StokHareketleriD(Session)
        {
            StokHareketleriM = cikisFisi,
            StokTanim = StokTanim,
            Miktar = Miktar,
            KaynakBelgeTipi = typeof(StokTransferi),
            KaynakBelgeOid = Oid
        };

        StokHareketleriM girisFisi = new StokHareketleriM(Session)
        {
            FisTuruTanim = girisFisTuru,
            FisTarihi = TransferTarihi,
            Depo = HedefDepo
        };
        // 4-ajanlı testte (2026-08-22, yazılım debugger bulgusu) tespit edildi: StokTanim
        // atanınca StokHareketleriD'nin kendi OnChanged'i DovizTanim'i StokTanim'in DÖVİZ
        // CİNSİNE göre ayarlayıp günlük kur arıyordu — TRY olmayan bir stok, o günün kuru
        // girilmemişse basit bir depo-içi transferde bile "Döviz kuru bulunamadı" hatasıyla
        // çöküyordu. Oysa BirimMaliyet zaten TL bazlı OrtalamaMaliyet'e sabitlenir (bkz.
        // aşağıdaki yorum) — transfer gerçek bir alım değil, dövizle hiç ilgisi yok. Çıkış
        // satırının kendi motoru (StokHareketleriD.ObjectSaving) Döviz alanlarını TRY/1'e
        // sabitlediği gibi, Giriş satırı için de burada AÇIKÇA TRY/1 atanır.
        DovizTanim tryDoviz = Session.FindObject<DovizTanim>(
            CriteriaOperator.FromLambda<DovizTanim>(x => x.DovizKodu == "TRY"));
        StokHareketleriD girisSatiri = new StokHareketleriD(Session)
        {
            StokHareketleriM = girisFisi,
            StokTanim = StokTanim,
            Miktar = Miktar,
            // Çıkış satırının motor tarafından o anki OrtalamaMaliyet'e sabitlenmiş birim
            // maliyeti, transferin diğer tarafındaki Giriş satırının maliyeti olur — transfer
            // stok değerini korumalı (ne kâr ne zarar üretmeli).
            BirimMaliyet = cikisSatiri.BirimMaliyet <= 0 ? StokTanim.OrtalamaMaliyet : cikisSatiri.BirimMaliyet,
            DovizTanim = tryDoviz,
            DovizKuru = 1,
            KaynakBelgeTipi = typeof(StokTransferi),
            KaynakBelgeOid = Oid
        };
    }

    public override void ObjectDeleting()
    {
        foreach (StokHareketleriD satir in new XPQuery<StokHareketleriD>(Session)
            .Where(x => x.KaynakBelgeTipi == typeof(StokTransferi) && x.KaynakBelgeOid == Oid)
            .ToList())
        {
            StokHareketleriM fis = satir.StokHareketleriM;
            Session.Delete(fis);
        }
    }
}
