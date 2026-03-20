using Xunit;
using RegionHR.VMS.Domain;

namespace RegionHR.VMS.Tests;

public class ContingentTimeReportTests
{
    [Fact]
    public void Skapa_SkaparTidrapport_MedDraftStatus()
    {
        var report = ContingentTimeReport.Skapa(Guid.NewGuid(), "2026-03", 160m, 8m, 4m);

        Assert.Equal("2026-03", report.Period);
        Assert.Equal(160m, report.Timmar);
        Assert.Equal(8m, report.OBTimmar);
        Assert.Equal(4m, report.Overtid);
        Assert.Equal(TimeReportStatus.Draft, report.Status);
        Assert.Null(report.AtesteradAv);
    }

    [Fact]
    public void SkickaIn_AndrarStatusTillSubmitted()
    {
        var report = ContingentTimeReport.Skapa(Guid.NewGuid(), "2026-03", 160m, 0m, 0m);

        report.SkickaIn();

        Assert.Equal(TimeReportStatus.Submitted, report.Status);
    }

    [Fact]
    public void SkickaIn_KastarFel_OmInteUtkast()
    {
        var report = ContingentTimeReport.Skapa(Guid.NewGuid(), "2026-03", 160m, 0m, 0m);
        report.SkickaIn();

        Assert.Throws<InvalidOperationException>(() => report.SkickaIn());
    }

    [Fact]
    public void Attestera_AndrarStatusTillAttested_OchSatterAttestant()
    {
        var report = ContingentTimeReport.Skapa(Guid.NewGuid(), "2026-03", 160m, 0m, 0m);
        report.SkickaIn();
        var attestantId = Guid.NewGuid();

        report.Attestera(attestantId);

        Assert.Equal(TimeReportStatus.Attested, report.Status);
        Assert.Equal(attestantId, report.AtesteradAv);
    }

    [Fact]
    public void Attestera_KastarFel_OmInteSubmitted()
    {
        var report = ContingentTimeReport.Skapa(Guid.NewGuid(), "2026-03", 160m, 0m, 0m);

        Assert.Throws<InvalidOperationException>(() => report.Attestera(Guid.NewGuid()));
    }

    [Fact]
    public void Attestera_KastarFel_OmRedanAttesterad()
    {
        var report = ContingentTimeReport.Skapa(Guid.NewGuid(), "2026-03", 160m, 0m, 0m);
        report.SkickaIn();
        report.Attestera(Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => report.Attestera(Guid.NewGuid()));
    }

    [Fact]
    public void FulltWorkflow_DraftTillAttested()
    {
        var report = ContingentTimeReport.Skapa(Guid.NewGuid(), "2026-03", 160m, 16m, 8m);
        var attestant = Guid.NewGuid();

        Assert.Equal(TimeReportStatus.Draft, report.Status);
        report.SkickaIn();
        Assert.Equal(TimeReportStatus.Submitted, report.Status);
        report.Attestera(attestant);
        Assert.Equal(TimeReportStatus.Attested, report.Status);
        Assert.Equal(attestant, report.AtesteradAv);
    }
}
