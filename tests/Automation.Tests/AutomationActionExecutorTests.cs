using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RegionHR.Automation.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Automation.Tests;

public class AutomationActionExecutorTests : IDisposable
{
    private readonly RegionHRDbContext _db;
    private readonly AutomationActionExecutor _executor;
    private readonly AutomationCategoryId _categoryId = AutomationCategoryId.New();

    public AutomationActionExecutorTests()
    {
        var options = new DbContextOptionsBuilder<RegionHRDbContext>()
            .UseInMemoryDatabase($"ActionExecutor-{Guid.NewGuid()}")
            .Options;

        _db = new RegionHRDbContext(options);

        var logger = NullLogger<AutomationActionExecutor>.Instance;
        _executor = new AutomationActionExecutor(_db, logger);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    // === Notify tests ===

    [Fact]
    public async Task ExecuteAsync_NotifyLevel_CreatesNotification()
    {
        var rule = CreateRule("Test notify-regel", "{\"typ\":\"notify\",\"mall\":\"test_mall\"}");
        var context = new Dictionary<string, object>
        {
            { "anstallId", Guid.NewGuid() }
        };

        var result = await _executor.ExecuteAsync(rule, AutomationLevel.Notify, context, CancellationToken.None);

        Assert.Contains("Notifiering skapad", result);
        Assert.Single(_db.Notifications.Local);
    }

    // === Suggest tests ===

    [Fact]
    public async Task ExecuteAsync_SuggestLevel_CreatesSuggestion()
    {
        var rule = CreateRule("Test suggest-regel", "{\"typ\":\"suggest\",\"mall\":\"test_forslag\"}");
        var context = new Dictionary<string, object>
        {
            { "anstallId", Guid.NewGuid() }
        };

        var result = await _executor.ExecuteAsync(rule, AutomationLevel.Suggest, context, CancellationToken.None);

        Assert.Contains("Förslag skapat", result);
        Assert.Single(_db.AutomationSuggestions.Local);
    }

    [Fact]
    public async Task ExecuteAsync_SuggestLevel_WithEmployeeId_SetsEmployeeOnSuggestion()
    {
        var empId = Guid.NewGuid();
        var rule = CreateRule("Test suggest", "{\"typ\":\"suggest\",\"mall\":\"test\"}");
        var context = new Dictionary<string, object>
        {
            { "anstallId", empId }
        };

        await _executor.ExecuteAsync(rule, AutomationLevel.Suggest, context, CancellationToken.None);

        var suggestion = _db.AutomationSuggestions.Local.Single();
        Assert.Equal(EmployeeId.From(empId), suggestion.SkapadFor);
    }

    // === Block tests ===

    [Fact]
    public async Task ExecuteAsync_BlockLevel_ThrowsAutomationBlockException()
    {
        var rule = CreateRule("ATL-block", "{\"typ\":\"block\"}");
        var context = new Dictionary<string, object>();

        await Assert.ThrowsAsync<AutomationBlockException>(() =>
            _executor.ExecuteAsync(rule, AutomationLevel.Block, context, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_BlockLevel_ExceptionContainsRuleName()
    {
        var rule = CreateRule("ATL max veckoarbetstid", "{\"typ\":\"block\"}");
        var context = new Dictionary<string, object>();

        var ex = await Assert.ThrowsAsync<AutomationBlockException>(() =>
            _executor.ExecuteAsync(rule, AutomationLevel.Block, context, CancellationToken.None));

        Assert.Equal("ATL max veckoarbetstid", ex.RuleName);
    }

    // === Autopilot: CreateRehabCase ===

    [Fact]
    public async Task ExecuteAsync_Autopilot_RehabStart_CreatesRehabCase()
    {
        var empGuid = Guid.NewGuid();
        var rule = CreateRule("Rehab-trigger", "{\"typ\":\"autopilot\",\"mall\":\"rehab_start\"}");
        var context = new Dictionary<string, object>
        {
            { "anstallId", empGuid }
        };

        var result = await _executor.ExecuteAsync(rule, AutomationLevel.Autopilot, context, CancellationToken.None);
        await _db.SaveChangesAsync();

        Assert.Contains("Rehabärende skapat", result);
        Assert.Single(_db.RehabCases.Local);
    }

    [Fact]
    public async Task ExecuteAsync_Autopilot_RehabStart_SkipsIfExistingActiveCase()
    {
        var empGuid = Guid.NewGuid();
        var empId = EmployeeId.From(empGuid);

        // Create an existing active rehab case
        var existingCase = RegionHR.HalsoSAM.Domain.RehabCase.Skapa(empId, RegionHR.HalsoSAM.Domain.RehabTrigger.ChefInitierat);
        _db.RehabCases.Add(existingCase);
        await _db.SaveChangesAsync();

        var rule = CreateRule("Rehab-trigger", "{\"typ\":\"autopilot\",\"mall\":\"rehab_start\"}");
        var context = new Dictionary<string, object>
        {
            { "anstallId", empGuid }
        };

        var result = await _executor.ExecuteAsync(rule, AutomationLevel.Autopilot, context, CancellationToken.None);

        Assert.Contains("finns redan", result);
    }

    // === Autopilot: CreateFKNotification ===

    [Fact]
    public async Task ExecuteAsync_Autopilot_FKAnmalan_CreatesSickLeaveNotification()
    {
        var empGuid = Guid.NewGuid();
        var rule = CreateRule("FK-anmälan", "{\"typ\":\"autopilot\",\"mall\":\"fk_anmalan\"}");
        var context = new Dictionary<string, object>
        {
            { "anstallId", empGuid }
        };

        var result = await _executor.ExecuteAsync(rule, AutomationLevel.Autopilot, context, CancellationToken.None);
        await _db.SaveChangesAsync();

        Assert.Contains("FK-anmälan skapad", result);
        Assert.Single(_db.SickLeaveNotifications.Local);
    }

    // === Autopilot: RetentionCleanup ===

    [Fact]
    public async Task ExecuteAsync_Autopilot_RetentionCleanup_AnonymizesExpiredRecords()
    {
        // Create an expired retention record
        var expired = RegionHR.GDPR.Domain.RetentionRecord.Skapa(
            "Employee", "123", DateTime.UtcNow.AddDays(-1), "Test reason");
        _db.RetentionRecords.Add(expired);
        await _db.SaveChangesAsync();

        var rule = CreateRule("Gallring", "{\"typ\":\"autopilot\",\"mall\":\"gallring\"}");
        var context = new Dictionary<string, object>();

        var result = await _executor.ExecuteAsync(rule, AutomationLevel.Autopilot, context, CancellationToken.None);

        Assert.Contains("1 poster anonymiserade", result);
        Assert.True(expired.IsAnonymized);
    }

    [Fact]
    public async Task ExecuteAsync_Autopilot_RetentionCleanup_SkipsNonExpiredRecords()
    {
        // Create a non-expired retention record
        var notExpired = RegionHR.GDPR.Domain.RetentionRecord.Skapa(
            "Employee", "456", DateTime.UtcNow.AddDays(30), "Future reason");
        _db.RetentionRecords.Add(notExpired);
        await _db.SaveChangesAsync();

        var rule = CreateRule("Gallring", "{\"typ\":\"autopilot\",\"mall\":\"gallring\"}");
        var context = new Dictionary<string, object>();

        var result = await _executor.ExecuteAsync(rule, AutomationLevel.Autopilot, context, CancellationToken.None);

        Assert.Contains("0 poster anonymiserade", result);
        Assert.False(notExpired.IsAnonymized);
    }

    // === Autopilot: CreateOffboardingCase ===

    [Fact]
    public async Task ExecuteAsync_Autopilot_OffboardingStart_CreatesOffboardingCase()
    {
        var empGuid = Guid.NewGuid();
        var rule = CreateRule("Offboarding", "{\"typ\":\"autopilot\",\"mall\":\"offboarding_start\"}");
        var context = new Dictionary<string, object>
        {
            { "anstallId", empGuid }
        };

        var result = await _executor.ExecuteAsync(rule, AutomationLevel.Autopilot, context, CancellationToken.None);
        await _db.SaveChangesAsync();

        Assert.Contains("Offboarding-ärende skapat", result);
        Assert.Single(_db.OffboardingCases.Local);
    }

    // === Autopilot: GenerateAGI ===

    [Fact]
    public async Task ExecuteAsync_Autopilot_AGI_CreatesNotification()
    {
        var rule = CreateRule("AGI-generering", "{\"typ\":\"autopilot\",\"mall\":\"agi_xml\"}");
        var context = new Dictionary<string, object>();

        var result = await _executor.ExecuteAsync(rule, AutomationLevel.Autopilot, context, CancellationToken.None);

        Assert.Contains("AGI-generering", result);
        Assert.Single(_db.Notifications.Local);
    }

    // === Autopilot: Generic (unknown mall) ===

    [Fact]
    public async Task ExecuteAsync_Autopilot_UnknownMall_CreatesGenericNotification()
    {
        var rule = CreateRule("Okaend regel", "{\"typ\":\"autopilot\",\"mall\":\"unknown_action\"}");
        var context = new Dictionary<string, object>
        {
            { "anstallId", Guid.NewGuid() }
        };

        var result = await _executor.ExecuteAsync(rule, AutomationLevel.Autopilot, context, CancellationToken.None);

        Assert.Contains("Generisk åtgärd utförd", result);
        Assert.Single(_db.Notifications.Local);
    }

    // === Missing context ===

    [Fact]
    public async Task ExecuteAsync_Autopilot_NoAnstallId_ReturnsAbortMessage()
    {
        var rule = CreateRule("Rehab utan anstallning", "{\"typ\":\"autopilot\",\"mall\":\"rehab_start\"}");
        var context = new Dictionary<string, object>(); // No anstallId

        var result = await _executor.ExecuteAsync(rule, AutomationLevel.Autopilot, context, CancellationToken.None);

        Assert.Contains("avbrut", result.ToLowerInvariant());
    }

    // === Helper ===

    private AutomationRule CreateRule(string name, string atgard, string villkor = "{}")
    {
        return AutomationRule.Skapa(
            name,
            _categoryId,
            "TestTrigger",
            villkor,
            atgard,
            AutomationLevel.Notify);
    }
}
