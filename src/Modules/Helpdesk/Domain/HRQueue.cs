namespace RegionHR.Helpdesk.Domain;

/// <summary>
/// HR-kö för routing av serviceärenden.
/// Medlemmar lagras som JSON-array av agent-GUIDs.
/// </summary>
public sealed class HRQueue
{
    public Guid Id { get; set; }
    public string Namn { get; set; } = string.Empty;
    public string Beskrivning { get; set; } = string.Empty;
    public List<Guid> Medlemmar { get; set; } = [];

    public static HRQueue Skapa(string namn, string beskrivning, List<Guid>? medlemmar = null)
    {
        return new HRQueue
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Beskrivning = beskrivning,
            Medlemmar = medlemmar ?? []
        };
    }
}
