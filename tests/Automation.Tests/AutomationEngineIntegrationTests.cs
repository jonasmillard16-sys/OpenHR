using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegionHR.Automation.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Automation.Tests;

public class AutomationEngineIntegrationTests : IDisposable
{
    private readonly RegionHRDbContext _db;
    private readonly AutomationEngineService _engine;
    private readonly AutomationCategoryId _complianceCategoryId;
    private readonly AutomationCategoryId _franvaroCategoryId;

    public AutomationEngineIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<RegionHRDbContext>()
            .UseInMemoryDatabase($"AutomationEngine-{Guid.NewGuid()}")
            .Options;

        _db = new RegionHRDbContext(options);

        var conditionEvaluator = new ConditionEvaluator();
        var actionLogger = NullLogger<AutomationActionExecutor>.Instance;
        var actionExecutor = new AutomationActionExecutor(_db, actionLogger);
        var engineLogger = NullLogger<AutomationEngineService>.Instance;

        _engine = new AutomationEngineService(_db, conditionEvaluator, actionExecutor, engineLogger);

        // Seed categories and configs
        _complianceCategoryId = AutomationCategoryId.New();
        _franvaroCategoryId = AutomationCategoryId.New();

        var complianceCategory = AutomationCategory.Skapa("Compliance", "Lagefterlevnad", "Gavel");
        var franvaroCategory = AutomationCategory.Skapa("Franvaro", "Sjukfranvaro", "EventBusy");

        _db.AutomationCategories.AddRange(complianceCategory, franvaroCategory);

        // Set compliance to Notify, franvaro to Autopilot
        _db.AutomationLevelConfigs.Add(
            AutomationLevelConfig.Skapa(_complianceCategoryId, AutomationLevel.Notify));
        _db.AutomationLevelConfigs.Add(
            AutomationLevelConfig.Skapa(_franvaroCategoryId, AutomationLevel.Autopilot));

        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    // === Event triggers matching rules ===

    [Fact]
    public async Task EvaluateAsync_MatchingRule_WithConditionMet_CreatesExecution()
    {
        // Arrange: Create a rule — TriggerTyp must match the event class name exactly
        var rule = AutomationRule.Skapa(
            "LAS-varning 300",
            _complianceCategoryId,
            nameof(LASAccumulationUpdated),
            "{\"AckumuleradeDagar\":{\">=\":300}}",
            "{\"typ\":\"notify\",\"mall\":\"las_varning\"}",
            AutomationLevel.Notify,
            true);
        _db.AutomationRules.Add(rule);
        await _db.SaveChangesAsync();

        var domainEvent = new LASAccumulationUpdated(
            EmployeeId.New(), EmploymentType.SAVA, 350);

        // Act
        await _engine.EvaluateAsync(domainEvent);

        // Assert: execution logged
        Assert.Single(_db.AutomationExecutions.Local);
        var execution = _db.AutomationExecutions.Local.First();
        Assert.Equal(nameof(LASAccumulationUpdated), execution.HandelseTyp);
        Assert.Contains("utvärderad", execution.Resultat);
    }

    [Fact]
    public async Task EvaluateAsync_MatchingRule_WithConditionNotMet_NoExecution()
    {
        var rule = AutomationRule.Skapa(
            "LAS-varning 300",
            _complianceCategoryId,
            nameof(LASAccumulationUpdated),
            "{\"AckumuleradeDagar\":{\">=\":300}}",
            "{\"typ\":\"notify\",\"mall\":\"las_varning\"}",
            AutomationLevel.Notify,
            true);
        _db.AutomationRules.Add(rule);
        await _db.SaveChangesAsync();

        // Only 100 days — condition not met
        var domainEvent = new LASAccumulationUpdated(
            EmployeeId.New(), EmploymentType.SAVA, 100);

        await _engine.EvaluateAsync(domainEvent);

        Assert.Empty(_db.AutomationExecutions.Local);
    }

    [Fact]
    public async Task EvaluateAsync_NoMatchingRules_DoesNothing()
    {
        // No rules seeded for this event type
        var domainEvent = new UnrelatedEvent();

        await _engine.EvaluateAsync(domainEvent);

        Assert.Empty(_db.AutomationExecutions.Local);
    }

    [Fact]
    public async Task EvaluateAsync_InactiveRule_IsSkipped()
    {
        var rule = AutomationRule.Skapa(
            "Inaktiv regel",
            _complianceCategoryId,
            nameof(LASAccumulationUpdated),
            "{}",
            "{\"typ\":\"notify\",\"mall\":\"test\"}",
            AutomationLevel.Notify);
        rule.Inaktivera(); // Make it inactive
        _db.AutomationRules.Add(rule);
        await _db.SaveChangesAsync();

        var domainEvent = new LASAccumulationUpdated(
            EmployeeId.New(), EmploymentType.SAVA, 350);

        await _engine.EvaluateAsync(domainEvent);

        Assert.Empty(_db.AutomationExecutions.Local);
    }

