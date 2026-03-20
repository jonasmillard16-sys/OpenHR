using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.VMS.Domain;

/// <summary>
/// Leverantör av inhyrd personal — aggregatrot.
/// Separerad från Employee; ingår ej i personalstyrka.
/// </summary>
public sealed class Vendor : AggregateRoot<VendorId>
{
    public string Namn { get; private set; } = string.Empty;
    public string OrgNummer { get; private set; } = string.Empty;
    public string Kontaktperson { get; private set; } = string.Empty;
    public string Epost { get; private set; } = string.Empty;
    public string Telefon { get; private set; } = string.Empty;
    public string Kategori { get; private set; } = string.Empty;
    public VendorStatus Status { get; private set; }

    private Vendor() { } // EF Core

    public static Vendor Skapa(
        string namn,
        string orgNummer,
        string kontaktperson,
        string epost,
        string telefon,
        string kategori)
    {
        return new Vendor
        {
            Id = VendorId.New(),
            Namn = namn,
            OrgNummer = orgNummer,
            Kontaktperson = kontaktperson,
            Epost = epost,
            Telefon = telefon,
            Kategori = kategori,
            Status = VendorStatus.Active
        };
    }

    public void Blockera()
    {
        if (Status == VendorStatus.Blocked)
            throw new InvalidOperationException("Leverantören är redan blockerad.");
        Status = VendorStatus.Blocked;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Aktivera()
    {
        if (Status == VendorStatus.Active)
            throw new InvalidOperationException("Leverantören är redan aktiv.");
        Status = VendorStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Inaktivera()
    {
        if (Status == VendorStatus.Inactive)
            throw new InvalidOperationException("Leverantören är redan inaktiv.");
        Status = VendorStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }
}
