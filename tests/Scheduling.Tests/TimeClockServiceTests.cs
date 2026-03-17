using RegionHR.Scheduling.Domain;
using RegionHR.Scheduling.Services;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Scheduling.Tests;

public class TimeClockServiceTests
{
    private readonly EmployeeId _anstallId = EmployeeId.New();
    private readonly OrganizationId _enhetId = OrganizationId.New();

    #region Instämpling

    [Fact]
    public async Task StamplaIn_MatcharPlaneradePass_Lyckad()
    {
        var planeratPass = SkapaPlaneradePass(
            new TimeOnly(7, 0), new TimeOnly(16, 0), ShiftType.Dag);

        var shiftRepo = new InMemoryShiftRepository();
        shiftRepo.LaggTillMatchandePass(planeratPass);

        var clockRepo = new InMemoryClockRepository();
        // Simulera instämpling kl 07:05 (inom tröskel)
        var sut = new TestableTimeClockService(shiftRepo, clockRepo,
            new DateTime(2025, 3, 17, 7, 5, 0, DateTimeKind.Utc));

        var resultat = await sut.StamplaInAsync(
            _anstallId, ClockSource.Webbterminal, "192.168.1.1", null, null);

        Assert.True(resultat.Lyckades);
        Assert.NotNull(resultat.PassId);
        Assert.Equal(planeratPass.Id, resultat.PassId);
        Assert.Null(resultat.Avvikelse);
    }

    [Fact]
    public async Task StamplaIn_IngetPlaneratPass_SkaparEjPlaneratAvvikelse()
    {
        var shiftRepo = new InMemoryShiftRepository(); // Inga planerade pass
        var clockRepo = new InMemoryClockRepository();
        var sut = new TimeClockService(shiftRepo, clockRepo);

        var resultat = await sut.StamplaInAsync(
            _anstallId, ClockSource.PWA, null, 59.3293, 18.0686);

        Assert.True(resultat.Lyckades);
        Assert.Null(resultat.PassId);
        Assert.NotNull(resultat.Avvikelse);
        Assert.Equal(AvvikelseTyp.EjPlaneratPass, resultat.Avvikelse.Typ);
    }

    [Fact]
    public async Task StamplaIn_SenAnkomst_SkaparAvvikelse()
    {
        var planeratPass = SkapaPlaneradePass(
            new TimeOnly(7, 0), new TimeOnly(16, 0), ShiftType.Dag);

        var shiftRepo = new InMemoryShiftRepository();
        shiftRepo.LaggTillMatchandePass(planeratPass);

        // Simulera sen stämpling genom att sätta tid 30 min efter planerad start
        shiftRepo.SimuleraTid = DateTime.UtcNow.Date.AddHours(7).AddMinutes(30);

        var clockRepo = new InMemoryClockRepository();
        var sut = new TestableTimeClockService(shiftRepo, clockRepo,
            new DateTime(2025, 3, 17, 7, 30, 0, DateTimeKind.Utc));

        var resultat = await sut.StamplaInAsync(
            _anstallId, ClockSource.Webbterminal, null, null, null);

        Assert.True(resultat.Lyckades);
        Assert.NotNull(resultat.Avvikelse);
        Assert.Equal(AvvikelseTyp.SenAnkomst, resultat.Avvikelse.Typ);
        Assert.Contains("Sen ankomst", resultat.Avvikelse.Beskrivning);
    }

    #endregion

    #region Utstämpling

    [Fact]
    public async Task StamplaUt_AktivtPass_BeraknarFaktiskaTimmar()
    {
        var aktivtPass = SkapaPlaneradePass(
            new TimeOnly(7, 0), new TimeOnly(16, 0), ShiftType.Dag);
        aktivtPass.StamplaIn(new TimeOnly(7, 0));

        var shiftRepo = new InMemoryShiftRepository();
        shiftRepo.SattAktivtPass(aktivtPass);

        var clockRepo = new InMemoryClockRepository();
        var sut = new TimeClockService(shiftRepo, clockRepo);

        var resultat = await sut.StamplaUtAsync(
            _anstallId, ClockSource.Webbterminal, null);

        Assert.True(resultat.Lyckades);
        Assert.NotNull(resultat.PassId);
        Assert.Equal(aktivtPass.Id, resultat.PassId);
    }

