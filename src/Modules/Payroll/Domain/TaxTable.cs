namespace RegionHR.Payroll.Domain;

using RegionHR.SharedKernel.Domain;

/// <summary>
/// Svensk skattetabell. Innehåller tabellnr (30-36+), kolumn (1-6),
/// inkomstintervall och skattebelopp per månad.
/// Laddas från Skatteverkets årliga publikation.
/// </summary>
public sealed class TaxTable
{
    public int Id { get; set; }
    public int Ar { get; set; }
    public int Tabellnummer { get; set; }   // 30-36 vanligast
    public int Kolumn { get; set; }         // 1-6

    private readonly List<TaxTableRow> _rader = [];
    public IReadOnlyList<TaxTableRow> Rader => _rader.AsReadOnly();

    public void LaggTillRad(TaxTableRow rad) => _rader.Add(rad);

    /// <summary>
    /// Slå upp skatt för given månadsinkomst.
    /// Returnerar skattebelopp i kronor.
    /// </summary>
    public Money BeraknaManadenSkatt(Money skattepliktigManadslon)
    {
        var inkomst = skattepliktigManadslon.Amount;

        // Hitta rätt intervall
        var rad = _rader
            .OrderBy(r => r.InkomstFran)
            .LastOrDefault(r => inkomst >= r.InkomstFran);

        if (rad is null)
            return Money.Zero;

        return Money.SEK(rad.Skattebelopp);
    }
}

public sealed class TaxTableRow
{
    public int Id { get; set; }
    public decimal InkomstFran { get; set; }    // Inkomstintervall från
    public decimal InkomstTill { get; set; }    // Inkomstintervall till
    public decimal Skattebelopp { get; set; }   // Skatt i kronor
}

/// <summary>
/// Tjänst för att ladda och cacha skattetabeller.
/// </summary>
public interface ITaxTableProvider
{
    Task<TaxTable?> GetTableAsync(int year, int tableNumber, int column, CancellationToken ct = default);
    Task<IReadOnlyList<TaxTable>> GetAllTablesForYearAsync(int year, CancellationToken ct = default);
}
