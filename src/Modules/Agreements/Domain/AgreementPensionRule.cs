using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

/// <summary>Pensionsregler per kollektivavtal</summary>
public sealed class AgreementPensionRule
{
    public Guid Id { get; private set; }
    public CollectiveAgreementId AvtalsId { get; private set; }
    public PensionType PensionsTyp { get; private set; }
    public decimal SatsUnderTak { get; private set; }
    public decimal SatsOverTak { get; private set; }
    public decimal Tak { get; private set; }

    /// <summary>JSON: beräkningsmodell med detaljer, t.ex. {"IBB": 599500, "TakMultipel": 7.5}</summary>
    public string BerakningsModell { get; private set; } = "{}";

    private AgreementPensionRule() { } // EF Core

    internal static AgreementPensionRule Skapa(
        CollectiveAgreementId avtalsId,
        PensionType pensionsTyp,
        decimal satsUnderTak,
        decimal satsOverTak,
        decimal tak,
        string berakningsModell)
    {
        return new AgreementPensionRule
        {
            Id = Guid.NewGuid(),
            AvtalsId = avtalsId,
            PensionsTyp = pensionsTyp,
            SatsUnderTak = satsUnderTak,
            SatsOverTak = satsOverTak,
            Tak = tak,
            BerakningsModell = berakningsModell
        };
    }
}
