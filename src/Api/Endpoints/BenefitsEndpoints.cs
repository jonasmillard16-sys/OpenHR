using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Benefits.Domain;

namespace RegionHR.Api.Endpoints;

public static class BenefitsEndpoints
{
    public static WebApplication MapBenefitsEndpoints(this WebApplication app)
    {
        var formaner = app.MapGroup("/api/v1/formaner").WithTags("Förmåner").RequireAuthorization();

        // ============================================================
        // Lista förmånskatalog
        // ============================================================

        formaner.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var benefits = await db.Benefits
                .OrderBy(b => b.Kategori)
                .ThenBy(b => b.Namn)
                .ToListAsync(ct);

            return Results.Ok(benefits.Select(b => new
            {
                b.Id, b.Namn, b.Beskrivning,
                Kategori = b.Kategori.ToString(),
                b.MaxBelopp, b.ArbetsgivarAndel, b.ArbetstagarAndel,
                b.ArSkattepliktig, b.ArAktiv
            }));
        }).WithName("ListBenefits");

        // ============================================================
        // Skapa förmån
        // ============================================================

        formaner.MapPost("/", async (CreateBenefitRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<BenefitCategory>(req.Kategori, true, out var kategori))
                return Results.BadRequest(new { error = $"Ogiltig kategori: {req.Kategori}. Giltiga värden: {string.Join(", ", Enum.GetNames<BenefitCategory>())}" });

            var benefit = Benefit.Skapa(req.Namn, req.Beskrivning, kategori, req.MaxBelopp, req.ArbetsgivarAndel, req.ArSkattepliktig, req.EligibilityRegler);
            await db.Benefits.AddAsync(benefit, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/formaner/{benefit.Id}", new
            {
                benefit.Id, benefit.Namn,
                Kategori = benefit.Kategori.ToString(),
                benefit.MaxBelopp, benefit.ArbetsgivarAndel, benefit.ArbetstagarAndel,
                benefit.ArSkattepliktig, benefit.ArAktiv
            });
        }).WithName("CreateBenefit");

        // ============================================================
        // Lista anmälningar för anställd
        // ============================================================

        formaner.MapGet("/anmalningar", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var enrollments = await db.EmployeeBenefits
                .Where(e => e.AnstallId == anstallId)
                .OrderByDescending(e => e.SkapadVid)
                .ToListAsync(ct);

            return Results.Ok(enrollments.Select(e => new
            {
                e.Id, e.AnstallId, e.BenefitId,
                Status = e.Status.ToString(),
                e.StartDatum, e.SlutDatum, e.ValtBelopp,
                e.LivshandardAnledning
            }));
        }).WithName("ListEmployeeBenefits");

        // ============================================================
        // Anmäl anställd till förmån
        // ============================================================

        formaner.MapPost("/anmalan", async (CreateEmployeeBenefitRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var benefit = await db.Benefits.FirstOrDefaultAsync(b => b.Id == req.BenefitId, ct);
            if (benefit is null) return Results.NotFound(new { error = "Förmån hittades inte" });
            if (!benefit.ArAktiv) return Results.BadRequest(new { error = "Förmånen är inte aktiv" });

            var enrollment = EmployeeBenefit.Anmala(req.AnstallId, req.BenefitId, req.StartDatum, req.ValtBelopp, req.Livshandelse);
            await db.EmployeeBenefits.AddAsync(enrollment, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/formaner/anmalan/{enrollment.Id}", new
            {
                enrollment.Id, enrollment.AnstallId, enrollment.BenefitId,
                Status = enrollment.Status.ToString(),
                enrollment.StartDatum, enrollment.ValtBelopp
            });
        }).WithName("EnrollEmployeeBenefit");

        // ============================================================
        // Godkänn anmälan
        // ============================================================

        formaner.MapPost("/anmalan/{id:guid}/godkann", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var enrollment = await db.EmployeeBenefits.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (enrollment is null) return Results.NotFound();

            enrollment.Godkann();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { enrollment.Id, Status = enrollment.Status.ToString() });
        }).WithName("ApproveBenefitEnrollment");

        // ============================================================
        // Avsluta förmån
        // ============================================================

        formaner.MapPost("/anmalan/{id:guid}/avsluta", async (Guid id, AvslutaBenefitRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var enrollment = await db.EmployeeBenefits.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (enrollment is null) return Results.NotFound();

            enrollment.Avsluta(req.SlutDatum);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { enrollment.Id, Status = enrollment.Status.ToString(), enrollment.SlutDatum });
        }).WithName("EndBenefitEnrollment");

        return app;
    }
}

// Request DTOs
record CreateBenefitRequest(string Namn, string Beskrivning, string Kategori, decimal MaxBelopp, decimal ArbetsgivarAndel, bool ArSkattepliktig, string? EligibilityRegler = null);
record CreateEmployeeBenefitRequest(Guid AnstallId, Guid BenefitId, DateOnly StartDatum, decimal ValtBelopp, string? Livshandelse = null);
record AvslutaBenefitRequest(DateOnly SlutDatum);
