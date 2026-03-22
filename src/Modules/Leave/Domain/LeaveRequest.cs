namespace RegionHR.Leave.Domain;

public enum LeaveType
{
    Semester,
    Sjukfranvaro,
    Foraldraledighet,
    VAB,
    Tjanstledighet,
    Komptid,
    Utbildning
}

public enum LeaveRequestStatus
{
    Utkast,
    Inskickad,
    Godkand,
    Avslagen,
    Aterkallad
}

/// <summary>
/// Frnvarobegran (semester, sjuk, frldraledighet, VAB, etc.).
/// Hanterar statusverg enligt arbetsflde:
///   Utkast -> Inskickad -> Godknd/Avslagen
///   Utkast/Inskickad -> terkallad
/// </summary>
public sealed class LeaveRequest
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public LeaveType Typ { get; private set; }
    public DateOnly FranDatum { get; private set; }
    public DateOnly TillDatum { get; private set; }
    public int AntalDagar { get; private set; }
    public string? Beskrivning { get; private set; }
    public LeaveRequestStatus Status { get; private set; }
    public Guid? GodkandAv { get; private set; }
    public DateTime? GodkandVid { get; private set; }
    public string? Kommentar { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private LeaveRequest() { } // EF Core

    /// <summary>
    /// Skapar en ny frnvarobegran med status Utkast.
    /// AntalDagar berknas automatiskt som arbetsdagar (exklusive helger).
    /// </summary>
    public static LeaveRequest Skapa(Guid anstallId, LeaveType typ, DateOnly from, DateOnly to, string? beskrivning)
    {
        if (to < from)
            throw new ArgumentException("Slutdatum kan inte vara fre startdatum.");

        // TODO: Validate no overlapping approved/pending requests for same employee

        return new LeaveRequest
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Typ = typ,
            FranDatum = from,
            TillDatum = to,
            AntalDagar = RaknaArbetsdagar(from, to),
            Beskrivning = beskrivning,
            Status = LeaveRequestStatus.Utkast,
            SkapadVid = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Skickar in begran fr godknnande (Utkast -> Inskickad).
    /// </summary>
    public void SkickaIn()
    {
        if (Status != LeaveRequestStatus.Utkast)
            throw new InvalidOperationException(
                $"Kan bara skicka in frn status Utkast. Nuvarande status: {Status}");

        Status = LeaveRequestStatus.Inskickad;
    }

    /// <summary>
    /// Godknner begran (Inskickad -> Godknd).
    /// </summary>
    public void Godkann(Guid godkannare, string? kommentar)
    {
        if (Status != LeaveRequestStatus.Inskickad)
            throw new InvalidOperationException(
                $"Kan bara godknna frn status Inskickad. Nuvarande status: {Status}");

        GodkandAv = godkannare;
        GodkandVid = DateTime.UtcNow;
        Kommentar = kommentar;
        Status = LeaveRequestStatus.Godkand;
    }

    /// <summary>
    /// Avvisar begran med obligatorisk kommentar (Inskickad -> Avslagen).
    /// </summary>
    public void Avvisa(Guid godkannare, string kommentar)
    {
        if (Status != LeaveRequestStatus.Inskickad)
            throw new InvalidOperationException(
                $"Kan bara avvisa frn status Inskickad. Nuvarande status: {Status}");

        ArgumentException.ThrowIfNullOrWhiteSpace(kommentar);

        GodkandAv = godkannare;
        GodkandVid = DateTime.UtcNow;
        Kommentar = kommentar;
        Status = LeaveRequestStatus.Avslagen;
    }

    /// <summary>
    /// terkalla begran. Kan gras frn Utkast eller Inskickad.
    /// </summary>
    public void Aterkalla()
    {
        if (Status != LeaveRequestStatus.Utkast && Status != LeaveRequestStatus.Inskickad)
            throw new InvalidOperationException(
                $"Kan bara terkalla frn status Utkast eller Inskickad. Nuvarande status: {Status}");

        Status = LeaveRequestStatus.Aterkallad;
    }

    /// <summary>
    /// Rknar arbetsdagar mellan tv datum (exklusive lrdagar och sndagar).
    /// </summary>
    private static int RaknaArbetsdagar(DateOnly from, DateOnly to)
    {
        var arbetsdagar = 0;
        var current = from;

        while (current <= to)
        {
            var dayOfWeek = current.DayOfWeek;
            if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday)
                arbetsdagar++;

            current = current.AddDays(1);
        }

        return arbetsdagar;
    }
}
