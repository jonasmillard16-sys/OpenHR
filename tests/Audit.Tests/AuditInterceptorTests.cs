using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RegionHR.Audit.Domain;
using RegionHR.Core.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Audit.Tests;

public class AuditInterceptorTests : IDisposable
{
    private readonly RegionHRDbContext _dbContext;

    public AuditInterceptorTests()
    {
        var interceptor = new AuditInterceptor();
        var options = new DbContextOptionsBuilder<RegionHRDbContext>()
            .UseInMemoryDatabase(databaseName: $"AuditTest-{Guid.NewGuid()}")
            .AddInterceptors(interceptor)
            .Options;

        _dbContext = new RegionHRDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task SaveChanges_WhenEntityAdded_CreatesAuditEntryWithCreateAction()
    {
        // Arrange
        var employee = Employee.Skapa(
            new Personnummer("198112289874"),
            "Anna",
            "Svensson");

        // Act
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        // Assert
        var auditEntries = _dbContext.AuditEntries
            .Where(a => a.EntityType == "Employee")
            .ToList();

        Assert.Single(auditEntries);
        var audit = auditEntries[0];
        Assert.Equal("Employee", audit.EntityType);
        Assert.Equal(employee.Id.ToString(), audit.EntityId);
        Assert.Equal(AuditAction.Create, audit.Action);
        Assert.Null(audit.OldValues);
        Assert.NotNull(audit.NewValues);
        Assert.Equal("system", audit.UserId);
        Assert.Equal("system", audit.UserName);

        // Verify newValues contains expected data
        var newValues = JsonDocument.Parse(audit.NewValues!);
        Assert.True(newValues.RootElement.TryGetProperty("fornamn", out var fornamn));
        Assert.Equal("Anna", fornamn.GetString());
    }

    [Fact]
    public async Task SaveChanges_WhenEntityModified_CreatesAuditEntryWithOldAndNewValues()
    {
        // Arrange - first create and save the employee
        var employee = Employee.Skapa(
            new Personnummer("198112289874"),
            "Anna",
            "Svensson");

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        // Clear audit entries from the create
        var createAudits = _dbContext.AuditEntries.Where(a => a.EntityType == "Employee").ToList();
        Assert.Single(createAudits);

        // Act - modify the employee
        employee.UppdateraKontaktuppgifter("anna@example.com", "070-1234567", null);
        await _dbContext.SaveChangesAsync();

        // Assert
        var updateAudits = _dbContext.AuditEntries
            .Where(a => a.EntityType == "Employee" && a.Action == AuditAction.Update)
            .ToList();

        Assert.Single(updateAudits);
        var audit = updateAudits[0];
        Assert.Equal(AuditAction.Update, audit.Action);
        Assert.Equal(employee.Id.ToString(), audit.EntityId);
        Assert.NotNull(audit.OldValues);
        Assert.NotNull(audit.NewValues);

        // Verify the new values contain updated fields
        var newValues = JsonDocument.Parse(audit.NewValues!);
        Assert.True(newValues.RootElement.TryGetProperty("epost", out var epost));
        Assert.Equal("anna@example.com", epost.GetString());
    }

    [Fact]
    public async Task SaveChanges_WhenEntityDeleted_CreatesAuditEntryWithDeleteAction()
    {
        // Arrange - first create and save the employee
        var employee = Employee.Skapa(
            new Personnummer("198112289874"),
            "Erik",
            "Johansson");

        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync();

        // Act - delete the employee
        _dbContext.Employees.Remove(employee);
        await _dbContext.SaveChangesAsync();

        // Assert
        var deleteAudits = _dbContext.AuditEntries
            .Where(a => a.EntityType == "Employee" && a.Action == AuditAction.Delete)
            .ToList();

        Assert.Single(deleteAudits);
        var audit = deleteAudits[0];
        Assert.Equal(AuditAction.Delete, audit.Action);
        Assert.Equal(employee.Id.ToString(), audit.EntityId);
        Assert.NotNull(audit.OldValues);
        Assert.Null(audit.NewValues);

        // Verify the old values contain the deleted entity data
        var oldValues = JsonDocument.Parse(audit.OldValues!);
        Assert.True(oldValues.RootElement.TryGetProperty("fornamn", out var fornamn));
        Assert.Equal("Erik", fornamn.GetString());
    }

    [Fact]
    public async Task SaveChanges_AuditEntryItself_IsNotAudited()
    {
        // Arrange - manually add an audit entry
        var auditEntry = AuditEntry.Create(
            entityType: "TestEntity",
            entityId: "test-1",
            action: AuditAction.Create,
            oldValues: null,
            newValues: "{}",
            userId: "user-1",
            userName: "test",
            ipAddress: null);

        // Act
        _dbContext.AuditEntries.Add(auditEntry);
        await _dbContext.SaveChangesAsync();

        // Assert - should only have the one we added, no recursive audit of the audit entry
        var allAudits = _dbContext.AuditEntries.ToList();
        Assert.Single(allAudits);
        Assert.Equal("TestEntity", allAudits[0].EntityType);
    }
}
