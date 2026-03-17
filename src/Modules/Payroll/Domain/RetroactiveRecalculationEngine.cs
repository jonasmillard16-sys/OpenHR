using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Domain;

/// <summary>
/// Hanterar retroaktiv löneomräkning.
/// Jämför originalresultat med omberäknat resultat och genererar
/// retroaktiva differensrader för utbetalning.
/// </summary>
public sealed class RetroactiveRecalculationEngine
{
    private readonly ITaxTableProvider _taxTableProvider;

    public RetroactiveRecalculationEngine(ITaxTableProvider taxTableProvider)
    {
        _taxTableProvider = taxTableProvider;
    }

    /// <summary>
    /// Utför retroaktiv omräkning genom att jämföra originalresultat
    /// med ett nytt resultat baserat på korrigerad input.
    /// </summary>
    /// <param name="original">Det ursprungliga löneresultatet.</param>
    /// <param name="recalculated">Det omberäknade löneresultatet med korrigerad input.</param>
    /// <param name="taxTableYear">Skattetabellår att använda (viktigt vid årsövergångar).</param>
    /// <param name="ct">Avbrytningstoken.</param>
    /// <returns>Retroaktivt resultat med differensrader.</returns>
    public Task<RetroactiveResult> RecalculateAsync(
        PayrollResult original,
        PayrollResult recalculated,
        int? taxTableYear = null,
        CancellationToken ct = default)
    {
        var differenceLines = new List<RetroactiveDifferenceLine>();
        var actualTaxYear = taxTableYear ?? original.Year;

        // Jämför grundlön
        var grundlonDiff = CompareLinesByCode(original, recalculated, "1100", "Retro månadslön", "7100");
        if (grundlonDiff is not null)
            differenceLines.Add(grundlonDiff);

        // Jämför OB-tillägg
        var obDiff = CompareAggregatedByCodePrefix(original, recalculated, "131", "Retro OB-tillägg", "7110");
        if (obDiff is not null)
            differenceLines.Add(obDiff);

        // Jämför övertid
        var overtidDiff = CompareAggregatedByCodePrefix(original, recalculated, "14", "Retro övertid", "7120");
        if (overtidDiff is not null)
            differenceLines.Add(overtidDiff);

        // Jämför jour
        var jourDiff = CompareLinesByCode(original, recalculated, "1500", "Retro jour", "7130");
        if (jourDiff is not null)
            differenceLines.Add(jourDiff);

        // Jämför beredskap
        var beredskapDiff = CompareLinesByCode(original, recalculated, "1510", "Retro beredskap", "7140");
        if (beredskapDiff is not null)
            differenceLines.Add(beredskapDiff);

        // Jämför sjuklön
        var sjuklonDiff = CompareAggregatedByCodePrefix(original, recalculated, "30", "Retro sjuklön", "7100");
        if (sjuklonDiff is not null)
            differenceLines.Add(sjuklonDiff);

        // Jämför semester
        var semesterDiff = CompareAggregatedByCodePrefix(original, recalculated, "27", "Retro semester", "7100");
        if (semesterDiff is not null)
            differenceLines.Add(semesterDiff);

        // Jämför föräldralöneutfyllnad
        var foraldralDiff = CompareLinesByCode(original, recalculated, "3100", "Retro föräldralön", "7100");
        if (foraldralDiff is not null)
            differenceLines.Add(foraldralDiff);

        // Beräkna bruttodifferens
        var bruttoDiff = recalculated.Brutto - original.Brutto;

        // Beräkna skattedifferens
        // Vid årsövergång kan skattetabellen skilja sig
        var originalSkatt = original.Skatt;
        var newSkatt = recalculated.Skatt;

        // Om retroaktivt belopp ska beskattas med ett annat års skattetabell
        if (taxTableYear.HasValue && taxTableYear.Value != original.Year)
        {
            // Vid årsövergång: den retroaktiva delen beskattas med en schablonskatt på 30%
            // enligt Skatteverkets regler för retroaktiv lön som avser annat inkomstår
            var retroSkatt = (bruttoDiff * 0.30m).RoundToKronor();
            if (bruttoDiff > Money.Zero)
            {
                newSkatt = originalSkatt + retroSkatt;
            }
        }

        var skatteDiff = newSkatt - originalSkatt;

        // Lägg till skattejusteringsrad om det finns en differens
        if (skatteDiff != Money.Zero)
        {
            differenceLines.Add(new RetroactiveDifferenceLine
            {
                LoneartKod = "7150",
                Benamning = "Retro skattejustering",
                OriginalBelopp = originalSkatt,
                NyttBelopp = newSkatt,
                Differens = skatteDiff,
                ArAvdrag = skatteDiff > Money.Zero
            });
        }

        // Nettodifferens = bruttodifferens - skattedifferens
        var nettoDiff = bruttoDiff - skatteDiff;

        // Arbetsgivaravgiftsdifferens
        var agDiff = recalculated.Arbetsgivaravgifter - original.Arbetsgivaravgifter;

        // Pensionsdifferens
        var pensionDiff = recalculated.Pensionsavgift - original.Pensionsavgift;

        return Task.FromResult(new RetroactiveResult
        {
            OriginalPeriod = $"{original.Year}-{original.Month:D2}",
            OriginalResultatId = original.Id,
            DifferenceLines = differenceLines.AsReadOnly(),
            BruttoDifferens = bruttoDiff,
            SkatteDifferens = skatteDiff,
            NettoDifferens = nettoDiff,
            ArbetsgivaravgiftDifferens = agDiff,
            PensionDifferens = pensionDiff
        });
    }

