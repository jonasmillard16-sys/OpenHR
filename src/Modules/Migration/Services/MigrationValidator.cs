using System.Globalization;
using RegionHR.Migration.Adapters;
using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Services;

/// <summary>
/// Djup validering av migrerade poster:
///   - Personnummer (längd, Luhn, korrigeringsförslag)
///   - Duplikat inom samma fil
///   - Datum (YYYY-MM-DD / YYYYMMDD)
///   - Belopp (sv-SE decimalformat)
///   - Obligatoriska fält (Employee-poster)
/// </summary>
public sealed class MigrationValidator
{
    private static readonly CultureInfo SvSE = CultureInfo.GetCultureInfo("sv-SE");

    /// <summary>
    /// Kör samtliga valideringsregler på alla poster och returnerar en lista
    /// av <see cref="MigrationValidationError"/> (kan vara tom om allt är OK).
    /// </summary>
    public List<MigrationValidationError> ValidateRecords(
        MigrationJobId jobId,
        List<ParsedRecord> records)
    {
        var errors = new List<MigrationValidationError>();
        var seenPnr = new Dictionary<string, int>(); // cleanPnr → first row

        for (int i = 0; i < records.Count; i++)
        {
            var record = records[i];
            var rowNumber = i + 1;

            // --- Personnummer validation ---
            if (TryGetPnrField(record, out var pnr))
            {
                ValidatePersonnummer(jobId, rowNumber, pnr, seenPnr, errors);
            }

            // --- Required fields for Employee records ---
            if (record.EntityType == "Employee")
            {
                ValidateRequired(jobId, rowNumber, record, "Fornamn", errors);
                ValidateRequired(jobId, rowNumber, record, "Efternamn", errors);
            }

            // --- Date validation ---
            foreach (var (key, value) in record.Fields)
            {
                if (IsDateField(key) && !string.IsNullOrEmpty(value) && !TryParseDate(value))
                {
                    errors.Add(MigrationValidationError.Skapa(
                        jobId, rowNumber, key,
                        "OgiltigtDatumformat",
                        value,
                        "Förväntat: YYYY-MM-DD eller YYYYMMDD"));
                }
            }

            // --- Amount validation ---
            foreach (var (key, value) in record.Fields)
            {
                if (IsAmountField(key) && !string.IsNullOrEmpty(value) &&
                    !decimal.TryParse(value, NumberStyles.Any, SvSE, out _) &&
                    !decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    errors.Add(MigrationValidationError.Skapa(
                        jobId, rowNumber, key,
                        "OgiltigtBelopp",
                        value,
                        "Kontrollera decimalformat (punkt eller komma)"));
                }
            }
        }

        return errors;
    }

    // ------------------------------------------------
    // Personnummer helpers
    // ------------------------------------------------

    private static bool TryGetPnrField(ParsedRecord record, out string pnr)
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

    private static void ValidatePersonnummer(
        MigrationJobId jobId,
        int rowNumber,
        string pnr,
        Dictionary<string, int> seenPnr,
        List<MigrationValidationError> errors)
    {
        var cleanPnr = pnr.Replace("-", "").Replace(" ", "").Replace("+", "");

        // Length check
        if (cleanPnr.Length != 12 && cleanPnr.Length != 10)
        {
            errors.Add(MigrationValidationError.Skapa(
                jobId, rowNumber, "Personnummer",
                "OgiltigLängd",
                pnr,
                $"Personnummer ska vara 10 eller 12 siffror, fick {cleanPnr.Length}"));
            return;
        }

        if (!cleanPnr.All(char.IsDigit))
        {
            errors.Add(MigrationValidationError.Skapa(
                jobId, rowNumber, "Personnummer",
                "IckeNumeriskt",
                pnr,
                "Personnummer får bara innehålla siffror (plus ev. bindestreck)"));
            return;
        }

        // Normalize to 10 digits for Luhn
        var tenDigit = cleanPnr.Length == 12 ? cleanPnr[2..] : cleanPnr;

        if (!LuhnCheck(tenDigit))
        {
            var corrected = TryCorrectCheckDigit(tenDigit);
            var suggestion = corrected != null
                ? $"Ogiltig kontrollsiffra — menade du {FormatPnr(cleanPnr.Length == 12 ? cleanPnr[..2] + corrected : corrected)}?"
                : "Ogiltig kontrollsiffra";

            errors.Add(MigrationValidationError.Skapa(
                jobId, rowNumber, "Personnummer",
                "OgiltigKontrollsiffra",
                pnr,
                suggestion));
        }

        // Duplicate within same file
        var normalizedKey = cleanPnr.Length == 10
            ? cleanPnr
            : cleanPnr[2..]; // always compare on 10-digit form

        if (seenPnr.TryGetValue(normalizedKey, out var firstRow))
        {
            errors.Add(MigrationValidationError.Skapa(
                jobId, rowNumber, "Personnummer",
                "Dubblett",
                pnr,
                $"Personnummer {pnr} förekommer även på rad {firstRow}"));
        }
        else
        {
            seenPnr[normalizedKey] = rowNumber;
        }
    }

