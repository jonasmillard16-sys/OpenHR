using Microsoft.EntityFrameworkCore;
using RegionHR.Helpdesk.Domain;
using RegionHR.Infrastructure.Persistence;

namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Router för serviceärenden. Hanterar automatisk kötilldelning,
/// round-robin agentfördelning och SLA-beräkning.
/// </summary>
public class ServiceRequestRouter
{
    private readonly RegionHRDbContext _db;
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Guid, int> _roundRobinIndex = new();
    private const int MaxTrackedQueues = 200;

    private static void CleanupIfNeeded()
    {
        if (_roundRobinIndex.Count > MaxTrackedQueues)
            _roundRobinIndex.Clear();
    }

    public ServiceRequestRouter(RegionHRDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Routar ett nytt serviceärende baserat på kategori.
    /// Tilldelar kö, agent (round-robin) och SLA-milstolpar.
    /// </summary>
    public async Task RouteAsync(ServiceRequest request, CancellationToken ct = default)
    {
        var category = await _db.ServiceCategories
            .FirstOrDefaultAsync(c => c.Id == request.KategoriId, ct);

        if (category is null) return;

        // Tilldela till standardkö
        if (category.DefaultKoId.HasValue)
        {
            request.TilldelaKo(category.DefaultKoId.Value);

            // Round-robin tilldelning inom kön
            var queue = await _db.HRQueues
                .FirstOrDefaultAsync(q => q.Id == category.DefaultKoId.Value, ct);

            if (queue?.Medlemmar is { Count: > 0 })
            {
                var agentId = GetNextAgent(queue);
                request.Tilldela(agentId);
            }
        }

        // SLA-beräkning
        var slaId = request.SLADefinitionId ?? category.DefaultSLAId;
        if (slaId.HasValue)
        {
            var sla = await _db.SLADefinitions
                .FirstOrDefaultAsync(s => s.Id == slaId.Value && s.ArAktiv, ct);

            if (sla is not null)
            {
                var now = DateTime.UtcNow;
                var deadline = now.AddMinutes(sla.LostidMinuter);
                request.StallInSLA(sla.Id, deadline);

                // Skapa SLA-milstolpar
                var responseMilestone = SLAMilestone.Skapa(
                    request.Id, "Response", now.AddMinutes(sla.ForsvarstidMinuter));
                request.LaggTillSLAMilestone(responseMilestone);

                var resolutionMilestone = SLAMilestone.Skapa(
                    request.Id, "Resolution", deadline);
                request.LaggTillSLAMilestone(resolutionMilestone);
            }
        }
    }

    /// <summary>
    /// Round-robin: väljer nästa agent i kön och uppdaterar index.
    /// </summary>
    private Guid GetNextAgent(HRQueue queue)
    {
        CleanupIfNeeded();
        if (!_roundRobinIndex.TryGetValue(queue.Id, out var index))
            index = 0;

        var agentId = queue.Medlemmar[index % queue.Medlemmar.Count];
        _roundRobinIndex[queue.Id] = (index + 1) % queue.Medlemmar.Count;

        return agentId;
    }

    /// <summary>
    /// Beräknar SLA-deadline baserat på SLA-definition.
    /// </summary>
    public static DateTime CalculateDeadline(SLADefinition sla, DateTime startTime)
    {
        return startTime.AddMinutes(sla.LostidMinuter);
    }

    /// <summary>
    /// Kontrollerar om svarstid-SLA överskrids och bör eskaleras.
    /// </summary>
    public async Task<List<ServiceRequest>> GetBreachedRequestsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        return await _db.ServiceRequests
            .Where(r =>
                r.SLADeadline.HasValue &&
                r.SLADeadline < now &&
                r.Status != ServiceRequestStatus.Resolved &&
                r.Status != ServiceRequestStatus.Closed)
            .OrderBy(r => r.SLADeadline)
            .ToListAsync(ct);
    }
}
