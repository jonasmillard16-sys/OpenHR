using RegionHR.Performance.Domain;
using Xunit;

namespace RegionHR.Performance.Tests;

public class PerformanceReviewTests
{
    private static PerformanceReview CreateReview() =>
        PerformanceReview.Skapa(Guid.NewGuid(), Guid.NewGuid(), 2026);

    [Fact]
    public void Skapa_SetsDefaults()
    {
        // Arrange
        var anstallId = Guid.NewGuid();
        var chefId = Guid.NewGuid();

        // Act
        var review = PerformanceReview.Skapa(anstallId, chefId, 2026);

        // Assert
        Assert.NotEqual(Guid.Empty, review.Id);
        Assert.Equal(anstallId, review.AnstallId);
        Assert.Equal(chefId, review.ChefId);
        Assert.Equal(2026, review.Ar);
        Assert.Equal(ReviewStatus.Planerad, review.Status);
        Assert.Null(review.SjalvBedomning);
        Assert.Null(review.ChefsBedomning);
        Assert.Null(review.OverallRating);
        Assert.Null(review.Malsattning);
        Assert.Null(review.GenomfordVid);
        Assert.True(review.SkapadVid <= DateTime.UtcNow);
    }

    [Fact]
    public void SattSjalvbedomning_FromPlanerad_TransitionsToSjalvbedomningKlar()
    {
        // Arrange
        var review = CreateReview();

        // Act
        review.SattSjalvbedomning("{\"kompetens\": 4, \"samarbete\": 5}");

        // Assert
        Assert.Equal(ReviewStatus.SjalvbedomningKlar, review.Status);
        Assert.NotNull(review.SjalvBedomning);
    }

    [Fact]
    public void SattChefsbedomning_WithValidRating_TransitionsToChefsbedomningKlar()
    {
        // Arrange
        var review = CreateReview();
        review.SattSjalvbedomning("{\"kompetens\": 4}");

        // Act
        review.SattChefsbedomning("{\"ledarskap\": 3, \"kommentar\": \"Bra insats\"}", 4);

        // Assert
        Assert.Equal(ReviewStatus.ChefsbedomningKlar, review.Status);
        Assert.NotNull(review.ChefsBedomning);
        Assert.Equal(4, review.OverallRating);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(100)]
    public void SattChefsbedomning_WithInvalidRating_Throws(int invalidRating)
    {
        // Arrange
        var review = CreateReview();
        review.SattSjalvbedomning("{\"kompetens\": 4}");

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => review.SattChefsbedomning("{\"omdome\": \"ok\"}", invalidRating));
        Assert.Contains("1", ex.Message);
        Assert.Contains("5", ex.Message);
    }

    [Fact]
    public void Genomfor_RequiresBothAssessments()
    {
        // Arrange - only self-assessment done
        var review = CreateReview();
        review.SattSjalvbedomning("{\"kompetens\": 4}");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => review.Genomfor());
    }

    [Fact]
    public void Genomfor_WhenBothAssessmentsDone_SetsGenomfordStatus()
    {
        // Arrange
        var review = CreateReview();
        review.SattSjalvbedomning("{\"kompetens\": 4}");
        review.SattChefsbedomning("{\"omdome\": \"bra\"}", 4);

        // Act
        review.Genomfor();

        // Assert
        Assert.Equal(ReviewStatus.Genomford, review.Status);
        Assert.NotNull(review.GenomfordVid);
        Assert.True(review.GenomfordVid <= DateTime.UtcNow);
    }

    [Fact]
    public void SattSjalvbedomning_WhenAlreadyComplete_Throws()
    {
        // Arrange
        var review = CreateReview();
        review.SattSjalvbedomning("{\"kompetens\": 4}");
        review.SattChefsbedomning("{\"omdome\": \"bra\"}", 4);
        review.Genomfor();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => review.SattSjalvbedomning("{\"nytt\": true}"));
    }

    [Fact]
    public void SattChefsbedomning_FromPlanerad_Throws()
    {
        // Arrange
        var review = CreateReview();

        // Act & Assert - can't set manager assessment before self-assessment
        Assert.Throws<InvalidOperationException>(
            () => review.SattChefsbedomning("{\"omdome\": \"bra\"}", 3));
    }

    [Fact]
    public void SattMalsattning_CanBeSetAtAnyTime()
    {
        // Arrange
        var review = CreateReview();

        // Act
        review.SattMalsattning("Utveckla ledarskapsförmåga under Q1-Q2");

        // Assert
        Assert.Equal("Utveckla ledarskapsförmåga under Q1-Q2", review.Malsattning);
    }
}
