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

    // CultureInfo.CurrentCulture ile biçimlendirilmiş bir tutar metni (tr-TR: "," ondalık,
    // "." binlik ayıracı) bekler — InvariantCulture ("1500.00") verilirse "." binlik ayıracı
    // sanılıp 1500 yanlışlıkla 15.000.000'a dönüşür (eski ERP'de kanıtlanmış hata, bkz. dosya
    // başı açıklama).
    public static string SayiyiYaziyaCevirVirgullu(string tutar, string paraBirimi)
    {
        try
        {
            string ondalikParaBirimi = paraBirimi.ToUpper(CultureInfo.InvariantCulture) switch
            {
                "TRY" => "Kuruş",
                "USD" => "Cent",
                "EUR" => "Cent",
                _ => ""
            };

            if (tutar == "") return "";
            decimal yuvarlanmis = Math.Round(Convert.ToDecimal(tutar, CultureInfo.CurrentCulture), 3);

            string[] parcalar = yuvarlanmis.ToString(CultureInfo.CurrentCulture).Split(',');
            string tamSayi = parcalar[0].Replace(".", "");
            string ondalikSayi = parcalar.Length > 1 ? parcalar[1] : "";

            string tamSayiSonuc = SayiyiYaziyaCevir(tamSayi);
            string ondalikSonuc = ondalikSayi != "" && Convert.ToInt32(ondalikSayi, CultureInfo.InvariantCulture) > 0
                ? SayiyiYaziyaCevir(ondalikSayi)
                : "";

            string paraBirimiBaslikli = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(paraBirimi);
            return ondalikSonuc.Length > 0
                ? $"{tamSayiSonuc} {paraBirimiBaslikli} {ondalikSonuc} {ondalikParaBirimi}"
                : $"{tamSayiSonuc} {paraBirimiBaslikli}";
        }
        catch (Exception)
        {
            return "";
        }
    }
}
