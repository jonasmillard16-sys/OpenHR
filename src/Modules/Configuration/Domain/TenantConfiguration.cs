namespace RegionHR.Configuration.Domain;

public class TenantConfiguration
{
    public Guid Id { get; private set; }
    public string TenantNamn { get; private set; } = "";
    public string Organisationsnummer { get; private set; } = "";
    public string Land { get; private set; } = "SE";
    public string Sprak { get; private set; } = "sv";
    public string Valuta { get; private set; } = "SEK";
    public string? LogoUrl { get; private set; }
    public string? Konfiguration { get; private set; } // JSON with tenant-specific settings
    public bool ArAktiv { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private TenantConfiguration() { }

    public static TenantConfiguration Skapa(string namn, string orgNr, string? land = "SE", string? sprak = "sv")
    {
        return new TenantConfiguration
        {
            Id = Guid.NewGuid(), TenantNamn = namn, Organisationsnummer = orgNr,
            Land = land ?? "SE", Sprak = sprak ?? "sv", Valuta = "SEK",
            ArAktiv = true, SkapadVid = DateTime.UtcNow
        };
    }

    public void UppdateraKonfiguration(string json) { Konfiguration = json; }
    public void Inaktivera() { ArAktiv = false; }
}
