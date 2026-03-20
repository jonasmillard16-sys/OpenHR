using RegionHR.Configuration.Domain;
using Xunit;

namespace RegionHR.Configuration.Tests;

public class WorkflowRunInstanceTests
{
    [Fact]
    public void Starta_SetsAllProperties()
    {
        var wfId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        var instance = WorkflowRunInstance.Starta(wfId, "Franvaro", entityId, """{"key":"val"}""");

        Assert.NotEqual(Guid.Empty, instance.Id);
        Assert.Equal(wfId, instance.WorkflowDefinitionId);
        Assert.Equal(1, instance.AktuellNodeOrdning);
        Assert.Equal(WorkflowRunStatus.Running, instance.Status);
        Assert.Equal("Franvaro", instance.EntityTyp);
        Assert.Equal(entityId, instance.EntityId);
        Assert.Equal("""{"key":"val"}""", instance.Data);
        Assert.Null(instance.AvslutadVid);
    }

    [Fact]
    public void Starta_DefaultsDataToEmptyObject()
    {
        var instance = WorkflowRunInstance.Starta(Guid.NewGuid(), "Case", Guid.NewGuid());

        Assert.Equal("{}", instance.Data);
    }

    [Fact]
    public void AvanceraSteg_IncrementsOrdning()
    {
        var instance = WorkflowRunInstance.Starta(Guid.NewGuid(), "Case", Guid.NewGuid());
        Assert.Equal(1, instance.AktuellNodeOrdning);

        instance.AvanceraSteg();

        Assert.Equal(2, instance.AktuellNodeOrdning);
    }

    [Fact]
    public void AvanceraSteg_MultipleTimes_IncrementsCorrectly()
    {
        var instance = WorkflowRunInstance.Starta(Guid.NewGuid(), "Case", Guid.NewGuid());

        instance.AvanceraSteg();
        instance.AvanceraSteg();
        instance.AvanceraSteg();

        Assert.Equal(4, instance.AktuellNodeOrdning);
    }

    [Fact]
    public void AvanceraSteg_WhenCompleted_ThrowsInvalidOperationException()
    {
        var instance = WorkflowRunInstance.Starta(Guid.NewGuid(), "Case", Guid.NewGuid());
        instance.Avsluta();

        Assert.Throws<InvalidOperationException>(() => instance.AvanceraSteg());
    }

    [Fact]
    public void AvanceraSteg_WhenCancelled_ThrowsInvalidOperationException()
    {
        var instance = WorkflowRunInstance.Starta(Guid.NewGuid(), "Case", Guid.NewGuid());
        instance.Avbryt();

        Assert.Throws<InvalidOperationException>(() => instance.AvanceraSteg());
    }

    [Fact]
    public void AvanceraSteg_WhenFailed_ThrowsInvalidOperationException()
    {
        var instance = WorkflowRunInstance.Starta(Guid.NewGuid(), "Case", Guid.NewGuid());
        instance.MarkeraSomMisslyckad();

        Assert.Throws<InvalidOperationException>(() => instance.AvanceraSteg());
    }

    [Fact]
    public void Avsluta_SetsStatusToCompletedAndSetsAvslutadVid()
    {
        var instance = WorkflowRunInstance.Starta(Guid.NewGuid(), "Case", Guid.NewGuid());

        instance.Avsluta();

        Assert.Equal(WorkflowRunStatus.Completed, instance.Status);
        Assert.NotNull(instance.AvslutadVid);
    }

    [Fact]
    public void Avbryt_SetsStatusToCancelledAndSetsAvslutadVid()
    {
        var instance = WorkflowRunInstance.Starta(Guid.NewGuid(), "Case", Guid.NewGuid());

        instance.Avbryt();

        Assert.Equal(WorkflowRunStatus.Cancelled, instance.Status);
        Assert.NotNull(instance.AvslutadVid);
    }

    [Fact]
    public void MarkeraSomMisslyckad_SetsStatusToFailedAndSetsAvslutadVid()
    {
        var instance = WorkflowRunInstance.Starta(Guid.NewGuid(), "Case", Guid.NewGuid());

        instance.MarkeraSomMisslyckad();

        Assert.Equal(WorkflowRunStatus.Failed, instance.Status);
        Assert.NotNull(instance.AvslutadVid);
    }

    [Fact]
    public void Starta_SetsStartadVidToApproximatelyUtcNow()
    {
        var before = DateTime.UtcNow;
        var instance = WorkflowRunInstance.Starta(Guid.NewGuid(), "Case", Guid.NewGuid());
        var after = DateTime.UtcNow;

        Assert.InRange(instance.StartadVid, before, after);
    }

    [Fact]
    public void WorkflowRunStatus_ContainsFourValues()
    {
        Assert.Equal(4, WorkflowRunStatus.All.Length);
    }

    [Theory]
    [InlineData("Running")]
    [InlineData("Completed")]
    [InlineData("Failed")]
    [InlineData("Cancelled")]
    public void WorkflowRunStatus_IsValid_ReturnsTrue(string status)
    {
        Assert.True(WorkflowRunStatus.IsValid(status));
    }
}
