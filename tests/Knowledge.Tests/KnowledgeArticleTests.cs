using RegionHR.Knowledge.Domain;
using Xunit;

namespace RegionHR.Knowledge.Tests;

public class KnowledgeArticleTests
{
    private readonly Guid _kategoriId = Guid.NewGuid();

    [Fact]
    public void Skapa_SkaparArtikelMedKorrekteVarden()
    {
        var article = KnowledgeArticle.Skapa("Test", "Innehåll", _kategoriId, ["tag1", "tag2"]);

        Assert.Equal("Test", article.Titel);
        Assert.Equal("Innehåll", article.Innehall);
        Assert.Equal(_kategoriId, article.KategoriId);
        Assert.False(article.ArPublicerad);
        Assert.Equal(0, article.VisningsAntal);
        Assert.Equal(0m, article.HjalpsamhetPoang);
    }

    [Fact]
    public void Skapa_KastarVidTomTitel()
    {
        Assert.Throws<ArgumentException>(() =>
            KnowledgeArticle.Skapa("", "Innehåll", _kategoriId, []));
    }

    [Fact]
    public void Skapa_KastarVidTomtInnehall()
    {
        Assert.Throws<ArgumentException>(() =>
            KnowledgeArticle.Skapa("Test", "", _kategoriId, []));
    }

    [Fact]
    public void Publicera_SatterArPublicerad()
    {
        var article = KnowledgeArticle.Skapa("Test", "Innehåll", _kategoriId, []);

        article.Publicera();

        Assert.True(article.ArPublicerad);
    }

    [Fact]
    public void Publicera_KastarOmRedanPublicerad()
    {
        var article = KnowledgeArticle.Skapa("Test", "Innehåll", _kategoriId, []);
        article.Publicera();

        Assert.Throws<InvalidOperationException>(() => article.Publicera());
    }

    [Fact]
    public void Avpublicera_SatterArPubliceradTillFalse()
    {
        var article = KnowledgeArticle.Skapa("Test", "Innehåll", _kategoriId, []);
        article.Publicera();

        article.Avpublicera();

        Assert.False(article.ArPublicerad);
    }

    [Fact]
    public void Avpublicera_KastarOmIntPublicerad()
    {
        var article = KnowledgeArticle.Skapa("Test", "Innehåll", _kategoriId, []);

        Assert.Throws<InvalidOperationException>(() => article.Avpublicera());
    }

    [Fact]
    public void OkaVisning_OkarMedEtt()
    {
        var article = KnowledgeArticle.Skapa("Test", "Innehåll", _kategoriId, []);

        article.OkaVisning();
        article.OkaVisning();
        article.OkaVisning();

        Assert.Equal(3, article.VisningsAntal);
    }

    [Fact]
    public void UppdateraHjalpsamhet_SatterPoang()
    {
        var article = KnowledgeArticle.Skapa("Test", "Innehåll", _kategoriId, []);

        article.UppdateraHjalpsamhet(4.5m);

        Assert.Equal(4.5m, article.HjalpsamhetPoang);
    }

    [Fact]
    public void UppdateraHjalpsamhet_KastarVidForHogPoang()
    {
        var article = KnowledgeArticle.Skapa("Test", "Innehåll", _kategoriId, []);

        Assert.Throws<ArgumentOutOfRangeException>(() => article.UppdateraHjalpsamhet(5.1m));
    }

    [Fact]
    public void UppdateraHjalpsamhet_KastarVidNegativPoang()
    {
        var article = KnowledgeArticle.Skapa("Test", "Innehåll", _kategoriId, []);

        Assert.Throws<ArgumentOutOfRangeException>(() => article.UppdateraHjalpsamhet(-0.1m));
    }

    [Fact]
    public void UppdateraHjalpsamhet_AccepterarGransvarden()
    {
        var article = KnowledgeArticle.Skapa("Test", "Innehåll", _kategoriId, []);

        article.UppdateraHjalpsamhet(0m);
        Assert.Equal(0m, article.HjalpsamhetPoang);

        article.UppdateraHjalpsamhet(5m);
        Assert.Equal(5m, article.HjalpsamhetPoang);
    }

    [Fact]
    public void HamtaTaggar_ReturnerarRattLista()
    {
        var article = KnowledgeArticle.Skapa("Test", "Innehåll", _kategoriId, ["semester", "ledighet"]);

        var taggar = article.HamtaTaggar();

        Assert.Equal(2, taggar.Count);
        Assert.Contains("semester", taggar);
        Assert.Contains("ledighet", taggar);
    }

    [Fact]
    public void HamtaSammanfattning_TrunkerarLangText()
    {
        var longText = new string('A', 500);
        var article = KnowledgeArticle.Skapa("Test", longText, _kategoriId, []);

        var summary = article.HamtaSammanfattning(200);

        Assert.True(summary.Length <= 204); // 200 + "..."
        Assert.EndsWith("...", summary);
    }

    [Fact]
    public void HamtaSammanfattning_ReturnHeltInnehallOmKort()
    {
        var article = KnowledgeArticle.Skapa("Test", "Kort text", _kategoriId, []);

        var summary = article.HamtaSammanfattning(200);

        Assert.Equal("Kort text", summary);
    }

    [Fact]
    public void UppdateraInnehall_AndrarTitelOchInnehall()
    {
        var article = KnowledgeArticle.Skapa("Gammal", "Gammalt innehåll", _kategoriId, ["tag1"]);

        article.UppdateraInnehall("Ny titel", "Nytt innehåll", ["tag2", "tag3"]);

        Assert.Equal("Ny titel", article.Titel);
        Assert.Equal("Nytt innehåll", article.Innehall);
        Assert.Equal(2, article.HamtaTaggar().Count);
        Assert.NotNull(article.UppdateradVid);
    }
}
