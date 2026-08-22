/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : WeightedAverageCostService.cs
 * Oluşturma Tarihi : 08/18/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/18/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : IWeightedAverageCostService implementasyonu.
 * ****************************************************************************
 */

namespace SofasisERP.Module.Services;

public sealed class WeightedAverageCostService : IWeightedAverageCostService
{
    public (decimal YeniMiktar, decimal YeniOrtalama) GirisUygula(
        decimal eskiMiktar, decimal eskiOrtalama, decimal girisMiktari, decimal girisBirimMaliyeti)
    {
        // eskiMiktar <= 0: stok zaten sifir ya da negatif (bozuk) durumda -- gecmis
        // ortalama anlamini yitirmis demektir. yeniMiktar <= 0: giris eski negatif
        // bakiyeyi tam sifira getirdi ya da hala eksi biraktı; her iki durumda da
        // bolme (yeniMiktar==0 ise DivideByZeroException) yerine yeni girisin maliyeti
        // taban alinir (22.08.2026 denetimi G2).
        decimal yeniMiktar = eskiMiktar + girisMiktari;
        decimal yeniOrtalama = (eskiMiktar <= 0 || yeniMiktar <= 0)
            ? girisBirimMaliyeti
            : ((eskiMiktar * eskiOrtalama) + (girisMiktari * girisBirimMaliyeti)) / yeniMiktar;
        return (yeniMiktar, yeniOrtalama);
    }

    public (decimal YeniMiktar, decimal AyniOrtalama) CikisUygula(
        decimal eskiMiktar, decimal eskiOrtalama, decimal cikisMiktari)
    {
        return (eskiMiktar - cikisMiktari, eskiOrtalama);
    }

    public (decimal ToplamMiktar, decimal Ortalama) YenidenHesapla(
        IEnumerable<(bool Giris, decimal Miktar, decimal BirimMaliyet)> hareketler)
    {
        decimal miktar = 0;
        decimal ortalama = 0;
        foreach (var hareket in hareketler)
        {
            (miktar, ortalama) = hareket.Giris
                ? GirisUygula(miktar, ortalama, hareket.Miktar, hareket.BirimMaliyet)
                : CikisUygula(miktar, ortalama, hareket.Miktar);
        }
        return (miktar, ortalama);
    }
}
