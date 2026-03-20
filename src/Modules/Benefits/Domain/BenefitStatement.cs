namespace RegionHR.Benefits.Domain;

public class BenefitStatement
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public int Ar { get; private set; }
    public string? AktivaFormaner { get; private set; } // JSON
    public decimal TotaltVarde { get; private set; }
    public DateTime GenereradVid { get; private set; }

    private BenefitStatement() { }

    public static BenefitStatement Generera(Guid anstallId, int ar, string? aktivaFormaner, decimal totaltVarde)
    {
        return new BenefitStatement
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Ar = ar,
            AktivaFormaner = aktivaFormaner,
            TotaltVarde = totaltVarde,
            GenereradVid = DateTime.UtcNow
        };
    }
}
