using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.Knowledge.Domain;
using Xunit;

namespace RegionHR.Knowledge.Tests;

public class KnowledgeBaseServiceTests : IDisposable
{
    private readonly RegionHRDbContext _db;
    private readonly KnowledgeBaseService _service;

    public KnowledgeBaseServiceTests()
    {
        var options = new DbContextOptionsBuilder<RegionHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new RegionHRDbContext(options);
        _service = new KnowledgeBaseService(_db);
        SeedTestData();
    }

    private void SeedTestData()
    {
        var cat = KnowledgeCategory.Skapa("Test", "Testkategori", 1, "Article");
        _db.KnowledgeCategories.Add(cat);

        var art1 = KnowledgeArticle.Skapa("Semesterregler", "Information om semester och ledighet", cat.Id, ["semester", "ledighet"]);
        art1.Publicera();
        var art2 = KnowledgeArticle.Skapa("OB-tillägg", "OB-tillägg för obekväm arbetstid", cat.Id, ["OB", "tillägg", "natt"]);
        art2.Publicera();
        var art3 = KnowledgeArticle.Skapa("Sjukanmälan steg", "Hur man sjukanmäler sig", cat.Id, ["sjuk", "anmälan"]);
        art3.Publicera();
        var art4 = KnowledgeArticle.Skapa("Opublicerad artikel", "Ska inte visas", cat.Id, ["test"]);
        // NOT published

        // Set different view counts
        for (int i = 0; i < 10; i++) art1.OkaVisning();
        for (int i = 0; i < 5; i++) art2.OkaVisning();

        _db.KnowledgeArticles.AddRange(art1, art2, art3, art4);
        _db.SaveChanges();
    }

    [Fact]
    public async Task SearchAsync_HittarArtikelPaTitel()
    {
        var results = await _service.SearchAsync("semester");

        Assert.Single(results);
        Assert.Equal("Semesterregler", results[0].Titel);
    }

    [Fact]
    public async Task SearchAsync_HittarArtikelPaTaggar()
    {
        var results = await _service.SearchAsync("natt");

        Assert.Single(results);
        Assert.Equal("OB-tillägg", results[0].Titel);
    }

    [Fact]
    public async Task SearchAsync_HittarArtikelPaInnehall()
    {
        var results = await _service.SearchAsync("obekväm arbetstid");

        Assert.Contains(results, a => a.Titel == "OB-tillägg");
    }

    [Fact]
    public async Task SearchAsync_ExkluderarOpublicerade()
    {
        var results = await _service.SearchAsync("test");

        Assert.DoesNotContain(results, a => a.Titel == "Opublicerad artikel");
    }

    [Fact]
    public async Task SearchAsync_ReturnerarTomListaForTomQuery()
    {
        var results = await _service.SearchAsync("");

        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_ReturnerarTomListaForNull()
    {
        var results = await _service.SearchAsync(null!);

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetPopularAsync_SorterarEfterVisningsAntal()
    {
        var results = await _service.GetPopularAsync(2);

        Assert.Equal(2, results.Count);
        Assert.Equal("Semesterregler", results[0].Titel); // 10 visningar
        Assert.Equal("OB-tillägg", results[1].Titel); // 5 visningar
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnerarEndastPubliceradeIKategori()
    {
        var cat = (await _db.KnowledgeCategories.FirstAsync());
        var results = await _service.GetByCategoryAsync(cat.Id);

        Assert.Equal(3, results.Count); // 3 publicerade av 4 totalt
        Assert.DoesNotContain(results, a => a.Titel == "Opublicerad artikel");
    }

    [Fact]
    public async Task GetArticleAsync_OkarVisningsAntal()
    {
        var article = await _db.KnowledgeArticles.FirstAsync(a => a.Titel == "Sjukanmälan steg");
        var initialViews = article.VisningsAntal;

        var result = await _service.GetArticleAsync(article.Id);

        Assert.NotNull(result);
        Assert.Equal(initialViews + 1, result!.VisningsAntal);
    }

    [Fact]
    public async Task GetArticleAsync_ReturnerarNullForOkandId()
    {
        var result = await _service.GetArticleAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task RateArticleAsync_SatterInitialPoang()
    {
        var article = await _db.KnowledgeArticles.FirstAsync(a => a.Titel == "Sjukanmälan steg");

        var success = await _service.RateArticleAsync(article.Id, 4.0m);

        Assert.True(success);
        var updated = await _db.KnowledgeArticles.FirstAsync(a => a.Id == article.Id);
        Assert.Equal(4.0m, updated.HjalpsamhetPoang);
    }

    [Fact]
    public async Task RateArticleAsync_BeraknarRullandeMedelvarde()
    {
        var article = await _db.KnowledgeArticles.FirstAsync(a => a.Titel == "Sjukanmälan steg");

        await _service.RateArticleAsync(article.Id, 4.0m);
        await _service.RateArticleAsync(article.Id, 2.0m);

        var updated = await _db.KnowledgeArticles.FirstAsync(a => a.Id == article.Id);
        // (4.0 * 0.7) + (2.0 * 0.3) = 2.8 + 0.6 = 3.4
        Assert.Equal(3.4m, updated.HjalpsamhetPoang);
    }

    [Fact]
    public async Task RateArticleAsync_ReturnerarFalseForOkandId()
    {
        var success = await _service.RateArticleAsync(Guid.NewGuid(), 3.0m);

        Assert.False(success);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
