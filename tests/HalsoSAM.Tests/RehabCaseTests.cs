using RegionHR.HalsoSAM.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.HalsoSAM.Tests;

public class RehabCaseTests
{
    [Fact]
    public void SkapaFranTrigger_SatterKorrektaUppfoljningsDatum()
    {
        // Arrange & Act
        var before = DateTime.UtcNow;
        var rehabCase = RehabCase.Skapa(EmployeeId.New(), RehabTrigger.SexTillfallenTolvManader);
        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(RehabStatus.Signal, rehabCase.Status);
        Assert.NotNull(rehabCase.Uppfoljning14Dagar);
        Assert.NotNull(rehabCase.Uppfoljning90Dagar);
        Assert.NotNull(rehabCase.Uppfoljning180Dagar);
        Assert.NotNull(rehabCase.Uppfoljning365Dagar);

        // Uppföljningsdatumen ska vara relativa till skapandetidpunkten
        Assert.InRange(rehabCase.Uppfoljning14Dagar!.Value,
            before.AddDays(14), after.AddDays(14));
        Assert.InRange(rehabCase.Uppfoljning90Dagar!.Value,
            before.AddDays(90), after.AddDays(90));
        Assert.InRange(rehabCase.Uppfoljning180Dagar!.Value,
            before.AddDays(180), after.AddDays(180));
        Assert.InRange(rehabCase.Uppfoljning365Dagar!.Value,
            before.AddDays(365), after.AddDays(365));
    }

    [Fact]
    public void SkapaFranTrigger_SatterKorrektTrigger()
    {
        var rehabCase = RehabCase.Skapa(EmployeeId.New(), RehabTrigger.FjortonSammanhangandeDagar);
        Assert.Equal(RehabTrigger.FjortonSammanhangandeDagar, rehabCase.Trigger);
    }

    [Fact]
    public void TilldelaArendeagare_BytarStatus()
    {
        // Arrange
        var rehabCase = RehabCase.Skapa(EmployeeId.New(), RehabTrigger.SexTillfallenTolvManader);
        var hrPerson = EmployeeId.New();

        // Act
        rehabCase.TilldelaArendeagare(hrPerson);

        // Assert
        Assert.Equal(RehabStatus.UnderUtredning, rehabCase.Status);
        Assert.Equal(hrPerson, rehabCase.ArendeagareHR);
    }

    [Fact]
    public void SattRehabPlan_BytarStatus()
    {
        // Arrange
        var rehabCase = RehabCase.Skapa(EmployeeId.New(), RehabTrigger.ChefInitierat);
        rehabCase.TilldelaArendeagare(EmployeeId.New());

        // Act
        rehabCase.SattRehabPlan("Gradvis återgång, 50% i 4 veckor");

        // Assert
        Assert.Equal(RehabStatus.AktivRehab, rehabCase.Status);
        Assert.Equal("Gradvis återgång, 50% i 4 veckor", rehabCase.RehabPlan);
    }

    [Fact]
    public void Avsluta_SatterGDPRGallringsDatum()
    {
        // Arrange
        var rehabCase = RehabCase.Skapa(EmployeeId.New(), RehabTrigger.MonsterDetekterat);
        rehabCase.TilldelaArendeagare(EmployeeId.New());
        rehabCase.SattRehabPlan("Testplan");

        // Act
        var before = DateTime.UtcNow;
        rehabCase.Avsluta("Medarbetaren är frisk");
        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(RehabStatus.Avslutad, rehabCase.Status);
        Assert.NotNull(rehabCase.GallringsDatum);
        // Gallringsdatum ska vara ~2 år efter avslut
        Assert.InRange(rehabCase.GallringsDatum!.Value,
            before.AddYears(2), after.AddYears(2));
    }

    [Fact]
    public void Avsluta_LaggTillAnteckning()
    {
        // Arrange
        var hrPerson = EmployeeId.New();
        var rehabCase = RehabCase.Skapa(EmployeeId.New(), RehabTrigger.ChefInitierat);
        rehabCase.TilldelaArendeagare(hrPerson);

        // Act
        rehabCase.Avsluta("Rehabilitering slutförd");

        // Assert
        Assert.Single(rehabCase.Anteckningar);
        Assert.Contains("Ärende avslutat: Rehabilitering slutförd", rehabCase.Anteckningar[0].Text);
    }

    [Fact]
    public void RegistreraUppfoljning_SpararsKorrekt()
    {
        // Arrange
        var hrPerson = EmployeeId.New();
        var rehabCase = RehabCase.Skapa(EmployeeId.New(), RehabTrigger.FjortonSammanhangandeDagar);

        // Act
        rehabCase.RegistreraUppfoljning(14, "Första uppföljningen genomförd", hrPerson);

        // Assert
        Assert.Single(rehabCase.Uppfoljningar);
        Assert.Equal(14, rehabCase.Uppfoljningar[0].DagNr);
        Assert.Equal("Första uppföljningen genomförd", rehabCase.Uppfoljningar[0].Kommentar);
        Assert.Equal(hrPerson, rehabCase.Uppfoljningar[0].UtfordAv);
    }

    [Fact]
    public void RegistreraUppfoljning_OgiltigtDagNr_KastarException()
    {
        var rehabCase = RehabCase.Skapa(EmployeeId.New(), RehabTrigger.ChefInitierat);

        Assert.Throws<ArgumentException>(() =>
            rehabCase.RegistreraUppfoljning(30, "Ogiltig dag", EmployeeId.New()));
    }

    [Fact]
    public void LaggTillAnteckning_SpararsKorrekt()
    {
        // Arrange
        var anstallId = EmployeeId.New();
        var hrPerson = EmployeeId.New();
        var rehabCase = RehabCase.Skapa(anstallId, RehabTrigger.MedarbetareInitierat);

        // Act
        rehabCase.LaggTillAnteckning("Samtal med medarbetaren genomfört", hrPerson);
        rehabCase.LaggTillAnteckning("Läkarintyg mottaget", hrPerson);

        // Assert
        Assert.Equal(2, rehabCase.Anteckningar.Count);
        Assert.Equal("Samtal med medarbetaren genomfört", rehabCase.Anteckningar[0].Text);
        Assert.Equal("Läkarintyg mottaget", rehabCase.Anteckningar[1].Text);
        Assert.Equal(hrPerson, rehabCase.Anteckningar[0].ForfattareId);
    }

    [Fact]
    public void FleraUppfoljningar_KanRegistreras()
    {
        // Arrange
        var hrPerson = EmployeeId.New();
        var rehabCase = RehabCase.Skapa(EmployeeId.New(), RehabTrigger.FjortonSammanhangandeDagar);

        // Act
        rehabCase.RegistreraUppfoljning(14, "Dag 14-uppföljning", hrPerson);
        rehabCase.RegistreraUppfoljning(90, "Dag 90-uppföljning", hrPerson);

        // Assert
        Assert.Equal(2, rehabCase.Uppfoljningar.Count);
        Assert.Equal(14, rehabCase.Uppfoljningar[0].DagNr);
        Assert.Equal(90, rehabCase.Uppfoljningar[1].DagNr);
    }
}
