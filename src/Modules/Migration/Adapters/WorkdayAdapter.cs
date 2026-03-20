using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Adapters;

/// <summary>
/// Adapter för Workday EIB (Enterprise Interface Builder) CSV-export.
/// Förväntade kolumner: Employee_ID,First_Name,Last_Name,National_ID,Hire_Date,Job_Title,Department,Annual_Salary,Currency
/// </summary>
public sealed class WorkdayAdapter : IMigrationAdapter
{
    public SourceSystem Source => SourceSystem.Workday;

    private static readonly string[] ExpectedHeaders =
        ["EMPLOYEE_ID", "FIRST_NAME", "LAST_NAME", "NATIONAL_ID", "HIRE_DATE", "JOB_TITLE", "DEPARTMENT", "ANNUAL_SALARY", "CURRENCY"];

    private static readonly Dictionary<string, string> FieldMap = new()
    {
        ["EMPLOYEE_ID"] = "PersonalId",
        ["FIRST_NAME"] = "Fornamn",
        ["LAST_NAME"] = "Efternamn",
        ["NATIONAL_ID"] = "Personnummer",
        ["HIRE_DATE"] = "Anstallningsdatum",
        ["JOB_TITLE"] = "Befattning",
        ["DEPARTMENT"] = "Enhetskod",
        ["ANNUAL_SALARY"] = "Arslon",
        ["CURRENCY"] = "Valuta"
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

        // Workday EIB exports use comma separator
        var headers = headerLine.Split(',').Select(h => h.Trim().ToUpperInvariant().Replace(" ", "_")).ToArray();

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
            MigrationMapping.Skapa(jobId, "EMPLOYEE_ID", "PersonalId"),
            MigrationMapping.Skapa(jobId, "FIRST_NAME", "Fornamn"),
            MigrationMapping.Skapa(jobId, "LAST_NAME", "Efternamn"),
            MigrationMapping.Skapa(jobId, "NATIONAL_ID", "Personnummer"),
            MigrationMapping.Skapa(jobId, "HIRE_DATE", "Anstallningsdatum"),
            MigrationMapping.Skapa(jobId, "JOB_TITLE", "Befattning"),
            MigrationMapping.Skapa(jobId, "DEPARTMENT", "Enhetskod"),
            MigrationMapping.Skapa(jobId, "ANNUAL_SALARY", "Arslon"),
        ];
    }

    /// <summary>Enkel CSV-parser med stöd för citattecken</summary>
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
