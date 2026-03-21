namespace RegionHR.Knowledge.Domain;

public class ConversationMessage
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public string Avsandare { get; private set; } = "";
    public string Innehall { get; private set; } = "";
    public DateTime Tidsstampel { get; private set; }
    public Guid? KallaArtikelId { get; private set; }
    public string? UtfordAction { get; private set; }

    private ConversationMessage() { }

    public static ConversationMessage Skapa(Guid sessionId, string avsandare, string innehall,
        Guid? kallaArtikelId = null, string? utfordAction = null)
    {
        if (string.IsNullOrWhiteSpace(innehall))
            throw new ArgumentException("Innehåll krävs", nameof(innehall));
        if (avsandare is not ("User" or "Assistant"))
            throw new ArgumentException("Avsändare måste vara 'User' eller 'Assistant'", nameof(avsandare));

        return new ConversationMessage
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Avsandare = avsandare,
            Innehall = innehall,
            Tidsstampel = DateTime.UtcNow,
            KallaArtikelId = kallaArtikelId,
            UtfordAction = utfordAction
        };
    }
}
