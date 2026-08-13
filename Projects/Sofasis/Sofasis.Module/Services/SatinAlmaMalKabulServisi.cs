using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using Sofasis.Module.BusinessObjects;
using System;
using System.Linq;

namespace Sofasis.Module.Services;

public sealed class SatinAlmaMalKabulServisi : ISatinAlmaMalKabulServisi
{
    public StokHareketleriM MalKabulTaslagiOlustur(IObjectSpace objectSpace, SatinAlmaSiparisiM siparis)
    {
        if (siparis == null)
            throw new UserFriendlyException("Sipariş bulunamadı.");
        if (siparis.Durum == SatinAlmaSiparisDurumu.IptalEdildi)
            throw new UserFriendlyException("İptal edilmiş bir sipariş için Mal Kabul oluşturulamaz.");
        if (!siparis.SatinAlmaSiparisiDs.Any(x => x.KalanMiktar > 0))
            throw new UserFriendlyException("Bu siparişte teslim alınacak kalan miktar yok — tüm kalemler zaten teslim alınmış.");

        // ÇİFTE-TIKLAMA / MÜKERRER TASLAK GÜVENLİK AĞI: "Mal Kabul Oluştur" art arda iki kez
        // tetiklenirse (double-click, ya da UI katmanındaki korumanın kapsamadığı bir yol), AYNI
        // paylaşılan ObjectSpace'te bu siparişe ait, HENÜZ COMMIT EDİLMEMİŞ ikinci bir
        // StokHareketleriM taslağının oluşmasını engeller. Bu, "İptal hayalet kayıt" bug'ıyla
        // (bkz. PaylasimliObjectSpacePopupIptalTemizlikController) AYNI kök desenin (sahipsiz
        // "yeni" nesnenin başka bir commit'e sessizce sürüklenmesi) kaynağında kapatılmasıdır —
        // canlı testte kanıtlanan çifte-STSAGR bug'ının kök nedeni budur (bkz.
        // docs/CHANGELOG.md 2026-08-13, "çift-Kaydet-tıklama" bulgusu).
        var siparisSatirOidleri = siparis.SatinAlmaSiparisiDs.Select(x => x.Oid).ToHashSet();
        bool zatenBekleyenTaslakVar = objectSpace.ModifiedObjects
            .OfType<StokHareketleriD>()
            .Any(d => d.KaynakBelgeTipi == typeof(SatinAlmaSiparisiD)
                && d.KaynakBelgeOid.HasValue
                && siparisSatirOidleri.Contains(d.KaynakBelgeOid.Value));
        if (zatenBekleyenTaslakVar)
            throw new UserFriendlyException(
                "Bu sipariş için zaten kaydedilmemiş bir Mal Kabul taslağı açık — lütfen önce onu kaydedin veya İptal ile kapatın.");

        FisTuruTanim fisTuru = objectSpace.FindObject<FisTuruTanim>(
            CriteriaOperator.Parse("FisTuruKodu = 'STSAGR'"));
        if (fisTuru == null)
            throw new UserFriendlyException("STSAGR (Satın Alma Mal Kabul Girişi) fiş türü tanımlı değil — sistem yöneticinizle irtibata geçin.");

        StokHareketleriM malKabulFisi = objectSpace.CreateObject<StokHareketleriM>();
        malKabulFisi.FisTuruTanim = fisTuru;
        // DepoTanim BİLİNÇLİ OLARAK boş bırakılır — sipariş bir depoya bağlı değil, kullanıcı
        // dönen taslak ekranında hangi depoya teslim alındığını seçer (zaten RuleRequiredField
        // boş kaydetmeye izin vermez).

        var kalanSatirlar = siparis.SatinAlmaSiparisiDs.Where(x => x.KalanMiktar > 0).ToList();
        foreach (SatinAlmaSiparisiD siparisSatiri in kalanSatirlar)
        {
            StokHareketleriD stokSatiri = objectSpace.CreateObject<StokHareketleriD>();
            // Sıra kritik (bkz. StokHareketleriD.OnChanged): StokHareketleriM ÖNCE (DovizKuruGuncelle
            // bu olmadan sessizce çıkar), StokTanim SONRA (OnChanged'in miras aldığı StokTanim'in
            // KENDİ dövizi burada geçicidir, hemen aşağıda EXPLICIT olarak sipariş döviziyle ezilir),
            // BirimMaliyet EN SON (CalculateYerelMaliyet artık doğru DovizKuru ile çalışır).
            stokSatiri.StokHareketleriM = malKabulFisi;
            stokSatiri.StokTanim = siparisSatiri.StokTanim;
            stokSatiri.DovizTanim = siparis.DovizTanim;
            stokSatiri.Miktar = siparisSatiri.KalanMiktar;
            stokSatiri.BirimMaliyet = siparisSatiri.BirimFiyat;
            stokSatiri.KaynakBelgeTipi = typeof(SatinAlmaSiparisiD);
            stokSatiri.KaynakBelgeOid = siparisSatiri.Oid;
        }

        return malKabulFisi;
    }

    public void TeslimatiIsle(StokHareketleriM malKabulFisi)
    {
        if (malKabulFisi == null) return;

        foreach (StokHareketleriD satir in malKabulFisi.StokHareketleriDs
            .Where(x => x.KaynakBelgeTipi == typeof(SatinAlmaSiparisiD) && x.KaynakBelgeOid != null))
        {
            SatinAlmaSiparisiD siparisSatiri = malKabulFisi.Session.GetObjectByKey<SatinAlmaSiparisiD>(satir.KaynakBelgeOid);
            if (siparisSatiri == null) continue;

            // Kullanıcı Mal Kabul popup'ında satır miktarını elle artırabildiğinden (taslak
            // üzerinde serbestçe düzenlenebilir), burada üst sınır ZORUNLU — aksi halde
            // TeslimEdilenMiktar sipariş Miktar'ını aşıp KalanMiktar negatife düşebilir
            // (veri bütünlüğü ihlali, hiçbir yerde başka bir kontrolle yakalanmaz).
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
