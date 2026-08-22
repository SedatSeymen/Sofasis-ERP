/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : SayiyiYaziyaCeviriciTests.cs
 * Oluşturma Tarihi : 08/22/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/22/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : 22.08.2026 denetiminde bulunan kuruş yer-değeri ve kültür
 *                    bağımlılığı hatalarının (DUZELTME_GOREVLERI.md G5) regresyon
 *                    testleri. Testler tr-TR kültürünü açıkça set/restore eder —
 *                    girdi string'leri ("1500,50" gibi) o kültürün ondalık
 *                    ayıracına (virgül) bağlıdır, çalıştırma ortamının varsayılan
 *                    kültüründen bağımsız olmalıdır.
 * ****************************************************************************
 */

using System.Globalization;
using SofasisERP.Module.Services;

namespace SofasisERP.Module.Tests;

public class SayiyiYaziyaCeviriciTests
{
    static string TrTrIle(Func<string> aksiyon)
    {
        CultureInfo eski = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
        try { return aksiyon(); }
        finally { CultureInfo.CurrentCulture = eski; }
    }

    [Fact]
    public void TamKurusluTutar_DogruYaziyaCevrilir()
    {
        string sonuc = TrTrIle(() => SayiyiYaziyaCevirici.SayiyiYaziyaCevirVirgullu("1500,50", "TRY"));
        Assert.Equal("Bin Beş Yüz Türk Lirası Elli Kuruş", sonuc);
    }

    [Fact]
    public void TekHaneliOndalik_YerDegeriDogruOkunur()
    {
        // 22.08.2026 denetiminde bulunan hata: "1500,5" (0,5 TL = 50 kuruş) eskiden
        // ondalık kısım yer-değersiz okunduğu için "Beş Kuruş" (yanlış) veriyordu.
        string sonuc = TrTrIle(() => SayiyiYaziyaCevirici.SayiyiYaziyaCevirVirgullu("1500,5", "TRY"));
        Assert.Equal("Bin Beş Yüz Türk Lirası Elli Kuruş", sonuc);
    }

    [Fact]
    public void SadeceKurus_TamKisimSifirGosterilir()
    {
        string sonuc = TrTrIle(() => SayiyiYaziyaCevirici.SayiyiYaziyaCevirVirgullu("0,05", "TRY"));
        Assert.Equal("Sıfır Türk Lirası Beş Kuruş", sonuc);
    }

    [Fact]
    public void TamSayiliTutar_KurusEklenmez()
    {
        string sonuc = TrTrIle(() => SayiyiYaziyaCevirici.SayiyiYaziyaCevirVirgullu("1000", "TRY"));
        Assert.Equal("Bin Türk Lirası", sonuc);
    }

    [Fact]
    public void NegatifTutar_ExceptionAtmazEksiOnekiyleDoner()
    {
        // 22.08.2026 denetiminde bulunan hata: negatif tutar eskiden istisna ile
        // yutulup boş string dönüyordu.
        string sonuc = TrTrIle(() => SayiyiYaziyaCevirici.SayiyiYaziyaCevirVirgullu("-250,75", "TRY"));
        Assert.Equal("Eksi İki Yüz Elli Türk Lirası Yetmiş Beş Kuruş", sonuc);
    }

    [Fact]
    public void BuyukTutar_DogruGruplanir()
    {
        string sonuc = TrTrIle(() => SayiyiYaziyaCevirici.SayiyiYaziyaCevirVirgullu("123456,78", "USD"));
        Assert.Equal("Yüz Yirmi Üç Bin Dört Yüz Elli Altı Amerikan Doları Yetmiş Sekiz Cent", sonuc);
    }

    [Fact]
    public void ParaBirimiAdi_ToTitleCaseKullanilmaz()
    {
        // Eski kod ToTitleCase("TRY") -> "Try" basıyordu; artık tam adı kullanılıyor.
        string sonuc = TrTrIle(() => SayiyiYaziyaCevirici.SayiyiYaziyaCevirVirgullu("10", "TRY"));
        Assert.DoesNotContain("Try", sonuc);
        Assert.Contains("Türk Lirası", sonuc);
    }

    [Fact]
    public void FormatVeParseAyniKulturdeYapilirsaSonucKulturdenBagimsizDogruCikar()
    {
        // Gerçek çağıran (HareketMakbuzuReportBuilder) tutarı ÖNCE CurrentCulture ile
        // ToString'e çevirip SONRA bu metoda veriyor — hangi kültür ambient olursa
        // olsun (tr-TR, invariant, en-US...) format+parse AYNI kültürle yapıldığı
        // sürece sonuç doğru kalmalı. Eski koddaki sabit Split(',') bu round-trip'i
        // tr-TR DIŞINDAKİ kültürlerde bozuyordu (denetim §4.4) — artık bozmuyor.
        foreach (string kulturAdi in new[] { "tr-TR", "en-US", "" /* Invariant */ })
        {
            CultureInfo eski = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(kulturAdi);
            try
            {
                decimal tutar = 1500.50m;
                string bicimli = tutar.ToString(CultureInfo.CurrentCulture);
                string sonuc = SayiyiYaziyaCevirici.SayiyiYaziyaCevirVirgullu(bicimli, "TRY");
                Assert.Equal("Bin Beş Yüz Türk Lirası Elli Kuruş", sonuc);
            }
            finally { CultureInfo.CurrentCulture = eski; }
        }
    }
}
