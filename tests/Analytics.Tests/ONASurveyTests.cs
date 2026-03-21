using RegionHR.Analytics.Domain;
using Xunit;

namespace RegionHR.Analytics.Tests;

public class ONASurveyTests
{
    [Fact]
    public void Skapa_SetsDefaults()
    {
        var survey = ONASurvey.Skapa("Samarbetsundersökning", "2026-Q1");

        Assert.NotEqual(Guid.Empty, survey.Id);
        Assert.Equal("Samarbetsundersökning", survey.Namn);
        Assert.Equal("2026-Q1", survey.Period);
        Assert.Equal(ONASurveyStatus.Draft, survey.Status);
        Assert.Equal("[]", survey.Fragor);
    }

    [Fact]
    public void Skapa_WithCustomFragor()
    {
        var fragor = """["Fråga 1","Fråga 2"]""";
        var survey = ONASurvey.Skapa("Test", "2026-Q2", fragor);

        Assert.Equal(fragor, survey.Fragor);
    }

    [Fact]
    public void Skapa_WithEmptyNamn_Throws()
    {
        Assert.Throws<ArgumentException>(() => ONASurvey.Skapa("", "2026-Q1"));
    }

    [Fact]
    public void Oppna_FromDraft_SetsOpen()
    {
        var survey = ONASurvey.Skapa("Test", "2026-Q1");

        survey.Oppna();

        Assert.Equal(ONASurveyStatus.Open, survey.Status);
    }

    [Fact]
    public void Oppna_FromOpen_Throws()
    {
        var survey = ONASurvey.Skapa("Test", "2026-Q1");
        survey.Oppna();

        Assert.Throws<InvalidOperationException>(() => survey.Oppna());
    }

    [Fact]
    public void Stang_FromOpen_SetsClosed()
    {
        var survey = ONASurvey.Skapa("Test", "2026-Q1");
        survey.Oppna();

        survey.Stang();

        Assert.Equal(ONASurveyStatus.Closed, survey.Status);
    }

    [Fact]
    public void Stang_FromDraft_Throws()
    {
        var survey = ONASurvey.Skapa("Test", "2026-Q1");

        Assert.Throws<InvalidOperationException>(() => survey.Stang());
    }

    [Fact]
    public void Analysera_FromClosed_SetsAnalyzed()
    {
        var survey = ONASurvey.Skapa("Test", "2026-Q1");
        survey.Oppna();
        survey.Stang();

        survey.Analysera();

        Assert.Equal(ONASurveyStatus.Analyzed, survey.Status);
    }

    [Fact]
    public void Analysera_FromOpen_Throws()
    {
        var survey = ONASurvey.Skapa("Test", "2026-Q1");
        survey.Oppna();

        Assert.Throws<InvalidOperationException>(() => survey.Analysera());
    }

    [Fact]
    public void FullLifecycle_DraftToAnalyzed()
    {
        var survey = ONASurvey.Skapa("Full lifecycle", "2026-Q4");

        Assert.Equal(ONASurveyStatus.Draft, survey.Status);

        survey.Oppna();
        Assert.Equal(ONASurveyStatus.Open, survey.Status);

        survey.Stang();
        Assert.Equal(ONASurveyStatus.Closed, survey.Status);

        survey.Analysera();
        Assert.Equal(ONASurveyStatus.Analyzed, survey.Status);
    }
}
