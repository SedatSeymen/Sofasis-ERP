/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokAltGrupTanim.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok kodlama hiyerarşisinin en alt seviyesi (ör. 150.01.01
 *                    ="30 Dansite Süngerler"). StokAltGrupKodu kümülatif koddur.
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

// TODO (Faz 7 — Security System aktifleşince, denetim raporu N8): saf Aggregated Detail
// olduğundan yalnızca StokGrupTanim'in DetailView'ından düzenlenir — StokGrupTanim
// muhasebe rolüne kısıtlanınca bu da dolaylı olarak kısıtlanmış olur, ayrı bir izin
// gerekmez.
[DefaultProperty(nameof(StokAltGrupAdi))]
[XafDisplayName("Stok Alt Grup Tanımlama")]
public class StokAltGrupTanim : BaseClassWithAuditAndDescription
{
    public StokAltGrupTanim(Session session) : base(session) { }

    StokGrupTanim stokGrupTanim;
    string stokAltGrupKodu;
    string stokAltGrupAdi;

    [Association("StokGrupTanim-StokAltGrupTanims")]
    [RuleRequiredField("RuleRequired_StokAltGrupTanim_StokGrupTanim", DefaultContexts.Save, "Lütfen Stok Grubunu Seçiniz...")]
    [XafDisplayName("Stok Grubu")]
    public StokGrupTanim StokGrupTanim
    {
        get => stokGrupTanim;
        set => SetPropertyValue(nameof(StokGrupTanim), ref stokGrupTanim, value);
    }

    // Kullanıcı kararı: elle girilmez — StokGrupTanim.StokGrupKodu + 2 haneli sıra
    // numarasıyla OnSaving'de otomatik üretilir (ör. "150.01" -> "150.01.01"). Bkz.
    // StokKoduJeneratoru.SonrakiStokAltGrupKodu.
    [Size(30)]
    [Indexed(Unique = true)]
    [Appearance("ED_StokAltGrupTanim_StokAltGrupKodu", Enabled = false, Context = "DetailView")]
    [XafDisplayName("Stok Alt Grup Kodu")]
    public string StokAltGrupKodu
    {
        get => stokAltGrupKodu;
        set => SetPropertyValue(nameof(StokAltGrupKodu), ref stokAltGrupKodu, value);
    }

    [Size(50)]
    [RuleRequiredField("RuleRequired_StokAltGrupTanim_StokAltGrupAdi", DefaultContexts.Save, "Lütfen Stok Alt Grup Adını Giriniz...")]
    [XafDisplayName("Stok Alt Grup Adı")]
    public string StokAltGrupAdi
    {
        get => stokAltGrupAdi;
        set => SetPropertyValue(nameof(StokAltGrupAdi), ref stokAltGrupAdi, value);
    }

    protected override void OnSaving()
    {
        if (Session.IsNewObject(this) && string.IsNullOrEmpty(StokAltGrupKodu) && StokGrupTanim != null)
        {
            // NumberSequenceService/SequenceGenerator YERİNE ModelSetDetay.SiraNo ile
            // kanıtlanmış deseni kullanır: kardeş kayıtlar StokGrupTanim.StokAltGrupTanims
            // koleksiyonundan (in-memory association) okunur. Sebep: aynı StokGrupTanim
            // DetailView'ı kaydedilmeden art arda "Yeni" popup'ıyla birden fazla
            // StokAltGrupTanim eklenince, her popup KENDİ nested ObjectSpace/Session'ında
            // çalışıyor — SequenceGenerator'ın Session-bazlı önbelleği bu nested
            // Session'lar arasında paylaşılmıyor, her popup veritabanından AYNI (henüz
            // commit edilmemiş) sayacı okuyup AYNI kodu üretiyordu (canlıda doğrulanan
            // hata: "duplicate key value violates unique constraint
            // iStokAltGrupKodu_StokAltGrupTanim"). Kardeş koleksiyonunu doğrudan okumak
            // nested Session'lar arasında da doğru çalışır (aynı üst nesneye bağlı
            // Session zinciri commit edilmeden de kardeşi görür).
            if (string.IsNullOrEmpty(StokGrupTanim.StokGrupKodu))
            {
                IStokKoduJeneratoru jenerator = new StokKoduJeneratoru();
                StokGrupTanim.StokGrupKodu = jenerator.SonrakiStokGrupKodu(Session, StokGrupTanim.StokTipiTanim);
            }

            int sonSiraNo = StokGrupTanim.StokAltGrupTanims
                .Where(x => x != this && !string.IsNullOrEmpty(x.StokAltGrupKodu))
                .Select(x => SonEkSiraNumarasi(x.StokAltGrupKodu))
                .DefaultIfEmpty(0)
                .Max();
            StokAltGrupKodu = $"{StokGrupTanim.StokGrupKodu}.{(sonSiraNo + 1):D2}";
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
            if (new XPQuery<StokTanim>(Session).Count(x => x.StokAltGrubu != null && x.StokAltGrubu == this) > 0)
                throw new UserFriendlyException("Bu stok alt grubu stok tanımlarında kullanılmış, silemezsiniz.");
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
