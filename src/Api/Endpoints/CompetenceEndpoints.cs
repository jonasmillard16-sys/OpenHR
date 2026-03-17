using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Competence.Domain;

namespace RegionHR.Api.Endpoints;

public static class CompetenceEndpoints
{
    public static WebApplication MapCompetenceEndpoints(this WebApplication app)
    {
        var kompetens = app.MapGroup("/api/v1/kompetens").WithTags("Kompetens").RequireAuthorization();

        // ============================================================
        // Lista certifieringar för anställd
        // ============================================================

        kompetens.MapGet("/", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var certs = await db.Certifications
                .Where(c => c.AnstallId == anstallId)
                .OrderBy(c => c.Namn)
                .ToListAsync(ct);

            return Results.Ok(certs.Select(c => new
            {
                c.Id, c.AnstallId, c.Namn,
                Typ = c.Typ.ToString(),
                c.Utfardare, c.GiltigFran, c.GiltigTill,
                c.ArObligatorisk,
                Status = c.Status.ToString()
            }));
        }).WithName("ListCertifications");

        // ============================================================
        // Certifieringar som utgår snart
        // ============================================================

        kompetens.MapGet("/utgaende", async (int dagar, RegionHRDbContext db, CancellationToken ct) =>
        {
            var idag = DateOnly.FromDateTime(DateTime.UtcNow);
            var slutdatum = idag.AddDays(dagar > 0 ? dagar : 90);

            var expiring = await db.Certifications
                .Where(c => c.GiltigTill != null && c.GiltigTill <= slutdatum && c.GiltigTill >= idag)
                .OrderBy(c => c.GiltigTill)
                .ToListAsync(ct);

            return Results.Ok(expiring.Select(c => new
            {
                c.Id, c.AnstallId, c.Namn,
                Typ = c.Typ.ToString(),
                c.GiltigTill, c.ArObligatorisk,
                Status = c.Status.ToString()
            }));
        }).WithName("ListExpiringCertifications");

        // ============================================================
        // Skapa certifiering
        // ============================================================

        kompetens.MapPost("/", async (CreateCertificationRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<CertificationType>(req.Typ, true, out var typ))
                return Results.BadRequest(new { error = $"Ogiltig typ: {req.Typ}. Giltiga värden: {string.Join(", ", Enum.GetNames<CertificationType>())}" });

            var cert = Certification.Skapa(
                req.AnstallId,
                req.Namn,
                typ,
                req.Utfardare,
                req.GiltigFran,
                req.GiltigTill,
                req.Obligatorisk);

            await db.Certifications.AddAsync(cert, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/kompetens/{cert.Id}", new
            {
                cert.Id, cert.AnstallId, cert.Namn,
                Typ = cert.Typ.ToString(),
                cert.GiltigFran, cert.GiltigTill,
                Status = cert.Status.ToString()
            });
        }).WithName("CreateCertification");

        // ============================================================
        // Lista obligatoriska utbildningar
        // ============================================================

        kompetens.MapGet("/obligatoriska", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var trainings = await db.MandatoryTrainings
                .OrderBy(t => t.RollNamn)
                .ThenBy(t => t.UtbildningNamn)
                .ToListAsync(ct);

            return Results.Ok(trainings);
        }).WithName("ListMandatoryTrainings");

        // ============================================================
        // Skapa obligatorisk utbildning
        // ============================================================

        kompetens.MapPost("/obligatorisk", async (CreateMandatoryTrainingRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var training = MandatoryTraining.Skapa(req.Roll, req.Utbildning, req.GiltighetManader, req.Beskrivning);
            await db.MandatoryTrainings.AddAsync(training, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/kompetens/obligatoriska", new
            {
                training.Id, training.RollNamn, training.UtbildningNamn,
                training.GiltighetManader
            });
        }).WithName("CreateMandatoryTraining");

        return app;
    }
}

// Request DTOs
record CreateCertificationRequest(Guid AnstallId, string Namn, string Typ, string? Utfardare, DateOnly? GiltigFran, DateOnly? GiltigTill, bool Obligatorisk);
record CreateMandatoryTrainingRequest(string Roll, string Utbildning, int GiltighetManader, string? Beskrivning);
