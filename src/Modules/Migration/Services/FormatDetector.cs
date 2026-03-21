using System.Text;
using RegionHR.Migration.Domain;

namespace RegionHR.Migration.Services;

/// <summary>
/// Auto-detekterar källsystem baserat på filinnehåll (header-analys).
/// Läser max 1 KB utan att konsumera strömmen (nollställer positionen).
/// </summary>
public static class FormatDetector
{
    /// <summary>
    /// Analyserar de första 1 024 byten i strömmen och returnerar
    /// det källsystem som bäst matchar filformatet.
    /// Strömmen nollställs efteråt så att parsern kan läsa från start.
    /// </summary>
    public static SourceSystem DetectFormat(Stream fileStream)
    {
        var buffer = new byte[1024];
        var read = fileStream.Read(buffer, 0, buffer.Length);
        fileStream.Position = 0; // Reset for actual parsing

        var header = Encoding.UTF8.GetString(buffer, 0, read);

        // XML-based formats
        if (header.TrimStart().StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) ||
            header.Contains("<paxml", StringComparison.OrdinalIgnoreCase) ||
            header.Contains("<personal", StringComparison.OrdinalIgnoreCase))
        {
            return SourceSystem.PAXml;
        }

        // SIE4i detection (Swedish accounting standard)
        if (header.Contains("#FLAGGA") || header.Contains("#SIETYP") || header.Contains("#PROGRAM"))
        {
            return SourceSystem.SIE4i;
        }

        // CSV / text-based formats — analyze first line
        var lines = header.Split('\n', 3);
        if (lines.Length >= 1)
        {
            var firstLine = lines[0].Trim().ToUpperInvariant();

            // HEROMA (semicolon-separated with PERSNR)
            if (firstLine.Contains("PERSNR") && firstLine.Contains(';'))
                return SourceSystem.HEROMA;

            // Personec P (semicolon-separated with PNR + FORNAMN)
            if (firstLine.Contains("PNR") && firstLine.Contains("FORNAMN") && firstLine.Contains(';'))
                return SourceSystem.PersonecP;

            // Hogia (PERSNR + ANSTTYP)
            if (firstLine.Contains("PERSNR") && firstLine.Contains("ANSTTYP"))
                return SourceSystem.Hogia;

            // Fortnox
            if (firstLine.Contains("ANSTÄLLNINGSNUMMER") || firstLine.Contains("FORTNOX"))
                return SourceSystem.Fortnox;

            // Workday (comma-separated with Employee_ID)
            if (firstLine.Contains("EMPLOYEE_ID") && firstLine.Contains(','))
                return SourceSystem.Workday;

            // SAP HCM
            if (firstLine.Contains("PERSONNEL NUMBER") || firstLine.Contains("PERS.NO."))
                return SourceSystem.SAP;

            // Oracle HCM
            if (firstLine.Contains("PERSON_NUMBER") || firstLine.Contains("ORACLE"))
                return SourceSystem.OracleHCM;
        }

        return SourceSystem.GenericCSV; // Fallback
    }

    /// <summary>
    /// Returnerar en användarvänlig beskrivning av det detekterade formatet.
    /// </summary>
    public static string GetFormatDescription(SourceSystem source) => source switch
    {
        SourceSystem.PAXml => "PAXml 2.0 — XML personaldata",
        SourceSystem.HEROMA => "HEROMA — semikolonseparerad CSV",
        SourceSystem.PersonecP => "Personec P — semikolonseparerad CSV",
        SourceSystem.Hogia => "Hogia Löneplus — CSV-export",
        SourceSystem.Fortnox => "Fortnox — lönexport",
        SourceSystem.SIE4i => "SIE typ 4i — bokföringsdata",
        SourceSystem.Workday => "Workday — kommaseparerad CSV",
        SourceSystem.SAP => "SAP HCM — personalexport",
        SourceSystem.OracleHCM => "Oracle HCM — personalexport",
        SourceSystem.GenericCSV => "Generisk CSV",
        _ => source.ToString()
    };
}
