/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : SeedCsvReader.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Gömülü (embedded) CSV seed dosyalarını okuyan ortak yardımcı.
 *                    Dosyalar: Resources/Seed/*.csv ('; ' ayraçlı, UTF-8).
 *                    Eski projeden aynen uyarlandı (bkz. docs/architecture/
 *                    00_DetailView_ve_Servis_Konvansiyonlari.md §3).
 * ****************************************************************************
 */

using System.Globalization;
using System.Reflection;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace SofasisERP.Module.DatabaseUpdate;

public static class SeedCsvReader
{
    // Örnek: SeedCsvReader.Read<OndalikAlanRow>("ondalik-alan-listesi.csv")
    public static List<T> Read<T>(string csvFileName)
    {
        Assembly asm = typeof(SeedCsvReader).Assembly;

        string resourceName = asm.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(".Seed." + csvFileName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Seed CSV gömülü kaynağı bulunamadı: {csvFileName}");

        using Stream stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Seed CSV akışı açılamadı: {resourceName}");
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim()
        };

        using var csv = new CsvReader(reader, config);
        return csv.GetRecords<T>().ToList();
    }
}
