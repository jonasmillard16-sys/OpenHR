namespace RegionHR.Offboarding.Domain;

public enum OffboardingStatus { Skapad, Pagar, Slutford }
public enum AvslutAnledning { EgenBegaran, Uppsagning, Pension, Vikariat_Slut, Provanstallning_Avbruten, Overgang, Dodsfall, Annat }

public class OffboardingCase
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public AvslutAnledning Anledning { get; private set; }
    public DateOnly SistaArbetsdag { get; private set; }
    public OffboardingStatus Status { get; private set; }
    public string? ExitSamtalKommentar { get; private set; }
    public bool ExitSamtalGenomfort { get; private set; }
    public bool ArReHireEligible { get; private set; }
    public string? ReHireKommentar { get; private set; }
    public DateTime SkapadVid { get; private set; }
    public DateTime? SlutfordVid { get; private set; }
    public List<OffboardingItem> Steg { get; private set; } = new();

    private OffboardingCase() { }

    public static OffboardingCase Skapa(Guid anstallId, AvslutAnledning anledning, DateOnly sistaArbetsdag)
    {
        if (sistaArbetsdag < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Sista arbetsdag måste vara idag eller i framtiden");

        var c = new OffboardingCase
        {
            Id = Guid.NewGuid(), AnstallId = anstallId, Anledning = anledning,
            SistaArbetsdag = sistaArbetsdag, Status = OffboardingStatus.Skapad,
            ArReHireEligible = true, SkapadVid = DateTime.UtcNow
        };
        // Standard offboarding steps
        c.Steg.Add(new OffboardingItem("Återlämning av utrustning (dator, telefon, nycklar, passerkort)"));
        c.Steg.Add(new OffboardingItem("Stängning av IT-behörigheter och systemkonton"));
        c.Steg.Add(new OffboardingItem("Slutdokument: tjänstgöringsintyg utfärdat"));
        c.Steg.Add(new OffboardingItem("Slutdokument: arbetsgivarintyg (AF) utfärdat"));
        c.Steg.Add(new OffboardingItem("Slutlön beräknad (semester, komptid, övertid)"));
        c.Steg.Add(new OffboardingItem("Exit-samtal genomfört"));
        c.Steg.Add(new OffboardingItem("Kunskapsöverföring genomförd"));
        c.Steg.Add(new OffboardingItem("GDPR-gallringsplan upprättad"));
        return c;
    }

    public void MarkeraSomPagar() { Status = OffboardingStatus.Pagar; }

    public void MarkeraStegKlart(int index)
    {
        if (index < 0 || index >= Steg.Count) throw new ArgumentOutOfRangeException(nameof(index));
        Steg[index].MarkeraKlar();
    }

    public void RegistreraExitSamtal(string kommentar)
    {
        ExitSamtalKommentar = kommentar;
        ExitSamtalGenomfort = true;
    }

    public void SattReHireStatus(bool eligible, string? kommentar = null)
    {
        ArReHireEligible = eligible;
        ReHireKommentar = kommentar;
    }

    public void Slutfor()
    {
        if (Steg.Any(s => !s.Klar))
            throw new InvalidOperationException("Alla steg måste vara klara innan offboarding kan slutföras");
        Status = OffboardingStatus.Slutford;
        SlutfordVid = DateTime.UtcNow;
    }
}

public class OffboardingItem
{
    public Guid Id { get; private set; }
    public string Beskrivning { get; private set; } = "";
    public bool Klar { get; private set; }
    public DateTime? KlarVid { get; private set; }
    public string? Kommentar { get; private set; }

    private OffboardingItem() { }
    public OffboardingItem(string beskrivning) { Id = Guid.NewGuid(); Beskrivning = beskrivning; }
    public void MarkeraKlar(string? kommentar = null) { Klar = true; KlarVid = DateTime.UtcNow; Kommentar = kommentar; }
}
