using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Pass-byte: hantering av byte/överlåtelse av pass mellan anställda.
/// Flöde: Begärd -> Erbjuden -> Accepterad -> Godkänd (av chef)
///
/// En begäran kan vara riktad (erbjuds till specifik person) eller öppen
/// (alla med rätt kompetens kan acceptera). Chef måste alltid godkänna.
/// </summary>
public sealed class ShiftSwapRequest : AggregateRoot<ShiftSwapId>
{
    public EmployeeId BegardAv { get; private set; }
    public EmployeeId? ErbjodsAv { get; private set; }
    public Guid UrsprungligtPassId { get; private set; }
    public Guid? ErsattningsPassId { get; private set; }
    public ShiftSwapStatus Status { get; private set; }
    public string? Motivering { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public string? GodkannareId { get; private set; }
    public string? AvvisningsAnledning { get; private set; }
    public DateTime? HandlagdVid { get; private set; }

    private ShiftSwapRequest() { }

    /// <summary>
    /// Skapa en ny begäran om pass-byte.
    /// </summary>
    public static ShiftSwapRequest Skapa(
        EmployeeId begardAv,
        Guid passId,
        string motivering)
    {
        if (string.IsNullOrWhiteSpace(motivering))
            throw new ArgumentException("Motivering krävs för pass-byte.", nameof(motivering));

        return new ShiftSwapRequest
        {
            Id = ShiftSwapId.New(),
            BegardAv = begardAv,
            UrsprungligtPassId = passId,
            Status = ShiftSwapStatus.Begard,
            Motivering = motivering,
            SkapadVid = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Erbjud bytet till en specifik anställd.
    /// </summary>
    public void Erbjud(EmployeeId till)
    {
        if (Status != ShiftSwapStatus.Begard)
            throw new InvalidOperationException(
                $"Kan inte erbjuda pass-byte i status {Status}. Måste vara {ShiftSwapStatus.Begard}.");

        if (till == BegardAv)
            throw new InvalidOperationException("Kan inte erbjuda pass-byte till sig själv.");

        ErbjodsAv = till;
        Status = ShiftSwapStatus.Erbjuden;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mottagande anställd accepterar bytet. Kan ange ersättningspass.
    /// </summary>
    public void Acceptera(EmployeeId av, Guid? ersattningsPassId = null)
    {
        if (Status != ShiftSwapStatus.Erbjuden && Status != ShiftSwapStatus.Begard)
            throw new InvalidOperationException(
                $"Kan inte acceptera pass-byte i status {Status}. Måste vara {ShiftSwapStatus.Erbjuden} eller {ShiftSwapStatus.Begard}.");

        if (av == BegardAv)
            throw new InvalidOperationException("Kan inte acceptera eget pass-byte.");

        if (ErbjodsAv.HasValue && av != ErbjodsAv.Value)
            throw new InvalidOperationException("Bara den erbjudna anställda kan acceptera.");

        ErbjodsAv = av;
        ErsattningsPassId = ersattningsPassId;
        Status = ShiftSwapStatus.Accepterad;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Chef godkänner pass-bytet. Bytet verkställs.
    /// </summary>
    public void Godkann(string godkannare)
    {
        if (Status != ShiftSwapStatus.Accepterad)
            throw new InvalidOperationException(
                $"Kan inte godkänna pass-byte i status {Status}. Måste vara {ShiftSwapStatus.Accepterad}.");

        if (string.IsNullOrWhiteSpace(godkannare))
            throw new ArgumentException("Godkännare krävs.", nameof(godkannare));

        GodkannareId = godkannare;
        Status = ShiftSwapStatus.Godkand;
        HandlagdVid = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Chef avvisar pass-bytet med anledning.
    /// </summary>
    public void Avvisa(string godkannare, string anledning)
    {
        if (Status != ShiftSwapStatus.Accepterad && Status != ShiftSwapStatus.Erbjuden
            && Status != ShiftSwapStatus.Begard)
            throw new InvalidOperationException(
                $"Kan inte avvisa pass-byte i status {Status}.");

        if (string.IsNullOrWhiteSpace(godkannare))
            throw new ArgumentException("Godkännare krävs.", nameof(godkannare));
        if (string.IsNullOrWhiteSpace(anledning))
            throw new ArgumentException("Anledning till avvisning krävs.", nameof(anledning));

        GodkannareId = godkannare;
        AvvisningsAnledning = anledning;
        Status = ShiftSwapStatus.Avvisad;
        HandlagdVid = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Makulera begäran (kan bara göras av den som begärt).
    /// </summary>
    public void Makulera(EmployeeId av)
    {
        if (av != BegardAv)
            throw new InvalidOperationException("Bara den som begärt bytet kan makulera.");

        if (Status == ShiftSwapStatus.Godkand)
            throw new InvalidOperationException("Kan inte makulera ett redan godkänt pass-byte.");

        Status = ShiftSwapStatus.Makulerad;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum ShiftSwapStatus
{
    Begard,
    Erbjuden,
    Accepterad,
    Godkand,
    Avvisad,
    Makulerad
}
