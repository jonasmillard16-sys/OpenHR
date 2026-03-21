using RegionHR.Scheduling.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Scheduling.Tests;

public class OpenShiftTests
{
    private readonly OrganizationId _enhetId = OrganizationId.From(Guid.NewGuid());

    [Fact]
    public void Skapa_SetsAllProperties()
    {
        // Arrange & Act
        var shift = OpenShift.Skapa(
            _enhetId,
            new DateOnly(2026, 4, 1),
            "Dag",
            new TimeOnly(7, 0),
            new TimeOnly(15, 30),
            "{\"skills\":[\"HLR\"]}",
            "OB");

        // Assert
        Assert.NotEqual(Guid.Empty, shift.Id);
        Assert.Equal(_enhetId, shift.EnhetId);
        Assert.Equal(new DateOnly(2026, 4, 1), shift.Datum);
        Assert.Equal("Dag", shift.PassTyp);
        Assert.Equal(new TimeOnly(7, 0), shift.StartTid);
        Assert.Equal(new TimeOnly(15, 30), shift.SlutTid);
        Assert.Equal("{\"skills\":[\"HLR\"]}", shift.KravProfil);
        Assert.Equal("OB", shift.Ersattning);
        Assert.Equal(OpenShiftStatus.Published, shift.Status);
        Assert.Null(shift.TilldeladAnstallId);
    }

    [Fact]
    public void Skapa_KasterNarPassTypSaknas()
    {
        Assert.Throws<ArgumentException>(() =>
            OpenShift.Skapa(_enhetId, DateOnly.FromDateTime(DateTime.Today), "", new TimeOnly(7, 0), new TimeOnly(15, 0)));
    }

    [Fact]
    public void Tilldela_SetsStatusAndEmployee()
    {
        // Arrange
        var shift = OpenShift.Skapa(_enhetId, DateOnly.FromDateTime(DateTime.Today), "Dag",
            new TimeOnly(7, 0), new TimeOnly(15, 0));
        var empId = EmployeeId.New();

        // Act
        shift.Tilldela(empId, "Seniority");

        // Assert
        Assert.Equal(OpenShiftStatus.Assigned, shift.Status);
        Assert.Equal(empId, shift.TilldeladAnstallId);
        Assert.Equal("Seniority", shift.TilldelningsMetod);
    }

    [Fact]
    public void Tilldela_KasterNarRedanTilldelad()
    {
        // Arrange
        var shift = OpenShift.Skapa(_enhetId, DateOnly.FromDateTime(DateTime.Today), "Dag",
            new TimeOnly(7, 0), new TimeOnly(15, 0));
        shift.Tilldela(EmployeeId.New(), "FCFS");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            shift.Tilldela(EmployeeId.New(), "Seniority"));
    }

    [Fact]
    public void Avbryt_SetsStatusToCancelled()
    {
        // Arrange
        var shift = OpenShift.Skapa(_enhetId, DateOnly.FromDateTime(DateTime.Today), "Kvall",
            new TimeOnly(15, 0), new TimeOnly(22, 0));

        // Act
        shift.Avbryt();

        // Assert
        Assert.Equal(OpenShiftStatus.Cancelled, shift.Status);
    }

    [Fact]
    public void Avbryt_KasterNarTilldelad()
    {
        // Arrange
        var shift = OpenShift.Skapa(_enhetId, DateOnly.FromDateTime(DateTime.Today), "Dag",
            new TimeOnly(7, 0), new TimeOnly(15, 0));
        shift.Tilldela(EmployeeId.New(), "FCFS");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => shift.Avbryt());
    }

    [Fact]
    public void LaggTillBud_ChangesBiddingStatus()
    {
        // Arrange
        var shift = OpenShift.Skapa(_enhetId, DateOnly.FromDateTime(DateTime.Today), "Dag",
            new TimeOnly(7, 0), new TimeOnly(15, 0));
        var bud = ShiftBid.Skapa(shift.Id, EmployeeId.New());

        // Act
        shift.LaggTillBud(bud);

        // Assert
        Assert.Equal(OpenShiftStatus.Bidding, shift.Status);
        Assert.Single(shift.Bud);
    }

    [Fact]
    public void StatusFlow_Published_Bidding_Assigned()
    {
        // Arrange
        var shift = OpenShift.Skapa(_enhetId, DateOnly.FromDateTime(DateTime.Today), "Natt",
            new TimeOnly(21, 0), new TimeOnly(7, 0));
        Assert.Equal(OpenShiftStatus.Published, shift.Status);

        // Act — bid transitions to Bidding
        shift.LaggTillBud(ShiftBid.Skapa(shift.Id, EmployeeId.New()));
        Assert.Equal(OpenShiftStatus.Bidding, shift.Status);

        // Act — assign transitions to Assigned
        shift.Tilldela(EmployeeId.New(), "Rotation");
        Assert.Equal(OpenShiftStatus.Assigned, shift.Status);
    }
}
