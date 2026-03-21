using RegionHR.Analytics.Domain;
using Xunit;

namespace RegionHR.Analytics.Tests;

public class NetworkNodeTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var surveyId = Guid.NewGuid();
        var anstallId = Guid.NewGuid();

        var node = NetworkNode.Skapa(surveyId, anstallId, 5, 3, 0.25m, "Kluster-A", "Influencer");

        Assert.NotEqual(Guid.Empty, node.Id);
        Assert.Equal(surveyId, node.SurveyId);
        Assert.Equal(anstallId, node.AnstallId);
        Assert.Equal(5, node.InDegree);
        Assert.Equal(3, node.OutDegree);
        Assert.Equal(0.25m, node.BetweennessCentrality);
        Assert.Equal("Kluster-A", node.Kluster);
        Assert.Equal("Influencer", node.Roll);
    }

    [Fact]
    public void Skapa_WithEmptySurveyId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            NetworkNode.Skapa(Guid.Empty, Guid.NewGuid(), 1, 1, 0, null, "ValueCreator"));
    }

    [Fact]
    public void Skapa_WithEmptyAnstallId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            NetworkNode.Skapa(Guid.NewGuid(), Guid.Empty, 1, 1, 0, null, "ValueCreator"));
    }

    [Fact]
    public void Skapa_WithEmptyRoll_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            NetworkNode.Skapa(Guid.NewGuid(), Guid.NewGuid(), 1, 1, 0, null, ""));
    }

    [Fact]
    public void Skapa_WithNullKluster_IsAllowed()
    {
        var node = NetworkNode.Skapa(Guid.NewGuid(), Guid.NewGuid(), 1, 1, 0, null, "ValueCreator");
        Assert.Null(node.Kluster);
    }
}

public class NetworkEdgeTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var surveyId = Guid.NewGuid();
        var franId = Guid.NewGuid();
        var tillId = Guid.NewGuid();

        var edge = NetworkEdge.Skapa(surveyId, franId, tillId, 0, 4.5m);

        Assert.NotEqual(Guid.Empty, edge.Id);
        Assert.Equal(surveyId, edge.SurveyId);
        Assert.Equal(franId, edge.FranAnstallId);
        Assert.Equal(tillId, edge.TillAnstallId);
        Assert.Equal(0, edge.FrageIndex);
        Assert.Equal(4.5m, edge.Styrka);
    }

    [Fact]
    public void Skapa_WithEmptySurveyId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            NetworkEdge.Skapa(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), 0, 1));
    }

    [Fact]
    public void Skapa_WithEmptyFranAnstallId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            NetworkEdge.Skapa(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 0, 1));
    }

    [Fact]
    public void Skapa_WithEmptyTillAnstallId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            NetworkEdge.Skapa(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 0, 1));
    }
}
