using RegionHR.Migration.Adapters;

namespace RegionHR.Migration.Services;

/// <summary>
/// Matchar importposter mot befintliga anställda i databasen via personnummer.
/// Returnerar träffar så att användaren kan välja "Uppdatera" eller "Hoppa över".
/// </summary>
public sealed class DuplicateDetector
{
    /// <summary>
    /// Söker igenom alla Employee-poster i <paramref name="records"/> och
    /// jämför mot befintliga personnummer i databasen via <paramref name="existingPnrLookup"/>.
    /// </summary>
    /// <param name="existingPnrLookup">
    /// Dictionary med normaliserat personnummer (12-siffrig sträng) → befintligt EmployeeId.
    /// Byggs av anroparen via DbContext-query.
    /// </param>
    /// <param name="records">Parsade poster att kontrollera.</param>
    public List<DuplicateMatch> FindDuplicates(
        Dictionary<string, Guid> existingPnrLookup,
        List<ParsedRecord> records)
    {
        var duplicates = new List<DuplicateMatch>();

        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            if (record.EntityType != "Employee")
                continue;

            if (!TryGetPnr(record, out var rawPnr))
                continue;

            var cleanPnr = NormalizePnr(rawPnr);
            if (string.IsNullOrEmpty(cleanPnr))
                continue;

            if (existingPnrLookup.TryGetValue(cleanPnr, out var existingId))
            {
                duplicates.Add(new DuplicateMatch(
                    RecordIndex: i,
                    Record: record,
                    ExistingEmployeeId: existingId,
                    Personnummer: rawPnr));
            }
        }

        return duplicates;
    }

    private static bool TryGetPnr(ParsedRecord record, out string pnr)
    {
        if (record.Fields.TryGetValue("Personnummer", out var v1) && !string.IsNullOrWhiteSpace(v1))
        {
            pnr = v1;
            return true;
        }
        if (record.Fields.TryGetValue("PERSNR", out var v2) && !string.IsNullOrWhiteSpace(v2))
        {
            pnr = v2;
            return true;
        }
        pnr = "";
        return false;
    }

    /// <summary>
    /// Normaliserar till 12-siffrig form (YYYYMMDDNNNN) för konsekvent jämförelse.
    /// </summary>
    internal static string NormalizePnr(string input)
    {
        var cleaned = input.Replace("-", "").Replace(" ", "").Replace("+", "");

        if (cleaned.Length == 10 && cleaned.All(char.IsDigit))
        {
            // Gissa århundrade — samma logik som SharedKernel Personnummer
            var twoDigitYear = int.Parse(cleaned[..2]);
            var currentYear = DateTime.Now.Year % 100;
            var currentCentury = DateTime.Now.Year / 100;
            var century = twoDigitYear <= currentYear ? currentCentury : (currentCentury - 1);
            cleaned = $"{century}{cleaned}";
        }

        if (cleaned.Length == 12 && cleaned.All(char.IsDigit))
            return cleaned;

        return string.Empty;
    }
}

/// <summary>
/// Representerar en matchning mellan en importpost och en befintlig anställd.
/// </summary>
public sealed record DuplicateMatch(
    int RecordIndex,
    ParsedRecord Record,
    Guid ExistingEmployeeId,
    string Personnummer);
