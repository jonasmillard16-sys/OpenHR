using RegionHR.Offboarding.Domain;
using Xunit;

namespace RegionHR.Offboarding.Tests;

public class OffboardingTests
{
    [Fact]
    public void Skapa_SetsDefaultsAndStandardSteps()
    {
        // Arrange
        var anstallId = Guid.NewGuid();
        var sistadag = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));

        // Act
        var offboarding = OffboardingCase.Skapa(anstallId, AvslutAnledning.EgenBegaran, sistadag);

        // Assert
        Assert.NotEqual(Guid.Empty, offboarding.Id);
        Assert.Equal(anstallId, offboarding.AnstallId);
        Assert.Equal(AvslutAnledning.EgenBegaran, offboarding.Anledning);
        Assert.Equal(sistadag, offboarding.SistaArbetsdag);
        Assert.Equal(OffboardingStatus.Skapad, offboarding.Status);
        Assert.True(offboarding.ArReHireEligible);
        Assert.Equal(8, offboarding.Steg.Count);
    }

    [Fact]
    public void MarkeraStegKlart_MarksStepAsComplete()
    {
        // Arrange
        var offboarding = OffboardingCase.Skapa(Guid.NewGuid(), AvslutAnledning.Pension, DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)));

        // Act
        offboarding.MarkeraStegKlart(0);

        // Assert
        Assert.True(offboarding.Steg[0].Klar);
        Assert.NotNull(offboarding.Steg[0].KlarVid);
        Assert.False(offboarding.Steg[1].Klar);
    }

    [Fact]
    public void MarkeraStegKlart_ThrowsForInvalidIndex()
    {
        // Arrange
        var offboarding = OffboardingCase.Skapa(Guid.NewGuid(), AvslutAnledning.EgenBegaran, DateOnly.FromDateTime(DateTime.UtcNow));

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => offboarding.MarkeraStegKlart(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => offboarding.MarkeraStegKlart(100));
    }

    [Fact]
    public void RegistreraExitSamtal_SetsCommentAndFlag()
    {
        // Arrange
        var offboarding = OffboardingCase.Skapa(Guid.NewGuid(), AvslutAnledning.EgenBegaran, DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        offboarding.RegistreraExitSamtal("Bra arbetsmiljö men önskade högre lön");

        // Assert
        Assert.True(offboarding.ExitSamtalGenomfort);
        Assert.Equal("Bra arbetsmiljö men önskade högre lön", offboarding.ExitSamtalKommentar);
    }

    [Fact]
    public void SattReHireStatus_SetsEligibilityAndComment()
    {
        // Arrange
        var offboarding = OffboardingCase.Skapa(Guid.NewGuid(), AvslutAnledning.Uppsagning, DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        offboarding.SattReHireStatus(false, "Allvarliga samarbetsproblem");

        // Assert
        Assert.False(offboarding.ArReHireEligible);
        Assert.Equal("Allvarliga samarbetsproblem", offboarding.ReHireKommentar);
    }

    [Fact]
    public void Slutfor_ThrowsWhenStepsIncomplete()
    {
        // Arrange
        var offboarding = OffboardingCase.Skapa(Guid.NewGuid(), AvslutAnledning.EgenBegaran, DateOnly.FromDateTime(DateTime.UtcNow));
        offboarding.MarkeraStegKlart(0); // only one step complete

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => offboarding.Slutfor());
    }

    [Fact]
    public void Slutfor_SucceedsWhenAllStepsComplete()
    {
        // Arrange
        var offboarding = OffboardingCase.Skapa(Guid.NewGuid(), AvslutAnledning.Pension, DateOnly.FromDateTime(DateTime.UtcNow));
        for (int i = 0; i < offboarding.Steg.Count; i++)
            offboarding.MarkeraStegKlart(i);

        // Act
        offboarding.Slutfor();

        // Assert
        Assert.Equal(OffboardingStatus.Slutford, offboarding.Status);
        Assert.NotNull(offboarding.SlutfordVid);
    }

    [Fact]
    public void MarkeraSomPagar_SetsStatus()
    {
        // Arrange
        var offboarding = OffboardingCase.Skapa(Guid.NewGuid(), AvslutAnledning.Vikariat_Slut, DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        offboarding.MarkeraSomPagar();

        // Assert
        Assert.Equal(OffboardingStatus.Pagar, offboarding.Status);
    }

    [Fact]
    public void Skapa_StandardStepsContainExpectedItems()
    {
        // Arrange & Act
        var offboarding = OffboardingCase.Skapa(Guid.NewGuid(), AvslutAnledning.EgenBegaran, DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert
        Assert.Contains(offboarding.Steg, s => s.Beskrivning.Contains("utrustning"));
        Assert.Contains(offboarding.Steg, s => s.Beskrivning.Contains("IT-behörigheter"));
        Assert.Contains(offboarding.Steg, s => s.Beskrivning.Contains("tjänstgöringsintyg"));
        Assert.Contains(offboarding.Steg, s => s.Beskrivning.Contains("arbetsgivarintyg"));
        Assert.Contains(offboarding.Steg, s => s.Beskrivning.Contains("Slutlön"));
        Assert.Contains(offboarding.Steg, s => s.Beskrivning.Contains("Exit-samtal"));
        Assert.Contains(offboarding.Steg, s => s.Beskrivning.Contains("Kunskapsöverföring"));
        Assert.Contains(offboarding.Steg, s => s.Beskrivning.Contains("GDPR"));
    }
}