    /// <summary>Luhn-algoritm på exakt 10 siffror (YYMMDDNNNC).</summary>
    internal static bool LuhnCheck(string digits)
    {
        if (digits.Length != 10 || !digits.All(char.IsDigit))
            return false;

        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
        {
            var digit = digits[i] - '0';
            var multiplied = digit * (i % 2 == 0 ? 2 : 1);
            sum += multiplied > 9 ? multiplied - 9 : multiplied;
        }
        return sum % 10 == 0;
    }

    /// <summary>
    /// Försöker beräkna korrekt kontrollsiffra genom att ersätta den sista siffran.
    /// Returnerar null om de första 9 siffrorna inte ger ett rimligt datum.
    /// </summary>
    internal static string? TryCorrectCheckDigit(string tenDigit)
    {
        if (tenDigit.Length != 10 || !tenDigit[..9].All(char.IsDigit))
            return null;

        var prefix = tenDigit[..9];
        int sum = 0;
        for (int i = 0; i < 9; i++)
        {
            int d = prefix[i] - '0';
            int m = d * (i % 2 == 0 ? 2 : 1);
            sum += m > 9 ? m - 9 : m;
        }
        int check = (10 - (sum % 10)) % 10;
        return prefix + check;
    }

    private static string FormatPnr(string digits)
    {
        if (digits.Length == 12)
            return $"{digits[..8]}-{digits[8..]}";
        if (digits.Length == 10)
            return $"{digits[..6]}-{digits[6..]}";
        return digits;
    }

    // ------------------------------------------------
    // Required field helper
    // ------------------------------------------------

    private static void ValidateRequired(
        MigrationJobId jobId,
        int rowNumber,
        ParsedRecord record,
        string fieldName,
        List<MigrationValidationError> errors)
    {
        if (!record.Fields.ContainsKey(fieldName) || string.IsNullOrWhiteSpace(record.Fields[fieldName]))
        {
            errors.Add(MigrationValidationError.Skapa(
                jobId, rowNumber, fieldName,
                "ObligatorisktFältSaknas",
                null,
                $"Fältet {fieldName} är obligatoriskt"));
        }
    }

    // ------------------------------------------------
    // Date parsing
    // ------------------------------------------------

    private static bool IsDateField(string key)
    {
        return key.Contains("Datum", StringComparison.OrdinalIgnoreCase) ||
               key.Contains("DAT", StringComparison.OrdinalIgnoreCase) ||
               key.Contains("Period", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool TryParseDate(string value)
    {
        var clean = value.Trim();

        // YYYY-MM-DD
        if (DateOnly.TryParseExact(clean, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out _))
            return true;

        // YYYYMMDD
        if (DateOnly.TryParseExact(clean, "yyyyMMdd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out _))
            return true;

        // YY-MM-DD
        if (DateOnly.TryParseExact(clean, "yy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out _))
            return true;

        // YYMMDD
        if (DateOnly.TryParseExact(clean, "yyMMdd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out _))
            return true;

        // ISO 8601 datetime
        if (DateTime.TryParse(clean, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out _))
            return true;

        return false;
    }

    // ------------------------------------------------
    // Amount field detection
    // ------------------------------------------------

    private static bool IsAmountField(string key)
    {
        var upper = key.ToUpperInvariant();
        return upper.Contains("LON") ||
               upper.Contains("BELOPP") ||
               upper.Contains("MANADSLON") ||
               upper.Contains("TIMLON") ||
               upper.Contains("ANTAL") ||
               upper.Contains("AMOUNT") ||
               upper.Contains("SALARY");
    }
}
