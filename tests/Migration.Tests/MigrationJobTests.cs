using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Migration.Tests;

public class MigrationJobTests
{
    [Fact]
    public void Skapa_SkaparJobbMedStatusCreated()
    {
        var job = MigrationJob.Skapa(SourceSystem.PAXml, "test.xml", "admin");

        Assert.Equal(MigrationJobStatus.Created, job.Status);
        Assert.Equal(SourceSystem.PAXml, job.Kalla);
        Assert.Equal("test.xml", job.FilNamn);
        Assert.Equal("admin", job.SkapadAv);
        Assert.Equal(0, job.TotaltAntalRader);
        Assert.Equal(0, job.ImporteradeRader);
        Assert.Equal(0, job.FelRader);
    }

    [Fact]
    public void Skapa_KastarFelOmFilnamnSaknas()
    {
        Assert.Throws<ArgumentException>(() =>
            MigrationJob.Skapa(SourceSystem.PAXml, "", "admin"));
    }

    [Fact]
    public void Skapa_KastarFelOmSkapadAvSaknas()
    {
        Assert.Throws<ArgumentException>(() =>
            MigrationJob.Skapa(SourceSystem.PAXml, "test.xml", ""));
    }

    [Fact]
    public void StartaValidering_FranCreated_LyckasBytStatus()
    {
        var job = MigrationJob.Skapa(SourceSystem.HEROMA, "data.csv", "user1");

        job.StartaValidering();

        Assert.Equal(MigrationJobStatus.Validating, job.Status);
    }

    [Fact]
    public void StartaValidering_FranFelStatus_KastarException()
    {
        var job = MigrationJob.Skapa(SourceSystem.HEROMA, "data.csv", "user1");
        job.StartaValidering(); // Now Validating

        Assert.Throws<InvalidOperationException>(() => job.StartaValidering());
    }

    [Fact]
    public void StartaDryRun_FranValidating_Lyckas()
    {
        var job = MigrationJob.Skapa(SourceSystem.PAXml, "test.xml", "admin");
        job.StartaValidering();

        job.StartaDryRun();

        Assert.Equal(MigrationJobStatus.DryRun, job.Status);
    }

    [Fact]
    public void StartaDryRun_FranCreated_KastarException()
    {
        var job = MigrationJob.Skapa(SourceSystem.PAXml, "test.xml", "admin");

        Assert.Throws<InvalidOperationException>(() => job.StartaDryRun());
    }

    [Fact]
    public void StartaImport_FranDryRun_Lyckas()
    {
        var job = MigrationJob.Skapa(SourceSystem.GenericCSV, "data.csv", "admin");
        job.StartaValidering();
        job.StartaDryRun();

        job.StartaImport();

        Assert.Equal(MigrationJobStatus.Importing, job.Status);
    }

    [Fact]
    public void StartaImport_FranValidating_KastarException()
    {
        var job = MigrationJob.Skapa(SourceSystem.GenericCSV, "data.csv", "admin");
        job.StartaValidering();

        Assert.Throws<InvalidOperationException>(() => job.StartaImport());
    }

    [Fact]
    public void Slutfor_FranImporting_Lyckas()
    {
        var job = MigrationJob.Skapa(SourceSystem.PAXml, "test.xml", "admin");
        job.StartaValidering();
        job.StartaDryRun();
        job.StartaImport();

        job.Slutfor(100, 95, 5);

        Assert.Equal(MigrationJobStatus.Complete, job.Status);
        Assert.Equal(100, job.TotaltAntalRader);
        Assert.Equal(95, job.ImporteradeRader);
        Assert.Equal(5, job.FelRader);
    }

    [Fact]
    public void Slutfor_FranCreated_KastarException()
    {
        var job = MigrationJob.Skapa(SourceSystem.PAXml, "test.xml", "admin");

        Assert.Throws<InvalidOperationException>(() => job.Slutfor(100, 95, 5));
    }

    [Fact]
    public void MarkeraMisslyckad_SatterStatusOchFelmeddelande()
    {
        var job = MigrationJob.Skapa(SourceSystem.PAXml, "test.xml", "admin");
        job.StartaValidering();

        job.MarkeraMisslyckad("Ogiltigt filformat");

        Assert.Equal(MigrationJobStatus.Failed, job.Status);
        Assert.Equal("Ogiltigt filformat", job.FelMeddelande);
    }

    [Fact]
    public void HeltFlode_Created_Till_Complete()
    {
        var job = MigrationJob.Skapa(SourceSystem.HEROMA, "heroma-export.csv", "hr-admin");

        Assert.Equal(MigrationJobStatus.Created, job.Status);

        job.StartaValidering();
        Assert.Equal(MigrationJobStatus.Validating, job.Status);

        job.StartaDryRun();
        Assert.Equal(MigrationJobStatus.DryRun, job.Status);

        job.StartaImport();
        Assert.Equal(MigrationJobStatus.Importing, job.Status);

        job.Slutfor(500, 498, 2);
        Assert.Equal(MigrationJobStatus.Complete, job.Status);
    }

    [Fact]
    public void LaggTillValideringsFel_OkarFelRader()
    {
        var job = MigrationJob.Skapa(SourceSystem.PAXml, "test.xml", "admin");
        var fel = MigrationValidationError.Skapa(job.Id, 1, "Personnummer", "Ogiltigt format", "12345", "198501151234");

        job.LaggTillValideringsFel(fel);

        Assert.Single(job.ValideringsFel);
        Assert.Equal(1, job.FelRader);
    }

    [Fact]
    public void LaggTillLogg_MedSuccess_OkarImporteradeRader()
    {
        var job = MigrationJob.Skapa(SourceSystem.PAXml, "test.xml", "admin");
        var logg = MigrationLog.Skapa(job.Id, "Employee", MigrationLogStatus.Success, Guid.NewGuid());

        job.LaggTillLogg(logg);

        Assert.Single(job.Logg);
        Assert.Equal(1, job.ImporteradeRader);
    }

    [Fact]
    public void LaggTillLogg_MedError_OkarFelRader()
    {
        var job = MigrationJob.Skapa(SourceSystem.PAXml, "test.xml", "admin");
        var logg = MigrationLog.Skapa(job.Id, "Employee", MigrationLogStatus.Error, felMeddelande: "Duplikat personnummer");

        job.LaggTillLogg(logg);

        Assert.Single(job.Logg);
        Assert.Equal(1, job.FelRader);
    }
}
