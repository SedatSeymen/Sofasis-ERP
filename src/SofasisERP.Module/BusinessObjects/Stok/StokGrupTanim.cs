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
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using SofasisERP.Module.Services;
using System.ComponentModel;
using System.Linq;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace SofasisERP.Module.BusinessObjects;

// TODO (Faz 7 — Security System aktifleşince, denetim raporu N8): bu ekran yalnızca
// muhasebe rolüne açık olmalı (plan §5 kararı, StokTipiTanim ile aynı desen).
[DefaultClassOptions]
[NavigationItem(false)]
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

    // Kullanıcı kararı: elle girilmez — StokTipiTanim.StokTipiKodu + 2 haneli sıra
    // numarasıyla OnSaving'de otomatik üretilir (ör. "150" -> "150.01"). Bkz.
    // StokKoduJeneratoru.SonrakiStokGrupKodu.
    [Size(20)]
    [Indexed(Unique = true)]
    [Appearance("ED_StokGrupTanim_StokGrupKodu", Enabled = false, Context = "DetailView")]
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

    [XafDisplayName("Stok Alt Grup Tanımlama")]
    [Association("StokGrupTanim-StokAltGrupTanims"), Aggregated]
    public XPCollection<StokAltGrupTanim> StokAltGrupTanims
    {
        get { return GetCollection<StokAltGrupTanim>(nameof(StokAltGrupTanims)); }
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this) && string.IsNullOrEmpty(StokGrupKodu) && StokTipiTanim != null)
        {
            IStokKoduJeneratoru jenerator = new StokKoduJeneratoru();
            StokGrupKodu = jenerator.SonrakiStokGrupKodu(Session, StokTipiTanim);
        }
        base.OnSaving();
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
