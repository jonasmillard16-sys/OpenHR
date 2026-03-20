namespace RegionHR.Benefits.Domain;

public class EligibilityRule
{
    public Guid Id { get; private set; }
    public Guid BenefitId { get; private set; }
    public string Namn { get; private set; } = "";
    public string Kombination { get; private set; } = "AND"; // AND / OR
    public List<EligibilityCondition> Villkor { get; private set; } = [];
    public DateTime SkapadVid { get; private set; }

    private EligibilityRule() { }

    public static EligibilityRule Skapa(Guid benefitId, string namn, string kombination = "AND")
    {
        if (kombination is not ("AND" or "OR"))
            throw new ArgumentException("Kombination måste vara AND eller OR", nameof(kombination));

        return new EligibilityRule
        {
            Id = Guid.NewGuid(),
            BenefitId = benefitId,
            Namn = namn,
            Kombination = kombination,
            SkapadVid = DateTime.UtcNow
        };
    }

    public EligibilityCondition LaggTillVillkor(string falt, string operatorTyp, string varde)
    {
        var villkor = EligibilityCondition.Skapa(Id, falt, operatorTyp, varde);
        Villkor.Add(villkor);
        return villkor;
    }

    public bool Utvardera(Dictionary<string, string> anstallningsData)
    {
        if (Villkor.Count == 0) return true;

        return Kombination == "AND"
            ? Villkor.All(v => v.Utvardera(anstallningsData))
            : Villkor.Any(v => v.Utvardera(anstallningsData));
    }
}
