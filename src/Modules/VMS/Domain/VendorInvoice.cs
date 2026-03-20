using RegionHR.SharedKernel.Domain;

namespace RegionHR.VMS.Domain;

/// <summary>
/// Leverantörsfaktura — matchas mot tidrapporter, godkänns och betalas.
/// </summary>
public sealed class VendorInvoice
{
    public Guid Id { get; private set; }
    public VendorId VendorId { get; private set; }
    public string Period { get; private set; } = string.Empty;
    public decimal Belopp { get; private set; }
    public bool MatchadMotTidrapporter { get; private set; }
    public decimal? Differens { get; private set; }
    public VendorInvoiceStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private VendorInvoice() { } // EF Core

    public static VendorInvoice Skapa(
        VendorId vendorId,
        string period,
        decimal belopp)
    {
        return new VendorInvoice
        {
            Id = Guid.NewGuid(),
            VendorId = vendorId,
            Period = period,
            Belopp = belopp,
            MatchadMotTidrapporter = false,
            Status = VendorInvoiceStatus.Received
        };
    }

    public void Matcha(decimal beraknatBelopp)
    {
        MatchadMotTidrapporter = true;
        Differens = Belopp - beraknatBelopp;
        Status = VendorInvoiceStatus.Matched;
    }

    public void Godkann()
    {
        if (Status != VendorInvoiceStatus.Matched)
            throw new InvalidOperationException("Kan bara godkänna matchade fakturor.");
        Status = VendorInvoiceStatus.Approved;
    }

    public void MarkeraBetald()
    {
        if (Status != VendorInvoiceStatus.Approved)
            throw new InvalidOperationException("Kan bara markera godkända fakturor som betalda.");
        Status = VendorInvoiceStatus.Paid;
    }
}
