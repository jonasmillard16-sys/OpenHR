using Xunit;
using RegionHR.VMS.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.VMS.Tests;

public class FSkattRegistrationTests
{
    [Fact]
    public void Skapa_SkaparRegistrering_MedVendorId()
    {
        var vendorId = VendorId.New();
        var reg = FSkattRegistration.Skapa("556789-1234", FSkattStatus.Active, new DateOnly(2027, 12, 31), vendorId: vendorId);

        Assert.NotEqual(Guid.Empty, reg.Id);
        Assert.Equal("556789-1234", reg.Organisationsnummer);
        Assert.Equal(FSkattStatus.Active, reg.FSkattStatus);
        Assert.Equal(new DateOnly(2027, 12, 31), reg.GiltigTill);
        Assert.Equal(vendorId, reg.VendorId);
        Assert.Null(reg.ContingentWorkerId);
    }

    [Fact]
    public void Skapa_SkaparRegistrering_MedContingentWorkerId()
    {
        var workerId = Guid.NewGuid();
        var reg = FSkattRegistration.Skapa("556789-1234", FSkattStatus.Active, null, contingentWorkerId: workerId);

        Assert.Equal(workerId, reg.ContingentWorkerId);
        Assert.Null(reg.VendorId);
    }

    [Fact]
    public void Skapa_KastarFel_UtanBadaIds()
    {
        Assert.Throws<ArgumentException>(() =>
            FSkattRegistration.Skapa("556789-1234", FSkattStatus.Active, null));
    }

    [Fact]
    public void KraverSkatteavdrag_TrueForInactive()
    {
        var reg = FSkattRegistration.Skapa("556789-1234", FSkattStatus.Inactive, null, vendorId: VendorId.New());

        Assert.True(reg.KräverSkatteavdrag);
    }

    [Fact]
    public void KraverSkatteavdrag_FalseForActive()
    {
        var reg = FSkattRegistration.Skapa("556789-1234", FSkattStatus.Active, new DateOnly(2099, 12, 31), vendorId: VendorId.New());

        Assert.False(reg.KräverSkatteavdrag);
    }

    [Fact]
    public void Uppdatera_AndrarStatusOchGiltigTill()
    {
        var reg = FSkattRegistration.Skapa("556789-1234", FSkattStatus.Active, new DateOnly(2027, 12, 31), vendorId: VendorId.New());

        reg.Uppdatera(FSkattStatus.Inactive, null);

        Assert.Equal(FSkattStatus.Inactive, reg.FSkattStatus);
        Assert.Null(reg.GiltigTill);
    }

    [Fact]
    public void SnartUtgaende_TrueForExpiringSoon()
    {
        var soonDate = DateOnly.FromDateTime(DateTime.Today.AddDays(15));
        var reg = FSkattRegistration.Skapa("556789-1234", FSkattStatus.Active, soonDate, vendorId: VendorId.New());

        Assert.True(reg.SnartUtgående(30));
    }

    [Fact]
    public void SnartUtgaende_FalseForFarFuture()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(365));
        var reg = FSkattRegistration.Skapa("556789-1234", FSkattStatus.Active, futureDate, vendorId: VendorId.New());

        Assert.False(reg.SnartUtgående(30));
    }

    [Fact]
    public void HarGattUt_TrueForPastDate()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));
        var reg = FSkattRegistration.Skapa("556789-1234", FSkattStatus.Active, pastDate, vendorId: VendorId.New());

        Assert.True(reg.HarGåttUt());
        Assert.True(reg.KräverSkatteavdrag);
    }
}
