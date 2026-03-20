using RegionHR.Analytics.Domain;
using Xunit;

namespace RegionHR.Analytics.Tests;

public class KPIDefinitionTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var kpi = KPIDefinition.Skapa(
            "Sjukfranvaro %", "Absence", "sick_days/total_days*100",
            "percent", "LowerIsBetter", 4, 6, 8);

        Assert.NotEqual(Guid.Empty, kpi.Id);
        Assert.Equal("Sjukfranvaro %", kpi.Namn);
        Assert.Equal("Absence", kpi.Kategori);
        Assert.Equal("sick_days/total_days*100", kpi.BerakningsFormel);
        Assert.Equal("percent", kpi.Enhet);
        Assert.Equal("LowerIsBetter", kpi.Riktning);
        Assert.Equal(4, kpi.GronTroskel);
        Assert.Equal(6, kpi.GulTroskel);
        Assert.Equal(8, kpi.RodTroskel);
        Assert.True(kpi.ArAktiv);
    }

    [Fact]
    public void Skapa_DefaultArAktivTrue()
    {
        var kpi = KPIDefinition.Skapa("Test", "Workforce", "COUNT(*)", "count", "HigherIsBetter", 10, 5, 1);
        Assert.True(kpi.ArAktiv);
    }

    [Fact]
    public void Skapa_CanBeCreatedInactive()
    {
        var kpi = KPIDefinition.Skapa("Test", "Workforce", "COUNT(*)", "count", "HigherIsBetter", 10, 5, 1, arAktiv: false);
        Assert.False(kpi.ArAktiv);
    }

    [Fact]
    public void ToggleAktiv_TogglesState()
    {
        var kpi = KPIDefinition.Skapa("Test", "Workforce", "COUNT(*)", "count", "HigherIsBetter", 10, 5, 1);
        Assert.True(kpi.ArAktiv);

        kpi.ToggleAktiv();
        Assert.False(kpi.ArAktiv);

        kpi.ToggleAktiv();
        Assert.True(kpi.ArAktiv);
    }

    [Fact]
    public void UppdateraTrosklar_ChangesValues()
    {
        var kpi = KPIDefinition.Skapa("Test", "Workforce", "COUNT(*)", "count", "HigherIsBetter", 10, 5, 1);

        kpi.UppdateraTrosklar(20, 15, 5);

        Assert.Equal(20, kpi.GronTroskel);
        Assert.Equal(15, kpi.GulTroskel);
        Assert.Equal(5, kpi.RodTroskel);
    }

    [Fact]
    public void Skapa_GeneratesUniqueIds()
    {
        var kpi1 = KPIDefinition.Skapa("KPI 1", "Workforce", "f1", "count", "HigherIsBetter", 10, 5, 1);
        var kpi2 = KPIDefinition.Skapa("KPI 2", "Workforce", "f2", "count", "HigherIsBetter", 10, 5, 1);

        Assert.NotEqual(kpi1.Id, kpi2.Id);
    }
}

public class KPISnapshotTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var kpiId = Guid.NewGuid();
        var before = DateTime.UtcNow;
        var snapshot = KPISnapshot.Skapa(kpiId, "2026-Q1", 42.5m, 40.0m, "Up");
        var after = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, snapshot.Id);
        Assert.Equal(kpiId, snapshot.KPIDefinitionId);
        Assert.Equal("2026-Q1", snapshot.Period);
        Assert.Equal(42.5m, snapshot.Varde);
        Assert.Equal(40.0m, snapshot.JamforelseVarde);
        Assert.Equal("Up", snapshot.Trend);
        Assert.InRange(snapshot.BeraknadVid, before, after);
    }

    [Fact]
    public void Skapa_WithNullJamforelseVarde()
    {
        var snapshot = KPISnapshot.Skapa(Guid.NewGuid(), "2026-Q1", 10, null, "Stable");
        Assert.Null(snapshot.JamforelseVarde);
    }
}

