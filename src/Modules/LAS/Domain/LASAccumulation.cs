using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.LAS.Domain;

/// <summary>
/// LAS-ackumulering per anställd.
///
/// Regler:
/// - SAVA: max 12 månader i en 5-årsperiod (sedan 2022-10-01), konvertering vid överskridande
/// - Vikariat: max 2 år i en 5-årsperiod
/// - 3-i-månad-regeln: 3+ SAVA-avtal i samma kalendermånad → mellanliggande perioder räknas
/// - Företrädesrätt: 9 månader efter anställningens slut
/// </summary>
public sealed class LASAccumulation : AggregateRoot<Guid>
{
    // Konstanter
    public const int SAVA_MAX_DAGAR_5AR = 365;      // ~12 månader
    public const int VIKARIAT_MAX_DAGAR_5AR = 730;   // 2 år
    public const int REFERENSFONSTER_AR = 5;
    public const int FORETRADESRATT_MANADER = 9;

    // Företrädesrätt: tröskelvärden i en 3-årsperiod
    // SAVA: 9 månader (~274 dagar) i 3 år
    // Tillsvidare/vikariat: 12 månader (~365 dagar) i 3 år
    public const int FORETRADESRATT_SAVA_MANADER = 9;
    public const int FORETRADESRATT_SAVA_DAGAR = 274;        // ~9 månader
    public const int FORETRADESRATT_VIKARIAT_MANADER = 12;
    public const int FORETRADESRATT_VIKARIAT_DAGAR = 365;     // ~12 månader
    public const int FORETRADESRATT_REFERENSFONSTER_AR = 3;

    // Tröskelvärden för alarm
    public const int SAVA_ALARM_10_MANADER = 305;
    public const int SAVA_ALARM_11_MANADER = 335;
    public const int SAVA_ALARM_12_MANADER = 365;

    public EmployeeId AnstallId { get; private set; }
    public EmploymentType Anstallningsform { get; private set; }
    public int AckumuleradeDagar { get; private set; }
    public DateOnly ReferensfonsterStart { get; private set; }
    public DateOnly ReferensfonsterSlut { get; private set; }
    public LASStatus Status { get; private set; }
    public DateOnly? KonverteringsDatum { get; private set; }
    public bool HarForetradesratt { get; private set; }
    public DateOnly? ForetradesrattUtgar { get; private set; }

    private readonly List<LASEvent> _handelser = [];
    public IReadOnlyList<LASEvent> Handelser => _handelser.AsReadOnly();

    private readonly List<LASPeriod> _perioder = [];
    public IReadOnlyList<LASPeriod> Perioder => _perioder.AsReadOnly();

    private LASAccumulation() { }

