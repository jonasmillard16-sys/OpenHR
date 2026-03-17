namespace RegionHR.Reporting.Domain;

public enum ReportType
{
    Personalrostter,
    Loneregister,
    Franvarostatistik,
    Overtidsrapport,
    LASStatus,
    Bemanningsanalys,
    SjukfranvaroKPI,
    KostnadPerEnhet,
    AdHoc
}

public class ReportDefinition
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = default!;
    public string Beskrivning { get; private set; } = default!;
    public ReportType Typ { get; private set; }
    public string? ParameterSchema { get; private set; }
    public bool ArSchemalagd { get; private set; }
    public string? CronExpression { get; private set; }
    public string? MottagareEpost { get; private set; }

    private ReportDefinition() { }

    public static ReportDefinition Skapa(string namn, string beskrivning, ReportType typ)
    {
        return new ReportDefinition
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            Beskrivning = beskrivning,
            Typ = typ,
            ArSchemalagd = false
        };
    }

    public void SattSchemalagd(string cronExpression, string mottagareEpost)
    {
        CronExpression = cronExpression;
        MottagareEpost = mottagareEpost;
        ArSchemalagd = true;
    }
}
