using RegionHR.Helpdesk.Domain;
using RegionHR.Infrastructure.Services;
using Xunit;

namespace RegionHR.Helpdesk.Tests;

public class SLATests
{
    [Fact]
    public void SLADefinition_Skapa_SetsAllProperties()
    {
        var sla = SLADefinition.Skapa("Standard", 240, 1440, 480);

        Assert.NotEqual(Guid.Empty, sla.Id);
        Assert.Equal("Standard", sla.Namn);
        Assert.Equal(240, sla.ForsvarstidMinuter);
        Assert.Equal(1440, sla.LostidMinuter);
        Assert.Equal(480, sla.EskaleringEfterMinuter);
        Assert.True(sla.ArAktiv);
    }

    [Fact]
    public void SLADefinition_Skapa_WithoutEscalation()
    {
        var sla = SLADefinition.Skapa("Enkel", 60, 480);

        Assert.Null(sla.EskaleringEfterMinuter);
        Assert.True(sla.ArAktiv);
    }

    [Fact]
    public void SLADefinition_Uppdatera_ChangesProperties()
    {
        var sla = SLADefinition.Skapa("Standard", 240, 1440, 480);

        sla.Uppdatera("Uppdaterad", 120, 720, 240, false);

        Assert.Equal("Uppdaterad", sla.Namn);
        Assert.Equal(120, sla.ForsvarstidMinuter);
        Assert.Equal(720, sla.LostidMinuter);
        Assert.Equal(240, sla.EskaleringEfterMinuter);
        Assert.False(sla.ArAktiv);
    }

    [Fact]
    public void SLAMilestone_Skapa_SetsProperties()
    {
        var requestId = Guid.NewGuid();
        var malTid = DateTime.UtcNow.AddHours(4);

        var milestone = SLAMilestone.Skapa(requestId, "Response", malTid);

        Assert.NotEqual(Guid.Empty, milestone.Id);
        Assert.Equal(requestId, milestone.ServiceRequestId);
        Assert.Equal("Response", milestone.Typ);
        Assert.Equal(malTid, milestone.MalTid);
        Assert.Null(milestone.FaktiskTid);
        Assert.Null(milestone.ArUppfylld);
    }

    [Fact]
    public void SLAMilestone_Uppfyll_WhenWithinSLA_SetsArUppfylldTrue()
    {
        var malTid = DateTime.UtcNow.AddHours(4);
        var milestone = SLAMilestone.Skapa(Guid.NewGuid(), "Response", malTid);

        var faktiskTid = DateTime.UtcNow; // Before deadline
        milestone.Uppfyll(faktiskTid);

        Assert.Equal(faktiskTid, milestone.FaktiskTid);
        Assert.True(milestone.ArUppfylld);
    }

    [Fact]
    public void SLAMilestone_Uppfyll_WhenAfterSLA_SetsArUppfylldFalse()
    {
        var malTid = DateTime.UtcNow.AddHours(-1); // Deadline was 1 hour ago
        var milestone = SLAMilestone.Skapa(Guid.NewGuid(), "Resolution", malTid);

        var faktiskTid = DateTime.UtcNow;
        milestone.Uppfyll(faktiskTid);

        Assert.Equal(faktiskTid, milestone.FaktiskTid);
        Assert.False(milestone.ArUppfylld);
    }

    [Fact]
    public void CalculateDeadline_ReturnsCorrectTime()
    {
        var sla = SLADefinition.Skapa("Test", 60, 480);
        var startTime = new DateTime(2026, 3, 21, 8, 0, 0, DateTimeKind.Utc);

        var deadline = ServiceRequestRouter.CalculateDeadline(sla, startTime);

        Assert.Equal(new DateTime(2026, 3, 21, 16, 0, 0, DateTimeKind.Utc), deadline);
    }

    [Fact]
    public void CalculateDeadline_CriticalSLA_4Hours()
    {
        var sla = SLADefinition.Skapa("Kritisk", 30, 240);
        var startTime = new DateTime(2026, 3, 21, 10, 0, 0, DateTimeKind.Utc);

        var deadline = ServiceRequestRouter.CalculateDeadline(sla, startTime);

        Assert.Equal(new DateTime(2026, 3, 21, 14, 0, 0, DateTimeKind.Utc), deadline);
    }

    [Fact]
    public void CaseSatisfaction_Skapa_ValidScore()
    {
        var satisfaction = CaseSatisfaction.Skapa(Guid.NewGuid(), 4, "Bra service!");

        Assert.Equal(4, satisfaction.Poang);
        Assert.Equal("Bra service!", satisfaction.Kommentar);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void CaseSatisfaction_Skapa_InvalidScore_Throws(int invalidScore)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CaseSatisfaction.Skapa(Guid.NewGuid(), invalidScore));
    }
}
