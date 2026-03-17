using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Domain;

/// <summary>
/// Interface för regelmotorn som hanterar kollektivavtalsregler.
/// Regler lagras som versionerad JSONB i databasen.
/// </summary>
public interface ICollectiveAgreementRulesEngine
{
    Task<decimal> GetOBRateAsync(CollectiveAgreementType agreement, OBCategory category, DateOnly date, CancellationToken ct = default);
    Task<OvertimeRules> GetOvertimeRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);
    Task<VacationRules> GetVacationRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);
    Task<VacationRules> GetVacationRulesAsync(CollectiveAgreementType agreement, DateOnly date, int? fodelseAr, CancellationToken ct = default);
    Task<SickPayRules> GetSickPayRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);
    Task<JourRegler> GetJourReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);
    Task<BeredskapsRegler> GetBeredskapsReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);
    Task<ForaldraloneRegler> GetForaldraloneReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);
}

/// <summary>
/// Standardimplementation av kollektivavtalsregler.
/// I produktion läser denna från JSONB i PostgreSQL.
/// </summary>
public sealed class CollectiveAgreementRulesEngine : ICollectiveAgreementRulesEngine
{
    // OB-tillägg per AB (2025 satser)
    private static readonly Dictionary<(CollectiveAgreementType, OBCategory), decimal> OBRates = new()
    {
        // AB - Allmänna bestämmelser
        { (CollectiveAgreementType.AB, OBCategory.VardagKvall), 126.50m },
        { (CollectiveAgreementType.AB, OBCategory.VardagNatt), 152.00m },
        { (CollectiveAgreementType.AB, OBCategory.Helg), 89.00m },
        { (CollectiveAgreementType.AB, OBCategory.Storhelg), 195.00m },

        // HOK
        { (CollectiveAgreementType.HOK, OBCategory.VardagKvall), 120.00m },
        { (CollectiveAgreementType.HOK, OBCategory.VardagNatt), 145.00m },
        { (CollectiveAgreementType.HOK, OBCategory.Helg), 85.00m },
        { (CollectiveAgreementType.HOK, OBCategory.Storhelg), 185.00m },
    };

    public Task<decimal> GetOBRateAsync(CollectiveAgreementType agreement, OBCategory category, DateOnly date, CancellationToken ct = default)
    {
        if (category == OBCategory.Ingen)
            return Task.FromResult(0m);

        var rate = OBRates.GetValueOrDefault((agreement, category), 0m);
        return Task.FromResult(rate);
    }

    public Task<OvertimeRules> GetOvertimeRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
    {
        // AB 25: Enkel övertid = 180% total, tillägg = 0.8x timlön
        // Kvalificerad övertid = 240% total, tillägg = 1.4x timlön
        return Task.FromResult(new OvertimeRules
        {
            EnkelOvertidFaktor = agreement == CollectiveAgreementType.AB ? 0.8m : 0.8m,
            KvalificeradOvertidFaktor = agreement == CollectiveAgreementType.AB ? 1.4m : 1.4m,
            MaxOvertidPerVecka = 48m,
            MaxOvertidPerManad = 50m,
            MaxOvertidPerAr = 200m,
            KomptidFaktor = 1.5m
        });
    }

    public Task<VacationRules> GetVacationRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
    {
        return GetVacationRulesAsync(agreement, date, fodelseAr: null, ct);
    }

    public Task<VacationRules> GetVacationRulesAsync(CollectiveAgreementType agreement, DateOnly date, int? fodelseAr, CancellationToken ct = default)
    {
        // AB 25: Semesterdagar baserat på ålder under semesteråret
        var dagarPerAr = 25;
        if (agreement == CollectiveAgreementType.AB && fodelseAr.HasValue)
        {
            var alder = date.Year - fodelseAr.Value;
            dagarPerAr = alder switch
            {
                >= 50 => 32,
                >= 40 => 31,
                _ => 25
            };
        }

        // AB 25: Semestertillägg 0.43% per dag av månadslön
        // AB 25: 12% av total variabel lön under intjänandeåret
        return Task.FromResult(new VacationRules
        {
            DagarPerAr = dagarPerAr,
            SammaloneregelProcent = 0.80m,
            SemestertillaggProcent = 0.43m,
            VariabelLonSemesterProcent = 12.0m,
            MaxSparadeDagar = 5,
            TotalMaxSparade = 40,
            IntjanandeArStart = new DateOnly(date.Year - 1, 4, 1),
            IntjanandeArSlut = new DateOnly(date.Year, 3, 31)
        });
    }

    public Task<SickPayRules> GetSickPayRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
    {
        return Task.FromResult(new SickPayRules
        {
            KarensavdragProcent = 20m,
            SjuklonDag2Till14Procent = 80m,
            FKAnmalanEfterDag = 14,
            MaxSjuklonedagar = 14,
            LakarintygEfterDag = 7
        });
    }

