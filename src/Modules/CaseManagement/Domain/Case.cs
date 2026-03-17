using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.CaseManagement.Domain;

/// <summary>
/// Ärende i HR-systemet. Används för frånvaro, anställningsändringar,
/// och andra godkännandeflöden.
/// </summary>
public sealed class Case : AggregateRoot<CaseId>
{
    public CaseType Typ { get; private set; }
    public EmployeeId AnstallId { get; private set; }
    public CaseStatus Status { get; private set; }
    public string Beskrivning { get; private set; } = string.Empty;
    public string? AktuellSteg { get; private set; }
    public EmployeeId? TilldeladTill { get; private set; }
    public DateTime? SlutfordVid { get; private set; }

    // Typspecifik data (JSONB i databas)
    public AbsenceData? FranvaroData { get; private set; }

    private readonly List<CaseApproval> _godkannanden = [];
    public IReadOnlyList<CaseApproval> Godkannanden => _godkannanden.AsReadOnly();

    private readonly List<CaseComment> _kommentarer = [];
    public IReadOnlyList<CaseComment> Kommentarer => _kommentarer.AsReadOnly();

    private Case() { }

    public static Case SkapaFranvaroarende(
        EmployeeId anstallId, AbsenceType franvaroTyp,
        DateOnly franDatum, DateOnly tillDatum, string beskrivning)
    {
        return new Case
        {
            Id = CaseId.New(),
            Typ = CaseType.Franvaro,
            AnstallId = anstallId,
            Status = CaseStatus.Oppnad,
            Beskrivning = beskrivning,
            AktuellSteg = "Inskickat",
            FranvaroData = new AbsenceData
            {
                FranvaroTyp = franvaroTyp,
                FranDatum = franDatum,
                TillDatum = tillDatum,
                AntalDagar = tillDatum.DayNumber - franDatum.DayNumber + 1
            }
        };
    }

    public static Case SkapaAnstallningsandring(
        EmployeeId anstallId, string beskrivning)
    {
        return new Case
        {
            Id = CaseId.New(),
            Typ = CaseType.Anstallningsandring,
            AnstallId = anstallId,
            Status = CaseStatus.Oppnad,
            Beskrivning = beskrivning,
            AktuellSteg = "Inskickat"
        };
    }

    public void TilldellaTill(EmployeeId handlaggare)
    {
        TilldeladTill = handlaggare;
        Status = CaseStatus.UnderBehandling;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SkickaForGodkannande(string steg, EmployeeId godkannare)
    {
        AktuellSteg = steg;
        Status = CaseStatus.VantarGodkannande;
        _godkannanden.Add(new CaseApproval
        {
            Steg = steg,
            GodkannareId = godkannare,
            Status = ApprovalStatus.Vantar
        });
    }

    public void Godkann(EmployeeId godkannare, string? kommentar = null)
    {
        var approval = _godkannanden.LastOrDefault(g => g.GodkannareId == godkannare && g.Status == ApprovalStatus.Vantar);
        if (approval is null) throw new InvalidOperationException("Inget väntande godkännande för denna person");

        approval.Status = ApprovalStatus.Godkand;
        approval.BeslutVid = DateTime.UtcNow;
        approval.Kommentar = kommentar;

        Status = CaseStatus.Godkand;
        RaiseDomainEvent(new CaseApprovedEvent(Id, Typ, AnstallId));
    }

    public void Avsluta()
    {
        Status = CaseStatus.Avslutad;
        SlutfordVid = DateTime.UtcNow;
    }

    public void LaggTillKommentar(EmployeeId forfattare, string text)
    {
        _kommentarer.Add(new CaseComment
        {
            ForfattareId = forfattare,
            Text = text,
            SkapadVid = DateTime.UtcNow
        });
    }
}

public enum CaseType
{
    Franvaro,
    Anstallningsandring,
    Lonandring,
    Omplacering,
    Rehabilitering,
    LAS
}

public sealed class AbsenceData
{
    public AbsenceType FranvaroTyp { get; set; }
    public DateOnly FranDatum { get; set; }
    public DateOnly TillDatum { get; set; }
    public int AntalDagar { get; set; }
    public decimal? Omfattning { get; set; } = 100m; // Procent
}

public sealed class CaseApproval
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Steg { get; set; } = string.Empty;
    public EmployeeId GodkannareId { get; set; }
    public ApprovalStatus Status { get; set; }
    public DateTime? BeslutVid { get; set; }
    public string? Kommentar { get; set; }
}

public enum ApprovalStatus { Vantar, Godkand, Avslagen }

public sealed class CaseComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public EmployeeId ForfattareId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime SkapadVid { get; set; }
}

public sealed record CaseApprovedEvent(CaseId CaseId, CaseType Typ, EmployeeId AnstallId) : DomainEvent;
