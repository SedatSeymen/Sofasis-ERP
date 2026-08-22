/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariHesapGrupTanim.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Cari Hesap kod hiyerarşisinin orta seviyesi (ör. 120.01=Yurtiçi
 *                    Alıcılar). CariHesapGrupKodu, CariHesapTipiTanim'in kodunu da
 *                    içeren KÜMÜLATİF koddur — StokGrupTanim ile BİREBİR aynı desen.
 * ****************************************************************************
 */

using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.ComponentModel;
using System.Linq;
using AggregatedAttribute = DevExpress.Xpo.AggregatedAttribute;

namespace SofasisERP.Module.BusinessObjects;

// TODO (Faz 7 — Security System aktifleşince, denetim raporu N8): bu ekran yalnızca
// muhasebe rolüne açık olmalı (StokGrupTanim ile aynı desen).
[DefaultClassOptions]
[DefaultProperty(nameof(CariHesapGrupAdi))]
[XafDisplayName("Cari Hesap Grup Tanımlama")]
public class CariHesapGrupTanim : BaseClassWithAuditAndDescription
{
    public CariHesapGrupTanim(Session session) : base(session) { }

    CariHesapTipiTanim cariHesapTipiTanim;
    string cariHesapGrupKodu;
    string cariHesapGrupAdi;
    YurtIciYurtDisiTipi? yurtIciYurtDisiTipi;

    [Association("CariHesapTipiTanim-CariHesapGrupTanims")]
    [RuleRequiredField("RuleRequired_CariHesapGrupTanim_CariHesapTipiTanim", DefaultContexts.Save, "Lütfen Cari Hesap Tipini Seçiniz...")]
    [XafDisplayName("Cari Hesap Tipi")]
    public CariHesapTipiTanim CariHesapTipiTanim
    {
        get => cariHesapTipiTanim;
        set => SetPropertyValue(nameof(CariHesapTipiTanim), ref cariHesapTipiTanim, value);
    }

    // Kullanıcı kararı: elle girilmez — CariHesapTipiTanim.CariHesapTipiKodu + 2 haneli
    // sıra numarasıyla OnSaving'de otomatik üretilir (ör. "120" -> "120.01"). Bkz.
    // CariHesapKoduJeneratoru.SonrakiCariHesapGrupKodu.
    [Size(20)]
    [Indexed(Unique = true)]
    [Appearance("ED_CariHesapGrupTanim_CariHesapGrupKodu", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Cari Hesap Grup Kodu")]
    public string CariHesapGrupKodu
    {
        get => cariHesapGrupKodu;
        set => SetPropertyValue(nameof(CariHesapGrupKodu), ref cariHesapGrupKodu, value);
    }

    [Size(50)]
    [Indexed(Unique = true)]
    [RuleRequiredField("RuleRequired_CariHesapGrupTanim_CariHesapGrupAdi", DefaultContexts.Save, "Lütfen Cari Hesap Grup Adını Giriniz...")]
    [XafDisplayName("Cari Hesap Grup Adı")]
    public string CariHesapGrupAdi
    {
        get => cariHesapGrupAdi;
        set => SetPropertyValue(nameof(CariHesapGrupAdi), ref cariHesapGrupAdi, value);
    }

    [RuleRequiredField("RuleRequired_CariHesapGrupTanim_YurtIciYurtDisiTipi", DefaultContexts.Save, "Lütfen Yurtiçi/Yurtdışı Seçiniz...")]
    [XafDisplayName("Yurtiçi/Yurtdışı")]
    public YurtIciYurtDisiTipi? YurtIciYurtDisiTipi
    {
        get => yurtIciYurtDisiTipi;
        set => SetPropertyValue(nameof(YurtIciYurtDisiTipi), ref yurtIciYurtDisiTipi, value);
    }

    [XafDisplayName("Cari Hesap Alt Grup Tanımlama")]
    [Association("CariHesapGrupTanim-CariHesapAltGrupTanims"), Aggregated]
    public XPCollection<CariHesapAltGrupTanim> CariHesapAltGrupTanims
    {
        get { return GetCollection<CariHesapAltGrupTanim>(nameof(CariHesapAltGrupTanims)); }
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this) && string.IsNullOrEmpty(CariHesapGrupKodu) && CariHesapTipiTanim != null)
        {
            // 4-ajanlı testte (2026-08-22) bulunan hata: NumberSequenceService, aynı
            // CariHesapTipiTanim'in Aggregated koleksiyonunda (DetailView'da) art arda
            // "Yeni" ile eklenen kardeşler arasında paylaşılmıyor (her popup kendi nested
            // Session'ında) — bu da aynı kodun iki kez üretilmesine (unique index ihlali)
            // yol açabiliyordu. CariHesapAltGrupTanim'de zaten uygulanan çözüm: kardeş
            // koleksiyonu NumberSequenceService YERİNE doğrudan oku (bkz. StokAltGrupTanim.cs
            // ayrıntılı açıklama).
            int sonSiraNo = CariHesapTipiTanim.CariHesapGrupTanims
                .Where(x => x != this && !string.IsNullOrEmpty(x.CariHesapGrupKodu))
                .Select(x => SonEkSiraNumarasi(x.CariHesapGrupKodu))
                .DefaultIfEmpty(0)
                .Max();
            CariHesapGrupKodu = $"{CariHesapTipiTanim.CariHesapTipiKodu}.{(sonSiraNo + 1):D2}";
        }
        base.OnSaving();
    }

    static int SonEkSiraNumarasi(string kod)
    {
        string sonSegment = kod.Substring(kod.LastIndexOf('.') + 1);
        return int.TryParse(sonSegment, out int n) ? n : 0;
    }

    protected override void OnDeleting()
    {
        try
        {
            if (new XPQuery<CariHesapAltGrupTanim>(Session).Count(x => x.CariHesapGrupTanim != null && x.CariHesapGrupTanim == this) > 0)
                throw new UserFriendlyException("Bu cari hesap grubu alt gruplarda kullanılmış, silemezsiniz.");
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
