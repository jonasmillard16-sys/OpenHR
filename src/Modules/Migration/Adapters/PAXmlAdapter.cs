using System.Xml.Linq;
using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Adapters;

/// <summary>
/// Adapter för PAXml 2.0 — svensk standard för utbyte av löne- och personaldata.
/// Mappar &lt;personal&gt; → Employee, &lt;lonetransaktioner&gt; → PayrollRecord,
/// &lt;tidtransaktioner&gt; → TimeRecord.
/// </summary>
public sealed class PAXmlAdapter : IMigrationAdapter
{
    public SourceSystem Source => SourceSystem.PAXml;

    public Task<ParsedMigrationData> ParseAsync(Stream fileStream, CancellationToken ct = default)
    {
        var result = new ParsedMigrationData();
        var doc = XDocument.Load(fileStream);
        var root = doc.Root;
        if (root is null)
        {
            result.Warnings.Add("Tomt XML-dokument");
            return Task.FromResult(result);
        }

        // Parse personal (employees)
        var personalElements = root.Descendants("personal");
        foreach (var personal in personalElements)
        {
            ct.ThrowIfCancellationRequested();
            var record = new ParsedRecord { EntityType = "Employee" };

            AddFieldIfPresent(record, personal, "persnr", "Personnummer");
            AddFieldIfPresent(record, personal, "fornamn", "Fornamn");
            AddFieldIfPresent(record, personal, "efternamn", "Efternamn");
            AddFieldIfPresent(record, personal, "epost", "Epost");
            AddFieldIfPresent(record, personal, "telefon", "Telefon");
            AddFieldIfPresent(record, personal, "gatuadress", "Gatuadress");
            AddFieldIfPresent(record, personal, "postnr", "Postnummer");
            AddFieldIfPresent(record, personal, "postort", "Ort");
            AddFieldIfPresent(record, personal, "anstform", "Anstallningsform");
            AddFieldIfPresent(record, personal, "avtal", "Kollektivavtal");
            AddFieldIfPresent(record, personal, "enhet", "Enhetskod");
            AddFieldIfPresent(record, personal, "befattning", "Befattning");
            AddFieldIfPresent(record, personal, "manlon", "Manadslon");

            result.Records.Add(record);
        }

        // Parse lonetransaktioner (payroll records)
        var loneElements = root.Descendants("lonetransaktion");
        foreach (var lone in loneElements)
        {
            ct.ThrowIfCancellationRequested();
            var record = new ParsedRecord { EntityType = "PayrollRecord" };

            AddFieldIfPresent(record, lone, "persnr", "Personnummer");
            AddFieldIfPresent(record, lone, "loneart", "Loneart");
            AddFieldIfPresent(record, lone, "belopp", "Belopp");
            AddFieldIfPresent(record, lone, "antal", "Antal");
            AddFieldIfPresent(record, lone, "period", "Period");
            AddFieldIfPresent(record, lone, "frandatum", "FranDatum");
            AddFieldIfPresent(record, lone, "tildatum", "TilDatum");

            result.Records.Add(record);
        }

        // Parse tidtransaktioner (time records)
        var tidElements = root.Descendants("tidtransaktion");
        foreach (var tid in tidElements)
        {
            ct.ThrowIfCancellationRequested();
            var record = new ParsedRecord { EntityType = "TimeRecord" };

            AddFieldIfPresent(record, tid, "persnr", "Personnummer");
            AddFieldIfPresent(record, tid, "tidkod", "Tidkod");
            AddFieldIfPresent(record, tid, "timmar", "Timmar");
            AddFieldIfPresent(record, tid, "datum", "Datum");
            AddFieldIfPresent(record, tid, "frandatum", "FranDatum");
            AddFieldIfPresent(record, tid, "tildatum", "TilDatum");

            result.Records.Add(record);
        }

        result.TotalRows = result.Records.Count;
        return Task.FromResult(result);
    }

    public MigrationMapping[] GetDefaultMappings()
    {
        var jobId = MigrationJobId.New();
        return
        [
            MigrationMapping.Skapa(jobId, "persnr", "Personnummer"),
            MigrationMapping.Skapa(jobId, "fornamn", "Fornamn"),
            MigrationMapping.Skapa(jobId, "efternamn", "Efternamn"),
            MigrationMapping.Skapa(jobId, "epost", "Epost"),
            MigrationMapping.Skapa(jobId, "telefon", "Telefon"),
            MigrationMapping.Skapa(jobId, "anstform", "Anstallningsform"),
            MigrationMapping.Skapa(jobId, "manlon", "Manadslon"),
            MigrationMapping.Skapa(jobId, "loneart", "Loneart"),
            MigrationMapping.Skapa(jobId, "belopp", "Belopp"),
        ];
    }

    private static void AddFieldIfPresent(ParsedRecord record, XElement element, string xmlName, string fieldName)
    {
        var child = element.Element(xmlName);
        if (child is not null && !string.IsNullOrWhiteSpace(child.Value))
        {
            record.Fields[fieldName] = child.Value.Trim();
        }
        else
        {
            // Also check attribute
            var attr = element.Attribute(xmlName);
            if (attr is not null && !string.IsNullOrWhiteSpace(attr.Value))
            {
                record.Fields[fieldName] = attr.Value.Trim();
            }
        }
    }
}
