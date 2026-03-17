using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Leave.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class LeaveEndpoints
{
    public static WebApplication MapLeaveEndpoints(this WebApplication app)
    {
        var ledighet = app.MapGroup("/api/v1/ledighet").WithTags("Ledighet").RequireAuthorization();

        // ============================================================
        // Semestersaldo
        // ============================================================

        ledighet.MapGet("/balanser", async (Guid anstallId, int ar, RegionHRDbContext db, CancellationToken ct) =>
        {
            var balance = await db.VacationBalances
                .FirstOrDefaultAsync(b => b.AnstallId == anstallId && b.Ar == ar, ct);

            return balance is not null ? Results.Ok(balance) : Results.NotFound();
        }).WithName("GetVacationBalance");

        ledighet.MapPost("/balans", async (CreateVacationBalanceRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var balance = VacationBalance.SkapaForAr(req.AnstallId, req.Ar, req.Alder);
            await db.VacationBalances.AddAsync(balance, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/ledighet/balanser?anstallId={req.AnstallId}&ar={req.Ar}", new
            {
                balance.Id,
                balance.AnstallId,
                balance.Ar,
                balance.Tilldelning,
                balance.TillgangligaDagar
            });
        }).WithName("CreateVacationBalance");

        // Auto-create balance using personnummer age calculation
        ledighet.MapPost("/balans/auto", async (AutoBalanceRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var emp = await db.Employees.FirstOrDefaultAsync(e => e.Id == EmployeeId.From(req.AnstallId), ct);
            if (emp is null) return Results.NotFound(new { error = "Anställd hittades inte" });

            // Calculate age from personnummer birth date
            var birthDate = emp.Personnummer.BirthDate;
            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - birthDate.Year;
            if (birthDate.AddYears(age) > today) age--;

            var balance = VacationBalance.SkapaForAr(req.AnstallId, req.Ar, age);
            await db.VacationBalances.AddAsync(balance, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/ledighet/balanser?anstallId={req.AnstallId}&ar={req.Ar}",
                new { balance.Id, balance.Tilldelning, balance.TillgangligaDagar, beraknadAlder = age });
        }).WithName("AutoCreateVacationBalance");

        // ============================================================
        // Frånvaroansökningar
        // ============================================================

        ledighet.MapGet("/ansokngar", async (Guid? anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.LeaveRequests.AsQueryable();
            if (anstallId.HasValue)
                query = query.Where(r => r.AnstallId == anstallId.Value);

            var requests = await query
                .OrderByDescending(r => r.SkapadVid)
                .ToListAsync(ct);

            return Results.Ok(requests);
        }).WithName("ListLeaveRequests");

        ledighet.MapPost("/ansokan", async (CreateLeaveRequestDto req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<LeaveType>(req.Typ, true, out var typ))
                return Results.BadRequest(new { error = $"Ogiltig typ: {req.Typ}. Giltiga värden: {string.Join(", ", Enum.GetNames<LeaveType>())}" });

            var leaveRequest = LeaveRequest.Skapa(req.AnstallId, typ, req.FranDatum, req.TillDatum, req.Beskrivning);
            await db.LeaveRequests.AddAsync(leaveRequest, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/ledighet/ansokan/{leaveRequest.Id}", new
            {
                leaveRequest.Id,
                leaveRequest.AnstallId,
                Typ = leaveRequest.Typ.ToString(),
                leaveRequest.FranDatum,
                leaveRequest.TillDatum,
                leaveRequest.AntalDagar,
                Status = leaveRequest.Status.ToString()
            });
        }).WithName("CreateLeaveRequest");

        ledighet.MapPost("/ansokan/{id:guid}/skickain", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var leaveRequest = await db.LeaveRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (leaveRequest is null) return Results.NotFound();

            try
            {
                leaveRequest.SkickaIn();
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { leaveRequest.Id, Status = leaveRequest.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("SubmitLeaveRequest");

        ledighet.MapPost("/ansokan/{id:guid}/godkann", async (Guid id, GodkannLeaveRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var leaveRequest = await db.LeaveRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (leaveRequest is null) return Results.NotFound();

            try
            {
                leaveRequest.Godkann(req.Godkannare, req.Kommentar);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { leaveRequest.Id, Status = leaveRequest.Status.ToString(), leaveRequest.GodkandAv });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("ApproveLeaveRequest");

        ledighet.MapPost("/ansokan/{id:guid}/avvisa", async (Guid id, AvvisaLeaveRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var leaveRequest = await db.LeaveRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (leaveRequest is null) return Results.NotFound();

            try
            {
                leaveRequest.Avvisa(req.Godkannare, req.Kommentar);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { leaveRequest.Id, Status = leaveRequest.Status.ToString(), leaveRequest.Kommentar });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("RejectLeaveRequest");

        // Schedule conflict check
        ledighet.MapGet("/ansokan/{id:guid}/konflikter", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var request = await db.LeaveRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (request is null) return Results.NotFound();

            // Check for scheduled shifts during the leave period
            var empId = EmployeeId.From(request.AnstallId);
            var conflicts = await db.ScheduledShifts
                .Where(s => s.AnstallId == empId && s.Datum >= request.FranDatum && s.Datum <= request.TillDatum)
                .OrderBy(s => s.Datum)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                ansokanId = request.Id,
                antalKonflikter = conflicts.Count,
                konflikter = conflicts.Select(s => new { s.Datum, PassTyp = s.PassTyp.ToString(), s.PlaneradStart, s.PlaneradSlut })
            });
        }).WithName("CheckLeaveConflicts");

        // ============================================================
        // Sjukanmälan
        // ============================================================

        ledighet.MapGet("/sjukanmalan", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var notifications = await db.SickLeaveNotifications
                .Where(s => s.AnstallId == anstallId)
                .OrderByDescending(s => s.StartDatum)
                .ToListAsync(ct);

            return Results.Ok(notifications);
        }).WithName("ListSickLeaveNotifications");

        return app;
    }
}

// Request DTOs
record CreateVacationBalanceRequest(Guid AnstallId, int Ar, int Alder);
record AutoBalanceRequest(Guid AnstallId, int Ar);
record CreateLeaveRequestDto(Guid AnstallId, string Typ, DateOnly FranDatum, DateOnly TillDatum, string? Beskrivning);
record GodkannLeaveRequest(Guid Godkannare, string? Kommentar);
record AvvisaLeaveRequest(Guid Godkannare, string Kommentar);
