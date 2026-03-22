namespace RegionHR.Benefits.Domain;

public enum BenefitEnrollmentStatus { Pending, Active, Cancelled }

public class BenefitEnrollment
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public Guid BenefitId { get; private set; }
    public BenefitEnrollmentStatus Status { get; private set; } = BenefitEnrollmentStatus.Pending;
    public DateOnly StartDatum { get; private set; }
    public string? ValdNiva { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private BenefitEnrollment() { }

    public static BenefitEnrollment Skapa(Guid anstallId, Guid benefitId, DateOnly startDatum, string? valdNiva = null)
    {
        return new BenefitEnrollment
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            BenefitId = benefitId,
            Status = BenefitEnrollmentStatus.Pending,
            StartDatum = startDatum,
            ValdNiva = valdNiva,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void Aktivera()
    {
        if (Status != BenefitEnrollmentStatus.Pending)
            throw new InvalidOperationException($"Kan inte aktivera en enrollment med status {Status}");
        Status = BenefitEnrollmentStatus.Active;
    }

    public void Avbryt()
    {
        if (Status == BenefitEnrollmentStatus.Cancelled)
            throw new InvalidOperationException("Enrollment är redan avbruten");
        Status = BenefitEnrollmentStatus.Cancelled;
    }
}
