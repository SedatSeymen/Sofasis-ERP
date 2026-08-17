/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokKoduJeneratoru.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : StokTanim.StokKodu üretimi. StokParametre'deki seçime göre
 *                    iki modda çalışır:
 *                    - Otomatik: sade, artan bir sıra numarası (INumberSequenceService).
 *                    - Jenerator: StokAltGrupTanim.StokAltGrupKodu + "." + 4 haneli
 *                      sıra numarası (ör. "150.01.01.0001") — bkz.
 *                      SOFASIS_ERP_MIMARI_TASARIM.md §45.4.
 * ****************************************************************************
 */

using System.Linq;
using DevExpress.Xpo;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.Services;

public interface IStokKoduJeneratoru
{
    string SonrakiStokKodu(Session session, StokAltGrupTanim stokAltGrubu);
}

public sealed class StokKoduJeneratoru : IStokKoduJeneratoru
{
    readonly INumberSequenceService numberSequenceService = new NumberSequenceService();

    public string SonrakiStokKodu(Session session, StokAltGrupTanim stokAltGrubu)
    {
        if (stokAltGrubu == null)
            throw new ArgumentNullException(nameof(stokAltGrubu));

        // Aynı Session'da henüz commit edilmemiş yeni bir StokParametre olabilir
        // (ör. seed script içinde) — önce oradan, sonra veritabanından ara.
        StokParametre parametre = session.GetObjectsToSave().OfType<StokParametre>().FirstOrDefault()
            ?? session.FindObject<StokParametre>(null);

        StokKoduUretimYontemi yontem = parametre?.StokKoduUretimYontemi ?? StokKoduUretimYontemi.Jenerator;

        if (yontem == StokKoduUretimYontemi.Otomatik)
        {
            int siraNo = numberSequenceService.SonrakiSiraNo(session, "StokTanim");
            return siraNo.ToString("D6");
        }

        int altGrupSiraNo = numberSequenceService.SonrakiSiraNo(session, stokAltGrubu.StokAltGrupKodu);
        return $"{stokAltGrubu.StokAltGrupKodu}.{altGrupSiraNo:D4}";
    }
}
