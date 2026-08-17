/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : DatabaseSeeder.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : CSV tabanlı başlangıç (seed) verisi yükleyici. Saf referans
 *                    listeleri Resources/Seed/*.csv'den okunur (SeedCsvReader).
 *                    Idempotent: tekrar çalışınca mevcut kayıtları çoğaltmaz.
 *                    Döviz/Ülke/Şehir/İlçe/KDV/Birim eski projeden uyarlandı;
 *                    StokTipi bu projeye özel (Tekdüzen 15-Stoklar numaralandırma
 *                    stiliyle). FisTuru/Depo/Banka/Operasyon/Kasa gibi bu projede
 *                    henüz mevcut olmayan sınıflara bağlı seed'ler BİLEREK atlandı,
 *                    ileride ilgili sınıflar eklendiğinde buraya eklenecek.
 * ****************************************************************************
 */

using System.Linq;
using DevExpress.ExpressApp;
using SofasisERP.Module.BusinessObjects;

namespace SofasisERP.Module.DatabaseUpdate;

// ---- CSV satır tipleri (başlıklar CSV ile birebir) ----
public class DovizRow { public string Kod { get; set; } public string Ad { get; set; } public bool Varsayilan { get; set; } public string Sembol { get; set; } }
public class UlkeRow { public string Kod { get; set; } public string Ad { get; set; } public string TelefonKodu { get; set; } public bool Varsayilan { get; set; } }
public class SehirRow { public string Ad { get; set; } public string PlakaKodu { get; set; } }
public class IlceRow { public string Ad { get; set; } public string SehirAdi { get; set; } }
public class KdvRow { public int Oran { get; set; } public bool Varsayilan { get; set; } }
public class AdVarsayilanRow { public string Ad { get; set; } public bool Varsayilan { get; set; } }
public class StokTipiRow { public string Kod { get; set; } public string Ad { get; set; } public bool MamulMu { get; set; } }
public class FisTuruRow { public string Kod { get; set; } public string Ad { get; set; } }

public class DatabaseSeeder
{
    readonly IObjectSpace os;
    ApplicationUser userAdmin;

    public DatabaseSeeder(IObjectSpace objectSpace) => os = objectSpace;

    public void Seed()
    {
        if (os == null) return;
        userAdmin = os.FirstOrDefault<ApplicationUser>(u => u.UserName == "Admin");

        SeedDoviz();
        SeedUlkeVeSehirler();
        SeedIlceler();      // Şehir'e bağlı
        SeedKdv();
        SeedBirim();
        SeedStokTipi();
        SeedFisTuru();
        SeedGenelParametre();
        os.CommitChanges();
    }

    // Denetim: yeni kayda oluşturan kullanıcıyı ata (base auto-audit'i ezmez).
    void SetOlusturan(object entity)
    {
        if (userAdmin != null && entity is BaseClassWithAudit a && a.CreatedBy == null)
            a.CreatedBy = userAdmin;
    }

