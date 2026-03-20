using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

/// <summary>Arbetstidsregler per kollektivavtal</summary>
public sealed class AgreementWorkingHours
{
    public Guid Id { get; private set; }
    public CollectiveAgreementId AvtalsId { get; private set; }
    public decimal NormalTimmarPerVecka { get; private set; }

    /// <summary>JSON: flexregler, t.ex. {"MaxFlexSaldo": 40, "KarnTidStart": "09:00", "KarnTidSlut": "15:00"}</summary>
    public string FlexRegler { get; private set; } = "{}";

    private AgreementWorkingHours() { } // EF Core

    internal static AgreementWorkingHours Skapa(
        CollectiveAgreementId avtalsId,
        decimal normalTimmarPerVecka,
        string flexRegler)
    {
        return new AgreementWorkingHours
        {
            Id = Guid.NewGuid(),
            AvtalsId = avtalsId,
            NormalTimmarPerVecka = normalTimmarPerVecka,
            FlexRegler = flexRegler
        };
    }
}