    [Fact]
    public async Task EvaluateAsync_EmptyCondition_AlwaysMatches()
    {
        var rule = AutomationRule.Skapa(
            "Alltid-trigger",
            _complianceCategoryId,
            nameof(LASAccumulationUpdated),
            "{}",
            "{\"typ\":\"notify\",\"mall\":\"test\"}",
            AutomationLevel.Notify);
        _db.AutomationRules.Add(rule);
        await _db.SaveChangesAsync();

        var domainEvent = new LASAccumulationUpdated(
            EmployeeId.New(), EmploymentType.SAVA, 50);

        await _engine.EvaluateAsync(domainEvent);

        Assert.Single(_db.AutomationExecutions.Local);
    }

    [Fact]
    public async Task EvaluateAsync_MultipleRules_EvaluatesAll()
    {
        var rule1 = AutomationRule.Skapa(
            "Regel 1",
            _complianceCategoryId,
            nameof(LASAccumulationUpdated),
            "{}",
            "{\"typ\":\"notify\",\"mall\":\"test1\"}",
            AutomationLevel.Notify);
        var rule2 = AutomationRule.Skapa(
            "Regel 2",
            _complianceCategoryId,
            nameof(LASAccumulationUpdated),
            "{}",
            "{\"typ\":\"notify\",\"mall\":\"test2\"}",
            AutomationLevel.Notify);
        _db.AutomationRules.AddRange(rule1, rule2);
        await _db.SaveChangesAsync();

        var domainEvent = new LASAccumulationUpdated(
            EmployeeId.New(), EmploymentType.SAVA, 350);

        await _engine.EvaluateAsync(domainEvent);

        Assert.Equal(2, _db.AutomationExecutions.Local.Count);
    }

    // === Block level ===

    [Fact]
    public async Task EvaluateAsync_BlockLevel_ThrowsAutomationBlockException()
    {
        // Category set to Block
        var blockCategoryId = AutomationCategoryId.New();
        _db.AutomationLevelConfigs.Add(
            AutomationLevelConfig.Skapa(blockCategoryId, AutomationLevel.Block));

        var rule = AutomationRule.Skapa(
            "ATL-block",
            blockCategoryId,
            nameof(ShiftCreated),
            "{}",
            "{\"typ\":\"block\"}",
            AutomationLevel.Block,
            true);
        _db.AutomationRules.Add(rule);
        await _db.SaveChangesAsync();

        var domainEvent = new ShiftCreated();

        await Assert.ThrowsAsync<AutomationBlockException>(
            () => _engine.EvaluateAsync(domainEvent));
    }

    // === Cron evaluation ===

    [Fact]
    public async Task EvaluateCronAsync_MatchingCronRules_CreatesExecutions()
    {
        var rule = AutomationRule.Skapa(
            "Daglig kontroll",
            _complianceCategoryId,
            "CronDaily",
            "{}",
            "{\"typ\":\"notify\",\"mall\":\"daglig_kontroll\"}",
            AutomationLevel.Notify);
        _db.AutomationRules.Add(rule);
        await _db.SaveChangesAsync();

        await _engine.EvaluateCronAsync("CronDaily");

        Assert.Single(_db.AutomationExecutions.Local);
    }

    // === BuildContextFromEvent ===

    [Fact]
    public void BuildContextFromEvent_ExtractsProperties()
    {
        var empId = EmployeeId.New();
        var domainEvent = new LASAccumulationUpdated(empId, EmploymentType.SAVA, 350);

        var context = AutomationEngineService.BuildContextFromEvent(domainEvent);

        Assert.Equal(empId, context["AnstallId"]);
        Assert.Equal(EmploymentType.SAVA, context["Anstallningsform"]);
        Assert.Equal(350, context["AckumuleradeDagar"]);
    }

    [Fact]
    public void BuildContextFromEvent_ExcludesBaseProperties()
    {
        var domainEvent = new LASAccumulationUpdated(EmployeeId.New(), EmploymentType.SAVA, 100);

        var context = AutomationEngineService.BuildContextFromEvent(domainEvent);

        Assert.False(context.ContainsKey("EventId"));
        Assert.False(context.ContainsKey("OccurredAt"));
    }

    [Fact]
    public void BuildContextFromEvent_AddsCamelCaseAliases()
    {
        var empId = EmployeeId.New();
        var domainEvent = new LASAccumulationUpdated(empId, EmploymentType.SAVA, 350);

        var context = AutomationEngineService.BuildContextFromEvent(domainEvent);

        // Both PascalCase and camelCase should exist
        Assert.True(context.ContainsKey("AnstallId"));
        Assert.True(context.ContainsKey("anstallId"));
        Assert.Equal(empId, context["anstallId"]);
    }
}

// Test event types — names must match the TriggerTyp used in rules
// These are at namespace level so GetType().Name returns the exact name

/// <summary>Test event matching trigger "LASAccumulationUpdated"</summary>
internal sealed record LASAccumulationUpdated(
    EmployeeId AnstallId,
    EmploymentType Anstallningsform,
    int AckumuleradeDagar) : DomainEvent;

/// <summary>Test event matching trigger "ShiftCreated"</summary>
internal sealed record ShiftCreated() : DomainEvent;

/// <summary>Test event that matches no rules</summary>
internal sealed record UnrelatedEvent() : DomainEvent;
