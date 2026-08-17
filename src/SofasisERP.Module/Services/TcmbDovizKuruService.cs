/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : TcmbDovizKuruService.cs
 * Oluşturma Tarihi : 08/17/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/17/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : TCMB günlük kur XML'ini okuyup ayrıştırır (eski projeden aynen).
 * ****************************************************************************
 */

using System.Globalization;
using System.Xml.Linq;

namespace SofasisERP.Module.Services;

// TCMB'nin günlük yayınladığı XML kur listesini okuyup ayrıştırır. Sorumluluğu
// yalnızca "veriyi çek ve ayrıştır" ile sınırlıdır; DB'ye yazma işini
// DovizKuruGuncellemeServisi yapar — test edilebilirlik için ayrıldı.
public sealed class TcmbDovizKuruService : IDovizKuruService
{
    public IReadOnlyList<DovizKuruDto> KurlariCek(DateTime tarih)
    {
        string url = tarih.Date == DateTime.UtcNow.Date
            ? "https://www.tcmb.gov.tr/kurlar/today.xml"
            : $"https://www.tcmb.gov.tr/kurlar/{tarih:yyyyMM}/{tarih:ddMMyyyy}.xml";

        try
        {
            XDocument document = XDocument.Load(url);
            return document.Root?
                .Elements("Currency")
                .Select(ToDto)
                .Where(dto => dto != null)
                .ToList()
                ?? new List<DovizKuruDto>();
        }
        catch (Exception)
        {
            // Ağ erişimi yok / TCMB o gün için henüz yayınlamadı / XML formatı değişti:
            // sessizce boş liste dön, çağıran taraf bir sonraki denemede tekrar dener.
            return Array.Empty<DovizKuruDto>();
        }
    }

    static DovizKuruDto ToDto(XElement currency)
    {
        string kod = currency.Attribute("Kod")?.Value;
        if (string.IsNullOrWhiteSpace(kod))
        {
            return null;
        }

        return new DovizKuruDto
        {
            DovizKodu = kod,
            DovizAlis = Parse(currency.Element("ForexBuying")?.Value),
            DovizSatis = Parse(currency.Element("ForexSelling")?.Value),
            EfektifAlis = Parse(currency.Element("BanknoteBuying")?.Value),
            EfektifSatis = Parse(currency.Element("BanknoteSelling")?.Value),
        };
    }

    // TCMB bazı satırlarda (ör. Efektif Alış) değeri boş bırakabilir; bu durumda 0 kabul edilir.
    static decimal Parse(string deger) =>
        decimal.TryParse(deger, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal sonuc) ? sonuc : 0m;
}
