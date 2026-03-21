using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RegionHR.Automation.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.Infrastructure.Services;

public class AutomationEngineService : IAutomationEngine
{
    private readonly RegionHRDbContext _db;
    private readonly ConditionEvaluator _conditionEvaluator;
    private readonly AutomationActionExecutor _actionExecutor;
    private readonly ILogger<AutomationEngineService> _logger;

    public AutomationEngineService(
        RegionHRDbContext db,
        ConditionEvaluator conditionEvaluator,
        AutomationActionExecutor actionExecutor,
        ILogger<AutomationEngineService> logger)
    {
        _db = db;
        _conditionEvaluator = conditionEvaluator;
        _actionExecutor = actionExecutor;
        _logger = logger;
    }

    public async Task EvaluateAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var eventType = domainEvent.GetType().Name;

        // Hämta aktiva regler som matchar trigger-typ
        var matchingRules = await _db.AutomationRules
            .Where(r => r.ArAktiv && r.TriggerTyp == eventType)
            .ToListAsync(ct);

        if (matchingRules.Count == 0)
            return;

        // Build context dictionary from the domain event
        var context = BuildContextFromEvent(domainEvent);

        foreach (var rule in matchingRules)
        {
            try
            {
                await EvaluateAndExecuteRule(rule, eventType, context, ct);
            }
            catch (AutomationBlockException)
            {
                throw; // Re-throw block exceptions to prevent the triggering action
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Fel vid evaluering av regel '{RuleName}' för händelse '{EventType}'",
                    rule.Namn, eventType);

                // Log the failed execution
                var failedExecution = AutomationExecution.Skapa(
                    rule.Id,
                    eventType,
                    $"FEL: {ex.Message}",
                    AutomationLevel.Notify,
                    $"Regel '{rule.Namn}' misslyckades");
                _db.AutomationExecutions.Add(failedExecution);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task EvaluateCronAsync(string cronCategory, CancellationToken ct = default)
    {
        // Hämta aktiva regler som matchar cron-kategori
        var matchingRules = await _db.AutomationRules
            .Where(r => r.ArAktiv && r.TriggerTyp == cronCategory)
            .ToListAsync(ct);

        if (matchingRules.Count == 0)
            return;

        // Cron jobs don't have a specific event context, use empty context
        var context = new Dictionary<string, object>();

        foreach (var rule in matchingRules)
        {
            try
            {
                await EvaluateAndExecuteRule(rule, cronCategory, context, ct);
            }
            catch (AutomationBlockException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Fel vid evaluering av cron-regel '{RuleName}' för '{CronCategory}'",
                    rule.Namn, cronCategory);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private async Task EvaluateAndExecuteRule(
        AutomationRule rule,
        string eventType,
        Dictionary<string, object> context,
        CancellationToken ct)
    {
        // 1. Evaluate JSON condition against the context
        var conditionMet = _conditionEvaluator.Evaluate(rule.Villkor, context);

        if (!conditionMet)
        {
            _logger.LogDebug(
                "Regel '{RuleName}' villkor ej uppfyllt för händelse '{EventType}'",
                rule.Namn, eventType);
            return;
        }

        // 2. Get the effective automation level for this category
        var effectiveLevel = await GetEffectiveLevel(rule.KategoriId, ct);

        // 3. Ensure effective level meets the rule's minimum
        if (effectiveLevel < rule.MinimumNiva)
        {
            effectiveLevel = rule.MinimumNiva;
        }

        // 4. Execute the action at the effective level
        string actionDescription;
        try
        {
            actionDescription = await _actionExecutor.ExecuteAsync(rule, effectiveLevel, context, ct);
        }
        catch (AutomationBlockException)
        {
            // Log the block execution before re-throwing
            var blockExecution = AutomationExecution.Skapa(
                rule.Id,
                eventType,
                $"Regel '{rule.Namn}' BLOCKERADE handling",
                AutomationLevel.Block,
                $"Handling blockerad av '{rule.Namn}'");
            _db.AutomationExecutions.Add(blockExecution);
            await _db.SaveChangesAsync(ct);
            throw;
        }

        // 5. Log the execution
        var execution = AutomationExecution.Skapa(
            rule.Id,
            eventType,
            $"Regel '{rule.Namn}' utvärderad — villkor uppfyllt",
            effectiveLevel,
            actionDescription);

        _db.AutomationExecutions.Add(execution);

        _logger.LogInformation(
            "Automation: Regel '{RuleName}' utförd på nivå {Level}: {Action}",
            rule.Namn, effectiveLevel, actionDescription);
    }

    private async Task<AutomationLevel> GetEffectiveLevel(
        SharedKernel.Domain.AutomationCategoryId kategoriId, CancellationToken ct)
    {
        var config = await _db.AutomationLevelConfigs
            .FirstOrDefaultAsync(c => c.KategoriId == kategoriId, ct);

        return config?.ValdNiva ?? AutomationLevel.Notify;
    }

    /// <summary>
    /// Build a context dictionary from a domain event by reflecting its public properties.
    /// The context dictionary uses camelCase keys matching the JSON condition field names.
    /// </summary>
    internal static Dictionary<string, object> BuildContextFromEvent(IDomainEvent domainEvent)
    {
        var context = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        var eventType = domainEvent.GetType();

        foreach (var prop in eventType.GetProperties())
        {
            // Skip the base IDomainEvent properties
            if (prop.Name is "EventId" or "OccurredAt")
                continue;

            var value = prop.GetValue(domainEvent);
            if (value is null)
                continue;

            // Use the property name directly (case-insensitive dictionary)
            context[prop.Name] = value;

            // Also add a camelCase alias for common fields
            var camelCase = char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..];
            if (camelCase != prop.Name)
                context[camelCase] = value;
        }

        return context;
    }
}
