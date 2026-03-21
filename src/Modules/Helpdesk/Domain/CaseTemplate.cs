namespace RegionHR.Helpdesk.Domain;

/// <summary>
/// Svars-/ärendemall med kategoritagg och checklista.
/// Agenter kan använda mallar för snabbare svar.
/// </summary>
public sealed class CaseTemplate
{
    public Guid Id { get; set; }
    public string Namn { get; set; } = string.Empty;
    public Guid KategoriId { get; set; }
    public string MallInnehall { get; set; } = string.Empty;
    public List<string> Checklista { get; set; } = [];

    public static CaseTemplate Skapa(string namn, Guid kategoriId, string mallInnehall, List<string>? checklista = null)
    {
        return new CaseTemplate
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            KategoriId = kategoriId,
            MallInnehall = mallInnehall,
            Checklista = checklista ?? []
        };
    }
}
