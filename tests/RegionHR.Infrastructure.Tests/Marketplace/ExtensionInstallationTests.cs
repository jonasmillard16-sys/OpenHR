using RegionHR.Platform.Domain;
using Xunit;

namespace RegionHR.Infrastructure.Tests.Marketplace;

public class ExtensionInstallationTests
{
    [Fact]
    public void Installera_SkaparAktivInstallation()
    {
        var extensionId = Guid.NewGuid();
        var installation = ExtensionInstallation.Installera(extensionId, "1.0.0");

        Assert.NotEqual(Guid.Empty, installation.Id);
        Assert.Equal(extensionId, installation.ExtensionId);
        Assert.Equal("1.0.0", installation.Version);
        Assert.Equal(InstallationStatus.Active, installation.Status);
        Assert.Equal("{}", installation.Konfiguration);
    }

    [Fact]
    public void Installera_MedKonfiguration_SparKonfiguration()
    {
        var config = """{"enabled":true}""";
        var installation = ExtensionInstallation.Installera(Guid.NewGuid(), "1.0.0", config);

        Assert.Equal(config, installation.Konfiguration);
    }

    [Fact]
    public void Installera_UtanExtensionId_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            ExtensionInstallation.Installera(Guid.Empty, "1.0.0"));
    }

    [Fact]
    public void Installera_UtanVersion_KastarException()
    {
        Assert.Throws<ArgumentException>(() =>
            ExtensionInstallation.Installera(Guid.NewGuid(), ""));
    }

    [Fact]
    public void Inaktivera_AktivInstallation_AndraTillDisabled()
    {
        var installation = ExtensionInstallation.Installera(Guid.NewGuid(), "1.0.0");

        installation.Inaktivera();

        Assert.Equal(InstallationStatus.Disabled, installation.Status);
    }

    [Fact]
    public void Inaktivera_RedanInaktiv_KastarException()
    {
        var installation = ExtensionInstallation.Installera(Guid.NewGuid(), "1.0.0");
        installation.Inaktivera();

        Assert.Throws<InvalidOperationException>(() => installation.Inaktivera());
    }

    [Fact]
    public void Aktivera_InaktivInstallation_AndraTillActive()
    {
        var installation = ExtensionInstallation.Installera(Guid.NewGuid(), "1.0.0");
        installation.Inaktivera();

        installation.Aktivera();

        Assert.Equal(InstallationStatus.Active, installation.Status);
    }

    [Fact]
    public void Aktivera_RedanAktiv_KastarException()
    {
        var installation = ExtensionInstallation.Installera(Guid.NewGuid(), "1.0.0");

        Assert.Throws<InvalidOperationException>(() => installation.Aktivera());
    }

    [Fact]
    public void StatusFlow_FullCykel()
    {
        var installation = ExtensionInstallation.Installera(Guid.NewGuid(), "2.0.0");
        Assert.Equal(InstallationStatus.Active, installation.Status);

        installation.Inaktivera();
        Assert.Equal(InstallationStatus.Disabled, installation.Status);

        installation.Aktivera();
        Assert.Equal(InstallationStatus.Active, installation.Status);

        installation.Inaktivera();
        Assert.Equal(InstallationStatus.Disabled, installation.Status);
    }
}
