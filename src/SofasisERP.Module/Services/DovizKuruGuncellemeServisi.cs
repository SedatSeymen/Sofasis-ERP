/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : DovizKuruGuncellemeServisi.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Bugüne ait TCMB kuru eksik olan dövizleri tamamlar.
 *                    Eski projede bir WindowController'daydı (kullanıcı girişi
 *                    tetikliyordu — Blazor Server'da her circuit/kullanıcı için
 *                    ayrı ayrı tetiklenirdi, gereksiz tekrar); kullanıcı isteğiyle
 *                    UI'dan bağımsız bir servise taşındı, DovizKuruGuncellemeWorker
 *                    (arka plan, process genelinde TEK, periyodik) tarafından çağrılır.
 * ****************************************************************************
 */

using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.Services;

public interface IDovizKuruGuncellemeServisi
{
    void BugununKuruGerekirseGuncelle(IObjectSpace objectSpace);
}

public sealed class DovizKuruGuncellemeServisi : IDovizKuruGuncellemeServisi
{
    readonly IDovizKuruService dovizKuruService;

    public DovizKuruGuncellemeServisi(IDovizKuruService dovizKuruService)
    {
        this.dovizKuruService = dovizKuruService;
    }

    public void BugununKuruGerekirseGuncelle(IObjectSpace objectSpace)
    {
        DateTime bugun = TurkiyeZamani.Bugun;

        // TRY taban birimdir; TCMB kur listesinde yayınlanmaz, kur girilmez.
        List<DovizTanim> tumDovizler = objectSpace.GetObjects<DovizTanim>()
            .Where(x => x.DovizKodu != "TRY")
            .ToList();

        DovizGunlukKurM master = objectSpace.FindObject<DovizGunlukKurM>(
            CriteriaOperator.FromLambda<DovizGunlukKurM>(x => x.KurTarihi == bugun));

        HashSet<string> mevcutKodlar = master == null
            ? new HashSet<string>()
            : master.DovizGunlukKurDetails.Select(x => x.DovizTanim.DovizKodu).ToHashSet();

        bool eksikVar = tumDovizler.Any(d => !mevcutKodlar.Contains(d.DovizKodu));
        if (!eksikVar)
        {
            return; // Bugüne ait, tanımlı tüm dövizlerin kuru zaten var; TCMB'ye tekrar sorma.
        }

        var kurlar = dovizKuruService.KurlariCek(bugun);
        if (kurlar.Count == 0)
        {
            return; // TCMB'ye ulaşılamadı; sessizce vazgeç, sonraki denemede tekrar denenir.
        }
        Dictionary<string, DovizKuruDto> kurSozluk = kurlar.ToDictionary(x => x.DovizKodu);

        master ??= objectSpace.CreateObject<DovizGunlukKurM>();
        master.KurTarihi = bugun;
        master.KurSaati = TurkiyeZamani.Simdi;

        foreach (DovizTanim dovizTanim in tumDovizler)
        {
            if (mevcutKodlar.Contains(dovizTanim.DovizKodu))
            {
                continue; // Bu döviz için bugünün satırı zaten var.
            }
            if (!kurSozluk.TryGetValue(dovizTanim.DovizKodu, out DovizKuruDto kur))
            {
                continue; // TCMB bugün bu kodu yayınlamıyor — atla.
            }

            var detay = objectSpace.CreateObject<DovizGunlukKurD>();
            detay.DovizGunlukKurMaster = master;
            detay.DovizTanim = dovizTanim;
            detay.KurTarihi = bugun;
            detay.DovizAlis = kur.DovizAlis;
            detay.DovizSatis = kur.DovizSatis;
            detay.EfektifDovizAlis = kur.EfektifAlis;
            detay.EfektifDovizSatis = kur.EfektifSatis;
        }

        objectSpace.CommitChanges();
    }
}
