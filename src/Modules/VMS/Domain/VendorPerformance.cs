using RegionHR.SharedKernel.Domain;

namespace RegionHR.VMS.Domain;

/// <summary>
/// Manuell leverantörsbedömning — poäng 1-5 per period (spec 15.14).
/// </summary>
public sealed class VendorPerformance
{
    public Guid Id { get; private set; }
    public VendorId VendorId { get; private set; }
    public string Period { get; private set; } = string.Empty;
    public int Poang { get; private set; }
    public string Kommentar { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private VendorPerformance() { } // EF Core

    public static VendorPerformance Skapa(
        VendorId vendorId,
        string period,
        int poang,
        string kommentar)
    {
        if (poang < 1 || poang > 5)
            throw new ArgumentOutOfRangeException(nameof(poang), "Poäng måste vara 1-5.");

        return new VendorPerformance
        {
            Id = Guid.NewGuid(),
            VendorId = vendorId,
            Period = period,
            Poang = poang,
            Kommentar = kommentar
        };
    }
}
