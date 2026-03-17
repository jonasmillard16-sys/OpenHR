using System.Globalization;
using System.Text;
using RegionHR.Payroll.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.IntegrationHub.Adapters.Raindance;

/// <summary>
/// Genererar bokföringsfil (konteringsfil) för Raindance ekonomisystem.
/// Skapar balanserade bokföringstransaktioner (debet = kredit) grupperade per kostnadsställe.
///
/// Kontoplanen:
/// 5010 - Löner
/// 5020 - OB-tillägg
/// 5030 - Övertid
/// 5040 - Jour och beredskap
/// 7510 - Arbetsgivaravgifter (kostnad)
/// 7410 - Pensionsavgifter (kostnad)
/// 2710 - Personalskatt (skuld, kredit)
/// 2730 - Arbetsgivaravgift skuld (kredit)
/// 2920 - Löneskuld (kredit)
/// 7420 - Pensionsskuld (kredit)
/// </summary>
public sealed class RaindanceKonteringsGenerator
{
    private const string KONTO_LONER = "5010";
    private const string KONTO_OB = "5020";
    private const string KONTO_OVERTID = "5030";
    private const string KONTO_JOUR_BEREDSKAP = "5040";
    private const string KONTO_AG_AVGIFTER = "7510";
    private const string KONTO_PENSION = "7410";
    private const string KONTO_PERSONALSKATT = "2710";
    private const string KONTO_AG_SKULD = "2730";
    private const string KONTO_LONESKULD = "2920";
    private const string KONTO_PENSION_SKULD = "7420";

    /// <summary>
    /// Genererar konteringsrader för en hel lönekörning.
    /// Grupperar per kostnadsställe och skapar balanserade bokföringsposter.
    /// </summary>
    /// <param name="run">Lönekörning med resultat.</param>
    /// <returns>Lista med konteringsrader.</returns>
    public IReadOnlyList<KonteringsRad> GenerateEntries(PayrollRun run)
    {
        var rader = new List<KonteringsRad>();

        // Gruppera resultat per kostnadsställe
        var grupperadeResultat = run.Resultat
            .GroupBy(r => ExtractKostnadsstalle(r))
            .ToList();

        foreach (var group in grupperadeResultat)
        {
            var kostnadsstalle = group.Key;
            var resultat = group.ToList();

            // Summera per kostnadsställe
            var totalLoner = Money.SEK(resultat.Sum(r =>
                r.Rader.Where(rad => rad.LoneartKod.StartsWith("11") || rad.LoneartKod.StartsWith("12") ||
                                     rad.LoneartKod.StartsWith("31") || rad.LoneartKod.StartsWith("27") ||
                                     rad.LoneartKod.StartsWith("71"))
                       .Sum(rad => rad.Belopp.Amount)));

            var totalOB = Money.SEK(resultat.Sum(r =>
                r.Rader.Where(rad => rad.LoneartKod.StartsWith("131") || rad.LoneartKod.StartsWith("132") ||
                                     rad.LoneartKod.StartsWith("133") || rad.LoneartKod.StartsWith("134"))
                       .Sum(rad => rad.Belopp.Amount)));

            var totalOvertid = Money.SEK(resultat.Sum(r =>
                r.Rader.Where(rad => rad.LoneartKod.StartsWith("14"))
                       .Sum(rad => rad.Belopp.Amount)));

            var totalJourBeredskap = Money.SEK(resultat.Sum(r =>
                r.Rader.Where(rad => rad.LoneartKod == "1500" || rad.LoneartKod == "1510")
                       .Sum(rad => rad.Belopp.Amount)));

            var totalBrutto = Money.SEK(resultat.Sum(r => r.Brutto.Amount));
            var totalSkatt = Money.SEK(resultat.Sum(r => r.Skatt.Amount));
            var totalNetto = Money.SEK(resultat.Sum(r => r.Netto.Amount));
            var totalAG = Money.SEK(resultat.Sum(r => r.Arbetsgivaravgifter.Amount));
            var totalPension = Money.SEK(resultat.Sum(r => r.Pensionsavgift.Amount));
            var totalAvdrag = Money.SEK(resultat.Sum(r => r.Loneutmatning.Amount + r.Fackavgift.Amount + r.OvrigaAvdrag.Amount));

            var period = run.Period;

            // Debet: Lönekostnader
            if (totalLoner > Money.Zero)
            {
                rader.Add(new KonteringsRad
                {
                    Kostnadsstalle = kostnadsstalle,
                    Konto = KONTO_LONER,
                    Debet = totalLoner,
                    Kredit = Money.Zero,
                    Text = $"Löner {period}",
                    Period = period
                });
            }

            if (totalOB > Money.Zero)
            {
                rader.Add(new KonteringsRad
                {
                    Kostnadsstalle = kostnadsstalle,
                    Konto = KONTO_OB,
                    Debet = totalOB,
                    Kredit = Money.Zero,
                    Text = $"OB-tillägg {period}",
                    Period = period
                });
            }

            if (totalOvertid > Money.Zero)
            {
                rader.Add(new KonteringsRad
                {
                    Kostnadsstalle = kostnadsstalle,
                    Konto = KONTO_OVERTID,
                    Debet = totalOvertid,
                    Kredit = Money.Zero,
                    Text = $"Övertid {period}",
                    Period = period
                });
            }

            if (totalJourBeredskap > Money.Zero)
            {
                rader.Add(new KonteringsRad
                {
                    Kostnadsstalle = kostnadsstalle,
                    Konto = KONTO_JOUR_BEREDSKAP,
                    Debet = totalJourBeredskap,
                    Kredit = Money.Zero,
                    Text = $"Jour/beredskap {period}",
                    Period = period
                });
            }

            // Debet: Arbetsgivaravgifter (kostnad)
            if (totalAG > Money.Zero)
            {
                rader.Add(new KonteringsRad
                {
                    Kostnadsstalle = kostnadsstalle,
                    Konto = KONTO_AG_AVGIFTER,
                    Debet = totalAG,
                    Kredit = Money.Zero,
                    Text = $"Arbetsgivaravgifter {period}",
                    Period = period
                });
            }

            // Debet: Pensionsavgifter (kostnad)
            if (totalPension > Money.Zero)
            {
                rader.Add(new KonteringsRad
                {
                    Kostnadsstalle = kostnadsstalle,
                    Konto = KONTO_PENSION,
                    Debet = totalPension,
                    Kredit = Money.Zero,
                    Text = $"Pensionsavgifter {period}",
                    Period = period
                });
            }

            // Kredit: Personalskatt (skuld till Skatteverket)
            if (totalSkatt > Money.Zero)
            {
                rader.Add(new KonteringsRad
                {
                    Kostnadsstalle = kostnadsstalle,
                    Konto = KONTO_PERSONALSKATT,
                    Debet = Money.Zero,
                    Kredit = totalSkatt,
                    Text = $"Personalskatt {period}",
                    Period = period
                });
            }

            // Kredit: Arbetsgivaravgift skuld
            if (totalAG > Money.Zero)
            {
                rader.Add(new KonteringsRad
                {
                    Kostnadsstalle = kostnadsstalle,
                    Konto = KONTO_AG_SKULD,
                    Debet = Money.Zero,
                    Kredit = totalAG,
                    Text = $"AG-avgift skuld {period}",
                    Period = period
                });
            }

            // Kredit: Löneskuld (netto att betala ut + avdrag)
            var loneskuld = totalNetto + totalAvdrag;
            if (loneskuld > Money.Zero)
            {
                rader.Add(new KonteringsRad
                {
                    Kostnadsstalle = kostnadsstalle,
                    Konto = KONTO_LONESKULD,
                    Debet = Money.Zero,
                    Kredit = loneskuld,
                    Text = $"Löneskuld {period}",
                    Period = period
                });
            }

            // Kredit: Pensionsskuld
            if (totalPension > Money.Zero)
            {
                rader.Add(new KonteringsRad
                {
                    Kostnadsstalle = kostnadsstalle,
                    Konto = KONTO_PENSION_SKULD,
                    Debet = Money.Zero,
                    Kredit = totalPension,
                    Text = $"Pensionsskuld {period}",
                    Period = period
                });
            }
        }

        return rader.AsReadOnly();
    }

