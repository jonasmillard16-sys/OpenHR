using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace RegionHR.Infrastructure.Export;

public class PayslipData
{
    public string AnstallNamn { get; set; } = "";
    public string Personnummer { get; set; } = "";
    public string Period { get; set; } = "";
    public string Enhet { get; set; } = "";
    public decimal Brutto { get; set; }
    public decimal Skatt { get; set; }
    public decimal Netto { get; set; }
    public decimal Arbetsgivaravgifter { get; set; }
    public decimal OBTillagg { get; set; }
    public decimal Overtid { get; set; }
    public decimal Sjuklon { get; set; }
    public decimal Semesterlon { get; set; }
    public decimal Pension { get; set; }
    public List<PayslipLine> Rader { get; set; } = new();
}

public class PayslipLine
{
    public string Loneart { get; set; } = "";
    public string Benamning { get; set; } = "";
    public decimal Antal { get; set; }
    public decimal Sats { get; set; }
    public decimal Belopp { get; set; }
}

public class PdfPayslipGenerator
{
    private static readonly XColor DarkBlue = XColor.FromArgb(0, 51, 102);
    private static readonly XColor Grey = XColor.FromArgb(128, 128, 128);
    private static readonly XColor LightGrey = XColor.FromArgb(230, 230, 230);
    private static readonly XColor LightBlue = XColor.FromArgb(230, 240, 255);

    public byte[] Generate(PayslipData data)
    {
        using var document = new PdfDocument();
        document.Info.Title = $"Lönespecifikation {data.Period}";
        document.Info.Author = "OpenHR";

        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);

        var fontTitle = new XFont("Arial", 18, XFontStyle.Bold);
        var fontSubtitle = new XFont("Arial", 12, XFontStyle.Regular);
        var fontSectionHeader = new XFont("Arial", 10, XFontStyle.Bold);
        var fontNormal = new XFont("Arial", 9, XFontStyle.Regular);
        var fontBold = new XFont("Arial", 9, XFontStyle.Bold);
        var fontNettoLabel = new XFont("Arial", 11, XFontStyle.Bold);

        double marginLeft = 40;
        double marginRight = page.Width - 40;
        double contentWidth = marginRight - marginLeft;
        double y = 40;

        // === HEADER ===
        gfx.DrawString("OpenHR", fontTitle, new XSolidBrush(DarkBlue), marginLeft, y);
        y += 22;

        gfx.DrawString($"Lönespecifikation — {data.Period}", fontSubtitle, new XSolidBrush(Grey), marginLeft, y);

        // Print date on the right
        var dateText = $"Utskriftsdatum: {DateTime.Now:yyyy-MM-dd}";
        var dateWidth = gfx.MeasureString(dateText, fontNormal).Width;
        gfx.DrawString(dateText, fontNormal, XBrushes.Black, marginRight - dateWidth, y);
        y += 10;

        // Horizontal line
        gfx.DrawLine(new XPen(XColor.FromArgb(200, 200, 200), 1), marginLeft, y, marginRight, y);
        y += 15;

        // === EMPLOYEE INFO ===
        gfx.DrawString("Anställd", fontSectionHeader, XBrushes.Black, marginLeft, y);
        gfx.DrawString("Enhet", fontSectionHeader, XBrushes.Black, marginLeft + contentWidth / 2, y);
        y += 14;

        gfx.DrawString(data.AnstallNamn, fontNormal, XBrushes.Black, marginLeft, y);
        gfx.DrawString(data.Enhet, fontNormal, XBrushes.Black, marginLeft + contentWidth / 2, y);
        y += 12;

        gfx.DrawString($"Personnummer: {data.Personnummer}", fontNormal, XBrushes.Black, marginLeft, y);
        y += 20;

        // === LINE ITEMS TABLE ===
        double col1 = marginLeft;
        double col2 = marginLeft + 60;
        double col3 = marginLeft + contentWidth - 200;
        double col4 = marginLeft + contentWidth - 130;
        double col5 = marginLeft + contentWidth - 50;

        // Table header background
        gfx.DrawRectangle(new XSolidBrush(LightGrey), marginLeft, y - 10, contentWidth, 16);

        gfx.DrawString("Löneart", fontBold, XBrushes.Black, col1, y);
        gfx.DrawString("Benämning", fontBold, XBrushes.Black, col2, y);
        DrawRightAligned(gfx, "Antal", fontBold, col3, y);
        DrawRightAligned(gfx, "Sats", fontBold, col4, y);
        DrawRightAligned(gfx, "Belopp", fontBold, col5, y);
        y += 14;

