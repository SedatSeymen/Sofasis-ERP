/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : Hesap.cs
 * Oluşturma Tarihi : 08/19/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/20/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Kasa/Banka/Cari'nin ortak temeli — gerçek Tekdüzen Hesap Planı
 *                    mantığında Kasa/Banka/Alıcılar-Satıcılar hepsi birer "Hesap"tır.
 *                    XPO'nun Class Table Inheritance'ı (özel bir attribute gerekmez,
 *                    C# kalıtımı yeterli) ile KasaTanim/BankaHesabiTanim/CariHesapTanim
 *                    bu sınıftan türer.
 *
 *                    BaseClassWithAuditAndDescription'dan türer (CariHesapTanim
 *                    zaten öyleydi) — Kasa/Banka da bu vesileyle "Özel Kodlar &
 *                    Açıklama" sekmesine kavuşuyor, ekran standardı tutarlılığı.
 *
 *                    Kullanıcı kararıyla (2026-08-20) Kasa/Banka/Cari/Çek-Senet
 *                    hareket fişi motoru (CariKasaBankaHareketM/D) tamamen
 *                    kaldırılmış, sonra YENİ (master-detail olmayan, düz)
 *                    KasaCariBankaHareketleri motoruyla geri getirilmişti.
 *                    HesapTuru ayırt edicisi bu motorun Kasa/Banka/Cari'ye özel
 *                    Hareketler ekranlarının DataSourceCriteria filtrelemesi için
 *                    YENİDEN eklendi (bkz. KasaCariBankaHareketleri.cs).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(HesapAdi))]
public abstract class Hesap : BaseClassWithAuditAndDescription
{
    protected Hesap(Session session) : base(session) { }

    string hesapAdi;
    DovizTanim dovizTanim;
    bool isVarsayilan;
    decimal guncelBakiye;
    HesapTuru hesapTuru;

    [Size(SizeAttribute.DefaultStringMappingFieldSize)]
    [XafDisplayName("Hesap Adı")]
    [RuleRequiredField("RuleRequired_Hesap_HesapAdi", DefaultContexts.Save, "Lütfen Hesap Adını Giriniz...")]
    public string HesapAdi
    {
        get => hesapAdi;
        set => SetPropertyValue(nameof(HesapAdi), ref hesapAdi, value);
    }

    [RuleRequiredField("RuleRequired_Hesap_DovizTanim", DefaultContexts.Save, "Lütfen Döviz Cinsini Seçiniz...")]
    [XafDisplayName("Döviz Cinsi")]
    public DovizTanim DovizTanim
    {
        get => dovizTanim;
        set => SetPropertyValue(nameof(DovizTanim), ref dovizTanim, value);
    }

    [XafDisplayName("Varsayılan mı ?")]
    public bool IsVarsayilan
    {
        get => isVarsayilan;
        set => SetPropertyValue(nameof(IsVarsayilan), ref isVarsayilan, value);
    }

    // NOT: HesapAdi burada kilitlenmez (Enabled=false) — Kasa/Cari'de kullanıcı doğrudan
    // yazar, yalnızca BankaHesabiTanim (otomatik "{BankaAdi}-{ŞubeAdi}-{HesapNo}" hesabı)
    // kendi sınıfında ayrı bir Appearance kuralıyla kilitler.

    // KasaCariBankaHareketleri.ObjectSaving/ObjectDeleting tarafından güncellenir —
    // kullanıcı tarafından elle değiştirilmez.
    [Appearance("ED_Hesap_GuncelBakiye", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Güncel Bakiye (TL)")]
    public decimal GuncelBakiye
    {
        get => guncelBakiye;
        set => SetPropertyValue(nameof(GuncelBakiye), ref guncelBakiye, value);
    }

    // Alt sınıf AfterConstruction'ında SABİT atanır, kullanıcı tarafından değiştirilmez —
    // yalnızca Kasa/Banka/Cari'ye özel Hareketler ekranlarının KaynakHesap/KarsiHesap
    // DataSourceCriteria filtrelemesi için kullanılır.
    [Browsable(false)]
    [VisibleInDetailView(false)]
    [VisibleInListView(false)]
    public HesapTuru HesapTuru
    {
        get => hesapTuru;
        set => SetPropertyValue(nameof(HesapTuru), ref hesapTuru, value);
    }
}
