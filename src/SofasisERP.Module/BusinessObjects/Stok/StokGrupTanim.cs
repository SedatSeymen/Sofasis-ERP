/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokGrupTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok kodlama hiyerarşisinin orta seviyesi (ör. 150.01=Sünger).
 *                    StokGrupKodu, StokTipiTanim'in kodunu da içeren KÜMÜLATİF
 *                    koddur (bkz. SOFASIS_ERP_MIMARI_TASARIM.md §45.4).
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.Linq;

namespace SofasisERP.Module.BusinessObjects;

[DefaultClassOptions]
[DefaultProperty(nameof(StokGrupAdi))]
[XafDisplayName("Stok Grup Tanımlama")]
public class StokGrupTanim : BaseClassWithAuditAndDescription
{
    public StokGrupTanim(Session session) : base(session) { }

    StokTipiTanim stokTipiTanim;
    string stokGrupKodu;
    string stokGrupAdi;

    [RuleRequiredField("RuleRequired_StokGrupTanim_StokTipiTanim", DefaultContexts.Save, "Lütfen Stok Tipini Seçiniz...")]
    [XafDisplayName("Stok Tipi")]
    public StokTipiTanim StokTipiTanim
    {
        get => stokTipiTanim;
        set => SetPropertyValue(nameof(StokTipiTanim), ref stokTipiTanim, value);
    }

    // Kümülatif kod (ör. "150.01") — yalnızca StokTipiTanim.StokTipiKodu ile
    // tutarlılık kontrolü için Appearance ile salt-okunur yapılmaz; muhasebe
    // elle girer (StokTipiTanim seçince otomatik önerilebilir — ileride
    // ViewController ile eklenecek, şimdilik elle giriliyor).
    [Size(20)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_StokGrupTanim_StokGrupKodu", DefaultContexts.Save, "Lütfen Stok Grup Kodunu Giriniz...")]
    [XafDisplayName("Stok Grup Kodu")]
    public string StokGrupKodu
    {
        get => stokGrupKodu;
        set => SetPropertyValue(nameof(StokGrupKodu), ref stokGrupKodu, value);
    }

    [Size(50)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_StokGrupTanim_StokGrupAdi", DefaultContexts.Save, "Lütfen Stok Grup Adını Giriniz...")]
    [XafDisplayName("Stok Grup Adı")]
    public string StokGrupAdi
    {
        get => stokGrupAdi;
        set => SetPropertyValue(nameof(StokGrupAdi), ref stokGrupAdi, value);
    }

    protected override void OnDeleting()
    {
        try
        {
            if (new XPQuery<StokAltGrupTanim>(Session).Count(x => x.StokGrupTanim != null && x.StokGrupTanim == this) > 0)
                throw new UserFriendlyException("Bu stok grubu alt gruplarda kullanılmış, silemezsiniz.");
            base.OnDeleting();
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Tracing.Tracer.LogError(ex);
            throw;
        }
    }
}
