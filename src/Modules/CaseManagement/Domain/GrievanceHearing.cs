namespace RegionHR.CaseManagement.Domain;

/// <summary>
/// Förhandling/hearing kopplad till ett klagomål.
/// </summary>
public sealed class GrievanceHearing
{
    public Guid Id { get; private set; }
    public GrievanceId GrievanceId { get; private set; }
    public DateTime Datum { get; private set; }

    /// <summary>Deltagare (JSON-array).</summary>
    public string? Deltagare { get; private set; }

    public string? Protokoll { get; private set; }
    public string? Beslut { get; private set; }

    private GrievanceHearing() { }

    internal static GrievanceHearing Skapa(GrievanceId grievanceId, DateTime datum, List<string> deltagare)
    {
        return new GrievanceHearing
        {
            Id = Guid.NewGuid(),
            GrievanceId = grievanceId,
            Datum = datum,
            Deltagare = System.Text.Json.JsonSerializer.Serialize(deltagare)
        };
    }

    public void RegistreraProtokoll(string protokoll, string? beslut = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protokoll);
        Protokoll = protokoll;
        Beslut = beslut;
    }
}
