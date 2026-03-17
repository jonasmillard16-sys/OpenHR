using RegionHR.CaseManagement.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.CaseManagement.Tests;

public class CaseTests
{
    private readonly EmployeeId _anstallId = EmployeeId.New();

    [Fact]
    public void SkapaFranvaroarende_SetsAllProperties()
    {
        // Arrange
        var fran = new DateOnly(2026, 4, 1);
        var till = new DateOnly(2026, 4, 10);

        // Act
        var arende = Case.SkapaFranvaroarende(
            _anstallId, AbsenceType.Sjukfranvaro, fran, till, "Sjukanmälan");

        // Assert
        Assert.Equal(CaseType.Franvaro, arende.Typ);
        Assert.Equal(_anstallId, arende.AnstallId);
        Assert.Equal(CaseStatus.Oppnad, arende.Status);
        Assert.Equal("Sjukanmälan", arende.Beskrivning);
        Assert.Equal("Inskickat", arende.AktuellSteg);
        Assert.NotNull(arende.FranvaroData);
        Assert.Equal(AbsenceType.Sjukfranvaro, arende.FranvaroData!.FranvaroTyp);
        Assert.Equal(fran, arende.FranvaroData.FranDatum);
        Assert.Equal(till, arende.FranvaroData.TillDatum);
        Assert.Equal(10, arende.FranvaroData.AntalDagar);
        Assert.Null(arende.TilldeladTill);
        Assert.Null(arende.SlutfordVid);
    }

    [Fact]
    public void SkapaAnstallningsandring_SetsCorrectType()
    {
        // Act
        var arende = Case.SkapaAnstallningsandring(_anstallId, "Byta till 80% tjänstgöring");

        // Assert
        Assert.Equal(CaseType.Anstallningsandring, arende.Typ);
        Assert.Equal(_anstallId, arende.AnstallId);
        Assert.Equal(CaseStatus.Oppnad, arende.Status);
        Assert.Equal("Byta till 80% tjänstgöring", arende.Beskrivning);
        Assert.Equal("Inskickat", arende.AktuellSteg);
        Assert.Null(arende.FranvaroData);
    }

    [Fact]
    public void TilldellaTill_SetsHandlaggareAndStatus()
    {
        // Arrange
        var arende = Case.SkapaAnstallningsandring(_anstallId, "Test");
        var handlaggare = EmployeeId.New();

        // Act
        arende.TilldellaTill(handlaggare);

        // Assert
        Assert.Equal(handlaggare, arende.TilldeladTill);
        Assert.Equal(CaseStatus.UnderBehandling, arende.Status);
    }

    [Fact]
    public void SkickaForGodkannande_SetsStegOchStatus()
    {
        // Arrange
        var arende = Case.SkapaAnstallningsandring(_anstallId, "Test");
        var godkannare = EmployeeId.New();

        // Act
        arende.SkickaForGodkannande("Chefsgodkännande", godkannare);

        // Assert
        Assert.Equal(CaseStatus.VantarGodkannande, arende.Status);
        Assert.Equal("Chefsgodkännande", arende.AktuellSteg);
        Assert.Single(arende.Godkannanden);
        Assert.Equal("Chefsgodkännande", arende.Godkannanden[0].Steg);
        Assert.Equal(godkannare, arende.Godkannanden[0].GodkannareId);
        Assert.Equal(ApprovalStatus.Vantar, arende.Godkannanden[0].Status);
    }

    [Fact]
    public void Godkann_SetsApprovalAndRaisesDomainEvent()
    {
        // Arrange
        var arende = Case.SkapaFranvaroarende(
            _anstallId, AbsenceType.Semester,
            new DateOnly(2026, 6, 1), new DateOnly(2026, 6, 5), "Semester");
        var godkannare = EmployeeId.New();
        arende.SkickaForGodkannande("Chefsgodkännande", godkannare);

        // Act
        arende.Godkann(godkannare, "Godkänt, trevlig semester!");

        // Assert
        Assert.Equal(CaseStatus.Godkand, arende.Status);
        Assert.Equal(ApprovalStatus.Godkand, arende.Godkannanden[0].Status);
        Assert.NotNull(arende.Godkannanden[0].BeslutVid);
        Assert.Equal("Godkänt, trevlig semester!", arende.Godkannanden[0].Kommentar);
        Assert.Single(arende.DomainEvents);
        Assert.IsType<CaseApprovedEvent>(arende.DomainEvents[0]);
    }

    [Fact]
    public void Godkann_KastarNarIngetVantandeGodkannande()
    {
        // Arrange
        var arende = Case.SkapaAnstallningsandring(_anstallId, "Test");
        var felGodkannare = EmployeeId.New();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => arende.Godkann(felGodkannare));
    }

    [Fact]
    public void Avsluta_SetsStatusAndTimestamp()
    {
        // Arrange
        var arende = Case.SkapaAnstallningsandring(_anstallId, "Test");

        // Act
        arende.Avsluta();

        // Assert
        Assert.Equal(CaseStatus.Avslutad, arende.Status);
        Assert.NotNull(arende.SlutfordVid);
        Assert.True(arende.SlutfordVid <= DateTime.UtcNow);
    }

    [Fact]
    public void LaggTillKommentar_LaggerTillIListan()
    {
        // Arrange
        var arende = Case.SkapaAnstallningsandring(_anstallId, "Test");
        var forfattare = EmployeeId.New();

        // Act
        arende.LaggTillKommentar(forfattare, "Behöver komplettering");

        // Assert
        Assert.Single(arende.Kommentarer);
        Assert.Equal(forfattare, arende.Kommentarer[0].ForfattareId);
        Assert.Equal("Behöver komplettering", arende.Kommentarer[0].Text);
        Assert.True(arende.Kommentarer[0].SkapadVid <= DateTime.UtcNow);
    }

    [Fact]
    public void LaggTillKommentar_FleraKommentarer()
    {
        // Arrange
        var arende = Case.SkapaAnstallningsandring(_anstallId, "Test");
        var forfattare1 = EmployeeId.New();
        var forfattare2 = EmployeeId.New();

        // Act
        arende.LaggTillKommentar(forfattare1, "Första kommentaren");
        arende.LaggTillKommentar(forfattare2, "Svar på kommentar");

        // Assert
        Assert.Equal(2, arende.Kommentarer.Count);
        Assert.Equal("Första kommentaren", arende.Kommentarer[0].Text);
        Assert.Equal("Svar på kommentar", arende.Kommentarer[1].Text);
    }

    [Fact]
    public void FranvaroData_AntalDagar_BerknasKorrekt()
    {
        // Arrange - 1 dag (samma datum)
        var arende = Case.SkapaFranvaroarende(
            _anstallId, AbsenceType.VAB,
            new DateOnly(2026, 3, 16), new DateOnly(2026, 3, 16), "VAB");

        // Assert
        Assert.Equal(1, arende.FranvaroData!.AntalDagar);
    }

    [Fact]
    public void FranvaroData_Omfattning_DefaultsTill100Procent()
    {
        // Arrange
        var arende = Case.SkapaFranvaroarende(
            _anstallId, AbsenceType.Foraldraledighet,
            new DateOnly(2026, 6, 1), new DateOnly(2026, 12, 31), "Föräldraledighet");

        // Assert
        Assert.Equal(100m, arende.FranvaroData!.Omfattning);
    }
}
