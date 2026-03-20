using RegionHR.SharedKernel.Domain;

namespace RegionHR.VMS.Domain;

/// <summary>
/// Inhyrd personal — INTE anställd, ingår ej i personalstyrka/headcount.
/// Separat entitet från Employee.
/// </summary>
public sealed class ContingentWorker
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = string.Empty;
    public VendorId VendorId { get; private set; }
    public StaffingRequestId StaffingRequestId { get; private set; }
    public DateOnly Tilltradesdatum { get; private set; }
    public DateOnly? Slutdatum { get; private set; }
    public decimal TimKostnad { get; private set; }
    public OrganizationId EnhetId { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private ContingentWorker() { } // EF Core

    public static ContingentWorker Skapa(
        string namn,
        VendorId vendorId,
        StaffingRequestId staffingRequestId,
        DateOnly tilltradesdatum,
        DateOnly? slutdatum,
        decimal timKostnad,
        OrganizationId enhetId)
    {
        return new ContingentWorker
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            VendorId = vendorId,
            StaffingRequestId = staffingRequestId,
            Tilltradesdatum = tilltradesdatum,
            Slutdatum = slutdatum,
            TimKostnad = timKostnad,
            EnhetId = enhetId
        };
    }
}
