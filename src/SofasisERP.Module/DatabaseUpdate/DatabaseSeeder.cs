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

using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.ExpressApp;
using SofasisERP.Module.BusinessObjects;
using SofasisERP.Module.Services;

namespace SofasisERP.Module.DatabaseUpdate;

// ---- CSV satır tipleri (başlıklar CSV ile birebir) ----
public class DovizRow { public string Kod { get; set; } public string Ad { get; set; } public bool Varsayilan { get; set; } public string Sembol { get; set; } }
public class UlkeRow { public string Kod { get; set; } public string Ad { get; set; } public string TelefonKodu { get; set; } public bool Varsayilan { get; set; } }
public class SehirRow { public string Ad { get; set; } public string PlakaKodu { get; set; } }
public class IlceRow { public string Ad { get; set; } public string SehirAdi { get; set; } }
public class KdvRow { public int Oran { get; set; } public bool Varsayilan { get; set; } }
public class AdVarsayilanRow { public string Ad { get; set; } public bool Varsayilan { get; set; } }
public class StokTipiRow { public string Kod { get; set; } public string Ad { get; set; } public bool MamulMu { get; set; } }
public class ModelRow { public string ModelAdi { get; set; } public string ModelAdiIngilizce { get; set; } }
public class StokGrupRow { public string StokTipiKodu { get; set; } public string StokGrupAdi { get; set; } }
public class CariHesapTipiRow { public string Kod { get; set; } public string Ad { get; set; } public CariHesapTipi Sinif { get; set; } }
public class CariHesapGrupRow { public string TipiKodu { get; set; } public string GrupAdi { get; set; } public YurtIciYurtDisiTipi YurtIciYurtDisi { get; set; } }
public class CariHesapAltGrupRow { public string GrupAdi { get; set; } public string AltGrupAdi { get; set; } public ToptanPerakendeTipi ToptanPerakende { get; set; } }
public class BankaRow { public string Ad { get; set; } public string SwiftKodu { get; set; } }
public class FisTuruRow
{
    public string Kod { get; set; }
    public string Ad { get; set; }
    public FinansModulTipi FinansModulTipi { get; set; }
    public StokHareketYonu StokHareketYonu { get; set; }
}

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
        SeedStokGrup();     // StokTipiTanim'e bağlı
        SeedStokParametre();
        SeedModelTanim();
        SeedFisTuru();
        SeedCariHesapTipi();
        SeedCariHesapGrup();    // CariHesapTipiTanim'e bağlı
        SeedCariHesapAltGrup(); // CariHesapGrupTanim'e bağlı
        SeedCariHesapParametre();
        SeedGenelParametre();
        SeedDepo();
        SeedKasa();
        SeedBankaTanim();
        SeedBanka();
        SeedAcilisDengeHesabi();
        os.CommitChanges();

        // 4-ajanlı testte (2026-08-22, yazılım mühendisi bulgusu) tespit edildi: bu
        // metod rastgele (sabit Random tohumuyla de olsa) KURGUSAL finansal hareketler
        // üretiyor — yalnızca geliştirme/demo ortamında anlamlı, production'da veri
        // kirliliği riski. Bkz. OrtamKontrolu.cs.
        if (OrtamKontrolu.GelistirmeOrtamiMi())
            SeedKasaCariBankaTestVerisi();
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

    // Bileşen/hammadde kategorileri — kullanıcının gerçek ürün kataloğundan (2026-08-18)
    // uyarlandı, tamamı StokTipiTanim=150 (Hammadde) altında (kullanıcı kararı). StokGrupKodu
    // burada elle üretilmez — StokGrupTanim.OnSaving zaten StokTipiTanim atanınca otomatik
    // üretir (bkz. StokKoduJeneratoru.SonrakiStokGrupKodu).
    void SeedStokGrup()
    {
        foreach (var r in SeedCsvReader.Read<StokGrupRow>("stok-grup-tanimlari.csv"))
        {
            if (os.FirstOrDefault<StokGrupTanim>(x => x.StokGrupAdi == r.StokGrupAdi) != null) continue;
            StokTipiTanim tip = os.FirstOrDefault<StokTipiTanim>(x => x.StokTipiKodu == r.StokTipiKodu);
            if (tip == null) continue;
            var e = os.CreateObject<StokGrupTanim>();
            e.StokTipiTanim = tip;
            e.StokGrupAdi = r.StokGrupAdi;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    // Kullanıcının gerçek ürün kataloğundan (2026-08-18) uyarlandı — Model Zorluk Derecesi
    // ve Resim bu turda BİLEREK atlandı (alan yok / gerçek görsel dosyaları elde yok).
    void SeedModelTanim()
    {
        foreach (var r in SeedCsvReader.Read<ModelRow>("model-tanimlari.csv"))
        {
            var mevcut = os.FirstOrDefault<ModelTanim>(x => x.ModelAdi == r.ModelAdi);
            if (mevcut != null)
            {
                if (string.IsNullOrEmpty(mevcut.ModelAdiIngilizce))
                    mevcut.ModelAdiIngilizce = r.ModelAdiIngilizce;
                continue;
            }
            var e = os.CreateObject<ModelTanim>();
            e.ModelAdi = r.ModelAdi;
            e.ModelAdiIngilizce = r.ModelAdiIngilizce;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    // StokTanimYeniKayitVarsayilanlariController'ın View.Id'ye göre StokTanim'e
    // atadığı fiş türleri (STOKTN/HZMTTN/MSRFTN) + CariHesapTanim'in kendi kodu
    // (CARITN) — bkz. NumberSequenceService.SonrakiNumara.
    void SeedFisTuru()
    {
        foreach (var r in SeedCsvReader.Read<FisTuruRow>("fis-turleri.csv"))
        {
            var mevcut = os.FirstOrDefault<FisTuruTanim>(x => x.FisTuruKodu == r.Kod);
            if (mevcut != null)
            {
                // Daha önce (FinansModulTipi/StokHareketYonu eklenmeden) seed edilmiş satırları geriye dönük tamamlar.
                if (mevcut.FinansModulTipi == FinansModulTipi.Yok)
                    mevcut.FinansModulTipi = r.FinansModulTipi;
                if (mevcut.StokHareketYonu == StokHareketYonu.Yok)
                    mevcut.StokHareketYonu = r.StokHareketYonu;
                continue;
            }
            var e = os.CreateObject<FisTuruTanim>();
            e.FisTuruKodu = r.Kod;
            e.FisTuruAdi = r.Ad;
            e.FinansModulTipi = r.FinansModulTipi;
            e.StokHareketYonu = r.StokHareketYonu;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    // Cari Hesap kod hiyerarşisinin en üst seviyesi — StokTipiTanim ile aynı gerekçe,
    // ama Tekdüzen Hesap Planı'nın GERÇEK 12x/22x/32x (Alıcılar/Alıcı-Satıcı/Satıcılar)
    // numaralandırma STİLİNDEN esinlenilmiştir (bkz. CariHesapKoduJeneratoru).
    void SeedCariHesapTipi()
    {
        foreach (var r in SeedCsvReader.Read<CariHesapTipiRow>("cari-hesap-tipi-tanimlari.csv"))
        {
            if (os.FirstOrDefault<CariHesapTipiTanim>(x => x.CariHesapTipiKodu == r.Kod) != null) continue;
            var e = os.CreateObject<CariHesapTipiTanim>();
            e.CariHesapTipiKodu = r.Kod;
            e.CariHesapTipiAdi = r.Ad;
            e.CariHesapSinifi = r.Sinif;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    // Orta seviye (Yurtiçi/Yurtdışı kırılımı) — CariHesapGrupKodu burada elle üretilmez,
    // CariHesapGrupTanim.OnSaving zaten CariHesapTipiTanim atanınca otomatik üretir (bkz.
    // CariHesapKoduJeneratoru.SonrakiCariHesapGrupKodu). CSV satır SIRASI önemli: her
    // TipiKodu için önce Yurtiçi sonra Yurtdışı yazılmalı ki .01/.02 doğru sırayla çıksın.
    void SeedCariHesapGrup()
    {
        foreach (var r in SeedCsvReader.Read<CariHesapGrupRow>("cari-hesap-grup-tanimlari.csv"))
        {
            if (os.FirstOrDefault<CariHesapGrupTanim>(x => x.CariHesapGrupAdi == r.GrupAdi) != null) continue;
            CariHesapTipiTanim tip = os.FirstOrDefault<CariHesapTipiTanim>(x => x.CariHesapTipiKodu == r.TipiKodu);
            if (tip == null) continue;
            var e = os.CreateObject<CariHesapGrupTanim>();
            e.CariHesapTipiTanim = tip;
            e.CariHesapGrupAdi = r.GrupAdi;
            e.YurtIciYurtDisiTipi = r.YurtIciYurtDisi;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    // En alt seviye (Toptan/Perakende kırılımı) — StokAltGrupTanim'in aksine burada CSV
    // ile önceden dolduruluyor (kullanıcı kararı 2026-08-22: sabit/kapalı 12 kombinasyon,
    // Stok'un açık uçlu/muhasebe-yönetimli AltGrup'undan farklı). Kod yine elle üretilmez.
    void SeedCariHesapAltGrup()
    {
        foreach (var r in SeedCsvReader.Read<CariHesapAltGrupRow>("cari-hesap-alt-grup-tanimlari.csv"))
        {
            if (os.FirstOrDefault<CariHesapAltGrupTanim>(x => x.CariHesapAltGrupAdi == r.AltGrupAdi) != null) continue;
            CariHesapGrupTanim grup = os.FirstOrDefault<CariHesapGrupTanim>(x => x.CariHesapGrupAdi == r.GrupAdi);
            if (grup == null) continue;
            var e = os.CreateObject<CariHesapAltGrupTanim>();
            e.CariHesapGrupTanim = grup;
            e.CariHesapAltGrupAdi = r.AltGrupAdi;
            e.ToptanPerakendeTipi = r.ToptanPerakende;
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

    // GenelParametre ile aynı gerekçe: tek satırlık singleton ekran hiç boş gelmemeli.
    // Ayrıca StokParametre.AfterConstruction'daki tekrar-oluşturma-engelleme (dedup) mantığı
    // yalnızca zaten bir kayıt VARKEN doğru çalışıyor — tablo tamamen boşken ilk "Yeni" tıklaması
    // kendi kendini mevcut kayıt sanıp yanlış davranabiliyordu (kullanıcı kararı 2026-08-22:
    // varsayılan kayıt Jenerator seçili olarak baştan var olsun).
    void SeedStokParametre()
    {
        if (os.GetObjects<StokParametre>().Any()) return;
        var e = os.CreateObject<StokParametre>();
        e.StokKoduUretimYontemi = StokKoduUretimYontemi.Jenerator;
        e.IsSystemRecord = true;
        SetOlusturan(e);
        os.CommitChanges();
    }

    // CariHesapParametre için aynı gerekçe — bkz. SeedStokParametre.
    void SeedCariHesapParametre()
    {
        if (os.GetObjects<CariHesapParametre>().Any()) return;
        var e = os.CreateObject<CariHesapParametre>();
        e.CariHesapKoduUretimYontemi = CariHesapKoduUretimYontemi.Jenerator;
        e.IsSystemRecord = true;
        SetOlusturan(e);
        os.CommitChanges();
    }

    // Faz 2 (StokHareketleriM/D) — bir depo olmadan hiçbir stok hareketi girilemez
    // (DepoTanim alanı zorunlu), bu yüzden en az bir "Merkez Depo" varsayılan olarak seed edilir.
    void SeedDepo()
    {
        if (os.GetObjects<DepoTanim>().Any()) return;
        var e = os.CreateObject<DepoTanim>();
        e.DepoKodu = "MERKEZ";
        e.DepoAdi = "Merkez Depo";
        e.IsVarsayilan = true;
        e.IsSystemRecord = true;
        SetOlusturan(e);
        os.CommitChanges();
    }

    // 4-ajanlı testte (2026-08-21) bulunan hata: Açılış Fişi ekranlarında Karşı Hesap
    // serbestçe seçilebiliyordu, bu da açılış bakiyesinin rastgele gerçek bir Cari'nin
    // ticari bakiyesine karışmasına yol açıyordu. Tek, sabit bir sistem hesabı — Model.
    // DesignedDiffs.xafml'deki 3 Açılış ekranı artık Karşı Hesap'ı CustomCode1'e göre
    // YALNIZCA bu kayda filtreliyor.
    void SeedAcilisDengeHesabi()
    {
        if (os.FirstOrDefault<CariHesapTanim>(x => x.CustomCode1 == "ACILIS_DENGE") != null) return;
        var doviz = os.FirstOrDefault<DovizTanim>(x => x.DovizKodu == "TRY");
        if (doviz == null) return;
        var e = os.CreateObject<CariHesapTanim>();
        e.HesapAdi = "Açılış Dengesi Hesabı";
        e.CustomCode1 = "ACILIS_DENGE";
        e.DovizTanim = doviz;
        e.IsSystemRecord = true;
        // Bu bir gerçek müşteri/tedarikçi değil, sistem hesabı — Cari Hesap Alt Grubu
        // sınıflandırması anlamsız, bu yüzden CariHesapKodu jeneratörü hiç TETİKLENMEDEN
        // elle atanır (sıfır veritabanında CariHesapAltGrubu boşken jeneratörün
        // ArgumentException fırlatıp seed'i çökertmesini önler — bkz. CariHesapKoduJeneratoru.
        // SonrakiCariHesapKodu, Jenerator modunda AltGrubu null ise istisna fırlatıyor).
        e.CariHesapKodu = "SYS.ACILIS";
        SetOlusturan(e);
        os.CommitChanges();
    }

    // Faz 3 (KasaHareketleriM/D) — DepoTanim ile aynı gerekçe: en az bir varsayılan Kasa olmadan hiçbir kasa hareketi girilemez.
    void SeedKasa()
    {
        if (os.GetObjects<KasaTanim>().Any()) return;
        var doviz = os.FirstOrDefault<DovizTanim>(x => x.DovizKodu == "TRY");
        if (doviz == null) return;
        var e = os.CreateObject<KasaTanim>();
        e.KasaKodu = "ANAKASA";
        e.HesapAdi = "Ana Kasa";
        e.DovizTanim = doviz;
        e.IsVarsayilan = true;
        e.IsSystemRecord = true;
        SetOlusturan(e);
        os.CommitChanges();
    }

    // Eski ERP'deki bankalar.csv ile birebir (BankaTanim.cs — banka KURUMU katalog tablosu,
    // BankaHesabiTanim'den [bir hesabın kendisi] AYRI bir katman).
    void SeedBankaTanim()
    {
        foreach (var r in SeedCsvReader.Read<BankaRow>("bankalar.csv"))
        {
            var mevcut = os.FirstOrDefault<BankaTanim>(x => x.BankaAdi == r.Ad);
            if (mevcut != null)
            {
                if (string.IsNullOrEmpty(mevcut.SwiftKodu))
                    mevcut.SwiftKodu = r.SwiftKodu;
                continue;
            }
            var e = os.CreateObject<BankaTanim>();
            e.BankaAdi = r.Ad;
            e.SwiftKodu = string.IsNullOrEmpty(r.SwiftKodu) ? null : r.SwiftKodu;
            e.IsSystemRecord = true;
            SetOlusturan(e);
        }
        os.CommitChanges();
    }

    // Faz 3 (BankaHareketleriM/D) — SeedKasa ile aynı gerekçe.
    void SeedBanka()
    {
        if (os.GetObjects<BankaHesabiTanim>().Any()) return;
        var doviz = os.FirstOrDefault<DovizTanim>(x => x.DovizKodu == "TRY");
        var banka = os.FirstOrDefault<BankaTanim>(x => x.BankaAdi == "T.C. Ziraat Bankası");
        if (doviz == null || banka == null) return;
        var e = os.CreateObject<BankaHesabiTanim>();
        e.BankaTanim = banka;
        e.SubeAdi = "Merkez Şubesi";
        e.HesapNo = "000000";
        e.DovizTanim = doviz;
        e.IsVarsayilan = true;
        e.IsSystemRecord = true;
        SetOlusturan(e);
        os.CommitChanges();
    }

    // 2026-08-20: kullanıcı talebiyle — Kasa/Cari/Banka Hareketleri ekranlarının
    // 4-ajanlı testi için en az 50 Kasa, 50 Banka, 50 Cari hareketi (çapraz
    // görünürlük OR kriteri sayesinde bir satır aynı anda iki tarafı da sayar).
    // Idempotent: bir kez üretilince bir daha tekrar etmez.
    void SeedKasaCariBankaTestVerisi()
    {
        if (os.GetObjects<KasaCariBankaHareketleri>().Any()) return;

        var acilisDengeHesabi = os.FirstOrDefault<CariHesapTanim>(x => x.CustomCode1 == "ACILIS_DENGE");
        var kasalar = os.GetObjects<KasaTanim>().Cast<Hesap>().ToList();
        var bankalar = os.GetObjects<BankaHesabiTanim>().Cast<Hesap>().ToList();
        var cariler = os.GetObjects<CariHesapTanim>().Where(x => x != acilisDengeHesabi).Cast<Hesap>().ToList();
        if (!kasalar.Any() || !bankalar.Any() || !cariler.Any() || acilisDengeHesabi == null) return;

        // Tedarikçi (alacaklı) açılış bakiyesi yön testi için en az bir Tedarikçi cari
        // olsun (4-ajanlı testte bulunan "Cari Açılış Tedarikçiyi ters yönde işliyor"
        // hatasının doğrulanabilmesi için) — idempotent, tekrar seed'de zararsız.
        var ilkCari = cariler.OfType<CariHesapTanim>().FirstOrDefault();
        var saticiAltGrubu = os.GetObjects<CariHesapAltGrupTanim>()
            .FirstOrDefault(x => x.CariHesapGrupTanim?.CariHesapTipiTanim?.CariHesapSinifi == CariHesapTipi.Tedarikci);
        if (ilkCari != null && saticiAltGrubu != null)
        {
            ilkCari.CariHesapGrubu = saticiAltGrubu.CariHesapGrupTanim;
            ilkCari.CariHesapAltGrubu = saticiAltGrubu;
        }

        var acilis = os.FirstOrDefault<FisTuruTanim>(x => x.FisTuruKodu == "ACILIS");
        var tahsil = os.FirstOrDefault<FisTuruTanim>(x => x.FisTuruKodu == "TAHSIL");
        var odeme = os.FirstOrDefault<FisTuruTanim>(x => x.FisTuruKodu == "ODEME");
        var virman = os.FirstOrDefault<FisTuruTanim>(x => x.FisTuruKodu == "VIRMAN");
        if (acilis == null || tahsil == null || odeme == null || virman == null) return;

        var rnd = new Random(20260820);
        Hesap Rastgele(List<Hesap> liste) => liste[rnd.Next(liste.Count)];
        decimal RastgeleTutar() => Math.Round((decimal)(rnd.Next(10000, 5000000)) / 100m, 2);
        DateTime RastgeleTarih() => TurkiyeZamani.Bugun.AddDays(-rnd.Next(0, 90));

        void Hareket(FisTuruTanim fisTuru, Hesap kaynak, Hesap karsi, bool odemeSekliVar)
        {
            var h = os.CreateObject<KasaCariBankaHareketleri>();
            h.FisTarihi = RastgeleTarih();
            h.FisTuruTanim = fisTuru;
            h.KaynakHesap = kaynak;
            h.KarsiHesap = karsi;
            h.BorcTutar = RastgeleTutar();
            if (odemeSekliVar)
                h.OdemeSekli = rnd.Next(2) == 0 ? OdemeSekli.Nakit : OdemeSekli.KrediKarti;
            SetOlusturan(h);
        }

        // A) Kasa <-> Cari (Tahsilat/Ödeme karışık) — Kasa VE Cari sayacına katkı.
        for (int i = 0; i < 35; i++)
        {
            var kasa = Rastgele(kasalar);
            var cari = Rastgele(cariler);
            if (rnd.Next(2) == 0)
                Hareket(tahsil, kasa, cari, odemeSekliVar: true);
            else
                Hareket(odeme, cari, kasa, odemeSekliVar: true);
        }

        // B) Banka <-> Cari (Tahsilat/Ödeme karışık) — Banka VE Cari sayacına katkı.
        for (int i = 0; i < 35; i++)
        {
            var banka = Rastgele(bankalar);
            var cari = Rastgele(cariler);
            if (rnd.Next(2) == 0)
                Hareket(tahsil, banka, cari, odemeSekliVar: true);
            else
                Hareket(odeme, cari, banka, odemeSekliVar: true);
        }

        // C) Kasa <-> Banka Virman — Kasa VE Banka sayacına katkı.
        for (int i = 0; i < 25; i++)
        {
            var kasa = Rastgele(kasalar);
            var banka = Rastgele(bankalar);
            if (rnd.Next(2) == 0)
                Hareket(virman, kasa, banka, odemeSekliVar: false);
            else
                Hareket(virman, banka, kasa, odemeSekliVar: false);
        }

        // D) Açılış — Karşı Hesap HER ZAMAN sabit "Açılış Dengesi Hesabı" (4-ajanlı
        // testte bulunan hata: eskiden rastgele bir gerçek Cari'ye karşı işleniyordu).
        foreach (var kasa in kasalar) Hareket(acilis, kasa, acilisDengeHesabi, odemeSekliVar: false);
        foreach (var banka in bankalar) Hareket(acilis, banka, acilisDengeHesabi, odemeSekliVar: false);
        foreach (var cari in cariler) Hareket(acilis, cari, acilisDengeHesabi, odemeSekliVar: true);

        os.CommitChanges();
    }
}
