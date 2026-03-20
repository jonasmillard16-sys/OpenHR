using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.VMS.Domain;

/// <summary>
/// Ramavtal med leverantör — giltighetsperiod, villkor, förlängning.
/// </summary>
public sealed class FrameworkAgreement : Entity<FrameworkAgreementId>
{
    public VendorId VendorId { get; private set; }
    public DateOnly GiltigFran { get; private set; }
    public DateOnly GiltigTill { get; private set; }
    public string Avtalsvillkor { get; private set; } = string.Empty;
    public int UppságningstidManader { get; private set; }
    public string ForlangningsKlausul { get; private set; } = string.Empty;
    public decimal? Avtalsvarde { get; private set; }

    private readonly List<RateCard> _rateCards = [];
    public IReadOnlyList<RateCard> RateCards => _rateCards.AsReadOnly();

    private FrameworkAgreement() { } // EF Core

    public static FrameworkAgreement Skapa(
        VendorId vendorId,
        DateOnly giltigFran,
        DateOnly giltigTill,
        string avtalsvillkor,
        int uppságningstidManader,
        string forlangningsKlausul,
        decimal? avtalsvarde = null)
    {
        return new FrameworkAgreement
        {
            Id = FrameworkAgreementId.New(),
            VendorId = vendorId,
            GiltigFran = giltigFran,
            GiltigTill = giltigTill,
            Avtalsvillkor = avtalsvillkor,
            UppságningstidManader = uppságningstidManader,
            ForlangningsKlausul = forlangningsKlausul,
            Avtalsvarde = avtalsvarde
        };
    }

    public RateCard LaggTillRateCard(string yrkesKategori, decimal timPris, decimal obPaslag, decimal overtidPaslag, decimal moms)
    {
        var rc = RateCard.Skapa(Id, yrkesKategori, timPris, obPaslag, overtidPaslag, moms);
        _rateCards.Add(rc);
        return rc;
    }
}
