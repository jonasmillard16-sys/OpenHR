namespace RegionHR.Analytics.Domain;

/// <summary>
/// Antagande i ett planeringsscenario.
/// Typ: HeadcountChange, AttritionRate, SalaryIncrease, NewHires, FreezeHiring.
/// EnhetId: valfritt — null betyder hela organisationen.
/// </summary>
public class ScenarioAssumption
{
    public Guid Id { get; private set; }
    public Guid ScenarioId { get; private set; }
    public string Typ { get; private set; } = ""; // HeadcountChange/AttritionRate/SalaryIncrease/NewHires/FreezeHiring
    public Guid? EnhetId { get; private set; }
    public decimal Värde { get; private set; }
    public string Beskrivning { get; private set; } = "";

    private ScenarioAssumption() { }

    public static ScenarioAssumption Skapa(
        Guid scenarioId,
        string typ,
        decimal värde,
        string beskrivning,
        Guid? enhetId = null)
    {
        if (typ is not ("HeadcountChange" or "AttritionRate" or "SalaryIncrease" or "NewHires" or "FreezeHiring"))
            throw new ArgumentException("Ogiltig typ för scenarioantagande.");

        return new ScenarioAssumption
        {
            Id = Guid.NewGuid(),
            ScenarioId = scenarioId,
            Typ = typ,
            Värde = värde,
            Beskrivning = beskrivning,
            EnhetId = enhetId
        };
    }
}
