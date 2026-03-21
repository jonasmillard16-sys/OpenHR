using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Knowledge.Domain;

namespace RegionHR.Infrastructure.Services;

public class KnowledgeBaseService
{
    private readonly RegionHRDbContext _db;

    public KnowledgeBaseService(RegionHRDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Sök artiklar med ILIKE-matchning mot titel, innehåll och taggar.
    /// Returnerar publicerade artiklar sorterade efter relevans (titel-träff viktas högst).
    /// </summary>
    public async Task<List<KnowledgeArticle>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        var terms = query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (terms.Length == 0)
            return [];

        // Get all published articles and score them in-memory for InMemory DB compatibility
        var articles = await _db.KnowledgeArticles
            .Where(a => a.ArPublicerad)
            .ToListAsync(ct);

        var scored = articles
            .Select(a => new
            {
                Article = a,
                Score = CalculateRelevanceScore(a, terms)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(10)
            .Select(x => x.Article)
            .ToList();

        return scored;
    }

    private static int CalculateRelevanceScore(KnowledgeArticle article, string[] terms)
    {
        var score = 0;
        var titelLower = article.Titel.ToLower();
        var innehallLower = article.Innehall.ToLower();
        var taggarLower = article.TaggarJson.ToLower();

        foreach (var term in terms)
        {
            if (titelLower.Contains(term)) score += 10;
            if (taggarLower.Contains(term)) score += 5;
            if (innehallLower.Contains(term)) score += 2;
        }

        return score;
    }

    /// <summary>Hämta de mest visade artiklarna.</summary>
    public async Task<List<KnowledgeArticle>> GetPopularAsync(int count = 5, CancellationToken ct = default)
    {
        return await _db.KnowledgeArticles
            .Where(a => a.ArPublicerad)
            .OrderByDescending(a => a.VisningsAntal)
            .Take(count)
            .ToListAsync(ct);
    }

    /// <summary>Hämta artiklar per kategori.</summary>
    public async Task<List<KnowledgeArticle>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default)
    {
        return await _db.KnowledgeArticles
            .Where(a => a.KategoriId == categoryId && a.ArPublicerad)
            .OrderBy(a => a.Titel)
            .ToListAsync(ct);
    }

    /// <summary>Hämta alla kategorier sorterade efter ordning.</summary>
    public async Task<List<KnowledgeCategory>> GetCategoriesAsync(CancellationToken ct = default)
    {
        return await _db.KnowledgeCategories
            .OrderBy(c => c.Ordning)
            .ToListAsync(ct);
    }

    /// <summary>Hämta artikel med ökad visningsräknare.</summary>
    public async Task<KnowledgeArticle?> GetArticleAsync(Guid id, CancellationToken ct = default)
    {
        var article = await _db.KnowledgeArticles.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (article != null)
        {
            article.OkaVisning();
            await _db.SaveChangesAsync(ct);
        }
        return article;
    }

    /// <summary>Uppdatera hjälpsamhetspoäng (rullande medelvärde).</summary>
    public async Task<bool> RateArticleAsync(Guid id, decimal rating, CancellationToken ct = default)
    {
        var article = await _db.KnowledgeArticles.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (article == null) return false;

        // Simple rolling average: (old * 0.7) + (new * 0.3)
        var newScore = article.HjalpsamhetPoang == 0m
            ? rating
            : Math.Round(article.HjalpsamhetPoang * 0.7m + rating * 0.3m, 1);
        article.UppdateraHjalpsamhet(newScore);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
