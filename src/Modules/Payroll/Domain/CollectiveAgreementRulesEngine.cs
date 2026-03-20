using RegionHR.Agreements.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Payroll.Domain;

/// <summary>
/// Interface for the rules engine that handles collective agreement rules.
/// Supports both legacy enum-based lookup and DB-backed CollectiveAgreement entities.
/// </summary>
public interface ICollectiveAgreementRulesEngine
{
    // Legacy enum-based methods (backward compatible)
    Task<decimal> GetOBRateAsync(CollectiveAgreementType agreement, OBCategory category, DateOnly date, CancellationToken ct = default);
    Task<OvertimeRules> GetOvertimeRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);
    Task<VacationRules> GetVacationRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);
    Task<VacationRules> GetVacationRulesAsync(CollectiveAgreementType agreement, DateOnly date, int? fodelseAr, CancellationToken ct = default);
    Task<SickPayRules> GetSickPayRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);
    Task<JourRegler> GetJourReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);
    Task<BeredskapsRegler> GetBeredskapsReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);
    Task<ForaldraloneRegler> GetForaldraloneReglerAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default);

    // DB-backed overloads using CollectiveAgreement entity
    decimal GetOBRate(CollectiveAgreement avtal, OBCategory category, DateOnly date);
    OvertimeRules GetOvertimeRules(CollectiveAgreement avtal);
    VacationRules GetVacationRules(CollectiveAgreement avtal, DateOnly date, int? fodelseAr = null);
}

