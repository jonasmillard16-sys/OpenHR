using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// En körning av schemaoptimeraren. Sparar parametrar, resultat och kostnadsmått.
/// </summary>
public sealed class SchedulingRun : AggregateRoot<SchedulingRunId>
{
    public OrganizationId EnhetId { get; private set; }
    public DateOnly PeriodFran { get; private set; }
    public DateOnly PeriodTill { get; private set; }

    /// <summary>JSON-parametrar som användes vid körningen.</summary>
    public string Parametrar { get; private set; } = "{}";

    public int GenereradePass { get; private set; }
    public decimal TotalOBKostnad { get; private set; }
    public decimal TotalOvertidKostnad { get; private set; }
    public bool ATLKompliant { get; private set; }

    /// <summary>Running / Complete / Failed</summary>
    public string Status { get; private set; } = "Running";

    public DateTime SkapadVid { get; private set; }

    private SchedulingRun() { }

    public static SchedulingRun Starta(
        OrganizationId enhetId,
        DateOnly periodFran,
        DateOnly periodTill,
        string parametrar)
    {
        return new SchedulingRun
        {
            Id = SchedulingRunId.New(),
            EnhetId = enhetId,
            PeriodFran = periodFran,
            PeriodTill = periodTill,
            Parametrar = parametrar,
            Status = "Running",
            SkapadVid = DateTime.UtcNow
        };
    }

    public void Slutfor(int genereradePass, decimal obKostnad, decimal overtidKostnad, bool atlKompliant)
    {
        GenereradePass = genereradePass;
        TotalOBKostnad = obKostnad;
        TotalOvertidKostnad = overtidKostnad;
        ATLKompliant = atlKompliant;
        Status = "Complete";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed()
    {
        Status = "Failed";
        UpdatedAt = DateTime.UtcNow;
    }
}
