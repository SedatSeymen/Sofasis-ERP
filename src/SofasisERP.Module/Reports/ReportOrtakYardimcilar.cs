/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : ReportOrtakYardimcilar.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Rapor Builder'ları arasında paylaşılan çizim yardımcıları (marka
 *                    rengi, etiket/satır oluşturma, toplam tablosu) — eski ERP'den
 *                    (D:\2025\SofasisERP\...\Sofasis.Module\Reports\ReportOrtakYardimcilar.cs)
 *                    uyarlandı. "n sayıda rapor" hedefiyle yeni bir rapor eklendiğinde
 *                    aynı görsel dili tekrar yazmadan buradan kullanılır.
 * ****************************************************************************
 */

using System.Drawing;
using DevExpress.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;

namespace SofasisERP.Module.Reports;

internal static class ReportOrtakYardimcilar
{
    internal const string DateFormat = "dd.MM.yyyy";
    internal const float RowHeight = 22f;

    internal static readonly Color AccentColor = Color.FromArgb(31, 58, 95);
    internal static readonly Color AccentLight = Color.FromArgb(226, 232, 240);
    internal static readonly Color ZebraColor = Color.FromArgb(244, 246, 249);
    internal static readonly Color BorderColor = Color.FromArgb(197, 205, 216);

    internal static XRLabel CreateLabel(string text, float x, float y, float width, float height)
    {
        return new XRLabel
        {
            Text = text,
            LocationF = new PointF(x, y),
            SizeF = new SizeF(width, height),
            Borders = BorderSide.None
        };
    }

    // Bir "Başlık: Değer" satırını, GENİŞLİĞİ SABİT ve BİRBİRİNİ EZEMEYEN bir XRTable
    // hücre çifti olarak çizer — iki bağımsız XRLabel'ı elle koordinatla hizalamanın
    // (eski yaklaşım) uzun başlık/değer metinlerinde bindirmeye/kırpılmaya yol açtığı
    // canlı olarak görülmüştü (bkz. Hesap Ekstresi raporu, 2026-08-21). Tablo hücreleri
    // genişlikleri toplamı kadar yer kapladığından bindirme YAPISAL OLARAK imkansız.
    internal static XRTableCell BuildCaptionCell(string caption, float width)
    {
        return new XRTableCell
        {
            Text = caption,
            WidthF = width,
            Font = new DXFont("Arial", 10, DXFontStyle.Bold),
            ForeColor = AccentColor,
            TextAlignment = TextAlignment.MiddleRight,
            Padding = new PaddingInfo(2, 6, 2, 2, 96),
            Borders = BorderSide.None,
            WordWrap = false,
            CanGrow = false
        };
    }

    internal static XRTableCell BuildValueCell(string expression, float width)
    {
        XRTableCell cell = new XRTableCell
        {
            WidthF = width,
            Font = new DXFont("Arial", 10),
            TextAlignment = TextAlignment.MiddleLeft,
            Padding = new PaddingInfo(6, 2, 2, 2, 96),
            Borders = BorderSide.None,
            WordWrap = false,
            CanGrow = false
        };
        if (!string.IsNullOrEmpty(expression))
        {
            cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", expression));
        }
        return cell;
    }

    internal static XRTableRow BuildFieldGridRow(float rowHeight, params (string caption, string expression, float captionWidth, float valueWidth)[] fields)
    {
        XRTableRow row = new XRTableRow { HeightF = rowHeight };
        foreach (var (caption, expression, captionWidth, valueWidth) in fields)
        {
            row.Cells.Add(BuildCaptionCell(caption, captionWidth));
            row.Cells.Add(BuildValueCell(expression, valueWidth));
        }
        return row;
    }

    // Toplamlar bloğu: TEK bir XRTable (2 sütun: başlık|tutar, HER İKİ sütun da sağa
    // hizalı) — hizalama manuel koordinat eşleşmesine değil tablo yapısına dayanır.
    internal static XRTable BuildTotalsTable(float tableLeft, float y, float tableWidth, (string caption, string expression, bool vurgulu)[] rows)
    {
        XRTable table = new XRTable
        {
            LocationF = new PointF(tableLeft, y),
            SizeF = new SizeF(tableWidth, 24 * rows.Length),
            Borders = BorderSide.All,
            BorderColor = BorderColor
        };
        foreach (var (caption, expression, vurgulu) in rows)
        {
            XRTableRow row = new XRTableRow { WidthF = tableWidth, HeightF = 24 };

            XRTableCell captionCell = new XRTableCell
            {
                Text = caption + ":",
                WidthF = tableWidth * 0.55f,
                Font = new DXFont("Arial", vurgulu ? 12 : 10, vurgulu ? DXFontStyle.Bold : DXFontStyle.Regular),
                TextAlignment = TextAlignment.MiddleRight,
                Padding = new PaddingInfo(6, 6, 2, 2, 96),
                Borders = BorderSide.All,
                BorderColor = BorderColor
            };
            XRTableCell valueCell = new XRTableCell
            {
                WidthF = tableWidth * 0.45f,
                Font = new DXFont("Arial", vurgulu ? 12 : 10, vurgulu ? DXFontStyle.Bold : DXFontStyle.Regular),
                TextAlignment = TextAlignment.MiddleRight,
                Padding = new PaddingInfo(2, 6, 2, 2, 96),
                Borders = BorderSide.All,
                BorderColor = BorderColor
            };
            // Summary property'si açıkça ayarlanmazsa sumSum(...) gibi özet fonksiyonları
            // Expression'da yazılı olsa bile hesaplanmadan boş kalabiliyor (DevExpress
            // resmi dokümantasyonu: "the summary functions are not available" uyarısı) —
            // bu satır olmadan tüm satırlar boş rakam gösteriyordu (canlı testte görüldü).
            valueCell.Summary = new XRSummary { Running = SummaryRunning.Report, Func = SummaryFunc.Sum };
            valueCell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", expression));
            if (vurgulu)
            {
                captionCell.BackColor = AccentLight;
                valueCell.BackColor = AccentLight;
            }
            row.Cells.AddRange(new[] { captionCell, valueCell });
            table.Rows.Add(row);
        }
        return table;
    }
}
