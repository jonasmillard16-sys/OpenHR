using Xunit;
using RegionHR.VMS.Domain;

namespace RegionHR.VMS.Tests;

public class VendorTests
{
    [Fact]
    public void Skapa_SkaparLeverantor_MedActiveStatus()
    {
        var vendor = Vendor.Skapa("MedStaff AB", "556789-1234", "Anna Svensson", "anna@medstaff.se", "031-123456", "Sjukvard");

        Assert.Equal("MedStaff AB", vendor.Namn);
        Assert.Equal("556789-1234", vendor.OrgNummer);
        Assert.Equal("Anna Svensson", vendor.Kontaktperson);
        Assert.Equal("anna@medstaff.se", vendor.Epost);
        Assert.Equal("Sjukvard", vendor.Kategori);
        Assert.Equal(VendorStatus.Active, vendor.Status);
        Assert.NotEqual(default, vendor.Id);
    }

    [Fact]
    public void Blockera_AndrarStatusTillBlocked()
    {
        var vendor = Vendor.Skapa("Test AB", "556000-0001", "Test", "test@test.se", "070-0000000", "IT");

        vendor.Blockera();

        Assert.Equal(VendorStatus.Blocked, vendor.Status);
    }

    [Fact]
    public void Blockera_KastarFel_OmRedanBlockerad()
    {
        var vendor = Vendor.Skapa("Test AB", "556000-0001", "Test", "test@test.se", "070-0000000", "IT");
        vendor.Blockera();

        Assert.Throws<InvalidOperationException>(() => vendor.Blockera());
    }

    [Fact]
    public void Aktivera_AndrarStatusTillActive()
    {
        var vendor = Vendor.Skapa("Test AB", "556000-0001", "Test", "test@test.se", "070-0000000", "IT");
        vendor.Blockera();

        vendor.Aktivera();

        Assert.Equal(VendorStatus.Active, vendor.Status);
    }

    [Fact]
    public void Aktivera_KastarFel_OmRedanAktiv()
    {
        var vendor = Vendor.Skapa("Test AB", "556000-0001", "Test", "test@test.se", "070-0000000", "IT");

        Assert.Throws<InvalidOperationException>(() => vendor.Aktivera());
    }

    [Fact]
    public void Inaktivera_AndrarStatusTillInactive()
    {
        var vendor = Vendor.Skapa("Test AB", "556000-0001", "Test", "test@test.se", "070-0000000", "IT");

        vendor.Inaktivera();

        Assert.Equal(VendorStatus.Inactive, vendor.Status);
    }

    [Fact]
    public void Blockera_Aktivera_Blockera_FungerarISekevens()
    {
        var vendor = Vendor.Skapa("Test AB", "556000-0001", "Test", "test@test.se", "070-0000000", "IT");

        vendor.Blockera();
        Assert.Equal(VendorStatus.Blocked, vendor.Status);

        vendor.Aktivera();
        Assert.Equal(VendorStatus.Active, vendor.Status);

        vendor.Blockera();
        Assert.Equal(VendorStatus.Blocked, vendor.Status);
    }
}
