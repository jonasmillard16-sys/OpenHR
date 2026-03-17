namespace RegionHR.Infrastructure.Export;

public class PdfGenerator
{
    public byte[] GenerateLonespecifikation(LonespecData data)
    {
        // Generate a simple text-based "PDF" (plain text document)
        // In production this would use QuestPDF or similar
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("╔══════════════════════════════════════════╗");
        sb.AppendLine("║           LÖNESPECIFIKATION              ║");
        sb.AppendLine("╚══════════════════════════════════════════╝");
        sb.AppendLine($"Namn: {data.Namn}");
        sb.AppendLine($"Personnummer: {data.Personnummer}");
        sb.AppendLine($"Period: {data.Period}");
        sb.AppendLine($"Utbetalningsdag: {data.Utbetalningsdag}");
        sb.AppendLine("──────────────────────────────────────────");
        sb.AppendLine($"Grundlön:              {data.Grundlon,12:N0} kr");
        sb.AppendLine($"OB-tillägg:            {data.OBTillagg,12:N0} kr");
        sb.AppendLine($"Övertid:               {data.Overtid,12:N0} kr");
        sb.AppendLine("──────────────────────────────────────────");
        sb.AppendLine($"BRUTTO:                {data.Brutto,12:N0} kr");
        sb.AppendLine($"Kommunalskatt:        -{data.KommunalSkatt,11:N0} kr");
        sb.AppendLine($"Statlig skatt:        -{data.StatligSkatt,11:N0} kr");
        sb.AppendLine("──────────────────────────────────────────");
        sb.AppendLine($"NETTO:                 {data.Netto,12:N0} kr");
        sb.AppendLine($"Arbetsgivaravgift:     {data.Arbetsgivaravgift,12:N0} kr");
        sb.AppendLine("══════════════════════════════════════════");
        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] GenerateTjanstgoringsintyg(TjanstgoringsintyData data)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("╔══════════════════════════════════════════╗");
        sb.AppendLine("║         TJÄNSTGÖRINGSINTYG               ║");
        sb.AppendLine("╚══════════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine($"Härmed intygas att {data.Namn} ({data.Personnummer})");
        sb.AppendLine($"har varit anställd hos {data.Arbetsgivare}");
        sb.AppendLine($"under perioden {data.StartDatum} — {data.SlutDatum}.");
        sb.AppendLine();
        sb.AppendLine($"Befattning: {data.Befattning}");
        sb.AppendLine($"Anställningsform: {data.Anstallningsform}");
        sb.AppendLine($"Sysselsättningsgrad: {data.Sysselsattningsgrad}");
        sb.AppendLine();
        sb.AppendLine($"Utfärdat: {DateTime.Today:yyyy-MM-dd}");
        sb.AppendLine($"Av: HR-avdelningen");
        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] GenerateAnstallningsavtal(AnstallningsavtalData data)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("╔══════════════════════════════════════════╗");
        sb.AppendLine("║         ANSTÄLLNINGSAVTAL                ║");
        sb.AppendLine("╚══════════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine($"Arbetsgivare: {data.Arbetsgivare}");
        sb.AppendLine($"Arbetstagare: {data.Namn} ({data.Personnummer})");
        sb.AppendLine();
        sb.AppendLine($"Befattning: {data.Befattning}");
        sb.AppendLine($"Tillträdesdag: {data.Tilltrade}");
        sb.AppendLine($"Anställningsform: {data.Anstallningsform}");
        sb.AppendLine($"Sysselsättningsgrad: {data.Grad}%");
        sb.AppendLine($"Månadslön: {data.Lon:N0} kr");
        sb.AppendLine($"Kollektivavtal: {data.Kollektivavtal}");
        sb.AppendLine();
        sb.AppendLine("Underskrifter:");
        sb.AppendLine("___________________________    ___________________________");
        sb.AppendLine("Arbetsgivare                   Arbetstagare");
        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }
}

public record LonespecData(string Namn, string Personnummer, string Period, string Utbetalningsdag, decimal Grundlon, decimal OBTillagg, decimal Overtid, decimal Brutto, decimal KommunalSkatt, decimal StatligSkatt, decimal Netto, decimal Arbetsgivaravgift);
public record TjanstgoringsintyData(string Namn, string Personnummer, string Arbetsgivare, string StartDatum, string SlutDatum, string Befattning, string Anstallningsform, string Sysselsattningsgrad);
public record AnstallningsavtalData(string Namn, string Personnummer, string Arbetsgivare, string Befattning, string Tilltrade, string Anstallningsform, int Grad, decimal Lon, string Kollektivavtal);
