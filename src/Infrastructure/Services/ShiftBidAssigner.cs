using RegionHR.Scheduling.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Motor för att tilldela öppna pass baserat på inkomna bud.
/// Stödjer fyra tilldelningsmetoder: FirstComeFirstServed, Seniority, Kompetens, Rotation.
/// Validerar ATL-efterlevnad (11h dygnsvila), inga dubbelpass, och trötthetsgräns.
/// </summary>
public sealed class ShiftBidAssigner
{
    /// <summary>Max fatigue score för att accepteras.</summary>
    private const int MAX_FATIGUE_SCORE = 70;

    /// <summary>Min dygnsvila i timmar.</summary>
    private const int MIN_REST_HOURS = 11;

    /// <summary>
    /// Resultat av tilldelning.
    /// </summary>
    public sealed class AssignmentResult
    {
        public bool Success { get; init; }
        public EmployeeId? VinnareId { get; init; }
        public string? Metod { get; init; }
        public string Motivering { get; init; } = string.Empty;
        public ShiftBidResult? BidResult { get; init; }
    }

    /// <summary>
    /// Information om en anställd som behövs för tilldelning.
    /// </summary>
    public sealed class EmployeeInfo
    {
        public EmployeeId AnstallId { get; init; }
        public DateOnly AnstallningsDatum { get; init; }
        public List<string> Kompetenser { get; init; } = [];
        public int AntalExtraPassSenaste30Dagar { get; init; }
        public int FatigueScore { get; init; }
        public bool HarPassPaDatum { get; init; }

        /// <summary>Senaste passets slut (för ATL-kontroll).</summary>
        public DateTime? SenastePassSlut { get; init; }
    }

    /// <summary>
    /// Tilldela ett öppet pass till den bästa kandidaten bland budgivarna.
    /// </summary>
    public AssignmentResult Tilldela(
        OpenShift openShift,
        IReadOnlyList<ShiftBid> bud,
        string metod,
        IReadOnlyList<EmployeeInfo> anstallda,
        List<string>? kravdaKompetenser = null)
    {
        if (bud.Count == 0)
            return new AssignmentResult { Success = false, Motivering = "Inga bud inkomna." };

        // Filtrera bort de som inte uppfyller grundkrav
        var kvalificerade = FiltreraKvalificerade(openShift, bud, anstallda);

        if (kvalificerade.Count == 0)
            return new AssignmentResult
            {
                Success = false,
                Motivering = "Inga budgivare uppfyller kraven (ATL, dubbelpass, trötthet)."
            };

        // Tillämpa vald metod
        var (vinnare, motivering) = metod switch
        {
            "FirstComeFirstServed" => TillampaFCFS(kvalificerade),
            "Seniority" => TillampaSeniority(kvalificerade, anstallda),
            "Kompetens" => TillampaKompetens(kvalificerade, anstallda, kravdaKompetenser),
            "Rotation" => TillampaRotation(kvalificerade, anstallda),
            _ => throw new ArgumentException($"Okänd metod: {metod}", nameof(metod))
        };

        // Skapa resultat
        var bidResult = ShiftBidResult.Skapa(openShift.Id, vinnare, metod, motivering);

        // Uppdatera passet
        openShift.Tilldela(vinnare, metod);

        // Uppdatera bud-status
        foreach (var b in bud)
        {
            if (b.AnstallId == vinnare)
                b.Acceptera();
            else
                b.Avvisa();
        }

        return new AssignmentResult
        {
            Success = true,
            VinnareId = vinnare,
            Metod = metod,
            Motivering = motivering,
            BidResult = bidResult
        };
    }

