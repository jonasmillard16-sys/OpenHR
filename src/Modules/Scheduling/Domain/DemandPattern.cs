using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Efterfrågemönster per enhet och veckodag. Används för att beräkna
/// baslinje-behov och säsongsvariationer.
/// </summary>
public sealed class DemandPattern
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public OrganizationId EnhetId { get; set; }

    /// <summary>Veckodag 0=Söndag .. 6=Lördag</summary>
    public int Veckodag { get; set; }

    /// <summary>Timme på året (0-8783) för granulär säsongsanalys. Null = gäller hela året.</summary>
    public int? TimPaAret { get; set; }

    /// <summary>Genomsnittlig belastning (antal personal som behövs).</summary>
    public decimal GenomsnittligBelastning { get; set; }

    /// <summary>Säsongsvariationsfaktor (1.0 = normal, 1.5 = 50% högre etc.).</summary>
    public decimal SasongsVariation { get; set; } = 1.0m;
}
