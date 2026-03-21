using RegionHR.SharedKernel.Domain;

namespace RegionHR.VMS.Domain;

/// <summary>
/// F-skatt registrering för leverantör eller inhyrd personal.
/// Spårar om en leverantör/konsult har giltig F-skattsedel.
/// Utan F-skatt: 30% källskatt + arbetsgivaravgifter.
/// </summary>
public sealed class FSkattRegistration
{
    public Guid Id { get; private set; }
    public Guid? ContingentWorkerId { get; private set; }
    public VendorId? VendorId { get; private set; }
    public string Organisationsnummer { get; private set; } = string.Empty;
    public FSkattStatus FSkattStatus { get; private set; }
    public DateTime KontrolleradVid { get; private set; }
    public DateOnly? GiltigTill { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    private FSkattRegistration() { } // EF Core

    public static FSkattRegistration Skapa(
        string organisationsnummer,
        FSkattStatus status,
        DateOnly? giltigTill,
        Guid? contingentWorkerId = null,
        VendorId? vendorId = null)
    {
        if (contingentWorkerId is null && vendorId is null)
            throw new ArgumentException("Antingen ContingentWorkerId eller VendorId måste anges.");

        return new FSkattRegistration
        {
            Id = Guid.NewGuid(),
            ContingentWorkerId = contingentWorkerId,
            VendorId = vendorId,
            Organisationsnummer = organisationsnummer,
            FSkattStatus = status,
            KontrolleradVid = DateTime.UtcNow,
            GiltigTill = giltigTill
        };
    }

    public void Uppdatera(FSkattStatus status, DateOnly? giltigTill)
    {
        FSkattStatus = status;
        GiltigTill = giltigTill;
        KontrolleradVid = DateTime.UtcNow;
    }

    /// <summary>
    /// Kontrollerar om F-skatten snart går ut (inom angivet antal dagar).
    /// </summary>
    public bool SnartUtgående(int dagar = 30)
    {
        if (GiltigTill is null) return false;
        return GiltigTill.Value <= DateOnly.FromDateTime(DateTime.Today).AddDays(dagar);
    }

    /// <summary>
    /// Kontrollerar om F-skatten har gått ut.
    /// </summary>
    public bool HarGåttUt()
    {
        if (GiltigTill is null) return false;
        return GiltigTill.Value < DateOnly.FromDateTime(DateTime.Today);
    }

    /// <summary>
    /// Anger om 30% källskatt + arbetsgivaravgifter krävs.
    /// </summary>
    public bool KräverSkatteavdrag => FSkattStatus != FSkattStatus.Active || HarGåttUt();
}
