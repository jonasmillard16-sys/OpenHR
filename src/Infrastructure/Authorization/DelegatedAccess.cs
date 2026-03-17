namespace RegionHR.Infrastructure.Authorization;

public class DelegatedAccess
{
    public Guid Id { get; private set; }
    public Guid DelegatorId { get; private set; }
    public Guid DelegatId { get; private set; }
    public string Roll { get; private set; } = ""; // delegated role
    public DateOnly FranDatum { get; private set; }
    public DateOnly TillDatum { get; private set; }
    public string? Anledning { get; private set; }
    public bool ArAktiv { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private DelegatedAccess() { }

    public static DelegatedAccess Skapa(Guid delegatorId, Guid delegatId, string roll, DateOnly fran, DateOnly till, string? anledning = null)
    {
        return new DelegatedAccess
        {
            Id = Guid.NewGuid(), DelegatorId = delegatorId, DelegatId = delegatId,
            Roll = roll, FranDatum = fran, TillDatum = till,
            Anledning = anledning, ArAktiv = true, SkapadVid = DateTime.UtcNow
        };
    }

    public void Avsluta() { ArAktiv = false; }
    public bool ArGiltig(DateOnly datum) => ArAktiv && datum >= FranDatum && datum <= TillDatum;
}
