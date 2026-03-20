namespace RegionHR.Analytics.Domain;

public class PredictionResult
{
    public Guid Id { get; private set; }
    public Guid PredictionModelId { get; private set; }
    public string EntityTyp { get; private set; } = ""; // Employee/OrgUnit
    public Guid EntityId { get; private set; }
    public decimal Score { get; private set; }
    public string RiskNiva { get; private set; } = ""; // Low/Medium/High/Critical
    public string BidragandeFaktorer { get; private set; } = "{}"; // JSON
    public DateTime BeraknadVid { get; private set; }

    private PredictionResult() { }

    public static PredictionResult Skapa(
        Guid predictionModelId, string entityTyp, Guid entityId,
        decimal score, string riskNiva, string bidragandeFaktorer = "{}")
    {
        return new PredictionResult
        {
            Id = Guid.NewGuid(),
            PredictionModelId = predictionModelId,
            EntityTyp = entityTyp,
            EntityId = entityId,
            Score = score,
            RiskNiva = riskNiva,
            BidragandeFaktorer = bidragandeFaktorer,
            BeraknadVid = DateTime.UtcNow
        };
    }
}
