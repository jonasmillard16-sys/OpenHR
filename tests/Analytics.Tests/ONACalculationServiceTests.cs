using RegionHR.Analytics.Domain;
using Xunit;

namespace RegionHR.Analytics.Tests;

public class ONACalculationServiceTests
{
    private readonly Guid _surveyId = Guid.NewGuid();
    private readonly Guid _personA = Guid.NewGuid();
    private readonly Guid _personB = Guid.NewGuid();
    private readonly Guid _personC = Guid.NewGuid();
    private readonly Guid _personD = Guid.NewGuid();

    [Fact]
    public void Berakna_EmptyResponses_ReturnsEmpty()
    {
        var result = ONACalculationService.Berakna(_surveyId, Array.Empty<ONAResponse>());

        Assert.Empty(result.Nodes);
        Assert.Empty(result.Edges);
    }

    [Fact]
    public void Berakna_SimpleNetwork_CalculatesCorrectDegrees()
    {
        // A -> B, A -> C, B -> C
        var responses = new[]
        {
            ONAResponse.Skapa(_surveyId, _personA, _personB, 0, 4),
            ONAResponse.Skapa(_surveyId, _personA, _personC, 0, 3),
            ONAResponse.Skapa(_surveyId, _personB, _personC, 0, 5),
        };

        var result = ONACalculationService.Berakna(_surveyId, responses);

        Assert.Equal(3, result.Nodes.Count);
        Assert.Equal(3, result.Edges.Count);

        var nodeA = result.Nodes.First(n => n.AnstallId == _personA);
        var nodeB = result.Nodes.First(n => n.AnstallId == _personB);
        var nodeC = result.Nodes.First(n => n.AnstallId == _personC);

        // A has out-degree 2 (sends to B and C), in-degree 0
        Assert.Equal(2, nodeA.OutDegree);
        Assert.Equal(0, nodeA.InDegree);

        // B has out-degree 1 (sends to C), in-degree 1 (from A)
        Assert.Equal(1, nodeB.OutDegree);
        Assert.Equal(1, nodeB.InDegree);

        // C has out-degree 0, in-degree 2 (from A and B)
        Assert.Equal(0, nodeC.OutDegree);
        Assert.Equal(2, nodeC.InDegree);
    }

    [Fact]
    public void Berakna_AllNodesHaveSurveyId()
    {
        var responses = new[]
        {
            ONAResponse.Skapa(_surveyId, _personA, _personB, 0, 3),
        };

        var result = ONACalculationService.Berakna(_surveyId, responses);

        Assert.All(result.Nodes, n => Assert.Equal(_surveyId, n.SurveyId));
        Assert.All(result.Edges, e => Assert.Equal(_surveyId, e.SurveyId));
    }

    [Fact]
    public void Berakna_AssignsRoleToEachNode()
    {
        var responses = new[]
        {
            ONAResponse.Skapa(_surveyId, _personA, _personB, 0, 4),
            ONAResponse.Skapa(_surveyId, _personB, _personA, 0, 3),
            ONAResponse.Skapa(_surveyId, _personC, _personA, 0, 5),
            ONAResponse.Skapa(_surveyId, _personD, _personA, 0, 4),
        };

        var result = ONACalculationService.Berakna(_surveyId, responses);

        Assert.Equal(4, result.Nodes.Count);
        Assert.All(result.Nodes, n =>
        {
            Assert.False(string.IsNullOrEmpty(n.Roll));
            Assert.Contains(n.Roll, new[] { "ValueCreator", "Influencer", "Bottleneck", "BoundarySpanner", "Isolated" });
        });
    }

    [Fact]
    public void Berakna_IsolatedNode_HasLowDegree()
    {
        // A -> B, C is isolated (only one connection)
        var responses = new[]
        {
            ONAResponse.Skapa(_surveyId, _personA, _personB, 0, 5),
            ONAResponse.Skapa(_surveyId, _personB, _personA, 0, 4),
            ONAResponse.Skapa(_surveyId, _personC, _personA, 0, 2),
        };

        var result = ONACalculationService.Berakna(_surveyId, responses);

        var nodeC = result.Nodes.First(n => n.AnstallId == _personC);
        // C only has out-degree 1, in-degree 0 — total degree = 1
        Assert.Equal(1, nodeC.OutDegree);
        Assert.Equal(0, nodeC.InDegree);
        Assert.Equal("Isolated", nodeC.Roll);
    }

    [Fact]
    public void Berakna_EdgeStrength_EqualsResponseValue()
    {
        var responses = new[]
        {
            ONAResponse.Skapa(_surveyId, _personA, _personB, 0, 4),
        };

        var result = ONACalculationService.Berakna(_surveyId, responses);

        var edge = result.Edges.Single();
        Assert.Equal(_personA, edge.FranAnstallId);
        Assert.Equal(_personB, edge.TillAnstallId);
        Assert.Equal(4m, edge.Styrka);
    }

    [Fact]
    public void Berakna_BetweennessIsNonNegative()
    {
        var responses = new[]
        {
            ONAResponse.Skapa(_surveyId, _personA, _personB, 0, 3),
            ONAResponse.Skapa(_surveyId, _personB, _personC, 0, 4),
            ONAResponse.Skapa(_surveyId, _personC, _personD, 0, 5),
            ONAResponse.Skapa(_surveyId, _personA, _personC, 0, 2),
        };

        var result = ONACalculationService.Berakna(_surveyId, responses);

        Assert.All(result.Nodes, n => Assert.True(n.BetweennessCentrality >= 0));
    }
}