        // Table rows
        foreach (var rad in data.Rader)
        {
            gfx.DrawString(rad.Loneart, fontNormal, XBrushes.Black, col1, y);
            gfx.DrawString(rad.Benamning, fontNormal, XBrushes.Black, col2, y);
            DrawRightAligned(gfx, rad.Antal.ToString("0.##"), fontNormal, col3, y);
            DrawRightAligned(gfx, rad.Sats.ToString("N2"), fontNormal, col4, y);
            DrawRightAligned(gfx, rad.Belopp.ToString("N2"), fontNormal, col5, y);
            y += 13;

            // Row separator
            gfx.DrawLine(new XPen(XColor.FromArgb(220, 220, 220), 0.5), marginLeft, y - 3, marginRight, y - 3);
        }

        y += 10;

        // === SUMMARY SECTION ===
        double summaryHeight = 100;
        gfx.DrawRectangle(new XSolidBrush(LightBlue), marginLeft, y - 10, contentWidth, summaryHeight);

        double leftCol = marginLeft + 10;
        double rightCol = marginLeft + contentWidth / 2 + 10;
        double sy = y;

        // Left column: income items
        DrawSummaryLine(gfx, fontNormal, "Bruttolön", data.Brutto, leftCol, sy);
        sy += 14;

        if (data.OBTillagg > 0)
        {
            DrawSummaryLine(gfx, fontNormal, "OB-tillägg", data.OBTillagg, leftCol, sy);
            sy += 14;
        }

        if (data.Overtid > 0)
        {
            DrawSummaryLine(gfx, fontNormal, "Övertid", data.Overtid, leftCol, sy);
            sy += 14;
        }

        if (data.Sjuklon > 0)
        {
            DrawSummaryLine(gfx, fontNormal, "Sjuklön", data.Sjuklon, leftCol, sy);
            sy += 14;
        }

        if (data.Semesterlon > 0)
        {
            DrawSummaryLine(gfx, fontNormal, "Semesterlön", data.Semesterlon, leftCol, sy);
        }

        // Right column: deductions and netto
        double ry = y;
        DrawSummaryLine(gfx, fontNormal, "Skatteavdrag", -data.Skatt, rightCol, ry);
        ry += 20;

        // Separator line before Nettolön
        double lineStart = rightCol;
        double lineEnd = marginRight - 10;
        gfx.DrawLine(new XPen(XColors.Black, 1), lineStart, ry - 3, lineEnd, ry - 3);

        gfx.DrawString("Nettolön", fontNettoLabel, XBrushes.Black, rightCol, ry);
        DrawRightAligned(gfx, data.Netto.ToString("N2") + " kr", fontNettoLabel, marginRight - 10, ry);

        y += summaryHeight + 10;

        // === EMPLOYER COSTS ===
        gfx.DrawString("Arbetsgivarens kostnader", fontSectionHeader, XBrushes.Black, marginLeft, y);
        y += 14;

        gfx.DrawString($"Arbetsgivaravgifter: {data.Arbetsgivaravgifter:N2} kr", fontNormal, XBrushes.Black, marginLeft, y);
        gfx.DrawString($"Pensionsavsättning (AKAP-KR): {data.Pension:N2} kr", fontNormal, XBrushes.Black, marginLeft + contentWidth / 2, y);
        y += 20;

        // === FOOTER ===
        var footerText = $"OpenHR — Lönespecifikation — Genererad {DateTime.Now:yyyy-MM-dd HH:mm}";
        var footerWidth = gfx.MeasureString(footerText, fontNormal).Width;
        gfx.DrawString(footerText, fontNormal, new XSolidBrush(Grey),
            marginLeft + (contentWidth - footerWidth) / 2, page.Height - 30);

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static void DrawRightAligned(XGraphics gfx, string text, XFont font, double rightX, double y)
    {
        var width = gfx.MeasureString(text, font).Width;
        gfx.DrawString(text, font, XBrushes.Black, rightX - width, y);
    }

    private static void DrawSummaryLine(XGraphics gfx, XFont font, string label, decimal amount, double x, double y)
    {
        gfx.DrawString(label, font, XBrushes.Black, x, y);
        var amountText = amount.ToString("N2") + " kr";
        var amountWidth = gfx.MeasureString(amountText, font).Width;
        gfx.DrawString(amountText, font, XBrushes.Black, x + 200 - amountWidth, y);
    }
}
