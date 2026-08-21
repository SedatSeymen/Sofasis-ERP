/* ****************************************************************************
 * Proje            : Sofasis Erp Project
 * Dosya Adı        : HesapEkstresiRaporuBuilder.cs
 * Oluşturma Tarihi : 08/21/2026
 * Oluşturan        : Sedat Seymen
 * Son Güncelleme   : 08/21/2026
 * Son Güncelleyen  : Sedat Seymen
 * Açıklama         : Netsis/Logo/Mikro tarzı, tek Hesap + tarih aralığı, koşan bakiye
 *                    sütunlu klasik ekstre — eski ERP'nin CariHesapEkstresiRaporuBuilder'ından
 *                    uyarlandı. Veri kaynağı KasaCariBankaHareketleri (CollectionDataSource);
 *                    kriter (KaynakHesap=X Veya KarsiHesap=X + tarih aralığı) launching
 *                    Controller tarafından ShowPreview(handle, criteria) ile uygulanır.
 *
 *                    ÖNEMLİ FARK (eski ERP'ye göre): eski projede her hareket kaydı TEK bir
 *                    Cari'ye aitti, Borç/Alacak zaten o Cari'nin bakış açısındandı. Bu projede
 *                    KaynakHesap/KarsiHesap ikilisi var — seçili Hesap bazen Kaynak bazen Karşı
 *                    tarafta olabilir (bkz. KasaCariBankaHareketleri.ObjectSaving: Borç Kaynak'ı
 *                    artırır, Alacak Karşı'yı azaltır). Bu yüzden "SeciliBorc"/"SeciliAlacak"
 *                    hesaplanmış alanları, seçili Hesap'ın (?SeciliHesapOid) o satırda Kaynak mı
 *                    Karşı mı olduğuna göre YerelBorcTutar/YerelAlacakTutar'ı doğru yöne çevirir.
 * ****************************************************************************
 */

using System;
using System.Drawing;
using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.Persistent.Base.ReportsV2;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using static SofasisERP.Module.Reports.ReportOrtakYardimcilar;

namespace SofasisERP.Module.Reports;

