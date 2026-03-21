namespace RegionHR.Knowledge.Domain;

public class AssistantAction
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string Beskrivning { get; private set; } = "";
    public string? Route { get; private set; }
    public string ActionTyp { get; private set; } = "";
    public bool ArAktiv { get; private set; }

    private AssistantAction() { }

    public static AssistantAction Skapa(string namn, string beskrivning, string actionTyp,
        string? route = null, bool arAktiv = true)
    {
        if (string.IsNullOrWhiteSpace(namn))
            throw new ArgumentException("Namn krävs", nameof(namn));

        return new AssistantAction
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Beskrivning = beskrivning,
            ActionTyp = actionTyp,
            Route = route,
            ArAktiv = arAktiv
        };
    }

    public void Aktivera() => ArAktiv = true;
    public void Inaktivera() => ArAktiv = false;
}
