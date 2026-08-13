using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Xpo;
using Sofasis.Module.BusinessObjects;
using Sofasis.Module.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sofasis.Module.Services;

public sealed class FaturaKaydetServisi : IFaturaKaydetServisi
{
    public FaturaM FaturaTaslagiOlustur(IObjectSpace objectSpace, IrsaliyeM irsaliye)
    {
        if (irsaliye == null)
            throw new UserFriendlyException("İrsaliye bulunamadı.");
        if (irsaliye.FisTuruTanim?.FisTuruKodu != "IRALIS")
            throw new UserFriendlyException("Yalnızca Alış İrsaliyelerinden fatura oluşturulabilir.");

        StokHareketleriM malKabulFisi = irsaliye.StokHareketleriM;
        if (malKabulFisi == null)
            throw new UserFriendlyException("İrsaliyenin bağlı bir stok hareket fişi bulunamadı.");

        // Performans: FaturaD tablosunun TAMAMINI çekmek yerine (tüm faturalar, tüm tedarikçiler)
        // yalnızca BU İrsaliyenin (stok hareket fişinin) satır Oid'leriyle sınırlı bir IN sorgusu
        // SQL'e itilir — sonuç kümesi bu fişin satır sayısıyla sınırlı kalır.
        List<Guid> malKabulSatirOidleri = malKabulFisi.StokHareketleriDs.Select(x => x.Oid).ToList();
        HashSet<Guid> faturalananSatirOidleri = new XPQuery<FaturaD>(malKabulFisi.Session)
            .Where(f => f.KaynakStokHareketiD != null && malKabulSatirOidleri.Contains(f.KaynakStokHareketiD.Oid))
            .Select(f => f.KaynakStokHareketiD.Oid)
            .ToHashSet();

        // ÇİFTE-TIKLAMA GÜVENLİK AĞI: yukarıdaki sorgu yalnızca DB'ye zaten YAZILMIŞ FaturaD'leri
        // görür — "Fatura Oluştur" art arda iki kez tetiklenirse, PAYLAŞILAN ObjectSpace'te henüz
        // commit edilmemiş bir önceki taslağın satırları bu sorguda GÖRÜNMEZ. Aynı kök nedenli
        // çifte-belge riskine karşı (bkz. SatinAlmaIrsaliyeServisi.IrsaliyeTaslagiOlustur,
        // docs/CHANGELOG.md 2026-08-13), ObjectSpace'teki BEKLEYEN (henüz kaydedilmemiş) FaturaD'ler
        // de dahil edilir — ikinci tetiklemede faturalanacakSatirlar boş çıkar, mevcut "zaten
        // faturalanmış" hatası devreye girer.
        foreach (FaturaD bekleyen in objectSpace.ModifiedObjects.OfType<FaturaD>())
            if (bekleyen.KaynakStokHareketiD != null)
                faturalananSatirOidleri.Add(bekleyen.KaynakStokHareketiD.Oid);

        var faturalanacakSatirlar = malKabulFisi.StokHareketleriDs
            .Where(x => !faturalananSatirOidleri.Contains(x.Oid))
            .ToList();
        if (faturalanacakSatirlar.Count == 0)
            throw new UserFriendlyException("Bu İrsaliyenin tüm satırları zaten faturalanmış.");

        // Kaynak Sipariş artık doğrudan İrsaliye'den okunur — İrsaliye ayrı bir iş nesnesi olarak
        // kurulmadan önce (bkz. docs/CHANGELOG.md 2026-08-14) bu bilgi StokHareketleriD.KaynakBelgeOid
        // üzerinden geriye izlenerek bulunuyordu; artık gerek yok.
        SatinAlmaSiparisiM siparis = irsaliye.KaynakSiparis;
        if (siparis == null)
            throw new UserFriendlyException("İrsaliyenin kaynak siparişi bulunamadı — fatura otomatik oluşturulamıyor.");

        FisTuruTanim fisTuru = objectSpace.FindObject<FisTuruTanim>(
            CriteriaOperator.Parse("FisTuruKodu = 'FAALIS'"));
        if (fisTuru == null)
            throw new UserFriendlyException("FAALIS (Alış Faturası) fiş türü tanımlı değil — sistem yöneticinizle irtibata geçin.");

        KDVTanim varsayilanKdv = siparis.Session.GetVarsayilan<KDVTanim>();

        FaturaM fatura = objectSpace.CreateObject<FaturaM>();
        fatura.FisTuruTanim = fisTuru;
        fatura.CariHesap = siparis.Tedarikci;
        // Fatura, SİPARİŞİN dövizinde gelir — Tedarikçi'nin kendi varsayılan dövizinden farklı
        // olabilir (ör. sipariş özel olarak USD ile verilmiş olabilir). CariHesap setter'ının
        // OnChanged'i miras aldığı değeri burada EXPLICIT olarak sipariş döviziyle eziyoruz —
        // StokHareketleriD'deki (StokTanim -> sipariş dövizi) aynı düzeltme deseni.
        fatura.DovizTanim = siparis.DovizTanim;
        fatura.KaynakSiparisTipi = typeof(SatinAlmaSiparisiM);
        fatura.KaynakSiparisOid = siparis.Oid;
        fatura.KaynakIrsaliye = irsaliye;

        foreach (StokHareketleriD satir in faturalanacakSatirlar)
        {
            SatinAlmaSiparisiD siparisSatiri = satir.KaynakBelgeOid.HasValue
                ? objectSpace.GetObjectByKey<SatinAlmaSiparisiD>(satir.KaynakBelgeOid.Value)
                : null;

            FaturaD satirFatura = objectSpace.CreateObject<FaturaD>();
            satirFatura.FaturaM = fatura;
            satirFatura.StokTanim = satir.StokTanim;
            satirFatura.Miktar = satir.Miktar;
            // Fatura fiyatı varsayılan olarak sipariş fiyatından doldurulur — kullanıcı tedarikçinin
            // gerçek faturasına göre elle düzeltebilir (3'lü eşleştirme bu sapmayı SONRADAN,
            // FaturaKilitleController öncesi kaydet anında kontrol eder — bkz. plan notu; v1.5
            // kapsamında UcluEslestirmePolitikasi henüz TÜKETİLMİYOR, şema hazır).
            satirFatura.BirimFiyat = siparisSatiri?.BirimFiyat ?? satir.BirimMaliyet;
            satirFatura.KaynakStokHareketiD = satir;
            satirFatura.KDVTanim = varsayilanKdv;
        }

        return fatura;
    }
}
