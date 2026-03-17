using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Scheduling.Domain;
using RegionHR.Scheduling.Optimization;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class SchedulingEndpoints
{
    public static WebApplication MapSchedulingEndpoints(this WebApplication app)
    {
        var schema = app.MapGroup("/api/v1/schema").WithTags("Schema").RequireAuthorization();
        var stampling = app.MapGroup("/api/v1/stampling").WithTags("Instämpling").RequireAuthorization();
        var bemanning = app.MapGroup("/api/v1/bemanning").WithTags("Bemanning").RequireAuthorization();

        // ============================================================
        // Scheman — CRUD
        // ============================================================

        schema.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var schedules = await db.Schedules
                .OrderByDescending(s => s.Period.Start)
                .Take(50)
                .ToListAsync(ct);

            return Results.Ok(schedules.Select(s => new
            {
                s.Id, s.Namn, Typ = s.Typ.ToString(), Status = s.Status.ToString(),
                s.EnhetId, PeriodStart = s.Period.Start, PeriodSlut = s.Period.End,
                s.CykelLangdVeckor, AntalPass = s.Pass.Count
            }));
        }).WithName("ListSchedules");

        schema.MapGet("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var schedule = await db.Schedules
                .Include(s => s.Pass)
                .FirstOrDefaultAsync(s => s.Id == ScheduleId.From(id), ct);

            return schedule is not null ? Results.Ok(schedule) : Results.NotFound();
        }).WithName("GetSchedule");

        schema.MapPost("/grundschema", async (CreateGrundschemaRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var schedule = Schedule.SkapaGrundschema(
                OrganizationId.From(req.EnhetId), req.Namn,
                req.StartDatum, req.CykelVeckor);

            await db.Schedules.AddAsync(schedule, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/schema/{schedule.Id}", new { schedule.Id, schedule.Namn });
        }).WithName("CreateGrundschema");

        schema.MapPost("/periodschema", async (CreatePeriodschemaRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var schedule = Schedule.SkapaPeriodschema(
                OrganizationId.From(req.EnhetId), req.Namn,
                req.StartDatum, req.SlutDatum);

            await db.Schedules.AddAsync(schedule, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/schema/{schedule.Id}", new { schedule.Id, schedule.Namn });
        }).WithName("CreatePeriodschema");

        schema.MapPost("/{id:guid}/pass", async (Guid id, LaggTillPassRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var schedule = await db.Schedules
                .Include(s => s.Pass)
                .FirstOrDefaultAsync(s => s.Id == ScheduleId.From(id), ct);
            if (schedule is null) return Results.NotFound();

            var shift = schedule.LaggTillPass(
                EmployeeId.From(req.AnstallId), req.Datum, req.PassTyp,
                req.Start, req.Slut, req.Rast);

            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/schema/{id}/pass/{shift.Id}", new
            {
                shift.Id, shift.AnstallId, shift.Datum,
                PassTyp = shift.PassTyp.ToString(),
                shift.PlaneradStart, shift.PlaneradSlut
            });
        }).WithName("AddShift");

        schema.MapPost("/{id:guid}/publicera", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var schedule = await db.Schedules.FirstOrDefaultAsync(s => s.Id == ScheduleId.From(id), ct);
            if (schedule is null) return Results.NotFound();

            try
            {
                schedule.Publicera();
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { schedule.Id, Status = schedule.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("PublishSchedule");

        // ============================================================
        // Schemaoptimering
        // ============================================================

        schema.MapPost("/optimera", (OptimeraSchemaRequest req, CancellationToken ct) =>
        {
            var solver = new ConstraintScheduleSolver();

            var problem = new ScheduleProblem
            {
                EnhetId = OrganizationId.From(req.EnhetId),
                Period = new DateRange(req.StartDatum, req.SlutDatum),
                PassBehov = req.Behov.Select(b => new StaffingRequirement
                {
                    Datum = b.Datum,
                    PassTyp = b.PassTyp,
                    Start = b.Start,
                    Slut = b.Slut,
                    Rast = b.Rast,
                    AntalBehov = b.AntalBehov,
                    KravdaKompetenser = b.KravdaKompetenser
                }).ToList(),
                TillgangligPersonal = req.Personal.Select(p => new PersonalInfo
                {
                    AnstallId = EmployeeId.From(p.AnstallId),
                    Namn = p.Namn,
                    Sysselsattningsgrad = p.Sysselsattningsgrad,
                    Kompetenser = p.Kompetenser,
                    LedigaDagar = p.LedigaDagar
                }).ToList()
            };

            var solution = solver.Solve(problem);
            return Results.Ok(new
            {
                ArFullstandigt = solution.ArFullstandig,
                AntalTilldelningar = solution.Tilldelningar.Count,
                ObemannadeBehov = solution.ObemannadeBehov.Count,
                TotalOBKostnad = solution.TotalKostnad.Amount,
                Tilldelningar = solution.Tilldelningar.Select(t => new
                {
                    t.AnstallId, t.Datum, PassTyp = t.PassTyp.ToString(),
                    t.Start, t.Slut, t.PlaneradeTimmar
                })
            });
        }).WithName("OptimizeSchedule");

        // ============================================================
        // Instämpling (Kom-och-gå)
        // ============================================================

        stampling.MapPost("/in", async (StamplingRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var ev = TimeClockEvent.StamplaIn(
                EmployeeId.From(req.AnstallId), req.Kalla, req.IPAdress);
            if (req.Latitud.HasValue) ev.Latitud = req.Latitud;
            if (req.Longitud.HasValue) ev.Longitud = req.Longitud;

            await db.TimeClockEvents.AddAsync(ev, ct);

            // Try to match to a planned shift
            var today = DateOnly.FromDateTime(DateTime.Today);
            var shift = await db.ScheduledShifts
                .Where(s => s.AnstallId == EmployeeId.From(req.AnstallId) && s.Datum == today && s.Status == ShiftStatus.Planerad)
                .OrderBy(s => s.PlaneradStart)
                .FirstOrDefaultAsync(ct);

            if (shift is not null)
            {
                shift.StamplaIn(TimeOnly.FromDateTime(DateTime.Now));
                ev.KopplatPassId = shift.Id;
            }

            await db.SaveChangesAsync(ct);
            return Results.Ok(new
            {
                StamplingId = ev.Id,
                ev.Tidpunkt,
                MatchatPass = shift?.Id,
                PassTyp = shift?.PassTyp.ToString()
            });
        }).WithName("ClockIn");

        stampling.MapPost("/ut", async (StamplingRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var ev = TimeClockEvent.StamplaUt(
                EmployeeId.From(req.AnstallId), req.Kalla, req.IPAdress);
            if (req.Latitud.HasValue) ev.Latitud = req.Latitud;
            if (req.Longitud.HasValue) ev.Longitud = req.Longitud;

            await db.TimeClockEvents.AddAsync(ev, ct);

            // Find active shift
            var shift = await db.ScheduledShifts
                .Where(s => s.AnstallId == EmployeeId.From(req.AnstallId) && s.Status == ShiftStatus.Pagaende)
                .FirstOrDefaultAsync(ct);

            if (shift is not null)
            {
                shift.StamplaUt(TimeOnly.FromDateTime(DateTime.Now));
                ev.KopplatPassId = shift.Id;
            }

            await db.SaveChangesAsync(ct);
            return Results.Ok(new
            {
                StamplingId = ev.Id,
                ev.Tidpunkt,
                MatchatPass = shift?.Id,
                FaktiskaTimmar = shift?.FaktiskaTimmar
            });
        }).WithName("ClockOut");

        stampling.MapGet("/status/{anstallId:guid}", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var activeShift = await db.ScheduledShifts
                .Where(s => s.AnstallId == EmployeeId.From(anstallId) && s.Status == ShiftStatus.Pagaende)
                .FirstOrDefaultAsync(ct);

            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayShifts = await db.ScheduledShifts
                .Where(s => s.AnstallId == EmployeeId.From(anstallId) && s.Datum == today)
                .OrderBy(s => s.PlaneradStart)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                ArInstamplad = activeShift is not null,
                AktivtPass = activeShift is not null ? new
                {
                    activeShift.Id,
                    PassTyp = activeShift.PassTyp.ToString(),
                    activeShift.FaktiskStart,
                    activeShift.PlaneradSlut
                } : null,
                DagensPass = todayShifts.Select(s => new
                {
                    s.Id, PassTyp = s.PassTyp.ToString(), Status = s.Status.ToString(),
                    s.PlaneradStart, s.PlaneradSlut, s.FaktiskStart, s.FaktiskSlut
                })
            });
        }).WithName("GetClockStatus");

        stampling.MapGet("/historik/{anstallId:guid}", async (
            Guid anstallId, DateOnly? fran, DateOnly? till, RegionHRDbContext db, CancellationToken ct) =>
        {
            var fromDate = fran ?? DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
            var toDate = till ?? DateOnly.FromDateTime(DateTime.Today);

            var events = await db.TimeClockEvents
                .Where(e => e.AnstallId == EmployeeId.From(anstallId) &&
                           e.Tidpunkt >= fromDate.ToDateTime(TimeOnly.MinValue) &&
                           e.Tidpunkt <= toDate.ToDateTime(TimeOnly.MaxValue))
                .OrderByDescending(e => e.Tidpunkt)
                .Take(200)
                .ToListAsync(ct);

            return Results.Ok(events.Select(e => new
            {
                e.Id, Typ = e.Typ.ToString(), e.Tidpunkt,
                Kalla = e.Kalla.ToString(), e.KopplatPassId, e.ArOfflineStampling
            }));
        }).WithName("GetClockHistory");

        // ============================================================
        // Bemanning
        // ============================================================

        bemanning.MapGet("/{enhetId:guid}/{datum}", async (
            Guid enhetId, DateOnly datum, RegionHRDbContext db, CancellationToken ct) =>
        {
            var shifts = await db.ScheduledShifts
                .Include(s => s.SchemaId)
                .Where(s => s.Datum == datum)
                .ToListAsync(ct);

            // Filter by enhet via schedule
            var scheduleIds = await db.Schedules
                .Where(s => s.EnhetId == OrganizationId.From(enhetId))
                .Select(s => s.Id)
                .ToListAsync(ct);

            var enhetShifts = shifts.Where(s => scheduleIds.Contains(s.SchemaId)).ToList();

            var grouped = enhetShifts
                .GroupBy(s => s.PassTyp)
                .Select(g => new
                {
                    PassTyp = g.Key.ToString(),
                    Planerade = g.Count(),
                    Instamplade = g.Count(s => s.Status is ShiftStatus.Pagaende or ShiftStatus.Avslutad),
                    Pass = g.Select(s => new
                    {
                        s.Id, s.AnstallId, Status = s.Status.ToString(),
                        s.PlaneradStart, s.PlaneradSlut, s.FaktiskStart, s.FaktiskSlut
                    })
                });

            return Results.Ok(new
            {
                EnhetId = enhetId,
                Datum = datum,
                PassOversikt = grouped
            });
        }).WithName("GetStaffingOverview");

        // ============================================================
        // Avvikelser
        // ============================================================

        schema.MapGet("/avvikelser/{datum}", async (
            DateOnly datum, RegionHRDbContext db, CancellationToken ct) =>
        {
            var shifts = await db.ScheduledShifts
                .Where(s => s.Datum == datum && s.Status == ShiftStatus.Avslutad)
                .ToListAsync(ct);

            var avvikelser = new List<object>();
            foreach (var shift in shifts)
            {
                if (shift.FaktiskStart.HasValue && shift.PlaneradStart != default)
                {
                    var diff = shift.FaktiskStart.Value.ToTimeSpan() - shift.PlaneradStart.ToTimeSpan();
                    if (diff.TotalMinutes > 15)
                    {
                        avvikelser.Add(new
                        {
                            shift.Id, shift.AnstallId, Typ = "SenAnkomst",
                            Beskrivning = $"Ankom {diff.TotalMinutes:F0} minuter sent",
                            Differens = diff
                        });
                    }
                }

                if (shift.FaktiskSlut.HasValue && shift.PlaneradSlut != default)
                {
                    var diff = shift.PlaneradSlut.ToTimeSpan() - shift.FaktiskSlut.Value.ToTimeSpan();
                    if (diff.TotalMinutes > 15)
                    {
                        avvikelser.Add(new
                        {
                            shift.Id, shift.AnstallId, Typ = "TidigAvgang",
                            Beskrivning = $"Gick {diff.TotalMinutes:F0} minuter tidigt",
                            Differens = diff
                        });
                    }
                }

                if (shift.FaktiskaTimmar.HasValue && shift.PlaneradeTimmar > 0)
                {
                    var overtid = shift.FaktiskaTimmar.Value - shift.PlaneradeTimmar;
                    if (overtid > 0.25m)
                    {
                        avvikelser.Add(new
                        {
                            shift.Id, shift.AnstallId, Typ = "Overtid",
                            Beskrivning = $"Övertid: {overtid:F1} timmar",
                            Differens = TimeSpan.FromHours((double)overtid)
                        });
                    }
                }
            }

            // Missing clock-outs
            var missingClockOut = await db.ScheduledShifts
                .Where(s => s.Datum == datum && s.Status == ShiftStatus.Pagaende)
                .ToListAsync(ct);

            foreach (var shift in missingClockOut)
            {
                avvikelser.Add(new
                {
                    shift.Id, shift.AnstallId, Typ = "SaknadUtstampling",
                    Beskrivning = "Saknar utstämpling",
                    Differens = (TimeSpan?)null
                });
            }

            return Results.Ok(new { Datum = datum, AntalAvvikelser = avvikelser.Count, Avvikelser = avvikelser });
        }).WithName("GetDeviations");

        return app;
    }
}

// Request DTOs for Scheduling
record CreateGrundschemaRequest(Guid EnhetId, string Namn, DateOnly StartDatum, int CykelVeckor);
record CreatePeriodschemaRequest(Guid EnhetId, string Namn, DateOnly StartDatum, DateOnly SlutDatum);
record LaggTillPassRequest(Guid AnstallId, DateOnly Datum, ShiftType PassTyp, TimeOnly Start, TimeOnly Slut, TimeSpan Rast);
record StamplingRequest(Guid AnstallId, ClockSource Kalla, string? IPAdress = null, double? Latitud = null, double? Longitud = null);
record OptimeraSchemaRequest(
    Guid EnhetId, DateOnly StartDatum, DateOnly SlutDatum,
    List<BehovInput> Behov, List<PersonalInput> Personal);
record BehovInput(
    DateOnly Datum, ShiftType PassTyp, TimeOnly Start, TimeOnly Slut, TimeSpan Rast,
    int AntalBehov = 1, List<string> KravdaKompetenser = null!);
record PersonalInput(
    Guid AnstallId, string Namn, decimal Sysselsattningsgrad,
    List<string> Kompetenser, List<DateOnly> LedigaDagar);
