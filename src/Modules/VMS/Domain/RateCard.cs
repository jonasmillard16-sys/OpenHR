using RegionHR.SharedKernel.Domain;

namespace RegionHR.VMS.Domain;

/// <summary>
/// Priskort per yrkeskategori under ett ramavtal.
/// </summary>
public sealed class RateCard
{
    public Guid Id { get; private set; }
    public FrameworkAgreementId FrameworkAgreementId { get; private set; }
    public string YrkesKategori { get; private set; } = string.Empty;
    public decimal TimPris { get; private set; }
    public decimal OBPaslag { get; private set; }
    public decimal OvertidPaslag { get; private set; }
    public decimal Moms { get; private set; }

    private RateCard() { } // EF Core

    internal static RateCard Skapa(
        FrameworkAgreementId frameworkAgreementId,
        string yrkesKategori,
        decimal timPris,
        decimal obPaslag,
        decimal overtidPaslag,
        decimal moms)
    {
        return new RateCard
        {
            Id = Guid.NewGuid(),
            FrameworkAgreementId = frameworkAgreementId,
            YrkesKategori = yrkesKategori,
            TimPris = timPris,
            OBPaslag = obPaslag,
            OvertidPaslag = overtidPaslag,
            Moms = moms
        };
    }
}
