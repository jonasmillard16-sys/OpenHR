namespace RegionHR.Leave.Domain;

/// <summary>
/// Semestersaldo per anstlld och r.
/// Tilldelning baseras p lder enligt kollektivavtal AB:
///   Under 40 r: 25 dagar
///   40-49 r: 31 dagar
///   50 r och ver: 32 dagar
/// Sparade dagar: max 5 rs tilldelning enligt semesterlagen.
/// </summary>
public sealed class VacationBalance
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public int Ar { get; private set; }
    public int IntjanadeDagar { get; private set; }
    public int UttagnaDagar { get; private set; }
    public int SparadeDagar { get; private set; }
    public int Tilldelning { get; private set; }

    /// <summary>
    /// Tillgngliga semesterdagar = tilldelning + sparade - uttagna.
    /// </summary>
    public int TillgangligaDagar => Tilldelning + SparadeDagar - UttagnaDagar;

    private VacationBalance() { } // EF Core

    /// <summary>
    /// Skapar ett nytt semestersaldo fr ett specifikt r.
    /// Tilldelning berknas utifrn lder enligt AB-avtal.
    /// </summary>
    public static VacationBalance SkapaForAr(Guid anstallId, int ar, int alder)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(alder);

        var tilldelning = alder switch
        {
            < 40 => 25,
            < 50 => 31,
            _ => 32
        };

        return new VacationBalance
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Ar = ar,
            IntjanadeDagar = 0,
            UttagnaDagar = 0,
            SparadeDagar = 0,
            Tilldelning = tilldelning
        };
    }

    /// <summary>
    /// Registrerar uttag av semesterdagar.
    /// Kastar undantag om det inte finns tillrckligt med tillgngliga dagar.
    /// </summary>
    public void RegistreraUttag(int dagar)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dagar);

        if (dagar > TillgangligaDagar)
            throw new InvalidOperationException(
                $"Otillrckligt antal semesterdagar. Tillgngliga: {TillgangligaDagar}, begrt uttag: {dagar}");

        UttagnaDagar += dagar;
    }

    /// <summary>
    /// Sparar semesterdagar till kommande r.
    /// Max sparade dagar r 5 gnger rlig tilldelning enligt semesterlagen.
    /// </summary>
    public void SparaDagar(int dagar)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dagar);

        var maxSparade = 5 * Tilldelning;
        if (SparadeDagar + dagar > maxSparade)
            throw new InvalidOperationException(
                $"Kan inte spara {dagar} dagar. Max sparade dagar: {maxSparade}, redan sparade: {SparadeDagar}");

        SparadeDagar += dagar;
    }
}
