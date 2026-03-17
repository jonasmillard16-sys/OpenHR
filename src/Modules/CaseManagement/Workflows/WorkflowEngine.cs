using RegionHR.SharedKernel.Domain;
using RegionHR.CaseManagement.Domain;

namespace RegionHR.CaseManagement.Workflows;

/// <summary>
/// Konfigurerbar godkännandeflödesmotor.
/// Flöden definieras per ärendetyp: anstalld -> chef -> HR -> lön.
/// </summary>
public sealed class WorkflowEngine
{
    private readonly Dictionary<CaseType, WorkflowDefinition> _definitions = new()
    {
        [CaseType.Franvaro] = new WorkflowDefinition
        {
            ArendeTyp = CaseType.Franvaro,
            Steg =
            [
                new WorkflowStep { Namn = "Inskickat", Ordning = 1, GodkannareRoll = "Anstalld" },
                new WorkflowStep { Namn = "Chefsgodkannande", Ordning = 2, GodkannareRoll = "Chef" },
                new WorkflowStep { Namn = "Verkstallande", Ordning = 3, GodkannareRoll = "System" }
            ]
        },
        [CaseType.Lonandring] = new WorkflowDefinition
        {
            ArendeTyp = CaseType.Lonandring,
            Steg =
            [
                new WorkflowStep { Namn = "Inskickat", Ordning = 1, GodkannareRoll = "Chef" },
                new WorkflowStep { Namn = "HR-granskning", Ordning = 2, GodkannareRoll = "HR" },
                new WorkflowStep { Namn = "Lonegodkannande", Ordning = 3, GodkannareRoll = "Loneadmin" },
                new WorkflowStep { Namn = "Verkstallande", Ordning = 4, GodkannareRoll = "System" }
            ]
        },
        [CaseType.Anstallningsandring] = new WorkflowDefinition
        {
            ArendeTyp = CaseType.Anstallningsandring,
            Steg =
            [
                new WorkflowStep { Namn = "Inskickat", Ordning = 1, GodkannareRoll = "Chef" },
                new WorkflowStep { Namn = "HR-granskning", Ordning = 2, GodkannareRoll = "HR" },
                new WorkflowStep { Namn = "Verkstallande", Ordning = 3, GodkannareRoll = "System" }
            ]
        }
    };

    public WorkflowDefinition? GetDefinition(CaseType typ) =>
        _definitions.GetValueOrDefault(typ);

    public string? GetNastaSteg(CaseType typ, string aktuellSteg)
    {
        if (!_definitions.TryGetValue(typ, out var def)) return null;
        var current = def.Steg.FirstOrDefault(s => s.Namn == aktuellSteg);
        if (current is null) return null;
        return def.Steg.FirstOrDefault(s => s.Ordning == current.Ordning + 1)?.Namn;
    }
}

public sealed class WorkflowDefinition
{
    public CaseType ArendeTyp { get; set; }
    public List<WorkflowStep> Steg { get; set; } = [];
}

public sealed class WorkflowStep
{
    public string Namn { get; set; } = string.Empty;
    public int Ordning { get; set; }
    public string GodkannareRoll { get; set; } = string.Empty;
    public bool KravSignering { get; set; }
}
