using RegionHR.SharedKernel.Domain;
using RegionHR.HalsoSAM.Domain;

namespace RegionHR.HalsoSAM.Services;

/// <summary>
/// Bevakar sjukfrånvaromönster och triggar rehabiliteringsärenden.
/// </summary>
public sealed class SickLeaveMonitor
{
    private const int MAX_TILLFALLEN_12_MANADER = 6;
    private const int MAX_SAMMANHANGANDE_DAGAR = 14;

    public RehabTrigger? Analysera(IReadOnlyList<SjukfranvaroPeriod> perioder)
    {
        if (perioder.Count == 0) return null;

        var senasteTolvManader = perioder
            .Where(p => p.StartDatum >= DateOnly.FromDateTime(DateTime.Today.AddMonths(-12)))
            .ToList();

        // Kontrollera 6+ tillfällen
        if (senasteTolvManader.Count >= MAX_TILLFALLEN_12_MANADER)
            return RehabTrigger.SexTillfallenTolvManader;

        // Kontrollera 14+ sammanhängande dagar
        var langstaPeriod = perioder.Max(p => p.AntalDagar);
        if (langstaPeriod >= MAX_SAMMANHANGANDE_DAGAR)
            return RehabTrigger.FjortonSammanhangandeDagar;

        // Mönsterdetektering (förenklad: kolla om >50% av sjukfrånvaron är samma veckodag)
        var dagarPerVeckodag = senasteTolvManader
            .GroupBy(p => p.StartDatum.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        if (dagarPerVeckodag is not null && dagarPerVeckodag.Count() > senasteTolvManader.Count * 0.5)
            return RehabTrigger.MonsterDetekterat;

        return null;
    }
}

public sealed class SjukfranvaroPeriod
{
    public DateOnly StartDatum { get; set; }
    public DateOnly SlutDatum { get; set; }
    public int AntalDagar => SlutDatum.DayNumber - StartDatum.DayNumber + 1;
}
