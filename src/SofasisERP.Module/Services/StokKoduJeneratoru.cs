/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : StokKoduJeneratoru.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Stok kodlama hiyerarşisinin TÜM seviyeleri için kod üretimi.
 *                    StokTanim.StokKodu, StokParametre'deki seçime göre iki modda
 *                    çalışır:
 *                    - Otomatik: eski projeden aynen taşınan fiş-türü-önekli/günlük
 *                      sıfırlanan numara (bkz. NumberSequenceService.SonrakiNumara,
 *                      FisTuruTanim.cs) — ör. "STOKTN-260817001".
 *                    - Jenerator: StokAltGrupTanim.StokAltGrupKodu + "." + 4 haneli
 *                      sıra numarası (ör. "150.01.01.0001") — bkz.
 *                      SOFASIS_ERP_MIMARI_TASARIM.md §45.4.
 *                    StokGrupTanim.StokGrupKodu ve StokAltGrupTanim.StokAltGrupKodu
 *                    ise her zaman üst seviyenin kodu + 2 haneli sıra numarasıyla
 *                    üretilir (ör. "150" -> "150.01" -> "150.01.01") — kullanıcı
 *                    kararı: bu üç seviye de manuel girilmez, otomatik oluşur.
 *                    Hizmet/Masraf kartlarının fiziksel stok hiyerarşisi yoktur,
 *                    StokKoduUretimYontemi'nden BAĞIMSIZ her zaman fiş-türü-önekli
 *                    numarayı kullanır (kendi HZMTTN/MSRFTN fiş türleriyle).
 * ****************************************************************************
 */

using System;
using System.Linq;
using DevExpress.Xpo;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.Services;

public interface IStokKoduJeneratoru
{
    string SonrakiStokKodu(Session session, StokTanim stokTanim);
    string SonrakiStokGrupKodu(Session session, StokTipiTanim stokTipi);
    string SonrakiStokAltGrupKodu(Session session, StokGrupTanim stokGrubu);
}

public sealed class StokKoduJeneratoru : IStokKoduJeneratoru
{
    readonly INumberSequenceService numberSequenceService = new NumberSequenceService();

    public string SonrakiStokKodu(Session session, StokTanim stokTanim)
    {
        if (stokTanim == null)
            throw new ArgumentNullException(nameof(stokTanim));

        // Hizmet/Masraf: fiziksel stok hiyerarşisi yok, StokKoduUretimYontemi'nden
        // bağımsız her zaman fiş-türü-önekli numara kullanılır.
        if (stokTanim.StokHizmetMasrafTipi != StokHizmetMasrafTipi.Stok)
            return FisTuruNumarasi(session, stokTanim);

        // Aynı Session'da henüz commit edilmemiş yeni bir StokParametre olabilir
        // (ör. seed script içinde) — önce oradan, sonra veritabanından ara.
        StokParametre parametre = session.GetObjectsToSave().OfType<StokParametre>().FirstOrDefault()
            ?? session.FindObject<StokParametre>(null);

        StokKoduUretimYontemi yontem = parametre?.StokKoduUretimYontemi ?? StokKoduUretimYontemi.Jenerator;

        if (yontem == StokKoduUretimYontemi.Otomatik)
            return FisTuruNumarasi(session, stokTanim);

        if (stokTanim.StokAltGrubu == null)
            throw new ArgumentException("Jenerator modunda Stok Alt Grubu seçili olmalıdır.", nameof(stokTanim));

        EnsureStokAltGrupKodu(session, stokTanim.StokAltGrubu);

        int altGrupSiraNo = numberSequenceService.SonrakiSiraNo(session, stokTanim.StokAltGrubu.StokAltGrupKodu);
        return $"{stokTanim.StokAltGrubu.StokAltGrupKodu}.{altGrupSiraNo:D4}";
    }

    string FisTuruNumarasi(Session session, StokTanim stokTanim)
    {
        if (stokTanim.FisTuruTanim == null)
            throw new ArgumentException("Fiş Türü ataması yapılmamış (StokTanimYeniKayitVarsayilanlariController kontrol edilmeli).", nameof(stokTanim));

        return numberSequenceService.SonrakiNumara(session, "StokTanim", stokTanim.FisTuruTanim, TurkiyeZamani.Bugun);
    }

    // StokTipiTanim.StokTipiKodu (ör. "150") kullanıcı tarafından anlamlı biçimde
    // elle atanır (150=Hammadde, 152=Mamul vb. — Tekdüzen'in kendisi gibi), bu
    // yüzden en üst seviye jenaratöre dahil DEĞİLDİR. Grup ve Alt Grup ise saf
    // sıra numarasıdır, üst seviyenin kodu + 2 haneli sıra ile üretilir.
    public string SonrakiStokGrupKodu(Session session, StokTipiTanim stokTipi)
    {
        if (stokTipi == null)
            throw new ArgumentNullException(nameof(stokTipi));

        int siraNo = numberSequenceService.SonrakiSiraNo(session, stokTipi.StokTipiKodu);
        return $"{stokTipi.StokTipiKodu}.{siraNo:D2}";
    }

    public string SonrakiStokAltGrupKodu(Session session, StokGrupTanim stokGrubu)
    {
        if (stokGrubu == null)
            throw new ArgumentNullException(nameof(stokGrubu));

        EnsureStokGrupKodu(session, stokGrubu);

        int siraNo = numberSequenceService.SonrakiSiraNo(session, stokGrubu.StokGrupKodu);
        return $"{stokGrubu.StokGrupKodu}.{siraNo:D2}";
    }

    // XPO'da aynı Session.CommitChanges() içinde birlikte kaydedilen birden fazla
    // yeni nesnenin OnSaving() çağrı SIRASI garanti değildir (ör. yeni bir
    // StokGrupTanim ile onun yeni bir StokAltGrupTanim çocuğu aynı commit'te
    // birlikte kaydedilirse, çocuğun OnSaving()'i üst nesnenin OnSaving()'inden
    // ÖNCE çalışabilir). Üst nesnenin kodu o an hâlâ boşsa burada anında/senkron
    // ürettirip nesneye yazıyoruz; üst nesnenin kendi OnSaving()'i daha sonra
    // çalıştığında "kod zaten dolu" göreceği için tekrar üretmez. Bu, sıralamaya
    // güvenmek yerine üretimi kendi kendini onaran hale getirir.
    void EnsureStokGrupKodu(Session session, StokGrupTanim stokGrubu)
    {
        if (!string.IsNullOrEmpty(stokGrubu.StokGrupKodu))
            return;
        if (stokGrubu.StokTipiTanim == null)
            throw new ArgumentException("Stok Grubunun Stok Tipi seçili olmalıdır.", nameof(stokGrubu));

        stokGrubu.StokGrupKodu = SonrakiStokGrupKodu(session, stokGrubu.StokTipiTanim);
    }

    void EnsureStokAltGrupKodu(Session session, StokAltGrupTanim stokAltGrubu)
    {
        if (!string.IsNullOrEmpty(stokAltGrubu.StokAltGrupKodu))
            return;
        if (stokAltGrubu.StokGrupTanim == null)
            throw new ArgumentException("Stok Alt Grubunun Stok Grubu seçili olmalıdır.", nameof(stokAltGrubu));

        stokAltGrubu.StokAltGrupKodu = SonrakiStokAltGrupKodu(session, stokAltGrubu.StokGrupTanim);
    }
}
