namespace RegionHR.Configuration.Domain;

/// <summary>
/// WorkflowNode — named per spec Section 15.8 to avoid collision with
/// CaseManagement.WorkflowStep (which is a simple POCO and must NOT be modified).
/// Represents a configurable step/node in a workflow definition.
/// </summary>
public class WorkflowNode
{
    public Guid Id { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public int Ordning { get; private set; }
    public string Typ { get; private set; } = ""; // Approval, Notification, FieldUpdate, ExternalCall, Condition, Wait
    public string Konfiguration { get; private set; } = "{}"; // JSON
    public string Namn { get; private set; } = "";

    private WorkflowNode() { }

    public static WorkflowNode Skapa(
        Guid workflowDefinitionId, int ordning, string typ, string namn, string? konfiguration = null)
    {
        return new WorkflowNode
        {
            Id = Guid.NewGuid(),
            WorkflowDefinitionId = workflowDefinitionId,
            Ordning = ordning,
            Typ = typ,
            Namn = namn,
            Konfiguration = konfiguration ?? "{}"
        };
    }

    public void UppdateraKonfiguration(string konfiguration) { Konfiguration = konfiguration; }
    public void UppdateraOrdning(int ordning) { Ordning = ordning; }
}

public static class WorkflowNodeTyp
{
    public const string Approval = "Approval";
    public const string Notification = "Notification";
    public const string FieldUpdate = "FieldUpdate";
    public const string ExternalCall = "ExternalCall";
    public const string Condition = "Condition";
    public const string Wait = "Wait";

    public static readonly string[] All = [Approval, Notification, FieldUpdate, ExternalCall, Condition, Wait];
    public static bool IsValid(string typ) => All.Contains(typ);
}
