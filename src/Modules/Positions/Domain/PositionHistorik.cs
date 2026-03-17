namespace RegionHR.Positions.Domain;

public class PositionHistorik
{
    public Guid Id { get; private set; }
    public Guid PositionId { get; private set; }
    public Guid? TidigareInnehavare { get; private set; }
    public Guid? NyInnehavare { get; private set; }
    public DateTime AndradVid { get; private set; }
    public string? Anledning { get; private set; }

    private PositionHistorik() { }
    public PositionHistorik(Guid positionId, Guid? tidigare, Guid? ny, DateTime vid, string? anledning = null)
    {
        Id = Guid.NewGuid(); PositionId = positionId; TidigareInnehavare = tidigare;
        NyInnehavare = ny; AndradVid = vid; Anledning = anledning;
    }
}
