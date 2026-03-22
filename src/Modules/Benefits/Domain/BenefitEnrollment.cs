namespace RegionHR.Benefits.Domain;

public class BenefitEnrollment
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public Guid BenefitId { get; private set; }
    // TODO: Migrate Status from string to BenefitEnrollmentStatus enum
    public string Status { get; private set; } = "Pending";
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
            Status = "Pending",
            StartDatum = startDatum,
            ValdNiva = valdNiva,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void Aktivera()
    {
        if (Status is not "Pending")
            throw new InvalidOperationException($"Kan inte aktivera en enrollment med status {Status}");
        Status = "Active";
    }

    public void Avbryt()
    {
        if (Status is "Cancelled")
            throw new InvalidOperationException("Enrollment är redan avbruten");
        Status = "Cancelled";
    }
}
