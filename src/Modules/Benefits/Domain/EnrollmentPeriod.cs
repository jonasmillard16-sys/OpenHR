namespace RegionHR.Benefits.Domain;

public class EnrollmentPeriod
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public DateOnly StartDatum { get; private set; }
    public DateOnly SlutDatum { get; private set; }
    public string? InkluderadePlaner { get; private set; } // JSON array of Benefit IDs
    public string Status { get; private set; } = "Upcoming";
    public DateTime SkapadVid { get; private set; }

    private EnrollmentPeriod() { }

    public static EnrollmentPeriod Skapa(string namn, DateOnly startDatum, DateOnly slutDatum, string? inkluderadePlaner = null)
    {
        if (slutDatum < startDatum)
            throw new ArgumentException("Slutdatum kan inte vara före startdatum", nameof(slutDatum));

        return new EnrollmentPeriod
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            StartDatum = startDatum,
            SlutDatum = slutDatum,
            InkluderadePlaner = inkluderadePlaner,
            Status = "Upcoming",
            SkapadVid = DateTime.UtcNow
        };
    }

    public void Oppna()
    {
        Status = "Open";
    }

    public void Stang()
    {
        Status = "Closed";
    }

    public bool ArOppen()
    {
        var idag = DateOnly.FromDateTime(DateTime.UtcNow);
        return Status == "Open" && idag >= StartDatum && idag <= SlutDatum;
    }

    public bool ArOppen(DateOnly datum)
    {
        return Status == "Open" && datum >= StartDatum && datum <= SlutDatum;
    }
}
