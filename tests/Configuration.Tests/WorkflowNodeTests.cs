using RegionHR.Configuration.Domain;
using Xunit;

namespace RegionHR.Configuration.Tests;

public class WorkflowNodeTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var wfId = Guid.NewGuid();
        var konfig = """{"godkannareRoll":"Chef"}""";

        var node = WorkflowNode.Skapa(wfId, 2, "Approval", "Chefsgodkannande", konfig);

        Assert.NotEqual(Guid.Empty, node.Id);
        Assert.Equal(wfId, node.WorkflowDefinitionId);
        Assert.Equal(2, node.Ordning);
        Assert.Equal("Approval", node.Typ);
        Assert.Equal("Chefsgodkannande", node.Namn);
        Assert.Equal(konfig, node.Konfiguration);
    }

    [Fact]
    public void Skapa_DefaultsKonfigurationToEmptyObject()
    {
        var node = WorkflowNode.Skapa(Guid.NewGuid(), 1, "Approval", "Test");

        Assert.Equal("{}", node.Konfiguration);
    }

    [Fact]
    public void UppdateraKonfiguration_ChangesKonfiguration()
    {
        var node = WorkflowNode.Skapa(Guid.NewGuid(), 1, "Approval", "Test");
        var nyKonfig = """{"timeoutMinuter":30}""";

        node.UppdateraKonfiguration(nyKonfig);

        Assert.Equal(nyKonfig, node.Konfiguration);
    }

    [Fact]
    public void UppdateraOrdning_ChangesOrdning()
    {
        var node = WorkflowNode.Skapa(Guid.NewGuid(), 1, "Approval", "Test");

        node.UppdateraOrdning(5);

        Assert.Equal(5, node.Ordning);
    }

    [Theory]
    [InlineData("Approval")]
    [InlineData("Notification")]
    [InlineData("FieldUpdate")]
    [InlineData("ExternalCall")]
    [InlineData("Condition")]
    [InlineData("Wait")]
    public void WorkflowNodeTyp_IsValid_ReturnsTrue(string typ)
    {
        Assert.True(WorkflowNodeTyp.IsValid(typ));
    }

    [Fact]
    public void WorkflowNodeTyp_IsValid_ReturnsFalseForInvalid()
    {
        Assert.False(WorkflowNodeTyp.IsValid("Invalid"));
    }

    [Fact]
    public void WorkflowNodeTyp_All_Contains6Types()
    {
        Assert.Equal(6, WorkflowNodeTyp.All.Length);
    }
}
