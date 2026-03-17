namespace RegionHR.Analytics.Domain;

public class Dashboard
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public Guid AgarId { get; private set; }
    public string Layout { get; private set; } = "[]"; // JSON array of widget positions
    public bool ArDelad { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private Dashboard() { }

    public static Dashboard Skapa(Guid agarId, string namn, string layout = "[]")
    {
        return new Dashboard
        {
            Id = Guid.NewGuid(), AgarId = agarId, Namn = namn,
            Layout = layout, SkapadVid = DateTime.UtcNow
        };
    }

    public void UppdateraLayout(string layout) { Layout = layout; }
    public void ToggleDelad() { ArDelad = !ArDelad; }
}
