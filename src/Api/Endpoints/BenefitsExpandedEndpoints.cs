using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Benefits.Domain;
using System.Text.Json;

namespace RegionHR.Api.Endpoints;

public static class BenefitsExpandedEndpoints
{
    public static WebApplication MapBenefitsExpandedEndpoints(this WebApplication app)
    {
        var formaner = app.MapGroup("/api/v1/formaner").WithTags("Förmåner — Utökad").RequireAuthorization();

        // ============================================================
        // Mina enrollments (B3 engine)
        // ============================================================
        formaner.MapGet("/enrollment", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var enrollments = await db.BenefitEnrollments
                .Where(e => e.AnstallId == anstallId)
                .OrderByDescending(e => e.SkapadVid)
                .ToListAsync(ct);

            return Results.Ok(enrollments.Select(e => new
            {
                e.Id, e.AnstallId, e.BenefitId,
                e.Status, e.StartDatum, e.ValdNiva
            }));
        }).WithName("ListBenefitEnrollments");

        // ============================================================
        // Enroll (skapa ny BenefitEnrollment)
        // ============================================================
        formaner.MapPost("/enrollment", async (CreateBenefitEnrollmentRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var benefit = await db.Benefits.FirstOrDefaultAsync(b => b.Id == req.BenefitId, ct);
            if (benefit is null) return Results.NotFound(new { error = "Förmån hittades inte" });
            if (!benefit.ArAktiv) return Results.BadRequest(new { error = "Förmånen är inte aktiv" });

            var enrollment = BenefitEnrollment.Skapa(req.AnstallId, req.BenefitId, req.StartDatum, req.ValdNiva);
            await db.BenefitEnrollments.AddAsync(enrollment, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/formaner/enrollment/{enrollment.Id}", new
            {
                enrollment.Id, enrollment.AnstallId, enrollment.BenefitId,
                enrollment.Status, enrollment.StartDatum, enrollment.ValdNiva
            });
        }).WithName("CreateBenefitEnrollment");

        // ============================================================
        // Registrera livshändelse
        // ============================================================
        formaner.MapPost("/livshandelse", async (RegisterLifeEventRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var lifeEvent = await db.LifeEvents.FirstOrDefaultAsync(le => le.Id == req.LifeEventId, ct);
            if (lifeEvent is null) return Results.NotFound(new { error = "Livshändelse-typ hittades inte" });

            if (!lifeEvent.ArInomTidsFonster(req.Datum))
                return Results.BadRequest(new { error = $"Tidsfönstret på {lifeEvent.TidsFonsterDagar} dagar har passerat" });

            var occurrence = LifeEventOccurrence.Registrera(req.LifeEventId, req.AnstallId, req.Datum, req.KoppladeAtgarder);
            await db.LifeEventOccurrences.AddAsync(occurrence, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/formaner/livshandelse/{occurrence.Id}", new
            {
                occurrence.Id, occurrence.LifeEventId, occurrence.AnstallId,
                occurrence.Datum, occurrence.Status
            });
        }).WithName("RegisterLifeEvent");

        // ============================================================
        // Förmånssammanställning
        // ============================================================
        formaner.MapGet("/sammanstallning/{anstallId:guid}", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var ar = DateTime.UtcNow.Year;

            // Check for existing statement
            var existing = await db.BenefitStatements
                .FirstOrDefaultAsync(s => s.AnstallId == anstallId && s.Ar == ar, ct);

            if (existing is not null)
            {
                return Results.Ok(new
                {
                    existing.Id, existing.AnstallId, existing.Ar,
                    existing.AktivaFormaner, existing.TotaltVarde, existing.GenereradVid
                });
            }

            // Generate on the fly
            var enrollments = await db.BenefitEnrollments
                .Where(e => e.AnstallId == anstallId && e.Status == "Active")
                .ToListAsync(ct);

            var benefitIds = enrollments.Select(e => e.BenefitId).Distinct().ToList();
            var benefits = await db.Benefits
                .Where(b => benefitIds.Contains(b.Id))
                .ToListAsync(ct);

            var aktivaFormaner = benefits.Select(b => new { b.Id, b.Namn, b.Kategori, b.MaxBelopp }).ToList();
            var totaltVarde = benefits.Sum(b => b.MaxBelopp);

            var statement = BenefitStatement.Generera(
                anstallId, ar,
                JsonSerializer.Serialize(aktivaFormaner),
                totaltVarde);

            await db.BenefitStatements.AddAsync(statement, ct);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                statement.Id, statement.AnstallId, statement.Ar,
                statement.AktivaFormaner, statement.TotaltVarde, statement.GenereradVid
            });
        }).WithName("GetBenefitStatement");

        // ============================================================
        // Eligibility rules per benefit
        // ============================================================
        formaner.MapGet("/regler/{benefitId:guid}", async (Guid benefitId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var rules = await db.EligibilityRules
                .Include(r => r.Villkor)
                .Where(r => r.BenefitId == benefitId)
                .ToListAsync(ct);

            return Results.Ok(rules.Select(r => new
            {
                r.Id, r.BenefitId, r.Namn, r.Kombination,
                Villkor = r.Villkor.Select(v => new { v.Id, v.Falt, v.Operator, v.Varde })
            }));
        }).WithName("GetEligibilityRules");

        // ============================================================
        // Enrollment periods
        // ============================================================
        formaner.MapGet("/perioder", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var periods = await db.EnrollmentPeriods
                .OrderByDescending(p => p.StartDatum)
                .ToListAsync(ct);

            return Results.Ok(periods.Select(p => new
            {
                p.Id, p.Namn, p.StartDatum, p.SlutDatum, p.Status, p.InkluderadePlaner
            }));
        }).WithName("ListEnrollmentPeriods");

        // ============================================================
        // Life events catalog
        // ============================================================
        formaner.MapGet("/livshandelser", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var events = await db.LifeEvents.OrderBy(e => e.Namn).ToListAsync(ct);
            return Results.Ok(events.Select(e => new
            {
                e.Id, e.Namn, e.Typ, e.TidsFonsterDagar, e.TillatnaAndringar
            }));
        }).WithName("ListLifeEvents");

        // ============================================================
        // Transactions per employee
        // ============================================================
        formaner.MapGet("/transaktioner/{anstallId:guid}", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var transactions = await db.BenefitTransactions
                .Where(t => t.AnstallId == anstallId)
                .OrderByDescending(t => t.Datum)
                .ToListAsync(ct);

            return Results.Ok(transactions.Select(t => new
            {
                t.Id, t.AnstallId, t.BenefitId, t.Typ, t.Belopp, t.Datum, t.Beskrivning
            }));
        }).WithName("ListBenefitTransactions");

        return app;
    }
}

// Request DTOs
record CreateBenefitEnrollmentRequest(Guid AnstallId, Guid BenefitId, DateOnly StartDatum, string? ValdNiva = null);
record RegisterLifeEventRequest(Guid LifeEventId, Guid AnstallId, DateOnly Datum, string? KoppladeAtgarder = null);
