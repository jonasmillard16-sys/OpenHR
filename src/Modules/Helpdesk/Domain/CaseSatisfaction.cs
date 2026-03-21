namespace RegionHR.Helpdesk.Domain;

/// <summary>
/// Nöjdhetsundersökning kopplad till ett serviceärende.
/// Sparas separat för analys och SLA-compliance-rapporter.
/// </summary>
public sealed class CaseSatisfaction
{
    public Guid Id { get; set; }
    public Guid ServiceRequestId { get; set; }
    public int Poang { get; set; } // 1-5
    public string? Kommentar { get; set; }
    public DateTime SkapadVid { get; set; }

    public static CaseSatisfaction Skapa(Guid serviceRequestId, int poang, string? kommentar = null)
    {
        if (poang < 1 || poang > 5)
            throw new ArgumentOutOfRangeException(nameof(poang), "Poäng måste vara mellan 1 och 5");

        return new CaseSatisfaction
        {
            Id = Guid.NewGuid(),
            ServiceRequestId = serviceRequestId,
            Poang = poang,
            Kommentar = kommentar,
            SkapadVid = DateTime.UtcNow
        };
    }
}
