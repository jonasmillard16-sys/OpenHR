using RegionHR.SharedKernel.Domain;

namespace RegionHR.Scheduling.Domain;

/// <summary>
/// Trötthetsvärdering (0-100) för en anställd baserad på arbetsbelastning.
/// Högre poäng = högre trötthet/risk. Beräknas av systemet.
/// </summary>
public sealed class FatigueScore
{
    public Guid Id { get; private set; }
    public EmployeeId AnstallId { get; private set; }

    /// <summary>Trötthetspoäng 0 (utvilad) till 100 (utmattad).</summary>
    public int Poang { get; private set; }

    /// <summary>Antal konsekutiva arbetsdagar.</summary>
    public int KonsekutivaDagar { get; private set; }

    /// <summary>Antal nattpass senaste 7 dagarna.</summary>
    public int NattpassSenaste7Dagar { get; private set; }

    /// <summary>Totalt antal arbetstimmar senaste 7 dagarna.</summary>
    public decimal TotalTimmarSenaste7Dagar { get; private set; }

    /// <summary>Antal pass med kort vila (under 11h dygnsvila).</summary>
    public int KortVila { get; private set; }

    /// <summary>Antal helgdagar arbetade senaste 4 veckorna.</summary>
    public int HelgarbeteSenaste4Veckor { get; private set; }

    public DateTime BeraknadVid { get; private set; }

    private FatigueScore() { }

    /// <summary>
    /// Beräkna trötthetspoäng baserat på arbetsbelastningsfaktorer.
    /// </summary>
    public static FatigueScore Berakna(
        EmployeeId anstallId,
        int konsekutivaDagar,
        int nattpassSenaste7Dagar,
        decimal totalTimmarSenaste7Dagar,
        int kortVila,
        int helgarbeteSenaste4Veckor)
    {
        // Poängberäkning:
        // - Konsekutiva dagar: 5p per dag över 4
        // - Nattpass: 8p per nattpass
        // - Timmar: 2p per timme över 38.25h/vecka
        // - Kort vila: 10p per tillfälle
        // - Helgarbete: 3p per helgdag
        var poang = 0;
        poang += Math.Max(0, konsekutivaDagar - 4) * 5;
        poang += nattpassSenaste7Dagar * 8;
        poang += (int)Math.Max(0, (totalTimmarSenaste7Dagar - 38.25m) * 2);
        poang += kortVila * 10;
        poang += helgarbeteSenaste4Veckor * 3;

        poang = Math.Clamp(poang, 0, 100);

        return new FatigueScore
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Poang = poang,
            KonsekutivaDagar = konsekutivaDagar,
            NattpassSenaste7Dagar = nattpassSenaste7Dagar,
            TotalTimmarSenaste7Dagar = totalTimmarSenaste7Dagar,
            KortVila = kortVila,
            HelgarbeteSenaste4Veckor = helgarbeteSenaste4Veckor,
            BeraknadVid = DateTime.UtcNow
        };
    }
}
