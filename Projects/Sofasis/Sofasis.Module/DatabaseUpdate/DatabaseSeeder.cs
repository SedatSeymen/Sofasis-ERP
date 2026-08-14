/* ****************************************************************************
 * Proje     : Sofasis Erp Project
 * Dosya     : DatabaseSeeder.cs
 * Açıklama  : CSV tabanlı başlangıç (seed) verisi yükleyici. Saf referans
 *             listeleri Resources/Seed/*.csv'den okunur (SeedCsvReader);
 *             ilişkili/tekil/kod-üretilen kayıtlar burada kod ile üretilir.
 *             Idempotent: tekrar çalışınca mevcut kayıtları çoğaltmaz.
 *             Denetim: CreatedBy = Admin (varsa).
 * ****************************************************************************
 */

using System.Drawing;
using DevExpress.ExpressApp;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Services;

namespace Sofasis.Module.DatabaseUpdate;

// ---- CSV satır tipleri (başlıklar CSV ile birebir) ----
public class DovizRow { public string Kod { get; set; } public string Ad { get; set; } public bool Varsayilan { get; set; } public string Sembol { get; set; } }
public class AdVarsayilanRow { public string Ad { get; set; } public bool Varsayilan { get; set; } }
public class AdRow { public string Ad { get; set; } }
public class BankaRow { public string Ad { get; set; } public string SwiftKodu { get; set; } }
public class UlkeRow { public string Kod { get; set; } public string Ad { get; set; } public string TelefonKodu { get; set; } public bool Varsayilan { get; set; } }
public class SehirRow { public string Ad { get; set; } public string PlakaKodu { get; set; } }
public class IlceRow { public string Ad { get; set; } public string SehirAdi { get; set; } }
public class KdvRow { public decimal Oran { get; set; } public bool Varsayilan { get; set; } }
public class StokGrupRow { public string Ad { get; set; } public bool SistemKaydi { get; set; } }
public class OperasyonRow { public int SiraNo { get; set; } public string Ad { get; set; } }
public class FisTuruRow
{
    public string Kod { get; set; }
    public string Ad { get; set; }
    public FinansModulTipi FinansModulTipi { get; set; }
    public FinansBorcAlacakTipi FinansBorcAlacakTipi { get; set; }
    public string ViewName { get; set; }
    public StokHareketYonu StokHareketYonu { get; set; }
}
public class FisTuruVarsayilanDegeriRow
{
    public string FisTuruKodu { get; set; }
    public string HedefTipAdi { get; set; }
    public string VarsayilanDegerKodu { get; set; }
    public bool Aktif { get; set; }
}

public class DatabaseSeeder
{
    readonly IObjectSpace os;
    readonly INumberSequenceService numberSequenceService = new NumberSequenceService();
    ApplicationUser userAdmin;

    public DatabaseSeeder(IObjectSpace objectSpace) => os = objectSpace;

    public void Seed()
    {
        if (os == null) return;
        userAdmin = os.FirstOrDefault<ApplicationUser>(u => u.UserName == "Admin");

        SeedFisTurleri();          // önce: kasa/üretim kod üretimi buna bağlı
        SeedDoviz();
        SeedUlkeVeSehirler();
        SeedIlceler();             // Şehir'e bağlı
        SeedKdv();
        SeedBirim();
        SeedFisTuruVarsayilanDegerleri(); // FişTürü + Kdv/Doviz/Birim'e bağlı
        SeedDepo();                // Ülke'ye bağlı
        SeedStokGrup();
        SeedStokParametre();       // StokGrup'a bağlı
        SeedBanka();
        SeedKasa();                // Döviz + FişTürü'ne bağlı
        SeedOdemeSekli();
        SeedMaliyetParametre();
        SeedUretimMerkezi();
        SeedOperasyon();
        SeedRota();                // Operasyon'a bağlı
        SeedSatinAlmaParametre();
        SeedSatisSiparisParametre();
        os.CommitChanges();
    }

    // Denetim: yeni kayda oluşturan kullanıcıyı ata (base auto-audit'i ezmez).
    void SetOlusturan(object entity)
    {
        if (userAdmin != null && entity is BaseClassWithAudit a && a.CreatedBy == null)
            a.CreatedBy = userAdmin;
    }

