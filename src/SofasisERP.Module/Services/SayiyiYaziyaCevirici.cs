/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : SayiyiYaziyaCevirici.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Ondalıklı bir parasal tutarı Türkçe yazıya çevirir (fiş/makbuz
 *                    raporlarındaki "Yalnız: ... Türk Lirası" satırı için) — eski
 *                    ERP'nin Helper.SayiyiYaziyaCevir/SayiyiYaziyaCevirVirgullu
 *                    metodlarından birebir taşındı.
 * ****************************************************************************
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SofasisERP.Module.Services;

public static class SayiyiYaziyaCevirici
{
    static string SayiyiYaziyaCevir(string sayi)
    {
        const int maxBasamakSayisi = 18;

        string[] birler = { "", "Bir", "İki", "Üç", "Dört", "Beş", "Altı", "Yedi", "Sekiz", "Dokuz" };
        string[] onlar = { "", "On", "Yirmi", "Otuz", "Kırk", "Elli", "Altmış", "Yetmiş", "Seksen", "Doksan" };
        string[] binler = { "Katrilyon", "Trilyon", "Milyar", "Milyon", "Bin", "" };

        int[] bas = new int[3];
        List<string> gruplar = new();
        sayi = sayi.PadLeft(maxBasamakSayisi, '0');
        for (int i = 0; i <= maxBasamakSayisi / 3 - 1; i++)
        {
            bas[0] = int.Parse(sayi.Substring(i * 3, 1));
            bas[1] = int.Parse(sayi.Substring(i * 3 + 1, 1));
            bas[2] = int.Parse(sayi.Substring(i * 3 + 2, 1));

            string yuzKismi = bas[0] == 0 ? "" : bas[0] == 1 ? "Yüz" : $"{birler[bas[0]]} Yüz";
            string grup = string.Join(" ", new[] { yuzKismi, onlar[bas[1]], birler[bas[2]] }.Where(p => p != ""));

            if (grup == "") continue;

            // "Bin" grubunda değer tam "Bir" ise "Bir Bin" değil sade "Bin" denir (Türkçe kural).
            if (binler[i] == "Bin" && grup == "Bir")
                gruplar.Add("Bin");
            else if (binler[i] != "")
                gruplar.Add($"{grup} {binler[i]}");
            else
                gruplar.Add(grup);
        }
        return gruplar.Count == 0 ? "Sıfır" : string.Join(" ", gruplar);
    }

    static string ParaBirimiAdi(string paraBirimi) => paraBirimi.ToUpper(CultureInfo.InvariantCulture) switch
    {
        "TRY" => "Türk Lirası",
        "USD" => "Amerikan Doları",
        "EUR" => "Euro",
        _ => paraBirimi
    };

    static string OndalikParaBirimi(string paraBirimi) => paraBirimi.ToUpper(CultureInfo.InvariantCulture) switch
    {
        "TRY" => "Kuruş",
        "USD" => "Cent",
        "EUR" => "Cent",
        _ => ""
    };

    // CultureInfo.CurrentCulture ile biçimlendirilmiş bir tutar metni (tr-TR: "," ondalık,
    // "." binlik ayıracı) bekler — InvariantCulture ("1500.00") verilirse "." binlik ayıracı
    // sanılıp 1500 yanlışlıkla 15.000.000'a dönüşür (eski ERP'de kanıtlanmış hata, bkz. dosya
    // başı açıklama).
    //
    // 22.08.2026 denetiminde bulunan iki hata düzeltildi (DUZELTME_GOREVLERI.md G5):
    // (a) Math.Round(...,3) yerine parasal 2 hane; (b) ondalık kısım string olarak
    // Split(',') ile okunuyordu (yer-değeri kayboluyordu: "1500,5" → "5" → "Beş Kuruş"
    // yerine olması gereken "Elli Kuruş") — artık decimal aritmetiğiyle (deger-tamKisim)*100
    // hesaplanıyor, string round-trip yok. Negatif tutar artık istisna ile boş dönmüyor,
    // "Eksi ..." öneki ile normal yoldan işleniyor.
    public static string SayiyiYaziyaCevirVirgullu(string tutar, string paraBirimi)
    {
        try
        {
            if (tutar == "") return "";

            decimal deger = Math.Round(Convert.ToDecimal(tutar, CultureInfo.CurrentCulture), 2, MidpointRounding.AwayFromZero);
            bool negatif = deger < 0;
            deger = Math.Abs(deger);

            long tamKisim = (long)Math.Truncate(deger);
            int kurus = (int)Math.Round((deger - tamKisim) * 100m, MidpointRounding.AwayFromZero);
            if (kurus == 100) { kurus = 0; tamKisim += 1; } // 2 haneye yuvarlama sınır durumu (ör. 999,995)

            string tamSonuc = SayiyiYaziyaCevir(tamKisim.ToString(CultureInfo.InvariantCulture));
            string paraBirimiAdi = ParaBirimiAdi(paraBirimi);

            string sonuc = kurus > 0
                ? $"{tamSonuc} {paraBirimiAdi} {SayiyiYaziyaCevir(kurus.ToString(CultureInfo.InvariantCulture))} {OndalikParaBirimi(paraBirimi)}"
                : $"{tamSonuc} {paraBirimiAdi}";

            return negatif ? $"Eksi {sonuc}" : sonuc;
        }
        catch (Exception)
        {
            return "";
        }
    }
}
