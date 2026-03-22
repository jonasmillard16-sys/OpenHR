using RegionHR.Core.Contracts;
using RegionHR.Payroll.Domain;
using RegionHR.Payroll.Engine;
using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Services;

/// <summary>
/// Orkestrerar en fullständig lönekörning.
/// Hämtar alla aktiva anställda, beräknar lön per anställd,
/// sammanställer resultat och markerar körningen som klar.
/// </summary>
public sealed class PayrollBatchService
{
    private readonly PayrollCalculationEngine _calculationEngine;
    private readonly RetroactiveRecalculationEngine _retroEngine;
    private readonly IRepository<PayrollRun, PayrollRunId> _payrollRunRepo;
    private readonly ICoreHRModule _coreHR;
    private readonly IUnitOfWork _unitOfWork;

    public PayrollBatchService(
        PayrollCalculationEngine calculationEngine,
        RetroactiveRecalculationEngine retroEngine,
        IRepository<PayrollRun, PayrollRunId> payrollRunRepo,
        ICoreHRModule coreHR,
        IUnitOfWork unitOfWork)
    {
        _calculationEngine = calculationEngine ?? throw new ArgumentNullException(nameof(calculationEngine));
        _retroEngine = retroEngine ?? throw new ArgumentNullException(nameof(retroEngine));
        _payrollRunRepo = payrollRunRepo ?? throw new ArgumentNullException(nameof(payrollRunRepo));
        _coreHR = coreHR ?? throw new ArgumentNullException(nameof(coreHR));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Kör en ordinarie lönekörning för given period.
    /// </summary>
    /// <param name="year">Lönekörningsår.</param>
    /// <param name="month">Löneköringsmånad.</param>
    /// <param name="startadAv">Användare som startade körningen.</param>
    /// <param name="ct">Avbrytningstoken.</param>
    /// <returns>Den färdiga lönekörningen med alla resultat.</returns>
    public async Task<PayrollRun> ExecutePayrollRunAsync(
        int year, int month, string startadAv, CancellationToken ct = default)
    {
        // 1. Skapa lönekörning
        var run = PayrollRun.Skapa(year, month, startadAv);
        run.Paborja();
        await _payrollRunRepo.AddAsync(run, ct);

        // 2. Hämta alla aktiva anställda (via alla organisationsenheter)
        var periodStart = new DateOnly(year, month, 1);
        var employees = await GetAllActiveEmployeesAsync(periodStart, ct);

        // 3-5. Beräkna lön per anställd
        foreach (var employee in employees)
        {
            var employment = await _coreHR.GetActiveEmploymentAsync(employee.Id, periodStart, ct);
            if (employment is null)
                continue;

            var input = await BuildPayrollInputAsync(employee, employment, year, month, ct);

            try
            {
                var result = await _calculationEngine.CalculateAsync(
                    run.Id, employee.Id, employment.Id, year, month, input, ct);

                // 5. Lägg till resultat
                run.LaggTillResultat(result);
            }
            catch (Exception ex)
            {
                run.LaggTillFel(employee.Id, ex.Message);
            }
        }

        // 6. Markera som beräknad
        run.MarkeraSomBeraknad();
        await _payrollRunRepo.UpdateAsync(run, ct);

        // 7. Spara allt
        await _unitOfWork.SaveChangesAsync(ct);

        return run;
    }

    /// <summary>
    /// Kör en retroaktiv lönekörning för en given period.
    /// Skapar en ny körning med retroaktiva differensrader.
    /// </summary>
    /// <param name="year">Nuvarande löneperiodens år.</param>
    /// <param name="month">Nuvarande löneperiodens månad.</param>
    /// <param name="retroPeriod">Perioden som ska omberäknas (format: YYYY-MM).</param>
    /// <param name="startadAv">Användare som startade körningen.</param>
    /// <param name="ct">Avbrytningstoken.</param>
    public async Task<PayrollRun> ExecuteRetroactiveRunAsync(
        int year, int month, string retroPeriod, string startadAv, CancellationToken ct = default)
    {
        // Parsa retroaktiv period
        var retroParts = retroPeriod.Split('-');
        if (retroParts.Length != 2 ||
            !int.TryParse(retroParts[0], out var retroYear) ||
            !int.TryParse(retroParts[1], out var retroMonth))
        {
            throw new ArgumentException($"Ogiltigt periodformat: {retroPeriod}. Förväntat format: YYYY-MM");
        }

        // 1. Skapa retroaktiv lönekörning
        var run = PayrollRun.Skapa(year, month, startadAv, retroaktiv: true, retroPeriod: retroPeriod);
        run.Paborja();
        await _payrollRunRepo.AddAsync(run, ct);

        // 2. Hämta den ursprungliga körningen för retroperioden
        var allRuns = await _payrollRunRepo.GetAllAsync(ct);
        var originalRun = allRuns.FirstOrDefault(r =>
            r.Year == retroYear && r.Month == retroMonth &&
            !r.ArRetroaktiv &&
            r.Status is PayrollRunStatus.Beraknad or PayrollRunStatus.Granskad
                      or PayrollRunStatus.Godkand or PayrollRunStatus.Utbetald);

        if (originalRun is null)
            throw new InvalidOperationException(
                $"Ingen ursprunglig lönekörning hittades för period {retroPeriod}");

        // 3. Beräkna om varje anställd med uppdaterade uppgifter
        var periodStart = new DateOnly(retroYear, retroMonth, 1);
        var taxTableYear = retroYear != year ? year : (int?)null;

        foreach (var originalResult in originalRun.Resultat)
        {
            var employment = await _coreHR.GetActiveEmploymentAsync(
                originalResult.AnstallId, periodStart, ct);
            if (employment is null)
                continue;

            var employee = await _coreHR.GetEmployeeAsync(originalResult.AnstallId, ct);
            if (employee is null)
                continue;

            var correctedInput = await BuildPayrollInputAsync(employee, employment, retroYear, retroMonth, ct);

            try
            {
                // Omberäkna med nuvarande uppgifter
                var recalculated = await _calculationEngine.CalculateAsync(
                    run.Id, originalResult.AnstallId, employment.Id,
                    retroYear, retroMonth, correctedInput, ct);

                // Beräkna differens
                var retroResult = await _retroEngine.RecalculateAsync(
                    originalResult, recalculated, taxTableYear, ct);

                // Skapa resultatrader med differenser
                if (retroResult.NettoDifferens != Money.Zero)
                {
                    var result = PayrollResult.Skapa(
                        run.Id, originalResult.AnstallId, employment.Id,
                        year, month, recalculated.Manadslon,
                        recalculated.Sysselsattningsgrad, recalculated.Kollektivavtal);

                    // Lägg till retroaktiva differensrader
                    foreach (var diffLine in retroResult.DifferenceLines)
                    {
                        result.LaggTillRad(new PayrollResultLine
                        {
                            LoneartKod = diffLine.LoneartKod,
                            Benamning = $"{diffLine.Benamning} ({retroPeriod})",
                            Antal = 1,
                            Sats = diffLine.Differens,
                            Belopp = diffLine.Differens,
                            Skattekategori = TaxCategory.Skattepliktig,
                            ArAvdrag = diffLine.ArAvdrag
                        });
                    }

                    result.Brutto = retroResult.BruttoDifferens;
                    result.Skatt = retroResult.SkatteDifferens;
                    result.Netto = retroResult.NettoDifferens;
                    result.Arbetsgivaravgifter = retroResult.ArbetsgivaravgiftDifferens;

                    run.LaggTillResultat(result);
                }
            }
            catch (Exception ex)
            {
                run.LaggTillFel(originalResult.AnstallId, ex.Message);
            }
        }

        // Markera som beräknad
        run.MarkeraSomBeraknad();
        await _payrollRunRepo.UpdateAsync(run, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return run;
    }

    /// <summary>
    /// Hämtar alla aktiva anställda för given period.
    /// I produktion: itererar alla organisationsenheter och hämtar anställda per enhet.
    /// </summary>
    private async Task<IReadOnlyList<EmployeeDto>> GetAllActiveEmployeesAsync(
        DateOnly periodStart, CancellationToken ct)
    {
        // Hämta alla organisationsenheter rekursivt
        // I fullständig implementation traverserar vi hela organisationsträdet
        // Här hämtar vi via root-enheten (förenklade i denna implementation)
        var allEmployees = new List<EmployeeDto>();
        var processedIds = new HashSet<Guid>();

        // Hämta alla tillgängliga anställningar och deras anställda
        // I produktion: implementera via en dedikerad metod på ICoreHRModule
        // som returnerar alla aktiva anställda för en period
        var rootUnits = await GetRootOrganizationUnitsAsync(ct);
        foreach (var unit in rootUnits)
        {
            var employees = await _coreHR.GetEmployeesByUnitAsync(unit.Id, periodStart, ct);
            foreach (var emp in employees)
            {
                if (processedIds.Add(emp.Id.Value))
                {
                    allEmployees.Add(emp);
                }
            }
        }

        return allEmployees.AsReadOnly();
    }

    /// <summary>
    /// Bygger PayrollInput för en anställd och period.
    /// I produktion: hämtar data från tidrapportering, frånvaroregister, etc.
    /// </summary>
    private Task<PayrollInput> BuildPayrollInputAsync(
        EmployeeDto employee,
        EmploymentDto employment,
        int year,
        int month,
        CancellationToken ct)
    {
        // I fullständig implementation hämtas detta från:
        // - Tidrapporteringssystem (arbetade timmar, OB, övertid, jour, beredskap)
        // - Frånvaroregister (sjukdagar, semester, föräldraledighet)
        // - Utmätningsregister (löneutmätning)
        // - Fackliga register (fackavgift)

        var lastDay = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        var firstDay = new DateOnly(year, month, 1);

        // Beräkna arbetsdagar i månaden (exklusive helger och helgdagar)
        var arbetsdagar = 0;
        for (var d = firstDay; d <= lastDay; d = d.AddDays(1))
        {
            if (d.DayOfWeek != DayOfWeek.Saturday &&
                d.DayOfWeek != DayOfWeek.Sunday &&
                !SvenskaHelgdagar.ArHelgdag(d))
            {
                arbetsdagar++;
            }
        }

        var input = new PayrollInput
        {
            ArbetadeDagar = arbetsdagar,
            ArbetsdagarIManadens = arbetsdagar,
            Kostnadsstalle = employment.EnhetId.ToString()
        };

        return Task.FromResult(input);
    }

    /// <summary>
    /// Hämtar rot-organisationsenheter.
    /// Placeholder -- i fullständig implementation hämtas dessa via ICoreHRModule.
    /// </summary>
    private Task<IReadOnlyList<OrganizationUnitDto>> GetRootOrganizationUnitsAsync(CancellationToken ct)
    {
        // Returnera tom lista som säkert fallback
        // I produktion: hämta via ICoreHRModule med en dedikerad metod
        return Task.FromResult<IReadOnlyList<OrganizationUnitDto>>(Array.Empty<OrganizationUnitDto>());
    }
}
