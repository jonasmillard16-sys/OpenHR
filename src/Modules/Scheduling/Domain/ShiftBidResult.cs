using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Resultat av budtilldelning: vem som vann, med vilken metod och motivering.
/// </summary>
public sealed class ShiftBidResult
{
    public Guid Id { get; private set; }
    public Guid OpenShiftId { get; private set; }
    public EmployeeId VinnareAnstallId { get; private set; }

    /// <summary>Tilldelningsmetod: FirstComeFirstServed, Seniority, Kompetens, Rotation.</summary>
    public string Metod { get; private set; } = string.Empty;

    /// <summary>Motivering för tilldelning.</summary>
    public string Motivering { get; private set; } = string.Empty;

    public DateTime SkapadVid { get; private set; }

    private ShiftBidResult() { }

    public static ShiftBidResult Skapa(
        Guid openShiftId,
        EmployeeId vinnare,
        string metod,
        string motivering)
    {
        return new ShiftBidResult
        {
            Id = Guid.NewGuid(),
            OpenShiftId = openShiftId,
            VinnareAnstallId = vinnare,
            Metod = metod,
            Motivering = motivering,
            SkapadVid = DateTime.UtcNow
        };
    }
}
