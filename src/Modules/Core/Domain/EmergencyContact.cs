namespace RegionHR.Core.Domain;

public class EmergencyContact
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public string Namn { get; private set; } = "";
    public string Relation { get; private set; } = "";
    public string Telefon { get; private set; } = "";
    public string? Epost { get; private set; }
    public bool ArPrimar { get; private set; }

    private EmergencyContact() { }

    public static EmergencyContact Skapa(Guid anstallId, string namn, string relation, string telefon, string? epost = null, bool primar = false)
    {
        return new EmergencyContact
        {
            Id = Guid.NewGuid(), AnstallId = anstallId, Namn = namn,
            Relation = relation, Telefon = telefon, Epost = epost, ArPrimar = primar
        };
    }

    public void Uppdatera(string namn, string relation, string telefon, string? epost)
    {
        Namn = namn; Relation = relation; Telefon = telefon; Epost = epost;
    }
}
