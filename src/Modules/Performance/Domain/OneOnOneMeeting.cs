namespace RegionHR.Performance.Domain;

public enum MeetingStatus { Scheduled, Completed, Cancelled }

/// <summary>
/// 1:1-möte mellan chef och medarbetare.
/// </summary>
public sealed class OneOnOneMeeting
{
    public Guid Id { get; private set; }
    public Guid ChefId { get; private set; }
    public Guid AnstallId { get; private set; }
    public DateTime Datum { get; private set; }
    public string? Agenda { get; private set; }
    public string? Anteckningar { get; private set; }
    public string AtgardsLista { get; private set; } = "[]"; // JSON array
    public MeetingStatus Status { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private OneOnOneMeeting() { } // EF Core

    public static OneOnOneMeeting Skapa(Guid chefId, Guid anstallId, DateTime datum, string? agenda = null)
    {
        if (chefId == Guid.Empty) throw new ArgumentException("ChefId krävs.", nameof(chefId));
        if (anstallId == Guid.Empty) throw new ArgumentException("AnstallId krävs.", nameof(anstallId));

        return new OneOnOneMeeting
        {
            Id = Guid.NewGuid(),
            ChefId = chefId,
            AnstallId = anstallId,
            Datum = datum,
            Agenda = agenda,
            Status = MeetingStatus.Scheduled,
            SkapadVid = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Markerar mötet som genomfört med anteckningar.
    /// </summary>
    public void Genomfor(string anteckningar)
    {
        if (Status != MeetingStatus.Scheduled)
            throw new InvalidOperationException($"Kan inte genomföra möte i status {Status}. Förväntat: Scheduled.");

        Anteckningar = anteckningar ?? throw new ArgumentNullException(nameof(anteckningar));
        Status = MeetingStatus.Completed;
    }

    /// <summary>
    /// Avbokar mötet.
    /// </summary>
    public void Avboka()
    {
        if (Status != MeetingStatus.Scheduled)
            throw new InvalidOperationException($"Kan inte avboka möte i status {Status}. Förväntat: Scheduled.");

        Status = MeetingStatus.Cancelled;
    }

    public void SattAgenda(string agenda)
    {
        Agenda = agenda ?? throw new ArgumentNullException(nameof(agenda));
    }

    public void SattAtgardsLista(string atgardsListaJson)
    {
        AtgardsLista = atgardsListaJson ?? throw new ArgumentNullException(nameof(atgardsListaJson));
    }
}