public class KPIAlertTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var kpiId = Guid.NewGuid();
        var alert = KPIAlert.Skapa(kpiId, 8.0m, "hr@region.se");

        Assert.NotEqual(Guid.Empty, alert.Id);
        Assert.Equal(kpiId, alert.KPIDefinitionId);
        Assert.Equal(8.0m, alert.Troskel);
        Assert.Equal("hr@region.se", alert.Mottagare);
        Assert.True(alert.ArAktiv);
    }

    [Fact]
    public void ToggleAktiv_TogglesState()
    {
        var alert = KPIAlert.Skapa(Guid.NewGuid(), 5, "test@test.se");
        Assert.True(alert.ArAktiv);

        alert.ToggleAktiv();
        Assert.False(alert.ArAktiv);
    }
}

public class PredictionModelTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var model = PredictionModel.Skapa("Attrition Model", "Attrition", """{"features":["tenure"]}""");

        Assert.NotEqual(Guid.Empty, model.Id);
        Assert.Equal("Attrition Model", model.Namn);
        Assert.Equal("Attrition", model.Typ);
        Assert.Contains("tenure", model.InputParametrar);
        Assert.Null(model.SenasteTranningsDatum);
        Assert.Null(model.Accuracy);
    }

    [Fact]
    public void UppdateraTranning_SetsAccuracyAndDate()
    {
        var model = PredictionModel.Skapa("Test", "Attrition");
        var before = DateTime.UtcNow;

        model.UppdateraTranning(0.85m);
        var after = DateTime.UtcNow;

        Assert.Equal(0.85m, model.Accuracy);
        Assert.NotNull(model.SenasteTranningsDatum);
        Assert.InRange(model.SenasteTranningsDatum!.Value, before, after);
    }
}

public class PredictionResultTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var modelId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var before = DateTime.UtcNow;

        var result = PredictionResult.Skapa(modelId, "Employee", entityId, 0.78m, "High", """{"factor":"tenure_short"}""");
        var after = DateTime.UtcNow;

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(modelId, result.PredictionModelId);
        Assert.Equal("Employee", result.EntityTyp);
        Assert.Equal(entityId, result.EntityId);
        Assert.Equal(0.78m, result.Score);
        Assert.Equal("High", result.RiskNiva);
        Assert.Contains("tenure_short", result.BidragandeFaktorer);
        Assert.InRange(result.BeraknadVid, before, after);
    }
}

public class ScheduledReportTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var templateId = Guid.NewGuid();
        var scheduled = RegionHR.Reporting.Domain.ScheduledReport.Skapa(templateId, "Weekly", "hr@region.se", "PDF");

        Assert.NotEqual(Guid.Empty, scheduled.Id);
        Assert.Equal(templateId, scheduled.ReportTemplateId);
        Assert.Equal("Weekly", scheduled.Frekvens);
        Assert.Equal("hr@region.se", scheduled.Mottagare);
        Assert.Equal("PDF", scheduled.Format);
        Assert.Null(scheduled.SenastKord);
        Assert.NotNull(scheduled.NastaKorning);
    }

    [Fact]
    public void MarkeraSomKord_SetsSenastKordAndRecalculates()
    {
        var scheduled = RegionHR.Reporting.Domain.ScheduledReport.Skapa(Guid.NewGuid(), "Daily", "a@b.se", "CSV");
        var originalNasta = scheduled.NastaKorning;

        scheduled.MarkeraSomKord();

        Assert.NotNull(scheduled.SenastKord);
        Assert.NotNull(scheduled.NastaKorning);
    }

    [Fact]
    public void UppdateraFrekvens_ChangesFrekvensAndRecalculates()
    {
        var scheduled = RegionHR.Reporting.Domain.ScheduledReport.Skapa(Guid.NewGuid(), "Daily", "a@b.se", "CSV");

        scheduled.UppdateraFrekvens("Monthly", "ny@mottagare.se", "Excel");

        Assert.Equal("Monthly", scheduled.Frekvens);
        Assert.Equal("ny@mottagare.se", scheduled.Mottagare);
        Assert.Equal("Excel", scheduled.Format);
    }
}
