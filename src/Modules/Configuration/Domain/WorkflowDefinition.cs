namespace RegionHR.Configuration.Domain;

public class WorkflowDefinition
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string TargetEntityType { get; private set; } = "";
    public string StegDefinition { get; private set; } = "[]"; // JSON array of steps
    public bool ArAktiv { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private WorkflowDefinition() { }

    public static WorkflowDefinition Skapa(string namn, string targetEntityType, string stegJson)
    {
        return new WorkflowDefinition
        {
            Id = Guid.NewGuid(), Namn = namn, TargetEntityType = targetEntityType,
            StegDefinition = stegJson, ArAktiv = true, SkapadVid = DateTime.UtcNow
        };
    }

    public void UppdateraSteg(string stegJson) { StegDefinition = stegJson; }
    public void Inaktivera() { ArAktiv = false; }
}
