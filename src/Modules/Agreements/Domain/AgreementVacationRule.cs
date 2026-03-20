using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

/// <summary>Semesterregler per kollektivavtal</summary>
public sealed class AgreementVacationRule
{
    public Guid Id { get; private set; }
    public CollectiveAgreementId AvtalsId { get; private set; }
    public int BasDagar { get; private set; }
    public int ExtraDagarVid40 { get; private set; }
    public int ExtraDagarVid50 { get; private set; }

    private AgreementVacationRule() { } // EF Core

    internal static AgreementVacationRule Skapa(
        CollectiveAgreementId avtalsId,
        int basDagar,
        int extraDagarVid40,
        int extraDagarVid50)
    {
        return new AgreementVacationRule
        {
            Id = Guid.NewGuid(),
            AvtalsId = avtalsId,
            BasDagar = basDagar,
            ExtraDagarVid40 = extraDagarVid40,
            ExtraDagarVid50 = extraDagarVid50
        };
    }
}
