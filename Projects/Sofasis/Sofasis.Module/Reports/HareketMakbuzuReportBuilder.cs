using System.Drawing;
using System.Globalization;
using DevExpress.Drawing;
using DevExpress.Drawing.Printing;
using DevExpress.Persistent.Base.ReportsV2;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Sofasis.Module.BusinessObjects;

namespace Sofasis.Module.Reports;

// CariHesapHareketleri ve KasaBankaHareketleri neredeyse aynı özellik adlarını taşıdığından
// (FisNo, FisTarihi, BorcTutar, AlacakTutar, CariHesapTanim, KasaBankaTanim, FisTuruTanim,
// Description, CreatedBy, CreatedDate) tek bir görsel yerleşim kodu iki rapor sınıfı arasında
// paylaşılabiliyor — kod tekrarını önler. Visual Studio Report Designer bu ortamda kullanılamadığı
// için tasarım prosedürel kodla (DevExpress'in resmi desteklediği "Create a Report in Code" yöntemi)
// yapılıyor.
//
// Sayfa A5 yatay (varsayılan ReportUnit=HundredthsOfAnInch için ~827x583 birim); ContentWidth/Height
// margin (30+30) düşüldükten sonraki kullanılabilir alana bolca pay bırakılarak tasarlandı (dar
// pay bırakmak — ör. tam sınıra denk gelen genişlikler — yuvarlama nedeniyle sayfanın taşıp ikinci
// bir sayfa oluşmasına yol açabiliyor; bu yüzden kullanılabilir alanın belirgin şekilde altında kalındı).
internal static class HareketMakbuzuReportBuilder
{
    const float RowHeight = 24f;
    // "Kasa / Banka Hesabı:" en uzun başlık — genişlik onun tek satıra sığması için yeterli
    // seçildi; diğer tüm tek-sütun başlıkları da aynı genişliği kullanır (hizalama için).
    const float SingleColumnCaptionWidth = 185f;
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

        // Tarih alanları saat kısmı olmadan, sadece gün.ay.yıl gösterilsin.
        const string dateFormat = "dd.MM.yyyy";

        float y = 0;

        XRLabel title = CreateLabel("[FisTuruTanim.FisTuruAdi]", 0, y, 700, 32);
        title.Font = new DXFont("Arial", 17, DXFontStyle.Bold);
        title.TextAlignment = TextAlignment.TopCenter;
        detail.Controls.Add(title);
        y += 65;

