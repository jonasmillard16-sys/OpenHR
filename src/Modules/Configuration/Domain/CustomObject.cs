namespace RegionHR.Configuration.Domain;

public class CustomObject
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string PluralNamn { get; private set; } = "";
    public string Beskrivning { get; private set; } = "";
    public string FaltSchema { get; private set; } = "[]"; // JSON array of field definitions
    public string Relationer { get; private set; } = "[]"; // JSON links to core entities
    public string Ikon { get; private set; } = "Extension";
    public DateTime SkapadVid { get; private set; }

    private CustomObject() { }

    public static CustomObject Skapa(
        string namn, string pluralNamn, string beskrivning,
        string faltSchema, string? relationer = null, string? ikon = null)
    {
        return new CustomObject
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            PluralNamn = pluralNamn,
            Beskrivning = beskrivning,
            FaltSchema = faltSchema,
            Relationer = relationer ?? "[]",
            Ikon = ikon ?? "Extension",
            SkapadVid = DateTime.UtcNow
        };
    }

    public void UppdateraSchema(string faltSchema) { FaltSchema = faltSchema; }
    public void UppdateraBeskrivning(string beskrivning) { Beskrivning = beskrivning; }
}

/// <summary>
/// Field types supported in CustomObject field schema.
/// </summary>
public static class CustomObjectFieldType
{
    public const string Text = "Text";
    public const string Number = "Number";
    public const string Date = "Date";
    public const string Dropdown = "Dropdown";
    public const string MultiSelect = "MultiSelect";
    public const string YesNo = "YesNo";
    public const string Email = "Email";
    public const string Phone = "Phone";
    public const string URL = "URL";

    public static readonly string[] All = [Text, Number, Date, Dropdown, MultiSelect, YesNo, Email, Phone, URL];

    public static bool IsValid(string type) => All.Contains(type);
}
