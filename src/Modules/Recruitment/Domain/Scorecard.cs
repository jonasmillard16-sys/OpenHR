namespace RegionHR.Recruitment.Domain;

public class Scorecard
{
    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public Guid BedomareId { get; private set; }
    public int KompetensPoang { get; private set; } // 1-5
    public int ErfarenhetsPoang { get; private set; } // 1-5
    public int PersonlighetPoang { get; private set; } // 1-5
    public int MotivationPoang { get; private set; } // 1-5
    public decimal TotalPoang => (KompetensPoang + ErfarenhetsPoang + PersonlighetPoang + MotivationPoang) / 4.0m;
    public string? Kommentar { get; private set; }
    public string? Rekommendation { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private Scorecard() { }

    public static Scorecard Skapa(Guid applicationId, Guid bedomareId, int kompetens, int erfarenhet, int personlighet, int motivation, string? kommentar = null, string? rekommendation = null)
    {
        if (kompetens < 1 || kompetens > 5) throw new ArgumentOutOfRangeException(nameof(kompetens));
        if (erfarenhet < 1 || erfarenhet > 5) throw new ArgumentOutOfRangeException(nameof(erfarenhet));
        if (personlighet < 1 || personlighet > 5) throw new ArgumentOutOfRangeException(nameof(personlighet));
        if (motivation < 1 || motivation > 5) throw new ArgumentOutOfRangeException(nameof(motivation));
        return new Scorecard
        {
            Id = Guid.NewGuid(), ApplicationId = applicationId, BedomareId = bedomareId,
            KompetensPoang = kompetens, ErfarenhetsPoang = erfarenhet,
            PersonlighetPoang = personlighet, MotivationPoang = motivation,
            Kommentar = kommentar, Rekommendation = rekommendation, SkapadVid = DateTime.UtcNow
        };
    }
}
