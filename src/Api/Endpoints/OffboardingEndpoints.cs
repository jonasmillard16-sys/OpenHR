using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Offboarding.Domain;

namespace RegionHR.Api.Endpoints;

public static class OffboardingEndpoints
{
    public static WebApplication MapOffboardingEndpoints(this WebApplication app)
    {
        var offboarding = app.MapGroup("/api/v1/offboarding").WithTags("Offboarding").RequireAuthorization();

        // ============================================================
        // Lista offboarding-ärenden
        // ============================================================

        offboarding.MapGet("/", async (Guid? anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.OffboardingCases.AsQueryable();
            if (anstallId.HasValue)
                query = query.Where(o => o.AnstallId == anstallId.Value);

            var cases = await query.OrderByDescending(o => o.SkapadVid).ToListAsync(ct);

            return Results.Ok(cases.Select(o => new
            {
                o.Id, o.AnstallId,
                Anledning = o.Anledning.ToString(),
                o.SistaArbetsdag,
                Status = o.Status.ToString(),
                o.ExitSamtalGenomfort, o.ArReHireEligible,
                o.SkapadVid, o.SlutfordVid,
                AntalSteg = o.Steg.Count,
                KlaraSteg = o.Steg.Count(s => s.Klar)
            }));
        }).WithName("ListOffboardingCases");

        // ============================================================
        // Skapa offboarding-ärende
        // ============================================================

        offboarding.MapPost("/", async (CreateOffboardingRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<AvslutAnledning>(req.Anledning, true, out var anledning))
                return Results.BadRequest(new { error = $"Ogiltig anledning: {req.Anledning}. Giltiga värden: {string.Join(", ", Enum.GetNames<AvslutAnledning>())}" });

            var offboardingCase = OffboardingCase.Skapa(req.AnstallId, anledning, req.SistaArbetsdag);
            await db.OffboardingCases.AddAsync(offboardingCase, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/offboarding/{offboardingCase.Id}", new
            {
                offboardingCase.Id, offboardingCase.AnstallId,
                Anledning = offboardingCase.Anledning.ToString(),
                offboardingCase.SistaArbetsdag,
                Status = offboardingCase.Status.ToString(),
                Steg = offboardingCase.Steg.Select((s, i) => new { Index = i, s.Beskrivning, s.Klar })
            });
        }).WithName("CreateOffboardingCase");

        // ============================================================
        // Markera steg som klart
        // ============================================================

        offboarding.MapPost("/{id:guid}/steg/{index:int}/klar", async (Guid id, int index, RegionHRDbContext db, CancellationToken ct) =>
        {
            var offboardingCase = await db.OffboardingCases.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (offboardingCase is null) return Results.NotFound();

            try
            {
                offboardingCase.MarkeraStegKlart(index);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new
                {
                    offboardingCase.Id,
                    Steg = offboardingCase.Steg.Select((s, i) => new { Index = i, s.Beskrivning, s.Klar, s.KlarVid })
                });
            }
            catch (ArgumentOutOfRangeException)
            {
                return Results.BadRequest(new { error = $"Ogiltigt stegindex: {index}" });
            }
        }).WithName("CompleteOffboardingStep");

        // ============================================================
        // Registrera exit-samtal
        // ============================================================

        offboarding.MapPost("/{id:guid}/exitsamtal", async (Guid id, ExitSamtalRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var offboardingCase = await db.OffboardingCases.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (offboardingCase is null) return Results.NotFound();

            offboardingCase.RegistreraExitSamtal(req.Kommentar);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { offboardingCase.Id, offboardingCase.ExitSamtalGenomfort, offboardingCase.ExitSamtalKommentar });
        }).WithName("RecordExitInterview");

        // ============================================================
        // Sätt rehire-status
        // ============================================================

        offboarding.MapPost("/{id:guid}/rehire", async (Guid id, ReHireRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var offboardingCase = await db.OffboardingCases.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (offboardingCase is null) return Results.NotFound();

            offboardingCase.SattReHireStatus(req.Eligible, req.Kommentar);
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { offboardingCase.Id, offboardingCase.ArReHireEligible, offboardingCase.ReHireKommentar });
        }).WithName("SetReHireStatus");

        // ============================================================
        // Slutför offboarding
        // ============================================================

        offboarding.MapPost("/{id:guid}/slutfor", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var offboardingCase = await db.OffboardingCases.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (offboardingCase is null) return Results.NotFound();

            try
            {
                offboardingCase.Slutfor();
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { offboardingCase.Id, Status = offboardingCase.Status.ToString(), offboardingCase.SlutfordVid });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("CompleteOffboarding");

        return app;
    }
}

// Request DTOs
record CreateOffboardingRequest(Guid AnstallId, string Anledning, DateOnly SistaArbetsdag);
record ExitSamtalRequest(string Kommentar);
record ReHireRequest(bool Eligible, string? Kommentar = null);
