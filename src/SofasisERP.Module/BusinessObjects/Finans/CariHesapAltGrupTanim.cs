/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariHesapAltGrupTanim.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Cari Hesap kod hiyerarşisinin en alt seviyesi (ör. 120.01.01
 *                    ="Yurtiçi Toptan Alıcılar"). CariHesapAltGrupKodu kümülatif
 *                    koddur — StokAltGrupTanim ile BİREBİR aynı desen (kardeş
 *                    koleksiyonundan sıra okuma dahil, bkz. StokAltGrupTanim.cs
 *                    yorumundaki nested-Session duplicate-key hata analizi).
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

namespace SofasisERP.Module.BusinessObjects;

[DefaultProperty(nameof(CariHesapAltGrupAdi))]
[XafDisplayName("Cari Hesap Alt Grup Tanımlama")]
public class CariHesapAltGrupTanim : BaseClassWithAuditAndDescription
{
    public CariHesapAltGrupTanim(Session session) : base(session) { }

    CariHesapGrupTanim cariHesapGrupTanim;
    string cariHesapAltGrupKodu;
    string cariHesapAltGrupAdi;
    ToptanPerakendeTipi? toptanPerakendeTipi;

    [Association("CariHesapGrupTanim-CariHesapAltGrupTanims")]
    [RuleRequiredField("RuleRequired_CariHesapAltGrupTanim_CariHesapGrupTanim", DefaultContexts.Save, "Lütfen Cari Hesap Grubunu Seçiniz...")]
    [XafDisplayName("Cari Hesap Grubu")]
    public CariHesapGrupTanim CariHesapGrupTanim
    {
        get => cariHesapGrupTanim;
        set => SetPropertyValue(nameof(CariHesapGrupTanim), ref cariHesapGrupTanim, value);
    }

    // Kullanıcı kararı: elle girilmez — CariHesapGrupTanim.CariHesapGrupKodu + 2 haneli
    // sıra numarasıyla OnSaving'de otomatik üretilir (ör. "120.01" -> "120.01.01").
    [Size(30)]
    [Indexed(Unique = true)]
    [Appearance("ED_CariHesapAltGrupTanim_CariHesapAltGrupKodu", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Cari Hesap Alt Grup Kodu")]
    public string CariHesapAltGrupKodu
    {
        get => cariHesapAltGrupKodu;
        set => SetPropertyValue(nameof(CariHesapAltGrupKodu), ref cariHesapAltGrupKodu, value);
    }

    [Size(50)]
    [RuleRequiredField("RuleRequired_CariHesapAltGrupTanim_CariHesapAltGrupAdi", DefaultContexts.Save, "Lütfen Cari Hesap Alt Grup Adını Giriniz...")]
    [XafDisplayName("Cari Hesap Alt Grup Adı")]
    public string CariHesapAltGrupAdi
    {
        get => cariHesapAltGrupAdi;
        set => SetPropertyValue(nameof(CariHesapAltGrupAdi), ref cariHesapAltGrupAdi, value);
    }

    [RuleRequiredField("RuleRequired_CariHesapAltGrupTanim_ToptanPerakendeTipi", DefaultContexts.Save, "Lütfen Toptan/Perakende Seçiniz...")]
    [XafDisplayName("Toptan/Perakende")]
    public ToptanPerakendeTipi? ToptanPerakendeTipi
    {
        get => toptanPerakendeTipi;
        set => SetPropertyValue(nameof(ToptanPerakendeTipi), ref toptanPerakendeTipi, value);
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this) && string.IsNullOrEmpty(CariHesapAltGrupKodu) && CariHesapGrupTanim != null)
        {
            // StokAltGrupTanim'deki nested-Session duplicate-key hatasından kaçınmak için
            // NumberSequenceService YERİNE kardeş koleksiyonu doğrudan okur (bkz.
            // StokAltGrupTanim.cs'deki ayrıntılı açıklama).
            if (string.IsNullOrEmpty(CariHesapGrupTanim.CariHesapGrupKodu))
            {
                ICariHesapKoduJeneratoru jenerator = new CariHesapKoduJeneratoru();
                CariHesapGrupTanim.CariHesapGrupKodu = jenerator.SonrakiCariHesapGrupKodu(Session, CariHesapGrupTanim.CariHesapTipiTanim);
            }

            int sonSiraNo = CariHesapGrupTanim.CariHesapAltGrupTanims
                .Where(x => x != this && !string.IsNullOrEmpty(x.CariHesapAltGrupKodu))
                .Select(x => SonEkSiraNumarasi(x.CariHesapAltGrupKodu))
                .DefaultIfEmpty(0)
                .Max();
            CariHesapAltGrupKodu = $"{CariHesapGrupTanim.CariHesapGrupKodu}.{(sonSiraNo + 1):D2}";
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
            if (new XPQuery<CariHesapTanim>(Session).Count(x => x.CariHesapAltGrubu != null && x.CariHesapAltGrubu == this) > 0)
                throw new UserFriendlyException("Bu cari hesap alt grubu cari hesap tanımlarında kullanılmış, silemezsiniz.");
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
