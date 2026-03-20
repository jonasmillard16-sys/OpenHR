using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

/// <summary>OB-tillägg per avtalsperiod</summary>
public sealed class AgreementOBRate
{
    public Guid Id { get; private set; }
    public CollectiveAgreementId AvtalsId { get; private set; }
    public OBCategory Tidstyp { get; private set; }
    public decimal Belopp { get; private set; }
    public DateOnly GiltigFran { get; private set; }
    public DateOnly? GiltigTill { get; private set; }

    private AgreementOBRate() { } // EF Core

    internal static AgreementOBRate Skapa(
        CollectiveAgreementId avtalsId,
        OBCategory tidstyp,
        decimal belopp,
        DateOnly giltigFran,
        DateOnly? giltigTill = null)
    {
        return new AgreementOBRate
        {
            Id = Guid.NewGuid(),
            AvtalsId = avtalsId,
            Tidstyp = tidstyp,
            Belopp = belopp,
            GiltigFran = giltigFran,
            GiltigTill = giltigTill
        };
    }
}
