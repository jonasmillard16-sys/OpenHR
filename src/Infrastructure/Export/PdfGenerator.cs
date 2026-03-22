using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace RegionHR.Infrastructure.Export;

public class PdfGenerator
{
    private static readonly XColor DarkBlue = XColor.FromArgb(0, 51, 102);
    private static readonly XColor Grey = XColor.FromArgb(128, 128, 128);
    private static readonly XColor LightGrey = XColor.FromArgb(230, 230, 230);

    private const string Ruler = "────────────────────────────────────────────────────";
    private const string DblRuler = "════════════════════════════════════════════════════";

    public byte[] GenerateLonespecifikation(LonespecData data)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(DblRuler);
        sb.AppendLine("  LÖNESPECIFIKATION");
        sb.AppendLine("  Västra Götalandsregionen — OpenHR");
        sb.AppendLine(DblRuler);
        sb.AppendLine();
        sb.AppendLine($"  Namn:              {data.Namn}");
        sb.AppendLine($"  Personnummer:      {data.Personnummer}");
        sb.AppendLine($"  Period:            {data.Period}");
        sb.AppendLine($"  Utbetalningsdag:   {data.Utbetalningsdag}");
        sb.AppendLine();
        sb.AppendLine(Ruler);
        sb.AppendLine("  INKOMSTER");
        sb.AppendLine(Ruler);
        sb.AppendLine($"  Grundlön               {data.Grundlon,12:N0} kr");
        sb.AppendLine($"  OB-tillägg             {data.OBTillagg,12:N0} kr");
        sb.AppendLine($"  Övertid                {data.Overtid,12:N0} kr");
        sb.AppendLine(Ruler);
        sb.AppendLine($"  BRUTTO                 {data.Brutto,12:N0} kr");
        sb.AppendLine();
        sb.AppendLine(Ruler);
        sb.AppendLine("  AVDRAG");
        sb.AppendLine(Ruler);
        sb.AppendLine($"  Kommunalskatt         -{data.KommunalSkatt,11:N0} kr");
        sb.AppendLine($"  Statlig skatt         -{data.StatligSkatt,11:N0} kr");
        sb.AppendLine(Ruler);
        sb.AppendLine($"  NETTO                  {data.Netto,12:N0} kr");
        sb.AppendLine();
        sb.AppendLine(Ruler);
        sb.AppendLine($"  Arbetsgivaravgift      {data.Arbetsgivaravgift,12:N0} kr");
        sb.AppendLine(DblRuler);
        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] GenerateTjanstgoringsintyg(TjanstgoringsintyData data)
    {
        using var document = new PdfDocument();
        document.Info.Title = "Tjänstgöringsintyg";
        document.Info.Author = "OpenHR";

        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);

        var fontTitle = new XFont("Arial", 18, XFontStyle.Bold);
        var fontSubtitle = new XFont("Arial", 11, XFontStyle.Regular);
        var fontSectionHeader = new XFont("Arial", 10, XFontStyle.Bold);
        var fontNormal = new XFont("Arial", 10, XFontStyle.Regular);
        var fontSmall = new XFont("Arial", 9, XFontStyle.Regular);

        double marginLeft = 50;
        double marginRight = page.Width - 50;
        double contentWidth = marginRight - marginLeft;
        double y = 50;

        // Header
        gfx.DrawString("OpenHR — Västra Götalandsregionen", fontTitle, new XSolidBrush(DarkBlue), marginLeft, y);
        y += 25;
        gfx.DrawLine(new XPen(XColor.FromArgb(200, 200, 200), 1), marginLeft, y, marginRight, y);
        y += 20;

        // Document title
        gfx.DrawString("TJÄNSTGÖRINGSINTYG", new XFont("Arial", 14, XFontStyle.Bold), XBrushes.Black, marginLeft, y);
        y += 30;

        // Introductory statement
        gfx.DrawString($"Härmed intygas att {data.Namn} (personnummer {data.Personnummer})", fontNormal, XBrushes.Black, marginLeft, y);
        y += 16;
        gfx.DrawString($"har varit anställd hos {data.Arbetsgivare}", fontNormal, XBrushes.Black, marginLeft, y);
        y += 16;
        gfx.DrawString($"under perioden {data.StartDatum} — {data.SlutDatum}.", fontNormal, XBrushes.Black, marginLeft, y);
        y += 30;

        // Section: Employment details
        gfx.DrawRectangle(new XSolidBrush(LightGrey), marginLeft, y - 12, contentWidth, 18);
        gfx.DrawString("ANSTÄLLNINGSUPPGIFTER", fontSectionHeader, XBrushes.Black, marginLeft + 4, y);
        y += 20;

        DrawLabelValue(gfx, fontNormal, "Befattning:", data.Befattning, marginLeft, y, 160);
        y += 18;
        DrawLabelValue(gfx, fontNormal, "Anställningsform:", data.Anstallningsform, marginLeft, y, 160);
        y += 18;
        DrawLabelValue(gfx, fontNormal, "Sysselsättningsgrad:", data.Sysselsattningsgrad, marginLeft, y, 160);
        y += 35;

        // Section: Issue info
        gfx.DrawLine(new XPen(XColor.FromArgb(200, 200, 200), 0.5), marginLeft, y, marginRight, y);
        y += 20;
        DrawLabelValue(gfx, fontNormal, "Utfärdat:", DateTime.Today.ToString("yyyy-MM-dd"), marginLeft, y, 100);
        y += 18;
        DrawLabelValue(gfx, fontNormal, "Av:", "HR-avdelningen", marginLeft, y, 100);
        y += 60;

        // Signature line
        gfx.DrawLine(new XPen(XColors.Black, 0.75), marginLeft, y, marginLeft + 200, y);
        y += 14;
        gfx.DrawString("Arbetsgivare", fontSmall, new XSolidBrush(Grey), marginLeft, y);

        // Footer
        var footerText = $"OpenHR — Tjänstgöringsintyg — Genererad {DateTime.Now:yyyy-MM-dd HH:mm}";
        var footerWidth = gfx.MeasureString(footerText, fontSmall).Width;
        gfx.DrawString(footerText, fontSmall, new XSolidBrush(Grey),
            marginLeft + (contentWidth - footerWidth) / 2, page.Height - 35);

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    public byte[] GenerateAnstallningsavtal(AnstallningsavtalData data)
    {
        using var document = new PdfDocument();
        document.Info.Title = "Anställningsavtal";
        document.Info.Author = "OpenHR";

        var page = document.AddPage();
        page.Size = PdfSharpCore.PageSize.A4;

        using var gfx = XGraphics.FromPdfPage(page);

        var fontTitle = new XFont("Arial", 18, XFontStyle.Bold);
        var fontSectionHeader = new XFont("Arial", 10, XFontStyle.Bold);
        var fontNormal = new XFont("Arial", 10, XFontStyle.Regular);
        var fontSmall = new XFont("Arial", 9, XFontStyle.Regular);

        double marginLeft = 50;
        double marginRight = page.Width - 50;
        double contentWidth = marginRight - marginLeft;
        double y = 50;

        // Header
        gfx.DrawString("OpenHR — Västra Götalandsregionen", fontTitle, new XSolidBrush(DarkBlue), marginLeft, y);
        y += 25;
        gfx.DrawLine(new XPen(XColor.FromArgb(200, 200, 200), 1), marginLeft, y, marginRight, y);
        y += 20;

        // Document title
        gfx.DrawString("ANSTÄLLNINGSAVTAL", new XFont("Arial", 14, XFontStyle.Bold), XBrushes.Black, marginLeft, y);
        y += 30;

        // Section: Parties
        gfx.DrawRectangle(new XSolidBrush(LightGrey), marginLeft, y - 12, contentWidth, 18);
        gfx.DrawString("PARTER", fontSectionHeader, XBrushes.Black, marginLeft + 4, y);
        y += 20;

        DrawLabelValue(gfx, fontNormal, "Arbetsgivare:", data.Arbetsgivare, marginLeft, y, 160);
        y += 18;
        DrawLabelValue(gfx, fontNormal, "Arbetstagare:", $"{data.Namn} ({data.Personnummer})", marginLeft, y, 160);
        y += 30;

        // Section: Employment conditions
        gfx.DrawRectangle(new XSolidBrush(LightGrey), marginLeft, y - 12, contentWidth, 18);
        gfx.DrawString("ANSTÄLLNINGSVILLKOR", fontSectionHeader, XBrushes.Black, marginLeft + 4, y);
        y += 20;

        DrawLabelValue(gfx, fontNormal, "Befattning:", data.Befattning, marginLeft, y, 160);
        y += 18;
        DrawLabelValue(gfx, fontNormal, "Tillträdesdag:", data.Tilltrade, marginLeft, y, 160);
        y += 18;
        DrawLabelValue(gfx, fontNormal, "Anställningsform:", data.Anstallningsform, marginLeft, y, 160);
        y += 18;
        DrawLabelValue(gfx, fontNormal, "Sysselsättningsgrad:", $"{data.Grad}%", marginLeft, y, 160);
        y += 18;
        DrawLabelValue(gfx, fontNormal, "Månadslön:", $"{data.Lon:N0} kr", marginLeft, y, 160);
        y += 18;
        DrawLabelValue(gfx, fontNormal, "Kollektivavtal:", data.Kollektivavtal, marginLeft, y, 160);
        y += 35;

        // Section: Signatures
        gfx.DrawRectangle(new XSolidBrush(LightGrey), marginLeft, y - 12, contentWidth, 18);
        gfx.DrawString("UNDERSKRIFTER", fontSectionHeader, XBrushes.Black, marginLeft + 4, y);
        y += 30;

        double sigColLeft = marginLeft;
        double sigColRight = marginLeft + contentWidth / 2 + 20;

        gfx.DrawLine(new XPen(XColors.Black, 0.75), sigColLeft, y, sigColLeft + 180, y);
        gfx.DrawLine(new XPen(XColors.Black, 0.75), sigColRight, y, sigColRight + 180, y);
        y += 14;
        gfx.DrawString("Arbetsgivare", fontSmall, new XSolidBrush(Grey), sigColLeft, y);
        gfx.DrawString("Arbetstagare", fontSmall, new XSolidBrush(Grey), sigColRight, y);
        y += 30;

        DrawLabelValue(gfx, fontNormal, "Datum:", DateTime.Today.ToString("yyyy-MM-dd"), marginLeft, y, 80);

        // Footer
        var footerText = $"OpenHR — Anställningsavtal — Genererad {DateTime.Now:yyyy-MM-dd HH:mm}";
        var footerWidth = gfx.MeasureString(footerText, fontSmall).Width;
        gfx.DrawString(footerText, fontSmall, new XSolidBrush(Grey),
            marginLeft + (contentWidth - footerWidth) / 2, page.Height - 35);

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static void DrawLabelValue(XGraphics gfx, XFont font, string label, string value, double x, double y, double valueOffset)
    {
        var labelFont = new XFont("Arial", font.Size, XFontStyle.Bold);
        gfx.DrawString(label, labelFont, XBrushes.Black, x, y);
        gfx.DrawString(value, font, XBrushes.Black, x + valueOffset, y);
    }
}

public record LonespecData(string Namn, string Personnummer, string Period, string Utbetalningsdag, decimal Grundlon, decimal OBTillagg, decimal Overtid, decimal Brutto, decimal KommunalSkatt, decimal StatligSkatt, decimal Netto, decimal Arbetsgivaravgift);
public record TjanstgoringsintyData(string Namn, string Personnummer, string Arbetsgivare, string StartDatum, string SlutDatum, string Befattning, string Anstallningsform, string Sysselsattningsgrad);
public record AnstallningsavtalData(string Namn, string Personnummer, string Arbetsgivare, string Befattning, string Tilltrade, string Anstallningsform, int Grad, decimal Lon, string Kollektivavtal);
