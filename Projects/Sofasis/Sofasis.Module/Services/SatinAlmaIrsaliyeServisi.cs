using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using Sofasis.Module.BusinessObjects;
using System;
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

        // ÇİFTE-TIKLAMA / MÜKERRER TASLAK GÜVENLİK AĞI — bkz. SatinAlmaMalKabulServisi'nin eski
        // eşdeğerindeki aynı gerekçe (docs/CHANGELOG.md 2026-08-13, "çift-Kaydet-tıklama" bulgusu).
        var siparisSatirOidleri = siparis.SatinAlmaSiparisiDs.Select(x => x.Oid).ToHashSet();
        bool zatenBekleyenTaslakVar = objectSpace.ModifiedObjects
            .OfType<IrsaliyeD>()
            .Any(d => d.KaynakSiparisD != null && siparisSatirOidleri.Contains(d.KaynakSiparisD.Oid));
        if (zatenBekleyenTaslakVar)
            throw new UserFriendlyException(
                "Bu sipariş için zaten kaydedilmemiş bir İrsaliye taslağı açık — lütfen önce onu kaydedin veya İptal ile kapatın.");

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
}
