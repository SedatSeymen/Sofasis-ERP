/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariHesapTipiTanim.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Cari Hesap kod hiyerarşisinin en üst seviyesi — StokTipiTanim
 *                    ile BİREBİR aynı desen (bkz. StokTipiTanim.cs). Tekdüzen
 *                    Hesap Planı tarzı kodlar kullanılır (120=Alıcılar,
 *                    220=Hem Alıcı Hem Satıcı, 320=Satıcılar) — gerçek Tekdüzen
 *                    hesap planına bağlı değildir, yalnızca kod jeneratörünün
 *                    üst seviyesi için tanıdık/tutarlı bir numaralandırma sağlar
 *                    (StokTipiTanim'deki 150/151/152 ile aynı gerekçe).
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
// muhasebe rolüne açık olmalı (StokTipiTanim ile aynı desen).
[DefaultClassOptions]
[DefaultProperty(nameof(CariHesapTipiAdi))]
[XafDisplayName("Cari Hesap Tipi Tanımlama")]
public class CariHesapTipiTanim : BaseClassWithAuditAndDescription
{
    public CariHesapTipiTanim(Session session) : base(session) { }

    string cariHesapTipiKodu;
    string cariHesapTipiAdi;
    CariHesapTipi cariHesapSinifi;

    [Size(10)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_CariHesapTipiTanim_CariHesapTipiKodu", DefaultContexts.Save, "Lütfen Cari Hesap Tipi Kodunu Giriniz...")]
    [XafDisplayName("Cari Hesap Tipi Kodu")]
    public string CariHesapTipiKodu
    {
        get => cariHesapTipiKodu;
        set => SetPropertyValue(nameof(CariHesapTipiKodu), ref cariHesapTipiKodu, value);
    }

    [Size(50)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_CariHesapTipiTanim_CariHesapTipiAdi", DefaultContexts.Save, "Lütfen Cari Hesap Tipi Adını Giriniz...")]
    [XafDisplayName("Cari Hesap Tipi Adı")]
    public string CariHesapTipiAdi
    {
        get => cariHesapTipiAdi;
        set => SetPropertyValue(nameof(CariHesapTipiAdi), ref cariHesapTipiAdi, value);
    }

    // Bu tipteki cari hesaplar Alıcı mı/Satıcı mı/İkisi mi davranır (KDV, açılış fişi
    // yön mantığı vb.) — StokTipiTanim.MamulMu ile aynı desen: veri satırına bağlı bir
    // sınıflandırma bayrağı, kod string'inden ("120" vb.) türetilmez.
    [XafDisplayName("Cari Hesap Sınıfı")]
    public CariHesapTipi CariHesapSinifi
    {
        get => cariHesapSinifi;
        set => SetPropertyValue(nameof(CariHesapSinifi), ref cariHesapSinifi, value);
    }

    [XafDisplayName("Cari Hesap Grup Tanımlama")]
    [Association("CariHesapTipiTanim-CariHesapGrupTanims"), Aggregated]
    public XPCollection<CariHesapGrupTanim> CariHesapGrupTanims
    {
        get { return GetCollection<CariHesapGrupTanim>(nameof(CariHesapGrupTanims)); }
    }

    protected override void OnDeleting()
    {
        try
        {
            if (new XPQuery<CariHesapGrupTanim>(Session).Count(x => x.CariHesapTipiTanim != null && x.CariHesapTipiTanim == this) > 0)
                throw new UserFriendlyException("Bu cari hesap tipi gruplarda kullanılmış, silemezsiniz.");
            base.OnDeleting();
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (System.Exception ex)
        {
            Tracing.Tracer.LogError(ex);
            throw;
        }
    }
}
