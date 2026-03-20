using System.Text.Json;

namespace RegionHR.Platform.Domain;

/// <summary>
/// Persisted domain event for audit and replay.
/// </summary>
public sealed class DomainEventRecord
{
    public Guid Id { get; private set; }
    public string Typ { get; private set; } = default!;
    public string AggregatTyp { get; private set; } = default!;
    public Guid AggregatId { get; private set; }
    public string Data { get; private set; } = default!;
    public Guid KorrelationsId { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private DomainEventRecord() { }

    public static DomainEventRecord Skapa(
        string typ,
        string aggregatTyp,
        Guid aggregatId,
        string data,
        Guid? korrelationsId = null)
    {
        return new DomainEventRecord
        {
            Id = Guid.NewGuid(),
            Typ = typ,
            AggregatTyp = aggregatTyp,
            AggregatId = aggregatId,
            Data = data,
            KorrelationsId = korrelationsId ?? Guid.NewGuid(),
            SkapadVid = DateTime.UtcNow
        };
    }
}
