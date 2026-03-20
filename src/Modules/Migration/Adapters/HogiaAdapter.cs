using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Adapters;

/// <summary>
/// Adapter för Hogia Lön — semikolonseparerad CSV-export.
/// Förväntade kolumner: PERSNR;FORNAMN;EFTERNAMN;ANSTTYP;AVDELNING;MANLON;SKATTETABELL;PROCENTSATS
/// </summary>
public sealed class HogiaAdapter : IMigrationAdapter
{
    public SourceSystem Source => SourceSystem.Hogia;

    private static readonly string[] ExpectedHeaders =
        ["PERSNR", "FORNAMN", "EFTERNAMN", "ANSTTYP", "AVDELNING", "MANLON", "SKATTETABELL", "PROCENTSATS"];

    private static readonly Dictionary<string, string> FieldMap = new()
    {
        ["PERSNR"] = "Personnummer",
        ["FORNAMN"] = "Fornamn",
        ["EFTERNAMN"] = "Efternamn",
        ["ANSTTYP"] = "Anstallningsform",
        ["AVDELNING"] = "Enhetskod",
        ["MANLON"] = "Manadslon",
        ["SKATTETABELL"] = "Skattetabell",
        ["PROCENTSATS"] = "Procentsats"
    };

    public async Task<ParsedMigrationData> ParseAsync(Stream fileStream, CancellationToken ct = default)
    {
        var result = new ParsedMigrationData();
        using var reader = new StreamReader(fileStream);
        var headerLine = await reader.ReadLineAsync(ct);

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            result.Warnings.Add("Tom fil — ingen header hittades");
            return result;
        }

        var headers = headerLine.Split(';').Select(h => h.Trim().ToUpperInvariant()).ToArray();

        foreach (var expected in ExpectedHeaders)
        {
            if (!headers.Contains(expected))
            {
                result.Warnings.Add($"Kolumn '{expected}' saknas i header");
            }
        }

        var rowNumber = 1;
        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct);
            rowNumber++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = line.Split(';');
            var record = new ParsedRecord { EntityType = "Employee" };

            for (int i = 0; i < Math.Min(headers.Length, values.Length); i++)
            {
                var header = headers[i];
                var value = values[i].Trim();

                if (!string.IsNullOrWhiteSpace(value) && FieldMap.TryGetValue(header, out var mappedField))
                {
                    record.Fields[mappedField] = value;
                }
            }

            result.Records.Add(record);
        }

        result.TotalRows = result.Records.Count;
        return result;
    }

    public MigrationMapping[] GetDefaultMappings()
    {
        var jobId = MigrationJobId.New();
        return
        [
            MigrationMapping.Skapa(jobId, "PERSNR", "Personnummer"),
            MigrationMapping.Skapa(jobId, "FORNAMN", "Fornamn"),
            MigrationMapping.Skapa(jobId, "EFTERNAMN", "Efternamn"),
            MigrationMapping.Skapa(jobId, "ANSTTYP", "Anstallningsform"),
            MigrationMapping.Skapa(jobId, "AVDELNING", "Enhetskod"),
            MigrationMapping.Skapa(jobId, "MANLON", "Manadslon"),
            MigrationMapping.Skapa(jobId, "SKATTETABELL", "Skattetabell"),
            MigrationMapping.Skapa(jobId, "PROCENTSATS", "Procentsats"),
        ];
    }
}
