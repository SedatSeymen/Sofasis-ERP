/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokTipiTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok kodlama hiyerarşisinin en üst seviyesi (ör. 150=Hammadde).
 *                    Yalnızca muhasebe rolü tarafından yönetilir (bkz.
 *                    SOFASIS_ERP_MIMARI_TASARIM.md §45.4).
 * ****************************************************************************
 */

using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;

namespace SofasisERP.Module.BusinessObjects;

// TODO (Faz 7 — Security System aktifleşince, denetim raporu N8): bu ekran yalnızca
// muhasebe rolüne açık olmalı (plan §5 kararı). Security modülü bu projede henüz aktif
// değil (bkz. project_stokkod_admin_kisiti_faz7 belleği) — şimdiden rol bazlı izin
// eklenemez, Security kurulduğunda StokKoduUretimYontemi ile aynı desende eklenecek.
[DefaultClassOptions]
[NavigationItem(false)]
[DefaultProperty(nameof(StokTipiAdi))]
[XafDisplayName("Stok Tipi Tanımlama")]
public class StokTipiTanim : BaseClassWithAuditAndDescription
{
    public StokTipiTanim(Session session) : base(session) { }

    string stokTipiKodu;
    string stokTipiAdi;
    bool mamulMu;

    [Size(10)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_StokTipiTanim_StokTipiKodu", DefaultContexts.Save, "Lütfen Stok Tipi Kodunu Giriniz...")]
    [XafDisplayName("Stok Tipi Kodu")]
    public string StokTipiKodu
    {
        get => stokTipiKodu;
        set => SetPropertyValue(nameof(StokTipiKodu), ref stokTipiKodu, value);
    }

    [Size(50)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_StokTipiTanim_StokTipiAdi", DefaultContexts.Save, "Lütfen Stok Tipi Adını Giriniz...")]
    [XafDisplayName("Stok Tipi Adı")]
    public string StokTipiAdi
    {
        get => stokTipiAdi;
        set => SetPropertyValue(nameof(StokTipiAdi), ref stokTipiAdi, value);
    }

    // Bu tipteki stok kalemleri satılabilir/üretilebilir MAMUL müdür? (ör. Mamul=Evet,
    // Hammadde/Yarı Mamul=Hayır). StokTanim.Model alanının görünürlüğü buna bağlıdır —
    // sabit bir "kod == 150" gibi kırılgan kritere değil, veriye dayalı bir bayrağa bağlı.
    [XafDisplayName("Mamul mü?")]
    public bool MamulMu
    {
        get => mamulMu;
        set => SetPropertyValue(nameof(MamulMu), ref mamulMu, value);
    }
}