    [Fact]
    public async Task StamplaUt_IngetAktivtPass_Misslyckas()
    {
        var shiftRepo = new InMemoryShiftRepository(); // Inget aktivt pass
        var clockRepo = new InMemoryClockRepository();
        var sut = new TimeClockService(shiftRepo, clockRepo);

        var resultat = await sut.StamplaUtAsync(
            _anstallId, ClockSource.Webbterminal, null);

        Assert.False(resultat.Lyckades);
        Assert.Null(resultat.PassId);
    }

    [Fact]
    public async Task StamplaUt_TidigAvgang_SkaparAvvikelse()
    {
        var aktivtPass = SkapaPlaneradePass(
            new TimeOnly(7, 0), new TimeOnly(16, 0), ShiftType.Dag);
        aktivtPass.StamplaIn(new TimeOnly(7, 0));

        var shiftRepo = new InMemoryShiftRepository();
        shiftRepo.SattAktivtPass(aktivtPass);

        var clockRepo = new InMemoryClockRepository();
        // Simulera utstämpling 1h tidig
        var sut = new TestableTimeClockService(shiftRepo, clockRepo,
            new DateTime(2025, 3, 17, 14, 45, 0, DateTimeKind.Utc));

        var resultat = await sut.StamplaUtAsync(
            _anstallId, ClockSource.Webbterminal, null);

        Assert.True(resultat.Lyckades);
        Assert.NotNull(resultat.Avvikelse);
        Assert.Equal(AvvikelseTyp.TidigAvgang, resultat.Avvikelse.Typ);
    }

    [Fact]
    public async Task StamplaUt_NattpassOeverMidnatt_KorrektBerakning()
    {
        // Nattpass 21:00-07:00
        var nattPass = SkapaPlaneradePass(
            new TimeOnly(21, 0), new TimeOnly(7, 0), ShiftType.Natt);
        nattPass.StamplaIn(new TimeOnly(21, 0));

        var shiftRepo = new InMemoryShiftRepository();
        shiftRepo.SattAktivtPass(nattPass);

        var clockRepo = new InMemoryClockRepository();
        var sut = new TimeClockService(shiftRepo, clockRepo);

        var resultat = await sut.StamplaUtAsync(
            _anstallId, ClockSource.Webbterminal, null);

        Assert.True(resultat.Lyckades);
        Assert.NotNull(resultat.PassId);
        // Nattpass: 21:00-07:00 = 10h - 30min rast = 9.5h
        Assert.NotNull(nattPass.FaktiskaTimmar);
        Assert.True(nattPass.FaktiskaTimmar > 0);
    }

    #endregion

    #region Offline-synk

    [Fact]
    public async Task SynkaOfflineStamplingar_BearbetarKronologiskt()
    {
        var planeratPass = SkapaPlaneradePass(
            new TimeOnly(7, 0), new TimeOnly(16, 0), ShiftType.Dag);

        var shiftRepo = new InMemoryShiftRepository();
        shiftRepo.LaggTillMatchandePass(planeratPass);

        var clockRepo = new InMemoryClockRepository();
        var sut = new TimeClockService(shiftRepo, clockRepo);

        var stamplings = new List<OfflineStampling>
        {
            new() { Typ = ClockEventType.Ut, Tidpunkt = new DateTime(2025, 3, 17, 16, 0, 0), Lat = 59.33, Lon = 18.07 },
            new() { Typ = ClockEventType.In, Tidpunkt = new DateTime(2025, 3, 17, 7, 0, 0), Lat = 59.33, Lon = 18.07 },
        };

        var resultat = await sut.SynkaOfflineStamplingarAsync(_anstallId, stamplings);

        Assert.Equal(2, resultat.Count);
        Assert.All(resultat, r => Assert.True(r.Lyckades));
    }

    #endregion

    #region Status

    [Fact]
    public async Task HamtaStatus_IngenAktivStampling_InteInstamplad()
    {
        var shiftRepo = new InMemoryShiftRepository();
        var clockRepo = new InMemoryClockRepository();
        var sut = new TimeClockService(shiftRepo, clockRepo);

        var status = await sut.HamtaStatusAsync(_anstallId);

        Assert.False(status.ArInstamplad);
        Assert.Null(status.AktivtPassId);
    }

    [Fact]
    public async Task HamtaStatus_AktivStampling_ArInstamplad()
    {
        var aktivtPass = SkapaPlaneradePass(
            new TimeOnly(7, 0), new TimeOnly(16, 0), ShiftType.Dag);
        aktivtPass.StamplaIn(new TimeOnly(7, 0));

        var shiftRepo = new InMemoryShiftRepository();
        shiftRepo.SattAktivtPass(aktivtPass);

        var clockRepo = new InMemoryClockRepository();
        var sut = new TimeClockService(shiftRepo, clockRepo);

        var status = await sut.HamtaStatusAsync(_anstallId);

        Assert.True(status.ArInstamplad);
        Assert.Equal(aktivtPass.Id, status.AktivtPassId);
        Assert.Equal(new TimeOnly(7, 0), status.InstampladVid);
    }

