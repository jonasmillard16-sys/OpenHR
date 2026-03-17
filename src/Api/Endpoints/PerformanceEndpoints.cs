using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Performance.Domain;

namespace RegionHR.Api.Endpoints;

public static class PerformanceEndpoints
{
    public static WebApplication MapPerformanceEndpoints(this WebApplication app)
    {
        var samtal = app.MapGroup("/api/v1/medarbetarsamtal").WithTags("Medarbetarsamtal").RequireAuthorization();

        // ============================================================
        // Lista medarbetarsamtal för år
        // ============================================================

        samtal.MapGet("/", async (int ar, RegionHRDbContext db, CancellationToken ct) =>
        {
            var reviews = await db.PerformanceReviews
                .Where(r => r.Ar == ar)
                .OrderByDescending(r => r.SkapadVid)
                .ToListAsync(ct);

            return Results.Ok(reviews.Select(r => new
            {
                r.Id, r.AnstallId, r.ChefId, r.Ar,
                Status = r.Status.ToString(),
                r.OverallRating, r.SkapadVid, r.GenomfordVid
            }));
        }).WithName("ListPerformanceReviews");

        // ============================================================
        // Hämta medarbetarsamtal
        // ============================================================

        samtal.MapGet("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var review = await db.PerformanceReviews.FirstOrDefaultAsync(r => r.Id == id, ct);
            return review is not null ? Results.Ok(review) : Results.NotFound();
        }).WithName("GetPerformanceReview");

        // ============================================================
        // Skapa medarbetarsamtal
        // ============================================================

        samtal.MapPost("/", async (CreatePerformanceReviewRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var review = PerformanceReview.Skapa(req.AnstallId, req.ChefId, req.Ar);
            await db.PerformanceReviews.AddAsync(review, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/medarbetarsamtal/{review.Id}", new
            {
                review.Id, review.AnstallId, review.ChefId, review.Ar,
                Status = review.Status.ToString()
            });
        }).WithName("CreatePerformanceReview");

        // ============================================================
        // Självbedömning
        // ============================================================

        samtal.MapPost("/{id:guid}/sjalvbedomning", async (Guid id, SjalvbedomningRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var review = await db.PerformanceReviews.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (review is null) return Results.NotFound();

            try
            {
                review.SattSjalvbedomning(req.Bedomning);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { review.Id, Status = review.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("SubmitSelfAssessment");

        // ============================================================
        // Chefsbedömning
        // ============================================================

        samtal.MapPost("/{id:guid}/chefsbedomning", async (Guid id, ChefsbedomningRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var review = await db.PerformanceReviews.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (review is null) return Results.NotFound();

            try
            {
                review.SattChefsbedomning(req.Bedomning, req.OverallRating);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { review.Id, Status = review.Status.ToString(), review.OverallRating });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("SubmitManagerAssessment");

        // ============================================================
        // Målsättning
        // ============================================================

        samtal.MapPost("/{id:guid}/malsattning", async (Guid id, MalsattningRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var review = await db.PerformanceReviews.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (review is null) return Results.NotFound();

            try
            {
                review.SattMalsattning(req.Malsattning);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { review.Id, review.Malsattning });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("SetGoals");

        // ============================================================
        // Genomför medarbetarsamtal
        // ============================================================

        samtal.MapPost("/{id:guid}/genomfor", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var review = await db.PerformanceReviews.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (review is null) return Results.NotFound();

            try
            {
                review.Genomfor();
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { review.Id, Status = review.Status.ToString(), review.GenomfordVid });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("CompletePerformanceReview");

        return app;
    }
}

// Request DTOs
record CreatePerformanceReviewRequest(Guid AnstallId, Guid ChefId, int Ar);
record SjalvbedomningRequest(string Bedomning);
record ChefsbedomningRequest(string Bedomning, int OverallRating);
record MalsattningRequest(string Malsattning);
