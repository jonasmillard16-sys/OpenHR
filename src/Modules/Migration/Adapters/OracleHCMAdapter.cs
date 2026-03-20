using System.Xml.Linq;
using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Adapters;

/// <summary>
/// Adapter för Oracle HCM Extract — stödjer både CSV- och XML-format.
/// CSV-kolumner: PersonNumber,FirstName,LastName,NationalIdentifier,HireDate,JobName,DepartmentName,Salary
/// XML-format: &lt;HCMExtract&gt;&lt;Worker&gt; element med motsvarande fält.
/// </summary>
public sealed class OracleHCMAdapter : IMigrationAdapter
{
    public SourceSystem Source => SourceSystem.OracleHCM;

    private static readonly string[] ExpectedCSVHeaders =
        ["PERSONNUMBER", "FIRSTNAME", "LASTNAME", "NATIONALIDENTIFIER", "HIREDATE", "JOBNAME", "DEPARTMENTNAME", "SALARY"];

    private static readonly Dictionary<string, string> CSVFieldMap = new()
    {
        ["PERSONNUMBER"] = "PersonalId",
        ["FIRSTNAME"] = "Fornamn",
        ["LASTNAME"] = "Efternamn",
        ["NATIONALIDENTIFIER"] = "Personnummer",
        ["HIREDATE"] = "Anstallningsdatum",
        ["JOBNAME"] = "Befattning",
        ["DEPARTMENTNAME"] = "Enhetskod",
        ["SALARY"] = "Manadslon"
    };

    public async Task<ParsedMigrationData> ParseAsync(Stream fileStream, CancellationToken ct = default)
    {
        // Peek at the first bytes to determine if XML or CSV
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, ct);
        memoryStream.Position = 0;

        using var peekReader = new StreamReader(memoryStream, leaveOpen: true);
        var firstLine = await peekReader.ReadLineAsync(ct);
        memoryStream.Position = 0;

        if (firstLine is not null && firstLine.TrimStart().StartsWith('<'))
        {
            return ParseXml(memoryStream, ct);
        }

        return await ParseCSV(memoryStream, ct);
    }

    private ParsedMigrationData ParseXml(Stream stream, CancellationToken ct)
    {
        var result = new ParsedMigrationData();
        var doc = XDocument.Load(stream);
        var root = doc.Root;

        if (root is null)
        {
            result.Warnings.Add("Tomt XML-dokument");
            return result;
        }

        var workers = root.Descendants("Worker")
            .Concat(root.Descendants("worker"))
            .Concat(root.Descendants("WORKER"));

        foreach (var worker in workers)
        {
            ct.ThrowIfCancellationRequested();
            var record = new ParsedRecord { EntityType = "Employee" };

            AddXmlField(record, worker, ["PersonNumber", "personNumber", "PERSON_NUMBER"], "PersonalId");
            AddXmlField(record, worker, ["FirstName", "firstName", "FIRST_NAME"], "Fornamn");
            AddXmlField(record, worker, ["LastName", "lastName", "LAST_NAME"], "Efternamn");
            AddXmlField(record, worker, ["NationalIdentifier", "nationalIdentifier", "NATIONAL_IDENTIFIER"], "Personnummer");
            AddXmlField(record, worker, ["HireDate", "hireDate", "HIRE_DATE"], "Anstallningsdatum");
            AddXmlField(record, worker, ["JobName", "jobName", "JOB_NAME"], "Befattning");
            AddXmlField(record, worker, ["DepartmentName", "departmentName", "DEPARTMENT_NAME"], "Enhetskod");
            AddXmlField(record, worker, ["Salary", "salary", "SALARY"], "Manadslon");

            result.Records.Add(record);
        }

        result.TotalRows = result.Records.Count;
        return result;
    }

    private async Task<ParsedMigrationData> ParseCSV(Stream stream, CancellationToken ct)
    {
        var result = new ParsedMigrationData();
        using var reader = new StreamReader(stream);
        var headerLine = await reader.ReadLineAsync(ct);

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            result.Warnings.Add("Tom fil — ingen header hittades");
            return result;
        }

        var headers = headerLine.Split(',').Select(h => h.Trim().ToUpperInvariant()).ToArray();

        foreach (var expected in ExpectedCSVHeaders)
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

                if (!string.IsNullOrWhiteSpace(value) && CSVFieldMap.TryGetValue(header, out var mappedField))
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
            MigrationMapping.Skapa(jobId, "PersonNumber", "PersonalId"),
            MigrationMapping.Skapa(jobId, "FirstName", "Fornamn"),
            MigrationMapping.Skapa(jobId, "LastName", "Efternamn"),
            MigrationMapping.Skapa(jobId, "NationalIdentifier", "Personnummer"),
            MigrationMapping.Skapa(jobId, "HireDate", "Anstallningsdatum"),
            MigrationMapping.Skapa(jobId, "JobName", "Befattning"),
            MigrationMapping.Skapa(jobId, "DepartmentName", "Enhetskod"),
            MigrationMapping.Skapa(jobId, "Salary", "Manadslon"),
        ];
    }

    private static void AddXmlField(ParsedRecord record, XElement element, string[] possibleNames, string fieldName)
    {
        foreach (var name in possibleNames)
        {
            var child = element.Element(name);
            if (child is not null && !string.IsNullOrWhiteSpace(child.Value))
            {
                record.Fields[fieldName] = child.Value.Trim();
                return;
            }

            var attr = element.Attribute(name);
            if (attr is not null && !string.IsNullOrWhiteSpace(attr.Value))
            {
                record.Fields[fieldName] = attr.Value.Trim();
                return;
            }
        }
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
