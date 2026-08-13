using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

namespace Sofasis.Module.BusinessObjects;

// Stok x Depo anlık miktar özeti. Yalnızca StokHareketleriD.ObjectSaving() tarafından
// bul-veya-oluşturulur; kullanıcı elle kayıt açamaz/silemez/düzenleyemez (salt-okunur rapor
// ekranı — bkz. Model.DesignedDiffs.xafml). Ağırlıklı ortalama maliyet stok bazında TEK
// tutulduğundan (ADR bkz. docs/01) OrtalamaMaliyet burada kopya bir sütun DEĞİL,
// StokTanim.OrtalamaMaliyet'ten PersistentAlias ile pasif türetilir.
[DefaultClassOptions]
[XafDisplayName("Stok Bakiye")]
public class StokBakiye : BaseClass
{
    public StokBakiye(Session session) : base(session) { }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        IsSystemRecord = true;
    }

    StokTanim stokTanim;
    DepoTanim depoTanim;
    decimal miktar;

    [RuleRequiredField]
    [XafDisplayName("Stok")]
    public StokTanim StokTanim
    {
        get => stokTanim;
        set => SetPropertyValue(nameof(StokTanim), ref stokTanim, value);
    }

    // (StokTanim, DepoTanim) bileşik benzersiz anahtar — FisTuruVarsayilanDegeri'deki
    // [Indexed("FisTuruTanim", Unique=true)] deseniyle aynı: bu alan diğerinin adını taşıyor.
    [Indexed("StokTanim", Unique = true)]
    [RuleRequiredField]
    [XafDisplayName("Depo")]
    public DepoTanim DepoTanim
    {
        get => depoTanim;
        set => SetPropertyValue(nameof(DepoTanim), ref depoTanim, value);
    }

    [DbType("decimal(18,4)")]
    [XafDisplayName("Miktar")]
    public decimal Miktar
    {
        get => miktar;
        set => SetPropertyValue(nameof(Miktar), ref miktar, value);
    }

    [PersistentAlias("StokTanim.OrtalamaMaliyet")]
    [XafDisplayName("Ortalama Maliyet")]
    public decimal OrtalamaMaliyet
    {
        get { return (decimal)EvaluateAlias(nameof(OrtalamaMaliyet)); }
    }

    [PersistentAlias("Miktar * StokTanim.OrtalamaMaliyet")]
    [XafDisplayName("Toplam Değer")]
    public decimal ToplamDeger
    {
        get { return (decimal)EvaluateAlias(nameof(ToplamDeger)); }
    }
}
