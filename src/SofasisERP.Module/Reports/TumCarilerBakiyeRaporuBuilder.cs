/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : TumCarilerBakiyeRaporuBuilder.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Tüm Cari hesapların (isteğe bağlı Cari Hesap Kodu aralığıyla
 *                    filtrelenmiş) seçili dönemdeki Devreden Bakiye/Dönem Borç/
 *                    Dönem Alacak/Kapanış Bakiyesi'ni tek tabloda listeler.
 *                    HesapEkstresiRaporu'nun aksine canlı bir XPO CollectionDataSource'a
 *                    değil, Controller'ın önceden hesapladığı List<CariBakiyeSatiri>'ye
 *                    bağlanır (bkz. TumCarilerBakiyeRaporuController, ReportPreviewContext).
 *                    Sütun genişlikleri ve WordWrap=false/sağ padding=2 kuralı,
 *                    HesapEkstresiRaporuBuilder'da canlı testle kanıtlanmış aynı
 *                    kırpma-önleme dersinden taşındı.
 * ****************************************************************************
 */

using System;
using System.Drawing;
using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using static SofasisERP.Module.Reports.ReportOrtakYardimcilar;

namespace SofasisERP.Module.Reports;

internal static class TumCarilerBakiyeRaporuBuilder
{
    // Sütun genişlikleri CANLI ekran görüntüsü (150% zoom) ile doğrulanarak belirlendi —
    // GDI+ MeasureString tabanlı teorik hesap ile piksel-analiz scriptleri güvenilmez çıktı
    // verdi (bkz. Git geçmişi/scratchpad), bu yüzden nihai değerler SADECE gerçek ekran
    // görüntüsüne bakarak kalibre edildi. Cari Hesap Kodu (16 hane, sabit format,
    // "CARITN-260819010") 0.139 oranında bile hâlâ belirgin boşluk bırakıyordu (kullanıcı
    // geri bildirimi) — 0.075'e agresif daraltıldı, açılan pay Cari Hesap Adı'na verildi.
    static readonly float[] ColumnRatios = { 0.075f, 0.314f, 0.183f, 0.144f, 0.161f, 0.123f };
    static readonly string[] ColumnCaptions = { "Cari Hesap Kodu", "Cari Hesap Adı", "Devreden Bakiye", "Dönem Borç", "Dönem Alacak", "Bakiye" };
    static readonly string[] Expressions =
    {
        "[CariHesapKodu]",
        "[CariHesapAdi]",
        "FormatString('{0:N2}', [DevredenBakiye])",
        "FormatString('{0:N2}', [DonemBorc])",
        "FormatString('{0:N2}', [DonemAlacak])",
        "FormatString('{0:N2}', [KapanisBakiyesi])"
    };

    public static void Build(XtraReport report)
    {
        report.PaperKind = DXPaperKind.A4;
        report.Landscape = true;
        report.Margins = new DXMargins(40, 40, 30, 30);

        AddHiddenParameter(report, "RaporBasligi", typeof(string));
        AddHiddenParameter(report, "BaslangicTarihi", typeof(DateTime));
        AddHiddenParameter(report, "BitisTarihi", typeof(DateTime));

        float contentWidth = report.PageWidth - report.Margins.Left - report.Margins.Right;

        XRControlStyle oddRowStyle = new XRControlStyle { Name = "TumCarilerTekSatir", BackColor = Color.White };
        oddRowStyle.StyleUsing.UseBackColor = true;
        XRControlStyle evenRowStyle = new XRControlStyle { Name = "TumCarilerCiftSatir", BackColor = ZebraColor };
        evenRowStyle.StyleUsing.UseBackColor = true;
        report.StyleSheet.AddRange(new[] { oddRowStyle, evenRowStyle });

        TopMarginBand topMargin = new TopMarginBand();
        BottomMarginBand bottomMargin = new BottomMarginBand();
        ReportHeaderBand reportHeader = new ReportHeaderBand();
        ReportFooterBand reportFooter = new ReportFooterBand { PrintAtBottom = true };
        DetailBand detail = new DetailBand { HeightF = RowHeight };
        report.Bands.AddRange(new Band[] { topMargin, reportHeader, detail, reportFooter, bottomMargin });

        BuildHeader(reportHeader, contentWidth);
        BuildColumnHeader(reportHeader, contentWidth);
        BuildDetailRow(detail, contentWidth, oddRowStyle.Name, evenRowStyle.Name);
        BuildFooter(reportFooter, contentWidth);
    }

    static void AddHiddenParameter(XtraReport report, string name, Type type)
    {
        report.Parameters.Add(new Parameter { Name = name, Type = type, Visible = false });
    }

    static float[] ColumnWidths(float contentWidth)
    {
        float[] widths = new float[ColumnRatios.Length];
        float used = 0;
        for (int i = 0; i < ColumnRatios.Length - 1; i++)
        {
            widths[i] = (float)Math.Round(contentWidth * ColumnRatios[i]);
            used += widths[i];
        }
        widths[^1] = contentWidth - used;
        return widths;
    }

