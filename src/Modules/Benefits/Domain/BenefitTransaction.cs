namespace RegionHR.Benefits.Domain;

public class BenefitTransaction
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public Guid BenefitId { get; private set; }
    public string Typ { get; private set; } = ""; // Uttag / Claim
    public decimal Belopp { get; private set; }
    public DateOnly Datum { get; private set; }
    public string Beskrivning { get; private set; } = "";
    public DateTime SkapadVid { get; private set; }

    private BenefitTransaction() { }

    public static BenefitTransaction Skapa(Guid anstallId, Guid benefitId, string typ, decimal belopp, DateOnly datum, string beskrivning)
    {
        if (typ is not ("Uttag" or "Claim"))
            throw new ArgumentException("Typ måste vara Uttag eller Claim", nameof(typ));
        if (belopp <= 0)
            throw new ArgumentException("Belopp måste vara positivt", nameof(belopp));

        return new BenefitTransaction
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            BenefitId = benefitId,
            Typ = typ,
            Belopp = belopp,
            Datum = datum,
            Beskrivning = beskrivning,
            SkapadVid = DateTime.UtcNow
        };
    }
}
