namespace RegionHR.Infrastructure.Provisioning;

/// <summary>
/// Kontrakt för extern identitetsprovisionering.
///
/// I v1 finns enbart LocalRecordingProvider som registrerar
/// händelser i databasen utan att anropa externa system.
///
/// Framtida implementationer:
/// - EntraIdProvider (Microsoft Entra ID via SCIM 2.0)
/// - ActiveDirectoryProvider (LDAP)
/// - GoogleWorkspaceProvider (Admin SDK)
/// </summary>
public interface IIdentityProvider
{
    /// <summary>
    /// Registrerar en provisioneringsåtgärd.
    /// I LocalRecordingProvider: sparar till DB.
    /// I framtida providers: anropar externt system och uppdaterar status.
    /// </summary>
    Task<ProvisioningEvent> RecordAsync(ProvisioningEvent evt, CancellationToken ct = default);
}
