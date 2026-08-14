using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sofasis.Module.Services;

public sealed class SatinAlmaIrsaliyeServisi : ISatinAlmaIrsaliyeServisi
{
    public IrsaliyeM IrsaliyeTaslagiOlustur(IObjectSpace objectSpace, SatinAlmaSiparisiM siparis)
    {
        if (siparis == null)
            throw new UserFriendlyException("Sipariş bulunamadı.");
        if (siparis.Durum == SatinAlmaSiparisDurumu.IptalEdildi)
            throw new UserFriendlyException("İptal edilmiş bir sipariş için İrsaliye oluşturulamaz.");
        if (!siparis.SatinAlmaSiparisiDs.Any(x => x.KalanMiktar > 0))
            throw new UserFriendlyException("Bu siparişte teslim alınacak kalan miktar yok — tüm kalemler zaten teslim alınmış.");

        // NOT: Burada daha önce ObjectSpace.ModifiedObjects taramasına dayanan bir "çifte-tıklama
        // güvenlik ağı" vardı — kaldırıldı (canlı testte kullanıcı ve 4-ajan turunda bağımsız
        // doğrulandı, bkz. docs/CHANGELOG.md 2026-08-14). Kullanıcı popup'ı Kaydet/İptal ile
        // kapatmadan (F5, geri tuşu, başka ekrana geçiş) View'dan ayrılırsa, Blazor Server circuit'i
        // canlı kaldığı sürece (sayfa yenilemesi bile circuit reconnection nedeniyle garanti
        // sıfırlamaz) ObjectSpace'teki commit edilmemiş "hayalet" nesne(ler) ModifiedObjects'te
        // asılı kalıyor ve bu kontrol kullanıcıyı o Sipariş için SÜRESİZ kilitliyordu — hiçbir
        // kullanıcı aksiyonuyla düzeltilemiyordu. Controller'daki `islemSuruyor` bayrağı zaten aynı
        // senkron çağrı içindeki çift-tıklamayı önlüyor; modal popup açıkken ana ekran zaten
        // bloklanıyor. Gerçek çoklu-oturum çakışması senaryosunu bu kontrol ZATEN önlemiyordu
        // (ModifiedObjects yalnız KENDİ ObjectSpace'ini görür, başka bir circuit'inkini göremez).
        FisTuruTanim irsaliyeFisTuru = objectSpace.FindObject<FisTuruTanim>(
            CriteriaOperator.Parse("FisTuruKodu = 'IRALIS'"));
        if (irsaliyeFisTuru == null)
            throw new UserFriendlyException("IRALIS (Alış İrsaliyesi) fiş türü tanımlı değil — sistem yöneticinizle irtibata geçin.");

        FisTuruTanim stokFisTuru = objectSpace.FindObject<FisTuruTanim>(
            CriteriaOperator.Parse("FisTuruKodu = 'STSAGR'"));
        if (stokFisTuru == null)
            throw new UserFriendlyException("STSAGR (Satın Alma Mal Kabul Girişi) fiş türü tanımlı değil — sistem yöneticinizle irtibata geçin.");

        IrsaliyeM irsaliye = objectSpace.CreateObject<IrsaliyeM>();
        irsaliye.FisTuruTanim = irsaliyeFisTuru;
        irsaliye.Tedarikci = siparis.Tedarikci;
        irsaliye.KaynakSiparis = siparis;
        // İrsaliye SİPARİŞİN dövizinde gelir — DovizTanim setter'ının OnChanged'i kur güncellemesini
        // İRSALİYE tarihine göre yapacağından, DovizKuru'yu EXPLICIT olarak sipariş kurundan miras
        // almak yerine (aynı gün farklı kur olabilir) DovizTanim atamasının kendi mekanizmasına
        // bırakıyoruz; yalnızca DovizTanim'i sipariş dövizine sabitliyoruz.
        irsaliye.DovizTanim = siparis.DovizTanim;
        // DepoTanim BİLİNÇLİ OLARAK AfterConstruction'daki varsayılana bırakılır — sipariş bir
        // depoya bağlı değil, kullanıcı dönen taslak ekranında hangi depoya teslim alındığını seçer.

        StokHareketleriM stokFisi = objectSpace.CreateObject<StokHareketleriM>();
        stokFisi.FisTuruTanim = stokFisTuru;
        irsaliye.StokHareketleriM = stokFisi;

        var kalanSatirlar = siparis.SatinAlmaSiparisiDs.Where(x => x.KalanMiktar > 0).ToList();
        foreach (SatinAlmaSiparisiD siparisSatiri in kalanSatirlar)
        {
            // Sıra kritik (bkz. StokHareketleriD.OnChanged): StokHareketleriM ÖNCE, StokTanim SONRA,
            // DovizTanim EXPLICIT sipariş döviziyle ezilir, BirimMaliyet EN SON.
            StokHareketleriD stokSatiri = objectSpace.CreateObject<StokHareketleriD>();
            stokSatiri.StokHareketleriM = stokFisi;
            stokSatiri.StokTanim = siparisSatiri.StokTanim;
            stokSatiri.DovizTanim = siparis.DovizTanim;
            stokSatiri.Miktar = siparisSatiri.KalanMiktar;
            stokSatiri.BirimMaliyet = siparisSatiri.BirimFiyat;
            stokSatiri.KaynakBelgeTipi = typeof(SatinAlmaSiparisiD);
            stokSatiri.KaynakBelgeOid = siparisSatiri.Oid;

            IrsaliyeD irsaliyeSatiri = objectSpace.CreateObject<IrsaliyeD>();
            irsaliyeSatiri.IrsaliyeM = irsaliye;
            irsaliyeSatiri.StokTanim = siparisSatiri.StokTanim;
            irsaliyeSatiri.Miktar = siparisSatiri.KalanMiktar;
            irsaliyeSatiri.BirimFiyat = siparisSatiri.BirimFiyat;
            irsaliyeSatiri.KaynakSiparisD = siparisSatiri;
            irsaliyeSatiri.StokHareketiD = stokSatiri;
        }

        return irsaliye;
    }

