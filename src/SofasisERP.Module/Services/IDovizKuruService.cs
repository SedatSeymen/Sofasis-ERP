/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : IDovizKuruService.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Döviz kuru kaynağı soyutlaması (eski projeden uyarlandı).
 * ****************************************************************************
 */

namespace SofasisERP.Module.Services;

public interface IDovizKuruService
{
    // Verilen tarihe ait TCMB döviz kurlarını çeker. Ağ/parse hatasında boş liste döner
    // (istisna fırlatmaz) — çağıran taraf boş listeyi "bu sefer güncellenemedi" olarak yorumlar.
    Task<IReadOnlyList<DovizKuruDto>> KurlariCekAsync(DateTime tarih);
}

public sealed class DovizKuruDto
{
    public string DovizKodu { get; init; }
    public decimal DovizAlis { get; init; }
    public decimal DovizSatis { get; init; }
    public decimal EfektifAlis { get; init; }
    public decimal EfektifSatis { get; init; }
}
