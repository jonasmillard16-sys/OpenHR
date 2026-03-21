using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.CaseManagement.Domain;

/// <summary>
/// Formellt klagomål/grievance. Hanterar hela processen från inlämning
/// genom utredning, förhandling, beslut och eventuellt överklagande.
/// </summary>
public sealed class Grievance : AggregateRoot<GrievanceId>
{
    public EmployeeId AnstallId { get; private set; }
    public GrievanceType Typ { get; private set; }
    public string Beskrivning { get; private set; } = string.Empty;
    public string? FackligRepresentant { get; private set; }
    public GrievanceStatus Status { get; private set; }
    public string? Beslut { get; private set; }
    public DateTime InlamnadVid { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private readonly List<GrievanceInvestigation> _utredningar = [];
    public IReadOnlyList<GrievanceInvestigation> Utredningar => _utredningar.AsReadOnly();

    private readonly List<GrievanceHearing> _forhandlingar = [];
    public IReadOnlyList<GrievanceHearing> Forhandlingar => _forhandlingar.AsReadOnly();

    private readonly List<GrievanceAppeal> _overklaganden = [];
    public IReadOnlyList<GrievanceAppeal> Overklaganden => _overklaganden.AsReadOnly();

    private Grievance() { }

    /// <summary>Skapa ett nytt klagomål.</summary>
    public static Grievance Skapa(
        EmployeeId anstallId,
        GrievanceType typ,
        string beskrivning,
        string? fackligRepresentant = null)
    {
        if (string.IsNullOrWhiteSpace(beskrivning))
            throw new ArgumentException("Beskrivning krävs.", nameof(beskrivning));

        return new Grievance
        {
            Id = GrievanceId.New(),
            AnstallId = anstallId,
            Typ = typ,
            Beskrivning = beskrivning,
            FackligRepresentant = fackligRepresentant,
            Status = GrievanceStatus.Filed,
            InlamnadVid = DateTime.UtcNow,
            SkapadVid = DateTime.UtcNow
        };
    }

    /// <summary>Bekräfta mottagande av klagomålet.</summary>
    public void Bekrafta()
    {
        if (Status != GrievanceStatus.Filed)
            throw new InvalidOperationException($"Kan inte bekräfta i status {Status}. Måste vara Filed.");
        Status = GrievanceStatus.Acknowledged;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Starta en utredning.</summary>
    public void StartaUtredning(string utredare)
    {
        if (Status != GrievanceStatus.Acknowledged)
            throw new InvalidOperationException($"Kan inte starta utredning i status {Status}. Måste vara Acknowledged.");

        ArgumentException.ThrowIfNullOrWhiteSpace(utredare);

        _utredningar.Add(GrievanceInvestigation.Skapa(Id, utredare));
        Status = GrievanceStatus.UnderInvestigation;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Håll förhandling/hearing.</summary>
    public void HallForhandling(DateTime datum, List<string> deltagare)
    {
        if (Status != GrievanceStatus.UnderInvestigation)
            throw new InvalidOperationException($"Kan inte hålla förhandling i status {Status}. Måste vara UnderInvestigation.");

        _forhandlingar.Add(GrievanceHearing.Skapa(Id, datum, deltagare));
        Status = GrievanceStatus.Hearing;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Fatta beslut i ärendet.</summary>
    public void FattaBeslut(string beslut)
    {
        if (Status != GrievanceStatus.Hearing && Status != GrievanceStatus.UnderInvestigation)
            throw new InvalidOperationException(
                $"Kan inte fatta beslut i status {Status}. Måste vara Hearing eller UnderInvestigation.");

        ArgumentException.ThrowIfNullOrWhiteSpace(beslut);

        Beslut = beslut;
        Status = GrievanceStatus.Decision;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Överklaga beslutet.</summary>
    public void Overklaga(string grund)
    {
        if (Status != GrievanceStatus.Decision)
            throw new InvalidOperationException($"Kan inte överklaga i status {Status}. Måste vara Decision.");

        ArgumentException.ThrowIfNullOrWhiteSpace(grund);

        _overklaganden.Add(GrievanceAppeal.Skapa(Id, grund));
        Status = GrievanceStatus.Appeal;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Lös ärendet.</summary>
    public void Los()
    {
        if (Status == GrievanceStatus.Closed)
            throw new InvalidOperationException("Ärendet är redan stängt.");

        Status = GrievanceStatus.Resolved;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>Stäng ärendet.</summary>
    public void Stang()
    {
        if (Status != GrievanceStatus.Resolved)
            throw new InvalidOperationException($"Kan inte stänga i status {Status}. Måste vara Resolved.");

        Status = GrievanceStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }
}

public readonly record struct GrievanceId(Guid Value)
{
    public static GrievanceId New() => new(Guid.NewGuid());
    public static GrievanceId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public enum GrievanceType
{
    FormalltKlagomal,
    Diskriminering,
    Trakasseri,
    Arbetsmiljo,
    Avtalsbrott
}

public enum GrievanceStatus
{
    Filed,
    Acknowledged,
    UnderInvestigation,
    Hearing,
    Decision,
    Appeal,
    Resolved,
    Closed
}