    public void TeslimatiIsle(IrsaliyeM irsaliye)
    {
        if (irsaliye?.StokHareketleriM == null) return;

        foreach (StokHareketleriD satir in irsaliye.StokHareketleriM.StokHareketleriDs
            .Where(x => x.KaynakBelgeTipi == typeof(SatinAlmaSiparisiD) && x.KaynakBelgeOid != null))
        {
            SatinAlmaSiparisiD siparisSatiri = irsaliye.Session.GetObjectByKey<SatinAlmaSiparisiD>(satir.KaynakBelgeOid);
            if (siparisSatiri == null) continue;

            // Kullanıcı İrsaliye popup'ında satır miktarını elle artırabildiğinden (taslak üzerinde
            // serbestçe düzenlenebilir), burada üst sınır ZORUNLU — aksi halde TeslimEdilenMiktar
            // sipariş Miktar'ını aşıp KalanMiktar negatife düşebilir.
            if (satir.Miktar > siparisSatiri.KalanMiktar)
                throw new UserFriendlyException(
                    $"'{siparisSatiri.StokTanim?.StokAdi}' kalemi için girilen miktar ({satir.Miktar}), kalan miktardan ({siparisSatiri.KalanMiktar}) fazla olamaz.");

            siparisSatiri.TeslimEdilenMiktar += satir.Miktar;

            SatinAlmaSiparisiM siparis = siparisSatiri.SatinAlmaSiparisiM;
            if (siparis == null) continue;

            bool tamamiTeslimEdildi = siparis.SatinAlmaSiparisiDs.All(x => x.KalanMiktar <= 0);
            siparis.Durum = tamamiTeslimEdildi
                ? SatinAlmaSiparisDurumu.MalKabulYapildi
                : SatinAlmaSiparisDurumu.KismiTeslimAlindi;
        }
    }

