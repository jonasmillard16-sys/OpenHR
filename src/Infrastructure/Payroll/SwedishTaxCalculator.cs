namespace RegionHR.Infrastructure.Payroll;

public class SwedishTaxCalculator
{
    // Swedish municipal tax 2026 (average ~32%)
    public decimal KommunalSkatt { get; set; } = 0.3213m;

    // State tax threshold (2026: ~613,900 kr/year = ~51,158/month)
    public decimal StatligSkattGrans { get; set; } = 51158m;
    public decimal StatligSkattSats { get; set; } = 0.20m;

    // Employer contributions (arbetsgivaravgift 2026: 31.42%)
    public decimal Arbetsgivaravgift { get; set; } = 0.3142m;

    // Reduced rate for employees born 1960 or later who turned 66
    public decimal ReduceradAvgift { get; set; } = 0.1021m;

    public LoneBerakning Berakna(decimal brutto, int fodelsear = 1985)
    {
        var alder = DateTime.Today.Year - fodelsear;
        var kommunalSkatt = Math.Round(brutto * KommunalSkatt, 0);
        var statligSkatt = brutto > StatligSkattGrans
            ? Math.Round((brutto - StatligSkattGrans) * StatligSkattSats, 0)
            : 0m;
        var totalSkatt = kommunalSkatt + statligSkatt;
        var netto = brutto - totalSkatt;
        var avgiftsSats = alder >= 66 ? ReduceradAvgift : Arbetsgivaravgift;
        var arbetsgivaravgift = Math.Round(brutto * avgiftsSats, 0);
        var semesterTillagg = Math.Round(brutto * 0.0043m * 12, 0); // 0.43% per month

        return new LoneBerakning(
            Brutto: brutto,
            KommunalSkatt: kommunalSkatt,
            StatligSkatt: statligSkatt,
            TotalSkatt: totalSkatt,
            Netto: netto,
            Arbetsgivaravgift: arbetsgivaravgift,
            TotalKostnad: brutto + arbetsgivaravgift,
            SemesterTillagg: semesterTillagg,
            Skattesats: Math.Round((totalSkatt / brutto) * 100, 1)
        );
    }
}

public record LoneBerakning(
    decimal Brutto, decimal KommunalSkatt, decimal StatligSkatt,
    decimal TotalSkatt, decimal Netto, decimal Arbetsgivaravgift,
    decimal TotalKostnad, decimal SemesterTillagg, decimal Skattesats);
