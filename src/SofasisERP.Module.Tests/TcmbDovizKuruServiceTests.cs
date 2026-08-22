/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : TcmbDovizKuruServiceTests.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : TcmbDovizKuruService.XmlAyristir için saf mantık testleri
 *                    (22.08.2026 denetimi G23) — örnek bir TCMB günlük kur XML'i
 *                    ile ayrıştırma doğrulanır. HTTP çağrısı test edilmez (o kısım
 *                    saf değil, dış servise bağımlı).
 * ****************************************************************************
 */

using SofasisERP.Module.Services;

namespace SofasisERP.Module.Tests;

public class TcmbDovizKuruServiceTests
{
    const string OrnekTcmbXml = """
        <Tarih_Date Tarih="22.08.2026" Date="08/22/2026" Bulten_No="2026/163">
          <Currency CrossOrder="0" Kod="USD" CurrencyCode="USD">
            <Unit>1</Unit>
            <Isim>ABD DOLARI</Isim>
            <CurrencyName>US DOLLAR</CurrencyName>
            <ForexBuying>34.1234</ForexBuying>
            <ForexSelling>34.2345</ForexSelling>
            <BanknoteBuying>34.1000</BanknoteBuying>
            <BanknoteSelling>34.2600</BanknoteSelling>
          </Currency>
          <Currency CrossOrder="0" Kod="EUR" CurrencyCode="EUR">
            <Unit>1</Unit>
            <Isim>EURO</Isim>
            <CurrencyName>EURO</CurrencyName>
            <ForexBuying>37.5000</ForexBuying>
            <ForexSelling>37.6200</ForexSelling>
            <BanknoteBuying></BanknoteBuying>
            <BanknoteSelling></BanknoteSelling>
          </Currency>
        </Tarih_Date>
        """;

    [Fact]
    public void XmlAyristir_OrnekTcmbXml_TumDovizleriDogruOkur()
    {
        var sonuc = TcmbDovizKuruService.XmlAyristir(OrnekTcmbXml);

        Assert.Equal(2, sonuc.Count);

        DovizKuruDto usd = sonuc.Single(x => x.DovizKodu == "USD");
        Assert.Equal(34.1234m, usd.DovizAlis);
        Assert.Equal(34.2345m, usd.DovizSatis);
        Assert.Equal(34.1000m, usd.EfektifAlis);
        Assert.Equal(34.2600m, usd.EfektifSatis);
    }

    [Fact]
    public void XmlAyristir_BosEfektifDeger_SifirKabulEdilir()
    {
        // TCMB bazı satırlarda (ör. Efektif Alış/Satış) değeri boş bırakır.
        var sonuc = TcmbDovizKuruService.XmlAyristir(OrnekTcmbXml);

        DovizKuruDto eur = sonuc.Single(x => x.DovizKodu == "EUR");
        Assert.Equal(0m, eur.EfektifAlis);
        Assert.Equal(0m, eur.EfektifSatis);
    }

    [Fact]
    public void XmlAyristir_KodEksikSatir_Atlanir()
    {
        const string xml = """
            <Tarih_Date>
              <Currency>
                <ForexSelling>10.00</ForexSelling>
              </Currency>
            </Tarih_Date>
            """;

        var sonuc = TcmbDovizKuruService.XmlAyristir(xml);

        Assert.Empty(sonuc);
    }

    [Fact]
    public void XmlAyristir_BosKokEleman_BosListeDoner()
    {
        const string xml = "<Tarih_Date></Tarih_Date>";

        var sonuc = TcmbDovizKuruService.XmlAyristir(xml);

        Assert.Empty(sonuc);
    }
}
