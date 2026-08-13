using System;

namespace Sofasis.Module.Services;

public sealed class VatCalculator : IVatCalculator
{
    public FaturaSatirTutarlari Hesapla(decimal miktar, decimal birimFiyat, decimal kdvOrani, decimal? tevkifatOrani)
    {
        decimal kdvHaricTutar = Math.Round(miktar * birimFiyat, 2, MidpointRounding.AwayFromZero);
        decimal kdvTutar = Math.Round(kdvHaricTutar * kdvOrani / 100m, 2, MidpointRounding.AwayFromZero);
        decimal tevkifatTutar = tevkifatOrani.GetValueOrDefault() > 0
            ? Math.Round(kdvTutar * tevkifatOrani.Value / 100m, 2, MidpointRounding.AwayFromZero)
            : 0m;
        decimal netTutar = kdvHaricTutar + kdvTutar - tevkifatTutar;

        return new FaturaSatirTutarlari(kdvHaricTutar, kdvTutar, tevkifatTutar, netTutar);
    }
}
