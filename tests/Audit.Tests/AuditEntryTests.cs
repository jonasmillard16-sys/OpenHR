using RegionHR.Audit.Domain;
using Xunit;

namespace RegionHR.Audit.Tests;

public class AuditEntryTests
{
    [Fact]
    public void Create_SetsAllProperties()
    {
        var entry = AuditEntry.Create(
            entityType: "Employee",
            entityId: "emp-123",
            action: AuditAction.Update,
            oldValues: """{"Name":"Anna"}""",
            newValues: """{"Name":"Anna Svensson"}""",
            userId: "user-1",
            userName: "admin",
            ipAddress: "192.168.1.1");

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal("Employee", entry.EntityType);
        Assert.Equal("emp-123", entry.EntityId);
        Assert.Equal(AuditAction.Update, entry.Action);
        Assert.Equal("""{"Name":"Anna"}""", entry.OldValues);
        Assert.Equal("""{"Name":"Anna Svensson"}""", entry.NewValues);
        Assert.Equal("user-1", entry.UserId);
        Assert.Equal("admin", entry.UserName);
        Assert.Equal("192.168.1.1", entry.IpAddress);
    }

    [Fact]
    public void Create_SetsTimestampToApproximatelyUtcNow()
    {
        var before = DateTime.UtcNow;
        var entry = AuditEntry.Create(
            entityType: "Employee",
            entityId: "emp-1",
            action: AuditAction.Create,
            oldValues: null,
            newValues: "{}",
            userId: "user-1",
            userName: null,
            ipAddress: null);
        var after = DateTime.UtcNow;

        Assert.InRange(entry.Timestamp, before, after);
    }

    [Theory]
    [InlineData(AuditAction.Create)]
    [InlineData(AuditAction.Update)]
    [InlineData(AuditAction.Delete)]
    public void AuditAction_HasExpectedValues(AuditAction action)
    {
        var entry = AuditEntry.Create(
            entityType: "Test",
            entityId: "1",
            action: action,
            oldValues: null,
            newValues: null,
            userId: "u",
            userName: null,
            ipAddress: null);

        Assert.Equal(action, entry.Action);
    }

    [Fact]
    public void AuditAction_ContainsExactlyThreeValues()
    {
        var values = Enum.GetValues<AuditAction>();
        Assert.Equal(3, values.Length);
    }
}
