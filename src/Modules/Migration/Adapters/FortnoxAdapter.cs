using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Adapters;

/// <summary>
/// Adapter för Fortnox — semikolonseparerad CSV-export av anställda.
/// Förväntade kolumner: PersonalId;Personnummer;Fornamn;Efternamn;Anstallningsdatum;MånadslönBrutto;Avdelning;Befattning
/// </summary>
public sealed class FortnoxAdapter : IMigrationAdapter
{
    public SourceSystem Source => SourceSystem.Fortnox;

    private static readonly string[] ExpectedHeaders =
        ["PERSONALID", "PERSONNUMMER", "FORNAMN", "EFTERNAMN", "ANSTALLNINGSDATUM", "MANADSLONBRUTTO", "AVDELNING", "BEFATTNING"];

    private static readonly Dictionary<string, string> FieldMap = new()
    {
        ["PERSONALID"] = "PersonalId",
        ["PERSONNUMMER"] = "Personnummer",
        ["FORNAMN"] = "Fornamn",
        ["EFTERNAMN"] = "Efternamn",
        ["ANSTALLNINGSDATUM"] = "Anstallningsdatum",
        ["MANADSLONBRUTTO"] = "Manadslon",
        ["AVDELNING"] = "Enhetskod",
        ["BEFATTNING"] = "Befattning"
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

        // Fortnox uses semicolon separator
        var headers = headerLine.Split(';').Select(h => h.Trim().ToUpperInvariant()
            .Replace("Å", "A").Replace("Ä", "A").Replace("Ö", "O")).ToArray();

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
            MigrationMapping.Skapa(jobId, "PERSONALID", "PersonalId"),
            MigrationMapping.Skapa(jobId, "PERSONNUMMER", "Personnummer"),
            MigrationMapping.Skapa(jobId, "FORNAMN", "Fornamn"),
            MigrationMapping.Skapa(jobId, "EFTERNAMN", "Efternamn"),
            MigrationMapping.Skapa(jobId, "ANSTALLNINGSDATUM", "Anstallningsdatum"),
            MigrationMapping.Skapa(jobId, "MANADSLONBRUTTO", "Manadslon"),
            MigrationMapping.Skapa(jobId, "AVDELNING", "Enhetskod"),
            MigrationMapping.Skapa(jobId, "BEFATTNING", "Befattning"),
        ];
    }
}