    // v1: yalnızca TAM iade (kısmi iade YOK — YAGNI, ADR-017'de dokümante edildi). Belge SİLİNMEZ/
    // DÜZENLENMEZ (IrsaliyeKilitleController immutable tutar) — düzeltme her zaman YENİ bir ters
    // belge ile yapılır (VUK boşluksuz seri + Netsis/Logo/Mikro "gün 1" konvansiyonu).
    public IrsaliyeM IadeTaslagiOlustur(IObjectSpace objectSpace, IrsaliyeM orijinalIrsaliye)
    {
        if (orijinalIrsaliye == null)
            throw new UserFriendlyException("İrsaliye bulunamadı.");
        if (orijinalIrsaliye.FisTuruTanim?.FisTuruKodu != "IRALIS")
            throw new UserFriendlyException("Yalnızca normal (İade olmayan) İrsaliyeler iade edilebilir.");
        if (orijinalIrsaliye.StokHareketleriM == null)
            throw new UserFriendlyException("İrsaliyenin bağlı bir stok hareket fişi bulunamadı.");

        Session session = orijinalIrsaliye.Session;

        IrsaliyeM mevcutIade = objectSpace.FindObject<IrsaliyeM>(
            new BinaryOperator(nameof(IrsaliyeM.KaynakIrsaliye), orijinalIrsaliye));
        if (mevcutIade != null)
            throw new UserFriendlyException($"Bu İrsaliye zaten '{mevcutIade.IrsaliyeNo}' ile iade edilmiş.");

        // NOT: ModifiedObjects tabanlı "çifte-tıklama güvenlik ağı" burada da kaldırıldı — bkz.
        // IrsaliyeTaslagiOlustur'daki gerekçe.

        // Bu İrsaliyenin satırlarından ZATEN faturalanmış, HENÜZ İADE EDİLMEMİŞ bir Faturaya bağlı
        // olan varsa doğrudan İrsaliye iadesi engellenir — önce o Fatura(lar) iade edilmeli. Aksi
        // halde stok geri alınırken Fatura hâlâ KDV/Tevkifat/Cari borcu taşımaya devam ederdi
        // (tutarsızlık). NOT: Fatura iade edildiğinde orijinal FaturaD.KaynakStokHareketiD referansı
        // SİLİNMEZ (orijinal Fatura hâlâ var, yanına yeni bir İade Faturası eklenir) — bu yüzden
        // yalnızca "faturalı satır var mı" değil, o Fatura'nın KENDİSİNİN henüz iade edilip
        // edilmediği de ayrıca kontrol edilir.
        List<Guid> stokSatirOidleri = orijinalIrsaliye.StokHareketleriM.StokHareketleriDs.Select(x => x.Oid).ToList();
        List<Guid> baglıFaturaOidleri = stokSatirOidleri.Count == 0
            ? new List<Guid>()
            : new XPQuery<FaturaD>(session)
                .Where(f => f.KaynakStokHareketiD != null && stokSatirOidleri.Contains(f.KaynakStokHareketiD.Oid))
                .Select(f => f.FaturaM.Oid)
                .Distinct()
                .ToList();
        // 4-ajan turunda bulundu: bu sorgu daha önce filtrelenmeden TÜM sistemdeki İade
        // Faturalarını (tüm tedarikçiler/dönemler) çekiyordu — FaturaKaydetServisi'nde daha
        // önce düzeltilmiş aynı sınıf hata burada tekrarlanmıştı. baglıFaturaOidleri'ye göre
        // sınırlandırıldı (IN filtresi SQL'e itilir).
        HashSet<Guid> iadeEdilmisFaturaOidleri = baglıFaturaOidleri.Count == 0
            ? new HashSet<Guid>()
            : new XPQuery<FaturaM>(session)
                .Where(x => x.KaynakFatura != null && baglıFaturaOidleri.Contains(x.KaynakFatura.Oid))
                .Select(x => x.KaynakFatura.Oid)
                .ToHashSet();
        bool iadeEdilmemisFaturaVar = baglıFaturaOidleri.Any(oid => !iadeEdilmisFaturaOidleri.Contains(oid));
        if (iadeEdilmemisFaturaVar)
            throw new UserFriendlyException(
                "Bu İrsaliyenin satırları faturalanmış — önce ilgili Fatura(lar)ı iade edin, sonra İrsaliyeyi iade edebilirsiniz.");

        FisTuruTanim iadeIrsaliyeFisTuru = objectSpace.FindObject<FisTuruTanim>(
            CriteriaOperator.Parse("FisTuruKodu = 'IRALID'"));
        if (iadeIrsaliyeFisTuru == null)
            throw new UserFriendlyException("IRALID (Alış İade İrsaliyesi) fiş türü tanımlı değil — sistem yöneticinizle irtibata geçin.");

        FisTuruTanim iadeStokFisTuru = objectSpace.FindObject<FisTuruTanim>(
            CriteriaOperator.Parse("FisTuruKodu = 'STSAIC'"));
        if (iadeStokFisTuru == null)
            throw new UserFriendlyException("STSAIC (Satın Alma İade Çıkışı) fiş türü tanımlı değil — sistem yöneticinizle irtibata geçin.");

        IrsaliyeM iade = objectSpace.CreateObject<IrsaliyeM>();
        iade.FisTuruTanim = iadeIrsaliyeFisTuru;
        iade.KaynakIrsaliye = orijinalIrsaliye;
        iade.Tedarikci = orijinalIrsaliye.Tedarikci;
        iade.KaynakSiparis = orijinalIrsaliye.KaynakSiparis;
        iade.DovizTanim = orijinalIrsaliye.DovizTanim;
        iade.DepoTanim = orijinalIrsaliye.StokHareketleriM.DepoTanim;

        StokHareketleriM iadeStokFisi = objectSpace.CreateObject<StokHareketleriM>();
        iadeStokFisi.FisTuruTanim = iadeStokFisTuru;
        iadeStokFisi.DepoTanim = orijinalIrsaliye.StokHareketleriM.DepoTanim;
        iade.StokHareketleriM = iadeStokFisi;

        foreach (IrsaliyeD orijinalSatir in orijinalIrsaliye.IrsaliyeDs)
        {
            // Sıra kritik (bkz. StokHareketleriD.OnChanged): StokHareketleriM ÖNCE, StokTanim SONRA.
            // Çıkış yönünde BirimMaliyet/DovizTanim/DovizKuru kullanıcı/servis tarafından SET EDİLMEZ
            // — motor ObjectSaving()'de bunları StokTanim.OrtalamaMaliyet'e/TRY'ye otomatik sabitler.
            StokHareketleriD iadeStokSatiri = objectSpace.CreateObject<StokHareketleriD>();
            iadeStokSatiri.StokHareketleriM = iadeStokFisi;
            iadeStokSatiri.StokTanim = orijinalSatir.StokTanim;
            iadeStokSatiri.Miktar = orijinalSatir.Miktar;
            iadeStokSatiri.KaynakBelgeTipi = typeof(SatinAlmaSiparisiD);
            iadeStokSatiri.KaynakBelgeOid = orijinalSatir.KaynakSiparisD?.Oid;

            IrsaliyeD iadeSatir = objectSpace.CreateObject<IrsaliyeD>();
            iadeSatir.IrsaliyeM = iade;
            iadeSatir.StokTanim = orijinalSatir.StokTanim;
            iadeSatir.Miktar = orijinalSatir.Miktar;
            iadeSatir.BirimFiyat = orijinalSatir.BirimFiyat;
            iadeSatir.KaynakSiparisD = orijinalSatir.KaynakSiparisD;
            iadeSatir.StokHareketiD = iadeStokSatiri;
        }

        return iade;
    }

