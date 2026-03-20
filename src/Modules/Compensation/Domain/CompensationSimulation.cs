namespace RegionHR.Compensation.Domain;

/// <summary>
/// Kompensationssimulering. Mojliggor kostnadsberakningar med parametrar och resultat i JSON.
/// </summary>
public sealed class CompensationSimulation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Namn { get; set; } = string.Empty;
    public string? Parametrar { get; set; }  // JSON
    public string? BeraknatResultat { get; set; }  // JSON
    public DateTime SkapadVid { get; set; } = DateTime.UtcNow;
    public string SkapadAv { get; set; } = string.Empty;

    public static CompensationSimulation Skapa(string namn, string parametrar, string skapadAv)
    {
        return new CompensationSimulation
        {
            Namn = namn,
            Parametrar = parametrar,
            SkapadAv = skapadAv,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void SattResultat(string resultatJson)
    {
        BeraknatResultat = resultatJson;
    }
}
