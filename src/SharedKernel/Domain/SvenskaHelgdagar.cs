namespace RegionHR.SharedKernel.Domain;

/// <summary>
/// Beräknar svenska helgdagar inklusive rörliga helgdagar.
/// Används för OB-kategoribestämning, arbetsschemaberäkningar och arbetsdagsräkning.
/// </summary>
public static class SvenskaHelgdagar
{
    /// <summary>Avgör om ett datum är en svensk helgdag (röd dag).</summary>
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
        if (datum >= new DateOnly(year, 12, 24) && datum <= new DateOnly(year, 12, 26))
            return true;

        // Nyårsafton (31 dec) - Nyårsdagen (1 jan)
        if (datum == new DateOnly(year, 12, 31) || datum == new DateOnly(year, 1, 1))
            return true;

        // Påskafton - Annandag påsk
        if (datum >= paskdagen.AddDays(-1) && datum <= paskdagen.AddDays(1))
            return true;

        // Midsommarafton - Midsommardagen
        var midsommardagen = BeraknaMidsommardagen(year);
        if (datum >= midsommardagen.AddDays(-1) && datum <= midsommardagen)
            return true;

        // Kristi himmelsfärd
        if (datum == paskdagen.AddDays(39))
            return true;

        return false;
    }

    /// <summary>Returnerar alla helgdagar för ett givet år.</summary>
    public static IReadOnlyList<DateOnly> HelgdagarForAr(int year)
    {
        var paskdagen = BeraknaPaskdagen(year);
        var helgdagar = new List<DateOnly>
        {
            new(year, 1, 1),    // Nyårsdagen
            new(year, 1, 6),    // Trettondedag jul
            new(year, 5, 1),    // Första maj
            new(year, 6, 6),    // Nationaldagen
            new(year, 12, 24),  // Julafton
            new(year, 12, 25),  // Juldagen
            new(year, 12, 26),  // Annandag jul
            new(year, 12, 31),  // Nyårsafton

            paskdagen.AddDays(-2),   // Långfredagen
            paskdagen.AddDays(-1),   // Påskafton
            paskdagen,               // Påskdagen
            paskdagen.AddDays(1),    // Annandag påsk
            paskdagen.AddDays(39),   // Kristi himmelsfärdsdag
            paskdagen.AddDays(49),   // Pingstdagen
        };

        var midsommardagen = BeraknaMidsommardagen(year);
        helgdagar.Add(midsommardagen.AddDays(-1));  // Midsommarafton
        helgdagar.Add(midsommardagen);               // Midsommardagen

        helgdagar.Add(BeraknaAllaHelgonsDag(year));

        helgdagar.Sort();
        return helgdagar.AsReadOnly();
    }

    /// <summary>
    /// Bestäm OB-kategori baserat på datum och tid.
    /// OB-tider per AB 25 (Allmänna bestämmelser, från 2025-04-01).
    /// </summary>
    public static OBCategory BeraknaOBKategori(DateOnly datum, TimeOnly tid)
    {
        if (ArStorhelg(datum))
            return OBCategory.Storhelg;

        if (ArHelgdag(datum) || datum.DayOfWeek == DayOfWeek.Saturday || datum.DayOfWeek == DayOfWeek.Sunday)
            return OBCategory.Helg;

        if (tid >= new TimeOnly(22, 0) || tid < new TimeOnly(6, 0))
            return OBCategory.VardagNatt;

        var kvallStart = new TimeOnly(19, 0);
        if (datum.DayOfWeek == DayOfWeek.Friday && datum >= new DateOnly(2025, 4, 1))
            kvallStart = new TimeOnly(17, 0);

        if (tid >= kvallStart && tid < new TimeOnly(22, 0))
            return OBCategory.VardagKvall;

        return OBCategory.Ingen;
    }

    /// <summary>Beräknar påskdagen med Anonymous Gregorian-algoritmen.</summary>
    internal static DateOnly BeraknaPaskdagen(int year)
    {
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

    /// <summary>Midsommardagen: lördagen mellan 20 och 26 juni.</summary>
    internal static DateOnly BeraknaMidsommardagen(int year)
    {
        var datum = new DateOnly(year, 6, 20);
        while (datum.DayOfWeek != DayOfWeek.Saturday)
            datum = datum.AddDays(1);
        return datum;
    }

    /// <summary>Alla helgons dag: lördagen mellan 31 oktober och 6 november.</summary>
    internal static DateOnly BeraknaAllaHelgonsDag(int year)
    {
        var datum = new DateOnly(year, 10, 31);
        while (datum.DayOfWeek != DayOfWeek.Saturday)
            datum = datum.AddDays(1);
        return datum;
    }
}
