namespace RegionHR.Reporting.Domain;

public enum ExecutionStatus
{
    Koar,
    Pagar,
    Klar,
    Fel
}

public class ReportExecution
{
    public Guid Id { get; private set; }
    public Guid ReportDefinitionId { get; private set; }
    public DateTime StartadVid { get; private set; }
    public DateTime? SlutfordVid { get; private set; }
    public ExecutionStatus Status { get; private set; }
    public string? ResultatFilSokvag { get; private set; }
    public string? FelMeddelande { get; private set; }
    public string? Parametrar { get; private set; }

    private ReportExecution() { }

    public static ReportExecution Starta(Guid reportId, string? parametrar = null)
    {
        return new ReportExecution
        {
            Id = Guid.NewGuid(),
            ReportDefinitionId = reportId,
            StartadVid = DateTime.UtcNow,
            Status = ExecutionStatus.Koar,
            Parametrar = parametrar
        };
    }

    public void Slutfor(string filSokvag)
    {
        Status = ExecutionStatus.Klar;
        SlutfordVid = DateTime.UtcNow;
        ResultatFilSokvag = filSokvag;
    }

    public void MarkeraFel(string meddelande)
    {
        Status = ExecutionStatus.Fel;
        SlutfordVid = DateTime.UtcNow;
        FelMeddelande = meddelande;
    }
}
