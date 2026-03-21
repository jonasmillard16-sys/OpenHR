using RegionHR.Helpdesk.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Helpdesk.Tests;

public class ServiceRequestTests
{
    private static ServiceRequest CreateTestRequest(
        ServiceRequestPriority priority = ServiceRequestPriority.Medium)
    {
        return ServiceRequest.Skapa(
            "Testärende",
            "Beskrivning av testärende",
            Guid.NewGuid(),
            priority,
            "Portal",
            EmployeeId.From(Guid.NewGuid()));
    }

    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var kategoriId = Guid.NewGuid();
        var empId = EmployeeId.From(Guid.NewGuid());

        var request = ServiceRequest.Skapa(
            "Min fråga", "Detaljer", kategoriId,
            ServiceRequestPriority.High, "Email", empId);

        Assert.NotEqual(Guid.Empty, request.Id);
        Assert.Equal("Min fråga", request.Titel);
        Assert.Equal("Detaljer", request.Beskrivning);
        Assert.Equal(kategoriId, request.KategoriId);
        Assert.Equal(ServiceRequestPriority.High, request.Prioritet);
        Assert.Equal(ServiceRequestStatus.New, request.Status);
        Assert.Equal("Email", request.KallKanal);
        Assert.Equal(empId, request.InrapportadAv);
        Assert.Null(request.TilldeladAgent);
        Assert.Null(request.TilldeladKo);
        Assert.Null(request.SLADeadline);
        Assert.Null(request.LostVid);
        Assert.Null(request.StangdVid);
        Assert.Null(request.NojdhetsPoang);
    }

    [Fact]
    public void Tilldela_SetsAgentAndStatusToAssigned()
    {
        var request = CreateTestRequest();
        var agentId = Guid.NewGuid();

        request.Tilldela(agentId);

        Assert.Equal(agentId, request.TilldeladAgent);
        Assert.Equal(ServiceRequestStatus.Assigned, request.Status);
    }

    [Fact]
    public void PaborjaArbete_FromNew_SetsStatusToInProgress()
    {
        var request = CreateTestRequest();

        request.PaborjaArbete();

        Assert.Equal(ServiceRequestStatus.InProgress, request.Status);
    }

    [Fact]
    public void PaborjaArbete_FromAssigned_SetsStatusToInProgress()
    {
        var request = CreateTestRequest();
        request.Tilldela(Guid.NewGuid());

        request.PaborjaArbete();

        Assert.Equal(ServiceRequestStatus.InProgress, request.Status);
    }

    [Fact]
    public void PaborjaArbete_FromResolved_ThrowsException()
    {
        var request = CreateTestRequest();
        request.PaborjaArbete();
        request.Los("Löst");

        Assert.Throws<InvalidOperationException>(() => request.PaborjaArbete());
    }

    [Fact]
    public void VantaPaAntalld_SetsStatusToWaitingOnEmployee()
    {
        var request = CreateTestRequest();

        request.VantaPaAntalld();

        Assert.Equal(ServiceRequestStatus.WaitingOnEmployee, request.Status);
    }

    [Fact]
    public void Los_SetsStatusAndTimestamp()
    {
        var request = CreateTestRequest();
        request.PaborjaArbete();

        request.Los("Problem löst genom att...");

        Assert.Equal(ServiceRequestStatus.Resolved, request.Status);
        Assert.NotNull(request.LostVid);
        Assert.True(request.Kommentarer.Count > 0);
        Assert.Contains(request.Kommentarer, c => c.Innehall == "Problem löst genom att...");
    }

    [Fact]
    public void Los_RaisesDomainEvent()
    {
        var request = CreateTestRequest();

        request.Los("Löst");

        Assert.Single(request.DomainEvents);
        var evt = request.DomainEvents[0] as ServiceRequestResolvedEvent;
        Assert.NotNull(evt);
        Assert.Equal(request.Id, evt!.ServiceRequestId);
    }

    [Fact]
    public void Stang_SetsStatusAndTimestamp()
    {
        var request = CreateTestRequest();
        request.Los("Löst");

        request.Stang();

        Assert.Equal(ServiceRequestStatus.Closed, request.Status);
        Assert.NotNull(request.StangdVid);
    }

    [Fact]
    public void Stang_WithoutResolving_SetsLostVid()
    {
        var request = CreateTestRequest();

        request.Stang();

        Assert.Equal(ServiceRequestStatus.Closed, request.Status);
        Assert.NotNull(request.LostVid);
        Assert.NotNull(request.StangdVid);
    }

    [Fact]
    public void SattNojdhet_ValidRange_SetsScore()
    {
        var request = CreateTestRequest();

        request.SattNojdhet(4);

        Assert.Equal(4, request.NojdhetsPoang);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void SattNojdhet_InvalidRange_ThrowsException(int invalidScore)
    {
        var request = CreateTestRequest();

        Assert.Throws<ArgumentOutOfRangeException>(() => request.SattNojdhet(invalidScore));
    }

    [Fact]
    public void LaggTillKommentar_AddsComment()
    {
        var request = CreateTestRequest();
        var authorId = EmployeeId.From(Guid.NewGuid());

        request.LaggTillKommentar(authorId, "En kommentar", false);

        Assert.Single(request.Kommentarer);
        Assert.Equal("En kommentar", request.Kommentarer[0].Innehall);
        Assert.Equal(authorId, request.Kommentarer[0].ForfattareId);
        Assert.False(request.Kommentarer[0].ArIntern);
    }

    [Fact]
    public void LaggTillKommentar_InternalComment_SetsArIntern()
    {
        var request = CreateTestRequest();

        request.LaggTillKommentar(null, "Intern anteckning", true);

        Assert.Single(request.Kommentarer);
        Assert.True(request.Kommentarer[0].ArIntern);
    }

    [Fact]
    public void StallInSLA_SetsSLAProperties()
    {
        var request = CreateTestRequest();
        var slaId = Guid.NewGuid();
        var deadline = DateTime.UtcNow.AddHours(4);

        request.StallInSLA(slaId, deadline);

        Assert.Equal(slaId, request.SLADefinitionId);
        Assert.Equal(deadline, request.SLADeadline);
    }

    // ====================================
    // Full status flow tests
    // ====================================

    [Fact]
    public void FullFlow_New_Assigned_InProgress_Resolved_Closed()
    {
        var request = CreateTestRequest();
        Assert.Equal(ServiceRequestStatus.New, request.Status);

        request.Tilldela(Guid.NewGuid());
        Assert.Equal(ServiceRequestStatus.Assigned, request.Status);

        request.PaborjaArbete();
        Assert.Equal(ServiceRequestStatus.InProgress, request.Status);

        request.Los("Allt löst");
        Assert.Equal(ServiceRequestStatus.Resolved, request.Status);
        Assert.NotNull(request.LostVid);

        request.Stang();
        Assert.Equal(ServiceRequestStatus.Closed, request.Status);
        Assert.NotNull(request.StangdVid);
    }

    [Fact]
    public void FullFlow_WithSatisfaction()
    {
        var request = CreateTestRequest();
        request.Tilldela(Guid.NewGuid());
        request.PaborjaArbete();
        request.Los("Löst");
        request.SattNojdhet(5);

        Assert.Equal(5, request.NojdhetsPoang);
    }

    [Fact]
    public void FullFlow_WithWaitingOnEmployee()
    {
        var request = CreateTestRequest();
        request.Tilldela(Guid.NewGuid());
        request.PaborjaArbete();
        request.VantaPaAntalld();
        Assert.Equal(ServiceRequestStatus.WaitingOnEmployee, request.Status);
    }
}
