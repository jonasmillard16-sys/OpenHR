using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class MigrationEndpoints
{
    public static WebApplication MapMigrationEndpoints(this WebApplication app)
    {
        var migration = app.MapGroup("/api/v1/migration").WithTags("Migrering").RequireAuthorization();

        // ============================================================
        // Lista migreringsjobb
        // ============================================================

        migration.MapGet("/", async (int? sida, int? perSida, RegionHRDbContext db, CancellationToken ct) =>
        {
            var pageSize = Math.Clamp(perSida ?? 20, 1, 100);
            var page = Math.Max(sida ?? 1, 1);

            var total = await db.MigrationJobs.CountAsync(ct);
            var jobs = await db.MigrationJobs
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                sida = page,
                perSida = pageSize,
                totalt = total,
                jobb = jobs.Select(j => new
                {
                    id = j.Id.Value,
                    kalla = j.Kalla.ToString(),
                    status = j.Status.ToString(),
                    j.FilNamn,
                    j.TotaltAntalRader,
                    j.ImporteradeRader,
                    j.FelRader,
                    j.SkapadAv,
                    j.CreatedAt
                })
            });
        }).WithName("ListMigrationJobs");

        // ============================================================
        // Jobbdetalj
        // ============================================================

        migration.MapGet("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var job = await db.MigrationJobs
                .Include(j => j.Mappningar)
                .Include(j => j.ValideringsFel)
                .Include(j => j.Logg)
                .FirstOrDefaultAsync(j => j.Id == MigrationJobId.From(id), ct);

            if (job is null)
                return Results.NotFound(new { error = "Migreringsjobb hittades inte" });

            return Results.Ok(new
            {
                id = job.Id.Value,
                kalla = job.Kalla.ToString(),
                status = job.Status.ToString(),
                job.FilNamn,
                job.TotaltAntalRader,
                job.ImporteradeRader,
                job.FelRader,
                job.SkapadAv,
                job.FelMeddelande,
                job.CreatedAt,
                mappningar = job.Mappningar.Select(m => new { m.KallFalt, m.MalFalt, m.TransformationsRegel }),
                valideringsfel = job.ValideringsFel.Select(v => new { v.RadNummer, v.Falt, v.FelTyp, v.OriginalVarde, v.ForeslagnKorrektion }),
                logg = job.Logg.Select(l => new { l.EntityTyp, status = l.Status.ToString(), l.FelMeddelande })
            });
        }).WithName("GetMigrationJob");

        // ============================================================
        // Starta nytt migreringsjobb
        // ============================================================

        migration.MapPost("/", async (StartMigrationRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<SourceSystem>(req.KallSystem, true, out var sourceSystem))
                return Results.BadRequest(new { error = $"Ogiltigt källsystem: {req.KallSystem}. Giltiga: {string.Join(", ", Enum.GetNames<SourceSystem>())}" });

            var job = MigrationJob.Skapa(sourceSystem, req.FilNamn, req.SkapadAv);
            await db.MigrationJobs.AddAsync(job, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/migration/{job.Id.Value}", new
            {
                id = job.Id.Value,
                status = job.Status.ToString(),
                kalla = job.Kalla.ToString(),
                job.FilNamn
            });
        }).WithName("StartMigrationJob");

        // ============================================================
        // Lista mallar
        // ============================================================

        migration.MapGet("/mallar", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var templates = await db.MigrationTemplates
                .OrderBy(t => t.Namn)
                .ToListAsync(ct);

            return Results.Ok(templates.Select(t => new
            {
                t.Id,
                t.Namn,
                kallSystem = t.KallSystem.ToString(),
                t.Mappningar
            }));
        }).WithName("ListMigrationTemplates");

        return app;
    }
}

// Request DTO
record StartMigrationRequest(string KallSystem, string FilNamn, string SkapadAv);