    void SeedFisTurleri()
    {
        foreach (var r in SeedCsvReader.Read<FisTuruRow>("fis-turleri.csv"))
        {
            var e = os.FirstOrDefault<FisTuruTanim>(x => x.FisTuruKodu == r.Kod);
            if (e == null)
            {
                e = os.CreateObject<FisTuruTanim>();
                e.FisTuruKodu = r.Kod;
                e.FisTuruAdi = r.Ad;
                e.FinansModulTipi = r.FinansModulTipi;
                e.FinansBorcAlacakTipi = r.FinansBorcAlacakTipi;
                e.ViewName = r.ViewName;
                e.StokHareketYonu = r.StokHareketYonu;
                e.IsSystemRecord = true;
                SetOlusturan(e);
            }
            else
            {
                // ViewName, Borç/Alacak yönü ve modül tipi CSV ile güncel tutulur (mevzuat/muhasebe
                // düzeltmesi sonradan CSV'ye yansıtılırsa var olan kayıtlara da işlensin diye — daha
                // önce FinansModulTipi bu listede unutulmuştu, IRALIS/IRALID'in ADR-017 düzeltmesi
                // CSV'de yapılmasına rağmen DB'ye hiç yansımamıştı, 4-ajan turunda bulundu).
                e.ViewName = r.ViewName;
                e.FinansModulTipi = r.FinansModulTipi;
                e.FinansBorcAlacakTipi = r.FinansBorcAlacakTipi;
                e.StokHareketYonu = r.StokHareketYonu;
            }
        }
        os.CommitChanges();
    }

    void SeedFisTuruVarsayilanDegerleri()
    {
        foreach (var r in SeedCsvReader.Read<FisTuruVarsayilanDegeriRow>("fis-turu-varsayilan-degerleri.csv"))
        {
            var fisTuru = os.FirstOrDefault<FisTuruTanim>(x => x.FisTuruKodu == r.FisTuruKodu);
            if (fisTuru == null) continue;

            object hedefDeger = r.HedefTipAdi switch
            {
                nameof(KDVTanim) => os.FirstOrDefault<KDVTanim>(x => x.KDVOrani == decimal.Parse(r.VarsayilanDegerKodu)),
                nameof(DovizTanim) => os.FirstOrDefault<DovizTanim>(x => x.DovizKodu == r.VarsayilanDegerKodu),
                nameof(BirimTanim) => os.FirstOrDefault<BirimTanim>(x => x.BirimAdi == r.VarsayilanDegerKodu),
                _ => null
            };
            if (hedefDeger == null) continue;

            Type hedefTip = hedefDeger.GetType();
            var mevcut = fisTuru.FisTuruVarsayilanDegerleri.FirstOrDefault(x => x.HedefTip == hedefTip);
            if (mevcut == null)
            {
                mevcut = os.CreateObject<FisTuruVarsayilanDegeri>();
                mevcut.FisTuruTanim = fisTuru;
                mevcut.HedefTip = hedefTip;
                SetOlusturan(mevcut);
            }
            mevcut.VarsayilanDeger.Target = hedefDeger;
            mevcut.Aktif = r.Aktif;
        }
        os.CommitChanges();
    }

    void SeedDoviz()
    {
        foreach (var r in SeedCsvReader.Read<DovizRow>("dovizler.csv"))
        {
            var mevcut = os.FirstOrDefault<DovizTanim>(x => x.DovizKodu == r.Kod);
            if (mevcut != null)
            {
                // Daha önce Sembol alanı olmadan seed edilmiş kayıtları geriye dönük tamamlar —
                // kullanıcının elle girdiği bir sembolün üzerine yazmaz.
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
            // Önce ISO koduyla, yoksa adla eşle (mevcut kayıtlarda kodu geriye doldur).
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
                s.PlakaKodu = r.PlakaKodu; // mevcut kayıtlarda plaka kodunu geriye doldur
            }
        }
        os.CommitChanges();
    }

