using System;
using System.Collections.Generic;

namespace Sofasis.Module.Services;

public interface IDovizKuruService
{
    // Verilen tarihe ait TCMB döviz kurlarını çeker. Ağ/parse hatasında boş liste döner
    // (istisna fırlatmaz) — çağıran taraf boş listeyi "bu sefer güncellenemedi" olarak yorumlar.
    IReadOnlyList<DovizKuruDto> KurlariCek(DateTime tarih);
}

public sealed class DovizKuruDto
{
    public string DovizKodu { get; init; }
    public decimal DovizAlis { get; init; }
    public decimal DovizSatis { get; init; }
    public decimal EfektifAlis { get; init; }
    public decimal EfektifSatis { get; init; }
}