    /// <summary>
    /// Jämför en specifik löneartsrad mellan original och omberäknat resultat.
    /// </summary>
    private static RetroactiveDifferenceLine? CompareLinesByCode(
        PayrollResult original,
        PayrollResult recalculated,
        string loneartKod,
        string retroBenamning,
        string retroLoneartKod)
    {
        var originalBelopp = SumLinesByCode(original, loneartKod);
        var nyttBelopp = SumLinesByCode(recalculated, loneartKod);
        var diff = nyttBelopp - originalBelopp;

        if (diff == Money.Zero)
            return null;

        return new RetroactiveDifferenceLine
        {
            LoneartKod = retroLoneartKod,
            Benamning = retroBenamning,
            OriginalBelopp = originalBelopp,
            NyttBelopp = nyttBelopp,
            Differens = diff,
            ArAvdrag = diff < Money.Zero
        };
    }

    /// <summary>
    /// Jämför aggregerat belopp för lönearter med visst prefix.
    /// </summary>
    private static RetroactiveDifferenceLine? CompareAggregatedByCodePrefix(
        PayrollResult original,
        PayrollResult recalculated,
        string kodPrefix,
        string retroBenamning,
        string retroLoneartKod)
    {
        var originalBelopp = SumLinesByCodePrefix(original, kodPrefix);
        var nyttBelopp = SumLinesByCodePrefix(recalculated, kodPrefix);
        var diff = nyttBelopp - originalBelopp;

        if (diff == Money.Zero)
            return null;

        return new RetroactiveDifferenceLine
        {
            LoneartKod = retroLoneartKod,
            Benamning = retroBenamning,
            OriginalBelopp = originalBelopp,
            NyttBelopp = nyttBelopp,
            Differens = diff,
            ArAvdrag = diff < Money.Zero
        };
    }

    private static Money SumLinesByCode(PayrollResult result, string kod)
    {
        var sum = result.Rader
            .Where(r => r.LoneartKod == kod)
            .Sum(r => r.Belopp.Amount);
        return Money.SEK(sum);
    }

    private static Money SumLinesByCodePrefix(PayrollResult result, string prefix)
    {
        var sum = result.Rader
            .Where(r => r.LoneartKod.StartsWith(prefix))
            .Sum(r => r.Belopp.Amount);
        return Money.SEK(sum);
    }
}

/// <summary>
/// Resultat av retroaktiv omräkning.
/// </summary>
public sealed class RetroactiveResult
{
    /// <summary>Originalperioden som omräknats (format YYYY-MM).</summary>
    public string OriginalPeriod { get; set; } = string.Empty;

    /// <summary>ID för det ursprungliga resultatet.</summary>
    public Guid OriginalResultatId { get; set; }

    /// <summary>Differensrader per löneart.</summary>
    public IReadOnlyList<RetroactiveDifferenceLine> DifferenceLines { get; set; } = [];

    /// <summary>Total bruttodifferens.</summary>
    public Money BruttoDifferens { get; set; } = Money.Zero;

    /// <summary>Skattedifferens.</summary>
    public Money SkatteDifferens { get; set; } = Money.Zero;

    /// <summary>Nettodifferens att betala ut (eller återkräva).</summary>
    public Money NettoDifferens { get; set; } = Money.Zero;

    /// <summary>Differens i arbetsgivaravgifter.</summary>
    public Money ArbetsgivaravgiftDifferens { get; set; } = Money.Zero;

    /// <summary>Differens i pensionsavgifter.</summary>
    public Money PensionDifferens { get; set; } = Money.Zero;
}

/// <summary>
/// En retroaktiv differensrad.
/// </summary>
public sealed class RetroactiveDifferenceLine
{
    public string LoneartKod { get; set; } = string.Empty;
    public string Benamning { get; set; } = string.Empty;
    public Money OriginalBelopp { get; set; } = Money.Zero;
    public Money NyttBelopp { get; set; } = Money.Zero;
    public Money Differens { get; set; } = Money.Zero;
    public bool ArAvdrag { get; set; }
}