    public Task<JourRegler> GetJourReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
    {
        return Task.FromResult(new JourRegler
        {
            PassivTimlonFaktor = agreement switch
            {
                CollectiveAgreementType.AB => 0.40m,
                CollectiveAgreementType.HOK => 0.38m,
                _ => 0.40m
            },
            AktivTimlonFaktor = agreement switch
            {
                CollectiveAgreementType.AB => 1.5m,
                CollectiveAgreementType.HOK => 1.5m,
                _ => 1.5m
            }
        });
    }

    public Task<BeredskapsRegler> GetBeredskapsReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
    {
        return Task.FromResult(new BeredskapsRegler
        {
            PassivTimlonFaktor = agreement switch
            {
                CollectiveAgreementType.AB => 0.20m,
                CollectiveAgreementType.HOK => 0.18m,
                _ => 0.20m
            },
            HogNivaTimgrans = agreement switch
            {
                CollectiveAgreementType.AB => 125m,
                _ => 125m
            },
            HogNivaPassivTimlonFaktor = agreement switch
            {
                CollectiveAgreementType.AB => 0.28m,
                CollectiveAgreementType.HOK => 0.25m,
                _ => 0.28m
            }
        });
    }

    public Task<ForaldraloneRegler> GetForaldraloneReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
    {
        return Task.FromResult(new ForaldraloneRegler
        {
            DagarMedUtfyllnad = agreement switch
            {
                CollectiveAgreementType.AB => 180,
                CollectiveAgreementType.HOK => 180,
                _ => 180
            },
            UtfyllnadProcent = agreement switch
            {
                CollectiveAgreementType.AB => 0.10m,
                CollectiveAgreementType.HOK => 0.10m,
                _ => 0.10m
            }
        });
    }
}

public sealed class OvertimeRules
{
    public decimal EnkelOvertidFaktor { get; set; }
    public decimal KvalificeradOvertidFaktor { get; set; }
    public decimal MaxOvertidPerVecka { get; set; }
    public decimal MaxOvertidPerManad { get; set; }
    public decimal MaxOvertidPerAr { get; set; }
    public decimal KomptidFaktor { get; set; }
}

public sealed class VacationRules
{
    public int DagarPerAr { get; set; }
    public decimal SammaloneregelProcent { get; set; }
    /// <summary>Semestertillägg i procent per semesterdag av månadslön (AB 25: 0.43%)</summary>
    public decimal SemestertillaggProcent { get; set; }
    /// <summary>Procentsats för semesterlön på variabel lön (AB 25: 12%)</summary>
    public decimal VariabelLonSemesterProcent { get; set; }
    public int MaxSparadeDagar { get; set; }
    public int TotalMaxSparade { get; set; }
    public DateOnly IntjanandeArStart { get; set; }
    public DateOnly IntjanandeArSlut { get; set; }
}

public sealed class SickPayRules
{
    public decimal KarensavdragProcent { get; set; }
    public decimal SjuklonDag2Till14Procent { get; set; }
    public int FKAnmalanEfterDag { get; set; }
    public int MaxSjuklonedagar { get; set; }
    public int LakarintygEfterDag { get; set; }
}

/// <summary>
/// Regler för jourersättning per kollektivavtal.
/// Passiv jour: väntetid med förpliktelse att kunna börja arbeta på kort tid.
/// Aktiv jour: faktiskt utfört arbete under jourpass.
/// </summary>
public sealed class JourRegler
{
    /// <summary>Faktor av timlön för passiv jourtid (AB: 0.40)</summary>
    public decimal PassivTimlonFaktor { get; set; }

    /// <summary>Faktor av timlön för aktiv jourtid (AB: 1.5)</summary>
    public decimal AktivTimlonFaktor { get; set; }
}

/// <summary>
/// Regler för beredskapsersättning per kollektivavtal.
/// Beredskap: arbetstagaren ska vara anträffbar och kunna infinna sig inom viss tid.
/// AB 25: Högre ersättning efter 125 timmars kumulativ beredskap.
/// </summary>
public sealed class BeredskapsRegler
{
    /// <summary>Faktor av timlön för beredskapstid (AB: 0.20)</summary>
    public decimal PassivTimlonFaktor { get; set; }

    /// <summary>Antal kumulativa beredskapstimmar innan högre sats aktiveras (AB 25: 125h)</summary>
    public decimal HogNivaTimgrans { get; set; } = 125m;

    /// <summary>Faktor av timlön för beredskapstid efter tröskel uppnåtts (AB 25: 0.28)</summary>
    public decimal HogNivaPassivTimlonFaktor { get; set; } = 0.28m;
}

/// <summary>
/// Regler för föräldralöneutfyllnad per kollektivavtal.
/// Utfyllnad utöver Försäkringskassans ersättning (80%) under föräldraledighet.
/// </summary>
public sealed class ForaldraloneRegler
{
    /// <summary>Antal dagar med rätt till utfyllnad (AB: 180)</summary>
    public int DagarMedUtfyllnad { get; set; }

    /// <summary>Utfyllnadsprocent av dagslön (AB: 10%)</summary>
    public decimal UtfyllnadProcent { get; set; }
}
