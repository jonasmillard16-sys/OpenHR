namespace RegionHR.Knowledge.Domain;

public class ConversationSession
{
    public Guid Id { get; private set; }
    public Guid? AnstallId { get; private set; }
    public DateTime StartadVid { get; private set; }
    public DateTime SenastAktivVid { get; private set; }
    public bool ArAktiv { get; private set; }

    private ConversationSession() { }

    public static ConversationSession Starta(Guid? anstallId = null)
    {
        var now = DateTime.UtcNow;
        return new ConversationSession
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            StartadVid = now,
            SenastAktivVid = now,
            ArAktiv = true
        };
    }

    public void UppdateraAktivitet()
    {
        SenastAktivVid = DateTime.UtcNow;
    }

    public void Avsluta()
    {
        if (!ArAktiv)
            throw new InvalidOperationException("Sessionen är redan avslutad");
        ArAktiv = false;
        SenastAktivVid = DateTime.UtcNow;
    }
}
