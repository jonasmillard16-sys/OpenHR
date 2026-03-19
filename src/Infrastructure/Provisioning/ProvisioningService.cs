using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;

namespace RegionHR.Infrastructure.Provisioning;

/// <summary>
/// Beräknar vilka provisioneringsåtgärder som ska skapas
/// baserat på konfigurerade regler och en given trigger.
/// Skriver events via IIdentityProvider.
/// </summary>
public class ProvisioningService
{
    private readonly RegionHRDbContext _db;
    private readonly IIdentityProvider _provider;

    public ProvisioningService(RegionHRDbContext db, IIdentityProvider provider)
    {
        _db = db;
        _provider = provider;
    }

    /// <summary>
    /// Beräknar och registrerar provisioneringsåtgärder för en anställd
    /// baserat på aktiva regler som matchar given trigger.
    /// </summary>
    public async Task<List<ProvisioningEvent>> BeraknaOchRegistreraAsync(
        Guid anstallId,
        string anstallNamn,
        ProvisioningTrigger trigger,
        CancellationToken ct = default)
    {
        var rules = await _db.ProvisioningRules
            .Where(r => r.ArAktiv && r.Trigger == trigger)
            .ToListAsync(ct);

        var events = new List<ProvisioningEvent>();

        foreach (var rule in rules)
        {
            var evt = ProvisioningEvent.Skapa(
                anstallId,
                anstallNamn,
                rule.TargetSystem,
                rule.Aktion,
                trigger,
                rule.Beskrivning);

            var recorded = await _provider.RecordAsync(evt, ct);
            events.Add(recorded);
        }

        return events;
    }
}
