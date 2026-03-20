using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

/// <summary>Uppsägningstider per anställningstid</summary>
public sealed class AgreementNoticePeriod
{
    public Guid Id { get; private set; }
    public CollectiveAgreementId AvtalsId { get; private set; }
    public int AnstallningstidManader { get; private set; }
    public int UppságningstidManader { get; private set; }

    private AgreementNoticePeriod() { } // EF Core

    internal static AgreementNoticePeriod Skapa(
        CollectiveAgreementId avtalsId,
        int anstallningstidManader,
        int uppsagningstidManader)
    {
        return new AgreementNoticePeriod
        {
            Id = Guid.NewGuid(),
            AvtalsId = avtalsId,
            AnstallningstidManader = anstallningstidManader,
            UppságningstidManader = uppsagningstidManader
        };
    }
}
