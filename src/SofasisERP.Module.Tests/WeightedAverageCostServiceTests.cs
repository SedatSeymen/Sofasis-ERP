/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : WeightedAverageCostServiceTests.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : 4-ajanlı testte (2026-08-22, yazılım mühendisi bulgusu) tespit
 *                    edildi: ağırlıklı ortalama maliyet motoru gibi finansal olarak
 *                    kritik, framework bağımsız (test maliyeti sıfıra yakın) bir
 *                    servis hiç test edilmeden duruyordu. Bu proje SofasisERP'nin
 *                    ilk otomatik test projesidir.
 * ****************************************************************************
 */

using SofasisERP.Module.Services;

namespace SofasisERP.Module.Tests;

public class WeightedAverageCostServiceTests
{
    readonly IWeightedAverageCostService servis = new WeightedAverageCostService();

    [Fact]
    public void GirisUygula_IlkGiris_OrtalamaGirisBirimMaliyetineEsitlenir()
    {
        (decimal yeniMiktar, decimal yeniOrtalama) = servis.GirisUygula(
            eskiMiktar: 0, eskiOrtalama: 0, girisMiktari: 10, girisBirimMaliyeti: 25m);

        Assert.Equal(10, yeniMiktar);
        Assert.Equal(25m, yeniOrtalama);
    }

    [Fact]
    public void GirisUygula_IkinciGiris_AgirlikliOrtalamaHesaplanir()
    {
        // 10 adet @ 20 TL zaten var; 10 adet @ 30 TL daha giriyor.
        // Beklenen ortalama: (10*20 + 10*30) / 20 = 25.
        (decimal yeniMiktar, decimal yeniOrtalama) = servis.GirisUygula(
            eskiMiktar: 10, eskiOrtalama: 20m, girisMiktari: 10, girisBirimMaliyeti: 30m);

        Assert.Equal(20, yeniMiktar);
        Assert.Equal(25m, yeniOrtalama);
    }

    [Fact]
    public void GirisUygula_FarkliMiktarlarda_AgirlikliOrtalamaMiktaraGoreKaymali()
    {
        // 100 adet @ 10 TL; 10 adet @ 100 TL giriyor (küçük miktar, büyük fiyat).
        // Beklenen: (100*10 + 10*100) / 110 = 2000/110.
        (decimal yeniMiktar, decimal yeniOrtalama) = servis.GirisUygula(
            eskiMiktar: 100, eskiOrtalama: 10m, girisMiktari: 10, girisBirimMaliyeti: 100m);

        Assert.Equal(110, yeniMiktar);
        Assert.Equal(2000m / 110m, yeniOrtalama);
    }

    [Fact]
    public void CikisUygula_MiktarAzalirOrtalamaDegismez()
    {
        (decimal yeniMiktar, decimal ayniOrtalama) = servis.CikisUygula(
            eskiMiktar: 50, eskiOrtalama: 42.5m, cikisMiktari: 20);

        Assert.Equal(30, yeniMiktar);
        Assert.Equal(42.5m, ayniOrtalama);
    }

    [Fact]
    public void CikisUygula_MiktarSifiraDusebilir()
    {
        (decimal yeniMiktar, decimal ayniOrtalama) = servis.CikisUygula(
            eskiMiktar: 15, eskiOrtalama: 5m, cikisMiktari: 15);

        Assert.Equal(0, yeniMiktar);
        Assert.Equal(5m, ayniOrtalama);
    }

    [Fact]
    public void YenidenHesapla_BosListe_SifirDoner()
    {
        (decimal toplamMiktar, decimal ortalama) = servis.YenidenHesapla(
            Array.Empty<(bool, decimal, decimal)>());

        Assert.Equal(0, toplamMiktar);
        Assert.Equal(0, ortalama);
    }

    [Fact]
    public void YenidenHesapla_KaristirilmisGirisCikisSirasi_AdimAdimUygulamaylaAyniSonucuUretir()
    {
        // Sırayla: +10@20, +10@30 (ortalama 25), -5 (ortalama 25 kalır, miktar 15),
        // +15@40 (ortalama (15*25+15*40)/30 = 32.5).
        var hareketler = new (bool Giris, decimal Miktar, decimal BirimMaliyet)[]
        {
            (true, 10, 20m),
            (true, 10, 30m),
            (false, 5, 0m),
            (true, 15, 40m),
        };

        (decimal toplamMiktar, decimal ortalama) = servis.YenidenHesapla(hareketler);

        Assert.Equal(30, toplamMiktar);
        Assert.Equal(32.5m, ortalama);
    }

    [Fact]
    public void YenidenHesapla_TekBirSatirSilindiktenSonraki_ReplaySonucuDogru()
    {
        // Orijinal sıra: +10@10, +10@20, +5@50, -8. Ortadaki (+10@20) satırı SİLİNİYOR
        // (kullanıcı yanlış girmiş) — kalanlar CreatedDate sırasıyla yeniden uygulanır:
        // +10@10, +5@50, -8. Beklenen: miktar=7, ortalama=(10*10+5*50)/15=350/15.
        var kalanHareketler = new (bool Giris, decimal Miktar, decimal BirimMaliyet)[]
        {
            (true, 10, 10m),
            (true, 5, 50m),
            (false, 8, 0m),
        };

        (decimal toplamMiktar, decimal ortalama) = servis.YenidenHesapla(kalanHareketler);

        Assert.Equal(7, toplamMiktar);
        Assert.Equal(350m / 15m, ortalama);
    }
}
