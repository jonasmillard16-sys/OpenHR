using Microsoft.EntityFrameworkCore;
using RegionHR.Helpdesk.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Helpdesk.Tests;

public class RouterTests
{
    private static RegionHRDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<RegionHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new RegionHRDbContext(options);
    }

    [Fact]
    public async Task RouteAsync_WithDefaultQueue_AssignsQueueToRequest()
    {
        using var db = CreateInMemoryDb();

        var queue = HRQueue.Skapa("Test-kö", "Testkö", new List<Guid> { Guid.NewGuid() });
        db.HRQueues.Add(queue);

        var category = ServiceCategory.Skapa("Test", "Testkategori", defaultKoId: queue.Id);
        db.ServiceCategories.Add(category);
        await db.SaveChangesAsync();

        var request = ServiceRequest.Skapa("Test", "Beskrivning", category.Id,
            ServiceRequestPriority.Medium, "Portal", EmployeeId.From(Guid.NewGuid()));

        var router = new ServiceRequestRouter(db);
        await router.RouteAsync(request);

        Assert.Equal(queue.Id, request.TilldeladKo);
    }

    [Fact]
    public async Task RouteAsync_WithQueueMembers_AssignsAgent()
    {
        using var db = CreateInMemoryDb();

        var agent1 = Guid.NewGuid();
        var agent2 = Guid.NewGuid();
        var queue = HRQueue.Skapa("Test-kö", "Test", new List<Guid> { agent1, agent2 });
        db.HRQueues.Add(queue);

        var category = ServiceCategory.Skapa("Test", "Test", defaultKoId: queue.Id);
        db.ServiceCategories.Add(category);
        await db.SaveChangesAsync();

        var request1 = ServiceRequest.Skapa("Ärende 1", "Besk", category.Id,
            ServiceRequestPriority.Medium, "Portal", EmployeeId.From(Guid.NewGuid()));

        var router = new ServiceRequestRouter(db);
        await router.RouteAsync(request1);

        // Should be assigned to first agent (round-robin start)
        Assert.NotNull(request1.TilldeladAgent);
        Assert.Equal(ServiceRequestStatus.Assigned, request1.Status);

        // Second request should get next agent
        var request2 = ServiceRequest.Skapa("Ärende 2", "Besk", category.Id,
            ServiceRequestPriority.Medium, "Portal", EmployeeId.From(Guid.NewGuid()));
        await router.RouteAsync(request2);

        Assert.NotNull(request2.TilldeladAgent);
        // Round-robin: different agent
        Assert.NotEqual(request1.TilldeladAgent, request2.TilldeladAgent);
    }

    [Fact]
    public async Task RouteAsync_WithSLA_SetsDeadlineAndMilestones()
    {
        using var db = CreateInMemoryDb();

        var sla = SLADefinition.Skapa("Standard", 240, 1440);
        db.SLADefinitions.Add(sla);

        var category = ServiceCategory.Skapa("Test", "Test", defaultSLAId: sla.Id);
        db.ServiceCategories.Add(category);
        await db.SaveChangesAsync();

        var request = ServiceRequest.Skapa("Test", "Besk", category.Id,
            ServiceRequestPriority.Medium, "Portal", EmployeeId.From(Guid.NewGuid()));

        var router = new ServiceRequestRouter(db);
        await router.RouteAsync(request);

        Assert.NotNull(request.SLADeadline);
        Assert.Equal(sla.Id, request.SLADefinitionId);
        Assert.Equal(2, request.SLAMilestones.Count);

        var responseMilestone = request.SLAMilestones.FirstOrDefault(m => m.Typ == "Response");
        var resolutionMilestone = request.SLAMilestones.FirstOrDefault(m => m.Typ == "Resolution");
        Assert.NotNull(responseMilestone);
        Assert.NotNull(resolutionMilestone);
    }

    [Fact]
    public async Task RouteAsync_WithNoCategory_DoesNotThrow()
    {
        using var db = CreateInMemoryDb();

        var request = ServiceRequest.Skapa("Test", "Besk", Guid.NewGuid(),
            ServiceRequestPriority.Medium, "Portal", EmployeeId.From(Guid.NewGuid()));

        var router = new ServiceRequestRouter(db);

        // Should not throw even with non-existent category
        await router.RouteAsync(request);

        Assert.Null(request.TilldeladKo);
        Assert.Null(request.TilldeladAgent);
    }

    [Fact]
    public async Task RouteAsync_WithInactiveSLA_DoesNotSetDeadline()
    {
        using var db = CreateInMemoryDb();

        var sla = SLADefinition.Skapa("Inaktiv", 60, 480, arAktiv: false);
        db.SLADefinitions.Add(sla);

        var category = ServiceCategory.Skapa("Test", "Test", defaultSLAId: sla.Id);
        db.ServiceCategories.Add(category);
        await db.SaveChangesAsync();

        var request = ServiceRequest.Skapa("Test", "Besk", category.Id,
            ServiceRequestPriority.Medium, "Portal", EmployeeId.From(Guid.NewGuid()));

        var router = new ServiceRequestRouter(db);
        await router.RouteAsync(request);

        Assert.Null(request.SLADeadline);
        Assert.Empty(request.SLAMilestones);
    }
}
