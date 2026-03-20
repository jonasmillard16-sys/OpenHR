using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

/// <summary>Övertidsregler per kollektivavtal</summary>
public sealed class AgreementOvertimeRule
{
    public Guid Id { get; private set; }
    public CollectiveAgreementId AvtalsId { get; private set; }
    public decimal Troskel { get; private set; }
    public decimal Multiplikator { get; private set; }
    public decimal MaxPerAr { get; private set; }

    private AgreementOvertimeRule() { } // EF Core

    internal static AgreementOvertimeRule Skapa(
        CollectiveAgreementId avtalsId,
        decimal troskel,
        decimal multiplikator,
        decimal maxPerAr)
    {
        return new AgreementOvertimeRule
        {
            Id = Guid.NewGuid(),
            AvtalsId = avtalsId,
            Troskel = troskel,
            Multiplikator = multiplikator,
            MaxPerAr = maxPerAr
        };
    }
}
