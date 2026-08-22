/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : CariHesapKoduJeneratoru.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Cari Hesap kodlama hiyerarşisinin TÜM seviyeleri için kod
 *                    üretimi — StokKoduJeneratoru ile BİREBİR aynı desen, üç
 *                    seviyeli (CariHesapTipiTanim -> CariHesapGrupTanim ->
 *                    CariHesapAltGrupTanim -> CariHesapTanim.CariHesapKodu).
 *                    StokTanim'deki gibi Otomatik/Jenerator ikili modu VAR —
 *                    CariHesapParametre.CariHesapKoduUretimYontemi'nden okunur
 *                    (kullanıcı kararı 2026-08-22).
 * ****************************************************************************
 */

using System;
using System.Linq;
using DevExpress.Xpo;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.Services;

public interface ICariHesapKoduJeneratoru
{
    string SonrakiCariHesapKodu(Session session, CariHesapTanim cariHesapTanim);
    string SonrakiCariHesapGrupKodu(Session session, CariHesapTipiTanim cariHesapTipi);
    string SonrakiCariHesapAltGrupKodu(Session session, CariHesapGrupTanim cariHesapGrubu);
}

public sealed class CariHesapKoduJeneratoru : ICariHesapKoduJeneratoru
{
    readonly INumberSequenceService numberSequenceService = new NumberSequenceService();

    public string SonrakiCariHesapKodu(Session session, CariHesapTanim cariHesapTanim)
    {
        if (cariHesapTanim == null)
            throw new ArgumentNullException(nameof(cariHesapTanim));

        // Aynı Session'da henüz commit edilmemiş yeni bir CariHesapParametre olabilir
        // (ör. seed script içinde) — önce oradan, sonra veritabanından ara (StokKoduJeneratoru
        // ile aynı desen).
        CariHesapParametre parametre = session.GetObjectsToSave().OfType<CariHesapParametre>().FirstOrDefault()
            ?? session.FindObject<CariHesapParametre>(null);

        CariHesapKoduUretimYontemi yontem = parametre?.CariHesapKoduUretimYontemi ?? CariHesapKoduUretimYontemi.Jenerator;

        if (yontem == CariHesapKoduUretimYontemi.Otomatik)
            return FisTuruNumarasi(session, cariHesapTanim);

        if (cariHesapTanim.CariHesapAltGrubu == null)
            throw new ArgumentException("Jenerator modunda Cari Hesap Alt Grubu seçili olmalıdır.", nameof(cariHesapTanim));

        EnsureCariHesapAltGrupKodu(session, cariHesapTanim.CariHesapAltGrubu);

        int altGrupSiraNo = numberSequenceService.SonrakiSiraNo(session, cariHesapTanim.CariHesapAltGrubu.CariHesapAltGrupKodu);
        return $"{cariHesapTanim.CariHesapAltGrubu.CariHesapAltGrupKodu}.{altGrupSiraNo:D4}";
    }

    string FisTuruNumarasi(Session session, CariHesapTanim cariHesapTanim)
    {
        if (cariHesapTanim.FisTuruTanim == null)
            throw new ArgumentException("Fiş Türü ataması yapılmamış.", nameof(cariHesapTanim));

        return numberSequenceService.SonrakiNumara(session, "CariHesapTanim", cariHesapTanim.FisTuruTanim, TurkiyeZamani.Bugun);
    }

    // CariHesapTipiTanim.CariHesapTipiKodu (Tekdüzen tarzı 120/220/320) kullanıcı
    // tarafından anlamlı biçimde elle atanır, bu yüzden en üst seviye jenaratöre
    // dahil DEĞİLDİR. Grup ve Alt Grup ise saf sıra numarasıdır.
    public string SonrakiCariHesapGrupKodu(Session session, CariHesapTipiTanim cariHesapTipi)
    {
        if (cariHesapTipi == null)
            throw new ArgumentNullException(nameof(cariHesapTipi));

        int siraNo = numberSequenceService.SonrakiSiraNo(session, cariHesapTipi.CariHesapTipiKodu);
        return $"{cariHesapTipi.CariHesapTipiKodu}.{siraNo:D2}";
    }

    public string SonrakiCariHesapAltGrupKodu(Session session, CariHesapGrupTanim cariHesapGrubu)
    {
        if (cariHesapGrubu == null)
            throw new ArgumentNullException(nameof(cariHesapGrubu));

        EnsureCariHesapGrupKodu(session, cariHesapGrubu);

        int siraNo = numberSequenceService.SonrakiSiraNo(session, cariHesapGrubu.CariHesapGrupKodu);
        return $"{cariHesapGrubu.CariHesapGrupKodu}.{siraNo:D2}";
    }

    // StokKoduJeneratoru'ndaki aynı "kendi kendini onaran" desen: aynı Session.CommitChanges()
    // içinde birlikte kaydedilen üst/alt nesnelerin OnSaving() sırası garanti değildir.
    void EnsureCariHesapGrupKodu(Session session, CariHesapGrupTanim cariHesapGrubu)
    {
        if (!string.IsNullOrEmpty(cariHesapGrubu.CariHesapGrupKodu))
            return;
        if (cariHesapGrubu.CariHesapTipiTanim == null)
            throw new ArgumentException("Cari Hesap Grubunun Cari Hesap Tipi seçili olmalıdır.", nameof(cariHesapGrubu));

        cariHesapGrubu.CariHesapGrupKodu = SonrakiCariHesapGrupKodu(session, cariHesapGrubu.CariHesapTipiTanim);
    }

    void EnsureCariHesapAltGrupKodu(Session session, CariHesapAltGrupTanim cariHesapAltGrubu)
    {
        if (!string.IsNullOrEmpty(cariHesapAltGrubu.CariHesapAltGrupKodu))
            return;
        if (cariHesapAltGrubu.CariHesapGrupTanim == null)
            throw new ArgumentException("Cari Hesap Alt Grubunun Cari Hesap Grubu seçili olmalıdır.", nameof(cariHesapAltGrubu));

        cariHesapAltGrubu.CariHesapAltGrupKodu = SonrakiCariHesapAltGrupKodu(session, cariHesapAltGrubu.CariHesapGrupTanim);
    }
}
