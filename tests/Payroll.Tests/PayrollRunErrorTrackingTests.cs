using Xunit;
using RegionHR.Payroll.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Tests;

/// <summary>
/// Enhetstester för felspårning i PayrollRun.
/// Verifierar att beräkningsfel samlas in korrekt i stället för att tyst ignoreras.
/// </summary>
public class PayrollRunErrorTrackingTests
{
    [Fact]
    public void PayrollRun_LaggTillFel_TracksErrors()
    {
        var run = PayrollRun.Skapa(2026, 3, "Test");
        var empId = EmployeeId.From(Guid.NewGuid());
        run.LaggTillFel(empId, "Skattetabell saknas");

        Assert.True(run.HarFel);
        Assert.Single(run.BerakningsFel);
        Assert.Contains("Skattetabell saknas", run.BerakningsFel[0]);
    }
}
