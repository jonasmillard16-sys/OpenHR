using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

/// <summary>Viloregler per kollektivavtal (dygnsvila, veckovila, rast)</summary>
public sealed class AgreementRestRule
{
    public Guid Id { get; private set; }
    public CollectiveAgreementId AvtalsId { get; private set; }
    public decimal MinDygnsvila { get; private set; }
    public decimal MinVeckovila { get; private set; }
    public decimal RastPerPass { get; private set; }

    private AgreementRestRule() { } // EF Core

    internal static AgreementRestRule Skapa(
        CollectiveAgreementId avtalsId,
        decimal minDygnsvila,
        decimal minVeckovila,
        decimal rastPerPass)
    {
        return new AgreementRestRule
        {
            Id = Guid.NewGuid(),
            AvtalsId = avtalsId,
            MinDygnsvila = minDygnsvila,
            MinVeckovila = minVeckovila,
            RastPerPass = rastPerPass
        };
    }
}
