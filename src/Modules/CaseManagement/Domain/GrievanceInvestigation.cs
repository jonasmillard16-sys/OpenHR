namespace RegionHR.CaseManagement.Domain;

/// <summary>
/// Utredning kopplad till ett klagomål. Samlar bevis och vittnesuttalanden.
/// </summary>
public sealed class GrievanceInvestigation
{
    public Guid Id { get; private set; }
    public GrievanceId GrievanceId { get; private set; }
    public string Utredare { get; private set; } = string.Empty;
    public string? Resultat { get; private set; }

    /// <summary>Bevisning (JSON-array).</summary>
    public string? Bevis { get; private set; }

    /// <summary>Vittnesuttalanden (JSON-array).</summary>
    public string? VittneUttalanden { get; private set; }

    public DateTime StartadVid { get; private set; }
    public DateTime? AvslutadVid { get; private set; }

    private GrievanceInvestigation() { }

    internal static GrievanceInvestigation Skapa(GrievanceId grievanceId, string utredare)
    {
        return new GrievanceInvestigation
        {
            Id = Guid.NewGuid(),
            GrievanceId = grievanceId,
            Utredare = utredare,
            StartadVid = DateTime.UtcNow
        };
    }

    public void RegistreraResultat(string resultat, string? bevis = null, string? vittneUttalanden = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resultat);
        Resultat = resultat;
        Bevis = bevis;
        VittneUttalanden = vittneUttalanden;
        AvslutadVid = DateTime.UtcNow;
    }
}
