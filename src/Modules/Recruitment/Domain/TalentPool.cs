namespace RegionHR.Recruitment.Domain;

public class TalentPoolEntry
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string Epost { get; private set; } = "";
    public string? Telefon { get; private set; }
    public string? KompetensOmrade { get; private set; }
    public string? Anteckningar { get; private set; }
    public Guid? UrsprungsAnsokanId { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private TalentPoolEntry() { }

    public static TalentPoolEntry Skapa(string namn, string epost, string? kompetensOmrade = null, string? anteckningar = null, Guid? ursprungsAnsokanId = null)
    {
        return new TalentPoolEntry
        {
            Id = Guid.NewGuid(), Namn = namn, Epost = epost,
            KompetensOmrade = kompetensOmrade, Anteckningar = anteckningar,
            UrsprungsAnsokanId = ursprungsAnsokanId, SkapadVid = DateTime.UtcNow
        };
    }
}
