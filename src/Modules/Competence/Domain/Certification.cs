namespace RegionHR.Competence.Domain;

public enum CertificationType
{
    Legitimation,
    Specialisering,
    ObligatoriskUtbildning,
    FrivilligUtbildning,
    Certifikat
}

public enum CertificationStatus
{
    Giltig,
    UtgarSnart,
    Utgangen,
    Saknas
}

public class Certification
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public string Namn { get; private set; } = default!;
    public CertificationType Typ { get; private set; }
    public string? Utfardare { get; private set; }
    public DateOnly? GiltigFran { get; private set; }
    public DateOnly? GiltigTill { get; private set; }
    public bool ArObligatorisk { get; private set; }
    public CertificationStatus Status { get; private set; }

    private Certification() { }

    public static Certification Skapa(
        Guid anstallId,
        string namn,
        CertificationType typ,
        string? utfardare,
        DateOnly? giltigFran,
        DateOnly? giltigTill,
        bool obligatorisk = false)
    {
        var cert = new Certification
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Namn = namn,
            Typ = typ,
            Utfardare = utfardare,
            GiltigFran = giltigFran,
            GiltigTill = giltigTill,
            ArObligatorisk = obligatorisk
        };
        cert.Status = cert.BeraknaStatus(DateOnly.FromDateTime(DateTime.UtcNow));
        return cert;
    }

    public CertificationStatus BeraknaStatus(DateOnly idag)
    {
        if (GiltigTill is null)
            return CertificationStatus.Giltig;

        if (idag > GiltigTill.Value)
            return CertificationStatus.Utgangen;

        if (GiltigTill.Value.DayNumber - idag.DayNumber <= 90)
            return CertificationStatus.UtgarSnart;

        return CertificationStatus.Giltig;
    }
}
