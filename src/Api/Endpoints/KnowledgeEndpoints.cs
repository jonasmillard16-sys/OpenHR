using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.Knowledge.Domain;

namespace RegionHR.Api.Endpoints;

public static class KnowledgeEndpoints
{
    public static WebApplication MapKnowledgeEndpoints(this WebApplication app)
    {
        var kb = app.MapGroup("/api/v1/knowledge").WithTags("Kunskapsbas").RequireAuthorization();

        // ============================================================
        // Articles — search/list
        // ============================================================
        kb.MapGet("/articles", async (string? query, Guid? kategoriId, int? top,
            KnowledgeBaseService kbService, CancellationToken ct) =>
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                var results = await kbService.SearchAsync(query, ct);
                return Results.Ok(results.Select(a => new
                {
                    a.Id, a.Titel, Sammanfattning = a.HamtaSammanfattning(),
                    a.KategoriId, Taggar = a.HamtaTaggar(), a.VisningsAntal, a.HjalpsamhetPoang
                }));
            }

            if (kategoriId.HasValue)
            {
                var results = await kbService.GetByCategoryAsync(kategoriId.Value, ct);
                return Results.Ok(results.Select(a => new
                {
                    a.Id, a.Titel, Sammanfattning = a.HamtaSammanfattning(),
                    a.KategoriId, Taggar = a.HamtaTaggar(), a.VisningsAntal, a.HjalpsamhetPoang
                }));
            }

            var popular = await kbService.GetPopularAsync(top ?? 10, ct);
            return Results.Ok(popular.Select(a => new
            {
                a.Id, a.Titel, Sammanfattning = a.HamtaSammanfattning(),
                a.KategoriId, Taggar = a.HamtaTaggar(), a.VisningsAntal, a.HjalpsamhetPoang
            }));
        }).WithName("SearchKnowledgeArticles");

        // ============================================================
        // Article detail
        // ============================================================
        kb.MapGet("/articles/{id:guid}", async (Guid id, KnowledgeBaseService kbService, CancellationToken ct) =>
        {
            var article = await kbService.GetArticleAsync(id, ct);
            if (article is null) return Results.NotFound();

            return Results.Ok(new
            {
                article.Id, article.Titel, article.Innehall, article.KategoriId,
                Taggar = article.HamtaTaggar(), article.ArPublicerad,
                article.SkapadVid, article.UppdateradVid,
                article.VisningsAntal, article.HjalpsamhetPoang
            });
        }).WithName("GetKnowledgeArticle");

        // ============================================================
        // Categories
        // ============================================================
        kb.MapGet("/categories", async (KnowledgeBaseService kbService, CancellationToken ct) =>
        {
            var categories = await kbService.GetCategoriesAsync(ct);
            return Results.Ok(categories.Select(c => new
            {
                c.Id, c.Namn, c.Beskrivning, c.Ordning, c.Ikon
            }));
        }).WithName("ListKnowledgeCategories");

        // ============================================================
        // Rate article helpfulness
        // ============================================================
        kb.MapPost("/articles/{id:guid}/rating", async (Guid id, RateArticleRequest req,
            KnowledgeBaseService kbService, CancellationToken ct) =>
        {
            var success = await kbService.RateArticleAsync(id, req.Poang, ct);
            return success
                ? Results.Ok(new { message = "Tack för din feedback!" })
                : Results.NotFound(new { error = "Artikeln hittades inte" });
        }).WithName("RateKnowledgeArticle");

        return app;
    }
}

record RateArticleRequest(decimal Poang);
