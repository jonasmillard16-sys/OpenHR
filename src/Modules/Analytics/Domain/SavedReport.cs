namespace RegionHR.Analytics.Domain;

public class SavedReport
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string Beskrivning { get; private set; } = "";
    public Guid SkapadAvId { get; private set; }
    public string QueryDefinition { get; private set; } = "{}"; // JSON: entity, fields, filters, groupBy, orderBy
    public string? Visualisering { get; private set; } // chart type: table, bar, line, pie
    public bool ArDelad { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public DateTime? SenastKordVid { get; private set; }

    private SavedReport() { }

    public static SavedReport Skapa(Guid skapadAv, string namn, string beskrivning, string queryDefinition, string? visualisering = null)
    {
        return new SavedReport
        {
            Id = Guid.NewGuid(), SkapadAvId = skapadAv, Namn = namn,
            Beskrivning = beskrivning, QueryDefinition = queryDefinition,
            Visualisering = visualisering, SkapadVid = DateTime.UtcNow
        };
    }

    public void Uppdatera(string queryDefinition, string? visualisering = null)
    {
        QueryDefinition = queryDefinition;
        if (visualisering != null) Visualisering = visualisering;
    }

    public void MarkeraSomKord() { SenastKordVid = DateTime.UtcNow; }
    public void ToggleDelad() { ArDelad = !ArDelad; }
}
