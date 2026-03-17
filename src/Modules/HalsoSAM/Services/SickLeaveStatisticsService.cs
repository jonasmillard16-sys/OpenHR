using RegionHR.SharedKernel.Domain;

namespace RegionHR.HalsoSAM.Services;

/// <summary>
/// Sjukfrånvarostatistik per enhet och period.
/// </summary>
public sealed class SjukfranvaroStatistik
{
    public OrganizationId EnhetId { get; init; }
    public decimal SjukfranvaroProcentTotal { get; init; }
    public decimal SjukfranvaroProcentKort { get; init; }    // < 14 dagar
    public decimal SjukfranvaroProcentLang { get; init; }    // >= 14 dagar
    public int AntalSjukfall { get; init; }
    public int AntalAktivaRehabArenden { get; init; }
    public decimal GenomsnittligaFranvarodagar { get; init; }
    public IReadOnlyList<ManadsStatistik> PerManad { get; init; } = [];
}

/// <summary>
/// Månadsvis sjukfrånvarostatistik.
/// </summary>
public sealed class ManadsStatistik
{
    public int Ar { get; init; }
    public int Manad { get; init; }
    public decimal Procent { get; init; }
    public int AntalFall { get; init; }
}

/// <summary>
/// Interface för att hämta sjukfrånvarodata från underliggande datalager.
/// </summary>
public interface ISickLeaveDataProvider
{
    Task<IReadOnlyList<SjukfranvaroRad>> HamtaSjukfranvaroAsync(
        OrganizationId enhetId, DateOnly from, DateOnly till, CancellationToken ct);

    Task<int> HamtaAntalAktivaRehabArendenAsync(OrganizationId enhetId, CancellationToken ct);

    Task<int> HamtaAntalAnstallda(OrganizationId enhetId, DateOnly datum, CancellationToken ct);
}

/// <summary>
/// Rådata per sjukfrånvarofall.
/// </summary>
public sealed class SjukfranvaroRad
{
    public EmployeeId AnstallId { get; init; }
    public DateOnly StartDatum { get; init; }
    public DateOnly SlutDatum { get; init; }
    public int AntalDagar => SlutDatum.DayNumber - StartDatum.DayNumber + 1;
}

/// <summary>
/// Tjänst för att beräkna sjukfrånvarostatistik per organisationsenhet.
/// </summary>
public sealed class SickLeaveStatisticsService
{
    private readonly ISickLeaveDataProvider _dataProvider;

    public SickLeaveStatisticsService(ISickLeaveDataProvider dataProvider)
    {
        _dataProvider = dataProvider;
    }

    /// <summary>
    /// Hämta sjukfrånvarostatistik för en enhet under en tidsperiod.
    /// </summary>
    public async Task<SjukfranvaroStatistik> HamtaStatistikAsync(
        OrganizationId enhetId, DateOnly from, DateOnly till, CancellationToken ct)
    {
        var rader = await _dataProvider.HamtaSjukfranvaroAsync(enhetId, from, till, ct);
        var antalAnstallda = await _dataProvider.HamtaAntalAnstallda(enhetId, till, ct);
        var antalAktivaRehab = await _dataProvider.HamtaAntalAktivaRehabArendenAsync(enhetId, ct);

        var totalKalenderdagar = till.DayNumber - from.DayNumber + 1;
        var totalArbetsdagar = antalAnstallda > 0 ? antalAnstallda * totalKalenderdagar : 1;

        var kortaFall = rader.Where(r => r.AntalDagar < 14).ToList();
        var langaFall = rader.Where(r => r.AntalDagar >= 14).ToList();

        var totalFranvarodagar = rader.Sum(r => r.AntalDagar);
        var kortFranvarodagar = kortaFall.Sum(r => r.AntalDagar);
        var langFranvarodagar = langaFall.Sum(r => r.AntalDagar);

        var totalProcent = totalArbetsdagar > 0
            ? Math.Round((decimal)totalFranvarodagar / totalArbetsdagar * 100, 2)
            : 0m;

        var kortProcent = totalArbetsdagar > 0
            ? Math.Round((decimal)kortFranvarodagar / totalArbetsdagar * 100, 2)
            : 0m;

        var langProcent = totalArbetsdagar > 0
            ? Math.Round((decimal)langFranvarodagar / totalArbetsdagar * 100, 2)
            : 0m;

        var genomsnittDagar = rader.Count > 0
            ? Math.Round((decimal)totalFranvarodagar / rader.Count, 1)
            : 0m;

        // Månadsvis statistik
        var perManad = BeraknaManadsstatistik(rader, from, till, antalAnstallda);

        return new SjukfranvaroStatistik
        {
            EnhetId = enhetId,
            SjukfranvaroProcentTotal = totalProcent,
            SjukfranvaroProcentKort = kortProcent,
            SjukfranvaroProcentLang = langProcent,
            AntalSjukfall = rader.Count,
            AntalAktivaRehabArenden = antalAktivaRehab,
            GenomsnittligaFranvarodagar = genomsnittDagar,
            PerManad = perManad
        };
    }

    private static List<ManadsStatistik> BeraknaManadsstatistik(
        IReadOnlyList<SjukfranvaroRad> rader, DateOnly from, DateOnly till, int antalAnstallda)
    {
        var result = new List<ManadsStatistik>();
        var current = new DateOnly(from.Year, from.Month, 1);
        var slutManad = new DateOnly(till.Year, till.Month, 1);

        while (current <= slutManad)
        {
            var manadStart = current;
            var manadSlut = current.AddMonths(1).AddDays(-1);

            var manadensRader = rader
                .Where(r => r.StartDatum <= manadSlut && r.SlutDatum >= manadStart)
                .ToList();

            var dagar = manadSlut.DayNumber - manadStart.DayNumber + 1;
            var franvarodagar = manadensRader.Sum(r =>
            {
                var effStart = r.StartDatum < manadStart ? manadStart : r.StartDatum;
                var effSlut = r.SlutDatum > manadSlut ? manadSlut : r.SlutDatum;
                return effSlut.DayNumber - effStart.DayNumber + 1;
            });

            var namnare = antalAnstallda > 0 ? antalAnstallda * dagar : 1;
            var procent = Math.Round((decimal)franvarodagar / namnare * 100, 2);

            result.Add(new ManadsStatistik
            {
                Ar = current.Year,
                Manad = current.Month,
                Procent = procent,
                AntalFall = manadensRader.Count
            });

            current = current.AddMonths(1);
        }

        return result;
    }
}
