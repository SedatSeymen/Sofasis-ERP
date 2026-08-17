/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : OndalikAlanKatalogu.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Miktar/Tutar/Yerel/Maliyet/Kur ondalık format kategorilerinin
 *                    AÇIK (sonek-tahmini değil) kaynağı — eski projeden aynen
 *                    uyarlandı. Yeni bir alan eklendiğinde tek satır CSV yeterli,
 *                    kod değişmez (Resources/Seed/ondalik-alan-listesi.csv).
 * ****************************************************************************
 */

using System.Reflection;
using SofasisERP.Module.DatabaseUpdate;

namespace SofasisERP.Module.BusinessObjects;

public class OndalikAlanRow
{
    public string Sinif { get; set; }
    public string Alan { get; set; }
    public string Kategori { get; set; }
}

public static class OndalikAlanKatalogu
{
    static readonly Lazy<Dictionary<(string Sinif, string Alan), string>> kategoriler = new(Yukle);

    public static IReadOnlyDictionary<(string Sinif, string Alan), string> Kategoriler => kategoriler.Value;

    static Dictionary<(string, string), string> Yukle()
    {
        var sonuc = new Dictionary<(string, string), string>();
        foreach (OndalikAlanRow satir in SeedCsvReader.Read<OndalikAlanRow>("ondalik-alan-listesi.csv"))
            sonuc[(satir.Sinif, satir.Alan)] = satir.Kategori;
        return sonuc;
    }

    // Satır (Detail) sınıfları çoğunlukla kendi DovizTanim'ini taşımaz — para birimi Master'dan
    // miras alınır. Önce nesnenin kendi DovizTanim'ine bakılır, yoksa nesnenin property'leri
    // arasında DovizTanim'i olan İLK referans kullanılır. Hem DetailView/ListView format
    // controller'ı (Module) hem canlı (OnChange-anlık) format controller'ı (Blazor.Server) bu
    // AYNI çözümlemeyi kullanır.
    public static DovizTanim DovizTaniminiCoz(object nesne, Type tip)
    {
        PropertyInfo dogrudan = tip.GetProperty("DovizTanim");
        if (dogrudan != null && dogrudan.PropertyType == typeof(DovizTanim))
            return dogrudan.GetValue(nesne) as DovizTanim;

        foreach (PropertyInfo prop in tip.GetProperties())
        {
            PropertyInfo masterDoviz = prop.PropertyType.GetProperty("DovizTanim");
            if (masterDoviz == null || masterDoviz.PropertyType != typeof(DovizTanim))
                continue;
            object master = prop.GetValue(nesne);
            if (master != null)
                return masterDoviz.GetValue(master) as DovizTanim;
        }
        return null;
    }
}
