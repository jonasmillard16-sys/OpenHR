namespace RegionHR.Configuration.Domain;

/// <summary>
/// Country-specific configuration for multi-country support.
/// Stores locale, currency, date format and national ID format.
/// Currently defaults to Sweden; future tenants can override per-country.
/// </summary>
public sealed class CountryConfiguration
{
    public Guid Id { get; private set; }

    /// <summary>ISO 3166-1 alpha-2 country code (e.g. "SE", "NO", "FI").</summary>
    public string LandKod { get; private set; } = "SE";

    /// <summary>ISO 4217 currency code (e.g. "SEK", "NOK", "EUR").</summary>
    public string Valuta { get; private set; } = "SEK";

    /// <summary>ISO 639-1 language code (e.g. "sv", "en", "nb").</summary>
    public string Sprak { get; private set; } = "sv";

    /// <summary>Date format string for display (e.g. "yyyy-MM-dd").</summary>
    public string DatumFormat { get; private set; } = "yyyy-MM-dd";

    /// <summary>CultureInfo name for number formatting (e.g. "sv-SE", "nb-NO").</summary>
    public string TalFormat { get; private set; } = "sv-SE";

    /// <summary>
    /// National ID format: "Swedish" (personnummer with Luhn validation),
    /// "Norwegian", "Finnish", or "None" for generic.
    /// </summary>
    public string Personnummerformat { get; private set; } = "Swedish";

    /// <summary>When this configuration was created.</summary>
    public DateTime SkapadVid { get; private set; }

    private CountryConfiguration() { }

    /// <summary>
    /// Creates a new country configuration with Swedish defaults.
    /// </summary>
    public static CountryConfiguration SkapaSverigeStandard()
    {
        return new CountryConfiguration
        {
            Id = Guid.NewGuid(),
            LandKod = "SE",
            Valuta = "SEK",
            Sprak = "sv",
            DatumFormat = "yyyy-MM-dd",
            TalFormat = "sv-SE",
            Personnummerformat = "Swedish",
            SkapadVid = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a country configuration for a specific country.
    /// </summary>
    public static CountryConfiguration Skapa(
        string landKod,
        string valuta,
        string sprak,
        string datumFormat,
        string talFormat,
        string personnummerformat = "None")
    {
        return new CountryConfiguration
        {
            Id = Guid.NewGuid(),
            LandKod = landKod,
            Valuta = valuta,
            Sprak = sprak,
            DatumFormat = datumFormat,
            TalFormat = talFormat,
            Personnummerformat = personnummerformat,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void UppdateraSprak(string sprak) => Sprak = sprak;
    public void UppdateraValuta(string valuta) => Valuta = valuta;
    public void UppdateraDatumFormat(string format) => DatumFormat = format;
}
