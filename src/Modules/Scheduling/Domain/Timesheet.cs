namespace RegionHR.Scheduling.Domain;

public enum TimesheetStatus { Oppen, Inskickad, Godkand, Avslagen }

public class Timesheet
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public int Ar { get; private set; }
    public int Manad { get; private set; }
    public TimesheetStatus Status { get; private set; }
    public decimal PlaneradeTimmar { get; private set; }
    public decimal FaktiskaTimmar { get; private set; }
    public decimal Avvikelse => FaktiskaTimmar - PlaneradeTimmar;
    public decimal Overtid { get; private set; }
    public Guid? GodkandAv { get; private set; }
    public DateTime? GodkandVid { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public string? Kommentar { get; private set; }

    private Timesheet() { }

    public static Timesheet Skapa(Guid anstallId, int ar, int manad, decimal planeradeTimmar)
    {
        return new Timesheet
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Ar = ar,
            Manad = manad,
            Status = TimesheetStatus.Oppen,
            PlaneradeTimmar = planeradeTimmar,
            FaktiskaTimmar = 0,
            Overtid = 0,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void RegistreraTimmar(decimal faktiska, decimal overtid = 0)
    {
        if (Status != TimesheetStatus.Oppen)
            throw new InvalidOperationException("Kan bara registrera timmar på öppen tidrapport");
        FaktiskaTimmar = faktiska;
        Overtid = overtid;
    }

    public void SkickaIn()
    {
        if (Status != TimesheetStatus.Oppen)
            throw new InvalidOperationException("Kan bara skicka in öppen tidrapport");
        Status = TimesheetStatus.Inskickad;
    }

    public void Godkann(Guid godkannare, string? kommentar = null)
    {
        if (Status != TimesheetStatus.Inskickad)
            throw new InvalidOperationException("Kan bara godkänna inskickad tidrapport");
        Status = TimesheetStatus.Godkand;
        GodkandAv = godkannare;
        GodkandVid = DateTime.UtcNow;
        Kommentar = kommentar;
    }

    public void Avvisa(Guid godkannare, string kommentar)
    {
        if (Status != TimesheetStatus.Inskickad)
            throw new InvalidOperationException("Kan bara avvisa inskickad tidrapport");
        Status = TimesheetStatus.Avslagen;
        GodkandAv = godkannare;
        GodkandVid = DateTime.UtcNow;
        Kommentar = kommentar;
    }

    public void AteroppnaEfterAvvisning()
    {
        if (Status != TimesheetStatus.Avslagen)
            throw new InvalidOperationException("Kan bara återöppna avslagen tidrapport");
        Status = TimesheetStatus.Oppen;
        GodkandAv = null;
        GodkandVid = null;
    }
}
