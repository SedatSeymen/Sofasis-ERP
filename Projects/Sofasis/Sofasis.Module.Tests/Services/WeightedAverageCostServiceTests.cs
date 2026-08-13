using Sofasis.Module.Services;

namespace Sofasis.Module.Tests.Services;

public class WeightedAverageCostServiceTests
{
    readonly IWeightedAverageCostService sut = new WeightedAverageCostService();

    [Fact]
    public void GirisUygula_SifirdanIlkGiris_OrtalamaGirisMaliyetineEsitlenir()
    {
        var (miktar, ortalama) = sut.GirisUygula(mevcutMiktar: 0, mevcutOrtalamaMaliyet: 0, girisMiktari: 100, girisBirimMaliyet: 10m);

        Assert.Equal(100, miktar);
        Assert.Equal(10m, ortalama);
    }

    [Fact]
    public void GirisUygula_SiraliIkinciGiris_OrtalamaAgirlikliOlarakYenidenHesaplanir()
    {
        // 100 adet @10 zaten mevcut; 50 adet @13 girişi -> (100*10 + 50*13) / 150 = 11.0
        var (miktar, ortalama) = sut.GirisUygula(mevcutMiktar: 100, mevcutOrtalamaMaliyet: 10m, girisMiktari: 50, girisBirimMaliyet: 13m);

        Assert.Equal(150, miktar);
        Assert.Equal(11m, ortalama);
    }

    [Fact]
    public void CikisUygula_OrtalamayiDegistirmez()
    {
        var (miktar, ortalama) = sut.CikisUygula(mevcutMiktar: 150, mevcutOrtalamaMaliyet: 11m, cikisMiktari: 60);

        Assert.Equal(90, miktar);
        Assert.Equal(11m, ortalama);
    }

    [Fact]
    public void CikisUygula_MevcutMiktariAsarsaNegatifBakiyeUretir_HataFirlatmaz()
    {
        // Negatif stok politikası (izin ver/uyar/engelle) bu servisin değil çağıranın işi;
        // servis saf matematiği uygular, negatif sonucu engellemez.
        var (miktar, ortalama) = sut.CikisUygula(mevcutMiktar: 10, mevcutOrtalamaMaliyet: 5m, cikisMiktari: 15);

        Assert.Equal(-5, miktar);
        Assert.Equal(5m, ortalama);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GirisUygula_SifirVeyaNegatifMiktar_HataFirlatir(decimal girisMiktari)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.GirisUygula(mevcutMiktar: 0, mevcutOrtalamaMaliyet: 0, girisMiktari: girisMiktari, girisBirimMaliyet: 10m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CikisUygula_SifirVeyaNegatifMiktar_HataFirlatir(decimal cikisMiktari)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.CikisUygula(mevcutMiktar: 100, mevcutOrtalamaMaliyet: 10m, cikisMiktari: cikisMiktari));
    }

    [Fact]
    public void GirisUygula_TekrarEdenOndalik_AltiHaneyeYuvarlar()
    {
        // (10*1 + 3*1)/13 = 1.0 tam; farklı bir tekrarlayan ondalık senaryosu:
        // (1*10 + 1*3.333333...)/2 -> 6.666666... -> 6 haneye yuvarlanmalı
        var (_, ortalama) = sut.GirisUygula(mevcutMiktar: 1, mevcutOrtalamaMaliyet: 10m, girisMiktari: 2, girisBirimMaliyet: 3.3333333m);

        Assert.Equal(5.555556m, ortalama);
    }

    [Fact]
    public void YenidenHesapla_GirisVeCikisZincirlemesiyleAyniSonucuVerir()
    {
        var hareketler = new (bool Giris, decimal Miktar, decimal BirimMaliyet)[]
        {
            (true, 100, 10m),
            (true, 50, 13m),
            (false, 60, 0m), // çıkışta birim maliyet parametresi kullanılmaz
        };

        var (miktar, ortalama) = sut.YenidenHesapla(hareketler);

        // Elle zincirleme: Giris(0,0,100,10)->(100,10); Giris(100,10,50,13)->(150,11); Cikis(150,11,60)->(90,11)
        Assert.Equal(90, miktar);
        Assert.Equal(11m, ortalama);
    }
}
