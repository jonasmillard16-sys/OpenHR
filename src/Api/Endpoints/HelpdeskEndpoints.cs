using Microsoft.EntityFrameworkCore;
using RegionHR.Helpdesk.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class HelpdeskEndpoints
{
    public static WebApplication MapHelpdeskEndpoints(this WebApplication app)
    {
        var hd = app.MapGroup("/api/v1/helpdesk").WithTags("Helpdesk").RequireAuthorization();

        // GET /api/v1/helpdesk — mina ärenden
        hd.MapGet("/", async (Guid? anstallId, string? status, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.ServiceRequests.AsQueryable();

            if (anstallId.HasValue)
                query = query.Where(r => r.InrapportadAv == EmployeeId.From(anstallId.Value));

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ServiceRequestStatus>(status, true, out var s))
                query = query.Where(r => r.Status == s);

            var requests = await query
                .OrderByDescending(r => r.CreatedAt)
                .Take(50)
                .ToListAsync(ct);

            return Results.Ok(requests.Select(r => new
            {
                r.Id,
                r.Titel,
                Prioritet = r.Prioritet.ToString(),
                Status = r.Status.ToString(),
                r.KategoriId,
                r.TilldeladAgent,
                r.SLADeadline,
                r.CreatedAt,
                r.LostVid
            }));
        }).WithName("ListHelpdeskRequests");

        // POST /api/v1/helpdesk — skapa ärende
        hd.MapPost("/", async (CreateServiceRequestDto req, RegionHRDbContext db, ServiceRequestRouter router, CancellationToken ct) =>
        {
            var request = ServiceRequest.Skapa(
                req.Titel,
                req.Beskrivning,
                req.KategoriId,
                req.Prioritet,
                req.KallKanal ?? "Portal",
                EmployeeId.From(req.InrapportadAv));

            await router.RouteAsync(request, ct);

            await db.ServiceRequests.AddAsync(request, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/helpdesk/{request.Id}", new
            {
                request.Id,
                request.Titel,
                Status = request.Status.ToString(),
                request.TilldeladAgent,
                request.SLADeadline
            });
        }).WithName("CreateHelpdeskRequest");

        // GET /api/v1/helpdesk/{id} — detalj
        hd.MapGet("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var request = await db.ServiceRequests
                .Include(r => r.Kommentarer)
                .Include(r => r.SLAMilestones)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (request is null) return Results.NotFound();

            return Results.Ok(new
            {
                request.Id,
                request.Titel,
                request.Beskrivning,
                Prioritet = request.Prioritet.ToString(),
                Status = request.Status.ToString(),
                request.KategoriId,
                request.KallKanal,
                InrapportadAv = request.InrapportadAv.Value,
                request.TilldeladAgent,
                request.TilldeladKo,
                request.SLADeadline,
                request.LostVid,
                request.StangdVid,
                request.NojdhetsPoang,
                request.CreatedAt,
                Kommentarer = request.Kommentarer.Select(k => new
                {
                    k.Id,
                    ForfattareId = k.ForfattareId?.Value,
                    k.Innehall,
                    k.ArIntern,
                    k.SkapadVid
                }),
                SLAMilestones = request.SLAMilestones.Select(m => new
                {
                    m.Id,
                    m.Typ,
                    m.MalTid,
                    m.FaktiskTid,
                    m.ArUppfylld
                })
            });
        }).WithName("GetHelpdeskRequest");

        // POST /api/v1/helpdesk/{id}/kommentar — lägg till kommentar
        hd.MapPost("/{id:guid}/kommentar", async (Guid id, AddCommentDto dto, RegionHRDbContext db, CancellationToken ct) =>
        {
            var request = await db.ServiceRequests
                .Include(r => r.Kommentarer)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
            if (request is null) return Results.NotFound();

            EmployeeId? authorId = dto.ForfattareId.HasValue ? EmployeeId.From(dto.ForfattareId.Value) : null;
            request.LaggTillKommentar(authorId, dto.Innehall, dto.ArIntern);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Kommentar tillagd" });
        }).WithName("AddHelpdeskComment");

        // PUT /api/v1/helpdesk/{id}/tilldela — tilldela agent
        hd.MapPut("/{id:guid}/tilldela", async (Guid id, AssignRequestDto dto, RegionHRDbContext db, CancellationToken ct) =>
        {
            var request = await db.ServiceRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (request is null) return Results.NotFound();

            request.Tilldela(dto.AgentId);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Ärende tilldelat", request.TilldeladAgent });
        }).WithName("AssignHelpdeskRequest");

        // PUT /api/v1/helpdesk/{id}/los — lös ärende
        hd.MapPut("/{id:guid}/los", async (Guid id, ResolveRequestDto dto, RegionHRDbContext db, CancellationToken ct) =>
        {
            var request = await db.ServiceRequests
                .Include(r => r.Kommentarer)
                .Include(r => r.SLAMilestones)
                .FirstOrDefaultAsync(r => r.Id == id, ct);
            if (request is null) return Results.NotFound();

            request.Los(dto.Losning);

            // Markera resolution milestone
            var resMilestone = request.SLAMilestones
                .FirstOrDefault(m => m.Typ == "Resolution" && m.FaktiskTid is null);
            resMilestone?.Uppfyll(DateTime.UtcNow);

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                message = "Ärende löst",
                Status = request.Status.ToString(),
                request.LostVid
            });
        }).WithName("ResolveHelpdeskRequest");

        // POST /api/v1/helpdesk/{id}/nojdhet — nöjdhetsbetyg
        hd.MapPost("/{id:guid}/nojdhet", async (Guid id, SatisfactionDto dto, RegionHRDbContext db, CancellationToken ct) =>
        {
            var request = await db.ServiceRequests.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (request is null) return Results.NotFound();

            request.SattNojdhet(dto.Poang);

            var satisfaction = CaseSatisfaction.Skapa(id, dto.Poang, dto.Kommentar);
            await db.CaseSatisfactions.AddAsync(satisfaction, ct);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { message = "Nöjdhet registrerad", dto.Poang });
        }).WithName("RateHelpdeskSatisfaction");

        // ============================================================
        // Agent endpoints
        // ============================================================

        // GET /api/v1/helpdesk/agent/mina — agentens ärenden
        hd.MapGet("/agent/mina/{agentId:guid}", async (Guid agentId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var requests = await db.ServiceRequests
                .Where(r => r.TilldeladAgent == agentId &&
                            r.Status != ServiceRequestStatus.Closed)
                .OrderBy(r => r.SLADeadline)
                .ThenByDescending(r => r.Prioritet)
                .Take(50)
                .ToListAsync(ct);

            return Results.Ok(requests.Select(r => new
            {
                r.Id,
                r.Titel,
                Prioritet = r.Prioritet.ToString(),
                Status = r.Status.ToString(),
                r.SLADeadline,
                r.CreatedAt,
                SLABreached = r.SLADeadline.HasValue && r.SLADeadline < DateTime.UtcNow
            }));
        }).WithName("ListAgentRequests");

        // GET /api/v1/helpdesk/agent/ko/{koId} — kövy
        hd.MapGet("/agent/ko/{koId:guid}", async (Guid koId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var requests = await db.ServiceRequests
                .Where(r => r.TilldeladKo == koId && r.TilldeladAgent == null)
                .OrderBy(r => r.SLADeadline)
                .ThenByDescending(r => r.Prioritet)
                .Take(50)
                .ToListAsync(ct);

            return Results.Ok(requests.Select(r => new
            {
                r.Id,
                r.Titel,
                Prioritet = r.Prioritet.ToString(),
                Status = r.Status.ToString(),
                r.SLADeadline,
                r.CreatedAt
            }));
        }).WithName("ListQueueRequests");

        // ============================================================
        // Admin endpoints — SLA, köer, mallar, kategorier
        // ============================================================

        // GET /api/v1/helpdesk/admin/sla
        hd.MapGet("/admin/sla", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var slas = await db.SLADefinitions.OrderBy(s => s.Namn).ToListAsync(ct);
            return Results.Ok(slas.Select(s => new
            {
                s.Id,
                s.Namn,
                s.ForsvarstidMinuter,
                s.LostidMinuter,
                s.EskaleringEfterMinuter,
                s.ArAktiv
            }));
        }).WithName("ListSLADefinitions");

        // GET /api/v1/helpdesk/admin/koer
        hd.MapGet("/admin/koer", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var queues = await db.HRQueues.OrderBy(q => q.Namn).ToListAsync(ct);
            return Results.Ok(queues.Select(q => new
            {
                q.Id,
                q.Namn,
                q.Beskrivning,
                AntalMedlemmar = q.Medlemmar.Count
            }));
        }).WithName("ListHRQueues");

        // GET /api/v1/helpdesk/admin/kategorier
        hd.MapGet("/admin/kategorier", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var categories = await db.ServiceCategories.OrderBy(c => c.Namn).ToListAsync(ct);
            return Results.Ok(categories.Select(c => new
            {
                c.Id,
                c.Namn,
                c.Beskrivning,
                c.ParentId,
                c.DefaultKoId,
                DefaultPrioritet = c.DefaultPrioritet?.ToString(),
                c.DefaultSLAId
            }));
        }).WithName("ListServiceCategories");

        // GET /api/v1/helpdesk/admin/mallar
        hd.MapGet("/admin/mallar", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var templates = await db.CaseTemplates_Helpdesk.OrderBy(t => t.Namn).ToListAsync(ct);
            return Results.Ok(templates.Select(t => new
            {
                t.Id,
                t.Namn,
                t.KategoriId,
                t.MallInnehall,
                t.Checklista
            }));
        }).WithName("ListCaseTemplates");

        // GET /api/v1/helpdesk/admin/sla/compliance — SLA compliance dashboard
        hd.MapGet("/admin/sla/compliance", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var total = await db.ServiceRequests.CountAsync(ct);
            var resolved = await db.ServiceRequests
                .CountAsync(r => r.Status == ServiceRequestStatus.Resolved || r.Status == ServiceRequestStatus.Closed, ct);
            var breached = await db.SLAMilestones
                .CountAsync(m => m.ArUppfylld == false, ct);
            var met = await db.SLAMilestones
                .CountAsync(m => m.ArUppfylld == true, ct);
            // Compute average resolution time in minutes.
            // We select both dates and compute the difference in-memory
            // because DateDiffMinute is SQL Server-specific (not available in Npgsql).
            var resolvedRequests = await db.ServiceRequests
                .Where(r => r.LostVid.HasValue)
                .Select(r => new { r.CreatedAt, LostVid = r.LostVid!.Value })
                .ToListAsync(ct);
            var avgResolution = resolvedRequests.Count > 0
                ? resolvedRequests.Average(r => (r.LostVid - r.CreatedAt).TotalMinutes)
                : 0.0;

            return Results.Ok(new
            {
                TotaltArenden = total,
                Losta = resolved,
                SLAUppfyllda = met,
                SLAOverskridna = breached,
                SLACompliancePercent = met + breached > 0 ? Math.Round(100.0 * met / (met + breached), 1) : 100.0,
                GenomsnittligLosningMinuter = Math.Round(avgResolution, 0)
            });
        }).WithName("GetSLACompliance");

        return app;
    }
}

// DTOs
record CreateServiceRequestDto(
    string Titel,
    string Beskrivning,
    Guid KategoriId,
    ServiceRequestPriority Prioritet,
    Guid InrapportadAv,
    string? KallKanal = "Portal");

record AddCommentDto(string Innehall, Guid? ForfattareId = null, bool ArIntern = false);
record AssignRequestDto(Guid AgentId);
record ResolveRequestDto(string Losning);
record SatisfactionDto(int Poang, string? Kommentar = null);