    private List<(ShiftBid Bud, EmployeeInfo Info)> FiltreraKvalificerade(
        OpenShift openShift,
        IReadOnlyList<ShiftBid> bud,
        IReadOnlyList<EmployeeInfo> anstallda)
    {
        var infoMap = anstallda.ToDictionary(e => e.AnstallId);
        var result = new List<(ShiftBid, EmployeeInfo)>();

        foreach (var b in bud.Where(b => b.Status == ShiftBidStatus.Pending))
        {
            if (!infoMap.TryGetValue(b.AnstallId, out var info))
                continue;

            // 1. Redan schemalagd på datumet
            if (info.HarPassPaDatum)
                continue;

            // 2. FatigueScore > 70
            if (info.FatigueScore >= MAX_FATIGUE_SCORE)
                continue;

            // 3. ATL: minst 11h vila sedan senaste pass
            if (info.SenastePassSlut.HasValue)
            {
                var passStart = openShift.Datum.ToDateTime(openShift.StartTid);
                var vilaTimmar = (passStart - info.SenastePassSlut.Value).TotalHours;
                if (vilaTimmar < MIN_REST_HOURS)
                    continue;
            }

            result.Add((b, info));
        }

        return result;
    }

    private static (EmployeeId Vinnare, string Motivering) TillampaFCFS(
        List<(ShiftBid Bud, EmployeeInfo Info)> kvalificerade)
    {
        var vinnare = kvalificerade.OrderBy(k => k.Bud.SkapadVid).First();
        return (vinnare.Bud.AnstallId,
            $"Tilldelad via first-come-first-served. Bud inkom {vinnare.Bud.SkapadVid:yyyy-MM-dd HH:mm}.");
    }

    private static (EmployeeId Vinnare, string Motivering) TillampaSeniority(
        List<(ShiftBid Bud, EmployeeInfo Info)> kvalificerade,
        IReadOnlyList<EmployeeInfo> anstallda)
    {
        var vinnare = kvalificerade
            .OrderBy(k => k.Info.AnstallningsDatum) // Earliest start = most senior
            .ThenBy(k => k.Bud.Prioritet)
            .First();

        var anstallningsar = (DateTime.Today - vinnare.Info.AnstallningsDatum.ToDateTime(TimeOnly.MinValue)).Days / 365;
        return (vinnare.Bud.AnstallId,
            $"Tilldelad via senioritet. Anställd sedan {vinnare.Info.AnstallningsDatum:yyyy-MM-dd} ({anstallningsar} år).");
    }

    private static (EmployeeId Vinnare, string Motivering) TillampaKompetens(
        List<(ShiftBid Bud, EmployeeInfo Info)> kvalificerade,
        IReadOnlyList<EmployeeInfo> anstallda,
        List<string>? kravdaKompetenser)
    {
        if (kravdaKompetenser is null || kravdaKompetenser.Count == 0)
        {
            // Om inga krav, välj den med flest kompetenser
            var vinnare = kvalificerade
                .OrderByDescending(k => k.Info.Kompetenser.Count)
                .ThenBy(k => k.Bud.Prioritet)
                .First();
            return (vinnare.Bud.AnstallId,
                $"Tilldelad via kompetens (ingen kravprofil). Anställd har {vinnare.Info.Kompetenser.Count} kompetenser.");
        }

        // Räkna matchande kompetenser
        var ranked = kvalificerade
            .Select(k => new
            {
                k.Bud,
                k.Info,
                Matchning = kravdaKompetenser.Count(krav =>
                    k.Info.Kompetenser.Any(komp =>
                        komp.Equals(krav, StringComparison.OrdinalIgnoreCase)))
            })
            .OrderByDescending(k => k.Matchning)
            .ThenBy(k => k.Bud.Prioritet)
            .First();

        return (ranked.Bud.AnstallId,
            $"Tilldelad via kompetensmatching. {ranked.Matchning}/{kravdaKompetenser.Count} krav uppfyllda.");
    }

    private static (EmployeeId Vinnare, string Motivering) TillampaRotation(
        List<(ShiftBid Bud, EmployeeInfo Info)> kvalificerade,
        IReadOnlyList<EmployeeInfo> anstallda)
    {
        // Minst antal extrapass senaste 30 dagarna = rättvisast
        var vinnare = kvalificerade
            .OrderBy(k => k.Info.AntalExtraPassSenaste30Dagar)
            .ThenBy(k => k.Bud.Prioritet)
            .First();

        return (vinnare.Bud.AnstallId,
            $"Tilldelad via rotation (rättvisa). Anställd har {vinnare.Info.AntalExtraPassSenaste30Dagar} extrapass senaste 30 dagarna.");
    }
}
