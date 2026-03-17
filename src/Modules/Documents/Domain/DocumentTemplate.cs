namespace RegionHR.Documents.Domain;

public class DocumentTemplate
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public DocumentCategory Kategori { get; private set; }
    public string MallInnehall { get; private set; } = ""; // HTML/text with merge fields like {{Fornamn}}, {{Efternamn}}
    public List<string> MergeFields { get; private set; } = new();
    public DateTime SkapadVid { get; private set; }

    private DocumentTemplate() { }

    public static DocumentTemplate Skapa(string namn, DocumentCategory kategori, string mallInnehall, List<string>? mergeFields = null)
    {
        return new DocumentTemplate
        {
            Id = Guid.NewGuid(), Namn = namn, Kategori = kategori,
            MallInnehall = mallInnehall,
            MergeFields = mergeFields ?? new List<string>(),
            SkapadVid = DateTime.UtcNow
        };
    }

    public string GenerateContent(Dictionary<string, string> values)
    {
        var content = MallInnehall;
        foreach (var kvp in values)
            content = content.Replace("{{" + kvp.Key + "}}", kvp.Value);
        return content;
    }
}
