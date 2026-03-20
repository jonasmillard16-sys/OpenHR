namespace RegionHR.Communication.Domain;

public sealed class Recognition
{
    public Guid Id { get; private set; }
    public Guid FranAnstallId { get; private set; }
    public Guid TillAnstallId { get; private set; }
    public string Kategori { get; private set; } = default!;
    public string Meddelande { get; private set; } = default!;
    public DateTime SkapadVid { get; private set; }

    private Recognition() { }

    public static Recognition Skapa(Guid fran, Guid till, string kategori, string meddelande)
    {
        if (fran == Guid.Empty) throw new ArgumentException("FranAnstallId krävs.", nameof(fran));
        if (till == Guid.Empty) throw new ArgumentException("TillAnstallId krävs.", nameof(till));
        ArgumentException.ThrowIfNullOrWhiteSpace(meddelande);
        return new Recognition { Id = Guid.NewGuid(), FranAnstallId = fran, TillAnstallId = till, Kategori = kategori, Meddelande = meddelande, SkapadVid = DateTime.UtcNow };
    }
}
