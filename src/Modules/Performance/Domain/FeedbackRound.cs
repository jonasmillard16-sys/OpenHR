namespace RegionHR.Performance.Domain;

public enum FeedbackRoundStatus { Utkast, Oppnad, Stangd }

public sealed class FeedbackRound
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public string Titel { get; private set; } = default!;
    public FeedbackRoundStatus Status { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public DateTime? OppnadVid { get; private set; }
    public DateTime? StangdVid { get; private set; }

    private FeedbackRound() { }

    public static FeedbackRound Skapa(Guid anstallId, string titel)
    {
        if (anstallId == Guid.Empty) throw new ArgumentException("AnstallId krävs.", nameof(anstallId));
        ArgumentException.ThrowIfNullOrWhiteSpace(titel);
        return new FeedbackRound { Id = Guid.NewGuid(), AnstallId = anstallId, Titel = titel, Status = FeedbackRoundStatus.Utkast, SkapadVid = DateTime.UtcNow };
    }

    public void Oppna() { if (Status != FeedbackRoundStatus.Utkast) throw new InvalidOperationException("Kan bara öppna utkast."); Status = FeedbackRoundStatus.Oppnad; OppnadVid = DateTime.UtcNow; }
    public void Stang() { if (Status != FeedbackRoundStatus.Oppnad) throw new InvalidOperationException("Kan bara stänga öppnad."); Status = FeedbackRoundStatus.Stangd; StangdVid = DateTime.UtcNow; }
}

public sealed class FeedbackResponse
{
    public Guid Id { get; private set; }
    public Guid RoundId { get; private set; }
    public Guid BedomareId { get; private set; }
    public string Relation { get; private set; } = default!; // Chef, Kollega, Underordnad, Extern
    public int Betyg { get; private set; } // 1-5
    public string? Kommentar { get; private set; }
    public DateTime SvaradVid { get; private set; }

    private FeedbackResponse() { }

    public static FeedbackResponse Skapa(Guid roundId, Guid bedomareId, string relation, int betyg, string? kommentar = null)
    {
        if (roundId == Guid.Empty) throw new ArgumentException("RoundId krävs.", nameof(roundId));
        if (bedomareId == Guid.Empty) throw new ArgumentException("BedomareId krävs.", nameof(bedomareId));
        if (betyg < 1 || betyg > 5) throw new ArgumentOutOfRangeException(nameof(betyg), "1-5.");
        return new FeedbackResponse { Id = Guid.NewGuid(), RoundId = roundId, BedomareId = bedomareId, Relation = relation, Betyg = betyg, Kommentar = kommentar, SvaradVid = DateTime.UtcNow };
    }
}
