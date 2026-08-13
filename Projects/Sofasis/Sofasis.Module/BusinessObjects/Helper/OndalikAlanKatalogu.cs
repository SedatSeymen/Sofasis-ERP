using System;
using System.Collections.Generic;
using System.Reflection;
using Sofasis.Module.DatabaseUpdate;

namespace Sofasis.Module.BusinessObjects;

// Sonek-tahmini (EndsWith("Tutar") vb.) yerine AÇIK, bakımı yapılan bir alan listesi — kullanıcı
// canlı testte "AlisFiyati" gibi hiçbir sonek kuralına uymayan bir alanın hiç formatlanmadığını
// yakaladı (bkz. docs/CHANGELOG.md). Sonek sezgisi her yeni alan adında tekrar kırılabilir; bu
// yüzden hem Module.cs'in ondalık-maski geçişi hem OndalikFormatController AYNI kaynaktan (bu CSV)
// okur — kaçırılan bir alan bulunduğunda tek satır eklenir (Resources/Seed/ondalik-alan-listesi.csv),
// kod değişmez.
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
    // (ör. SatisSiparisD.SatisSiparisM) miras alınır. Önce nesnenin kendi DovizTanim'ine bakılır,
    // yoksa nesnenin property'leri arasında DovizTanim'i olan İLK referans (bu projenin
    // master-detail konvansiyonuna göre her Detail'in tam olarak bir Master referansı vardır)
    // kullanılır. Hem DetailView/ListView format controller'ı (Sofasis.Module) hem canlı
    // (OnChange-anlık) format controller'ı (Sofasis.Blazor.Server) bu AYNI çözümlemeyi kullanır.
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
