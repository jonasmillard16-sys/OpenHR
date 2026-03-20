namespace RegionHR.Reporting.Domain;

public class ScheduledReport
{
    public Guid Id { get; private set; }
    public Guid ReportTemplateId { get; private set; } // FK to report_definitions
    public string Frekvens { get; private set; } = ""; // Daily/Weekly/Monthly
    public string Mottagare { get; private set; } = "";
    public string Format { get; private set; } = ""; // PDF/Excel/CSV
    public DateTime? SenastKord { get; private set; }
    public DateTime? NastaKorning { get; private set; }

    private ScheduledReport() { }

    public static ScheduledReport Skapa(
        Guid reportTemplateId, string frekvens, string mottagare, string format)
    {
        var scheduled = new ScheduledReport
        {
            Id = Guid.NewGuid(),
            ReportTemplateId = reportTemplateId,
            Frekvens = frekvens,
            Mottagare = mottagare,
            Format = format
        };
        scheduled.BeraknaNextKorning();
        return scheduled;
    }

    public void MarkeraSomKord()
    {
        SenastKord = DateTime.UtcNow;
        BeraknaNextKorning();
    }

    public void UppdateraFrekvens(string frekvens, string mottagare, string format)
    {
        Frekvens = frekvens;
        Mottagare = mottagare;
        Format = format;
        BeraknaNextKorning();
    }

    private void BeraknaNextKorning()
    {
        var nu = DateTime.UtcNow;
        NastaKorning = Frekvens switch
        {
            "Daily" => nu.Date.AddDays(1).AddHours(6),
            "Weekly" => nu.Date.AddDays(7 - (int)nu.DayOfWeek).AddHours(6),
            "Monthly" => new DateTime(nu.Year, nu.Month, 1).AddMonths(1).AddHours(6),
            _ => nu.AddDays(1)
        };
    }
}
