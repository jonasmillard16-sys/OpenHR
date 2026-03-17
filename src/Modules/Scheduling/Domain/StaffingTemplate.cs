using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Bemanningsmall som definierar återkommande bemanningsbehov per organisationsenhet.
/// Används som underlag vid schemaläggning och för att beräkna bemanning vs. behov.
/// </summary>
public sealed class StaffingTemplate : AggregateRoot<StaffingTemplateId>
{
    public OrganizationId EnhetId { get; private set; }
    public string Namn { get; private set; } = string.Empty;
    public DateRange Giltighet { get; private set; } = null!;

    private readonly List<StaffingRequirementLine> _rader = [];
    public IReadOnlyList<StaffingRequirementLine> Rader => _rader.AsReadOnly();

    private StaffingTemplate() { }

    /// <summary>
    /// Skapa en ny bemanningsmall.
    /// </summary>
    public static StaffingTemplate Skapa(
        OrganizationId enhetId,
        string namn,
        DateOnly giltigFrom)
    {
        if (string.IsNullOrWhiteSpace(namn))
            throw new ArgumentException("Namn får inte vara tomt.", nameof(namn));

        return new StaffingTemplate
        {
            Id = StaffingTemplateId.New(),
            EnhetId = enhetId,
            Namn = namn,
            Giltighet = DateRange.Infinite(giltigFrom)
        };
    }

    /// <summary>
    /// Skapa en ny bemanningsmall med giltighetstid.
    /// </summary>
    public static StaffingTemplate Skapa(
        OrganizationId enhetId,
        string namn,
        DateOnly giltigFrom,
        DateOnly giltigTom)
    {
        if (string.IsNullOrWhiteSpace(namn))
            throw new ArgumentException("Namn får inte vara tomt.", nameof(namn));

        return new StaffingTemplate
        {
            Id = StaffingTemplateId.New(),
            EnhetId = enhetId,
            Namn = namn,
            Giltighet = new DateRange(giltigFrom, giltigTom)
        };
    }

    /// <summary>
    /// Lägg till en bemanningsrad i mallen.
    /// </summary>
    public void LaggTillRad(StaffingRequirementLine rad)
    {
        ArgumentNullException.ThrowIfNull(rad);

        if (rad.MinAntal < 0)
            throw new ArgumentException("MinAntal kan inte vara negativt.", nameof(rad));
        if (rad.OptimalAntal < rad.MinAntal)
            throw new ArgumentException("OptimalAntal kan inte vara lägre än MinAntal.", nameof(rad));

        _rader.Add(rad);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ta bort en bemanningsrad.
    /// </summary>
    public void TaBortRad(int index)
    {
        if (index < 0 || index >= _rader.Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        _rader.RemoveAt(index);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Avsluta mallens giltighet.
    /// </summary>
    public void AvslutaGiltighet(DateOnly slutDatum)
    {
        Giltighet = new DateRange(Giltighet.Start, slutDatum);
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// En rad i bemanningsmallen som beskriver behovet för en specifik veckodag och passtyp.
/// </summary>
public sealed class StaffingRequirementLine
{
    public DayOfWeek Veckodag { get; init; }
    public ShiftType PassTyp { get; init; }
    public TimeOnly Start { get; init; }
    public TimeOnly Slut { get; init; }
    public TimeSpan Rast { get; init; }
    public int MinAntal { get; init; }
    public int OptimalAntal { get; init; }
    public List<string> KravdaKompetenser { get; init; } = [];

    /// <summary>Planerade arbetstimmar exklusive rast.</summary>
    public decimal PlaneradeTimmar
    {
        get
        {
            var total = Slut.ToTimeSpan() - Start.ToTimeSpan();
            if (total < TimeSpan.Zero) total += TimeSpan.FromHours(24); // Nattpass
            return (decimal)(total - Rast).TotalHours;
        }
    }
}
