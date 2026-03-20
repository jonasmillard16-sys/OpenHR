namespace RegionHR.Wellness.Domain;

public enum WellnessClaimStatus { Inskickad, Godkand, Avslagen }

/// <summary>
/// Friskvårdsbidragsansökan. Kopplad till verklig anställd via AnstallId.
/// Max 5000 kr/år per anställd (standard friskvårdsbidrag).
/// </summary>
public sealed class WellnessClaim
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public string Aktivitet { get; private set; } = default!;
    public decimal Belopp { get; private set; }
    public DateOnly Datum { get; private set; }
    public WellnessClaimStatus Status { get; private set; }
    public string? KvittoFilId { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public Guid? GodkandAv { get; private set; }
    public DateTime? GodkandVid { get; private set; }
    public string? Kommentar { get; private set; }

    private WellnessClaim() { }

    public static WellnessClaim Skapa(Guid anstallId, string aktivitet, decimal belopp, DateOnly datum, string? kvittoFilId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(aktivitet);
        if (anstallId == Guid.Empty) throw new ArgumentException("AnstallId krävs.", nameof(anstallId));
        if (belopp <= 0) throw new ArgumentOutOfRangeException(nameof(belopp));

        return new WellnessClaim
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Aktivitet = aktivitet,
            Belopp = belopp,
            Datum = datum,
            Status = WellnessClaimStatus.Inskickad,
            KvittoFilId = kvittoFilId,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void Godkann(Guid godkannare, string? kommentar = null)
    {
        if (Status != WellnessClaimStatus.Inskickad)
            throw new InvalidOperationException($"Kan bara godkänna inskickad ansökan. Nuvarande: {Status}");
        Status = WellnessClaimStatus.Godkand;
        GodkandAv = godkannare;
        GodkandVid = DateTime.UtcNow;
        Kommentar = kommentar;
    }

    public void Avvisa(Guid godkannare, string kommentar)
    {
        if (Status != WellnessClaimStatus.Inskickad)
            throw new InvalidOperationException($"Kan bara avvisa inskickad ansökan. Nuvarande: {Status}");
        ArgumentException.ThrowIfNullOrWhiteSpace(kommentar);
        Status = WellnessClaimStatus.Avslagen;
        GodkandAv = godkannare;
        GodkandVid = DateTime.UtcNow;
        Kommentar = kommentar;
    }
}
