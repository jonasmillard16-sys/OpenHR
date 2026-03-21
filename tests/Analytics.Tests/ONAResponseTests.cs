using RegionHR.Analytics.Domain;
using Xunit;

namespace RegionHR.Analytics.Tests;

public class ONAResponseTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var surveyId = Guid.NewGuid();
        var respondentId = Guid.NewGuid();
        var nomineradId = Guid.NewGuid();

        var response = ONAResponse.Skapa(surveyId, respondentId, nomineradId, 0, 4);

        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Equal(surveyId, response.SurveyId);
        Assert.Equal(respondentId, response.RespondentId);
        Assert.Equal(nomineradId, response.NomineradId);
        Assert.Equal(0, response.FrageIndex);
        Assert.Equal(4, response.Varde);
    }

    [Fact]
    public void Skapa_WithEmptySurveyId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            ONAResponse.Skapa(Guid.Empty, Guid.NewGuid(), Guid.NewGuid(), 0, 3));
    }

    [Fact]
    public void Skapa_WithEmptyRespondentId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            ONAResponse.Skapa(Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 0, 3));
    }

    [Fact]
    public void Skapa_WithEmptyNomineradId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            ONAResponse.Skapa(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 0, 3));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(100)]
    public void Skapa_WithInvalidVarde_Throws(int invalidVarde)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ONAResponse.Skapa(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0, invalidVarde));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Skapa_WithValidVarde_Succeeds(int varde)
    {
        var response = ONAResponse.Skapa(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0, varde);
        Assert.Equal(varde, response.Varde);
    }
}
