namespace RegionHR.Performance.Domain;

public enum ReviewStatus
{
    Planerad,
    Paborjad,
    SjalvbedomningKlar,
    ChefsbedomningKlar,
    Genomford,
    Avslutat
}

/// <summary>
/// Medarbetarsamtal (performance review / utvecklingssamtal).
/// </summary>
public sealed class PerformanceReview
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public Guid ChefId { get; private set; }
    public int Ar { get; private set; }
    public ReviewStatus Status { get; private set; }
    public string? SjalvBedomning { get; private set; }
    public string? ChefsBedomning { get; private set; }
    public int? OverallRating { get; private set; }
    public string? Malsattning { get; private set; }
    public string? Kommentar { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public DateTime? GenomfordVid { get; private set; }

    private PerformanceReview() { } // EF Core

    public static PerformanceReview Skapa(Guid anstallId, Guid chefId, int ar)
    {
        return new PerformanceReview
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            ChefId = chefId,
            Ar = ar,
            Status = ReviewStatus.Planerad,
            SkapadVid = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Medarbetaren lämnar sin självbedömning.
    /// </summary>
    public void SattSjalvbedomning(string bedomning)
    {
        if (Status is not (ReviewStatus.Planerad or ReviewStatus.Paborjad))
            throw new InvalidOperationException(
                $"Kan inte sätta självbedömning i status {Status}. Förväntad status: Planerad eller Påbörjad.");

        SjalvBedomning = bedomning ?? throw new ArgumentNullException(nameof(bedomning));
        Status = ReviewStatus.SjalvbedomningKlar;
    }

    /// <summary>
    /// Chefen lämnar sin bedömning och övergripande betyg.
    /// </summary>
    public void SattChefsbedomning(string bedomning, int overallRating)
    {
        if (Status is not ReviewStatus.SjalvbedomningKlar)
            throw new InvalidOperationException(
                $"Kan inte sätta chefsbedömning i status {Status}. Självbedömning måste vara klar först.");

        if (overallRating < 1 || overallRating > 5)
            throw new ArgumentOutOfRangeException(
                nameof(overallRating), overallRating,
                "Betyg måste vara mellan 1 och 5.");

        ChefsBedomning = bedomning ?? throw new ArgumentNullException(nameof(bedomning));
        OverallRating = overallRating;
        Status = ReviewStatus.ChefsbedomningKlar;
    }

    /// <summary>
    /// Sätt målsättning/utvecklingsplan för kommande period.
    /// </summary>
    public void SattMalsattning(string malsattning)
    {
        Malsattning = malsattning ?? throw new ArgumentNullException(nameof(malsattning));
    }

    /// <summary>
    /// Markerar medarbetarsamtalet som genomfört.
    /// Kräver att både självbedömning och chefsbedömning är klara.
    /// </summary>
    public void Genomfor()
    {
        if (Status is not ReviewStatus.ChefsbedomningKlar)
            throw new InvalidOperationException(
                $"Kan inte genomföra samtalet i status {Status}. Både självbedömning och chefsbedömning måste vara klara.");

        Status = ReviewStatus.Genomford;
        GenomfordVid = DateTime.UtcNow;
    }
}
