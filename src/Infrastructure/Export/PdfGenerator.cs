namespace RegionHR.Infrastructure.Export;

public class PdfGenerator
{
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
        sb.AppendLine();
        sb.AppendLine("  PDF-generering kräver QuestPDF-paketet.");
        sb.AppendLine("  Installera med: dotnet add package QuestPDF");
        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] GenerateTjanstgoringsintyg(TjanstgoringsintyData data)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(DblRuler);
        sb.AppendLine("  TJÄNSTGÖRINGSINTYG");
        sb.AppendLine("  Västra Götalandsregionen — OpenHR");
        sb.AppendLine(DblRuler);
        sb.AppendLine();
        sb.AppendLine($"  Härmed intygas att {data.Namn} ({data.Personnummer})");
        sb.AppendLine($"  har varit anställd hos {data.Arbetsgivare}");
        sb.AppendLine($"  under perioden {data.StartDatum} — {data.SlutDatum}.");
        sb.AppendLine();
        sb.AppendLine(Ruler);
        sb.AppendLine("  ANSTÄLLNINGSUPPGIFTER");
        sb.AppendLine(Ruler);
        sb.AppendLine($"  Befattning:            {data.Befattning}");
        sb.AppendLine($"  Anställningsform:      {data.Anstallningsform}");
        sb.AppendLine($"  Sysselsättningsgrad:   {data.Sysselsattningsgrad}");
        sb.AppendLine();
        sb.AppendLine(Ruler);
        sb.AppendLine($"  Utfärdat:  {DateTime.Today:yyyy-MM-dd}");
        sb.AppendLine($"  Av:        HR-avdelningen");
        sb.AppendLine();
        sb.AppendLine();
        sb.AppendLine("  ___________________________");
        sb.AppendLine("  Arbetsgivare");
        sb.AppendLine(DblRuler);
        sb.AppendLine();
        sb.AppendLine("  PDF-generering kräver QuestPDF-paketet.");
        sb.AppendLine("  Installera med: dotnet add package QuestPDF");
        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] GenerateAnstallningsavtal(AnstallningsavtalData data)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine(DblRuler);
        sb.AppendLine("  ANSTÄLLNINGSAVTAL");
        sb.AppendLine("  Västra Götalandsregionen — OpenHR");
        sb.AppendLine(DblRuler);
        sb.AppendLine();
        sb.AppendLine(Ruler);
        sb.AppendLine("  PARTER");
        sb.AppendLine(Ruler);
        sb.AppendLine($"  Arbetsgivare:  {data.Arbetsgivare}");
        sb.AppendLine($"  Arbetstagare:  {data.Namn} ({data.Personnummer})");
        sb.AppendLine();
        sb.AppendLine(Ruler);
        sb.AppendLine("  ANSTÄLLNINGSVILLKOR");
        sb.AppendLine(Ruler);
        sb.AppendLine($"  Befattning:            {data.Befattning}");
        sb.AppendLine($"  Tillträdesdag:         {data.Tilltrade}");
        sb.AppendLine($"  Anställningsform:      {data.Anstallningsform}");
        sb.AppendLine($"  Sysselsättningsgrad:   {data.Grad}%");
        sb.AppendLine($"  Månadslön:             {data.Lon:N0} kr");
        sb.AppendLine($"  Kollektivavtal:        {data.Kollektivavtal}");
        sb.AppendLine();
        sb.AppendLine(Ruler);
        sb.AppendLine("  UNDERSKRIFTER");
        sb.AppendLine(Ruler);
        sb.AppendLine();
        sb.AppendLine("  ___________________________    ___________________________");
        sb.AppendLine("  Arbetsgivare                   Arbetstagare");
        sb.AppendLine();
        sb.AppendLine($"  Datum: {DateTime.Today:yyyy-MM-dd}");
        sb.AppendLine(DblRuler);
        sb.AppendLine();
        sb.AppendLine("  PDF-generering kräver QuestPDF-paketet.");
        sb.AppendLine("  Installera med: dotnet add package QuestPDF");
        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }
}

public record LonespecData(string Namn, string Personnummer, string Period, string Utbetalningsdag, decimal Grundlon, decimal OBTillagg, decimal Overtid, decimal Brutto, decimal KommunalSkatt, decimal StatligSkatt, decimal Netto, decimal Arbetsgivaravgift);
public record TjanstgoringsintyData(string Namn, string Personnummer, string Arbetsgivare, string StartDatum, string SlutDatum, string Befattning, string Anstallningsform, string Sysselsattningsgrad);
public record AnstallningsavtalData(string Namn, string Personnummer, string Arbetsgivare, string Befattning, string Tilltrade, string Anstallningsform, int Grad, decimal Lon, string Kollektivavtal);
