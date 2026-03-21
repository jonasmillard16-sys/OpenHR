using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Öppet pass som publiceras för budgivning bland anställda.
/// Flöde: Published → Bidding → Assigned (eller Cancelled).
/// </summary>
public sealed class OpenShift
{
    public Guid Id { get; private set; }
    public OrganizationId EnhetId { get; private set; }
    public DateOnly Datum { get; private set; }
    public string PassTyp { get; private set; } = string.Empty; // Dag/Kvall/Natt
    public TimeOnly StartTid { get; private set; }
    public TimeOnly SlutTid { get; private set; }

    /// <summary>Kravprofil: kompetenser/certifieringar som krävs (JSON).</summary>
    public string? KravProfil { get; private set; }

    /// <summary>Ersättningstyp: Ordinarie, OB, Overtid.</summary>
    public string Ersattning { get; private set; } = "Ordinarie";

    public OpenShiftStatus Status { get; private set; }
    public EmployeeId? TilldeladAnstallId { get; private set; }
    public string? TilldelningsMetod { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private readonly List<ShiftBid> _bud = [];
    public IReadOnlyList<ShiftBid> Bud => _bud.AsReadOnly();

    private OpenShift() { }

    /// <summary>Skapa ett nytt öppet pass.</summary>
    public static OpenShift Skapa(
        OrganizationId enhetId,
        DateOnly datum,
        string passTyp,
        TimeOnly startTid,
        TimeOnly slutTid,
        string? kravProfil = null,
        string ersattning = "Ordinarie")
    {
        if (string.IsNullOrWhiteSpace(passTyp))
            throw new ArgumentException("Passtyp krävs.", nameof(passTyp));

        return new OpenShift
        {
            Id = Guid.NewGuid(),
            EnhetId = enhetId,
            Datum = datum,
            PassTyp = passTyp,
            StartTid = startTid,
            SlutTid = slutTid,
            KravProfil = kravProfil,
            Ersattning = ersattning,
            Status = OpenShiftStatus.Published,
            SkapadVid = DateTime.UtcNow
        };
    }

    /// <summary>Publicera passet för budgivning.</summary>
    public void Publicera()
    {
        if (Status != OpenShiftStatus.Published)
            throw new InvalidOperationException($"Kan inte publicera i status {Status}.");
        // Already published — transition to Bidding happens when first bid arrives
    }

    /// <summary>Tilldela passet till en anställd.</summary>
    public void Tilldela(EmployeeId anstallId, string metod)
    {
        if (Status != OpenShiftStatus.Published && Status != OpenShiftStatus.Bidding)
            throw new InvalidOperationException(
                $"Kan inte tilldela i status {Status}. Måste vara Published eller Bidding.");

        if (string.IsNullOrWhiteSpace(metod))
            throw new ArgumentException("Tilldelningsmetod krävs.", nameof(metod));

        TilldeladAnstallId = anstallId;
        TilldelningsMetod = metod;
        Status = OpenShiftStatus.Assigned;
    }

    /// <summary>Avbryt det öppna passet.</summary>
    public void Avbryt()
    {
        if (Status == OpenShiftStatus.Assigned)
            throw new InvalidOperationException("Kan inte avbryta ett redan tilldelat pass.");

        Status = OpenShiftStatus.Cancelled;
    }

    /// <summary>Markera att budgivning pågår.</summary>
    public void MarkeraBudgivning()
    {
        if (Status == OpenShiftStatus.Published)
            Status = OpenShiftStatus.Bidding;
    }

    /// <summary>Lägg till ett bud.</summary>
    public void LaggTillBud(ShiftBid bud)
    {
        _bud.Add(bud);
        if (Status == OpenShiftStatus.Published)
            Status = OpenShiftStatus.Bidding;
    }
}

public enum OpenShiftStatus
{
    Published,
    Bidding,
    Assigned,
    Cancelled
}