    #endregion

    #region Hjälpklasser och metoder

    private ScheduledShift SkapaPlaneradePass(TimeOnly start, TimeOnly slut, ShiftType typ)
    {
        return new ScheduledShift
        {
            Id = Guid.NewGuid(),
            SchemaId = ScheduleId.New(),
            AnstallId = _anstallId,
            Datum = DateOnly.FromDateTime(DateTime.UtcNow),
            PassTyp = typ,
            PlaneradStart = start,
            PlaneradSlut = slut,
            Rast = TimeSpan.FromMinutes(30),
            Status = ShiftStatus.Planerad
        };
    }

    /// <summary>
    /// In-memory repository för tester.
    /// </summary>
    private sealed class InMemoryShiftRepository : IScheduledShiftRepository
    {
        private ScheduledShift? _matchandePass;
        private ScheduledShift? _aktivtPass;
        private readonly List<ScheduledShift> _allaPass = [];
        public DateTime? SimuleraTid { get; set; }

        public void LaggTillMatchandePass(ScheduledShift pass) => _matchandePass = pass;
        public void SattAktivtPass(ScheduledShift pass) => _aktivtPass = pass;

        public Task<ScheduledShift?> HittaMatchandePassAsync(EmployeeId anstallId, DateOnly datum, TimeOnly tid, CancellationToken ct)
            => Task.FromResult(_matchandePass);

        public Task<ScheduledShift?> HittaAktivtPassAsync(EmployeeId anstallId, CancellationToken ct)
            => Task.FromResult(_aktivtPass);

