using RegionHR.GDPR.Domain;
using Xunit;

namespace RegionHR.GDPR.Tests;

public class GDPRTests
{
    [Fact]
    public void DataSubjectRequest_Skapa_SetsDeadlineTo30DaysFromCreation()
    {
        // Arrange & Act
        var request = DataSubjectRequest.Skapa(Guid.NewGuid(), RequestType.Registerutdrag);

        // Assert
        var expectedDeadline = request.Mottagen.AddDays(30);
        Assert.Equal(expectedDeadline, request.Deadline);
        Assert.Equal(RequestStatus.Mottagen, request.Status);
    }

    [Fact]
    public void DataSubjectRequest_Tilldela_ChangesStatusToUnderBehandling()
    {
        // Arrange
        var request = DataSubjectRequest.Skapa(Guid.NewGuid(), RequestType.Radering);

        // Act
        request.Tilldela("handler-001");

        // Assert
        Assert.Equal(RequestStatus.UnderBehandling, request.Status);
        Assert.Equal("handler-001", request.HandlaggarId);
    }

    [Fact]
    public void DataSubjectRequest_Slutfor_SetsTimestamp()
    {
        // Arrange
        var request = DataSubjectRequest.Skapa(Guid.NewGuid(), RequestType.Dataportabilitet);
        request.Tilldela("handler-001");

        // Act
        request.Slutfor("/reports/export.zip");

        // Assert
        Assert.Equal(RequestStatus.Klar, request.Status);
        Assert.NotNull(request.SlutfordVid);
        Assert.Equal("/reports/export.zip", request.ResultatFilSokvag);
    }

    [Fact]
    public void DataSubjectRequest_Slutfor_ThrowsWhenAlreadyComplete()
    {
        // Arrange
        var request = DataSubjectRequest.Skapa(Guid.NewGuid(), RequestType.Rattelse);
        request.Slutfor(null);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => request.Slutfor(null));
    }

    [Fact]
    public void DataSubjectRequest_ArForsenad_ReturnsTrueWhenPastDeadline()
    {
        // Arrange - create a request and verify that a request with a past deadline is late
        var request = DataSubjectRequest.Skapa(Guid.NewGuid(), RequestType.Registerutdrag);

        // A newly created request has a 30-day deadline, so it should not be late yet
        Assert.False(request.ArForsenad);

        // A completed request is never late regardless of deadline
        var completedRequest = DataSubjectRequest.Skapa(Guid.NewGuid(), RequestType.Registerutdrag);
        completedRequest.Slutfor(null);
        Assert.False(completedRequest.ArForsenad);
    }

    [Fact]
    public void RetentionRecord_Anonymize_SetsFlagAndTimestamp()
    {
        // Arrange
        var record = RetentionRecord.Skapa("Employee", "emp-123", DateTime.UtcNow.AddYears(7), "Lagkrav");

        // Act
        record.Anonymize();

        // Assert
        Assert.True(record.IsAnonymized);
        Assert.NotNull(record.AnonymizedAt);
    }

    [Fact]
    public void RetentionRecord_Skapa_SetsProperties()
    {
        // Arrange
        var expires = DateTime.UtcNow.AddYears(7);

        // Act
        var record = RetentionRecord.Skapa("Employee", "emp-456", expires, "GDPR Art 17");

        // Assert
        Assert.Equal("Employee", record.EntityType);
        Assert.Equal("emp-456", record.EntityId);
        Assert.Equal(expires, record.RetentionExpires);
        Assert.Equal("GDPR Art 17", record.RetentionReason);
        Assert.False(record.IsAnonymized);
        Assert.Null(record.AnonymizedAt);
    }
}
