namespace RegionHR.Benefits.Domain;

public class LifeEvent
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string Typ { get; private set; } = "";
    public string? TillatnaAndringar { get; private set; } // JSON
    public int TidsFonsterDagar { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private LifeEvent() { }

    private static readonly string[] GiltigaTyper =
    [
        "Nyanstallning", "Gift", "BarnFott", "Skilsmassa",
        "Befattningsandring", "Sysselsattningsandring", "Avtalsandring",
        "Alder65", "Avslut"
    ];

    public static LifeEvent Skapa(string namn, string typ, int tidsFonsterDagar, string? tillatnaAndringar = null)
    {
        if (!GiltigaTyper.Contains(typ))
            throw new ArgumentException($"Ogiltig typ: {typ}. Giltiga: {string.Join(", ", GiltigaTyper)}", nameof(typ));
        if (tidsFonsterDagar < 0)
            throw new ArgumentException("Tidsfönster kan inte vara negativt", nameof(tidsFonsterDagar));

        return new LifeEvent
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Typ = typ,
            TidsFonsterDagar = tidsFonsterDagar,
            TillatnaAndringar = tillatnaAndringar,
            SkapadVid = DateTime.UtcNow
        };
    }

    public bool ArInomTidsFonster(DateOnly handelseDatum)
    {
        if (TidsFonsterDagar == 0) return true; // 0 = always valid
        var senasteDatum = handelseDatum.AddDays(TidsFonsterDagar);
        return DateOnly.FromDateTime(DateTime.UtcNow) <= senasteDatum;
    }
}
