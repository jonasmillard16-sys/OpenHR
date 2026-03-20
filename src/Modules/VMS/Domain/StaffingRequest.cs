using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.VMS.Domain;

/// <summary>
/// Beställning av inhyrd personal — aggregatrot med statusflöde.
/// </summary>
public sealed class StaffingRequest : AggregateRoot<StaffingRequestId>
{
    public OrganizationId EnhetId { get; private set; }
    public string Befattning { get; private set; } = string.Empty;
    public DateOnly PeriodFran { get; private set; }
    public DateOnly PeriodTill { get; private set; }
    public int AntalPersoner { get; private set; }
    public string Kravprofil { get; private set; } = string.Empty;
    public StaffingRequestStatus Status { get; private set; }

    private StaffingRequest() { } // EF Core

    public static StaffingRequest Skapa(
        OrganizationId enhetId,
        string befattning,
        DateOnly periodFran,
        DateOnly periodTill,
        int antalPersoner,
        string kravprofil)
    {
        return new StaffingRequest
        {
            Id = StaffingRequestId.New(),
            EnhetId = enhetId,
            Befattning = befattning,
            PeriodFran = periodFran,
            PeriodTill = periodTill,
            AntalPersoner = antalPersoner,
            Kravprofil = kravprofil,
            Status = StaffingRequestStatus.Draft
        };
    }

    public void SkickaIn()
    {
        if (Status != StaffingRequestStatus.Draft)
            throw new InvalidOperationException("Kan bara skicka in utkast.");
        Status = StaffingRequestStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Godkann()
    {
        if (Status != StaffingRequestStatus.Submitted)
            throw new InvalidOperationException("Kan bara godkänna inskickade beställningar.");
        Status = StaffingRequestStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Tillsatt()
    {
        if (Status != StaffingRequestStatus.Approved)
            throw new InvalidOperationException("Kan bara markera godkända beställningar som tillsatta.");
        Status = StaffingRequestStatus.Filled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Stang()
    {
        if (Status == StaffingRequestStatus.Closed)
            throw new InvalidOperationException("Beställningen är redan stängd.");
        Status = StaffingRequestStatus.Closed;
        UpdatedAt = DateTime.UtcNow;
    }
}
