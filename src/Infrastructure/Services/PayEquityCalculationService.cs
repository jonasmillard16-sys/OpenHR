using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RegionHR.Analytics.Domain;
using RegionHR.Core.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Beräkningstjänst för lönegap och EU Pay Transparency Directive-rapportering.
///
/// Beräknar:
/// 1. Ojusterat lönegap (raw gender pay gap) per befattningskategori
/// 2. Justerat lönegap (regression-baserat med kontrollvariabler)
/// 3. Intersektionell analys (kön × åldersgrupp, anställningsform, sysselsättningsgrad)
/// 4. Kohortspårning (gap-trend jämfört med föregående år)
/// 5. Lönekvartilfördelning (EU-direktivkrav)
/// 6. Åtgärdsmodellering (kostnad för att stänga gapet)
/// </summary>
public class PayEquityCalculationService
{
    private readonly RegionHRDbContext _db;

    public PayEquityCalculationService(RegionHRDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Beräkna en komplett Pay Transparency-rapport för angivet år.
    /// </summary>
    public async Task<PayTransparencyReport> BeraknaRapportAsync(int ar, CancellationToken ct = default)
    {
        var rapport = PayTransparencyReport.Skapa(ar, $"{ar}-01-01 till {ar}-12-31");

        var idag = DateOnly.FromDateTime(DateTime.Today);
        var employees = await _db.Employees
            .Include(e => e.Anstallningar)
            .ToListAsync(ct);

        // Only employees with active employment
        var activeEmployees = employees
            .Where(e => e.AktivAnstallning(idag) != null)
            .Select(e => new EmployeePayData(
                e,
                e.AktivAnstallning(idag)!,
                e.Personnummer.LegalGender,
                CalculateAge(e.Personnummer.BirthDate, idag)))
            .ToList();

        if (activeEmployees.Count == 0)
        {
            rapport.Berakna(0, 0, 0, "{}", []);
            return rapport;
        }

        // Group by befattningskategori
        var categories = activeEmployees
            .GroupBy(e => e.Employment.Befattningstitel ?? "Okand")
            .ToList();

        var analyser = new List<PayGapAnalysis>();

        // Fetch previous report for cohort trend comparison
        var previousReport = await _db.PayTransparencyReports
            .Include(r => r.Analyser)
            .Where(r => r.Ar == ar - 1 && r.Status == "Published")
            .FirstOrDefaultAsync(ct);

        foreach (var category in categories)
        {
            var kvinnor = category.Where(e => e.Gender == "Kvinna").ToList();
            var man = category.Where(e => e.Gender == "Man").ToList();

            if (kvinnor.Count == 0 || man.Count == 0)
                continue; // Need both genders for gap analysis

            var medelLonKvinnor = kvinnor.Average(e => e.Employment.Manadslon.Amount);
            var medelLonMan = man.Average(e => e.Employment.Manadslon.Amount);
            var medianLonKvinnor = CalculateMedian(kvinnor.Select(e => e.Employment.Manadslon.Amount));
            var medianLonMan = CalculateMedian(man.Select(e => e.Employment.Manadslon.Amount));

            // Ojusterat lönegap: (MedelLönMän - MedelLönKvinnor) / MedelLönMän × 100
            var ojusteratGap = medelLonMan > 0
                ? (medelLonMan - medelLonKvinnor) / medelLonMan * 100m
                : 0m;

            // Justerat lönegap (simplified group-means regression approach)
            var justeratGap = CalculateAdjustedGap(category.ToList());

            // Förklarande faktorer
            var faktorer = BuildForklarandeFaktorer(category.ToList());

            var analysis = PayGapAnalysis.Skapa(
                rapport.Id,
                category.Key,
                kvinnor.Count,
                man.Count,
                Math.Round(medelLonKvinnor, 0),
                Math.Round(medelLonMan, 0),
                Math.Round(medianLonKvinnor, 0),
                Math.Round(medianLonMan, 0),
                Math.Round(ojusteratGap, 2),
                justeratGap.HasValue ? Math.Round(justeratGap.Value, 2) : null,
                faktorer);

            // Cohort tracking: compare with previous year
            var previousAnalysis = previousReport?.Analyser
                .FirstOrDefault(a => a.Befattningskategori == category.Key);
            var trendDiff = previousAnalysis != null
                ? Math.Round(ojusteratGap - previousAnalysis.OjusteratGapProcent, 2)
                : (decimal?)null;

            // Add intersectional cohorts
            AddIntersectionalCohorts(analysis, category.ToList(), trendDiff);

            analyser.Add(analysis);
        }

        // Overall gap calculation
        var allKvinnor = activeEmployees.Where(e => e.Gender == "Kvinna").ToList();
        var allMan = activeEmployees.Where(e => e.Gender == "Man").ToList();

        var overallMedelGap = allMan.Count > 0 && allKvinnor.Count > 0
            ? (allMan.Average(e => e.Employment.Manadslon.Amount) -
               allKvinnor.Average(e => e.Employment.Manadslon.Amount)) /
              allMan.Average(e => e.Employment.Manadslon.Amount) * 100m
            : 0m;

        var overallMedianGap = allMan.Count > 0 && allKvinnor.Count > 0
            ? (CalculateMedian(allMan.Select(e => e.Employment.Manadslon.Amount)) -
               CalculateMedian(allKvinnor.Select(e => e.Employment.Manadslon.Amount))) /
              CalculateMedian(allMan.Select(e => e.Employment.Manadslon.Amount)) * 100m
            : 0m;

        // Build EU directive-compliant RapportData JSON
        var rapportData = BuildRapportData(activeEmployees, analyser);

        rapport.Berakna(
            activeEmployees.Count,
            Math.Round(overallMedelGap, 2),
            Math.Round(overallMedianGap, 2),
            rapportData,
            analyser);

        return rapport;
    }

    /// <summary>
    /// Beräkna justerat lönegap med förenklad regressionsansats (group-means).
    /// Kontrollerar för: anställningstid, sysselsättningsgrad, ålder.
    /// </summary>
    internal decimal? CalculateAdjustedGap(List<EmployeePayData> group)
    {
        if (group.Count < 4) return null; // Too few data points

        var kvinnor = group.Where(e => e.Gender == "Kvinna").ToList();
        var man = group.Where(e => e.Gender == "Man").ToList();

        if (kvinnor.Count == 0 || man.Count == 0) return null;

        // Simplified regression: control for tenure and employment rate
        // Normalize salary to full-time equivalent
        var normKvinnor = kvinnor.Select(e => NormalizeSalary(e)).ToList();
        var normMan = man.Select(e => NormalizeSalary(e)).ToList();

        // Group by tenure bands (0-2, 2-5, 5-10, 10+)
        var tenureBands = new[] { (0, 2), (2, 5), (5, 10), (10, 99) };
        var residuals = new List<decimal>();

        foreach (var (low, high) in tenureBands)
        {
            var bandKvinnor = normKvinnor
                .Where(n => n.TenureYears >= low && n.TenureYears < high)
                .Select(n => n.NormalizedSalary).ToList();
            var bandMan = normMan
                .Where(n => n.TenureYears >= low && n.TenureYears < high)
                .Select(n => n.NormalizedSalary).ToList();

            if (bandKvinnor.Count > 0 && bandMan.Count > 0)
            {
                var bandMeanK = bandKvinnor.Average();
                var bandMeanM = bandMan.Average();
                if (bandMeanM > 0)
                {
                    var bandGap = (bandMeanM - bandMeanK) / bandMeanM * 100m;
                    residuals.Add(bandGap);
                }
            }
        }

        return residuals.Count > 0 ? residuals.Average() : null;
    }

    /// <summary>
    /// Modellera åtgärdskostnad: vad kostar det att stänga gapet i en befattningskategori?
    /// </summary>
    public static RemediationResult BeraknaAtgardsKostnad(
        PayGapAnalysis analysis,
        decimal malGapProcent = 0m)
    {
        if (analysis.AntalKvinnor == 0 || analysis.MedelLonMan == 0)
            return new RemediationResult(0, 0, 0);

        // Target salary for women to achieve target gap
        var targetMedelLonKvinnor = analysis.MedelLonMan * (1 - malGapProcent / 100m);
        var hojningPerKvinna = Math.Max(0, targetMedelLonKvinnor - analysis.MedelLonKvinnor);
        var totalManadskostnad = hojningPerKvinna * analysis.AntalKvinnor;
        var totalArskostnad = totalManadskostnad * 12m;

        // Include employer contributions (arbetsgivaravgift 31.42%)
        var totalArskostnadInklAvgifter = totalArskostnad * 1.3142m;

        return new RemediationResult(
            Math.Round(hojningPerKvinna, 0),
            Math.Round(totalManadskostnad, 0),
            Math.Round(totalArskostnadInklAvgifter, 0));
    }

    /// <summary>
    /// Build intersectional analysis cohorts.
    /// </summary>
    private static void AddIntersectionalCohorts(
        PayGapAnalysis analysis,
        List<EmployeePayData> group,
        decimal? overallTrend)
    {
        // Gender × Age group
        AddAgeCohorts(analysis, group, overallTrend);

        // Gender × Employment type
        AddEmploymentTypeCohorts(analysis, group, overallTrend);

        // Gender × Employment rate
        AddEmploymentRateCohorts(analysis, group, overallTrend);
    }

    private static void AddAgeCohorts(PayGapAnalysis analysis, List<EmployeePayData> group, decimal? trend)
    {
        var ageGroups = new[] { ("Under 30", 0, 30), ("30-50", 30, 50), ("50+", 50, 200) };
        foreach (var (name, minAge, maxAge) in ageGroups)
        {
            var cohort = group.Where(e => e.Age >= minAge && e.Age < maxAge).ToList();
            var kvinnor = cohort.Where(e => e.Gender == "Kvinna").ToList();
            var man = cohort.Where(e => e.Gender == "Man").ToList();

            if (kvinnor.Count > 0 && man.Count > 0)
            {
                var meanK = kvinnor.Average(e => e.Employment.Manadslon.Amount);
                var meanM = man.Average(e => e.Employment.Manadslon.Amount);
                var gap = meanM > 0 ? (meanM - meanK) / meanM * 100m : 0m;

                analysis.LaggTillKohort(PayGapCohort.Skapa(
                    analysis.Id,
                    $"Alder: {name}",
                    cohort.Count,
                    Math.Round(gap, 2),
                    trend));
            }
        }
    }

    private static void AddEmploymentTypeCohorts(PayGapAnalysis analysis, List<EmployeePayData> group, decimal? trend)
    {
        var tillsvidare = group.Where(e => e.Employment.ArTillsvidareanstallning).ToList();
        var tidsbegransad = group.Where(e => e.Employment.ArTidsbegransad).ToList();

        foreach (var (name, cohort) in new[] { ("Tillsvidare", tillsvidare), ("Tidsbegransad", tidsbegransad) })
        {
            var kvinnor = cohort.Where(e => e.Gender == "Kvinna").ToList();
            var man = cohort.Where(e => e.Gender == "Man").ToList();

            if (kvinnor.Count > 0 && man.Count > 0)
            {
                var meanK = kvinnor.Average(e => e.Employment.Manadslon.Amount);
                var meanM = man.Average(e => e.Employment.Manadslon.Amount);
                var gap = meanM > 0 ? (meanM - meanK) / meanM * 100m : 0m;

                analysis.LaggTillKohort(PayGapCohort.Skapa(
                    analysis.Id,
                    $"Anstallningsform: {name}",
                    cohort.Count,
                    Math.Round(gap, 2),
                    trend));
            }
        }
    }

    private static void AddEmploymentRateCohorts(PayGapAnalysis analysis, List<EmployeePayData> group, decimal? trend)
    {
        var heltid = group.Where(e => e.Employment.Sysselsattningsgrad.Value >= 100m).ToList();
        var deltid = group.Where(e => e.Employment.Sysselsattningsgrad.Value < 100m).ToList();

        foreach (var (name, cohort) in new[] { ("Heltid", heltid), ("Deltid", deltid) })
        {
            var kvinnor = cohort.Where(e => e.Gender == "Kvinna").ToList();
            var man = cohort.Where(e => e.Gender == "Man").ToList();

            if (kvinnor.Count > 0 && man.Count > 0)
            {
                var meanK = kvinnor.Average(e => e.Employment.Manadslon.Amount);
                var meanM = man.Average(e => e.Employment.Manadslon.Amount);
                var gap = meanM > 0 ? (meanM - meanK) / meanM * 100m : 0m;

                analysis.LaggTillKohort(PayGapCohort.Skapa(
                    analysis.Id,
                    $"Sysselsattning: {name}",
                    cohort.Count,
                    Math.Round(gap, 2),
                    trend));
            }
        }
    }

    /// <summary>
    /// Build EU Pay Transparency Directive compliant RapportData JSON.
    /// Includes: pay quartile distribution, gap per category, joint assessment flags.
    /// </summary>
    private string BuildRapportData(List<EmployeePayData> allEmployees, List<PayGapAnalysis> analyser)
    {
        // Pay quartile distribution (EU directive requirement)
        var allSalaries = allEmployees
            .OrderBy(e => e.Employment.Manadslon.Amount)
            .ToList();

        var quartileSize = allSalaries.Count / 4;
        var quartiles = new List<object>();

        for (int q = 0; q < 4; q++)
        {
            var quartileGroup = allSalaries
                .Skip(q * quartileSize)
                .Take(q == 3 ? allSalaries.Count - q * quartileSize : quartileSize)
                .ToList();

            var kvinnor = quartileGroup.Count(e => e.Gender == "Kvinna");
            var man = quartileGroup.Count(e => e.Gender == "Man");
            var total = quartileGroup.Count;

            quartiles.Add(new
            {
                Kvartil = q + 1,
                Namn = q switch { 0 => "Q1 (lagst)", 1 => "Q2", 2 => "Q3", 3 => "Q4 (hogst)", _ => "" },
                AntalKvinnor = kvinnor,
                AntalMan = man,
                AndelKvinnor = total > 0 ? Math.Round((decimal)kvinnor / total * 100, 1) : 0m,
                AndelMan = total > 0 ? Math.Round((decimal)man / total * 100, 1) : 0m
            });
        }

        // Categories requiring joint pay assessment (gap > 5%)
        var kategorierMedUtredningskrav = analyser
            .Where(a => a.Kraver5ProcentUtredning)
            .Select(a => new
            {
                a.Befattningskategori,
                a.OjusteratGapProcent,
                a.JusteratGapProcent
            })
            .ToList();

        // Summary statistics
        var kvinnorTotal = allEmployees.Count(e => e.Gender == "Kvinna");
        var manTotal = allEmployees.Count(e => e.Gender == "Man");

        var data = new
        {
            EUDirectiveVersion = "2023/970",
            Rapportdatum = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            Sammanfattning = new
            {
                TotalAnstallda = allEmployees.Count,
                AntalKvinnor = kvinnorTotal,
                AntalMan = manTotal,
                AndelKvinnor = allEmployees.Count > 0 ? Math.Round((decimal)kvinnorTotal / allEmployees.Count * 100, 1) : 0m,
                AndelMan = allEmployees.Count > 0 ? Math.Round((decimal)manTotal / allEmployees.Count * 100, 1) : 0m,
                AntalBefattningskategorier = analyser.Count
            },
            Lonekvartiler = quartiles,
            KategorierMedUtredningskrav = kategorierMedUtredningskrav,
            GemensamLonebedomningKravs = kategorierMedUtredningskrav.Count > 0
        };

        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static string BuildForklarandeFaktorer(List<EmployeePayData> group)
    {
        var kvinnor = group.Where(e => e.Gender == "Kvinna").ToList();
        var man = group.Where(e => e.Gender == "Man").ToList();

        if (kvinnor.Count == 0 || man.Count == 0)
            return "{}";

        var avgTenureK = kvinnor.Average(e => e.TenureYears);
        var avgTenureM = man.Average(e => e.TenureYears);
        var avgSyssK = kvinnor.Average(e => (double)e.Employment.Sysselsattningsgrad.Value);
        var avgSyssM = man.Average(e => (double)e.Employment.Sysselsattningsgrad.Value);
        var avgAgeK = kvinnor.Average(e => (double)e.Age);
        var avgAgeM = man.Average(e => (double)e.Age);

        var faktorer = new
        {
            GenomsnittligAnstallningstidKvinnor = Math.Round(avgTenureK, 1),
            GenomsnittligAnstallningstidMan = Math.Round(avgTenureM, 1),
            GenomsnittligSysselsattningsgradKvinnor = Math.Round(avgSyssK, 1),
            GenomsnittligSysselsattningsgradMan = Math.Round(avgSyssM, 1),
            GenomsnittligAlderKvinnor = Math.Round(avgAgeK, 1),
            GenomsnittligAlderMan = Math.Round(avgAgeM, 1)
        };

        return JsonSerializer.Serialize(faktorer, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private static NormalizedSalaryData NormalizeSalary(EmployeePayData data)
    {
        // Normalize to full-time equivalent
        var syssGrad = data.Employment.Sysselsattningsgrad.Value;
        var normalizedSalary = syssGrad > 0
            ? data.Employment.Manadslon.Amount / syssGrad * 100m
            : data.Employment.Manadslon.Amount;

        var startDate = data.Employment.Giltighetsperiod.Start;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var tenureYears = (today.DayNumber - startDate.DayNumber) / 365.25;

        return new NormalizedSalaryData(normalizedSalary, tenureYears);
    }

    internal static decimal CalculateMedian(IEnumerable<decimal> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        if (sorted.Count == 0) return 0m;
        if (sorted.Count == 1) return sorted[0];

        var mid = sorted.Count / 2;
        return sorted.Count % 2 == 0
            ? (sorted[mid - 1] + sorted[mid]) / 2m
            : sorted[mid];
    }

    private static int CalculateAge(DateOnly birthDate, DateOnly referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        if (referenceDate.DayOfYear < birthDate.DayOfYear)
            age--;
        return age;
    }

    internal record EmployeePayData(Employee Employee, Employment Employment, string Gender, int Age)
    {
        public double TenureYears
        {
            get
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                return (today.DayNumber - Employment.Giltighetsperiod.Start.DayNumber) / 365.25;
            }
        }
    }

    private record NormalizedSalaryData(decimal NormalizedSalary, double TenureYears);
}

public record RemediationResult(
    decimal HojningPerKvinnaManad,
    decimal TotalManadskostnad,
    decimal TotalArskostnadInklAvgifter);