        // İki sütunlu satırlar: sol sütun 0-330, sağ sütun (tarih alanları) 430-755 — sağ
        // sütun kasıtlı olarak sayfanın sağına doğru kaydırıldı ki ortada sıkışık durmasın.
        // Sol sütun başlıkları
        // (Fiş No/Belge No/Düzenleyen), aşağıdaki tek-sütunlu satırlarla (Sayın vb.) aynı
        // captionWidth'i (SingleColumnCaptionWidth) kullanır ki sayfanın SOLUNDAKİ tüm ":"
        // işaretleri de (sağ sütundan bağımsız olarak) tek bir dikey çizgide hizalı olsun.
        const float LeftColumnValueWidth = 145f;
        y = AddFieldRow(detail, "Fiş No:", "[FisNo]", y, LeftColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        AddFieldDateRow(detail, "Fiş Tarihi:", "FisTarihi", dateFormat, y - RowHeight, 220, captionLeft: 430, captionWidth: 105);
        y = AddFieldRow(detail, "Belge No:", "[BelgeNo]", y, LeftColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        AddFieldDateRow(detail, "Belge Tarihi:", "BelgeTarihi", dateFormat, y - RowHeight, 220, captionLeft: 430, captionWidth: 105);
        y += 12;

        // Tek-sütunlu satırların (Sayın, Kasa/Banka Hesabı, Tutar, Yalnız, Açıklama) tümü aynı
        // captionWidth (SingleColumnCaptionWidth) kullanır ki farklı uzunlukta başlıklar yüzünden
        // ":" işaretleri satırdan satıra kaymasın — hepsi tek bir dikey çizgide hizalı kalsın.
        XRLabel cariRow = AddFieldRowControl(detail, "Sayın:", "[CariHesapTanim.CariHesapAdi]", y, SingleColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        cariRow.BeforePrint += (sender, e) =>
        {
            var lbl = (XRLabel)sender!;
            lbl.Visible = lbl.Report.GetCurrentColumnValue("CariHesapTanim") != null;
        };
        // Aynı satırdaki "Sayın:" etiketini de aynı görünürlükle eşleştir (bir önceki eklenen kontrol).
        XRLabel cariCaption = (XRLabel)detail.Controls[detail.Controls.Count - 2];
        cariCaption.BeforePrint += (sender, e) => ((XRLabel)sender!).Visible = cariRow.Visible;
        y += RowHeight + 6;

        y = AddFieldRow(detail, "Kasa / Banka Hesabı:", "[KasaBankaTanim.HesapAdi]", y, SingleColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        y += 12;

        XRLabel tutarLabel = AddFieldRowControl(detail, "Tutar:", "", y, 300, captionWidth: SingleColumnCaptionWidth);
        tutarLabel.Font = new DXFont("Arial", 14, DXFontStyle.Bold);
        tutarLabel.BeforePrint += (sender, e) =>
        {
            var lbl = (XRLabel)sender!;
            decimal? borc = lbl.Report.GetCurrentColumnValue("BorcTutar") as decimal?;
            decimal? alacak = lbl.Report.GetCurrentColumnValue("AlacakTutar") as decimal?;
            string dovizKodu = lbl.Report.GetCurrentColumnValue("DovizTanim.DovizKodu") as string ?? "TRY";
            decimal tutar = borc.GetValueOrDefault() != 0 ? borc.GetValueOrDefault() : alacak.GetValueOrDefault();
            lbl.Text = tutar.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")) + " " + dovizKodu;
        };
        y += RowHeight + 6;

        XRLabel tutarYaziLabel = AddFieldRowControl(detail, "Yalnız:", "", y, SingleColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        tutarYaziLabel.Font = new DXFont("Arial", 11, DXFontStyle.Italic);
        tutarYaziLabel.BeforePrint += (sender, e) =>
        {
            var lbl = (XRLabel)sender!;
            decimal? borc = lbl.Report.GetCurrentColumnValue("BorcTutar") as decimal?;
            decimal? alacak = lbl.Report.GetCurrentColumnValue("AlacakTutar") as decimal?;
            string dovizKodu = lbl.Report.GetCurrentColumnValue("DovizTanim.DovizKodu") as string ?? "TRY";
            decimal tutar = borc.GetValueOrDefault() != 0 ? borc.GetValueOrDefault() : alacak.GetValueOrDefault();
            // Helper.SayiyiYaziyaCevirVirgullu, aldığı string'i CurrentCulture ile Convert.ToDecimal
            // kullanarak geri ayrıştırıyor (tr-TR'de "," ondalık, "." binlik ayıracı) — bu yüzden
            // burada da CurrentCulture ile biçimlendirmek gerekiyor; InvariantCulture ("1500.00")
            // tr-TR'de "." binlik ayıracı sayılıp 1500'ün 15.000.000'a dönüşmesine yol açıyordu.
            lbl.Text = Helper.SayiyiYaziyaCevirVirgullu(
                tutar.ToString(CultureInfo.CurrentCulture), dovizKodu).Replace("#", " ").Trim();
            while (lbl.Text.Contains("  ")) lbl.Text = lbl.Text.Replace("  ", " ");
        };
        y += RowHeight + 10;

        y = AddFieldRow(detail, "Açıklama:", "[Description]", y, SingleColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        y += 12;
        y = AddFieldRow(detail, "Düzenleyen:", "[CreatedBy.UserName]", y, LeftColumnValueWidth, captionWidth: SingleColumnCaptionWidth);
        AddFieldDateRow(detail, "Tarih:", "CreatedDate", dateFormat, y - RowHeight, 220, captionLeft: 430, captionWidth: 105);
        y += 30;

        // İmza / kaşe alanları — iki sütun, güvenli payla.
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
        // Başlıklar sağa hizalanır ki aynı sütundaki tüm ":" işaretleri (farklı uzunlukta
        // "Fiş No:" / "Fiş Tarihi:" gibi metinlerde bile) aynı dikey çizgide hizalansın.
        XRLabel captionLabel = CreateLabel(caption, captionLeft, y, captionWidth - 6, 22);
        captionLabel.Font = new DXFont("Arial", 11, DXFontStyle.Bold);
        captionLabel.TextAlignment = TextAlignment.TopRight;
        detail.Controls.Add(captionLabel);

        XRLabel valueLabel = new XRLabel
        {
            LocationF = new PointF(captionLeft + captionWidth, y),
            SizeF = new SizeF(valueWidth, 22),
            Font = new DXFont("Arial", 11),
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