    public static LASAccumulation Skapa(EmployeeId anstallId, EmploymentType anstallningsform)
    {
        if (anstallningsform is not (EmploymentType.SAVA or EmploymentType.Vikariat))
            throw new ArgumentException("LAS-ackumulering gäller bara SAVA och vikariat");

        var now = DateOnly.FromDateTime(DateTime.Today);
        return new LASAccumulation
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Anstallningsform = anstallningsform,
            AckumuleradeDagar = 0,
            ReferensfonsterStart = now.AddYears(-REFERENSFONSTER_AR),
            ReferensfonsterSlut = now,
            Status = LASStatus.UnderGrans
        };
    }

    /// <summary>Registrera en anställningsperiod för LAS-ackumulering.</summary>
    public void LaggTillPeriod(DateOnly startDatum, DateOnly slutDatum, string? anstallningsId = null)
    {
        var period = new LASPeriod
        {
            StartDatum = startDatum,
            SlutDatum = slutDatum,
            AntalDagar = slutDatum.DayNumber - startDatum.DayNumber + 1,
            AnstallningsId = anstallningsId
        };
        _perioder.Add(period);

        // Omberäkna ackumulering inom referensfönstret
        Omberakna(DateOnly.FromDateTime(DateTime.Today));
    }

    /// <summary>Omberäkna ackumulerade dagar baserat på perioder inom referensfönstret.</summary>
    public void Omberakna(DateOnly referensDatum)
    {
        ReferensfonsterStart = referensDatum.AddYears(-REFERENSFONSTER_AR);
        ReferensfonsterSlut = referensDatum;

        AckumuleradeDagar = _perioder
            .Where(p => p.SlutDatum >= ReferensfonsterStart && p.StartDatum <= ReferensfonsterSlut)
            .Sum(p =>
            {
                var effectiveStart = p.StartDatum < ReferensfonsterStart ? ReferensfonsterStart : p.StartDatum;
                var effectiveEnd = p.SlutDatum > ReferensfonsterSlut ? ReferensfonsterSlut : p.SlutDatum;
                return effectiveEnd.DayNumber - effectiveStart.DayNumber + 1;
            });

        // Kontrollera 3-i-månad-regeln
        KontrolleraTreIManad();

        // Uppdatera status
        UppdateraStatus();
    }

    private void KontrolleraTreIManad()
    {
        // Gruppera perioder per kalendermånad
        var perioderPerManad = _perioder
            .Where(p => p.SlutDatum >= ReferensfonsterStart)
            .GroupBy(p => new { p.StartDatum.Year, p.StartDatum.Month });

        foreach (var group in perioderPerManad)
        {
            if (group.Count() >= 3 && Anstallningsform == EmploymentType.SAVA)
            {
                // Mellanliggande perioder ska räknas
                var sortedPeriods = group.OrderBy(p => p.StartDatum).ToList();
                for (int i = 0; i < sortedPeriods.Count - 1; i++)
                {
                    var gap = sortedPeriods[i + 1].StartDatum.DayNumber - sortedPeriods[i].SlutDatum.DayNumber - 1;
                    if (gap > 0)
                    {
                        AckumuleradeDagar += gap;
                        _handelser.Add(LASEvent.Skapa(
                            LASEventTyp.TreIManadRegelTillampad,
                            $"3-i-månadsregeln: {gap} mellanliggande dagar räknas ({sortedPeriods[i].SlutDatum} - {sortedPeriods[i + 1].StartDatum})"));
                    }
                }
            }
        }
    }

    private void UppdateraStatus()
    {
        var maxDagar = Anstallningsform == EmploymentType.SAVA ? SAVA_MAX_DAGAR_5AR : VIKARIAT_MAX_DAGAR_5AR;
        var previousStatus = Status;

        if (AckumuleradeDagar >= maxDagar)
        {
            Status = LASStatus.KonverteradTillTillsvidare;
            KonverteringsDatum ??= DateOnly.FromDateTime(DateTime.Today);
            if (previousStatus != LASStatus.KonverteradTillTillsvidare)
            {
                _handelser.Add(LASEvent.Skapa(
                    LASEventTyp.Konvertering,
                    $"Konverterad till tillsvidareanställning efter {AckumuleradeDagar} dagar"));
                RaiseDomainEvent(new LASConversionTriggeredEvent(AnstallId, Anstallningsform, AckumuleradeDagar));
            }
        }
        else if (Anstallningsform == EmploymentType.SAVA && AckumuleradeDagar >= SAVA_ALARM_11_MANADER)
        {
            Status = LASStatus.KritiskNara;
            if (previousStatus != LASStatus.KritiskNara)
                _handelser.Add(LASEvent.Skapa(LASEventTyp.Alarm, $"KRITISKT: {AckumuleradeDagar} dagar av {maxDagar}"));
        }
        else if (Anstallningsform == EmploymentType.SAVA && AckumuleradeDagar >= SAVA_ALARM_10_MANADER)
        {
            Status = LASStatus.NaraGrans;
            if (previousStatus != LASStatus.NaraGrans)
                _handelser.Add(LASEvent.Skapa(LASEventTyp.Alarm, $"Varning: {AckumuleradeDagar} dagar av {maxDagar}"));
        }
        else
        {
            Status = LASStatus.UnderGrans;
        }
    }

    /// <summary>
    /// Sätt företrädesrätt efter anställningens slut.
    /// Per 25§ LAS:
    /// - SAVA: företrädesrätt efter minst 9 månader (~274 dagar) i en 3-årsperiod
    /// - Vikariat/tillsvidare: företrädesrätt efter minst 12 månader (~365 dagar) i en 3-årsperiod
    /// Företrädesrätten gäller i 9 månader från anställningens slut.
    /// </summary>
    public void SattForetradesratt(DateOnly anstallningSlut)
    {
        // Beräkna dagar i 3-årsperioden
        var treArsStart = anstallningSlut.AddYears(-FORETRADESRATT_REFERENSFONSTER_AR);
        var dagarITreArsfonster = _perioder
            .Where(p => p.SlutDatum >= treArsStart && p.StartDatum <= anstallningSlut)
            .Sum(p =>
            {
                var effectiveStart = p.StartDatum < treArsStart ? treArsStart : p.StartDatum;
                var effectiveEnd = p.SlutDatum > anstallningSlut ? anstallningSlut : p.SlutDatum;
                return effectiveEnd.DayNumber - effectiveStart.DayNumber + 1;
            });

        var kravDagar = Anstallningsform == EmploymentType.SAVA
            ? FORETRADESRATT_SAVA_DAGAR
            : FORETRADESRATT_VIKARIAT_DAGAR;

        if (dagarITreArsfonster >= kravDagar)
        {
            HarForetradesratt = true;
            ForetradesrattUtgar = anstallningSlut.AddMonths(FORETRADESRATT_MANADER);
            _handelser.Add(LASEvent.Skapa(
                LASEventTyp.ForetradesrattBeviljad,
                $"Företrädesrätt till {ForetradesrattUtgar.Value} ({dagarITreArsfonster} dagar i 3-årsperiod, krav: {kravDagar} dagar)"));
        }
    }

    /// <summary>Generera turordningslista per driftsenhet.</summary>
    public int BeraknaLASDagar() => AckumuleradeDagar;
}

public sealed class LASPeriod
{
    public DateOnly StartDatum { get; set; }
    public DateOnly SlutDatum { get; set; }
    public int AntalDagar { get; set; }
    public string? AnstallningsId { get; set; }
}

public sealed class LASEvent
{
    public Guid Id { get; private set; }
    public LASEventTyp Typ { get; private set; }
    public DateTime Tidpunkt { get; private set; }
    public string Beskrivning { get; private set; } = string.Empty;

    public static LASEvent Skapa(LASEventTyp typ, string beskrivning) => new()
    {
        Id = Guid.NewGuid(),
        Typ = typ,
        Tidpunkt = DateTime.UtcNow,
        Beskrivning = beskrivning
    };
}

public enum LASEventTyp
{
    PeriodRegistrerad,
    Alarm,
    Konvertering,
    ForetradesrattBeviljad,
    ForetradesrattUtgangen,
    TreIManadRegelTillampad
}

public enum LASStatus
{
    UnderGrans,
    NaraGrans,
    KritiskNara,
    KonverteradTillTillsvidare
}

public sealed record LASConversionTriggeredEvent(
    EmployeeId AnstallId,
    EmploymentType Anstallningsform,
    int AckumuleradeDagar) : DomainEvent;
