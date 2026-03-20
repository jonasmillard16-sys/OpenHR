using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Automation.Domain;

public sealed class AutomationRule : AggregateRoot<AutomationRuleId>
{
    public string Namn { get; private set; } = string.Empty;
    public AutomationCategoryId KategoriId { get; private set; }
    public string TriggerTyp { get; private set; } = string.Empty;
    public string Villkor { get; private set; } = "{}";   // JSON
    public string Atgard { get; private set; } = "{}";    // JSON
    public bool ArAktiv { get; private set; }
    public AutomationLevel MinimumNiva { get; private set; }
    public bool ArSystemRegel { get; private set; }

    private AutomationRule() { } // EF Core

    public static AutomationRule Skapa(
        string namn,
        AutomationCategoryId kategoriId,
        string triggerTyp,
        string villkor,
        string atgard,
        AutomationLevel minimumNiva,
        bool arSystemRegel = false)
    {
        if (string.IsNullOrWhiteSpace(namn))
            throw new ArgumentException("Namn krävs", nameof(namn));
        if (string.IsNullOrWhiteSpace(triggerTyp))
            throw new ArgumentException("TriggerTyp krävs", nameof(triggerTyp));

        return new AutomationRule
        {
            Id = AutomationRuleId.New(),
            Namn = namn,
            KategoriId = kategoriId,
            TriggerTyp = triggerTyp,
            Villkor = villkor,
            Atgard = atgard,
            ArAktiv = true,
            MinimumNiva = minimumNiva,
            ArSystemRegel = arSystemRegel
        };
    }

    public void Aktivera()
    {
        ArAktiv = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Inaktivera()
    {
        if (ArSystemRegel)
            throw new InvalidOperationException("Systemregler kan inte inaktiveras");

        ArAktiv = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