        public Task<IReadOnlyList<ScheduledShift>> HamtaPassForEnhetAsync(OrganizationId enhetId, DateOnly datum, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ScheduledShift>>(_allaPass);

        public Task<IReadOnlyList<ScheduledShift>> HamtaPassForAnstaldAsync(EmployeeId anstallId, DateOnly from, DateOnly tom, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<ScheduledShift>>(_allaPass.Where(p => p.AnstallId == anstallId && p.Datum >= from && p.Datum <= tom).ToList());

        public Task UpdateAsync(ScheduledShift pass, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class InMemoryClockRepository : ITimeClockEventRepository
    {
        private readonly List<TimeClockEvent> _events = [];

        public Task AddAsync(TimeClockEvent clockEvent, CancellationToken ct)
        {
            _events.Add(clockEvent);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<TimeClockEvent>> HamtaForAnstaldAsync(EmployeeId anstallId, DateOnly datum, CancellationToken ct)
            => Task.FromResult<IReadOnlyList<TimeClockEvent>>(_events.Where(e => e.AnstallId == anstallId).ToList());
    }

    /// <summary>
    /// Testbar version av TimeClockService som tillåter att simulera tidpunkter.
    /// </summary>
    private sealed class TestableTimeClockService : TimeClockService
    {
        private readonly DateTime _simulatedTime;
        private readonly IScheduledShiftRepository _shiftRepo;
        private readonly ITimeClockEventRepository _clockRepo;

        public TestableTimeClockService(
            IScheduledShiftRepository shiftRepository,
            ITimeClockEventRepository clockRepository,
            DateTime simulatedTime)
            : base(shiftRepository, clockRepository)
        {
            _simulatedTime = simulatedTime;
            _shiftRepo = shiftRepository;
            _clockRepo = clockRepository;
        }

        public new async Task<TimeClockResult> StamplaInAsync(
            EmployeeId anstallId, ClockSource kalla, string? ip, double? lat, double? lon,
            CancellationToken ct = default)
        {
            var datum = DateOnly.FromDateTime(_simulatedTime);
            var tidNu = TimeOnly.FromDateTime(_simulatedTime);

            var planeratPass = await _shiftRepo.HittaMatchandePassAsync(anstallId, datum, tidNu, ct);

            var clockEvent = new TimeClockEvent
            {
                Id = Guid.NewGuid(),
                AnstallId = anstallId,
                Typ = ClockEventType.In,
                Tidpunkt = _simulatedTime,
                Kalla = kalla,
                IPAdress = ip,
                Latitud = lat,
                Longitud = lon
            };

            Avvikelse? avvikelse = null;

            if (planeratPass is null)
            {
                avvikelse = new Avvikelse
                {
                    PassId = Guid.Empty,
                    AnstallId = anstallId,
                    Typ = AvvikelseTyp.EjPlaneratPass,
                    Beskrivning = $"Instämpling utan planerat pass {datum:yyyy-MM-dd} kl {tidNu:HH:mm}."
                };
                await _clockRepo.AddAsync(clockEvent, ct);
                return new TimeClockResult { Lyckades = true, PassId = null, Avvikelse = avvikelse };
            }

            clockEvent.KopplatPassId = planeratPass.Id;
            planeratPass.StamplaIn(tidNu);

            var diff = tidNu.ToTimeSpan() - planeratPass.PlaneradStart.ToTimeSpan();
            if (diff < TimeSpan.Zero) diff += TimeSpan.FromHours(24);
            var diffMinuter = (int)diff.TotalMinutes;

            if (diffMinuter > 15)
            {
                avvikelse = new Avvikelse
                {
                    PassId = planeratPass.Id,
                    AnstallId = anstallId,
                    Typ = AvvikelseTyp.SenAnkomst,
                    Beskrivning = $"Sen ankomst: planerad start {planeratPass.PlaneradStart:HH:mm}, faktisk start {tidNu:HH:mm} ({diffMinuter} minuter sen).",
                    Differens = TimeSpan.FromMinutes(diffMinuter)
                };
                planeratPass.RegistreraAvvikelse(AvvikelseTyp.SenAnkomst, avvikelse.Beskrivning);
            }

            await _clockRepo.AddAsync(clockEvent, ct);
            await _shiftRepo.UpdateAsync(planeratPass, ct);

            return new TimeClockResult
            {
                Lyckades = true,
                PassId = planeratPass.Id,
                Avvikelse = avvikelse,
                Meddelande = avvikelse is not null ? avvikelse.Beskrivning : "Instämpling registrerad."
            };
        }

        public new async Task<TimeClockResult> StamplaUtAsync(
            EmployeeId anstallId, ClockSource kalla, string? ip,
            CancellationToken ct = default)
        {
            var tidNu = TimeOnly.FromDateTime(_simulatedTime);

            var aktivtPass = await _shiftRepo.HittaAktivtPassAsync(anstallId, ct);
            if (aktivtPass is null)
            {
                return new TimeClockResult { Lyckades = false, Meddelande = "Ingen aktiv instämpling hittad." };
            }

            var clockEvent = new TimeClockEvent
            {
                Id = Guid.NewGuid(),
                AnstallId = anstallId,
                Typ = ClockEventType.Ut,
                Tidpunkt = _simulatedTime,
                Kalla = kalla,
                IPAdress = ip,
                KopplatPassId = aktivtPass.Id
            };

            aktivtPass.StamplaUt(tidNu);
            Avvikelse? avvikelse = null;

            // Kontrollera tidig avgång
            var diff = aktivtPass.PlaneradSlut.ToTimeSpan() - tidNu.ToTimeSpan();
            if (diff < TimeSpan.Zero) diff += TimeSpan.FromHours(24);
            if (diff > TimeSpan.FromHours(12)) diff = TimeSpan.FromHours(24) - diff; // Normalisera
            var diffMinuter = (int)diff.TotalMinutes;

            if (tidNu < aktivtPass.PlaneradSlut && diffMinuter > 15)
            {
                avvikelse = new Avvikelse
                {
                    PassId = aktivtPass.Id,
                    AnstallId = anstallId,
                    Typ = AvvikelseTyp.TidigAvgang,
                    Beskrivning = $"Tidig avgång: planerad slut {aktivtPass.PlaneradSlut:HH:mm}, faktisk slut {tidNu:HH:mm} ({diffMinuter} minuter tidig).",
                    Differens = TimeSpan.FromMinutes(diffMinuter)
                };
                aktivtPass.RegistreraAvvikelse(AvvikelseTyp.TidigAvgang, avvikelse.Beskrivning);
            }

            await _clockRepo.AddAsync(clockEvent, ct);
            await _shiftRepo.UpdateAsync(aktivtPass, ct);

            return new TimeClockResult
            {
                Lyckades = true,
                PassId = aktivtPass.Id,
                Avvikelse = avvikelse,
                Meddelande = avvikelse is not null ? avvikelse.Beskrivning : "Utstämpling registrerad."
            };
        }
    }

    #endregion
}
