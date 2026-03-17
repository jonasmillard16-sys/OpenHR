using RegionHR.Reporting.Domain;
using Xunit;

namespace RegionHR.Reporting.Tests;

public class ReportingTests
{
    [Fact]
    public void ReportDefinition_Skapa_SetsProperties()
    {
        // Act
        var report = ReportDefinition.Skapa("Sjukfrånvaro KPI", "Månatlig sjukfrånvarorapport", ReportType.SjukfranvaroKPI);

        // Assert
        Assert.Equal("Sjukfrånvaro KPI", report.Namn);
        Assert.Equal("Månatlig sjukfrånvarorapport", report.Beskrivning);
        Assert.Equal(ReportType.SjukfranvaroKPI, report.Typ);
        Assert.False(report.ArSchemalagd);
        Assert.NotEqual(Guid.Empty, report.Id);
    }

    [Fact]
    public void ReportDefinition_SattSchemalagd_EnablesScheduling()
    {
        // Arrange
        var report = ReportDefinition.Skapa("Löneregister", "Månatligt löneregister", ReportType.Loneregister);

        // Act
        report.SattSchemalagd("0 8 1 * *", "hr@region.se");

        // Assert
        Assert.True(report.ArSchemalagd);
        Assert.Equal("0 8 1 * *", report.CronExpression);
        Assert.Equal("hr@region.se", report.MottagareEpost);
    }

    [Fact]
    public void ReportExecution_Starta_SetsInitialState()
    {
        // Act
        var reportId = Guid.NewGuid();
        var execution = ReportExecution.Starta(reportId, "{\"month\": 3}");

        // Assert
        Assert.Equal(reportId, execution.ReportDefinitionId);
        Assert.Equal(ExecutionStatus.Koar, execution.Status);
        Assert.Equal("{\"month\": 3}", execution.Parametrar);
        Assert.Null(execution.SlutfordVid);
        Assert.NotEqual(Guid.Empty, execution.Id);
    }

    [Fact]
    public void ReportExecution_Slutfor_CompletesSuccessfully()
    {
        // Arrange
        var execution = ReportExecution.Starta(Guid.NewGuid());

        // Act
        execution.Slutfor("/reports/output.xlsx");

        // Assert
        Assert.Equal(ExecutionStatus.Klar, execution.Status);
        Assert.NotNull(execution.SlutfordVid);
        Assert.Equal("/reports/output.xlsx", execution.ResultatFilSokvag);
        Assert.Null(execution.FelMeddelande);
    }

    [Fact]
    public void ReportExecution_MarkeraFel_SetsErrorState()
    {
        // Arrange
        var execution = ReportExecution.Starta(Guid.NewGuid());

        // Act
        execution.MarkeraFel("Databaskoppling misslyckades");

        // Assert
        Assert.Equal(ExecutionStatus.Fel, execution.Status);
        Assert.NotNull(execution.SlutfordVid);
        Assert.Equal("Databaskoppling misslyckades", execution.FelMeddelande);
        Assert.Null(execution.ResultatFilSokvag);
    }

    [Fact]
    public void ReportExecution_Starta_WithoutParameters_SetsNullParametrar()
    {
        // Act
        var execution = ReportExecution.Starta(Guid.NewGuid());

        // Assert
        Assert.Null(execution.Parametrar);
        Assert.Equal(ExecutionStatus.Koar, execution.Status);
    }
}
