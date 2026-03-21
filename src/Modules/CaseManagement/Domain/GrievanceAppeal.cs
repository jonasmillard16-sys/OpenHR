namespace RegionHR.CaseManagement.Domain;

/// <summary>
/// Överklagande av ett klagomålsbeslut.
/// </summary>
public sealed class GrievanceAppeal
{
    public Guid Id { get; private set; }
    public GrievanceId GrievanceId { get; private set; }
    public string Grund { get; private set; } = string.Empty;
    public DateTime InlamnadVid { get; private set; }
    public string? Resultat { get; private set; }
    public DateTime? AvgjordVid { get; private set; }

    private GrievanceAppeal() { }

    internal static GrievanceAppeal Skapa(GrievanceId grievanceId, string grund)
    {
        return new GrievanceAppeal
        {
            Id = Guid.NewGuid(),
            GrievanceId = grievanceId,
            Grund = grund,
            InlamnadVid = DateTime.UtcNow
        };
    }

    public void Avgora(string resultat)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resultat);
        Resultat = resultat;
        AvgjordVid = DateTime.UtcNow;
    }
}
