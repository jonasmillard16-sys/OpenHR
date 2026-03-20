using Microsoft.EntityFrameworkCore;
using RegionHR.Automation.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Api.Endpoints;

public static class AutomationEndpoints
{
    public static WebApplication MapAutomationEndpoints(this WebApplication app)
    {
        var automation = app.MapGroup("/api/v1/automation").WithTags("Automatisering").RequireAuthorization();

        // ============================================================
        // Kategorier med nivå
        // ============================================================

        automation.MapGet("/kategorier", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var categories = await db.AutomationCategories.ToListAsync(ct);
            var levelConfigs = await db.AutomationLevelConfigs.ToListAsync(ct);

            return Results.Ok(categories.Select(c =>
            {
                var config = levelConfigs.FirstOrDefault(lc => lc.KategoriId == c.Id);
                return new
                {
                    id = c.Id.Value,
                    c.Namn,
                    c.Beskrivning,
                    c.Ikon,
                    valdNiva = config?.ValdNiva.ToString() ?? AutomationLevel.Notify.ToString()
                };
            }));
        }).WithName("ListAutomationCategories");

        // ============================================================
        // Ändra nivå för en kategori
        // ============================================================

        automation.MapPut("/kategorier/{id:guid}/niva", async (Guid id, ChangeAutomationLevelRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<AutomationLevel>(req.Niva, true, out var newLevel))
                return Results.BadRequest(new { error = $"Ogiltig nivå: {req.Niva}. Giltiga: {string.Join(", ", Enum.GetNames<AutomationLevel>())}" });

            var categoryId = AutomationCategoryId.From(id);
            var config = await db.AutomationLevelConfigs
                .FirstOrDefaultAsync(lc => lc.KategoriId == categoryId, ct);

            if (config is null)
            {
                config = AutomationLevelConfig.Skapa(categoryId, newLevel);
                await db.AutomationLevelConfigs.AddAsync(config, ct);
            }
            else
            {
                config.AndraNiva(newLevel, AutomationLevel.Notify);
            }

            await db.SaveChangesAsync(ct);
            return Results.Ok(new { kategoriId = id, niva = newLevel.ToString() });
        }).WithName("ChangeAutomationLevel");

        // ============================================================
        // Regler
        // ============================================================

        automation.MapGet("/regler", async (Guid? kategoriId, bool? aktiva, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.AutomationRules.AsQueryable();

            if (kategoriId.HasValue)
                query = query.Where(r => r.KategoriId == AutomationCategoryId.From(kategoriId.Value));
            if (aktiva.HasValue)
                query = query.Where(r => r.ArAktiv == aktiva.Value);

            var rules = await query.OrderBy(r => r.Namn).ToListAsync(ct);

            return Results.Ok(rules.Select(r => new
            {
                id = r.Id.Value,
                r.Namn,
                kategoriId = r.KategoriId.Value,
                r.TriggerTyp,
                r.ArAktiv,
                minimumNiva = r.MinimumNiva.ToString(),
                r.ArSystemRegel
            }));
        }).WithName("ListAutomationRules");

        // ============================================================
        // Körningslogg
        // ============================================================

        automation.MapGet("/logg", async (int? sida, int? perSida, RegionHRDbContext db, CancellationToken ct) =>
        {
            var pageSize = Math.Clamp(perSida ?? 50, 1, 200);
            var page = Math.Max(sida ?? 1, 1);

            var total = await db.AutomationExecutions.CountAsync(ct);
            var executions = await db.AutomationExecutions
                .OrderByDescending(e => e.Tidsstampel)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return Results.Ok(new
            {
                sida = page,
                perSida = pageSize,
                totalt = total,
                korningar = executions.Select(e => new
                {
                    e.Id,
                    regelId = e.RegelId.Value,
                    e.HandelseTyp,
                    e.Resultat,
                    anvandNiva = e.AnvandNiva.ToString(),
                    e.UtfordAtgard,
                    e.Tidsstampel
                })
            });
        }).WithName("ListAutomationExecutionLog");

        // ============================================================
        // Förslag (väntande)
        // ============================================================

        automation.MapGet("/forslag", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var suggestions = await db.AutomationSuggestions
                .Where(s => s.Status == SuggestionStatus.Pending && s.GiltigTill > DateTime.UtcNow)
                .OrderByDescending(s => s.SkapadVid)
                .ToListAsync(ct);

            return Results.Ok(suggestions.Select(s => new
            {
                s.Id,
                regelId = s.RegelId.Value,
                s.ForeslagenAtgard,
                skapadFor = s.SkapadFor?.Value,
                status = s.Status.ToString(),
                s.SkapadVid,
                s.GiltigTill
            }));
        }).WithName("ListAutomationSuggestions");

        // ============================================================
        // Acceptera förslag
        // ============================================================

        automation.MapPost("/forslag/{id:guid}/acceptera", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var suggestion = await db.AutomationSuggestions.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (suggestion is null)
                return Results.NotFound(new { error = "Förslag hittades inte" });

            try
            {
                suggestion.Acceptera();
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { suggestion.Id, status = suggestion.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("AcceptAutomationSuggestion");

        // ============================================================
        // Avvisa förslag
        // ============================================================

        automation.MapPost("/forslag/{id:guid}/avvisa", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var suggestion = await db.AutomationSuggestions.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (suggestion is null)
                return Results.NotFound(new { error = "Förslag hittades inte" });

            try
            {
                suggestion.Avvisa();
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { suggestion.Id, status = suggestion.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("DismissAutomationSuggestion");

        return app;
    }
}

// Request DTO
record ChangeAutomationLevelRequest(string Niva);