/// <summary>
/// Standard implementation of collective agreement rules.
/// Supports both hardcoded enum-based lookup (backward compatible)
/// and DB-backed CollectiveAgreement entity queries.
/// </summary>
public sealed class CollectiveAgreementRulesEngine : ICollectiveAgreementRulesEngine
{
    // OB-tillagg per AB (2025 satser) — legacy hardcoded rates
    private static readonly Dictionary<(CollectiveAgreementType, OBCategory), decimal> OBRates = new()
    {
        // AB - Allmanna bestammelser
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

    // === Legacy enum-based methods (backward compatible) ===

    public Task<decimal> GetOBRateAsync(CollectiveAgreementType agreement, OBCategory category, DateOnly date, CancellationToken ct = default)
    {
        if (category == OBCategory.Ingen)
            return Task.FromResult(0m);

        var rate = OBRates.GetValueOrDefault((agreement, category), 0m);
        return Task.FromResult(rate);
    }

    public Task<OvertimeRules> GetOvertimeRulesAsync(CollectiveAgreementType agreement, DateOnly date, CancellationToken ct = default)
    {
        // AB 25: Enkel overtid = 180% total, tillagg = 0.8x timlon
        // Kvalificerad overtid = 240% total, tillagg = 1.4x timlon
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
        // AB 25: Semesterdagar baserat pa alder under semesteraret
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

        // AB 25: Semestertillagg 0.43% per dag av manadslon
        // AB 25: 12% av total variabel lon under intjanandearet
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

    // === DB-backed overloads using CollectiveAgreement entity ===

    /// <summary>Get OB rate from a DB-loaded CollectiveAgreement entity</summary>
    public decimal GetOBRate(CollectiveAgreement avtal, OBCategory category, DateOnly date)
    {
        if (category == OBCategory.Ingen)
            return 0m;

        return avtal.HamtaOBSats(category, date);
    }

    /// <summary>Get overtime rules from a DB-loaded CollectiveAgreement entity</summary>
    public OvertimeRules GetOvertimeRules(CollectiveAgreement avtal)
    {
        var regel = avtal.OvertidsRegler.FirstOrDefault();
        if (regel is null)
        {
            return new OvertimeRules
            {
                EnkelOvertidFaktor = 0.8m,
                KvalificeradOvertidFaktor = 1.4m,
                MaxOvertidPerVecka = 48m,
                MaxOvertidPerManad = 50m,
                MaxOvertidPerAr = 200m,
                KomptidFaktor = 1.5m
            };
        }

        return new OvertimeRules
        {
            EnkelOvertidFaktor = regel.Multiplikator - 1.0m, // Convert total multiplier to addon
            KvalificeradOvertidFaktor = 1.4m,
            MaxOvertidPerVecka = 48m,
            MaxOvertidPerManad = 50m,
            MaxOvertidPerAr = regel.MaxPerAr,
            KomptidFaktor = 1.5m
        };
    }

    /// <summary>Get vacation rules from a DB-loaded CollectiveAgreement entity</summary>
    public VacationRules GetVacationRules(CollectiveAgreement avtal, DateOnly date, int? fodelseAr = null)
    {
        var regel = avtal.SemesterRegler.FirstOrDefault();
        var dagarPerAr = regel?.BasDagar ?? 25;

        if (regel is not null && fodelseAr.HasValue)
        {
            var alder = date.Year - fodelseAr.Value;
            dagarPerAr = alder switch
            {
                >= 50 => regel.ExtraDagarVid50,
                >= 40 => regel.ExtraDagarVid40,
                _ => regel.BasDagar
            };
        }

        return new VacationRules
        {
            DagarPerAr = dagarPerAr,
            SammaloneregelProcent = 0.80m,
            SemestertillaggProcent = 0.43m,
            VariabelLonSemesterProcent = 12.0m,
            MaxSparadeDagar = 5,
            TotalMaxSparade = 40,
            IntjanandeArStart = new DateOnly(date.Year - 1, 4, 1),
            IntjanandeArSlut = new DateOnly(date.Year, 3, 31)
        };
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
    /// <summary>Semestertillagg i procent per semesterdag av manadslon (AB 25: 0.43%)</summary>
    public decimal SemestertillaggProcent { get; set; }
    /// <summary>Procentsats for semesterlon pa variabel lon (AB 25: 12%)</summary>
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
/// Regler for jourersattning per kollektivavtal.
/// Passiv jour: vantetid med forpliktelse att kunna borja arbeta pa kort tid.
/// Aktiv jour: faktiskt utfort arbete under jourpass.
/// </summary>
public sealed class JourRegler
{
    /// <summary>Faktor av timlon for passiv jourtid (AB: 0.40)</summary>
    public decimal PassivTimlonFaktor { get; set; }

    /// <summary>Faktor av timlon for aktiv jourtid (AB: 1.5)</summary>
    public decimal AktivTimlonFaktor { get; set; }
}

/// <summary>
/// Regler for beredskapsersattning per kollektivavtal.
/// Beredskap: arbetstagaren ska vara antraffbar och kunna infinna sig inom viss tid.
/// AB 25: Hogre ersattning efter 125 timmars kumulativ beredskap.
/// </summary>
public sealed class BeredskapsRegler
{
    /// <summary>Faktor av timlon for beredskapstid (AB: 0.20)</summary>
    public decimal PassivTimlonFaktor { get; set; }

    /// <summary>Antal kumulativa beredskapstimmar innan hogre sats aktiveras (AB 25: 125h)</summary>
    public decimal HogNivaTimgrans { get; set; } = 125m;

    /// <summary>Faktor av timlon for beredskapstid efter troskel uppnatts (AB 25: 0.28)</summary>
    public decimal HogNivaPassivTimlonFaktor { get; set; } = 0.28m;
}

/// <summary>
/// Regler for foraldralonautfyllnad per kollektivavtal.
/// Utfyllnad utover Forsakringskassans ersattning (80%) under foraldraledighet.
/// </summary>
public sealed class ForaldraloneRegler
{
    /// <summary>Antal dagar med ratt till utfyllnad (AB: 180)</summary>
    public int DagarMedUtfyllnad { get; set; }

    /// <summary>Utfyllnadsprocent av dagslon (AB: 10%)</summary>
    public decimal UtfyllnadProcent { get; set; }
}
