namespace RegionHR.Configuration.Domain;

/// <summary>
/// Key-value store for system-wide configuration values that change over time,
/// such as statutory amounts (IBB, PBB) that are updated annually by law.
///
/// Keys follow the convention: CATEGORY_NAME_YEAR, e.g. "IBB_2025", "PBB_2026".
/// Values are stored as strings; callers must parse to the appropriate type.
/// </summary>
public sealed class SystemSetting
{
    public Guid Id { get; private set; }

    /// <summary>Unique setting key, e.g. "IBB_2025".</summary>
    public string Nyckel { get; private set; } = "";

    /// <summary>Setting value as string, e.g. "80600".</summary>
    public string Varde { get; private set; } = "";

    /// <summary>Optional human-readable description of the setting.</summary>
    public string? Beskrivning { get; private set; }

    /// <summary>Optional category for grouping, e.g. "Basbelopp".</summary>
    public string? Kategori { get; private set; }

    public DateTime SkapadVid { get; private set; }
    public DateTime? UppdateradVid { get; private set; }

    private SystemSetting() { }

    public static SystemSetting Skapa(string nyckel, string varde, string? beskrivning = null, string? kategori = null)
    {
        return new SystemSetting
        {
            Id = Guid.NewGuid(),
            Nyckel = nyckel,
            Varde = varde,
            Beskrivning = beskrivning,
            Kategori = kategori,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void UppdateraVarde(string nyttVarde)
    {
        Varde = nyttVarde;
        UppdateradVid = DateTime.UtcNow;
    }

    /// <summary>Attempts to parse the value as decimal. Returns null if parsing fails.</summary>
    public decimal? HamtaDecimal()
        => decimal.TryParse(Varde, System.Globalization.NumberStyles.Any,
               System.Globalization.CultureInfo.InvariantCulture, out var result)
           ? result : null;
}
