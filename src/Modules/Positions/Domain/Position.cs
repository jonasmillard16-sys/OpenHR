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
    /// <summary>
    /// LEGACY: Fritextlista. Ersatt av PositionSkillRequirement för gap-analys.
    /// Behålls för bakåtkompatibilitet men används inte i ny funktionalitet.
    /// </summary>
    [Obsolete("Använd PositionSkillRequirement istället")]
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
    [Obsolete("Använd PositionSkillRequirement istället")]
    public void LaggTillKompetenskrav(string kompetens)
    {
        #pragma warning disable CS0618
        if (!KravdaKompetenser.Contains(kompetens)) KravdaKompetenser.Add(kompetens);
        #pragma warning restore CS0618
    }
}
