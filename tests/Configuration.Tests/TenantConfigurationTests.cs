using RegionHR.Configuration.Domain;
using Xunit;

namespace RegionHR.Configuration.Tests;

public class TenantConfigurationTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var tenant = TenantConfiguration.Skapa("Region Västra Götaland", "232100-0131");

        Assert.NotEqual(Guid.Empty, tenant.Id);
        Assert.Equal("Region Västra Götaland", tenant.TenantNamn);
        Assert.Equal("232100-0131", tenant.Organisationsnummer);
        Assert.Equal("SE", tenant.Land);
        Assert.Equal("sv", tenant.Sprak);
        Assert.Equal("SEK", tenant.Valuta);
        Assert.True(tenant.ArAktiv);
    }

    [Fact]
    public void Skapa_UsesDefaultLandAndSprak()
    {
        var tenant = TenantConfiguration.Skapa("Test", "123456-7890");

        Assert.Equal("SE", tenant.Land);
        Assert.Equal("sv", tenant.Sprak);
    }

    [Fact]
    public void Skapa_AcceptsCustomLandAndSprak()
    {
        var tenant = TenantConfiguration.Skapa("Test", "123456-7890", "NO", "no");

        Assert.Equal("NO", tenant.Land);
        Assert.Equal("no", tenant.Sprak);
    }

    [Fact]
    public void UppdateraKonfiguration_SetsJsonString()
    {
        var tenant = TenantConfiguration.Skapa("Test", "123456-7890");
        var json = """{"theme":"dark","modules":["payroll","scheduling"]}""";

        tenant.UppdateraKonfiguration(json);

        Assert.Equal(json, tenant.Konfiguration);
    }

    [Fact]
    public void Inaktivera_SetsArAktivToFalse()
    {
        var tenant = TenantConfiguration.Skapa("Test", "123456-7890");
        Assert.True(tenant.ArAktiv);

        tenant.Inaktivera();

        Assert.False(tenant.ArAktiv);
    }

    [Fact]
    public void Skapa_SetsSkapadVidToApproximatelyUtcNow()
    {
        var before = DateTime.UtcNow;
        var tenant = TenantConfiguration.Skapa("Test", "123456-7890");
        var after = DateTime.UtcNow;

        Assert.InRange(tenant.SkapadVid, before, after);
    }
}
