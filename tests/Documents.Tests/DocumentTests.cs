using RegionHR.Documents.Domain;
using Xunit;

namespace RegionHR.Documents.Tests;

public class DocumentTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        // Arrange
        var anstallId = Guid.NewGuid();
        var kategori = DocumentCategory.Anstallningsavtal;
        var fileName = "avtal.pdf";
        var storagePath = "/documents/2026/avtal.pdf";
        var fileSize = 1024L;
        var contentType = "application/pdf";
        var uppladdadAv = "admin@region.se";
        var klassificering = DataClassification.Kansllig;
        var beskrivning = "Anställningsavtal för läkare";

        // Act
        var doc = Document.Skapa(
            anstallId, kategori, fileName, storagePath,
            fileSize, contentType, uppladdadAv, klassificering, beskrivning);

        // Assert
        Assert.NotEqual(Guid.Empty, doc.Id);
        Assert.Equal(anstallId, doc.AnstallId);
        Assert.Equal(kategori, doc.Kategori);
        Assert.Equal(fileName, doc.FileName);
        Assert.Equal(storagePath, doc.StoragePath);
        Assert.Equal(fileSize, doc.FileSizeBytes);
        Assert.Equal(contentType, doc.ContentType);
        Assert.Equal(uppladdadAv, doc.UppladdadAv);
        Assert.Equal(klassificering, doc.Klassificering);
        Assert.Equal(beskrivning, doc.Beskrivning);
        Assert.False(doc.IsArchived);
        Assert.Null(doc.RetentionUntil);
        Assert.True(doc.UppladdadVid <= DateTime.UtcNow);
    }

    [Fact]
    public void Archive_SetsIsArchivedTrue()
    {
        // Arrange
        var doc = Document.Skapa(
            Guid.NewGuid(), DocumentCategory.Ovrigt,
            "test.txt", "/path", 100, "text/plain", "user");

        // Act
        doc.Archive();

        // Assert
        Assert.True(doc.IsArchived);
    }

    [Fact]
    public void ShouldBeRetained_WithFutureDate_ReturnsTrue()
    {
        // Arrange
        var doc = Document.Skapa(
            Guid.NewGuid(), DocumentCategory.Lakarintyg,
            "intyg.pdf", "/path", 200, "application/pdf", "user");
        doc.SetRetention(DateTime.UtcNow.AddYears(1));

        // Act
        var result = doc.ShouldBeRetained();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldBeRetained_WithPastDate_ReturnsFalse()
    {
        // Arrange
        var doc = Document.Skapa(
            Guid.NewGuid(), DocumentCategory.Lakarintyg,
            "intyg.pdf", "/path", 200, "application/pdf", "user");
        doc.SetRetention(DateTime.UtcNow.AddDays(-1));

        // Act
        var result = doc.ShouldBeRetained();

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(DocumentCategory.Lonespecifikation, 7)]
    [InlineData(DocumentCategory.Lakarintyg, 2)]
    [InlineData(DocumentCategory.Legitimation, 10)]
    [InlineData(DocumentCategory.Betyg, 2)]
    [InlineData(DocumentCategory.Anstallningsavtal, 7)]
    [InlineData(DocumentCategory.Policy, 5)]
    [InlineData(DocumentCategory.Tjanstgoringsbevis, 5)]
    [InlineData(DocumentCategory.Ovrigt, 5)]
    public void RetentionPolicy_ReturnsCorrectDuration(DocumentCategory category, int expectedYears)
    {
        // Arrange
        var referenceDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = RetentionPolicy.CalculateRetention(category, referenceDate);

        // Assert
        Assert.Equal(referenceDate.AddYears(expectedYears), result);
    }
}
