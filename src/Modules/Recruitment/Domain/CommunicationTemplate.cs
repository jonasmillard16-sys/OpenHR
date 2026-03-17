namespace RegionHR.Recruitment.Domain;

public enum TemplateType { Kallelse, Avslag, Erbjudande, Onboarding }

public class CommunicationTemplate
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public TemplateType Typ { get; private set; }
    public string Amne { get; private set; } = "";
    public string Brodtext { get; private set; } = "";
    public DateTime SkapadVid { get; private set; }

    private CommunicationTemplate() { }

    public static CommunicationTemplate Skapa(string namn, TemplateType typ, string amne, string brodtext)
    {
        return new CommunicationTemplate
        {
            Id = Guid.NewGuid(), Namn = namn, Typ = typ, Amne = amne,
            Brodtext = brodtext, SkapadVid = DateTime.UtcNow
        };
    }
}
