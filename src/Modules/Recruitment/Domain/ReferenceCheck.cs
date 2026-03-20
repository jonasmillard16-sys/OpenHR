namespace RegionHR.Recruitment.Domain;

public enum ReferenceCheckStatus { Begard, Genomford, Godkand, Avvikelse }

public sealed class ReferenceCheck
{
    public Guid Id { get; private set; }
    public Guid VacancyId { get; private set; }
    public string KandidatNamn { get; private set; } = default!;
    public string ReferensNamn { get; private set; } = default!;
    public string ReferensRelation { get; private set; } = default!;
    public ReferenceCheckStatus Status { get; private set; }
    public string? Kommentar { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private ReferenceCheck() { }

    public static ReferenceCheck Skapa(Guid vacancyId, string kandidat, string referens, string relation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kandidat);
        ArgumentException.ThrowIfNullOrWhiteSpace(referens);
        return new ReferenceCheck { Id = Guid.NewGuid(), VacancyId = vacancyId, KandidatNamn = kandidat, ReferensNamn = referens, ReferensRelation = relation, Status = ReferenceCheckStatus.Begard, SkapadVid = DateTime.UtcNow };
    }

    public void Genomfor(string kommentar, bool godkand)
    {
        Status = godkand ? ReferenceCheckStatus.Godkand : ReferenceCheckStatus.Avvikelse;
        Kommentar = kommentar;
    }
}
