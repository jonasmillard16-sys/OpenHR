using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

/// <summary>Försäkringspaket per kollektivavtal (TGL, AGS, TFA, AFA, PSA)</summary>
public sealed class AgreementInsurancePackage
{
    public Guid Id { get; private set; }
    public CollectiveAgreementId AvtalsId { get; private set; }

    /// <summary>JSON: Tjänstegrupplivförsäkring, t.ex. {"Belopp": 285000, "Forman": true}</summary>
    public string TGL { get; private set; } = "{}";

    /// <summary>JSON: Avtalsgruppsjukförsäkring, t.ex. {"DagErsattning": 810, "MaxDagar": 360}</summary>
    public string AGS { get; private set; } = "{}";

    /// <summary>JSON: Trygghetsförsäkring vid arbetsskada</summary>
    public string TFA { get; private set; } = "{}";

    /// <summary>JSON: Avtalsförsäkring AFA</summary>
    public string AFA { get; private set; } = "{}";

    /// <summary>JSON: Premiebestämd sjukförsäkring</summary>
    public string PSA { get; private set; } = "{}";

    private AgreementInsurancePackage() { } // EF Core

    internal static AgreementInsurancePackage Skapa(
        CollectiveAgreementId avtalsId,
        string tgl,
        string ags,
        string tfa,
        string afa,
        string psa)
    {
        return new AgreementInsurancePackage
        {
            Id = Guid.NewGuid(),
            AvtalsId = avtalsId,
            TGL = tgl,
            AGS = ags,
            TFA = tfa,
            AFA = afa,
            PSA = psa
        };
    }
}
