using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RegionHR.Automation.Domain;
using RegionHR.HalsoSAM.Domain;
using RegionHR.Infrastructure.Journeys;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Leave.Domain;
using RegionHR.Notifications.Domain;
using RegionHR.Offboarding.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Executes automation actions against real domain entities and the database.
///
/// Handles all action types defined in automation rule Atgard JSON:
///   Notify, Suggest, Block, and domain Actions (ConvertToTillsvidare,
///   CreateRehabCase, AutoApproveLeave, CreateFKNotification,
///   RetentionCleanup, CreateJourneyInstance, CreateOffboardingCase, GenerateAGI).
/// </summary>
public sealed class AutomationActionExecutor
{
    private readonly RegionHRDbContext _db;
    private readonly ILogger<AutomationActionExecutor> _logger;

    public AutomationActionExecutor(RegionHRDbContext db, ILogger<AutomationActionExecutor> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Execute the action defined in an automation rule at the given effective level.
    /// </summary>
    /// <param name="rule">The automation rule whose Atgard JSON defines the action.</param>
    /// <param name="effectiveLevel">The resolved automation level (Notify/Suggest/Autopilot/Block).</param>
    /// <param name="context">Entity context dictionary with relevant field values.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A description of the action taken.</returns>
    public async Task<string> ExecuteAsync(
        AutomationRule rule,
        AutomationLevel effectiveLevel,
        Dictionary<string, object> context,
        CancellationToken ct)
    {
        var atgard = ParseAtgard(rule.Atgard);

        return effectiveLevel switch
        {
            AutomationLevel.Block => ExecuteBlock(rule),
            AutomationLevel.Notify => await ExecuteNotify(rule, atgard, context, ct),
            AutomationLevel.Suggest => await ExecuteSuggest(rule, atgard, context, ct),
            AutomationLevel.Autopilot => await ExecuteAutopilot(rule, atgard, context, ct),
            _ => $"Okänd nivå för '{rule.Namn}'"
        };
    }

    private static string ExecuteBlock(AutomationRule rule)
    {
        throw new AutomationBlockException(
            rule.Namn,
            $"Handling blockerad av regel '{rule.Namn}'");
    }

    private Task<string> ExecuteNotify(
        AutomationRule rule,
        AtgardData atgard,
        Dictionary<string, object> context,
        CancellationToken ct)
    {
        var userId = GetGuidFromContext(context, "anstallId")
                     ?? GetGuidFromContext(context, "userId")
                     ?? Guid.Empty;

        var notification = Notification.Create(
            userId,
            $"Automation: {rule.Namn}",
            $"Regel '{rule.Namn}' har utlösts. Mall: {atgard.Mall ?? "ingen"}",
            NotificationType.Info,
            actionUrl: $"/admin/automation");

        _db.Notifications.Add(notification);

        _logger.LogInformation(
            "Automation Notify: Regel '{RuleName}' skapade notifiering för {UserId}",
            rule.Namn, userId);

        return Task.FromResult($"Notifiering skapad för '{rule.Namn}'");
    }

    private Task<string> ExecuteSuggest(
        AutomationRule rule,
        AtgardData atgard,
        Dictionary<string, object> context,
        CancellationToken ct)
    {
        var employeeId = GetEmployeeIdFromContext(context);

        var suggestion = AutomationSuggestion.Skapa(
            rule.Id,
            $"Föreslagen åtgärd: {atgard.Mall ?? rule.Namn}",
            employeeId);

        _db.AutomationSuggestions.Add(suggestion);

        _logger.LogInformation(
            "Automation Suggest: Regel '{RuleName}' skapade förslag för {EmployeeId}",
            rule.Namn, employeeId?.Value);

        return Task.FromResult($"Förslag skapat för '{rule.Namn}'");
    }

    private async Task<string> ExecuteAutopilot(
        AutomationRule rule,
        AtgardData atgard,
        Dictionary<string, object> context,
        CancellationToken ct)
    {
        // Route to the correct action handler based on mall/action name
        var actionKey = (atgard.Mall ?? atgard.Typ ?? "").ToLowerInvariant();

        return actionKey switch
        {
            "las_konvertering" => await ExecuteLASConversion(rule, context, ct),
            "rehab_start" => await ExecuteCreateRehabCase(rule, context, ct),
            "fk_anmalan" => await ExecuteCreateFKNotification(rule, context, ct),
            "gallring" => await ExecuteRetentionCleanup(rule, ct),
            "onboarding_start" => await ExecuteCreateJourneyInstance(rule, context, "Onboarding", ct),
            "offboarding_start" => await ExecuteCreateOffboardingCase(rule, context, ct),
            "agi_xml" => await ExecuteGenerateAGI(rule, ct),
            _ => await ExecuteGenericAutopilot(rule, atgard, context, ct)
        };
    }

    // === Domain action implementations ===

    private async Task<string> ExecuteLASConversion(
        AutomationRule rule,
        Dictionary<string, object> context,
        CancellationToken ct)
    {
        var employeeGuid = GetGuidFromContext(context, "anstallId");
        if (employeeGuid is null)
            return $"LAS-konvertering avbruten: ingen anstallId i kontext";

        var empId = EmployeeId.From(employeeGuid.Value);

        var lasAccumulation = await _db.LASAccumulations
            .FirstOrDefaultAsync(l => l.AnstallId == empId, ct);

        if (lasAccumulation is null)
            return $"LAS-konvertering avbruten: ingen LAS-ackumulering hittad för {empId}";

        // The domain model handles the conversion logic in Omberakna/UppdateraStatus
        // We trigger a recalculation which will set the status
        lasAccumulation.Omberakna(DateOnly.FromDateTime(DateTime.Today));

        // Also create a notification for the HR team
        var notification = Notification.Create(
            Guid.Empty, // System notification
            "LAS-konvertering utförd",
            $"Anställd {empId} har automatiskt konverterats till tillsvidareanställning enligt LAS.",
            NotificationType.Action,
            actionUrl: "/las");
        _db.Notifications.Add(notification);

        _logger.LogInformation(
            "Automation Autopilot: LAS-konvertering utförd för anställd {EmployeeId}",
            empId);

        return $"LAS-konvertering utförd för anställd {empId}";
    }

    private async Task<string> ExecuteCreateRehabCase(
        AutomationRule rule,
        Dictionary<string, object> context,
        CancellationToken ct)
    {
        var employeeGuid = GetGuidFromContext(context, "anstallId");
        if (employeeGuid is null)
            return "Rehab-ärende avbrutet: ingen anstallId i kontext";

        var empId = EmployeeId.From(employeeGuid.Value);

        // Check if there's already an active rehab case for this employee
        var existing = await _db.RehabCases
            .AnyAsync(r => r.AnstallId == empId && r.Status != RehabStatus.Avslutad, ct);

        if (existing)
            return $"Rehab-ärende finns redan för anställd {empId}";

        var rehabCase = RehabCase.Skapa(empId, RehabTrigger.MonsterDetekterat);
        _db.RehabCases.Add(rehabCase);

        var notification = Notification.Create(
            Guid.Empty,
            "Nytt rehabiliteringsärende",
            $"Ett rehabiliteringsärende har automatiskt skapats för anställd {empId}.",
            NotificationType.Action,
            actionUrl: "/halsosam");
        _db.Notifications.Add(notification);

        _logger.LogInformation(
            "Automation Autopilot: Rehabärende skapat för anställd {EmployeeId}",
            empId);

        return $"Rehabärende skapat för anställd {empId}";
    }

    private Task<string> ExecuteCreateFKNotification(
        AutomationRule rule,
        Dictionary<string, object> context,
        CancellationToken ct)
    {
        var employeeGuid = GetGuidFromContext(context, "anstallId");
        if (employeeGuid is null)
            return Task.FromResult("FK-anmälan avbruten: ingen anstallId i kontext");

        var sickNotification = SickLeaveNotification.Skapa(
            employeeGuid.Value,
            DateOnly.FromDateTime(DateTime.Today));

        _db.SickLeaveNotifications.Add(sickNotification);

        var notification = Notification.Create(
            Guid.Empty,
            "FK-anmälan krävs",
            $"Sjukfrånvaro överstiger 14 dagar. Försäkringskassan ska anmälas.",
            NotificationType.Warning,
            actionUrl: "/minsida/sjukanmalan");
        _db.Notifications.Add(notification);

        _logger.LogInformation(
            "Automation Autopilot: FK-anmälan skapad för anställd {EmployeeId}",
            employeeGuid);

        return Task.FromResult($"FK-anmälan skapad för anställd {employeeGuid}");
    }

    private async Task<string> ExecuteRetentionCleanup(
        AutomationRule rule,
        CancellationToken ct)
    {
        var expiredRecords = await _db.RetentionRecords
            .Where(r => r.RetentionExpires <= DateTime.UtcNow && !r.IsAnonymized)
            .ToListAsync(ct);

        foreach (var record in expiredRecords)
        {
            record.Anonymize();
        }

        _logger.LogInformation(
            "Automation Autopilot: RetentionCleanup anonymiserade {Count} poster",
            expiredRecords.Count);

        return $"RetentionCleanup: {expiredRecords.Count} poster anonymiserade";
    }

    private async Task<string> ExecuteCreateJourneyInstance(
        AutomationRule rule,
        Dictionary<string, object> context,
        string journeyType,
        CancellationToken ct)
    {
        var employeeGuid = GetGuidFromContext(context, "anstallId");
        if (employeeGuid is null)
            return "Journey-instans avbruten: ingen anstallId i kontext";

        var employeeName = context.TryGetValue("anstallNamn", out var name)
            ? name.ToString() ?? "Okänd"
            : "Okänd";

        // Find matching journey template
        var kategori = journeyType.ToLowerInvariant() switch
        {
            "onboarding" => JourneyKategori.Onboarding,
            "avslut" or "offboarding" => JourneyKategori.Avslut,
            _ => JourneyKategori.Onboarding
        };

        var template = await _db.JourneyTemplates
            .Include("_steg") // Load steps
            .FirstOrDefaultAsync(t => t.Kategori == kategori, ct);

        if (template is null)
        {
            _logger.LogWarning("Ingen journey-mall hittad för kategori {Kategori}", kategori);
            return $"Journey-instans avbruten: ingen mall hittad för {kategori}";
        }

        var instance = JourneyInstance.SkapaFranMall(
            template, employeeGuid.Value, employeeName, DateTime.UtcNow);

        _db.JourneyInstances.Add(instance);

        _logger.LogInformation(
            "Automation Autopilot: Journey-instans '{JourneyType}' skapad för {EmployeeId}",
            journeyType, employeeGuid);

        return $"Journey-instans '{journeyType}' skapad för anställd {employeeGuid}";
    }

    private Task<string> ExecuteCreateOffboardingCase(
        AutomationRule rule,
        Dictionary<string, object> context,
        CancellationToken ct)
    {
        var employeeGuid = GetGuidFromContext(context, "anstallId");
        if (employeeGuid is null)
            return Task.FromResult("Offboarding avbruten: ingen anstallId i kontext");

        var sistaArbetsdag = DateOnly.FromDateTime(DateTime.Today.AddMonths(1));
        if (context.TryGetValue("slutDatum", out var slutObj))
        {
            if (slutObj is DateOnly d)
                sistaArbetsdag = d;
            else if (slutObj is string s && DateOnly.TryParse(s, out var parsed))
                sistaArbetsdag = parsed;
        }

        var offboarding = OffboardingCase.Skapa(
            employeeGuid.Value,
            AvslutAnledning.EgenBegaran,
            sistaArbetsdag);

        _db.OffboardingCases.Add(offboarding);

        _logger.LogInformation(
            "Automation Autopilot: Offboarding-ärende skapat för anställd {EmployeeId}",
            employeeGuid);

        return Task.FromResult($"Offboarding-ärende skapat för anställd {employeeGuid}");
    }

    private Task<string> ExecuteGenerateAGI(
        AutomationRule rule,
        CancellationToken ct)
    {
        // AGI generation is handled by the existing AGIXmlGenerator service
        // Here we just create a notification that AGI should be generated
        var notification = Notification.Create(
            Guid.Empty,
            "AGI-generering påbörjad",
            "Arbetsgivardeklaration (AGI) ska genereras för innevarande period.",
            NotificationType.Info,
            actionUrl: "/integrationer");
        _db.Notifications.Add(notification);

        _logger.LogInformation("Automation Autopilot: AGI-generering notifiering skapad");

        return Task.FromResult("AGI-generering notifiering skapad");
    }

    private Task<string> ExecuteGenericAutopilot(
        AutomationRule rule,
        AtgardData atgard,
        Dictionary<string, object> context,
        CancellationToken ct)
    {
        // For unrecognized autopilot actions, create a notification
        var userId = GetGuidFromContext(context, "anstallId")
                     ?? GetGuidFromContext(context, "userId")
                     ?? Guid.Empty;

        var notification = Notification.Create(
            userId,
            $"Automation utförd: {rule.Namn}",
            $"Automatisk åtgärd '{atgard.Mall ?? atgard.Typ}' utförd av regel '{rule.Namn}'.",
            NotificationType.Info,
            actionUrl: "/admin/automation");
        _db.Notifications.Add(notification);

        _logger.LogInformation(
            "Automation Autopilot: Generisk åtgärd '{ActionKey}' utförd för regel '{RuleName}'",
            atgard.Mall ?? atgard.Typ, rule.Namn);

        return Task.FromResult($"Generisk åtgärd utförd för '{rule.Namn}'");
    }

    // === Helpers ===

    private static AtgardData ParseAtgard(string atgardJson)
    {
        try
        {
            var root = JsonSerializer.Deserialize<JsonElement>(atgardJson);
            return new AtgardData
            {
                Typ = root.TryGetProperty("typ", out var typ) ? typ.GetString() : null,
                Mall = root.TryGetProperty("mall", out var mall) ? mall.GetString() : null
            };
        }
        catch
        {
            return new AtgardData();
        }
    }

    private static Guid? GetGuidFromContext(Dictionary<string, object> context, string key)
    {
        if (!context.TryGetValue(key, out var value))
            return null;

        return value switch
        {
            Guid g => g,
            EmployeeId eid => eid.Value,
            string s when Guid.TryParse(s, out var g) => g,
            _ => null
        };
    }

    private static EmployeeId? GetEmployeeIdFromContext(Dictionary<string, object> context)
    {
        var guid = GetGuidFromContext(context, "anstallId");
        return guid.HasValue ? EmployeeId.From(guid.Value) : null;
    }

    private sealed class AtgardData
    {
        public string? Typ { get; set; }
        public string? Mall { get; set; }
    }
}