    /// <summary>
    /// Exporterar konteringsrader som semikolonseparerad textfil (CSV).
    /// Format: Kostnadsställe;Konto;Debet;Kredit;Text;Period
    /// </summary>
    public string GenerateFile(IReadOnlyList<KonteringsRad> rader)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Kostnadsstalle;Konto;Debet;Kredit;Text;Period");

        foreach (var rad in rader)
        {
            sb.AppendLine(string.Join(';',
                rad.Kostnadsstalle,
                rad.Konto,
                rad.Debet.Amount.ToString("F2", CultureInfo.InvariantCulture),
                rad.Kredit.Amount.ToString("F2", CultureInfo.InvariantCulture),
                rad.Text,
                rad.Period));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Validerar att konteringarna är balanserade (total debet = total kredit).
    /// </summary>
    public bool ValidateBalance(IReadOnlyList<KonteringsRad> rader)
    {
        var totalDebet = rader.Sum(r => r.Debet.Amount);
        var totalKredit = rader.Sum(r => r.Kredit.Amount);

        // Tillåt 1 öre avrundningsdifferens
        return Math.Abs(totalDebet - totalKredit) < 0.01m;
    }

    private static string ExtractKostnadsstalle(PayrollResult result)
    {
        // Försök hämta kostnadsställe från första raden med satt kostnadssälle
        var ks = result.Rader.FirstOrDefault(r => !string.IsNullOrEmpty(r.Kostnadsstalle))?.Kostnadsstalle;
        return ks ?? "0000";
    }
}

/// <summary>
/// En enskild konteringsrad för Raindance.
/// </summary>
public sealed class KonteringsRad
{
    public string Kostnadsstalle { get; set; } = string.Empty;
    public string Konto { get; set; } = string.Empty;
    public Money Debet { get; set; } = Money.Zero;
    public Money Kredit { get; set; } = Money.Zero;
    public string Text { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
}
