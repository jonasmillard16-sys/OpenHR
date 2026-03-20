using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Medarbetartillgänglighet: anger när en anställd vill/kan/inte kan jobba.
/// Kan vara kopplad till specifikt datum eller repeterande per veckodag.
/// </summary>
public sealed class EmployeeAvailability
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public EmployeeId AnstallId { get; set; }

    /// <summary>Veckodag 0-6. Null = specifikt datum.</summary>
    public int? Veckodag { get; set; }

    /// <summary>Specifikt datum. Null = repeterande per veckodag.</summary>
    public DateOnly? Datum { get; set; }

    public TimeOnly? TidFran { get; set; }
    public TimeOnly? TidTill { get; set; }

    /// <summary>VillJobba / KanJobba / KanInte</summary>
    public string Preferens { get; set; } = "KanJobba";

    /// <summary>Om true, gäller varje vecka tills vidare.</summary>
    public bool ArRepeterande { get; set; }
}
