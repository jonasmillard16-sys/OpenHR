using RegionHR.Core.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Analytics;

/// <summary>
/// Regelbaserad beräkning av uppsägningsrisk per anställd.
///
/// Version 1: Enkel heuristik baserad på 4 signaler från befintlig domänmodell.
/// Detta är INTE en prediktiv AI-modell eller statistisk modell.
/// Poängen indikerar relativ risk baserat på kända mönster, inte sannolikhet.
///
/// Begränsningar i v1:
/// - Ingen sjukfrånvarodata (aggregering saknas i domänmodellen)
/// - Ingen lönehistorik (bara nuvarande lön, inga historiska ändringar)
/// - Ingen medarbetarsamtalsdata
/// - Ingen marknadslönejämförelse
/// - Bristyrke-matchning baseras på enkel string-matchning mot Befattningstitel
/// </summary>
public class FlightRiskService
{
    /// <summary>
    /// Beräknar uppsägningsrisk för alla anställda med aktiv anställning.
    /// Anställda utan aktiv anställning exkluderas.
    /// </summary>
    public List<FlightRiskResult> BeraknaForAlla(IEnumerable<Employee> employees)
    {
        var idag = DateOnly.FromDateTime(DateTime.Today);
        var results = new List<FlightRiskResult>();

        foreach (var emp in employees)
        {
            var aktivAnstallning = emp.AktivAnstallning(idag);
            if (aktivAnstallning is null)
                continue; // Exkludera — ingen aktiv anställning

            var result = Berakna(emp, aktivAnstallning, idag);
            results.Add(result);
        }

        return results.OrderByDescending(r => r.Poang).ToList();
    }

    private static FlightRiskResult Berakna(Employee emp, Employment anst, DateOnly idag)
    {
        var faktorer = new List<FlightRiskFaktor>();
        var totalPoang = 0;

        // Signal 1: Tenure högrisk-period (2-4 år)
        // Forskning visar att anställda som varit 2-4 år tenderar ha högre omsättning.
        var tenureAr = (idag.DayNumber - anst.Giltighetsperiod.Start.DayNumber) / 365.25;
        if (tenureAr >= 2.0 && tenureAr <= 4.0)
        {
            totalPoang += 25;
            faktorer.Add(new("Tenure i högrisk-period", 25,
                $"Anställd i {tenureAr:F1} år (2-4 år är statistisk högriskperiod för byte)",
                "Employment.Giltighetsperiod.Start"));
        }

        // Signal 2: Tidsbegränsad anställning
        // Vikariat, SAVA, säsong har per definition osäkrare framtid.
        if (anst.Anstallningsform is EmploymentType.Vikariat
            or EmploymentType.SAVA
            or EmploymentType.Sasongsanstallning)
        {
            totalPoang += 20;
            faktorer.Add(new("Tidsbegränsad anställning", 20,
                $"Anställningsform: {anst.Anstallningsform}",
                "Employment.Anstallningsform"));
        }

        // Signal 3: Bristyrke (enkel heuristik)
        // OBS: Detta är en grov string-matchning mot Befattningstitel, inte en
        // kvalificerad klassificering. Matchar "Sjuksköterska" och "Läkare"
        // (case-insensitive) som är kända bristyrken i svensk sjukvård.
        // Begränsning: Befattningstitel är fritext och kan vara null, felstavad,
        // eller använda andra benämningar (t.ex. "SSK", "Leg. sjuksköterska").
        if (ArBristyrke(anst.Befattningstitel))
        {
            totalPoang += 15;
            faktorer.Add(new("Bristyrke (heuristik)", 15,
                $"Befattning \"{anst.Befattningstitel}\" matchar bristyrkeslista",
                "Employment.Befattningstitel"));
        }

        // Signal 4: Deltid (<75%)
        // Deltidsanställda kan ha lägre engagemang eller söka heltid på annat håll.
        if (anst.Sysselsattningsgrad.Value < 75m)
        {
            totalPoang += 10;
            faktorer.Add(new("Deltidsanställning", 10,
                $"Sysselsättningsgrad: {anst.Sysselsattningsgrad.Value}%",
                "Employment.Sysselsattningsgrad"));
        }

        var niva = totalPoang switch
        {
            >= 51 => FlightRiskNiva.Hog,
            >= 31 => FlightRiskNiva.Medel,
            _ => FlightRiskNiva.Lag
        };

        return new FlightRiskResult(
            AnstallId: emp.Id,
            Namn: $"{emp.Fornamn} {emp.Efternamn}",
            Befattning: anst.Befattningstitel ?? "—",
            Enhet: anst.EnhetId,
            Anstallningsform: anst.Anstallningsform.ToString(),
            TenureAr: Math.Round(tenureAr, 1),
            Manadslon: anst.Manadslon.Amount,
            Poang: totalPoang,
            MaxPoang: 70,
            Niva: niva,
            Faktorer: faktorer);
    }

    /// <summary>
    /// Enkel heuristik: matchar kända bristyrken i svensk sjukvård.
    /// Case-insensitive, matchar delsträngar.
    /// Begränsning: Befattningstitel är fritext och kan vara null eller felstavad.
    /// </summary>
    private static bool ArBristyrke(string? befattning)
    {
        if (string.IsNullOrWhiteSpace(befattning))
            return false;

        var lower = befattning.ToLowerInvariant();
        return lower.Contains("sjuksköterska")
            || lower.Contains("sjukskoterska") // utan ö
            || lower.Contains("läkare")
            || lower.Contains("lakare"); // utan ä
    }
}

public record FlightRiskResult(
    EmployeeId AnstallId,
    string Namn,
    string Befattning,
    OrganizationId Enhet,
    string Anstallningsform,
    double TenureAr,
    decimal Manadslon,
    int Poang,
    int MaxPoang,
    FlightRiskNiva Niva,
    List<FlightRiskFaktor> Faktorer);

public record FlightRiskFaktor(
    string Signal,
    int Poang,
    string Beskrivning,
    string Datakalla);

public enum FlightRiskNiva { Lag, Medel, Hog }
