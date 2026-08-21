/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : DovizGunlukKurD.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Günlük döviz kuru satırı (bir tarih + bir döviz için).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace SofasisERP.Module.BusinessObjects;

// [DefaultClassOptions] BİLEREK yok — bu saf bir Detail (D) sınıfı, yalnızca
// DovizGunlukKurM'in Aggregated koleksiyonu içinden düzenlenir, menüde ayrı
// bağımsız bir liste olarak görünmemeli.
[XafDisplayName("Günlük Döviz Kur Girişi")]
public class DovizGunlukKurD : BaseClass
{
    public DovizGunlukKurD(Session session) : base(session) { }

    DovizTanim dovizTanim;
    decimal efektifDovizSatis;
    decimal efektifDovizAlis;
    decimal dovizSatis;
    decimal dovizAlis;
    DovizGunlukKurM dovizGunlukKurMaster;
    DateTime kurTarihi;

    [Indexed("KurTarihi", Unique = true)]
    [XafDisplayName("Döviz Kodu")]
    public DovizTanim DovizTanim
    {
        get => dovizTanim;
        set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
    }

    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    public DateTime KurTarihi
    {
        get => kurTarihi;
        set => SetPropertyValue(nameof(KurTarihi), ref kurTarihi, value);
    }

    [XafDisplayName("Döviz Alış")]
    [ModelDefault("EditMask", "N")]
    [ModelDefault("DisplayFormat", "{0:N}")]
    public decimal DovizAlis
    {
        get => dovizAlis;
        set => SetPropertyValue(nameof(DovizAlis), ref dovizAlis, value);
    }

    [XafDisplayName("Döviz Satış")]
    [ModelDefault("EditMask", "N")]
    [ModelDefault("DisplayFormat", "{0:N}")]
    public decimal DovizSatis
    {
        get => dovizSatis;
        set => SetPropertyValue(nameof(DovizSatis), ref dovizSatis, value);
    }

    [XafDisplayName("Efektif Döviz Alış")]
    [ModelDefault("EditMask", "N")]
    [ModelDefault("DisplayFormat", "{0:N}")]
    public decimal EfektifDovizAlis
    {
        get => efektifDovizAlis;
        set => SetPropertyValue(nameof(EfektifDovizAlis), ref efektifDovizAlis, value);
    }

    [XafDisplayName("Efektif Döviz Satış")]
    [ModelDefault("EditMask", "N")]
    [ModelDefault("DisplayFormat", "{0:N}")]
    public decimal EfektifDovizSatis
    {
        get => efektifDovizSatis;
        set => SetPropertyValue(nameof(EfektifDovizSatis), ref efektifDovizSatis, value);
    }

    [XafDisplayName("Günlük Kur Tanımlama")]
    [Association("DovizGunlukKurMaster-DovizGunlukKurDetails")]
    [VisibleInListView(false)]
    [VisibleInDetailView(false)]
    public DovizGunlukKurM DovizGunlukKurMaster
    {
        get => dovizGunlukKurMaster;
        set => SetPropertyValue(nameof(DovizGunlukKurMaster), ref dovizGunlukKurMaster, value);
    }

    // KurTarihi (Master ile birlikte DovizTanim+KurTarihi benzersiz indeksini oluşturuyor) yalnızca
    // DovizKuruGuncellemeServisi tarafından dolduruluyordu — kullanıcı Master'ın Aggregated
    // koleksiyonundan elle yeni satır eklerse boş (DateTime.MinValue) kalıyor ve aynı dövizi ikinci
    // kez eklemeye çalışınca unique index çakışması oluşuyordu (denetim raporunda bulundu, doğrulandı).
    // Artık her kayıtta Master'dan otomatik senkronize ediliyor.
    protected override void OnSaving()
    {
        if (DovizGunlukKurMaster != null)
            KurTarihi = DovizGunlukKurMaster.KurTarihi;
        base.OnSaving();
    }
}
