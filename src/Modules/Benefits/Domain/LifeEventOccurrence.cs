namespace RegionHR.Benefits.Domain;

public class LifeEventOccurrence
{
    public Guid Id { get; private set; }
    public Guid LifeEventId { get; private set; }
    public Guid AnstallId { get; private set; }
    public DateOnly Datum { get; private set; }
    public string Status { get; private set; } = "Registered";
    public string? KoppladeAtgarder { get; private set; } // JSON
    public DateTime SkapadVid { get; private set; }

    private LifeEventOccurrence() { }

    public static LifeEventOccurrence Registrera(Guid lifeEventId, Guid anstallId, DateOnly datum, string? koppladeAtgarder = null)
    {
        return new LifeEventOccurrence
        {
            Id = Guid.NewGuid(),
            LifeEventId = lifeEventId,
            AnstallId = anstallId,
            Datum = datum,
            Status = "Registered",
            KoppladeAtgarder = koppladeAtgarder,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void Bearbeta()
    {
        Status = "Processed";
    }
}