internal static class HesapEkstresiRaporuBuilder
{
    public static void Build(XtraReport report)
    {
        report.PaperKind = DXPaperKind.A4;
        report.Landscape = false;
        report.Margins = new DXMargins(40, 40, 30, 30);

        var dataSource = new CollectionDataSource
        {
            ObjectTypeName = "SofasisERP.Module.BusinessObjects.KasaCariBankaHareketleri"
        };
        report.ComponentStorage.Add(dataSource);
        report.DataSource = dataSource;

        AddHiddenParameter(report, "RaporBasligi", typeof(string));
        AddHiddenParameter(report, "HesapAdi", typeof(string));
        AddHiddenParameter(report, "DovizKodu", typeof(string));
        AddHiddenParameter(report, "BaslangicTarihi", typeof(DateTime));
        AddHiddenParameter(report, "BitisTarihi", typeof(DateTime));
        AddHiddenParameter(report, "AcilisBakiyesi", typeof(decimal));
        AddHiddenParameter(report, "SeciliHesapOid", typeof(Guid));

        // Seçili Hesap'ın o satırda Kaynak mı Karşı mı olduğuna göre Borç/Alacak'ı doğru
        // yöne çevirir (bkz. dosya başındaki açıklama). NetHareket koşan bakiye için.
        report.CalculatedFields.Add(new CalculatedField
        {
            Name = "SeciliBorc",
            FieldType = FieldType.Decimal,
            Expression = "Iif([KaynakHesap.Oid] = ?SeciliHesapOid, [YerelBorcTutar], 0)"
        });
        report.CalculatedFields.Add(new CalculatedField
        {
            Name = "SeciliAlacak",
            FieldType = FieldType.Decimal,
            Expression = "Iif([KarsiHesap.Oid] = ?SeciliHesapOid, [YerelAlacakTutar], 0)"
        });
        report.CalculatedFields.Add(new CalculatedField
        {
            Name = "NetHareket",
            FieldType = FieldType.Decimal,
            Expression = "[SeciliBorc] - [SeciliAlacak]"
        });

        float contentWidth = report.PageWidth - report.Margins.Left - report.Margins.Right;

        XRControlStyle oddRowStyle = new XRControlStyle { Name = "EkstreTekSatir", BackColor = Color.White };
        oddRowStyle.StyleUsing.UseBackColor = true;
        XRControlStyle evenRowStyle = new XRControlStyle { Name = "EkstreCiftSatir", BackColor = ZebraColor };
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

    // Fiş Türü sütunu kaldırıldı. Genişlikler 100% zoom canlı testle tek tek
    // doğrulandı (2026-08-21): Fiş No/Tarih/Borç/Alacak kendi içeriklerine göre
    // (padding sağ 6→2 düşürülerek) tam sığacak şekilde ayarlandı — Tarih özellikle
    // "gg.aa.yyyy" formatının TAMAMI görünecek kadar geniş olmalı (daha dar denendi,
    // son basamak kırpıldığı canlı testte görüldü). Kalan alan Karşı Hesap'a verildi.
    static readonly float[] ColumnRatios = { 0.196f, 0.249f, 0.256f, 0.153f, 0.146f };
    static readonly string[] ColumnCaptions = { "Fiş No", "Tarih", "Karşı Hesap", "Borç", "Alacak" };

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

    const float BakiyeWidth = 160f;

    static void BuildHeader(ReportHeaderBand header, float contentWidth)
    {
        float y = 0;

        XRLabel brand = CreateLabel("SOFASİS", 0, y, 300, 26);
        brand.Font = new DXFont("Arial", 16, DXFontStyle.Bold);
        brand.ForeColor = AccentColor;
        header.Controls.Add(brand);

        XRLabel titleBadge = new XRLabel
        {
            LocationF = new PointF(contentWidth - 260, y),
            SizeF = new SizeF(260, 26),
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

        float panelHeight = 74;
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

        // Sol/sağ sütun genişlikleri toplamı = contentWidth - 24 (panel iç boşluğu) —
        // bir XRTable içindeki hücreler olduğundan bindirme YAPISAL OLARAK imkansız
        // (bkz. ReportOrtakYardimcilar.BuildFieldGridRow açıklaması).
        float half = (contentWidth - 24) / 2;
        const float leftCaptionW = 90f;
        const float rightCaptionW = 150f;
        float leftValueW = half - leftCaptionW;
        float rightValueW = half - rightCaptionW;

        XRTable infoTable = new XRTable
        {
            LocationF = new PointF(12, (panelHeight - 3 * RowHeight) / 2),
            SizeF = new SizeF(contentWidth - 24, 3 * RowHeight),
            Borders = BorderSide.None
        };
        infoTable.Rows.Add(BuildFieldGridRow(RowHeight,
            ("Hesap:", "?HesapAdi", leftCaptionW, leftValueW),
            ("Dönem Başlangıç:", "FormatString('{0:dd.MM.yyyy}', ?BaslangicTarihi)", rightCaptionW, rightValueW)));
        infoTable.Rows.Add(BuildFieldGridRow(RowHeight,
            ("Para Birimi:", "?DovizKodu", leftCaptionW, leftValueW),
            ("Dönem Bitiş:", "FormatString('{0:dd.MM.yyyy}', ?BitisTarihi)", rightCaptionW, rightValueW)));
        infoTable.Rows.Add(BuildFieldGridRow(RowHeight,
            ("", "", leftCaptionW, leftValueW),
            ("Devreden Bakiye:", "FormatString('{0:N2}', ?AcilisBakiyesi)", rightCaptionW, rightValueW)));
        infoPanel.Controls.Add(infoTable);

        y += panelHeight + 10;
        header.HeightF = y;
    }

    static void BuildColumnHeader(ReportHeaderBand header, float contentWidth)
    {
        float[] widths = ColumnWidths(contentWidth - BakiyeWidth);
        float y = header.HeightF;

        XRTable table = new XRTable { LocationF = new PointF(0, y), SizeF = new SizeF(contentWidth - BakiyeWidth, 26), Borders = BorderSide.All, BorderColor = BorderColor };
        XRTableRow row = new XRTableRow { WidthF = table.WidthF };
        table.Rows.Add(row);
        for (int i = 0; i < widths.Length; i++)
        {
            bool rightAlign = i == 3 || i == 4;
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

        XRTableCell bakiyeHeaderCell = new XRTableCell
        {
            Text = "Bakiye",
            WidthF = BakiyeWidth,
            Font = new DXFont("Arial", 10, DXFontStyle.Bold),
            ForeColor = Color.White,
            BackColor = AccentColor,
            TextAlignment = TextAlignment.MiddleRight,
            Padding = new PaddingInfo(6, 2, 2, 2, 96),
            Borders = BorderSide.All,
            BorderColor = BorderColor,
            WordWrap = false,
            CanGrow = false
        };
        XRTable bakiyeHeaderTable = new XRTable { LocationF = new PointF(contentWidth - BakiyeWidth, y), SizeF = new SizeF(BakiyeWidth, 26) };
        XRTableRow bakiyeHeaderRow = new XRTableRow { WidthF = BakiyeWidth };
        bakiyeHeaderRow.Cells.Add(bakiyeHeaderCell);
        bakiyeHeaderTable.Rows.Add(bakiyeHeaderRow);
        header.Controls.Add(bakiyeHeaderTable);

        header.HeightF = y + 26;
    }

    static void BuildDetailRow(DetailBand detail, float contentWidth, string oddStyleName, string evenStyleName)
    {
        float[] widths = ColumnWidths(contentWidth - BakiyeWidth);

        XRTable table = new XRTable { LocationF = new PointF(0, 0), SizeF = new SizeF(contentWidth - BakiyeWidth, RowHeight), Borders = BorderSide.All, BorderColor = BorderColor };
        table.OddStyleName = oddStyleName;
        table.EvenStyleName = evenStyleName;
        XRTableRow row = new XRTableRow { WidthF = table.WidthF };
        table.Rows.Add(row);

        string[] expressions =
        {
            "[FisNo]",
            "FormatString('{0:dd.MM.yyyy}', [FisTarihi])",
            "Iif([KaynakHesap.Oid] = ?SeciliHesapOid, [KarsiHesap.HesapAdi], [KaynakHesap.HesapAdi])",
            "Iif([SeciliBorc] > 0, FormatString('{0:N2}', [SeciliBorc]), '')",
            "Iif([SeciliAlacak] > 0, FormatString('{0:N2}', [SeciliAlacak]), '')"
        };
        for (int i = 0; i < widths.Length; i++)
        {
            bool rightAlign = i == 3 || i == 4;
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
            cell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", expressions[i]));
            row.Cells.Add(cell);
        }
        detail.Controls.Add(table);

        XRTable bakiyeTable = new XRTable { LocationF = new PointF(contentWidth - BakiyeWidth, 0), SizeF = new SizeF(BakiyeWidth, RowHeight) };
        bakiyeTable.OddStyleName = oddStyleName;
        bakiyeTable.EvenStyleName = evenStyleName;
        XRTableRow bakiyeRow = new XRTableRow { WidthF = BakiyeWidth };
        XRTableCell bakiyeCell = new XRTableCell
        {
            WidthF = BakiyeWidth,
            Font = new DXFont("Arial", 9, DXFontStyle.Bold),
            TextAlignment = TextAlignment.MiddleRight,
            Padding = new PaddingInfo(6, 2, 2, 2, 96),
            Borders = BorderSide.All,
            BorderColor = BorderColor,
            WordWrap = false,
            CanGrow = false
        };
        // Koşan bakiye: Devreden Bakiye (gizli parametre) + o ana kadarki net hareketlerin
        // kümülatif toplamı. XRSummary (Running=Report) olmadan sumRunningSum hesaplanmaz.
        bakiyeCell.Summary = new XRSummary { Running = SummaryRunning.Report };
        bakiyeCell.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text",
            "FormatString('{0:N2}', ?AcilisBakiyesi + sumRunningSum([NetHareket]))"));
        bakiyeRow.Cells.Add(bakiyeCell);
        bakiyeTable.Rows.Add(bakiyeRow);
        detail.Controls.Add(bakiyeTable);
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
            ("Dönem Toplam Borç", "FormatString('{0:N2}', sumSum([SeciliBorc]))", false),
            ("Dönem Toplam Alacak", "FormatString('{0:N2}', sumSum([SeciliAlacak]))", false),
            ("Kapanış Bakiyesi", "FormatString('{0:N2} {1}', ?AcilisBakiyesi + sumSum([NetHareket]), ?DovizKodu)", true),
        };
        XRTable table = BuildTotalsTable(tableLeft, y, tableWidth, rows);
        foreach (XRTableRow row in table.Rows)
        {
            row.Cells[1].Summary = new XRSummary { Running = SummaryRunning.Report };
        }
        footer.Controls.Add(table);

        footer.HeightF = y + 24 * rows.Length + 20;
    }
}
