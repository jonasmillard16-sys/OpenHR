using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Adapters;

/// <summary>
/// Adapter för SIE typ 4i — svensk standard för bokföringsdata.
/// Parsear #VER (verifikationer) och #TRANS (transaktioner) taggar.
/// </summary>
public sealed class SIE4iAdapter : IMigrationAdapter
{
    public SourceSystem Source => SourceSystem.SIE4i;

    public async Task<ParsedMigrationData> ParseAsync(Stream fileStream, CancellationToken ct = default)
    {
        var result = new ParsedMigrationData();
        using var reader = new StreamReader(fileStream, System.Text.Encoding.GetEncoding("iso-8859-1"));

        string? currentVerSerie = null;
        string? currentVerNr = null;
        string? currentVerDatum = null;
        string? currentVerText = null;

        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct);

            if (string.IsNullOrWhiteSpace(line))
                continue;

            line = line.Trim();

            // Parse #VER — verifikation header
            if (line.StartsWith("#VER"))
            {
                var parts = ParseSIELine(line);
                currentVerSerie = parts.Length > 1 ? parts[1] : "";
                currentVerNr = parts.Length > 2 ? parts[2] : "";
                currentVerDatum = parts.Length > 3 ? parts[3] : "";
                currentVerText = parts.Length > 4 ? parts[4] : "";

                var verRecord = new ParsedRecord { EntityType = "AccountingVoucher" };
                verRecord.Fields["VerifikationsSerie"] = currentVerSerie;
                verRecord.Fields["VerifikationsNummer"] = currentVerNr;
                if (!string.IsNullOrWhiteSpace(currentVerDatum))
                    verRecord.Fields["Datum"] = currentVerDatum;
                if (!string.IsNullOrWhiteSpace(currentVerText))
                    verRecord.Fields["Text"] = currentVerText;

                result.Records.Add(verRecord);
            }
            // Parse #TRANS — transaktion within a verifikation
            else if (line.StartsWith("#TRANS"))
            {
                var parts = ParseSIELine(line);
                var record = new ParsedRecord { EntityType = "AccountingTransaction" };

                if (parts.Length > 1)
                    record.Fields["Konto"] = parts[1];
                if (parts.Length > 2)
                    record.Fields["Dimensioner"] = parts[2];
                if (parts.Length > 3)
                    record.Fields["Belopp"] = parts[3];
                if (parts.Length > 4)
                    record.Fields["TransDatum"] = parts[4];
                if (parts.Length > 5)
                    record.Fields["Text"] = parts[5];

                // Link to parent verification
                if (!string.IsNullOrWhiteSpace(currentVerSerie))
                    record.Fields["VerifikationsSerie"] = currentVerSerie;
                if (!string.IsNullOrWhiteSpace(currentVerNr))
                    record.Fields["VerifikationsNummer"] = currentVerNr;

                result.Records.Add(record);
            }
            // Parse #KONTO — kontoplan
            else if (line.StartsWith("#KONTO"))
            {
                var parts = ParseSIELine(line);
                if (parts.Length >= 3)
                {
                    var record = new ParsedRecord { EntityType = "Account" };
                    record.Fields["Kontonummer"] = parts[1];
                    record.Fields["Kontonamn"] = parts[2];
                    result.Records.Add(record);
                }
            }
        }

        result.TotalRows = result.Records.Count;
        return result;
    }

    public MigrationMapping[] GetDefaultMappings()
    {
        var jobId = MigrationJobId.New();
        return
        [
            MigrationMapping.Skapa(jobId, "VerifikationsSerie", "VerifikationsSerie"),
            MigrationMapping.Skapa(jobId, "VerifikationsNummer", "VerifikationsNummer"),
            MigrationMapping.Skapa(jobId, "Datum", "Datum"),
            MigrationMapping.Skapa(jobId, "Konto", "Konto"),
            MigrationMapping.Skapa(jobId, "Belopp", "Belopp"),
            MigrationMapping.Skapa(jobId, "Text", "Text"),
        ];
    }

    /// <summary>
    /// Parsear en SIE-rad med hänsyn till citattecken och klammerpar.
    /// Exempel: #VER "A" "1" 20240101 "Löneutbetalning" → ["#VER", "A", "1", "20240101", "Löneutbetalning"]
    /// </summary>
    private static string[] ParseSIELine(string line)
    {
        var parts = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;
        var inBraces = false;

        foreach (var ch in line)
        {
            if (ch == '"' && !inBraces)
            {
                inQuotes = !inQuotes;
                continue;
            }
            if (ch == '{' && !inQuotes)
            {
                inBraces = true;
                continue;
            }
            if (ch == '}' && !inQuotes)
            {
                inBraces = false;
                parts.Add(current.ToString().Trim());
                current.Clear();
                continue;
            }
            if (ch == ' ' && !inQuotes && !inBraces)
            {
                if (current.Length > 0)
                {
                    parts.Add(current.ToString().Trim());
                    current.Clear();
                }
                continue;
            }

            current.Append(ch);
        }

        if (current.Length > 0)
            parts.Add(current.ToString().Trim());

        return parts.ToArray();
    }
}
