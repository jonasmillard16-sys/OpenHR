namespace RegionHR.Benefits.Domain;

public enum EnrollmentStatus { Ansokt, Aktiv, Avslutad, Nekad }

public class EmployeeBenefit
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public Guid BenefitId { get; private set; }
    public EnrollmentStatus Status { get; private set; }
    public DateOnly StartDatum { get; private set; }
    public DateOnly? SlutDatum { get; private set; }
    public decimal ValtBelopp { get; private set; }
    public string? LivshandardAnledning { get; private set; } // life event reason
    public DateTime SkapadVid { get; private set; }

    private EmployeeBenefit() { }

    public static EmployeeBenefit Anmala(Guid anstallId, Guid benefitId, DateOnly startDatum, decimal valtBelopp, string? livshandelse = null)
    {
        return new EmployeeBenefit
        {
            Id = Guid.NewGuid(), AnstallId = anstallId, BenefitId = benefitId,
            Status = EnrollmentStatus.Ansokt, StartDatum = startDatum,
            ValtBelopp = valtBelopp, LivshandardAnledning = livshandelse,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void Godkann() { Status = EnrollmentStatus.Aktiv; }
    public void Neka() { Status = EnrollmentStatus.Nekad; }
    public void Avsluta(DateOnly slutDatum) { Status = EnrollmentStatus.Avslutad; SlutDatum = slutDatum; }
}
