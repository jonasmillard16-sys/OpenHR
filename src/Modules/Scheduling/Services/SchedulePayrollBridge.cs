using RegionHR.SharedKernel.Domain;
using RegionHR.Scheduling.Domain;

namespace RegionHR.Scheduling.Services;

/// <summary>
/// Brygga mellan schema- och lönemodulerna.
/// Aggregerar arbetade timmar, OB-timmar, övertid m.m. till löneunderlag.
/// </summary>
public sealed class SchedulePayrollBridge
{
    private readonly IScheduledShiftRepository _shiftRepository;
    private readonly IOBKategoriProvider _obKategoriProvider;

    public SchedulePayrollBridge(
        IScheduledShiftRepository shiftRepository,
        IOBKategoriProvider obKategoriProvider)
    {
        _shiftRepository = shiftRepository;
        _obKategoriProvider = obKategoriProvider;
    }

    /// <summary>
    /// Hämta löneunderlag för en anställd och en given månad.
    /// Aggregerar alla arbetade timmar uppdelat per OB-kategori.
    /// </summary>
    public async Task<PayrollScheduleData> HamtaLoneunderlagAsync(
        EmployeeId anstallId,
        int year,
        int month,
        CancellationToken ct = default)
    {
        var periodStart = new DateOnly(year, month, 1);
        var periodSlut = periodStart.AddMonths(1).AddDays(-1);

        var pass = await _shiftRepository.HamtaPassForAnstaldAsync(
            anstallId, periodStart, periodSlut, ct);

        // Filtrera pass med faktiska tider (dvs. instämplade och avslutade)
        var arbetadePass = pass
            .Where(p => p.FaktiskStart.HasValue && p.FaktiskSlut.HasValue)
            .ToList();

        // Beräkna arbetade dagar
        var arbetadeDagar = arbetadePass
            .Select(p => p.Datum)
            .Distinct()
            .Count();

        // Beräkna totala arbetsdagar i månaden (mån-fre)
        var arbetsdagarIManaden = 0;
        for (var d = periodStart; d <= periodSlut; d = d.AddDays(1))
        {
            if (d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                arbetsdagarIManaden++;
        }

        // Beräkna OB-timmar per kategori
        var obTimmar = new Dictionary<OBCategory, decimal>();
        var totalOvertid = 0m;
        var totalJour = 0m;
        var totalBeredskap = 0m;

        foreach (var p in arbetadePass)
        {
            if (p.PassTyp == ShiftType.Jour)
            {
                totalJour += p.FaktiskaTimmar ?? p.PlaneradeTimmar;
                continue;
            }

            if (p.PassTyp == ShiftType.Beredskap)
            {
                totalBeredskap += p.FaktiskaTimmar ?? p.PlaneradeTimmar;
                continue;
            }

            // Beräkna OB per timme i passet
            var obForPass = BeraknaOBTimmarForPass(p);
            foreach (var (kategori, timmar) in obForPass)
            {
                if (!obTimmar.ContainsKey(kategori))
                    obTimmar[kategori] = 0m;
                obTimmar[kategori] += timmar;
            }

            // Övertid
            if (p.OvertidTimmar.HasValue && p.OvertidTimmar > 0)
            {
                totalOvertid += p.OvertidTimmar.Value;
            }
        }

        // Bedöm om övertiden är kvalificerad (> 2h per dag = kvalificerad)
        var kvalificeradOvertid = arbetadePass
            .Any(p => p.OvertidTimmar.HasValue && p.OvertidTimmar > 2m);

        return new PayrollScheduleData
        {
            ArbetadeDagar = arbetadeDagar,
            ArbetsdagarIManaden = arbetsdagarIManaden,
            OBTimmar = obTimmar
                .Where(kv => kv.Key != OBCategory.Ingen)
                .Select(kv => new OBSummary { Kategori = kv.Key, Timmar = kv.Value })
                .ToList(),
            OvertidTimmar = totalOvertid,
            KvalificeradOvertid = kvalificeradOvertid,
            JourTimmar = totalJour,
            BeredskapsTimmar = totalBeredskap
        };
    }

    /// <summary>
    /// Beräkna OB-timmar per kategori för ett enskilt pass.
    /// Itererar timme för timme genom passet och använder IOBKategoriProvider
    /// för att avgöra kategori baserat på datum och tidpunkt.
    /// </summary>
    private Dictionary<OBCategory, decimal> BeraknaOBTimmarForPass(ScheduledShift pass)
    {
        var result = new Dictionary<OBCategory, decimal>();

        var start = pass.FaktiskStart ?? pass.PlaneradStart;
        var slut = pass.FaktiskSlut ?? pass.PlaneradSlut;

        // Hantera nattpass som korsar midnatt
        var startDT = pass.Datum.ToDateTime(start);
        var slutDT = pass.Datum.ToDateTime(slut);
        if (slutDT <= startDT)
            slutDT = slutDT.AddDays(1);

        // Subtrahera rast från total tid
        var totalMinuter = (slutDT - startDT).TotalMinutes;
        var rastMinuter = pass.Rast.TotalMinutes;

        // Beräkna per 15-minutersintervall för precision
        var current = startDT;
        var intervall = TimeSpan.FromMinutes(15);
        var totalIntervall = 0;
        var kategoriFördelning = new Dictionary<OBCategory, int>();

        while (current < slutDT)
        {
            var datum = DateOnly.FromDateTime(current);
            var tid = TimeOnly.FromDateTime(current);
            var kategori = _obKategoriProvider.BeraknaKategori(datum, tid);

            if (!kategoriFördelning.ContainsKey(kategori))
                kategoriFördelning[kategori] = 0;
            kategoriFördelning[kategori]++;
            totalIntervall++;

            current = current.Add(intervall);
        }

        // Konvertera till timmar, proportionellt minskade med rast
        if (totalIntervall == 0) return result;

        var effektivTid = (decimal)(totalMinuter - rastMinuter);
        if (effektivTid < 0) effektivTid = 0;

        foreach (var (kategori, antal) in kategoriFördelning)
        {
            var andel = (decimal)antal / totalIntervall;
            var timmar = Math.Round(andel * effektivTid / 60m, 2);
            if (timmar > 0)
                result[kategori] = timmar;
        }

        return result;
    }
}

/// <summary>
/// Aggregerat löneunderlag från schemamodulen.
/// </summary>
public sealed class PayrollScheduleData
{
    public int ArbetadeDagar { get; init; }
    public int ArbetsdagarIManaden { get; init; }
    public List<OBSummary> OBTimmar { get; init; } = [];
    public decimal OvertidTimmar { get; init; }
    public bool KvalificeradOvertid { get; init; }
    public decimal JourTimmar { get; init; }
    public decimal BeredskapsTimmar { get; init; }
}

/// <summary>
/// Sammanfattning av OB-timmar per kategori.
/// </summary>
public sealed class OBSummary
{
    public OBCategory Kategori { get; init; }
    public decimal Timmar { get; init; }
}
