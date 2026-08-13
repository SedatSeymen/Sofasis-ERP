using DevExpress.ExpressApp;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;
using System;
using System.Linq;

namespace Sofasis.Module.Services;

public sealed class SatinAlmaOnayServisi : ISatinAlmaOnayServisi
{
    public void Gonder(SatinAlmaTalebiM talep)
    {
        if (talep.Durum != SatinAlmaTalepDurumu.Taslak)
            throw new UserFriendlyException("Yalnızca taslak durumundaki bir talep onaya gönderilebilir.");

        SatinAlmaParametre parametre = GetParametre(talep.Session);
        if (parametre != null && !parametre.OnayGerekliMi)
        {
            // Onay akışı parametreden kapatılmış (küçük şirket senaryosu, bkz. plan SA-6) —
            // OnayBekliyor adımı atlanır. OnaylayanKullanici boş bırakılır: kimse elle onaylamadı,
            // yalnızca OnayTarihi doluluğu bu talebin otomatik onaylandığını işaret eder.
            talep.Durum = SatinAlmaTalepDurumu.Onaylandi;
            talep.OnayTarihi = DateTime.Now;
            return;
        }

        talep.Durum = SatinAlmaTalepDurumu.OnayBekliyor;
    }

    public void Onayla(SatinAlmaTalebiM talep, ApplicationUser onaylayan)
    {
        if (talep.Durum != SatinAlmaTalepDurumu.OnayBekliyor)
            throw new UserFriendlyException("Yalnızca onay bekleyen bir talep onaylanabilir.");

        // Kendi talebini onaylayamama kontrolü (segregation of duties) — SatinAlmaParametre
        // üzerinden kapatılabilir; parametre kaydı yoksa varsayılan olarak açık kabul edilir.
        SatinAlmaParametre parametre = GetParametre(talep.Session);
        bool kendiTalebiniOnaylayamaz = parametre?.KendiTalebiniOnaylayamaz ?? true;
        if (kendiTalebiniOnaylayamaz && talep.TalepEdenKullanici != null && talep.TalepEdenKullanici.Oid == onaylayan.Oid)
            throw new UserFriendlyException("Kendi talebinizi onaylayamazsınız.");

        talep.Durum = SatinAlmaTalepDurumu.Onaylandi;
        talep.OnaylayanKullanici = onaylayan;
        talep.OnayTarihi = DateTime.Now;
    }

    public void Reddet(SatinAlmaTalebiM talep, ApplicationUser reddeden, string neden)
    {
        if (talep.Durum != SatinAlmaTalepDurumu.OnayBekliyor)
            throw new UserFriendlyException("Yalnızca onay bekleyen bir talep reddedilebilir.");
        if (string.IsNullOrWhiteSpace(neden))
            throw new UserFriendlyException("Lütfen red nedenini giriniz.");

        talep.Durum = SatinAlmaTalepDurumu.Reddedildi;
        talep.ReddedenKullanici = reddeden;
        talep.RedTarihi = DateTime.Now;
        talep.RedNedeni = neden;
    }

    public void TaslagaDondur(SatinAlmaTalebiM talep)
    {
        if (talep.Durum != SatinAlmaTalepDurumu.Reddedildi)
            throw new UserFriendlyException("Yalnızca reddedilmiş bir talep taslağa döndürülebilir.");

        // Red bilgileri temizlenir ki talep, yeniden onaya gönderildiğinde önceki red kaydı
        // ekranda yanlışlıkla hâlâ geçerliymiş gibi görünmesin (SatinAlmaTalebiOnayController'daki
        // "Onay/Red Bilgisi" sekmesi Durum'dan bağımsız olarak bu alanları gösterir).
        talep.Durum = SatinAlmaTalepDurumu.Taslak;
        talep.ReddedenKullanici = null;
        talep.RedTarihi = null;
        talep.RedNedeni = null;
    }

    static SatinAlmaParametre GetParametre(Session session) =>
        new XPCollection<SatinAlmaParametre>(session).FirstOrDefault();
}
