using RegionHR.Competence.Domain;
using Xunit;

namespace RegionHR.Competence.Tests;

public class CertificationTests
{
    [Fact]
    public void BeraknaStatus_ReturnsGiltig_WhenExpiryIsFarInFuture()
    {
        // Arrange
        var cert = Certification.Skapa(
            Guid.NewGuid(),
            "HLR",
            CertificationType.ObligatoriskUtbildning,
            "Röda Korset",
            DateOnly.FromDateTime(DateTime.UtcNow),
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2)));

        // Act
        var status = cert.BeraknaStatus(DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert
        Assert.Equal(CertificationStatus.Giltig, status);
    }

    [Fact]
    public void BeraknaStatus_ReturnsUtgangen_WhenExpiryIsInPast()
    {
        // Arrange
        var cert = Certification.Skapa(
            Guid.NewGuid(),
            "Brandskydd",
            CertificationType.ObligatoriskUtbildning,
            "MSB",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-2)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)));

        // Act
        var status = cert.BeraknaStatus(DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert
        Assert.Equal(CertificationStatus.Utgangen, status);
    }

    [Fact]
    public void BeraknaStatus_ReturnsUtgarSnart_WhenWithin90Days()
    {
        // Arrange
        var cert = Certification.Skapa(
            Guid.NewGuid(),
            "Sjuksköterskelegitimation",
            CertificationType.Legitimation,
            "Socialstyrelsen",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(60)));

        // Act
        var status = cert.BeraknaStatus(DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert
        Assert.Equal(CertificationStatus.UtgarSnart, status);
    }

    [Fact]
    public void BeraknaStatus_ReturnsGiltig_WhenGiltigTillIsNull()
    {
        // Arrange
        var cert = Certification.Skapa(
            Guid.NewGuid(),
            "Specialistläkare",
            CertificationType.Specialisering,
            "Socialstyrelsen",
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-5)),
            null);

        // Act
        var status = cert.BeraknaStatus(DateOnly.FromDateTime(DateTime.UtcNow));

        // Assert
        Assert.Equal(CertificationStatus.Giltig, status);
    }

    [Fact]
    public void MandatoryTraining_Skapa_SetsPropertiesCorrectly()
    {
        // Act
        var training = MandatoryTraining.Skapa("Sjuksköterska", "HLR", 24, "Obligatorisk för all vårdpersonal");

        // Assert
        Assert.Equal("Sjuksköterska", training.RollNamn);
        Assert.Equal("HLR", training.UtbildningNamn);
        Assert.Equal(24, training.GiltighetManader);
        Assert.Equal("Obligatorisk för all vårdpersonal", training.Beskrivning);
        Assert.NotEqual(Guid.Empty, training.Id);
    }

    [Fact]
    public void MandatoryTraining_Skapa_WithoutBeskrivning_SetsNull()
    {
        // Act
        var training = MandatoryTraining.Skapa("Undersköterska", "Brandskydd", 12);

        // Assert
        Assert.Null(training.Beskrivning);
    }
}
