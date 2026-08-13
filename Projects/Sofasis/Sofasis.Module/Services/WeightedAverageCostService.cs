using System;
using System.Collections.Generic;

namespace Sofasis.Module.Services;

// Hareketli ağırlıklı ortalama maliyet motoru (ADR-005). Session/XPO'dan tamamen bağımsız, saf
// fonksiyonlardan oluşur — bu sayede gerçek bir veritabanı olmadan birim test edilebilir
// (Sofasis.Module.Tests/Services/WeightedAverageCostServiceTests.cs). Çağıran: StokHareketleriD
// (StokHareketleriM/D master-detail çiftinin satır sınıfı, motor orkestrasyonunun tamamı orada).
public sealed class WeightedAverageCostService : IWeightedAverageCostService
{
    public (decimal YeniMiktar, decimal YeniOrtalamaMaliyet) GirisUygula(
        decimal mevcutMiktar, decimal mevcutOrtalamaMaliyet, decimal girisMiktari, decimal girisBirimMaliyet)
    {
        if (girisMiktari <= 0)
            throw new ArgumentOutOfRangeException(nameof(girisMiktari), "Giriş miktarı sıfırdan büyük olmalıdır.");

        decimal yeniMiktar = mevcutMiktar + girisMiktari;
        decimal yeniOrtalama = Math.Round(
            ((mevcutMiktar * mevcutOrtalamaMaliyet) + (girisMiktari * girisBirimMaliyet)) / yeniMiktar,
            6, MidpointRounding.AwayFromZero);

        return (yeniMiktar, yeniOrtalama);
    }

    public (decimal YeniMiktar, decimal YeniOrtalamaMaliyet) CikisUygula(
        decimal mevcutMiktar, decimal mevcutOrtalamaMaliyet, decimal cikisMiktari)
    {
        if (cikisMiktari <= 0)
            throw new ArgumentOutOfRangeException(nameof(cikisMiktari), "Çıkış miktarı sıfırdan büyük olmalıdır.");

        // Ortalama maliyet ÇIKIŞTA DEĞİŞMEZ (ADR-005) — negatif stok politikası (izin ver/uyar/
        // engelle) bu servisin değil, çağıranın (StokHareketleriD.ObjectSaving + StokParametre)
        // sorumluluğudur; burada miktarın negatife düşmesi engellenmez.
        return (mevcutMiktar - cikisMiktari, mevcutOrtalamaMaliyet);
    }

    public (decimal Miktar, decimal OrtalamaMaliyet) YenidenHesapla(
        IEnumerable<(bool Giris, decimal Miktar, decimal BirimMaliyet)> hareketlerKayitSirasiyla)
    {
        decimal miktar = 0;
        decimal ortalama = 0;

        foreach (var hareket in hareketlerKayitSirasiyla)
        {
            if (hareket.Giris)
                (miktar, ortalama) = GirisUygula(miktar, ortalama, hareket.Miktar, hareket.BirimMaliyet);
            else
                (miktar, ortalama) = CikisUygula(miktar, ortalama, hareket.Miktar);
        }

        return (miktar, ortalama);
    }
}
