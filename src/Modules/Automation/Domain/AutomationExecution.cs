using RegionHR.SharedKernel.Domain;

namespace RegionHR.Automation.Domain;

public sealed class AutomationExecution
{
    public Guid Id { get; private set; }
    public AutomationRuleId RegelId { get; private set; }
    public string HandelseTyp { get; private set; } = string.Empty;
    public string Resultat { get; private set; } = string.Empty;
    public AutomationLevel AnvandNiva { get; private set; }
    public string UtfordAtgard { get; private set; } = string.Empty;
    public DateTime Tidsstampel { get; private set; }
    public Guid? AuditEntryId { get; private set; }

    private AutomationExecution() { } // EF Core

    public static AutomationExecution Skapa(
        AutomationRuleId regelId,
        string handelseTyp,
        string resultat,
        AutomationLevel anvandNiva,
        string utfordAtgard,
        Guid? auditEntryId = null)
    {
        return new AutomationExecution
        {
            Id = Guid.NewGuid(),
            RegelId = regelId,
            HandelseTyp = handelseTyp,
            Resultat = resultat,
            AnvandNiva = anvandNiva,
            UtfordAtgard = utfordAtgard,
            Tidsstampel = DateTime.UtcNow,
            AuditEntryId = auditEntryId
        };
    }
}
