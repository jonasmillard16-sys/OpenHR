using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Adapters;

/// <summary>
/// Adapter för SAP SuccessFactors Employee Export — kommaseparerad CSV.
/// Förväntade kolumner: userId,firstName,lastName,personalIdExternal,hireDate,jobTitle,department,payGrade,annualSalary
/// </summary>
public sealed class SAPAdapter : IMigrationAdapter
{
    public SourceSystem Source => SourceSystem.SAP;

    private static readonly string[] ExpectedHeaders =
        ["USERID", "FIRSTNAME", "LASTNAME", "PERSONALIDEXTERNAL", "HIREDATE", "JOBTITLE", "DEPARTMENT", "PAYGRADE", "ANNUALSALARY"];

    private static readonly Dictionary<string, string> FieldMap = new()
    {
        ["USERID"] = "PersonalId",
        ["FIRSTNAME"] = "Fornamn",
        ["LASTNAME"] = "Efternamn",
        ["PERSONALIDEXTERNAL"] = "Personnummer",
        ["HIREDATE"] = "Anstallningsdatum",
        ["JOBTITLE"] = "Befattning",
        ["DEPARTMENT"] = "Enhetskod",
        ["PAYGRADE"] = "Loneklass",
        ["ANNUALSALARY"] = "Arslon"
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

        var headers = headerLine.Split(',').Select(h => h.Trim().ToUpperInvariant()).ToArray();

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

            var values = ParseCSVLine(line);
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
            MigrationMapping.Skapa(jobId, "userId", "PersonalId"),
            MigrationMapping.Skapa(jobId, "firstName", "Fornamn"),
            MigrationMapping.Skapa(jobId, "lastName", "Efternamn"),
            MigrationMapping.Skapa(jobId, "personalIdExternal", "Personnummer"),
            MigrationMapping.Skapa(jobId, "hireDate", "Anstallningsdatum"),
            MigrationMapping.Skapa(jobId, "jobTitle", "Befattning"),
            MigrationMapping.Skapa(jobId, "department", "Enhetskod"),
            MigrationMapping.Skapa(jobId, "annualSalary", "Arslon"),
        ];
    }

    private static string[] ParseCSVLine(string line)
    {
        var fields = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }
            if (ch == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Clear();
                continue;
            }
            current.Append(ch);
        }

        fields.Add(current.ToString());
        return fields.ToArray();
    }
}
