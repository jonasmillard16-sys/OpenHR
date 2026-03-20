using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Efterfrågeprognos per enhet och datum. Beräknar förväntat personalbehov
/// baserat på historisk data, mönster och händelser.
/// </summary>
public sealed class DemandForecast : AggregateRoot<DemandForecastId>
{
    public OrganizationId EnhetId { get; private set; }
    public DateOnly Datum { get; private set; }
    public int BeraknatAntal { get; private set; }
    public decimal BeraknadeTidmmar { get; private set; }
    public decimal Konfidensgrad { get; private set; }
    public DateTime BeraknadVid { get; private set; }

    private DemandForecast() { }

    /// <summary>
    /// Skapa en ny efterfrågeprognos.
    /// </summary>
    public static DemandForecast Skapa(
        OrganizationId enhetId,
        DateOnly datum,
        int beraknatAntal,
        decimal beraknadeTidmmar,
        decimal konfidensgrad)
    {
        if (beraknatAntal < 0)
            throw new ArgumentException("Beräknat antal kan inte vara negativt.", nameof(beraknatAntal));
        if (konfidensgrad < 0 || konfidensgrad > 100)
            throw new ArgumentException("Konfidensgrad måste vara 0-100.", nameof(konfidensgrad));

        return new DemandForecast
        {
            Id = DemandForecastId.New(),
            EnhetId = enhetId,
            Datum = datum,
            BeraknatAntal = beraknatAntal,
            BeraknadeTidmmar = beraknadeTidmmar,
            Konfidensgrad = konfidensgrad,
            BeraknadVid = DateTime.UtcNow
        };
    }
}
