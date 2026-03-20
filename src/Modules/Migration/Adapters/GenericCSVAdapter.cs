using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Adapters;

/// <summary>
/// Generisk CSV-adapter med konfigurerbar separator och headerdetektering.
/// Kan användas för godtyckliga CSV-filer.
/// </summary>
public sealed class GenericCSVAdapter : IMigrationAdapter
{
    public SourceSystem Source => SourceSystem.GenericCSV;

    public char Separator { get; set; } = ',';
    public bool HasHeader { get; set; } = true;
    public string DefaultEntityType { get; set; } = "Employee";

    public async Task<ParsedMigrationData> ParseAsync(Stream fileStream, CancellationToken ct = default)
    {
        var result = new ParsedMigrationData();
        using var reader = new StreamReader(fileStream);

        string[]? headers = null;

        if (HasHeader)
        {
            var headerLine = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                result.Warnings.Add("Tom fil — ingen header hittades");
                return result;
            }
            headers = headerLine.Split(Separator).Select(h => h.Trim()).ToArray();
        }

        var rowNumber = HasHeader ? 1 : 0;
        while (!reader.EndOfStream)
        {
            ct.ThrowIfCancellationRequested();
            var line = await reader.ReadLineAsync(ct);
            rowNumber++;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = line.Split(Separator);
            var record = new ParsedRecord { EntityType = DefaultEntityType };

            for (int i = 0; i < values.Length; i++)
            {
                var value = values[i].Trim();
                if (string.IsNullOrWhiteSpace(value))
                    continue;

                var fieldName = headers is not null && i < headers.Length
                    ? headers[i]
                    : $"Column{i}";

                record.Fields[fieldName] = value;
            }

            result.Records.Add(record);
        }

        result.TotalRows = result.Records.Count;
        return result;
    }

    public MigrationMapping[] GetDefaultMappings()
    {
        // Generic CSV has no default mappings — user configures them
        return [];
    }
}
