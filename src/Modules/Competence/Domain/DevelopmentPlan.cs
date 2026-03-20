using RegionHR.SharedKernel.Domain;

namespace RegionHR.Competence.Domain;

public enum DevelopmentPlanStatus
{
    Draft,
    Active,
    Completed
}

public enum MilestoneStatus
{
    Pending,
    InProgress,
    Completed
}

/// <summary>
/// Utvecklingsplan för en anställd med mål-roll och milstolpar.
/// </summary>
public class DevelopmentPlan
{
    public DevelopmentPlanId Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public string MalRoll { get; private set; } = default!;
    public DevelopmentPlanStatus Status { get; private set; }
    public DateOnly StartDatum { get; private set; }
    public DateOnly? MalDatum { get; private set; }

    private readonly List<DevelopmentMilestone> _milstolpar = [];
    public IReadOnlyList<DevelopmentMilestone> Milstolpar => _milstolpar.AsReadOnly();

    private DevelopmentPlan() { }

    public static DevelopmentPlan Skapa(Guid anstallId, string malRoll, DateOnly startDatum, DateOnly? malDatum = null)
    {
        return new DevelopmentPlan
        {
            Id = DevelopmentPlanId.New(),
            AnstallId = anstallId,
            MalRoll = malRoll,
            Status = DevelopmentPlanStatus.Draft,
            StartDatum = startDatum,
            MalDatum = malDatum
        };
    }

    public void Aktivera()
    {
        if (Status != DevelopmentPlanStatus.Draft)
            throw new InvalidOperationException("Kan bara aktivera utkast");
        Status = DevelopmentPlanStatus.Active;
    }

    public void Slutfor()
    {
        if (Status != DevelopmentPlanStatus.Active)
            throw new InvalidOperationException("Kan bara slutföra aktiva planer");
        Status = DevelopmentPlanStatus.Completed;
    }

    public DevelopmentMilestone LaggTillMilstolpe(string beskrivning, string typ, DateOnly? malDatum = null)
    {
        var milstolpe = DevelopmentMilestone.Skapa(Id, beskrivning, typ, malDatum);
        _milstolpar.Add(milstolpe);
        return milstolpe;
    }
}

/// <summary>
/// En milstolpe i en utvecklingsplan.
/// </summary>
public class DevelopmentMilestone
{
    public Guid Id { get; private set; }
    public DevelopmentPlanId DevelopmentPlanId { get; private set; }
    public string Beskrivning { get; private set; } = default!;

    /// <summary>Skill, Certifiering, Kurs, Erfarenhet</summary>
    public string Typ { get; private set; } = default!;

    public DateOnly? MalDatum { get; private set; }
    public MilestoneStatus Status { get; private set; }

    private DevelopmentMilestone() { }

    internal static DevelopmentMilestone Skapa(DevelopmentPlanId planId, string beskrivning, string typ, DateOnly? malDatum)
    {
        return new DevelopmentMilestone
        {
            Id = Guid.NewGuid(),
            DevelopmentPlanId = planId,
            Beskrivning = beskrivning,
            Typ = typ,
            MalDatum = malDatum,
            Status = MilestoneStatus.Pending
        };
    }

    public void MarkeraPaborjad()
    {
        if (Status != MilestoneStatus.Pending)
            throw new InvalidOperationException("Kan bara påbörja väntande milstolpar");
        Status = MilestoneStatus.InProgress;
    }

    public void MarkeraKlar()
    {
        if (Status == MilestoneStatus.Completed)
            throw new InvalidOperationException("Milstolpe redan klar");
        Status = MilestoneStatus.Completed;
    }
}