    void SeedDoviz()
    {
        foreach (var r in SeedCsvReader.Read<DovizRow>("dovizler.csv"))
        {
            var mevcut = os.FirstOrDefault<DovizTanim>(x => x.DovizKodu == r.Kod);
            if (mevcut != null)
            {
                if (string.IsNullOrEmpty(mevcut.Sembol))
                    mevcut.Sembol = r.Sembol;
                continue;
            }
            var e = os.CreateObject<DovizTanim>();
            e.DovizKodu = r.Kod;
            e.DovizAdi = r.Ad;
            e.IsVarsayilan = r.Varsayilan;
            e.Sembol = r.Sembol;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    void SeedUlkeVeSehirler()
    {
        UlkeTanim varsayilanUlke = null;
        foreach (var r in SeedCsvReader.Read<UlkeRow>("ulkeler.csv"))
        {
            var e = os.FirstOrDefault<UlkeTanim>(x => x.UlkeKodu == r.Kod)
                 ?? os.FirstOrDefault<UlkeTanim>(x => x.UlkeAdi == r.Ad);
            if (e == null)
            {
                e = os.CreateObject<UlkeTanim>();
                e.UlkeKodu = r.Kod;
                e.UlkeAdi = r.Ad;
                e.UlkeTelefonKodu = r.TelefonKodu;
                e.IsVarsayilan = r.Varsayilan;
                e.IsSystemRecord = true;
                SetOlusturan(e);
            }
            else
            {
                if (string.IsNullOrEmpty(e.UlkeKodu)) e.UlkeKodu = r.Kod;
                if (string.IsNullOrEmpty(e.UlkeTelefonKodu)) e.UlkeTelefonKodu = r.TelefonKodu;
            }
            if (r.Varsayilan) varsayilanUlke = e;
        }
        os.CommitChanges();

        varsayilanUlke ??= os.FirstOrDefault<UlkeTanim>(x => x.IsVarsayilan);
        foreach (var r in SeedCsvReader.Read<SehirRow>("sehirler.csv"))
        {
            var s = os.FirstOrDefault<SehirTanim>(x => x.SehirAdi == r.Ad);
            if (s == null)
            {
                s = os.CreateObject<SehirTanim>();
                s.SehirAdi = r.Ad;
                s.PlakaKodu = r.PlakaKodu;
                s.UlkeTanim = varsayilanUlke;
                s.IsSystemRecord = true;
                SetOlusturan(s);
            }
            else if (string.IsNullOrEmpty(s.PlakaKodu))
            {
                s.PlakaKodu = r.PlakaKodu;
            }
        }
        os.CommitChanges();
    }

    void SeedIlceler()
    {
        foreach (var r in SeedCsvReader.Read<IlceRow>("ilceler.csv"))
        {
            var sehir = os.FirstOrDefault<SehirTanim>(x => x.SehirAdi == r.SehirAdi);
            if (sehir == null) continue;
            if (os.FirstOrDefault<IlceTanim>(x => x.IlceAdi == r.Ad && x.SehirTanim == sehir) != null) continue;
            var e = os.CreateObject<IlceTanim>();
            e.IlceAdi = r.Ad;
            e.SehirTanim = sehir;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    void SeedKdv()
    {
        foreach (var r in SeedCsvReader.Read<KdvRow>("kdv-oranlari.csv"))
        {
            if (os.FirstOrDefault<KDVTanim>(x => x.KDVOrani == r.Oran) != null) continue;
            var e = os.CreateObject<KDVTanim>();
            e.KDVOrani = r.Oran;
            e.IsVarsayilan = r.Varsayilan;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    void SeedBirim()
    {
        foreach (var r in SeedCsvReader.Read<AdVarsayilanRow>("birimler.csv"))
        {
            if (os.FirstOrDefault<BirimTanim>(x => x.BirimAdi == r.Ad) != null) continue;
            var e = os.CreateObject<BirimTanim>();
            e.BirimAdi = r.Ad;
            e.IsVarsayilan = r.Varsayilan;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    // Tekdüzen Hesap Planı'nın "15-Stoklar" grubu numaralandırma STİLİNDEN esinlenilmiştir
    // (150/151/152/153/157) — gerçek hesap planına bağlı değildir, yalnızca kod jeneratörünün
    // üst seviyesi için tanıdık/tutarlı bir numaralandırma sağlar (bkz. §45.4).
    void SeedStokTipi()
    {
        foreach (var r in SeedCsvReader.Read<StokTipiRow>("stok-tipleri.csv"))
        {
            if (os.FirstOrDefault<StokTipiTanim>(x => x.StokTipiKodu == r.Kod) != null) continue;
            var e = os.CreateObject<StokTipiTanim>();
            e.StokTipiKodu = r.Kod;
            e.StokTipiAdi = r.Ad;
            e.MamulMu = r.MamulMu;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    // StokTanimYeniKayitVarsayilanlariController'ın View.Id'ye göre StokTanim'e
    // atadığı fiş türleri (STOKTN/HZMTTN/MSRFTN) — bkz. NumberSequenceService.SonrakiNumara.
    void SeedFisTuru()
    {
        foreach (var r in SeedCsvReader.Read<FisTuruRow>("fis-turleri.csv"))
        {
            if (os.FirstOrDefault<FisTuruTanim>(x => x.FisTuruKodu == r.Kod) != null) continue;
            var e = os.CreateObject<FisTuruTanim>();
            e.FisTuruKodu = r.Kod;
            e.FisTuruAdi = r.Ad;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    // Tek satırlık singleton (bkz. GenelParametre.AfterConstruction). CSV'ye gerek yok — sabit
    // varsayılanlar (Basamak4/Basamak2/Basamak6) sınıfın kendi alan başlatıcılarından gelir,
    // burada yalnızca kaydın hiç yokken oluşturulması sağlanır (eski projede bu ekran hiç boş
    // gelmiyordu, kullanıcı geri bildirimiyle burada da aynı davranış sağlandı).
    void SeedGenelParametre()
    {
        if (os.GetObjects<GenelParametre>().Any()) return;
        var e = os.CreateObject<GenelParametre>();
        e.IsSystemRecord = true;
        SetOlusturan(e);
        os.CommitChanges();
    }
}
