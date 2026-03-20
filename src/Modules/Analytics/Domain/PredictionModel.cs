namespace RegionHR.Analytics.Domain;

public class PredictionModel
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string Typ { get; private set; } = ""; // Attrition/HeadcountForecast/SickLeaveForecast/LaborCostForecast
    public string InputParametrar { get; private set; } = "{}"; // JSON
    public DateTime? SenasteTranningsDatum { get; private set; }
    public decimal? Accuracy { get; private set; }

    private PredictionModel() { }

    public static PredictionModel Skapa(string namn, string typ, string inputParametrar = "{}")
    {
        return new PredictionModel
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Typ = typ,
            InputParametrar = inputParametrar
        };
    }

    public void UppdateraTranning(decimal accuracy)
    {
        SenasteTranningsDatum = DateTime.UtcNow;
        Accuracy = accuracy;
    }
}
