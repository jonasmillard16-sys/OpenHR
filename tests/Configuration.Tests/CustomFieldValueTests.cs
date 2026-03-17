using RegionHR.Configuration.Domain;
using Xunit;

namespace RegionHR.Configuration.Tests;

public class CustomFieldValueTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var fieldId = Guid.NewGuid();
        var value = CustomFieldValue.Skapa(fieldId, "emp-123", "AB+");

        Assert.NotEqual(Guid.Empty, value.Id);
        Assert.Equal(fieldId, value.CustomFieldId);
        Assert.Equal("emp-123", value.EntityId);
        Assert.Equal("AB+", value.Varde);
    }

    [Fact]
    public void UppdateraVarde_ChangesValueAndTimestamp()
    {
        var value = CustomFieldValue.Skapa(Guid.NewGuid(), "emp-1", "gammal");
        var firstTimestamp = value.UppdateradVid;

        value.UppdateraVarde("ny");

        Assert.Equal("ny", value.Varde);
        Assert.True(value.UppdateradVid >= firstTimestamp);
    }

    [Fact]
    public void Skapa_SetsUppdateradVidToApproximatelyUtcNow()
    {
        var before = DateTime.UtcNow;
        var value = CustomFieldValue.Skapa(Guid.NewGuid(), "emp-1", "test");
        var after = DateTime.UtcNow;

        Assert.InRange(value.UppdateradVid, before, after);
    }
}
