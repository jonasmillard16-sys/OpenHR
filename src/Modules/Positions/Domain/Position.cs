namespace RegionHR.Positions.Domain;

public enum PositionStatus { Aktiv, Vakant, Frysta, Avvecklad }

public class Position
{
    public Guid Id { get; private set; }
    public Guid EnhetId { get; private set; }
    public string Titel { get; private set; } = "";
    public string? BESTAKod { get; private set; }
    public string? AIDKod { get; private set; }
    public PositionStatus Status { get; private set; }
    public decimal BudgeteradManadslon { get; private set; }
    public decimal Sysselsattningsgrad { get; private set; } // FTE 0-100
    public Guid? InnehavareAnstallId { get; private set; }
    public Guid? EftertradarePlanerad { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public DateTime? AvveckladVid { get; private set; }
    public List<PositionHistorik> Historik { get; private set; } = new();
    public List<string> KravdaKompetenser { get; private set; } = new();

    private Position() { }

    public static Position Skapa(Guid enhetId, string titel, decimal budgeteradManadslon, decimal sysselsattningsgrad, string? bestaKod = null, string? aidKod = null)
    {
        return new Position
        {
            Id = Guid.NewGuid(), EnhetId = enhetId, Titel = titel,
            BESTAKod = bestaKod, AIDKod = aidKod,
            Status = PositionStatus.Vakant,
            BudgeteradManadslon = budgeteradManadslon,
            Sysselsattningsgrad = sysselsattningsgrad,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void Tillsatt(Guid anstallId)
    {
        if (Status == PositionStatus.Avvecklad) throw new InvalidOperationException("Kan inte tillsätta avvecklad position");
        Historik.Add(new PositionHistorik(Id, InnehavareAnstallId, anstallId, DateTime.UtcNow));
        InnehavareAnstallId = anstallId;
        Status = PositionStatus.Aktiv;
    }

    public void Vakansatt(string? anledning = null)
    {
        Historik.Add(new PositionHistorik(Id, InnehavareAnstallId, null, DateTime.UtcNow, anledning));
        InnehavareAnstallId = null;
        Status = PositionStatus.Vakant;
    }

    public void Frys() { Status = PositionStatus.Frysta; }
    public void Avveckla() { Status = PositionStatus.Avvecklad; AvveckladVid = DateTime.UtcNow; }
    public void SattEftertrardare(Guid anstallId) { EftertradarePlanerad = anstallId; }
    public void UppdateraBudget(decimal nyManadslon) { BudgeteradManadslon = nyManadslon; }
    public void LaggTillKompetenskrav(string kompetens) { if (!KravdaKompetenser.Contains(kompetens)) KravdaKompetenser.Add(kompetens); }
}
