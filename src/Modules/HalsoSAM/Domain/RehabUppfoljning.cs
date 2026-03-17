using RegionHR.SharedKernel.Domain;

namespace RegionHR.HalsoSAM.Domain;

/// <summary>
/// Registrerad uppföljning i ett rehabiliteringsärende.
/// Dag 14, 90, 180 och 365 enligt Försäkringskassans regelverk.
/// </summary>
public sealed class RehabUppfoljning
{
    public Guid Id { get; private set; }
    public int DagNr { get; private set; } // 14, 90, 180, 365
    public DateTime UtfordVid { get; private set; }
    public string Kommentar { get; private set; } = string.Empty;
    public EmployeeId UtfordAv { get; private set; }

    private RehabUppfoljning() { }

    public static RehabUppfoljning Skapa(int dagNr, string kommentar, EmployeeId utfordAv)
    {
        if (dagNr is not (14 or 90 or 180 or 365))
            throw new ArgumentException("Uppföljningsdagnummer måste vara 14, 90, 180 eller 365.", nameof(dagNr));

        return new RehabUppfoljning
        {
            Id = Guid.NewGuid(),
            DagNr = dagNr,
            UtfordVid = DateTime.UtcNow,
            Kommentar = kommentar,
            UtfordAv = utfordAv
        };
    }
}
