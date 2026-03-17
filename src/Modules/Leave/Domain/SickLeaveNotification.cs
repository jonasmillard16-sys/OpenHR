namespace RegionHR.Leave.Domain;

/// <summary>
/// Sjukfrnvaroanmlan med svensk sjukskrivningsuppfljning.
/// Lkarintyg krvs frn dag 8, Frskringskassan ska anmlas frn dag 15.
/// </summary>
public sealed class SickLeaveNotification
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public DateOnly StartDatum { get; private set; }
    public DateOnly? SlutDatum { get; private set; }
    public int SjukDag { get; private set; }

    /// <summary>
    /// True om sjukdag >= 8: lkarintyg krvs.
    /// </summary>
    public bool LakarintygKravs { get; private set; }

    /// <summary>
    /// True om sjukdag >= 15: anmlan till Frskringskassan krvs.
    /// </summary>
    public bool FKAnmalanKravs { get; private set; }

    public bool LakarintygInlamnat { get; private set; }
    public bool FKAnmalanGjord { get; private set; }

    private SickLeaveNotification() { } // EF Core

    /// <summary>
    /// Skapar en ny sjukanmlan med dag 1.
    /// </summary>
    public static SickLeaveNotification Skapa(Guid anstallId, DateOnly start)
    {
        return new SickLeaveNotification
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            StartDatum = start,
            SlutDatum = null,
            SjukDag = 1,
            LakarintygKravs = false,
            FKAnmalanKravs = false,
            LakarintygInlamnat = false,
            FKAnmalanGjord = false
        };
    }

    /// <summary>
    /// Uppdaterar sjukdagnummer och stter flaggor fr lkarintyg och FK-anmlan.
    /// </summary>
    public void UppdateraDag(int dagNr)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(dagNr, 1);

        SjukDag = dagNr;
        LakarintygKravs = dagNr >= 8;
        FKAnmalanKravs = dagNr >= 15;
    }
}
