using RegionHR.SharedKernel.Domain;
using RegionHR.Scheduling.Domain;

namespace RegionHR.Scheduling.Services;

/// <summary>
/// Hanterar instämplings- och utstämplingsflödet.
/// Stöder webb-terminaler, PWA med offline-synk och manuell registrering.
/// Detekterar avvikelser automatiskt (sen ankomst, tidig avgång, saknad utstämpling, övertid).
/// </summary>
public class TimeClockService
{
    private readonly IScheduledShiftRepository _shiftRepository;
    private readonly ITimeClockEventRepository _clockRepository;

    /// <summary>Tröskel i minuter för sen ankomst.</summary>
    private const int SEN_ANKOMST_TRÖSKEL_MINUTER = 15;

    /// <summary>Tröskel i minuter för tidig avgång.</summary>
    private const int TIDIG_AVGANG_TRÖSKEL_MINUTER = 15;

    public TimeClockService(
        IScheduledShiftRepository shiftRepository,
        ITimeClockEventRepository clockRepository)
    {
        _shiftRepository = shiftRepository;
        _clockRepository = clockRepository;
    }

    /// <summary>
    /// Stämpla in: hitta matchande planerat pass, skapa TimeClockEvent, uppdatera pass-status.
    /// </summary>
    public async Task<TimeClockResult> StamplaInAsync(
        EmployeeId anstallId,
        ClockSource kalla,
        string? ip,
        double? lat,
        double? lon,
        CancellationToken ct = default)
    {
        var nu = DateTime.UtcNow;
        var idag = DateOnly.FromDateTime(nu);
        var tidNu = TimeOnly.FromDateTime(nu);

        // Hitta närmast planerat pass (inom +/- 2 timmar från nu)
        var planeratPass = await _shiftRepository.HittaMatchandePassAsync(
            anstallId, idag, tidNu, ct);

        var clockEvent = new TimeClockEvent
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Typ = ClockEventType.In,
            Tidpunkt = nu,
            Kalla = kalla,
            IPAdress = ip,
            Latitud = lat,
            Longitud = lon
        };

        Avvikelse? avvikelse = null;

        if (planeratPass is null)
        {
            // Ingen planerat pass — skapa avvikelse
            avvikelse = new Avvikelse
            {
                PassId = Guid.Empty,
                AnstallId = anstallId,
                Typ = AvvikelseTyp.EjPlaneratPass,
                Beskrivning = $"Instämpling utan planerat pass {idag:yyyy-MM-dd} kl {tidNu:HH:mm}.",
                Differens = null
            };

            await _clockRepository.AddAsync(clockEvent, ct);

            return new TimeClockResult
            {
                Lyckades = true,
                Meddelande = "Instämpling registrerad, men inget planerat pass hittades.",
                PassId = null,
                Avvikelse = avvikelse
            };
        }

        // Koppla stämpling till pass
        clockEvent.KopplatPassId = planeratPass.Id;
        planeratPass.StamplaIn(tidNu);

        // Kontrollera sen ankomst
        var differensMinuter = BeraknaDifferensMinuter(tidNu, planeratPass.PlaneradStart);
        if (differensMinuter > SEN_ANKOMST_TRÖSKEL_MINUTER)
        {
            avvikelse = new Avvikelse
            {
                PassId = planeratPass.Id,
                AnstallId = anstallId,
                Typ = AvvikelseTyp.SenAnkomst,
                Beskrivning = $"Sen ankomst: planerad start {planeratPass.PlaneradStart:HH:mm}, faktisk start {tidNu:HH:mm} ({differensMinuter} minuter sen).",
                Differens = TimeSpan.FromMinutes(differensMinuter)
            };

            planeratPass.RegistreraAvvikelse(
                AvvikelseTyp.SenAnkomst,
                avvikelse.Beskrivning);
        }

        await _clockRepository.AddAsync(clockEvent, ct);
        await _shiftRepository.UpdateAsync(planeratPass, ct);

