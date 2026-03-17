using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Domain;

/// <summary>
/// Beräknar svenska helgdagar inklusive rörliga helgdagar.
/// Används för OB-kategoribestämning och arbetsschemaberäkningar.
/// </summary>
public static class SvenskaHelgdagar
{
    /// <summary>
    /// Avgör om ett datum är en svensk helgdag (röd dag).
    /// </summary>
    public static bool ArHelgdag(DateOnly datum)
    {
        var helgdagar = HelgdagarForAr(datum.Year);
        return helgdagar.Contains(datum);
    }

    /// <summary>
    /// Avgör om ett datum infaller under en storhelgsperiod.
    /// Storhelg: julafton-annandag jul, nyårsafton-nyårsdagen,
    /// påskafton-annandag påsk, midsommarafton-midsommardagen,
    /// Kristi himmelsfärd.
    /// </summary>
    public static bool ArStorhelg(DateOnly datum)
    {
        var year = datum.Year;
        var paskdagen = BeraknaPaskdagen(year);

        // Julafton (24 dec) - Annandag jul (26 dec)
        var julafton = new DateOnly(year, 12, 24);
        var annandagJul = new DateOnly(year, 12, 26);
        if (datum >= julafton && datum <= annandagJul)
            return true;

        // Nyårsafton (31 dec) - Nyårsdagen (1 jan)
        // Nyårsafton samma år
        if (datum == new DateOnly(year, 12, 31))
            return true;
        // Nyårsdagen
        if (datum == new DateOnly(year, 1, 1))
            return true;

        // Påskafton - Annandag påsk
        var paskafton = paskdagen.AddDays(-1);
        var annandagPask = paskdagen.AddDays(1);
        if (datum >= paskafton && datum <= annandagPask)
            return true;

        // Midsommarafton - Midsommardagen
        var midsommardagen = BeraknaMidsommardagen(year);
        var midsommarafton = midsommardagen.AddDays(-1);
        if (datum >= midsommarafton && datum <= midsommardagen)
            return true;

        // Kristi himmelsfärd (torsdag, 39 dagar efter påskdagen)
        var kristiHimmelsfardsdag = paskdagen.AddDays(39);
        if (datum == kristiHimmelsfardsdag)
            return true;

        return false;
    }

    /// <summary>
    /// Returnerar alla helgdagar för ett givet år.
    /// </summary>
    public static IReadOnlyList<DateOnly> HelgdagarForAr(int year)
    {
        var paskdagen = BeraknaPaskdagen(year);
        var helgdagar = new List<DateOnly>
        {
            // Fasta helgdagar
            new(year, 1, 1),    // Nyårsdagen
            new(year, 1, 6),    // Trettondedag jul
            new(year, 5, 1),    // Första maj
            new(year, 6, 6),    // Nationaldagen
            new(year, 12, 24),  // Julafton
            new(year, 12, 25),  // Juldagen
            new(year, 12, 26),  // Annandag jul
            new(year, 12, 31),  // Nyårsafton

            // Rörliga helgdagar baserade på påsk
            paskdagen.AddDays(-2),   // Långfredagen
            paskdagen.AddDays(-1),   // Påskafton
            paskdagen,               // Påskdagen
            paskdagen.AddDays(1),    // Annandag påsk
            paskdagen.AddDays(39),   // Kristi himmelsfärdsdag
            paskdagen.AddDays(49),   // Pingstdagen
        };

        // Midsommarafton och midsommardagen
        var midsommardagen = BeraknaMidsommardagen(year);
        helgdagar.Add(midsommardagen.AddDays(-1));  // Midsommarafton
        helgdagar.Add(midsommardagen);               // Midsommardagen

        // Alla helgons dag (lördag mellan 31 okt och 6 nov)
        helgdagar.Add(BeraknaAllaHelgonsDag(year));

        helgdagar.Sort();
        return helgdagar.AsReadOnly();
    }

    /// <summary>
    /// Bestäm OB-kategori baserat på datum och tid.
    /// Storhelg > helg > vardag kväll/natt > ingen.
    ///
    /// OB-tider per AB 25 (Allmänna bestämmelser, från 2025-04-01):
    /// - Vardagkväll: mån-tors 19:00-22:00, fre 17:00-22:00 (AB 25: fredag OB från 17:00)
    /// - Vardagnatt: mån-fre 22:00-06:00
    /// - Helg: lör 07:00 - sön 24:00 (samt helgdagar)
    /// - Storhelg: storhelgsperioder dygnet runt
    ///
    /// Före 2025-04-01: fredag kväll OB börjar kl 19:00 (som övriga vardagar).
    /// </summary>
    public static OBCategory BeraknaOBKategori(DateOnly datum, TimeOnly tid)
    {
        // Storhelg har högst prioritet
        if (ArStorhelg(datum))
            return OBCategory.Storhelg;

        // Helgdag (röd dag som inte är storhelg) eller lördag/söndag
        if (ArHelgdag(datum) || datum.DayOfWeek == DayOfWeek.Saturday || datum.DayOfWeek == DayOfWeek.Sunday)
            return OBCategory.Helg;

        // Vardagstider (måndag-fredag)
        // Natt: 22:00-06:00
        if (tid >= new TimeOnly(22, 0) || tid < new TimeOnly(6, 0))
            return OBCategory.VardagNatt;

        // AB 25: Fredag kväll OB från 17:00 (istället för 19:00), gäller från 2025-04-01
        var kvallStart = new TimeOnly(19, 0);
        if (datum.DayOfWeek == DayOfWeek.Friday && datum >= new DateOnly(2025, 4, 1))
        {
            kvallStart = new TimeOnly(17, 0);
        }

        // Kväll: kvallStart-22:00
        if (tid >= kvallStart && tid < new TimeOnly(22, 0))
            return OBCategory.VardagKvall;

        // Dagtid vardag: ingen OB
        return OBCategory.Ingen;
    }

    /// <summary>
    /// Beräknar påskdagen med Anonymous Gregorian-algoritmen.
    /// </summary>
    internal static DateOnly BeraknaPaskdagen(int year)
    {
        // Anonymous Gregorian algorithm (Meeus/Jones/Butcher)
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day = ((h + l - 7 * m + 114) % 31) + 1;

        return new DateOnly(year, month, day);
    }

    /// <summary>
    /// Midsommardagen: lördagen mellan 20 och 26 juni.
    /// </summary>
    internal static DateOnly BeraknaMidsommardagen(int year)
    {
        var datum = new DateOnly(year, 6, 20);
        while (datum.DayOfWeek != DayOfWeek.Saturday)
            datum = datum.AddDays(1);
        return datum;
    }

    /// <summary>
    /// Alla helgons dag: lördagen mellan 31 oktober och 6 november.
    /// </summary>
    internal static DateOnly BeraknaAllaHelgonsDag(int year)
    {
        var datum = new DateOnly(year, 10, 31);
        while (datum.DayOfWeek != DayOfWeek.Saturday)
            datum = datum.AddDays(1);
        return datum;
    }
}