    public void IadeyiIsle(IrsaliyeM iadeIrsaliyesi)
    {
        if (iadeIrsaliyesi?.StokHareketleriM == null) return;

        foreach (StokHareketleriD satir in iadeIrsaliyesi.StokHareketleriM.StokHareketleriDs
            .Where(x => x.KaynakBelgeTipi == typeof(SatinAlmaSiparisiD) && x.KaynakBelgeOid != null))
        {
            SatinAlmaSiparisiD siparisSatiri = iadeIrsaliyesi.Session.GetObjectByKey<SatinAlmaSiparisiD>(satir.KaynakBelgeOid);
            if (siparisSatiri == null) continue;

            if (satir.Miktar > siparisSatiri.TeslimEdilenMiktar)
                throw new UserFriendlyException(
                    $"'{siparisSatiri.StokTanim?.StokAdi}' kalemi için iade miktarı ({satir.Miktar}), teslim edilen miktardan ({siparisSatiri.TeslimEdilenMiktar}) fazla olamaz.");

            siparisSatiri.TeslimEdilenMiktar -= satir.Miktar;

            SatinAlmaSiparisiM siparis = siparisSatiri.SatinAlmaSiparisiM;
            if (siparis == null) continue;

            bool hicTeslimatYok = siparis.SatinAlmaSiparisiDs.All(x => x.TeslimEdilenMiktar <= 0);
            bool tamamiTeslimEdildi = siparis.SatinAlmaSiparisiDs.All(x => x.KalanMiktar <= 0);
            siparis.Durum = hicTeslimatYok
                ? SatinAlmaSiparisDurumu.Verildi
                : tamamiTeslimEdildi
                    ? SatinAlmaSiparisDurumu.MalKabulYapildi
                    : SatinAlmaSiparisDurumu.KismiTeslimAlindi;
        }
    }
}
