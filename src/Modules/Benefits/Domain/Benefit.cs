namespace RegionHR.Benefits.Domain;

public enum BenefitCategory { Friskvard, Pension, Forsakring, Tjanstebil, Sjukvard, Utbildning, Ovrigt }
public enum BenefitStatus { Aktiv, Inaktiv, UtgarSnart }

public class Benefit
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = "";
    public string Beskrivning { get; private set; } = "";
    public BenefitCategory Kategori { get; private set; }
    public decimal MaxBelopp { get; private set; }
    public decimal ArbetsgivarAndel { get; private set; } // percentage
    public decimal ArbetstagarAndel { get; private set; } // percentage
    public bool ArSkattepliktig { get; private set; }
    public bool ArAktiv { get; private set; }
    public string? EligibilityRegler { get; private set; } // JSON eligibility rules
    public DateTime SkapadVid { get; private set; }

    private Benefit() { }

    public static Benefit Skapa(string namn, string beskrivning, BenefitCategory kategori, decimal maxBelopp, decimal arbetsgivarAndel, bool skattepliktig, string? eligibilityRegler = null)
    {
        return new Benefit
        {
            Id = Guid.NewGuid(), Namn = namn, Beskrivning = beskrivning,
            Kategori = kategori, MaxBelopp = maxBelopp,
            ArbetsgivarAndel = arbetsgivarAndel, ArbetstagarAndel = 100 - arbetsgivarAndel,
            ArSkattepliktig = skattepliktig, ArAktiv = true,
            EligibilityRegler = eligibilityRegler, SkapadVid = DateTime.UtcNow
        };
    }

    public void Inaktivera() { ArAktiv = false; }
    public void Aktivera() { ArAktiv = true; }
}
