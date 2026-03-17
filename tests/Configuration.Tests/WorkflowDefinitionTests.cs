using RegionHR.Configuration.Domain;
using Xunit;

namespace RegionHR.Configuration.Tests;

public class WorkflowDefinitionTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var steg = """[{"steg":1,"namn":"Granskning","godkannare":"Chef"},{"steg":2,"namn":"Beslut","godkannare":"HR"}]""";
        var workflow = WorkflowDefinition.Skapa("Ledighetsansökan", "LeaveRequest", steg);

        Assert.NotEqual(Guid.Empty, workflow.Id);
        Assert.Equal("Ledighetsansökan", workflow.Namn);
        Assert.Equal("LeaveRequest", workflow.TargetEntityType);
        Assert.Equal(steg, workflow.StegDefinition);
        Assert.True(workflow.ArAktiv);
    }

    [Fact]
    public void UppdateraSteg_ChangesStegDefinition()
    {
        var workflow = WorkflowDefinition.Skapa("Test", "Case", "[]");
        var nyaSteg = """[{"steg":1,"namn":"Auto-godkänn"}]""";

        workflow.UppdateraSteg(nyaSteg);

        Assert.Equal(nyaSteg, workflow.StegDefinition);
    }

    [Fact]
    public void Inaktivera_SetsArAktivToFalse()
    {
        var workflow = WorkflowDefinition.Skapa("Test", "Case", "[]");
        Assert.True(workflow.ArAktiv);

        workflow.Inaktivera();

        Assert.False(workflow.ArAktiv);
    }

    [Fact]
    public void Skapa_SetsSkapadVidToApproximatelyUtcNow()
    {
        var before = DateTime.UtcNow;
        var workflow = WorkflowDefinition.Skapa("Test", "Case", "[]");
        var after = DateTime.UtcNow;

        Assert.InRange(workflow.SkapadVid, before, after);
    }
}
