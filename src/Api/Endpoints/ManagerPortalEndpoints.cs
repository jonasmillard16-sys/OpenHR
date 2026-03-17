using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class ManagerPortalEndpoints
{
    public static WebApplication MapManagerPortalEndpoints(this WebApplication app)
    {
        var chef = app.MapGroup("/api/v1/chef").WithTags("Chefsportal");

        // Dashboard - aggregated overview for a manager
        chef.MapGet("/dashboard/{chefId:guid}", async (Guid chefId, RegionHRDbContext db, CancellationToken ct) =>
        {
            // Pending approvals across modules:
            // - Leave requests with status Inskickad
            var pendingLeave = await db.LeaveRequests.CountAsync(r => r.Status == Leave.Domain.LeaveRequestStatus.Inskickad, ct);

            // - Timesheets with status Inskickad
            var pendingTimesheets = await db.Timesheets.CountAsync(t => t.Status == Scheduling.Domain.TimesheetStatus.Inskickad, ct);

            // - Cases pending approval
            var pendingCases = await db.Cases.CountAsync(c => c.Status == CaseStatus.VantarGodkannande, ct);

            // LAS warnings
            var lasWarnings = await db.LASAccumulations.CountAsync(a => a.Status == LAS.Domain.LASStatus.NaraGrans || a.Status == LAS.Domain.LASStatus.KritiskNara, ct);

            // Active rehab cases
            var activeRehab = await db.RehabCases.CountAsync(r => r.Status != HalsoSAM.Domain.RehabStatus.Avslutad, ct);

            // Staffing today - shifts scheduled for today
            var today = DateOnly.FromDateTime(DateTime.Today);
            var todayShifts = await db.ScheduledShifts.CountAsync(s => s.Datum == today, ct);

            // Performance reviews pending
            var pendingReviews = await db.PerformanceReviews.CountAsync(r => r.ChefId == chefId && r.Status != Performance.Domain.ReviewStatus.Avslutat && r.Status != Performance.Domain.ReviewStatus.Genomford, ct);

            // Expiring certifications in team
            var deadline = DateOnly.FromDateTime(DateTime.Today.AddDays(90));
            var expiringCerts = await db.Certifications.CountAsync(c => c.GiltigTill != null && c.GiltigTill <= deadline && c.GiltigTill >= today, ct);

            return Results.Ok(new
            {
                godkannandeKo = new { ledighet = pendingLeave, tidrapporter = pendingTimesheets, arenden = pendingCases },
                varningar = new { lasAlarm = lasWarnings, aktivRehab = activeRehab, utgaendeCertifikat = expiringCerts },
                bemanningIdag = new { antalPass = todayShifts },
                medarbetarsamtal = new { pagaende = pendingReviews }
            });
        }).WithName("GetManagerDashboard");

        // Attestation queue - all pending items
        chef.MapGet("/attestko", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var leaveReqs = await db.LeaveRequests
                .Where(r => r.Status == Leave.Domain.LeaveRequestStatus.Inskickad)
                .OrderBy(r => r.SkapadVid)
                .Take(50)
                .ToListAsync(ct);

            var timesheets = await db.Timesheets
                .Where(t => t.Status == Scheduling.Domain.TimesheetStatus.Inskickad)
                .OrderBy(t => t.SkapadVid)
                .Take(50)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                ledighetsansokningar = leaveReqs.Select(r => new { r.Id, r.AnstallId, Typ = r.Typ.ToString(), r.FranDatum, r.TillDatum, r.AntalDagar, r.SkapadVid }),
                tidrapporter = timesheets.Select(t => new { t.Id, t.AnstallId, t.Ar, t.Manad, t.PlaneradeTimmar, t.FaktiskaTimmar, t.Overtid, t.SkapadVid })
            });
        }).WithName("GetAttestationQueue");

        // Team overview
        chef.MapGet("/team/{enhetId:guid}", async (Guid enhetId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var orgUnit = await db.OrganizationUnits.FirstOrDefaultAsync(u => u.Id == OrganizationId.From(enhetId), ct);
            if (orgUnit is null) return Results.NotFound();

            var today = DateOnly.FromDateTime(DateTime.Today);

            // Get employees with active employment at this unit
            var employees = await db.Employees
                .Include(e => e.Anstallningar)
                .Where(e => e.Anstallningar.Any(a => a.EnhetId == OrganizationId.From(enhetId) && (a.Giltighetsperiod.End == null || a.Giltighetsperiod.End >= today)))
                .OrderBy(e => e.Efternamn)
                .Take(200)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                enhet = new { orgUnit.Namn, Typ = orgUnit.Typ.ToString() },
                anstallda = employees.Select(e => new { e.Id, e.Fornamn, e.Efternamn, Personnummer = e.Personnummer.ToMaskedString() }),
                antal = employees.Count
            });
        }).WithName("GetTeamOverview");

        return app;
    }
}
