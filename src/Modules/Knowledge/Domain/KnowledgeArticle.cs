using System.Text.Json;

namespace RegionHR.Knowledge.Domain;

public class KnowledgeArticle
{
    public Guid Id { get; private set; }
    public string Titel { get; private set; } = "";
    public string Innehall { get; private set; } = "";
    public Guid KategoriId { get; private set; }
    public string TaggarJson { get; private set; } = "[]";
    public bool ArPublicerad { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public DateTime? UppdateradVid { get; private set; }
    public int VisningsAntal { get; private set; }
    public decimal HjalpsamhetPoang { get; private set; }

    private KnowledgeArticle() { }

    public static KnowledgeArticle Skapa(string titel, string innehall, Guid kategoriId, List<string> taggar)
    {
        if (string.IsNullOrWhiteSpace(titel))
            throw new ArgumentException("Titel krävs", nameof(titel));
        if (string.IsNullOrWhiteSpace(innehall))
            throw new ArgumentException("Innehåll krävs", nameof(innehall));

        return new KnowledgeArticle
        {
            Id = Guid.NewGuid(),
            Titel = titel,
            Innehall = innehall,
            KategoriId = kategoriId,
            TaggarJson = JsonSerializer.Serialize(taggar),
            ArPublicerad = false,
            SkapadVid = DateTime.UtcNow,
            VisningsAntal = 0,
            HjalpsamhetPoang = 0m
        };
    }

    public void Publicera()
    {
        if (ArPublicerad)
            throw new InvalidOperationException("Artikeln är redan publicerad");
        ArPublicerad = true;
        UppdateradVid = DateTime.UtcNow;
    }

    public void Avpublicera()
    {
        if (!ArPublicerad)
            throw new InvalidOperationException("Artikeln är redan avpublicerad");
        ArPublicerad = false;
        UppdateradVid = DateTime.UtcNow;
    }

    public void OkaVisning()
    {
        VisningsAntal++;
    }

    public void UppdateraHjalpsamhet(decimal poang)
    {
        if (poang < 0m || poang > 5m)
            throw new ArgumentOutOfRangeException(nameof(poang), "Poäng måste vara mellan 0 och 5");
        HjalpsamhetPoang = poang;
        UppdateradVid = DateTime.UtcNow;
    }

    public void UppdateraInnehall(string titel, string innehall, List<string> taggar)
    {
        if (string.IsNullOrWhiteSpace(titel))
            throw new ArgumentException("Titel krävs", nameof(titel));
        if (string.IsNullOrWhiteSpace(innehall))
            throw new ArgumentException("Innehåll krävs", nameof(innehall));

        Titel = titel;
        Innehall = innehall;
        TaggarJson = JsonSerializer.Serialize(taggar);
        UppdateradVid = DateTime.UtcNow;
    }

    public List<string> HamtaTaggar()
    {
        try { return JsonSerializer.Deserialize<List<string>>(TaggarJson) ?? []; }
        catch { return []; }
    }

    /// <summary>Returnerar de första ~200 tecknen av innehållet som sammanfattning.</summary>
    public string HamtaSammanfattning(int maxLength = 200)
    {
        if (Innehall.Length <= maxLength) return Innehall;
        var cutoff = Innehall.LastIndexOf(' ', maxLength);
        if (cutoff < 50) cutoff = maxLength;
        return Innehall[..cutoff] + "...";
    }
}