    void SeedIlceler()
    {
        foreach (var r in SeedCsvReader.Read<IlceRow>("ilceler.csv"))
        {
            var sehir = os.FirstOrDefault<SehirTanim>(x => x.SehirAdi == r.SehirAdi);
            if (sehir == null) continue; // ilgili şehir bulunamazsa (veri tutarsızlığı) atla
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

    void SeedDepo()
    {
        if (os.FirstOrDefault<DepoTanim>(x => x.DepoAdi == "Ana Depo") != null) return;
        var e = os.CreateObject<DepoTanim>();
        e.DepoAdi = "Ana Depo";
        e.UlkeTanim = os.FirstOrDefault<UlkeTanim>(x => x.UlkeAdi == "Türkiye");
        e.IsVarsayilan = true;
        e.IsSystemRecord = true;
        SetOlusturan(e);
        os.CommitChanges();
    }

    void SeedStokGrup()
    {
        foreach (var r in SeedCsvReader.Read<StokGrupRow>("stok-gruplari.csv"))
        {
            if (os.FirstOrDefault<StokGrupTanim>(x => x.StokGrupAdi == r.Ad) != null) continue;
            var e = os.CreateObject<StokGrupTanim>();
            e.StokGrupAdi = r.Ad;
            e.IsSystemRecord = r.SistemKaydi;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    void SeedStokParametre()
    {
        if (os.GetObjects<StokParametre>().Count > 0) return;
        var p = os.CreateObject<StokParametre>();
        p.KoltukStokGrubu = os.FirstOrDefault<StokGrupTanim>(x => x.StokGrupAdi == "Koltuk");
        p.KumasStokGrubu = os.FirstOrDefault<StokGrupTanim>(x => x.StokGrupAdi == "Kumaş");
        p.SungerStokGrubu = os.FirstOrDefault<StokGrupTanim>(x => x.StokGrupAdi == "Sünger");
        p.KirlentStokGrubu = os.FirstOrDefault<StokGrupTanim>(x => x.StokGrupAdi == "Kırlent");
        p.AyakStokGrubu = os.FirstOrDefault<StokGrupTanim>(x => x.StokGrupAdi == "Ayak");
        SetOlusturan(p);
        os.CommitChanges();
    }

    void SeedBanka()
    {
        foreach (var r in SeedCsvReader.Read<BankaRow>("bankalar.csv"))
        {
            var e = os.FirstOrDefault<BankaTanim>(x => x.BankaAdi == r.Ad);
            if (e == null)
            {
                e = os.CreateObject<BankaTanim>();
                e.BankaAdi = r.Ad;
                e.SwiftKodu = r.SwiftKodu;
                e.IsSystemRecord = true;
                SetOlusturan(e);
            }
            else if (string.IsNullOrEmpty(e.SwiftKodu))
            {
                // Swift kodu sonradan eklendi; mevcut kayıtlarda boşsa CSV'den tamamlanır.
                e.SwiftKodu = r.SwiftKodu;
            }
        }
        os.CommitChanges();
    }

    void SeedKasa()
    {
        if (os.GetObjects<KasaBankaTanim>().Count > 0) return;
        var fisTuru = os.FirstOrDefault<FisTuruTanim>(x => x.ViewName == "KasaHesapTanim_DetailView");
        if (fisTuru == null) return;
        foreach (var doviz in os.GetObjects<DovizTanim>())
        {
            var kasa = os.CreateObject<KasaBankaTanim>();
            kasa.DovizTanim = doviz;
            kasa.HesapNo = numberSequenceService.SonrakiNumara(kasa.Session, kasa.GetType().FullName, fisTuru, DateTime.UtcNow);
            kasa.HesapAdi = doviz.DovizKodu + " Kasası";
            kasa.CariKasaBankaTipi = CariKasaBankaTipi.Kasa;
            kasa.AcilisBakiyesi = 0;
            kasa.AcilisBakiyesiTarihi = DateTime.UtcNow.Date;
            kasa.IsSystemRecord = true;
            SetOlusturan(kasa);
        }
        os.CommitChanges();
    }

    void SeedOdemeSekli()
    {
        foreach (var r in SeedCsvReader.Read<AdVarsayilanRow>("odeme-sekilleri.csv"))
        {
            if (os.FirstOrDefault<OdemeSekliTanim>(x => x.OdemeSekliAdi == r.Ad) != null) continue;
            var e = os.CreateObject<OdemeSekliTanim>();
            e.OdemeSekliAdi = r.Ad;
            e.IsVarsayilan = r.Varsayilan;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    void SeedMaliyetParametre()
    {
        if (os.GetObjects<MaliyetParametre>().Count > 0) return;
        var e = os.CreateObject<MaliyetParametre>();
        e.UretilecekTakimSayisi = 80;
        e.USDAlisKuru = 1;
        e.USDSatisKuru = 1;
        e.EuroAlisKuru = 1;
        e.EuroSatisKuru = 1;
        SetOlusturan(e);
        os.CommitChanges();
    }

    void SeedUretimMerkezi()
    {
        if (os.GetObjects<UretimMerkeziTanim>().Count > 0) return;
        var fisTuru = os.FirstOrDefault<FisTuruTanim>(x => x.ViewName == "UretimMerkeziTanim_DetailView");
        if (fisTuru == null) return;
        foreach (var r in SeedCsvReader.Read<AdRow>("uretim-merkezleri.csv"))
        {
            var e = os.CreateObject<UretimMerkeziTanim>();
            e.UretimMerkeziKodu = numberSequenceService.SonrakiNumara(e.Session, e.GetType().FullName, fisTuru, DateTime.UtcNow);
            e.UretimMerkeziAdi = r.Ad;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    void SeedOperasyon()
    {
        if (os.GetObjects<OperasyonTanim>().Count > 0) return;
        var fisTuru = os.FirstOrDefault<FisTuruTanim>(x => x.ViewName == "OperasyonTanim_DetailView");
        if (fisTuru == null) return;
        foreach (var r in SeedCsvReader.Read<OperasyonRow>("operasyonlar.csv").OrderBy(x => x.SiraNo))
        {
            var e = os.CreateObject<OperasyonTanim>();
            e.OperasyonKodu = numberSequenceService.SonrakiNumara(e.Session, e.GetType().FullName, fisTuru, DateTime.UtcNow);
            e.OperasyonSiraNo = r.SiraNo;
            e.OperasyonAdi = r.Ad;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    void SeedRota()
    {
        if (os.GetObjects<RotaTanimM>().Count > 0) return;
        var fisTuru = os.FirstOrDefault<FisTuruTanim>(x => x.ViewName == "RotaTanimM_DetailView");
        if (fisTuru == null) return;

        var rota = os.CreateObject<RotaTanimM>();
        rota.RotaKodu = numberSequenceService.SonrakiNumara(rota.Session, rota.GetType().FullName, fisTuru, DateTime.UtcNow);
        rota.RotaAdi = "Standart Rota";
        rota.IsSystemRecord = true;
        SetOlusturan(rota);

        int sira = 1;
        foreach (var op in os.GetObjects<OperasyonTanim>().OrderBy(x => x.OperasyonSiraNo))
        {
            var d = os.CreateObject<RotaTanimD>();
            d.RotaSiraNo = sira++;
            d.OperasyonTanim = op;
            d.RotaTanimM = rota;
            d.IsSystemRecord = true;
        }
        os.CommitChanges();
    }

    void SeedSatinAlmaParametre()
    {
        if (os.GetObjects<SatinAlmaParametre>().Count > 0) return;
        var p = os.CreateObject<SatinAlmaParametre>();
        p.TerminSuresi = 13;
        p.TeslimSuresi = 3;
        p.GecikenSiparisSuresi = 5;
        p.GecikenSiparisRengi = Color.Red;
        p.IsSystemRecord = true;
        SetOlusturan(p);
        os.CommitChanges();
    }

    void SeedSatisSiparisParametre()
    {
        if (os.GetObjects<SatisSiparisParametre>().Count > 0) return;
        var p = os.CreateObject<SatisSiparisParametre>();
        p.TeshirTeslimSuresi = 15;
        p.SiparisTeslimSuresi = 21;
        p.IsSystemRecord = true;
        SetOlusturan(p);
        os.CommitChanges();
    }
}
