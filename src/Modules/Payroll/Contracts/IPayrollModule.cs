using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Contracts;

public interface IPayrollModule
{
    Task<PayrollRunSummary> StartPayrollRunAsync(int year, int month, string startedBy, CancellationToken ct = default);
    Task<PayrollRunSummary?> GetPayrollRunAsync(PayrollRunId runId, CancellationToken ct = default);
    Task<PayrollResultSummary?> GetEmployeePayrollResultAsync(EmployeeId employeeId, int year, int month, CancellationToken ct = default);
}

public record PayrollRunSummary(
    PayrollRunId Id,
    string Period,
    PayrollRunStatus Status,
    int AntalAnstallda,
    decimal TotalBrutto,
    decimal TotalNetto,
    decimal TotalSkatt,
    decimal TotalArbetsgivaravgifter);

public record PayrollResultSummary(
    EmployeeId AnstallId,
    string Period,
    decimal Brutto,
    decimal Skatt,
    decimal Netto,
    decimal Arbetsgivaravgifter,
    IReadOnlyList<PayrollLineSummary> Rader);

public record PayrollLineSummary(
    string LoneartKod,
    string Benamning,
    decimal Antal,
    decimal Sats,
    decimal Belopp);