        return new TimeClockResult
        {
            Lyckades = true,
            Meddelande = avvikelse is not null
                ? $"Instämpling registrerad med avvikelse: {avvikelse.Beskrivning}"
                : "Instämpling registrerad.",
            PassId = planeratPass.Id,
            Avvikelse = avvikelse
        };
    }

    /// <summary>
    /// Stämpla ut: hitta aktivt pass, skapa ut-händelse, beräkna faktiska timmar, detektera avvikelser.
    /// </summary>
    public async Task<TimeClockResult> StamplaUtAsync(
        EmployeeId anstallId,
        ClockSource kalla,
        string? ip,
        CancellationToken ct = default)
    {
        var nu = DateTime.UtcNow;
        var tidNu = TimeOnly.FromDateTime(nu);

        var aktivtPass = await _shiftRepository.HittaAktivtPassAsync(anstallId, ct);
        if (aktivtPass is null)
        {
            return new TimeClockResult
            {
                Lyckades = false,
                Meddelande = "Ingen aktiv instämpling hittad.",
                PassId = null,
                Avvikelse = null
            };
        }

        var clockEvent = new TimeClockEvent
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Typ = ClockEventType.Ut,
            Tidpunkt = nu,
            Kalla = kalla,
            IPAdress = ip,
            KopplatPassId = aktivtPass.Id
        };

        aktivtPass.StamplaUt(tidNu);
        Avvikelse? avvikelse = null;

        // Kontrollera tidig avgång
        var differensMinuter = BeraknaDifferensMinuter(aktivtPass.PlaneradSlut, tidNu);
        if (differensMinuter > TIDIG_AVGANG_TRÖSKEL_MINUTER)
        {
            avvikelse = new Avvikelse
            {
                PassId = aktivtPass.Id,
                AnstallId = anstallId,
                Typ = AvvikelseTyp.TidigAvgang,
                Beskrivning = $"Tidig avgång: planerad slut {aktivtPass.PlaneradSlut:HH:mm}, faktisk slut {tidNu:HH:mm} ({differensMinuter} minuter tidig).",
                Differens = TimeSpan.FromMinutes(differensMinuter)
            };

            aktivtPass.RegistreraAvvikelse(
                AvvikelseTyp.TidigAvgang,
                avvikelse.Beskrivning);
        }

        // Kontrollera övertid
        if (aktivtPass.OvertidTimmar.HasValue && aktivtPass.OvertidTimmar > 0)
        {
            var overtidAvvikelse = new Avvikelse
            {
                PassId = aktivtPass.Id,
                AnstallId = anstallId,
                Typ = AvvikelseTyp.Overtid,
                Beskrivning = $"Övertid: {aktivtPass.OvertidTimmar:F2} timmar utöver planerad tid.",
                Differens = TimeSpan.FromHours((double)aktivtPass.OvertidTimmar.Value)
            };

            // Om det inte redan finns en avvikelse, registrera övertid
            if (avvikelse is null)
            {
                avvikelse = overtidAvvikelse;
                aktivtPass.RegistreraAvvikelse(
                    AvvikelseTyp.Overtid,
                    overtidAvvikelse.Beskrivning);
            }
        }

        await _clockRepository.AddAsync(clockEvent, ct);
        await _shiftRepository.UpdateAsync(aktivtPass, ct);

        return new TimeClockResult
        {
            Lyckades = true,
            Meddelande = avvikelse is not null
                ? $"Utstämpling registrerad med avvikelse: {avvikelse.Beskrivning}"
                : $"Utstämpling registrerad. Arbetade timmar: {aktivtPass.FaktiskaTimmar:F2}h.",
            PassId = aktivtPass.Id,
            Avvikelse = avvikelse
        };
    }

    /// <summary>
    /// Synkronisera offline-stämplingar från PWA.
    /// Bearbetar stämplingar i kronologisk ordning.
    /// </summary>
    public async Task<IReadOnlyList<TimeClockResult>> SynkaOfflineStamplingarAsync(
        EmployeeId anstallId,
        IReadOnlyList<OfflineStampling> stamplings,
        CancellationToken ct = default)
    {
        var results = new List<TimeClockResult>();
        var sorterade = stamplings.OrderBy(s => s.Tidpunkt).ToList();

        foreach (var stampling in sorterade)
        {
            var clockEvent = new TimeClockEvent
            {
                Id = Guid.NewGuid(),
                AnstallId = anstallId,
                Typ = stampling.Typ,
                Tidpunkt = stampling.Tidpunkt,
                Kalla = ClockSource.PWA,
                Latitud = stampling.Lat,
                Longitud = stampling.Lon,
                ArOfflineStampling = true,
                SynkadVid = DateTime.UtcNow
            };

            var datum = DateOnly.FromDateTime(stampling.Tidpunkt);
            var tid = TimeOnly.FromDateTime(stampling.Tidpunkt);

            if (stampling.Typ == ClockEventType.In)
            {
                var pass = await _shiftRepository.HittaMatchandePassAsync(anstallId, datum, tid, ct);
                if (pass is not null)
                {
                    clockEvent.KopplatPassId = pass.Id;
                    pass.StamplaIn(tid);
                    await _shiftRepository.UpdateAsync(pass, ct);
                }

                await _clockRepository.AddAsync(clockEvent, ct);
                results.Add(new TimeClockResult
                {
                    Lyckades = true,
                    Meddelande = $"Offline-instämpling synkad för {datum:yyyy-MM-dd} {tid:HH:mm}.",
                    PassId = pass?.Id,
                    Avvikelse = pass is null ? new Avvikelse
                    {
                        PassId = Guid.Empty,
                        AnstallId = anstallId,
                        Typ = AvvikelseTyp.EjPlaneratPass,
                        Beskrivning = $"Offline-instämpling utan planerat pass {datum:yyyy-MM-dd} kl {tid:HH:mm}."
                    } : null
                });
            }
            else if (stampling.Typ == ClockEventType.Ut)
            {
                var pass = await _shiftRepository.HittaAktivtPassAsync(anstallId, ct);
                if (pass is not null)
                {
                    clockEvent.KopplatPassId = pass.Id;
                    pass.StamplaUt(tid);
                    await _shiftRepository.UpdateAsync(pass, ct);
                }

                await _clockRepository.AddAsync(clockEvent, ct);
                results.Add(new TimeClockResult
                {
                    Lyckades = true,
                    Meddelande = $"Offline-utstämpling synkad för {datum:yyyy-MM-dd} {tid:HH:mm}.",
                    PassId = pass?.Id,
                    Avvikelse = null
                });
            }
        }

        return results;
    }

    /// <summary>
    /// Hämta nuvarande stämplingsstatus för en anställd.
    /// </summary>
    public async Task<StamplingStatus> HamtaStatusAsync(
        EmployeeId anstallId,
        CancellationToken ct = default)
    {
        var aktivtPass = await _shiftRepository.HittaAktivtPassAsync(anstallId, ct);

        return new StamplingStatus
        {
            ArInstamplad = aktivtPass is not null,
            AktivtPassId = aktivtPass?.Id,
            InstampladVid = aktivtPass?.FaktiskStart
        };
    }

    /// <summary>
    /// Hämta avvikelser för en enhet och ett datum.
    /// </summary>
    public async Task<IReadOnlyList<Avvikelse>> HamtaAvvikelserAsync(
        OrganizationId enhetId,
        DateOnly datum,
        CancellationToken ct = default)
    {
        var pass = await _shiftRepository.HamtaPassForEnhetAsync(enhetId, datum, ct);
        var avvikelser = new List<Avvikelse>();
        var nu = DateTime.UtcNow;
        var tidNu = TimeOnly.FromDateTime(nu);
        var dagensDate = DateOnly.FromDateTime(nu);

        foreach (var p in pass)
        {
            // Redan registrerade avvikelser
            if (p.HarAvvikelse && p.Avvikelse.HasValue)
            {
                avvikelser.Add(new Avvikelse
                {
                    PassId = p.Id,
                    AnstallId = p.AnstallId,
                    Typ = p.Avvikelse.Value,
                    Beskrivning = p.AvvikelseBeskrivning ?? "Avvikelse utan beskrivning.",
                    Differens = null
                });
            }

            // Detektera saknad utstämpling: pass med instämpling men utan utstämpling
            // och planerad sluttid har passerat med mer än 30 minuter
            if (p.Status == ShiftStatus.Pagaende && p.FaktiskStart.HasValue && !p.FaktiskSlut.HasValue)
            {
                var planeradSlutDT = p.Datum.ToDateTime(p.PlaneradSlut);
                if (p.PlaneradSlut < p.PlaneradStart) planeradSlutDT = planeradSlutDT.AddDays(1);

                if (nu > planeradSlutDT.AddMinutes(30))
                {
                    avvikelser.Add(new Avvikelse
                    {
                        PassId = p.Id,
                        AnstallId = p.AnstallId,
                        Typ = AvvikelseTyp.SaknadUtstampling,
                        Beskrivning = $"Saknad utstämpling: instämplad kl {p.FaktiskStart:HH:mm}, planerad slut {p.PlaneradSlut:HH:mm}.",
                        Differens = TimeSpan.FromMinutes((nu - planeradSlutDT).TotalMinutes)
                    });
                }
            }
        }

        return avvikelser;
    }

    /// <summary>
    /// Beräkna positiv differens i minuter mellan två tidpunkter.
    /// Hanterar tider som korsar midnatt.
    /// </summary>
    private static int BeraknaDifferensMinuter(TimeOnly faktisk, TimeOnly planerad)
    {
        var diff = faktisk.ToTimeSpan() - planerad.ToTimeSpan();
        if (diff < TimeSpan.Zero) diff += TimeSpan.FromHours(24);
        return (int)diff.TotalMinutes;
    }
}

