namespace RegionHR.Configuration.Domain;

/// <summary>
/// Tracks the execution state of a workflow instance.
/// </summary>
public class WorkflowRunInstance
{
    public Guid Id { get; private set; }
    public Guid WorkflowDefinitionId { get; private set; }
    public int AktuellNodeOrdning { get; private set; }
    public string Status { get; private set; } = ""; // Running, Completed, Failed, Cancelled
    public string EntityTyp { get; private set; } = "";
    public Guid EntityId { get; private set; }
    public string Data { get; private set; } = "{}"; // JSON
    public DateTime StartadVid { get; private set; }
    public DateTime? AvslutadVid { get; private set; }

    private WorkflowRunInstance() { }

    public static WorkflowRunInstance Starta(
        Guid workflowDefinitionId, string entityTyp, Guid entityId, string? data = null)
    {
        return new WorkflowRunInstance
        {
            Id = Guid.NewGuid(),
            WorkflowDefinitionId = workflowDefinitionId,
            AktuellNodeOrdning = 1,
            Status = WorkflowRunStatus.Running,
            EntityTyp = entityTyp,
            EntityId = entityId,
            Data = data ?? "{}",
            StartadVid = DateTime.UtcNow
        };
    }

    public void AvanceraSteg()
    {
        if (Status != WorkflowRunStatus.Running)
            throw new InvalidOperationException($"Kan inte avancera steg i status: {Status}");
        AktuellNodeOrdning++;
    }

    public void Avsluta()
    {
        Status = WorkflowRunStatus.Completed;
        AvslutadVid = DateTime.UtcNow;
    }

    public void Avbryt()
    {
        Status = WorkflowRunStatus.Cancelled;
        AvslutadVid = DateTime.UtcNow;
    }

    public void MarkeraSomMisslyckad()
    {
        Status = WorkflowRunStatus.Failed;
        AvslutadVid = DateTime.UtcNow;
    }
}

public static class WorkflowRunStatus
{
    public const string Running = "Running";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";

    public static readonly string[] All = [Running, Completed, Failed, Cancelled];
    public static bool IsValid(string status) => All.Contains(status);
}