    static void BuildHeader(ReportHeaderBand header, float contentWidth)
    {
        float y = 0;

        XRLabel brand = CreateLabel("SOFASİS", 0, y, 300, 26);
        brand.Font = new DXFont("Arial", 16, DXFontStyle.Bold);
        brand.ForeColor = AccentColor;
        header.Controls.Add(brand);

        XRLabel titleBadge = new XRLabel
        {
            LocationF = new PointF(contentWidth - 300, y),
            SizeF = new SizeF(300, 26),
            Font = new DXFont("Arial", 12, DXFontStyle.Bold),
            ForeColor = Color.White,
            BackColor = AccentColor,
            TextAlignment = TextAlignment.MiddleCenter,
            Borders = BorderSide.None
        };
        titleBadge.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "Upper(?RaporBasligi)"));
        header.Controls.Add(titleBadge);
        y += 36;

        XRLine rule = new XRLine { LocationF = new PointF(0, y), SizeF = new SizeF(contentWidth, 2) };
        rule.ForeColor = AccentColor;
        header.Controls.Add(rule);
        y += 12;

        float panelHeight = 30;
        XRPanel infoPanel = new XRPanel
        {
            LocationF = new PointF(0, y),
            SizeF = new SizeF(contentWidth, panelHeight),
            BackColor = ZebraColor,
            BorderColor = BorderColor,
            Borders = BorderSide.All,
            BorderWidth = 1
        };
        header.Controls.Add(infoPanel);

        XRTable infoTable = new XRTable
        {
            LocationF = new PointF(12, (panelHeight - RowHeight) / 2),
            SizeF = new SizeF(contentWidth - 24, RowHeight),
            Borders = BorderSide.None
        };
        infoTable.Rows.Add(BuildFieldGridRow(RowHeight,
            ("Dönem Başlangıç:", "FormatString('{0:dd.MM.yyyy}', ?BaslangicTarihi)", 130, 110),
            ("Dönem Bitiş:", "FormatString('{0:dd.MM.yyyy}', ?BitisTarihi)", 100, 110)));
        infoPanel.Controls.Add(infoTable);

        y += panelHeight + 10;
        header.HeightF = y;
    }

    static void BuildColumnHeader(ReportHeaderBand header, float contentWidth)
    {
        float[] widths = ColumnWidths(contentWidth);
        float y = header.HeightF;

        XRTable table = new XRTable { LocationF = new PointF(0, y), SizeF = new SizeF(contentWidth, 26), Borders = BorderSide.All, BorderColor = BorderColor };
        XRTableRow row = new XRTableRow { WidthF = table.WidthF };
        table.Rows.Add(row);
        for (int i = 0; i < widths.Length; i++)
        {
            bool rightAlign = i >= 2;
            row.Cells.Add(new XRTableCell
            {
                Text = ColumnCaptions[i],
                WidthF = widths[i],
                Font = new DXFont("Arial", 10, DXFontStyle.Bold),
                ForeColor = Color.White,
                BackColor = AccentColor,
                TextAlignment = rightAlign ? TextAlignment.MiddleRight : TextAlignment.MiddleLeft,
                Padding = new PaddingInfo(6, 2, 2, 2, 96),
                Borders = BorderSide.All,
                BorderColor = BorderColor,
                WordWrap = false,
                CanGrow = false
            });
        }
        header.Controls.Add(table);
        header.HeightF = y + 26;
    }

    static void BuildDetailRow(DetailBand detail, float contentWidth, string oddStyleName, string evenStyleName)
    {
        float[] widths = ColumnWidths(contentWidth);

        XRTable table = new XRTable { LocationF = new PointF(0, 0), SizeF = new SizeF(contentWidth, RowHeight), Borders = BorderSide.All, BorderColor = BorderColor };
        table.OddStyleName = oddStyleName;
        table.EvenStyleName = evenStyleName;
        XRTableRow row = new XRTableRow { WidthF = table.WidthF };
        table.Rows.Add(row);

        for (int i = 0; i < widths.Length; i++)
        {
            bool rightAlign = i >= 2;
            XRTableCell cell = new XRTableCell
            {
                WidthF = widths[i],
                Font = new DXFont("Arial", 9),
                TextAlignment = rightAlign ? TextAlignment.MiddleRight : TextAlignment.MiddleLeft,
                Padding = new PaddingInfo(6, 2, 2, 2, 96),
                Borders = BorderSide.All,
                BorderColor = BorderColor,
                WordWrap = false,
                CanGrow = false
            };
            cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", Expressions[i]));
            row.Cells.Add(cell);
        }
        detail.Controls.Add(table);
    }

    static void BuildFooter(ReportFooterBand footer, float contentWidth)
    {
        float tableWidth = 320f;
        float tableLeft = contentWidth - tableWidth;
        float y = 14;

        XRLine rule = new XRLine { LocationF = new PointF(0, 0), SizeF = new SizeF(contentWidth, 1) };
        rule.ForeColor = BorderColor;
        footer.Controls.Add(rule);

        (string caption, string expression, bool vurgulu)[] rows =
        {
            ("Genel Toplam Borç", "FormatString('{0:N2}', sumSum([DonemBorc]))", false),
            ("Genel Toplam Alacak", "FormatString('{0:N2}', sumSum([DonemAlacak]))", false),
            ("Genel Kapanış Bakiyesi", "FormatString('{0:N2}', sumSum([KapanisBakiyesi]))", true),
        };
        XRTable table = BuildTotalsTable(tableLeft, y, tableWidth, rows);
        footer.Controls.Add(table);

        footer.HeightF = y + 24 * rows.Length + 20;
    }
}
