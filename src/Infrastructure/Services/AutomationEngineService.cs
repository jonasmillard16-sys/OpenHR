using Microsoft.EntityFrameworkCore;
using RegionHR.Automation.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.Infrastructure.Services;

public class AutomationEngineService : IAutomationEngine
{
    private readonly RegionHRDbContext _db;

    public AutomationEngineService(RegionHRDbContext db)
    {
        _db = db;
    }

    public async Task EvaluateAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var eventType = domainEvent.GetType().Name;

        // Hämta aktiva regler som matchar trigger-typ
        var matchingRules = await _db.AutomationRules
            .Where(r => r.ArAktiv && r.TriggerTyp == eventType)
            .ToListAsync(ct);

        foreach (var rule in matchingRules)
        {
            var effectiveLevel = await GetEffectiveLevel(rule.KategoriId, ct);

            // Skapa exekveringslogg
            var execution = AutomationExecution.Skapa(
                rule.Id,
                eventType,
                $"Regel '{rule.Namn}' utvärderad",
                effectiveLevel,
                GetActionDescription(effectiveLevel, rule.Namn));

            _db.AutomationExecutions.Add(execution);

            // Om Suggest-nivå: skapa förslag
            if (effectiveLevel == AutomationLevel.Suggest)
            {
                var suggestion = AutomationSuggestion.Skapa(
                    rule.Id,
                    $"Föreslagen åtgärd: {rule.Namn}");

                _db.AutomationSuggestions.Add(suggestion);
            }
        }

        if (matchingRules.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task EvaluateCronAsync(string cronCategory, CancellationToken ct = default)
    {
        // Hämta aktiva regler som matchar cron-kategori
        var matchingRules = await _db.AutomationRules
            .Where(r => r.ArAktiv && r.TriggerTyp == cronCategory)
            .ToListAsync(ct);

        foreach (var rule in matchingRules)
        {
            var effectiveLevel = await GetEffectiveLevel(rule.KategoriId, ct);

            var execution = AutomationExecution.Skapa(
                rule.Id,
                cronCategory,
                $"Cron-regel '{rule.Namn}' utvärderad",
                effectiveLevel,
                GetActionDescription(effectiveLevel, rule.Namn));

            _db.AutomationExecutions.Add(execution);

            if (effectiveLevel == AutomationLevel.Suggest)
            {
                var suggestion = AutomationSuggestion.Skapa(
                    rule.Id,
                    $"Schemalagd åtgärd: {rule.Namn}");

                _db.AutomationSuggestions.Add(suggestion);
            }
        }

        if (matchingRules.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
        }
    }

    private async Task<AutomationLevel> GetEffectiveLevel(
        SharedKernel.Domain.AutomationCategoryId kategoriId, CancellationToken ct)
    {
        var config = await _db.AutomationLevelConfigs
            .FirstOrDefaultAsync(c => c.KategoriId == kategoriId, ct);

        return config?.ValdNiva ?? AutomationLevel.Notify;
    }

    private static string GetActionDescription(AutomationLevel level, string regelNamn)
    {
        return level switch
        {
            AutomationLevel.Notify => $"Notifiering skickad för '{regelNamn}'",
            AutomationLevel.Suggest => $"Förslag skapat för '{regelNamn}'",
            AutomationLevel.Autopilot => $"Åtgärd utförd automatiskt för '{regelNamn}'",
            AutomationLevel.Block => $"Handling blockerad av '{regelNamn}'",
            _ => $"Okänd nivå för '{regelNamn}'"
        };
    }
}
