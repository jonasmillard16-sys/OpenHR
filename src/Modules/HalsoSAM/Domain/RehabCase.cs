using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.HalsoSAM.Domain;

/// <summary>
/// Rehabiliteringsärende (HälsoSAM).
/// Triggas automatiskt vid sjukfrånvaromönster.
/// </summary>
public sealed class RehabCase : AggregateRoot<Guid>
{
    /// <summary>GDPR: antal år efter avslut innan gallring.</summary>
    private const int GALLRINGS_AR = 2;

    public EmployeeId AnstallId { get; private set; }
    public RehabTrigger Trigger { get; private set; }
    public RehabStatus Status { get; private set; }
    public EmployeeId? ArendeagareHR { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public string? RehabPlan { get; private set; }

    // Uppföljningsdagar enligt FK-regler
    public DateTime? Uppfoljning14Dagar { get; private set; }
    public DateTime? Uppfoljning90Dagar { get; private set; }
    public DateTime? Uppfoljning180Dagar { get; private set; }
    public DateTime? Uppfoljning365Dagar { get; private set; }

    /// <summary>GDPR: automatiskt satt till (ärendet avslutat + 2 år) vid avslut.</summary>
    public DateTime? GallringsDatum { get; private set; }

    private readonly List<RehabNote> _anteckningar = [];
    public IReadOnlyList<RehabNote> Anteckningar => _anteckningar.AsReadOnly();

    private readonly List<RehabUppfoljning> _uppfoljningar = [];
    public IReadOnlyList<RehabUppfoljning> Uppfoljningar => _uppfoljningar.AsReadOnly();

    private RehabCase() { }

    public static RehabCase Skapa(EmployeeId anstallId, RehabTrigger trigger)
    {
        var now = DateTime.UtcNow;
        return new RehabCase
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Trigger = trigger,
            Status = RehabStatus.Signal,
            SkapadVid = now,
            Uppfoljning14Dagar = now.AddDays(14),
            Uppfoljning90Dagar = now.AddDays(90),
            Uppfoljning180Dagar = now.AddDays(180),
            Uppfoljning365Dagar = now.AddDays(365)
        };
    }

    public void TilldelaArendeagare(EmployeeId hrPerson)
    {
        ArendeagareHR = hrPerson;
        Status = RehabStatus.UnderUtredning;
    }

    public void SattRehabPlan(string plan)
    {
        RehabPlan = plan;
        Status = RehabStatus.AktivRehab;
    }

    public void LaggTillAnteckning(string text, EmployeeId forfattare)
    {
        _anteckningar.Add(new RehabNote
        {
            Text = text,
            ForfattareId = forfattare,
            SkapadVid = DateTime.UtcNow
        });
    }

    /// <summary>Registrera att en uppföljning har genomförts.</summary>
    public void RegistreraUppfoljning(int dagNr, string kommentar, EmployeeId utfordAv)
    {
        var uppfoljning = RehabUppfoljning.Skapa(dagNr, kommentar, utfordAv);
        _uppfoljningar.Add(uppfoljning);
    }

    public void Avsluta(string slutsats)
    {
        Status = RehabStatus.Avslutad;
        GallringsDatum = DateTime.UtcNow.AddYears(GALLRINGS_AR);
        LaggTillAnteckning($"Ärende avslutat: {slutsats}", ArendeagareHR ?? AnstallId);
    }
}

public enum RehabTrigger
{
    SexTillfallenTolvManader,       // 6+ sjuktillfällen på 12 månader
    FjortonSammanhangandeDagar,     // 14+ sammanhängande sjukdagar
    MonsterDetekterat,              // Mönsterdetektering (t.ex. alltid fredagar)
    ChefInitierat,                  // Initierat av chef
    MedarbetareInitierat            // Initierat av medarbetaren själv
}

public enum RehabStatus
{
    Signal,
    UnderUtredning,
    AktivRehab,
    Uppfoljning,
    Avslutad
}

public sealed class RehabNote
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public EmployeeId ForfattareId { get; set; }
    public DateTime SkapadVid { get; set; }
}
