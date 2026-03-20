namespace RegionHR.PolicyManagement.Domain;

/// <summary>
/// Bekräftelse att en anställd har läst och godkänt en policy.
/// Kopplad till verklig AnstallId — ingen anonymitet, ingen Guid.Empty.
/// </summary>
public sealed class PolicyConfirmation
{
    public Guid Id { get; private set; }
    public Guid PolicyId { get; private set; }
    public Guid AnstallId { get; private set; }
    public int PolicyVersion { get; private set; }
    public DateTime BekraftadVid { get; private set; }

    private PolicyConfirmation() { }

    public static PolicyConfirmation Skapa(Guid policyId, Guid anstallId, int policyVersion)
    {
        if (policyId == Guid.Empty)
            throw new ArgumentException("PolicyId krävs.", nameof(policyId));
        if (anstallId == Guid.Empty)
            throw new ArgumentException("AnstallId krävs — bekräftelser måste kopplas till verklig anställd.", nameof(anstallId));

        return new PolicyConfirmation
        {
            Id = Guid.NewGuid(),
            PolicyId = policyId,
            AnstallId = anstallId,
            PolicyVersion = policyVersion,
            BekraftadVid = DateTime.UtcNow
        };
    }
}