/// <summary>Resultat av en stämplingsoperation.</summary>
public sealed class TimeClockResult
{
    public bool Lyckades { get; init; }
    public string? Meddelande { get; init; }
    public Guid? PassId { get; init; }
    public Avvikelse? Avvikelse { get; init; }
}

/// <summary>Offline-stämpling för PWA-synk.</summary>
public sealed class OfflineStampling
{
    public ClockEventType Typ { get; init; }
    public DateTime Tidpunkt { get; init; }
    public double? Lat { get; init; }
    public double? Lon { get; init; }
}

/// <summary>Nuvarande stämplingsstatus för en anställd.</summary>
public sealed class StamplingStatus
{
    public bool ArInstamplad { get; init; }
    public Guid? AktivtPassId { get; init; }
    public TimeOnly? InstampladVid { get; init; }
}

/// <summary>Registrerad avvikelse.</summary>
public sealed class Avvikelse
{
    public Guid PassId { get; init; }
    public EmployeeId AnstallId { get; init; }
    public AvvikelseTyp Typ { get; init; }
    public string Beskrivning { get; init; } = string.Empty;
    public TimeSpan? Differens { get; init; }
}

/// <summary>
/// Repository-interface för ScheduledShift. Definieras här för att tjänsten kan testas med mock.
/// </summary>
public interface IScheduledShiftRepository
{
    /// <summary>Hitta närmast matchande planerat pass (inom +/- 2 timmar).</summary>
    Task<ScheduledShift?> HittaMatchandePassAsync(EmployeeId anstallId, DateOnly datum, TimeOnly tid, CancellationToken ct = default);

    /// <summary>Hitta pågående pass (status = Pagaende) för en anställd.</summary>
    Task<ScheduledShift?> HittaAktivtPassAsync(EmployeeId anstallId, CancellationToken ct = default);

    /// <summary>Hämta alla pass för en enhet och ett datum.</summary>
    Task<IReadOnlyList<ScheduledShift>> HamtaPassForEnhetAsync(OrganizationId enhetId, DateOnly datum, CancellationToken ct = default);

    /// <summary>Hämta alla pass för en anställd i en period.</summary>
    Task<IReadOnlyList<ScheduledShift>> HamtaPassForAnstaldAsync(EmployeeId anstallId, DateOnly from, DateOnly tom, CancellationToken ct = default);

    Task UpdateAsync(ScheduledShift pass, CancellationToken ct = default);
}

/// <summary>
/// Repository-interface för TimeClockEvent.
/// </summary>
public interface ITimeClockEventRepository
{
    Task AddAsync(TimeClockEvent clockEvent, CancellationToken ct = default);
    Task<IReadOnlyList<TimeClockEvent>> HamtaForAnstaldAsync(EmployeeId anstallId, DateOnly datum, CancellationToken ct = default);
}
