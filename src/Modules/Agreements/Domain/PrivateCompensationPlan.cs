using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

/// <summary>Privat ersättningsplan (bonus, provision, aktier, tjänstebil)</summary>
public sealed class PrivateCompensationPlan
{
    public Guid Id { get; private set; }
    public CollectiveAgreementId AvtalsId { get; private set; }

    /// <summary>JSON: bonusvillkor, t.ex. {"MaxProcent": 15, "UtbetalningsMånad": 3}</summary>
    public string Bonus { get; private set; } = "{}";

    /// <summary>JSON: provisionsvillkor</summary>
    public string Provision { get; private set; } = "{}";

    /// <summary>JSON: aktieprogram</summary>
    public string Aktier { get; private set; } = "{}";

    /// <summary>JSON: tjänstebilsförmån</summary>
    public string Tjanstebil { get; private set; } = "{}";

    private PrivateCompensationPlan() { } // EF Core

    internal static PrivateCompensationPlan Skapa(
        CollectiveAgreementId avtalsId,
        string bonus,
        string provision,
        string aktier,
        string tjanstebil)
    {
        return new PrivateCompensationPlan
        {
            Id = Guid.NewGuid(),
            AvtalsId = avtalsId,
            Bonus = bonus,
            Provision = provision,
            Aktier = aktier,
            Tjanstebil = tjanstebil
        };
    }
}
