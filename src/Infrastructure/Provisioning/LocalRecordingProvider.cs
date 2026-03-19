using Microsoft.Extensions.Logging;
using RegionHR.Infrastructure.Persistence;

namespace RegionHR.Infrastructure.Provisioning;

/// <summary>
/// Lokal provider som registrerar provisioneringshändelser i databasen.
/// Anropar INGA externa system. Status sätts alltid till RegistreradLokalt.
/// </summary>
public class LocalRecordingProvider : IIdentityProvider
{
    private readonly RegionHRDbContext _db;
    private readonly ILogger<LocalRecordingProvider> _logger;

    public LocalRecordingProvider(RegionHRDbContext db, ILogger<LocalRecordingProvider> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<ProvisioningEvent> RecordAsync(ProvisioningEvent evt, CancellationToken ct = default)
    {
        _db.ProvisioningEvents.Add(evt);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Provisionering registrerad lokalt: {Aktion} för {AnstallNamn} i {System} (ingen extern koppling)",
            evt.Aktion, evt.AnstallNamn, evt.TargetSystem);

        return evt;
    }
}
