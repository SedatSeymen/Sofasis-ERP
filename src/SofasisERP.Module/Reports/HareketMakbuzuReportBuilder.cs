/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : HareketMakbuzuReportBuilder.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Tek bir Kasa/Cari/Banka hareket fişinin (Açılış/Tahsilat/Ödeme/Virman)
 *                    A5 yatay makbuz çıktısı — eski ERP'nin HareketMakbuzuReportBuilder'ından
 *                    uyarlandı. Eski projede CariHesapHareketleri/KasaBankaHareketleri AYRI
 *                    sınıflardı (Sayın=Cari, Kasa/Banka Hesabı=ayrı alan, biri null olabilirdi);
 *                    bu projede TEK birleşik KasaCariBankaHareketleri sınıfı ve KaynakHesap/
 *                    KarsiHesap ikilisi var, ikisi de HER ZAMAN dolu (Hesap ortak taban tipi
 *                    Cari/Kasa/Banka'yı kapsıyor) — bu yüzden eski projedeki "CariHesapTanim
 *                    null ise satırı gizle" koşullu görünürlük mantığına gerek kalmadı, kod
 *                    daha basit.
 * ****************************************************************************
 */

using System;
using System.Drawing;
using System.Globalization;
using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.Persistent.Base.ReportsV2;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using SofasisERP.Module.Services;

namespace SofasisERP.Module.Reports;

internal static class HareketMakbuzuReportBuilder
{
    const float RowHeight = 24f;
    // "Kaynak Hesap:" / "Karşı Hesap:" en uzun başlıklardan biri — genişlik onların tek
    // satıra sığması için yeterli seçildi; diğer tüm tek-sütun başlıkları da aynı genişliği
    // kullanır (hizalama için) — bkz. bugünkü Hesap Ekstresi raporunda öğrenilen ders:
    // WordWrap açık + yetersiz genişlik = başlığın ikinci satıra taşıp bir alttaki satırla
    // çakışması. Burada hem generous genişlik hem WordWrap=false ile önlendi.
    const float SingleColumnCaptionWidth = 150f;
    const float SingleColumnValueWidth = 475f;

    public static void Build(XtraReport report, string objectTypeName)
    {
        report.PaperKind = DXPaperKind.A5;
        report.Landscape = true;
        report.Margins = new DXMargins(30, 30, 24, 24);

        var dataSource = new CollectionDataSource { ObjectTypeName = objectTypeName };
        report.ComponentStorage.Add(dataSource);
        report.DataSource = dataSource;

        TopMarginBand topMargin = new TopMarginBand();
        BottomMarginBand bottomMargin = new BottomMarginBand();
        DetailBand detail = new DetailBand();
        report.Bands.AddRange(new Band[] { topMargin, detail, bottomMargin });

        const string dateFormat = "dd.MM.yyyy";

        float y = 0;

        XRLabel title = CreateLabel("[FisTuruTanim.FisTuruAdi]", 0, y, 700, 32);
        title.Font = new DXFont("Arial", 17, DXFontStyle.Bold);
        title.TextAlignment = TextAlignment.TopCenter;
        title.WordWrap = false;
        detail.Controls.Add(title);
        y += 65;

        const float leftColumnValueWidth = 145f;
        y = AddFieldRow(detail, "Fiş No:", "[FisNo]", y, leftColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        AddFieldDateRow(detail, "Fiş Tarihi:", "FisTarihi", dateFormat, y - RowHeight, 220, captionLeft: 430, captionWidth: 105);
        y = AddFieldRow(detail, "Belge No:", "[BelgeNo]", y, leftColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        AddFieldDateRow(detail, "Belge Tarihi:", "BelgeTarihi", dateFormat, y - RowHeight, 220, captionLeft: 430, captionWidth: 105);
        y += 12;

        y = AddFieldRow(detail, "Kaynak Hesap:", "[KaynakHesap.HesapAdi]", y, SingleColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        y = AddFieldRow(detail, "Karşı Hesap:", "[KarsiHesap.HesapAdi]", y, SingleColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        y += 12;

        XRLabel tutarLabel = AddFieldRowControl(detail, "Tutar:", "", y, 300, captionWidth: SingleColumnCaptionWidth);
        tutarLabel.Font = new DXFont("Arial", 14, DXFontStyle.Bold);
        tutarLabel.BeforePrint += (sender, e) =>
        {
            var lbl = (XRLabel)sender!;
            decimal borc = lbl.Report.GetCurrentColumnValue("BorcTutar") as decimal? ?? 0m;
            decimal alacak = lbl.Report.GetCurrentColumnValue("AlacakTutar") as decimal? ?? 0m;
            string dovizKodu = lbl.Report.GetCurrentColumnValue("DovizTanim.DovizKodu") as string ?? "TRY";
            decimal tutar = borc != 0 ? borc : alacak;
            lbl.Text = tutar.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")) + " " + dovizKodu;
        };
        y += RowHeight + 6;

        XRLabel tutarYaziLabel = AddFieldRowControl(detail, "Yalnız:", "", y, SingleColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        tutarYaziLabel.Font = new DXFont("Arial", 11, DXFontStyle.Italic);
        tutarYaziLabel.BeforePrint += (sender, e) =>
        {
            var lbl = (XRLabel)sender!;
            decimal borc = lbl.Report.GetCurrentColumnValue("BorcTutar") as decimal? ?? 0m;
            decimal alacak = lbl.Report.GetCurrentColumnValue("AlacakTutar") as decimal? ?? 0m;
            string dovizKodu = lbl.Report.GetCurrentColumnValue("DovizTanim.DovizKodu") as string ?? "TRY";
            decimal tutar = borc != 0 ? borc : alacak;
            // CultureInfo.CurrentCulture ile biçimlendirilmeli — bkz. SayiyiYaziyaCevirici
            // açıklaması (InvariantCulture ile tr-TR'de tutar 10000 kat büyür).
            lbl.Text = SayiyiYaziyaCevirici.SayiyiYaziyaCevirVirgullu(
                tutar.ToString(CultureInfo.CurrentCulture), dovizKodu).Trim();
        };
        y += RowHeight + 10;

        y = AddFieldRow(detail, "Açıklama:", "[Description]", y, SingleColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        y += 12;
        y = AddFieldRow(detail, "Düzenleyen:", "[CreatedBy.UserName]", y, leftColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        AddFieldDateRow(detail, "Tarih:", "CreatedDate", dateFormat, y - RowHeight, 220, captionLeft: 430, captionWidth: 105);
        y += 30;

        XRLine line1 = new XRLine { LocationF = new PointF(0, y), SizeF = new SizeF(280, 1) };
        XRLine line2 = new XRLine { LocationF = new PointF(360, y), SizeF = new SizeF(280, 1) };
        detail.Controls.Add(line1);
        detail.Controls.Add(line2);
        y += 6;
        XRLabel teslimEden = CreateLabel("Teslim Eden", 0, y, 280, 20);
        teslimEden.TextAlignment = TextAlignment.TopCenter;
        teslimEden.Font = new DXFont("Arial", 11);
        XRLabel teslimAlan = CreateLabel("Teslim Alan", 360, y, 280, 20);
        teslimAlan.TextAlignment = TextAlignment.TopCenter;
        teslimAlan.Font = new DXFont("Arial", 11);
        detail.Controls.Add(teslimEden);
        detail.Controls.Add(teslimAlan);

        detail.HeightF = y + 26;
    }

    static float AddFieldRow(DetailBand detail, string caption, string expression, float y, float valueWidth, float captionLeft = 0, float captionWidth = 140)
    {
        AddFieldRowControl(detail, caption, expression, y, valueWidth, captionLeft, captionWidth);
        return y + RowHeight;
    }

    static void AddFieldDateRow(DetailBand detail, string caption, string fieldName, string dateFormat, float y, float valueWidth, float captionLeft, float captionWidth)
    {
        AddFieldRowControl(detail, caption, $"FormatString('{{0:{dateFormat}}}', [{fieldName}])", y, valueWidth, captionLeft, captionWidth);
    }

    static XRLabel AddFieldRowControl(DetailBand detail, string caption, string expression, float y, float valueWidth, float captionLeft = 0, float captionWidth = 140)
    {
        XRLabel captionLabel = CreateLabel(caption, captionLeft, y, captionWidth - 6, 22);
        captionLabel.Font = new DXFont("Arial", 11, DXFontStyle.Bold);
        captionLabel.TextAlignment = TextAlignment.TopRight;
        captionLabel.WordWrap = false;
        detail.Controls.Add(captionLabel);

        XRLabel valueLabel = new XRLabel
        {
            LocationF = new PointF(captionLeft + captionWidth, y),
            SizeF = new SizeF(valueWidth, 22),
            Font = new DXFont("Arial", 11),
            WordWrap = false,
            Borders = BorderSide.None
        };
        if (!string.IsNullOrEmpty(expression))
        {
            valueLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", expression));
        }
        detail.Controls.Add(valueLabel);
        return valueLabel;
    }

    static XRLabel CreateLabel(string text, float x, float y, float width, float height)
    {
        return new XRLabel
        {
            Text = text,
            LocationF = new PointF(x, y),
            SizeF = new SizeF(width, height),
            Borders = BorderSide.None
        };
    }
}
