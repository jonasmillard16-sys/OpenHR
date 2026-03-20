namespace RegionHR.Compensation.Domain;

/// <summary>
/// Loneband per befattningskategori. Definierar min/mal/max lon, samt stegbaserade band (1-4).
/// </summary>
public sealed class CompensationBand
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Befattningskategori { get; set; } = string.Empty;
    public decimal Min { get; set; }
    public decimal Mal { get; set; }
    public decimal Max { get; set; }

    // Steg-baserade band (frivilliga)
    public decimal? Steg1Min { get; set; }
    public decimal? Steg1Max { get; set; }
    public decimal? Steg2Min { get; set; }
    public decimal? Steg2Max { get; set; }
    public decimal? Steg3Min { get; set; }
    public decimal? Steg3Max { get; set; }
    public decimal? Steg4Min { get; set; }
    public decimal? Steg4Max { get; set; }

    /// <summary>Kontrollerar om en given lon befinner sig inom bandet.</summary>
    public bool ArInomBand(decimal lon) => lon >= Min && lon <= Max;

    /// <summary>Beraknar hur langt procentuellt genom bandet en given lon befinner sig.</summary>
    public decimal BandPosition(decimal lon)
    {
        if (Max == Min) return 100m;
        return Math.Clamp((lon - Min) / (Max - Min) * 100m, 0m, 100m);
    }
}
