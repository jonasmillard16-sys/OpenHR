namespace RegionHR.Performance.Domain;

/// <summary>
/// Coachingnotis till chef — proaktiva påminnelser och tips.
/// </summary>
public sealed class CoachingNudge
{
    public Guid Id { get; private set; }
    public Guid ChefId { get; private set; }
    public string Typ { get; private set; } = default!; // MissedOneOnOne, HighTurnover, LowEngagement, DevelopmentStalled, NewTeamMember
    public string Meddelande { get; private set; } = default!;
    public bool ArLast { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private CoachingNudge() { } // EF Core

    public static CoachingNudge Skapa(Guid chefId, string typ, string meddelande)
    {
        if (chefId == Guid.Empty) throw new ArgumentException("ChefId krävs.", nameof(chefId));
        ArgumentException.ThrowIfNullOrWhiteSpace(typ);
        ArgumentException.ThrowIfNullOrWhiteSpace(meddelande);

        return new CoachingNudge
        {
            Id = Guid.NewGuid(),
            ChefId = chefId,
            Typ = typ,
            Meddelande = meddelande,
            ArLast = false,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void MarkeraSomLast()
    {
        ArLast = true;
    }
}
