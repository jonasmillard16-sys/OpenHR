namespace RegionHR.Competence.Domain;

/// <summary>
/// Mentorrelation mellan mentor och adept.
/// </summary>
public class MentorRelation
{
    public Guid Id { get; private set; }
    public Guid MentorId { get; private set; }
    public Guid AdeptId { get; private set; }
    public string FokusOmrade { get; private set; } = default!;
    public DateOnly StartDatum { get; private set; }

    /// <summary>Active, Completed, Cancelled</summary>
    public string Status { get; private set; } = default!;

    public int MotesFrekvensDagar { get; private set; }

    private MentorRelation() { }

    public static MentorRelation Skapa(Guid mentorId, Guid adeptId, string fokusOmrade,
        DateOnly startDatum, int motesFrekvensDagar = 14)
    {
        return new MentorRelation
        {
            Id = Guid.NewGuid(),
            MentorId = mentorId,
            AdeptId = adeptId,
            FokusOmrade = fokusOmrade,
            StartDatum = startDatum,
            Status = "Active",
            MotesFrekvensDagar = motesFrekvensDagar
        };
    }
}
