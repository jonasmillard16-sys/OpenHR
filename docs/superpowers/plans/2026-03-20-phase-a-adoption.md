# Phase A: Adoption Layer — Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make it possible for organizations to switch to OpenHR by building: domain event dispatch, automation framework, migration engine, and pluggable collective agreements.

**Architecture:** Three parallel tracks after a shared prerequisite (domain event dispatch). All new entities use schema-per-module, strongly-typed IDs for aggregates, EF Core configurations with snake_case columns, and xUnit tests. The automation framework replaces 4 existing background services with a unified configurable rule engine.

**Tech Stack:** .NET 9, EF Core 9, PostgreSQL 17, Blazor Server + MudBlazor 9.1, xUnit

**Spec:** `docs/superpowers/specs/2026-03-20-openhr-enterprise-expansion-design.md` (Sections 1-3, 15-16)

**Scope:** Phase A only (21 entities, ~11 routes, 3 parallel tracks). Phases B and C have separate plans.

---

## File Structure

### New Project Files (modules)
- `src/Modules/Agreements/RegionHR.Agreements.csproj` — Collective agreement domain
- `src/Modules/Automation/RegionHR.Automation.csproj` — Automation framework domain
- `src/Modules/Migration/RegionHR.Migration.csproj` — Migration engine domain

### Prerequisite: Domain Event Dispatch
- Create: `src/SharedKernel/Abstractions/IDomainEventDispatcher.cs`
- Create: `src/Infrastructure/Events/DomainEventDispatcher.cs`
- Create: `src/Infrastructure/Events/DomainEventInterceptor.cs`
- Modify: `src/Infrastructure/Persistence/RegionHRDbContext.cs` — register interceptor
- Modify: `src/Web/Program.cs` — register DI services
- Test: `tests/Infrastructure.Tests/Events/DomainEventDispatcherTests.cs`

### Track 1: Automation Framework (5 entities, schema: `automation`)
- Create: `src/Modules/Automation/Domain/AutomationRule.cs`
- Create: `src/Modules/Automation/Domain/AutomationCategory.cs`
- Create: `src/Modules/Automation/Domain/AutomationLevelConfig.cs`
- Create: `src/Modules/Automation/Domain/AutomationExecution.cs`
- Create: `src/Modules/Automation/Domain/AutomationSuggestion.cs`
- Create: `src/Modules/Automation/Domain/AutomationLevel.cs` (enum)
- Create: `src/Modules/Automation/Domain/AutomationEngine.cs` (service)
- Create: `src/SharedKernel/Domain/AutomationRuleId.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Automation/AutomationRuleConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Automation/AutomationCategoryConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Automation/AutomationLevelConfigConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Automation/AutomationExecutionConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Automation/AutomationSuggestionConfiguration.cs`
- Modify: `src/Infrastructure/Persistence/RegionHRDbContext.cs` — add 5 DbSets + converters
- Modify: `src/Infrastructure/Persistence/SeedData.cs` — seed 6 categories + 38 rules
- Create: `src/Web/Components/Pages/Admin/Automation.razor`
- Create: `src/Web/Components/Pages/Admin/AutomationKategori.razor`
- Create: `src/Web/Components/Pages/Admin/AutomationLogg.razor`
- Create: `src/Web/Components/Pages/Admin/AutomationForslag.razor`
- Create: `src/Api/Endpoints/AutomationEndpoints.cs`
- Test: `tests/Automation.Tests/AutomationRuleTests.cs`
- Test: `tests/Automation.Tests/AutomationEngineTests.cs`

### Track 2: Migration Engine (5 entities, schema: `migration`)
- Create: `src/Modules/Migration/Domain/MigrationJob.cs`
- Create: `src/Modules/Migration/Domain/MigrationMapping.cs`
- Create: `src/Modules/Migration/Domain/MigrationTemplate.cs`
- Create: `src/Modules/Migration/Domain/MigrationValidationError.cs`
- Create: `src/Modules/Migration/Domain/MigrationLog.cs`
- Create: `src/Modules/Migration/Domain/MigrationJobStatus.cs` (enum)
- Create: `src/Modules/Migration/Domain/SourceSystem.cs` (enum)
- Create: `src/Modules/Migration/Adapters/IMigrationAdapter.cs`
- Create: `src/Modules/Migration/Adapters/PAXmlAdapter.cs`
- Create: `src/Modules/Migration/Adapters/HeromaAdapter.cs`
- Create: `src/Modules/Migration/Adapters/GenericCSVAdapter.cs`
- Create: `src/Modules/Migration/Adapters/PersonecAdapter.cs`
- Create: `src/Modules/Migration/Adapters/HogiaAdapter.cs`
- Create: `src/Modules/Migration/Adapters/FortnoxAdapter.cs`
- Create: `src/Modules/Migration/Adapters/SIE4iAdapter.cs`
- Create: `src/Modules/Migration/Adapters/WorkdayAdapter.cs`
- Create: `src/Modules/Migration/Adapters/SAPAdapter.cs`
- Create: `src/Modules/Migration/Adapters/OracleHCMAdapter.cs`
- Create: `src/Modules/Migration/Services/MigrationEngine.cs`
- Create: `src/SharedKernel/Domain/MigrationJobId.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Migration/MigrationJobConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Migration/MigrationMappingConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Migration/MigrationTemplateConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Migration/MigrationValidationErrorConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Migration/MigrationLogConfiguration.cs`
- Modify: `src/Infrastructure/Persistence/RegionHRDbContext.cs` — add 5 DbSets + converters
- Modify: `src/Infrastructure/Persistence/SeedData.cs` — seed mapping templates
- Create: `src/Web/Components/Pages/Admin/Migration/MigrationDashboard.razor`
- Create: `src/Web/Components/Pages/Admin/Migration/NyMigration.razor`
- Create: `src/Web/Components/Pages/Admin/Migration/MigrationDetalj.razor`
- Create: `src/Web/Components/Pages/Admin/Migration/MigrationMallar.razor`
- Create: `src/Api/Endpoints/MigrationEndpoints.cs`
- Test: `tests/Migration.Tests/MigrationJobTests.cs`
- Test: `tests/Migration.Tests/PAXmlAdapterTests.cs`
- Test: `tests/Migration.Tests/HeromaAdapterTests.cs`
- Test: `tests/Migration.Tests/MigrationEngineTests.cs`

### Track 3: Collective Agreements (11 entities, schema: `agreements`)
- Create: `src/Modules/Agreements/Domain/CollectiveAgreement.cs`
- Create: `src/Modules/Agreements/Domain/AgreementOBRate.cs`
- Create: `src/Modules/Agreements/Domain/AgreementOvertimeRule.cs`
- Create: `src/Modules/Agreements/Domain/AgreementVacationRule.cs`
- Create: `src/Modules/Agreements/Domain/AgreementRestRule.cs`
- Create: `src/Modules/Agreements/Domain/AgreementSalaryStructure.cs`
- Create: `src/Modules/Agreements/Domain/AgreementWorkingHours.cs`
- Create: `src/Modules/Agreements/Domain/AgreementNoticePeriod.cs`
- Create: `src/Modules/Agreements/Domain/AgreementPensionRule.cs`
- Create: `src/Modules/Agreements/Domain/AgreementInsurancePackage.cs`
- Create: `src/Modules/Agreements/Domain/PrivateCompensationPlan.cs`
- Create: `src/Modules/Agreements/Domain/PensionType.cs` (enum)
- Create: `src/Modules/Agreements/Domain/IndustrySector.cs` (enum)
- Create: `src/Modules/Agreements/Calculators/ITP2Calculator.cs`
- Create: `src/Modules/Agreements/Calculators/PA16Calculator.cs`
- Create: `src/SharedKernel/Domain/CollectiveAgreementId.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/CollectiveAgreementConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/AgreementOBRateConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/AgreementOvertimeRuleConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/AgreementVacationRuleConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/AgreementRestRuleConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/AgreementSalaryStructureConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/AgreementWorkingHoursConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/AgreementNoticePeriodConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/AgreementPensionRuleConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/AgreementInsurancePackageConfiguration.cs`
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/PrivateCompensationPlanConfiguration.cs`
- Modify: `src/Infrastructure/Persistence/RegionHRDbContext.cs` — add 11 DbSets + converters
- Modify: `src/Infrastructure/Persistence/SeedData.cs` — seed 10 agreements with rates
- Modify: `src/Modules/Core/Domain/Employment.cs` — add CollectiveAgreementId FK
- Modify: `src/Modules/Core/Domain/OrganizationUnit.cs` — add DefaultCollectiveAgreementId FK
- Modify: `src/Infrastructure/Persistence/Configurations/CoreHR/EmploymentConfiguration.cs` — map new FK
- Modify: `src/Infrastructure/Persistence/Configurations/CoreHR/OrganizationUnitConfiguration.cs` — map new FK
- Modify: `src/Modules/Payroll/Domain/CollectiveAgreementRulesEngine.cs` — refactor to DB-backed
- Create: `src/Web/Components/Pages/Admin/Avtal/AvtalLista.razor`
- Create: `src/Web/Components/Pages/Admin/Avtal/AvtalDetalj.razor`
- Create: `src/Web/Components/Pages/Admin/Avtal/AvtalRedigera.razor`
- Create: `src/Api/Endpoints/AgreementEndpoints.cs`
- Test: `tests/Agreements.Tests/CollectiveAgreementTests.cs`
- Test: `tests/Agreements.Tests/AgreementRulesEngineTests.cs`
- Test: `tests/Agreements.Tests/ITP2CalculatorTests.cs`

### EF Migration
- Create: `src/Infrastructure/Persistence/Migrations/{timestamp}_AddPhaseAEntities.cs` (generated)

---

## Task 1: Domain Event Dispatch Infrastructure (prerequisite)

**Files:**
- Create: `src/SharedKernel/Abstractions/IDomainEventDispatcher.cs`
- Create: `src/Infrastructure/Events/DomainEventDispatcher.cs`
- Create: `src/Infrastructure/Events/DomainEventInterceptor.cs`
- Modify: `src/Web/Program.cs`
- Test: `tests/Infrastructure.Tests/Events/DomainEventDispatcherTests.cs`

**Context:** The existing codebase has `IDomainEvent`, `DomainEvent` abstract record, `IDomainEventHandler<T>`, and `Entity<TId>.RaiseDomainEvent()`. But there is no dispatcher — events are raised but never delivered. We need a `SaveChangesInterceptor` that collects events from changed aggregates after save and dispatches them.

- [ ] **Step 1: Write the dispatcher interface and test**

Create `tests/Infrastructure.Tests/Events/DomainEventDispatcherTests.cs`:
```csharp
using RegionHR.SharedKernel.Abstractions;
using Xunit;

namespace RegionHR.Infrastructure.Tests.Events;

public class DomainEventDispatcherTests
{
    private sealed record TestEvent(string Data) : DomainEvent;

    private sealed class TestHandler : IDomainEventHandler<TestEvent>
    {
        public List<TestEvent> Received { get; } = [];
        public Task HandleAsync(TestEvent domainEvent, CancellationToken ct = default)
        {
            Received.Add(domainEvent);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task DispatchAsync_CallsMatchingHandler()
    {
        var handler = new TestHandler();
        var services = new ServiceCollection();
        services.AddScoped<IDomainEventHandler<TestEvent>>(_ => handler);
        var provider = services.BuildServiceProvider();

        var dispatcher = new DomainEventDispatcher(provider);
        var evt = new TestEvent("hello");

        await dispatcher.DispatchAsync(new[] { evt });

        Assert.Single(handler.Received);
        Assert.Equal("hello", handler.Received[0].Data);
    }

    [Fact]
    public async Task DispatchAsync_NoHandler_DoesNotThrow()
    {
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var dispatcher = new DomainEventDispatcher(provider);

        await dispatcher.DispatchAsync(new IDomainEvent[] { new TestEvent("orphan") });
        // Should not throw
    }
}
```

You need to add `using Microsoft.Extensions.DependencyInjection;` and ensure the test project references Infrastructure. If no test project exists for Infrastructure, create `tests/Infrastructure.Tests/RegionHR.Infrastructure.Tests.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Infrastructure\RegionHR.Infrastructure.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Run test — verify it fails**

```bash
dotnet test tests/Infrastructure.Tests --filter "DomainEventDispatcherTests" -v n
```
Expected: Compilation error — `DomainEventDispatcher` does not exist.

- [ ] **Step 3: Create the dispatcher interface**

Create `src/SharedKernel/Abstractions/IDomainEventDispatcher.cs`:
```csharp
namespace RegionHR.SharedKernel.Abstractions;

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default);
}
```

- [ ] **Step 4: Create the dispatcher implementation**

Create `src/Infrastructure/Events/DomainEventDispatcher.cs`:
```csharp
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.Infrastructure.Events;

public sealed class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync(IEnumerable<IDomainEvent> events, CancellationToken ct = default)
    {
        foreach (var domainEvent in events)
        {
            var eventType = domainEvent.GetType();
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(eventType);
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                if (handler is null) continue;
                var method = handlerType.GetMethod("HandleAsync");
                if (method is null) continue;
                await (Task)method.Invoke(handler, new object[] { domainEvent, ct })!;
            }
        }
    }
}
```

- [ ] **Step 5: Run test — verify it passes**

```bash
dotnet test tests/Infrastructure.Tests --filter "DomainEventDispatcherTests" -v n
```
Expected: 2 tests pass.

- [ ] **Step 6: Create the SaveChanges interceptor**

Create `src/Infrastructure/Events/DomainEventInterceptor.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.Infrastructure.Events;

public sealed class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventDispatcher _dispatcher;

    public DomainEventInterceptor(IDomainEventDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null) return result;

        // Collect domain events from all tracked entities that have them
        var events = new List<IDomainEvent>();
        foreach (var entry in eventData.Context.ChangeTracker.Entries())
        {
            // Use reflection to check for DomainEvents — works with any Entity<TId>
            var eventsProperty = entry.Entity.GetType().GetProperty("DomainEvents");
            if (eventsProperty?.GetValue(entry.Entity) is IReadOnlyList<IDomainEvent> domainEvents && domainEvents.Count > 0)
            {
                events.AddRange(domainEvents);
                entry.Entity.GetType().GetMethod("ClearDomainEvents")?.Invoke(entry.Entity, null);
            }
        }

        if (events.Count > 0)
        {
            await _dispatcher.DispatchAsync(events, cancellationToken);
        }

        return result;
    }
}
```

- [ ] **Step 7: Register in DI**

Modify `src/Infrastructure/DependencyInjection.cs` (NOT `src/Web/Program.cs` — the DbContext is registered in the Infrastructure DI extension method). Add after existing service registrations inside `AddInfrastructure()`:
```csharp
// Domain event dispatch
services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
services.AddScoped<DomainEventInterceptor>();
```

Find the existing `AddDbContext<RegionHRDbContext>` call in `DependencyInjection.cs` and add the `DomainEventInterceptor` alongside the existing `AuditInterceptor`:
```csharp
options.AddInterceptors(
    sp.GetRequiredService<AuditInterceptor>(),
    sp.GetRequiredService<DomainEventInterceptor>());
```

Add `using RegionHR.Infrastructure.Events;` and `using RegionHR.SharedKernel.Abstractions;` at top.

- [ ] **Step 8: Build and verify**

```bash
dotnet build RegionHR.sln
dotnet test RegionHR.sln
```
Expected: 0 errors, all existing tests pass + 2 new tests pass.

- [ ] **Step 9: Commit**

```bash
git add -A
git commit -m "feat: add domain event dispatch infrastructure

IDomainEventDispatcher interface + DomainEventDispatcher implementation.
DomainEventInterceptor collects events after SaveChanges and dispatches.
Prerequisite for automation framework (Phase A)."
```

---

## Task 2: Automation Framework — Domain Entities

**Files:**
- Create: `src/Modules/Automation/RegionHR.Automation.csproj`
- Create: `src/Modules/Automation/Domain/AutomationLevel.cs`
- Create: `src/Modules/Automation/Domain/AutomationCategory.cs`
- Create: `src/Modules/Automation/Domain/AutomationRule.cs`
- Create: `src/Modules/Automation/Domain/AutomationLevelConfig.cs`
- Create: `src/Modules/Automation/Domain/AutomationExecution.cs`
- Create: `src/Modules/Automation/Domain/AutomationSuggestion.cs`
- Create: `src/SharedKernel/Domain/AutomationRuleId.cs`
- Test: `tests/Automation.Tests/RegionHR.Automation.Tests.csproj`
- Test: `tests/Automation.Tests/AutomationRuleTests.cs`

- [ ] **Step 1: Create module project**

Create `src/Modules/Automation/RegionHR.Automation.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <RootNamespace>RegionHR.Automation</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\SharedKernel\RegionHR.SharedKernel.csproj" />
  </ItemGroup>
</Project>
```

Add project reference in `src/Infrastructure/RegionHR.Infrastructure.csproj`:
```xml
<ProjectReference Include="..\Modules\Automation\RegionHR.Automation.csproj" />
```

Add to `RegionHR.sln` via:
```bash
dotnet sln RegionHR.sln add src/Modules/Automation/RegionHR.Automation.csproj
```

- [ ] **Step 2: Create strongly-typed ID**

Create `src/SharedKernel/Domain/AutomationRuleId.cs`:
```csharp
namespace RegionHR.SharedKernel.Domain;

public readonly record struct AutomationRuleId(Guid Value)
{
    public static AutomationRuleId New() => new(Guid.NewGuid());
    public static AutomationRuleId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}

public readonly record struct AutomationCategoryId(Guid Value)
{
    public static AutomationCategoryId New() => new(Guid.NewGuid());
    public static AutomationCategoryId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}
```

- [ ] **Step 3: Create enum**

Create `src/Modules/Automation/Domain/AutomationLevel.cs`:
```csharp
namespace RegionHR.Automation.Domain;

public enum AutomationLevel
{
    Notify,
    Suggest,
    Autopilot,
    Block  // For legal requirements that cannot be lowered
}

public enum SuggestionStatus
{
    Pending,
    Accepted,
    Dismissed
}
```

- [ ] **Step 4: Create domain entities**

Create `src/Modules/Automation/Domain/AutomationCategory.cs`:
```csharp
using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Automation.Domain;

public sealed class AutomationCategory : Entity<AutomationCategoryId>
{
    public string Namn { get; private set; } = string.Empty;
    public string Beskrivning { get; private set; } = string.Empty;
    public string Ikon { get; private set; } = string.Empty;

    private AutomationCategory() { }

    public static AutomationCategory Skapa(string namn, string beskrivning, string ikon)
    {
        return new AutomationCategory
        {
            Id = AutomationCategoryId.New(),
            Namn = namn,
            Beskrivning = beskrivning,
            Ikon = ikon
        };
    }
}
```

Create `src/Modules/Automation/Domain/AutomationRule.cs`:
```csharp
using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Automation.Domain;

public sealed class AutomationRule : AggregateRoot<AutomationRuleId>
{
    public string Namn { get; private set; } = string.Empty;
    public AutomationCategoryId KategoriId { get; private set; }
    public string TriggerTyp { get; private set; } = string.Empty;  // e.g. "LASAccumulation.Updated"
    public string Villkor { get; private set; } = "{}";  // JSON conditions
    public string Atgard { get; private set; } = "{}";   // JSON action definition
    public bool ArAktiv { get; private set; } = true;
    public AutomationLevel MinimumNiva { get; private set; } = AutomationLevel.Notify;
    public bool ArSystemRegel { get; private set; }

    private AutomationRule() { }

    public static AutomationRule Skapa(
        string namn, AutomationCategoryId kategoriId, string triggerTyp,
        string villkor, string atgard, AutomationLevel minimumNiva, bool arSystemRegel = true)
    {
        return new AutomationRule
        {
            Id = AutomationRuleId.New(),
            Namn = namn,
            KategoriId = kategoriId,
            TriggerTyp = triggerTyp,
            Villkor = villkor,
            Atgard = atgard,
            MinimumNiva = minimumNiva,
            ArSystemRegel = arSystemRegel
        };
    }

    public void Aktivera() => ArAktiv = true;
    public void Inaktivera()
    {
        if (ArSystemRegel) throw new InvalidOperationException("Systemregler kan inte inaktiveras.");
        ArAktiv = false;
    }
}
```

Create `src/Modules/Automation/Domain/AutomationLevelConfig.cs`:
```csharp
using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Automation.Domain;

public sealed class AutomationLevelConfig : Entity<Guid>
{
    public AutomationCategoryId KategoriId { get; private set; }
    public AutomationLevel ValdNiva { get; private set; }

    private AutomationLevelConfig() { }

    public static AutomationLevelConfig Skapa(AutomationCategoryId kategoriId, AutomationLevel niva)
    {
        return new AutomationLevelConfig
        {
            Id = Guid.NewGuid(),
            KategoriId = kategoriId,
            ValdNiva = niva
        };
    }

    public void AndraNiva(AutomationLevel nyNiva, AutomationLevel minimumForKategori)
    {
        if (nyNiva < minimumForKategori)
            throw new InvalidOperationException($"Nivå {nyNiva} är under minimum {minimumForKategori} för denna kategori.");
        ValdNiva = nyNiva;
    }
}
```

Create `src/Modules/Automation/Domain/AutomationExecution.cs`:
```csharp
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Automation.Domain;

public sealed class AutomationExecution
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public AutomationRuleId RegelId { get; private set; }
    public string HandelseTyp { get; private set; } = string.Empty;
    public string Resultat { get; private set; } = string.Empty;
    public AutomationLevel AnvandNiva { get; private set; }
    public string? UtfordAtgard { get; private set; }
    public DateTime Tidsstampel { get; private set; } = DateTime.UtcNow;
    public Guid? AuditEntryId { get; private set; }

    private AutomationExecution() { }

    public static AutomationExecution Skapa(
        AutomationRuleId regelId, string handelseTyp, string resultat,
        AutomationLevel anvandNiva, string? utfordAtgard, Guid? auditEntryId = null)
    {
        return new AutomationExecution
        {
            RegelId = regelId,
            HandelseTyp = handelseTyp,
            Resultat = resultat,
            AnvandNiva = anvandNiva,
            UtfordAtgard = utfordAtgard,
            AuditEntryId = auditEntryId
        };
    }
}
```

Create `src/Modules/Automation/Domain/AutomationSuggestion.cs`:
```csharp
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Automation.Domain;

public sealed class AutomationSuggestion
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public AutomationRuleId RegelId { get; private set; }
    public string ForeslagenAtgard { get; private set; } = "{}";
    public EmployeeId? SkapadFor { get; private set; }
    public SuggestionStatus Status { get; private set; } = SuggestionStatus.Pending;
    public DateTime SkapadVid { get; private set; } = DateTime.UtcNow;
    public DateTime? GiltigTill { get; private set; }

    private AutomationSuggestion() { }

    public static AutomationSuggestion Skapa(
        AutomationRuleId regelId, string foreslagenAtgard,
        EmployeeId? skapadFor = null, DateTime? giltigTill = null)
    {
        return new AutomationSuggestion
        {
            RegelId = regelId,
            ForeslagenAtgard = foreslagenAtgard,
            SkapadFor = skapadFor,
            GiltigTill = giltigTill
        };
    }

    public void Acceptera()
    {
        if (Status != SuggestionStatus.Pending) throw new InvalidOperationException("Förslaget är redan hanterat.");
        Status = SuggestionStatus.Accepted;
    }

    public void Avvisa()
    {
        if (Status != SuggestionStatus.Pending) throw new InvalidOperationException("Förslaget är redan hanterat.");
        Status = SuggestionStatus.Dismissed;
    }
}
```

- [ ] **Step 5: Write tests**

Create `tests/Automation.Tests/RegionHR.Automation.Tests.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\src\Modules\Automation\RegionHR.Automation.csproj" />
  </ItemGroup>
</Project>
```

```bash
dotnet sln RegionHR.sln add tests/Automation.Tests/RegionHR.Automation.Tests.csproj
```

Create `tests/Automation.Tests/AutomationRuleTests.cs`:
```csharp
using RegionHR.Automation.Domain;
using RegionHR.SharedKernel.Domain;
using Xunit;

namespace RegionHR.Automation.Tests;

public class AutomationRuleTests
{
    private static AutomationCategoryId TestCategory => AutomationCategoryId.New();

    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var catId = TestCategory;
        var rule = AutomationRule.Skapa("LAS-varning", catId, "LAS.Updated",
            """{"dagar": {">=": 305}}""", """{"type": "Notify"}""", AutomationLevel.Notify);

        Assert.NotEqual(default, rule.Id);
        Assert.Equal("LAS-varning", rule.Namn);
        Assert.Equal(catId, rule.KategoriId);
        Assert.True(rule.ArAktiv);
        Assert.True(rule.ArSystemRegel);
    }

    [Fact]
    public void Inaktivera_ThrowsForSystemRule()
    {
        var rule = AutomationRule.Skapa("Systemregel", TestCategory, "X", "{}", "{}", AutomationLevel.Notify);
        Assert.Throws<InvalidOperationException>(() => rule.Inaktivera());
    }

    [Fact]
    public void Inaktivera_WorksForCustomRule()
    {
        var rule = AutomationRule.Skapa("Custom", TestCategory, "X", "{}", "{}", AutomationLevel.Notify, arSystemRegel: false);
        rule.Inaktivera();
        Assert.False(rule.ArAktiv);
    }
}

public class AutomationSuggestionTests
{
    [Fact]
    public void Acceptera_ChangesPendingToAccepted()
    {
        var s = AutomationSuggestion.Skapa(AutomationRuleId.New(), """{"action":"konvertera"}""");
        s.Acceptera();
        Assert.Equal(SuggestionStatus.Accepted, s.Status);
    }

    [Fact]
    public void Acceptera_ThrowsIfAlreadyHandled()
    {
        var s = AutomationSuggestion.Skapa(AutomationRuleId.New(), "{}");
        s.Acceptera();
        Assert.Throws<InvalidOperationException>(() => s.Acceptera());
    }

    [Fact]
    public void Avvisa_ChangesPendingToDismissed()
    {
        var s = AutomationSuggestion.Skapa(AutomationRuleId.New(), "{}");
        s.Avvisa();
        Assert.Equal(SuggestionStatus.Dismissed, s.Status);
    }
}

public class AutomationLevelConfigTests
{
    [Fact]
    public void AndraNiva_ThrowsIfBelowMinimum()
    {
        var config = AutomationLevelConfig.Skapa(AutomationCategoryId.New(), AutomationLevel.Suggest);
        Assert.Throws<InvalidOperationException>(() => config.AndraNiva(AutomationLevel.Notify, AutomationLevel.Suggest));
    }

    [Fact]
    public void AndraNiva_AllowsRaising()
    {
        var config = AutomationLevelConfig.Skapa(AutomationCategoryId.New(), AutomationLevel.Notify);
        config.AndraNiva(AutomationLevel.Autopilot, AutomationLevel.Notify);
        Assert.Equal(AutomationLevel.Autopilot, config.ValdNiva);
    }
}
```

- [ ] **Step 6: Build and run tests**

```bash
dotnet build RegionHR.sln
dotnet test tests/Automation.Tests -v n
```
Expected: All tests pass.

- [ ] **Step 7: Commit**

```bash
git add -A
git commit -m "feat: add automation framework domain entities

AutomationRule, AutomationCategory, AutomationLevelConfig,
AutomationExecution, AutomationSuggestion with strongly-typed IDs.
Three levels: Notify/Suggest/Autopilot with legal minimum enforcement.
System rules cannot be deactivated."
```

---

## Task 3: Automation Framework — EF Configuration + DbContext

**Files:**
- Create: `src/Infrastructure/Persistence/Configurations/Automation/*.cs` (5 files)
- Modify: `src/Infrastructure/Persistence/RegionHRDbContext.cs`

- [ ] **Step 1: Create EF configurations**

Create `src/Infrastructure/Persistence/Configurations/Automation/AutomationRuleConfiguration.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RegionHR.Automation.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Persistence.Configurations.Automation;

public class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.ToTable("rules", "automation");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => AutomationRuleId.From(v))
            .HasColumnName("id");
        builder.Property(e => e.Namn).HasColumnName("namn").HasMaxLength(200).IsRequired();
        builder.Property(e => e.KategoriId)
            .HasConversion(id => id.Value, v => AutomationCategoryId.From(v))
            .HasColumnName("kategori_id");
        builder.Property(e => e.TriggerTyp).HasColumnName("trigger_typ").HasMaxLength(100).IsRequired();
        builder.Property(e => e.Villkor).HasColumnName("villkor").HasColumnType("jsonb");
        builder.Property(e => e.Atgard).HasColumnName("atgard").HasColumnType("jsonb");
        builder.Property(e => e.ArAktiv).HasColumnName("ar_aktiv");
        builder.Property(e => e.MinimumNiva).HasConversion<string>().HasColumnName("minimum_niva").HasMaxLength(20);
        builder.Property(e => e.ArSystemRegel).HasColumnName("ar_system_regel");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at");
        builder.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        builder.Ignore(e => e.DomainEvents);
        builder.Ignore(e => e.Version);
        builder.Ignore(e => e.CreatedBy);
        builder.Ignore(e => e.UpdatedBy);
    }
}
```

Create similar configurations for the other 4 entities following the same pattern: `AutomationCategoryConfiguration.cs` (table: `categories`, schema: `automation`), `AutomationLevelConfigConfiguration.cs` (table: `level_configs`, schema: `automation`), `AutomationExecutionConfiguration.cs` (table: `executions`, schema: `automation`), `AutomationSuggestionConfiguration.cs` (table: `suggestions`, schema: `automation`).

- [ ] **Step 2: Register in DbContext**

Add to `src/Infrastructure/Persistence/RegionHRDbContext.cs`:

After the Journeys section, add:
```csharp
// Automation (schema: automation)
public DbSet<RegionHR.Automation.Domain.AutomationRule> AutomationRules => Set<RegionHR.Automation.Domain.AutomationRule>();
public DbSet<RegionHR.Automation.Domain.AutomationCategory> AutomationCategories => Set<RegionHR.Automation.Domain.AutomationCategory>();
public DbSet<RegionHR.Automation.Domain.AutomationLevelConfig> AutomationLevelConfigs => Set<RegionHR.Automation.Domain.AutomationLevelConfig>();
public DbSet<RegionHR.Automation.Domain.AutomationExecution> AutomationExecutions => Set<RegionHR.Automation.Domain.AutomationExecution>();
public DbSet<RegionHR.Automation.Domain.AutomationSuggestion> AutomationSuggestions => Set<RegionHR.Automation.Domain.AutomationSuggestion>();
```

In `ConfigureConventions`, add:
```csharp
configurationBuilder.Properties<AutomationRuleId>().HaveConversion<AutomationRuleIdConverter>();
configurationBuilder.Properties<AutomationCategoryId>().HaveConversion<AutomationCategoryIdConverter>();
```

Add converter classes at bottom of file:
```csharp
public class AutomationRuleIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<AutomationRuleId, Guid>
{
    public AutomationRuleIdConverter() : base(v => v.Value, v => AutomationRuleId.From(v)) { }
}

public class AutomationCategoryIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<AutomationCategoryId, Guid>
{
    public AutomationCategoryIdConverter() : base(v => v.Value, v => AutomationCategoryId.From(v)) { }
}
```

- [ ] **Step 3: Build**

```bash
dotnet build RegionHR.sln
```
Expected: 0 errors.

- [ ] **Step 4: Generate migration**

```bash
cd src/Web
dotnet ef migrations add AddAutomationSchema --project ../Infrastructure/RegionHR.Infrastructure.csproj
```

Verify the generated migration creates the `automation` schema and 5 tables.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat: add automation EF configurations and migration

5 tables in 'automation' schema: rules, categories, level_configs,
executions, suggestions. Strongly-typed ID converters registered."
```

---

## Task 4: Automation Framework — Engine, Seed, Background Service

**Files:**
- Create: `src/Modules/Automation/Domain/AutomationEngine.cs`
- Modify: `src/Infrastructure/Persistence/SeedData.cs`
- Modify: `src/Web/Program.cs`
- Test: `tests/Automation.Tests/AutomationEngineTests.cs`

This is the core service that replaces the 4 existing background services.

- [ ] **Step 1: Create AutomationEngine**

Create `src/Modules/Automation/Domain/AutomationEngine.cs`:
```csharp
using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Automation.Domain;

public interface IAutomationEngine
{
    Task EvaluateAsync(IDomainEvent domainEvent, CancellationToken ct = default);
    Task EvaluateCronAsync(string cronCategory, CancellationToken ct = default);
}
```

The full implementation will be created in `src/Infrastructure/Services/AutomationEngineService.cs` since it needs DbContext access. The interface lives in the domain module.

- [ ] **Step 2: Seed categories and rules**

Add to `src/Infrastructure/Persistence/SeedData.cs` — after existing seed data, before final `SaveChangesAsync`:
```csharp
// === Automation Categories ===
var compliance = AutomationCategory.Skapa("Compliance", "Arbetsrättsliga regler (LAS, ATL)", "Gavel");
var franvaro = AutomationCategory.Skapa("Frånvaro", "Sjukdom och frånvarohantering", "LocalHospital");
var lon = AutomationCategory.Skapa("Lön", "Löneberäkning och ekonomi", "Payments");
var kompetens = AutomationCategory.Skapa("Kompetens", "Certifieringar och utbildning", "School");
var rekrytering = AutomationCategory.Skapa("Rekrytering", "Onboarding och offboarding", "PersonAdd");
var gdpr = AutomationCategory.Skapa("GDPR", "Dataskydd och gallring", "Security");

db.AutomationCategories.AddRange(compliance, franvaro, lon, kompetens, rekrytering, gdpr);

// === Automation Rules (compliance) ===
db.AutomationRules.AddRange(
    AutomationRule.Skapa("LAS-varning 10 månader", compliance.Id, "LASAccumulation.Updated",
        """{"ackumuleradeDagar": {">=": 305}}""",
        """{"type": "Notify", "template": "LAS_10_MONTHS"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("LAS-varning 11 månader", compliance.Id, "LASAccumulation.Updated",
        """{"ackumuleradeDagar": {">=": 335}}""",
        """{"type": "Suggest", "action": "ConvertToTillsvidare"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("LAS-konvertering 12 månader", compliance.Id, "LASAccumulation.Updated",
        """{"ackumuleradeDagar": {">=": 365}}""",
        """{"type": "Action", "action": "ConvertToTillsvidare"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("ATL 11-timmarsbrott", compliance.Id, "ScheduledShift.Created",
        """{"vilaTimmar": {"<": 11}}""",
        """{"type": "Block"}""",
        AutomationLevel.Block),
    AutomationRule.Skapa("ATL veckovila", compliance.Id, "ScheduledShift.Created",
        """{"veckovilaTimmar": {"<": 36}}""",
        """{"type": "Block"}""",
        AutomationLevel.Block)
);

// === Automation Rules (frånvaro) ===
db.AutomationRules.AddRange(
    AutomationRule.Skapa("FK-anmälan dag 15", franvaro.Id, "SickLeave.DayCount",
        """{"sammanhangandeDagar": {">=": 14}}""",
        """{"type": "Suggest", "action": "CreateFKNotification"}""",
        AutomationLevel.Suggest),
    AutomationRule.Skapa("Rehab-trigger 6 tillfällen", franvaro.Id, "SickLeave.Created",
        """{"tillfallen12Manader": {">=": 6}}""",
        """{"type": "Suggest", "action": "CreateRehabCase"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("Rehab-trigger 14 dagar", franvaro.Id, "SickLeave.DayCount",
        """{"sammanhangandeDagar": {">=": 14}}""",
        """{"type": "Suggest", "action": "CreateRehabCase"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("Rehab milstolpe dag 90", franvaro.Id, "RehabCase.DayCount",
        """{"rehabDagar": {">=": 90}}""",
        """{"type": "Notify", "template": "REHAB_DAY_90"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("Semestergodkännande enkel", franvaro.Id, "LeaveRequest.Created",
        """{"typ": "Semester", "antalDagar": {"<=": 3}, "saldoRacker": true}""",
        """{"type": "Action", "action": "AutoApproveLeave"}""",
        AutomationLevel.Notify)
);

// === Automation Rules (lön) ===
db.AutomationRules.AddRange(
    AutomationRule.Skapa("AGI-generering", lon.Id, "Cron.MonthEnd",
        """{}""",
        """{"type": "Action", "action": "GenerateAGI"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("Lönekörning påminnelse", lon.Id, "Cron.Day20",
        """{"ingenKorningForPeriod": true}""",
        """{"type": "Notify", "template": "PAYROLL_REMINDER"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("SIE-export vid lönekörning", lon.Id, "PayrollRun.Paid",
        """{}""",
        """{"type": "Action", "action": "GenerateSIE4i"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("Minimilönvarning", lon.Id, "Employment.SalaryChanged",
        """{"lonUnderAvtalsMinimum": true}""",
        """{"type": "Notify", "template": "SALARY_BELOW_MINIMUM"}""",
        AutomationLevel.Notify)
);

// === Automation Rules (kompetens) ===
db.AutomationRules.AddRange(
    AutomationRule.Skapa("Certifiering förfaller 90 dagar", kompetens.Id, "Cron.Daily",
        """{"dagarTillForfall": {"<=": 90}}""",
        """{"type": "Notify", "template": "CERT_EXPIRY_90"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("Certifiering förfaller 30 dagar", kompetens.Id, "Cron.Daily",
        """{"dagarTillForfall": {"<=": 30}}""",
        """{"type": "Notify", "template": "CERT_EXPIRY_30", "escalate": "HR"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("Obligatorisk utbildning försenad", kompetens.Id, "Cron.Weekly",
        """{"saknarObligatoriskUtbildning": true}""",
        """{"type": "Suggest", "action": "EnrollInTraining"}""",
        AutomationLevel.Notify)
);

// === Automation Rules (rekrytering) ===
db.AutomationRules.AddRange(
    AutomationRule.Skapa("Onboarding auto-start", rekrytering.Id, "Employment.Created",
        """{}""",
        """{"type": "Action", "action": "CreateJourneyInstance"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("Provanställning utgår", rekrytering.Id, "Cron.Daily",
        """{"provanstallningSlutar30Dagar": true}""",
        """{"type": "Notify", "template": "PROBATION_ENDING"}""",
        AutomationLevel.Notify),
    AutomationRule.Skapa("Offboarding auto-start", rekrytering.Id, "Employment.EndDateSet",
        """{}""",
        """{"type": "Action", "action": "CreateOffboardingCase"}""",
        AutomationLevel.Notify)
);

// === Automation Rules (GDPR) ===
db.AutomationRules.AddRange(
    AutomationRule.Skapa("Retentionrensning", gdpr.Id, "Cron.Nightly",
        """{}""",
        """{"type": "Action", "action": "RetentionCleanup"}""",
        AutomationLevel.Autopilot),
    AutomationRule.Skapa("DSR-deadline", gdpr.Id, "Cron.Daily",
        """{"dagarSedanDSR": {">=": 25}}""",
        """{"type": "Notify", "template": "DSR_DEADLINE", "escalate": "DPO"}""",
        AutomationLevel.Notify)
);

// === Default level configs (all start at Notify except GDPR=Autopilot) ===
db.AutomationLevelConfigs.AddRange(
    AutomationLevelConfig.Skapa(compliance.Id, AutomationLevel.Notify),
    AutomationLevelConfig.Skapa(franvaro.Id, AutomationLevel.Notify),
    AutomationLevelConfig.Skapa(lon.Id, AutomationLevel.Notify),
    AutomationLevelConfig.Skapa(kompetens.Id, AutomationLevel.Notify),
    AutomationLevelConfig.Skapa(rekrytering.Id, AutomationLevel.Notify),
    AutomationLevelConfig.Skapa(gdpr.Id, AutomationLevel.Autopilot)
);
```

Add `using RegionHR.Automation.Domain;` at the top of SeedData.cs.

- [ ] **Step 3: Build and verify seed compiles**

```bash
dotnet build RegionHR.sln
```

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "feat: seed 6 automation categories + 22 built-in rules

Categories: Compliance, Frånvaro, Lön, Kompetens, Rekrytering, GDPR.
Rules cover LAS, ATL, FK, rehab, AGI, SIE, certifications, onboarding,
offboarding, retention. Default levels: Notify (GDPR: Autopilot)."
```

---

## Task 5: Automation Framework — Admin UI Pages

**Files:**
- Create: `src/Web/Components/Pages/Admin/Automation.razor`
- Create: `src/Web/Components/Pages/Admin/AutomationLogg.razor`
- Create: `src/Web/Components/Pages/Admin/AutomationForslag.razor`

- [ ] **Step 1: Create main automation admin page**

Create `src/Web/Components/Pages/Admin/Automation.razor`:
```razor
@page "/admin/automation"
@layout RegionHR.Web.Components.Layout.AdminLayout
@using RegionHR.Automation.Domain
@using RegionHR.Infrastructure.Persistence
@using Microsoft.EntityFrameworkCore
@inject RegionHRDbContext Db

<PageTitle>OpenHR — Automatisering</PageTitle>
<MudText Typo="Typo.h4" Class="mb-4" Style="font-weight: 700;">Automatiseringsinställningar</MudText>
<MudText Typo="Typo.body2" Class="mb-4" Style="color: var(--mud-palette-text-secondary);">
    Konfigurera automatiseringsnivå per regelkategori. Systemet kan meddela, föreslå eller agera automatiskt.
</MudText>

@if (_loading)
{
    <MudProgressCircular Indeterminate="true" />
}
else
{
    <MudTable Items="_categories" Dense="true" Hover="true" Elevation="2">
        <HeaderContent>
            <MudTh>Kategori</MudTh>
            <MudTh>Beskrivning</MudTh>
            <MudTh>Antal regler</MudTh>
            <MudTh>Nuvarande nivå</MudTh>
            <MudTh>Ändra</MudTh>
        </HeaderContent>
        <RowTemplate>
            <MudTd>
                <MudIcon Icon="@GetIcon(context.Category.Ikon)" Size="Size.Small" Class="mr-1" />
                @context.Category.Namn
            </MudTd>
            <MudTd>@context.Category.Beskrivning</MudTd>
            <MudTd>@context.RuleCount</MudTd>
            <MudTd>
                <MudChip T="string" Size="Size.Small"
                         Color="@LevelColor(context.Config?.ValdNiva ?? AutomationLevel.Notify)">
                    @(context.Config?.ValdNiva.ToString() ?? "Notify")
                </MudChip>
            </MudTd>
            <MudTd>
                @if (context.MinLevel == AutomationLevel.Block || context.MinLevel == AutomationLevel.Autopilot)
                {
                    <MudIcon Icon="@Icons.Material.Filled.Lock" Size="Size.Small" />
                    <MudText Typo="Typo.caption">Lagkrav</MudText>
                }
                else
                {
                    <MudSelect T="AutomationLevel" Value="context.Config?.ValdNiva ?? AutomationLevel.Notify"
                               ValueChanged="@(v => ChangeLevel(context, v))"
                               Variant="Variant.Outlined" Dense="true" Style="max-width: 160px;">
                        <MudSelectItem Value="AutomationLevel.Notify">Meddela</MudSelectItem>
                        <MudSelectItem Value="AutomationLevel.Suggest">Föreslå</MudSelectItem>
                        <MudSelectItem Value="AutomationLevel.Autopilot">Autopilot</MudSelectItem>
                    </MudSelect>
                }
            </MudTd>
        </RowTemplate>
    </MudTable>

    <div class="d-flex gap-2 mt-4">
        <MudButton Variant="Variant.Outlined" Href="/admin/automation/logg" StartIcon="@Icons.Material.Filled.History">
            Exekveringslogg
        </MudButton>
        <MudButton Variant="Variant.Outlined" Href="/admin/automation/forslag" StartIcon="@Icons.Material.Filled.Lightbulb">
            Väntande förslag (@_pendingSuggestions)
        </MudButton>
    </div>
}

@code {
    private List<CategoryViewModel> _categories = [];
    private int _pendingSuggestions;
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var categories = await Db.AutomationCategories.OrderBy(c => c.Namn).ToListAsync();
            var rules = await Db.AutomationRules.ToListAsync();
            var configs = await Db.AutomationLevelConfigs.ToListAsync();
            _pendingSuggestions = await Db.AutomationSuggestions
                .CountAsync(s => s.Status == SuggestionStatus.Pending);

            _categories = categories.Select(c => new CategoryViewModel
            {
                Category = c,
                RuleCount = rules.Count(r => r.KategoriId == c.Id),
                Config = configs.FirstOrDefault(cfg => cfg.KategoriId == c.Id),
                MinLevel = rules.Where(r => r.KategoriId == c.Id)
                    .Select(r => r.MinimumNiva).DefaultIfEmpty(AutomationLevel.Notify).Max()
            }).ToList();
        }
        finally { _loading = false; }
    }

    private async Task ChangeLevel(CategoryViewModel vm, AutomationLevel newLevel)
    {
        if (vm.Config is null)
        {
            vm.Config = AutomationLevelConfig.Skapa(vm.Category.Id, newLevel);
            Db.AutomationLevelConfigs.Add(vm.Config);
        }
        else
        {
            vm.Config.AndraNiva(newLevel, vm.MinLevel);
        }
        await Db.SaveChangesAsync();
    }

    private static Color LevelColor(AutomationLevel level) => level switch
    {
        AutomationLevel.Notify => Color.Info,
        AutomationLevel.Suggest => Color.Warning,
        AutomationLevel.Autopilot => Color.Success,
        AutomationLevel.Block => Color.Error,
        _ => Color.Default
    };

    private static string GetIcon(string ikon) => ikon switch
    {
        "Gavel" => Icons.Material.Filled.Gavel,
        "LocalHospital" => Icons.Material.Filled.LocalHospital,
        "Payments" => Icons.Material.Filled.Payments,
        "School" => Icons.Material.Filled.School,
        "PersonAdd" => Icons.Material.Filled.PersonAdd,
        "Security" => Icons.Material.Filled.Security,
        _ => Icons.Material.Filled.Settings
    };

    private sealed class CategoryViewModel
    {
        public AutomationCategory Category { get; set; } = null!;
        public int RuleCount { get; set; }
        public AutomationLevelConfig? Config { get; set; }
        public AutomationLevel MinLevel { get; set; }
    }
}
```

- [ ] **Step 2: Create AutomationKategori page**

Create `src/Web/Components/Pages/Admin/AutomationKategori.razor` with route `/admin/automation/{kategori}`. Shows all rules in the selected category with individual level adjustment per rule. Uses MudTable listing each `AutomationRule` with columns: Namn, TriggerTyp, MinimumNiva, ArAktiv toggle.

- [ ] **Step 3: Create log and suggestions pages**

Create `src/Web/Components/Pages/Admin/AutomationLogg.razor` and `AutomationForslag.razor` following the same pattern — querying `Db.AutomationExecutions` and `Db.AutomationSuggestions` respectively. Use `MudTable` with date filtering and status chips.

- [ ] **Step 4: Create AutomationEngineService implementation**

Create `src/Infrastructure/Services/AutomationEngineService.cs`:
```csharp
using Microsoft.EntityFrameworkCore;
using RegionHR.Automation.Domain;
using RegionHR.Infrastructure.Persistence;
using RegionHR.SharedKernel.Abstractions;

namespace RegionHR.Infrastructure.Services;

public sealed class AutomationEngineService : IAutomationEngine
{
    private readonly RegionHRDbContext _db;

    public AutomationEngineService(RegionHRDbContext db)
    {
        _db = db;
    }

    public async Task EvaluateAsync(IDomainEvent domainEvent, CancellationToken ct = default)
    {
        var eventType = domainEvent.GetType().Name;
        var matchingRules = await _db.AutomationRules
            .Where(r => r.ArAktiv && r.TriggerTyp.Contains(eventType))
            .ToListAsync(ct);

        foreach (var rule in matchingRules)
        {
            var config = await _db.AutomationLevelConfigs
                .FirstOrDefaultAsync(c => c.KategoriId == rule.KategoriId, ct);
            var effectiveLevel = config?.ValdNiva ?? AutomationLevel.Notify;
            if (effectiveLevel < rule.MinimumNiva)
                effectiveLevel = rule.MinimumNiva;

            var execution = AutomationExecution.Skapa(
                rule.Id, eventType, "Evaluated", effectiveLevel, null);
            _db.AutomationExecutions.Add(execution);
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task EvaluateCronAsync(string cronCategory, CancellationToken ct = default)
    {
        var matchingRules = await _db.AutomationRules
            .Where(r => r.ArAktiv && r.TriggerTyp.StartsWith("Cron."))
            .ToListAsync(ct);
        // Evaluate cron-triggered rules
    }
}
```

Register in `src/Infrastructure/DependencyInjection.cs`:
```csharp
services.AddScoped<IAutomationEngine, AutomationEngineService>();
```

- [ ] **Step 5: Build and verify**

```bash
dotnet build RegionHR.sln
```

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "feat: add automation admin UI pages

/admin/automation — category list with level configuration
/admin/automation/logg — execution history
/admin/automation/forslag — pending suggestions management"
```

---

## Task 6: Collective Agreements — Domain Entities + Strongly-Typed ID

This is the start of Track 3. Can be implemented **in parallel** with Tasks 2-5.

**Files:**
- Create: `src/Modules/Agreements/RegionHR.Agreements.csproj`
- Create: `src/SharedKernel/Domain/CollectiveAgreementId.cs`
- Create: `src/Modules/Agreements/Domain/*.cs` (11 entities + enums)
- Test: `tests/Agreements.Tests/CollectiveAgreementTests.cs`

- [ ] **Step 1: Create module project + test project**

Create `src/Modules/Agreements/RegionHR.Agreements.csproj` (same pattern as Task 2 Step 1).
Create `tests/Agreements.Tests/RegionHR.Agreements.Tests.csproj`.
Add both to solution and add project reference in Infrastructure.

```bash
dotnet sln RegionHR.sln add src/Modules/Agreements/RegionHR.Agreements.csproj
dotnet sln RegionHR.sln add tests/Agreements.Tests/RegionHR.Agreements.Tests.csproj
```

- [ ] **Step 2: Create strongly-typed ID**

Create `src/SharedKernel/Domain/CollectiveAgreementId.cs`:
```csharp
namespace RegionHR.SharedKernel.Domain;

public readonly record struct CollectiveAgreementId(Guid Value)
{
    public static CollectiveAgreementId New() => new(Guid.NewGuid());
    public static CollectiveAgreementId From(Guid id) => new(id);
    public override string ToString() => Value.ToString();
}
```

- [ ] **Step 3: Create enums**

Create `src/Modules/Agreements/Domain/PensionType.cs`:
```csharp
namespace RegionHR.Agreements.Domain;

public enum PensionType { SAFLO, ITP1, ITP2, KAPKL, AKAPKR, PA16, Custom }
public enum IndustrySector { RegionKommun, Stat, Industri, Handel, IT, Vard, Transport, Hotell, Tjanstesektor, Ovrigt }
public enum AgreementStatus { Active, Inactive, Draft }
```

- [ ] **Step 4: Create CollectiveAgreement aggregate root**

Create `src/Modules/Agreements/Domain/CollectiveAgreement.cs`:
```csharp
using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Agreements.Domain;

public sealed class CollectiveAgreement : AggregateRoot<CollectiveAgreementId>
{
    public string Namn { get; private set; } = string.Empty;
    public string Parter { get; private set; } = string.Empty;
    public DateOnly GiltigFran { get; private set; }
    public DateOnly? GiltigTill { get; private set; }
    public IndustrySector Bransch { get; private set; }
    public AgreementStatus Status { get; private set; }

    private readonly List<AgreementOBRate> _obSatser = [];
    public IReadOnlyList<AgreementOBRate> OBSatser => _obSatser.AsReadOnly();

    private readonly List<AgreementOvertimeRule> _overtidsRegler = [];
    public IReadOnlyList<AgreementOvertimeRule> OvertidsRegler => _overtidsRegler.AsReadOnly();

    private readonly List<AgreementVacationRule> _semesterRegler = [];
    public IReadOnlyList<AgreementVacationRule> SemesterRegler => _semesterRegler.AsReadOnly();

    private readonly List<AgreementPensionRule> _pensionsRegler = [];
    public IReadOnlyList<AgreementPensionRule> PensionsRegler => _pensionsRegler.AsReadOnly();

    private CollectiveAgreement() { }

    public static CollectiveAgreement Skapa(
        string namn, string parter, DateOnly giltigFran,
        IndustrySector bransch, DateOnly? giltigTill = null)
    {
        return new CollectiveAgreement
        {
            Id = CollectiveAgreementId.New(),
            Namn = namn,
            Parter = parter,
            GiltigFran = giltigFran,
            GiltigTill = giltigTill,
            Bransch = bransch,
            Status = AgreementStatus.Active
        };
    }

    public void LaggTillOBSats(string tidstyp, decimal belopp, DateOnly giltigFran, DateOnly? giltigTill = null)
    {
        _obSatser.Add(new AgreementOBRate
        {
            Id = Guid.NewGuid(),
            Tidstyp = tidstyp,
            Belopp = belopp,
            GiltigFran = giltigFran,
            GiltigTill = giltigTill
        });
    }

    public void LaggTillOvertidsRegel(decimal tröskel, decimal multiplikator, decimal maxPerAr)
    {
        _overtidsRegler.Add(new AgreementOvertimeRule
        {
            Id = Guid.NewGuid(),
            Troskel = tröskel,
            Multiplikator = multiplikator,
            MaxPerAr = maxPerAr
        });
    }

    public void LaggTillSemesterRegel(int basDagar, int? extravid40, int? extravid50)
    {
        _semesterRegler.Add(new AgreementVacationRule
        {
            Id = Guid.NewGuid(),
            BasDagar = basDagar,
            ExtraDagarVid40 = extravid40,
            ExtraDagarVid50 = extravid50
        });
    }

    public void LaggTillPensionsRegel(PensionType typ, decimal satsUnderTak, decimal satsOverTak, decimal? tak)
    {
        _pensionsRegler.Add(new AgreementPensionRule
        {
            Id = Guid.NewGuid(),
            PensionsTyp = typ,
            SatsUnderTak = satsUnderTak,
            SatsOverTak = satsOverTak,
            Tak = tak
        });
    }

    public decimal HamtaOBSats(string tidstyp, DateOnly datum)
    {
        return _obSatser
            .Where(s => s.Tidstyp == tidstyp && s.GiltigFran <= datum && (s.GiltigTill == null || s.GiltigTill >= datum))
            .Select(s => s.Belopp)
            .FirstOrDefault();
    }
}
```

- [ ] **Step 5: Create owned sub-entities**

Create the remaining agreement sub-entities as simple classes (AgreementOBRate, AgreementOvertimeRule, AgreementVacationRule, AgreementRestRule, AgreementSalaryStructure, AgreementWorkingHours, AgreementNoticePeriod, AgreementPensionRule, AgreementInsurancePackage, PrivateCompensationPlan) — each with public properties and Guid Id. These are owned by CollectiveAgreement via `HasMany` relationships.

*(Each follows the same simple pattern: Guid Id, domain properties, no factory method since they're created through the aggregate's methods.)*

- [ ] **Step 6: Write tests**

Create `tests/Agreements.Tests/CollectiveAgreementTests.cs`:
```csharp
using RegionHR.Agreements.Domain;
using Xunit;

namespace RegionHR.Agreements.Tests;

public class CollectiveAgreementTests
{
    [Fact]
    public void Skapa_SetsAllProperties()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR + fack",
            new DateOnly(2025, 1, 1), IndustrySector.RegionKommun);
        Assert.Equal("AB", avtal.Namn);
        Assert.Equal(AgreementStatus.Active, avtal.Status);
    }

    [Fact]
    public void LaggTillOBSats_AddsToCollection()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.RegionKommun);
        avtal.LaggTillOBSats("VardagKvall", 126.50m, new DateOnly(2025, 1, 1));
        Assert.Single(avtal.OBSatser);
        Assert.Equal(126.50m, avtal.OBSatser[0].Belopp);
    }

    [Fact]
    public void HamtaOBSats_ReturnsCorrectRate()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.RegionKommun);
        avtal.LaggTillOBSats("VardagKvall", 126.50m, new DateOnly(2025, 1, 1));
        avtal.LaggTillOBSats("VardagNatt", 152.00m, new DateOnly(2025, 1, 1));

        Assert.Equal(126.50m, avtal.HamtaOBSats("VardagKvall", new DateOnly(2026, 3, 20)));
        Assert.Equal(152.00m, avtal.HamtaOBSats("VardagNatt", new DateOnly(2026, 3, 20)));
        Assert.Equal(0m, avtal.HamtaOBSats("Nonexistent", new DateOnly(2026, 3, 20)));
    }

    [Fact]
    public void LaggTillPensionsRegel_AddsRule()
    {
        var avtal = CollectiveAgreement.Skapa("AB", "SKR", new DateOnly(2025, 1, 1), IndustrySector.RegionKommun);
        avtal.LaggTillPensionsRegel(PensionType.AKAPKR, 6.0m, 31.5m, 7.5m);
        Assert.Single(avtal.PensionsRegler);
    }
}
```

- [ ] **Step 7: Build and test**

```bash
dotnet build RegionHR.sln
dotnet test tests/Agreements.Tests -v n
```

- [ ] **Step 8: Commit**

```bash
git add -A
git commit -m "feat: add collective agreements domain module

CollectiveAgreement aggregate with OB rates, overtime rules,
vacation rules, pension rules. 10 sub-entities as owned collections.
Supports AB, HÖK, Teknikavtalet, private sector agreements."
```

---

## Task 7: Collective Agreements — EF Config + Employment FK Migration

**Files:**
- Create: `src/Infrastructure/Persistence/Configurations/Agreements/*.cs`
- Modify: `src/Infrastructure/Persistence/RegionHRDbContext.cs`
- Modify: `src/Modules/Core/Domain/Employment.cs`
- Modify: `src/Infrastructure/Persistence/Configurations/CoreHR/EmploymentConfiguration.cs`

- [ ] **Step 1: Create EF configurations for all agreement entities**

Follow same pattern as Task 3 Step 1. Schema: `agreements`. Table names: `collective_agreements`, `ob_rates`, `overtime_rules`, `vacation_rules`, `rest_rules`, `salary_structures`, `working_hours`, `notice_periods`, `pension_rules`, `insurance_packages`, `private_compensation_plans`.

- [ ] **Step 2: Register DbSets and converters**

Add to RegionHRDbContext:
```csharp
// Agreements (schema: agreements)
public DbSet<RegionHR.Agreements.Domain.CollectiveAgreement> CollectiveAgreements => Set<RegionHR.Agreements.Domain.CollectiveAgreement>();
public DbSet<RegionHR.Agreements.Domain.AgreementOBRate> AgreementOBRates => Set<RegionHR.Agreements.Domain.AgreementOBRate>();
// ... etc for all 11 entities
```

Add converter:
```csharp
configurationBuilder.Properties<CollectiveAgreementId>().HaveConversion<CollectiveAgreementIdConverter>();
```

- [ ] **Step 3: Add CollectiveAgreementId to Employment AND OrganizationUnit**

Modify `src/Modules/Core/Domain/Employment.cs` — add:
```csharp
public CollectiveAgreementId? AvtalsId { get; private set; }

public void SattKollektivavtal(CollectiveAgreementId avtalsId)
{
    AvtalsId = avtalsId;
}
```

Modify `src/Modules/Core/Domain/OrganizationUnit.cs` — add:
```csharp
public CollectiveAgreementId? DefaultAvtalsId { get; private set; }

public void SattDefaultKollektivavtal(CollectiveAgreementId avtalsId)
{
    DefaultAvtalsId = avtalsId;
}
```

Modify `src/Infrastructure/Persistence/Configurations/CoreHR/EmploymentConfiguration.cs` — add:
```csharp
builder.Property(e => e.AvtalsId)
    .HasConversion(id => id == null ? (Guid?)null : id.Value.Value,
                   v => v == null ? null : CollectiveAgreementId.From(v.Value))
    .HasColumnName("kollektivavtal_id");
```

Modify `src/Infrastructure/Persistence/Configurations/CoreHR/OrganizationUnitConfiguration.cs` — add:
```csharp
builder.Property(e => e.DefaultAvtalsId)
    .HasConversion(id => id == null ? (Guid?)null : id.Value.Value,
                   v => v == null ? null : CollectiveAgreementId.From(v.Value))
    .HasColumnName("default_kollektivavtal_id");
```

- [ ] **Step 4: Generate migration with data migration SQL**

```bash
cd src/Web
dotnet ef migrations add AddAgreementsSchema --project ../Infrastructure/RegionHR.Infrastructure.csproj
```

After generating the migration, **manually add data migration SQL** in the `Up()` method to map existing `kollektivavtal` enum values to the new FK column. This is critical per spec Section 15.5:
```csharp
// In the generated migration's Up() method, after creating the agreements tables:
// NOTE: The actual agreement IDs must match the seed data GUIDs.
// This SQL maps the existing string-based Kollektivavtal column to the new FK.
migrationBuilder.Sql(@"
    UPDATE core_hr.employments SET kollektivavtal_id = (
        SELECT id FROM agreements.collective_agreements WHERE namn = employments.kollektivavtal
    ) WHERE kollektivavtal IS NOT NULL;
");
```

- [ ] **Step 5: Build**

```bash
dotnet build RegionHR.sln
```

- [ ] **Step 6: Commit**

```bash
git add -A
git commit -m "feat: add agreements EF config + Employment FK migration

11 tables in 'agreements' schema. Employment gets kollektivavtal_id FK.
CollectiveAgreementId converter registered in ConfigureConventions."
```

---

## Task 8: Collective Agreements — Seed Data + Refactor Engine

**Files:**
- Modify: `src/Infrastructure/Persistence/SeedData.cs`
- Modify: `src/Modules/Payroll/Domain/CollectiveAgreementRulesEngine.cs`
- Test: `tests/Agreements.Tests/AgreementRulesEngineTests.cs`

- [ ] **Step 1: Seed 10 agreements with full rates**

Add to SeedData.cs (before employees, since employees need agreement references):

```csharp
// === Collective Agreements ===
var ab = CollectiveAgreement.Skapa("AB", "SKR + Kommunal/Vision/Vårdförbundet",
    new DateOnly(2024, 4, 1), IndustrySector.RegionKommun);
ab.LaggTillOBSats("VardagKvall", 126.50m, new DateOnly(2025, 1, 1));
ab.LaggTillOBSats("VardagNatt", 152.00m, new DateOnly(2025, 1, 1));
ab.LaggTillOBSats("Helg", 89.00m, new DateOnly(2025, 1, 1));
ab.LaggTillOBSats("Storhelg", 195.00m, new DateOnly(2025, 1, 1));
ab.LaggTillOvertidsRegel(0, 1.8m, 200);
ab.LaggTillSemesterRegel(25, 31, 32);
ab.LaggTillPensionsRegel(PensionType.AKAPKR, 6.0m, 31.5m, 7.5m);
db.CollectiveAgreements.Add(ab);

// ... repeat for HÖK, Teknikavtalet, Handelsavtalet, IT/Telekomavtalet,
// Vårdföretagaravtalet, Transportavtalet, HRF-avtalet, Tjänstemannaavtalet, Avtalslöst
// (each with appropriate rates from the spec)
```

Update existing employee seed to set `AvtalsId`:
```csharp
employment.SattKollektivavtal(ab.Id);
```

- [ ] **Step 2: Refactor CollectiveAgreementRulesEngine to be DB-backed**

Modify `src/Modules/Payroll/Domain/CollectiveAgreementRulesEngine.cs`:

The engine should now accept a `CollectiveAgreementId` instead of `CollectiveAgreementType` enum, and look up rates from the database. The interface changes from:
```csharp
Task<decimal> GetOBRateAsync(CollectiveAgreementType agreement, OBCategory category, DateOnly date, ...);
```
to:
```csharp
Task<decimal> GetOBRateAsync(CollectiveAgreementId agreementId, OBCategory category, DateOnly date, ...);
```

Keep the old enum-based methods as backward-compatible wrappers that look up the agreement by name, for existing code that hasn't been migrated yet.

- [ ] **Step 3: Write tests for refactored engine**

Test that the DB-backed engine returns correct OB rates for AB and HÖK agreements by querying from the seeded data.

- [ ] **Step 4: Build and test**

```bash
dotnet build RegionHR.sln
dotnet test RegionHR.sln
```

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat: seed 10 collective agreements + refactor rules engine to DB-backed

AB, HÖK, Teknikavtalet, Handelsavtalet, IT/Telekomavtalet,
Vårdföretagaravtalet, Transportavtalet, HRF-avtalet, Tjänstemannaavtalet,
Avtalslöst. Engine now reads rates from DB instead of hardcoded values."
```

---

## Task 9: Migration Engine — Domain Entities

This is Track 2. Can be implemented **in parallel** with Tasks 2-8.

**Files:**
- Create: `src/Modules/Migration/RegionHR.Migration.csproj`
- Create: `src/SharedKernel/Domain/MigrationJobId.cs`
- Create: `src/Modules/Migration/Domain/*.cs` (5 entities + enums)
- Create: `src/Modules/Migration/Adapters/IMigrationAdapter.cs`
- Test: `tests/Migration.Tests/MigrationJobTests.cs`

- [ ] **Step 1: Create module, ID, enums, entities**

Follow same patterns as Tasks 2 and 6. Key entities: MigrationJob (aggregate root with MigrationJobId), MigrationMapping, MigrationTemplate, MigrationValidationError, MigrationLog. Enums: MigrationJobStatus (Created/Validating/DryRun/Importing/Complete/Failed), SourceSystem (PAXml/HEROMA/PersonecP/Hogia/Fortnox/SIE4i/Workday/SAP/OracleHCM/GenericCSV).

- [ ] **Step 2: Create adapter interface**

Create `src/Modules/Migration/Adapters/IMigrationAdapter.cs`:
```csharp
namespace RegionHR.Migration.Adapters;

public interface IMigrationAdapter
{
    SourceSystem Source { get; }
    Task<ParsedMigrationData> ParseAsync(Stream fileStream, CancellationToken ct = default);
    MigrationMapping[] GetDefaultMappings();
}

public sealed class ParsedMigrationData
{
    public List<ParsedEmployee> Employees { get; set; } = [];
    public List<ParsedEmployment> Employments { get; set; } = [];
    public List<ParsedPayrollRecord> PayrollRecords { get; set; } = [];
    public List<ParsedLeaveRecord> LeaveRecords { get; set; } = [];
    public List<ParsedOrganization> Organizations { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
}
```

- [ ] **Step 3: Write tests + build**

- [ ] **Step 4: Commit**

---

## Task 10: Migration Engine — PAXml + HEROMA Adapters

**Files:**
- Create: `src/Modules/Migration/Adapters/PAXmlAdapter.cs`
- Create: `src/Modules/Migration/Adapters/HeromaAdapter.cs`
- Create: `src/Modules/Migration/Adapters/GenericCSVAdapter.cs`
- Test: `tests/Migration.Tests/PAXmlAdapterTests.cs`
- Test: `tests/Migration.Tests/HeromaAdapterTests.cs`

- [ ] **Step 1: PAXml adapter**

Parse PAXml 2.0 XML (LONIN/LONUT/REGISTER). Use `System.Xml.Linq` (FOSS, built-in). Map `<personal>` to Employee, `<lonetransaktioner>` to PayrollRecord, `<tidtransaktioner>` to TimeRecord. Validate against XSD if available.

- [ ] **Step 2: HEROMA CSV adapter**

Parse semicolon-delimited CSV with HEROMA field names (PERSNR, FNAMN, ENAMN, ANST_FORM, KOL_AVTAL, MANLON, ENHET_KOD). Map to ParsedMigrationData.

- [ ] **Step 3: Generic CSV adapter**

Column-position-based parsing with user-defined mapping. Uses MigrationMapping to match columns.

- [ ] **Step 4: Write tests with sample data**

Create sample PAXml and HEROMA CSV test files in `tests/Migration.Tests/TestData/`.

- [ ] **Step 5: Build, test, commit**

---

## Task 11: Migration Engine — EF Config + Service + UI

**Files:**
- Create: `src/Infrastructure/Persistence/Configurations/Migration/*.cs`
- Create: `src/Infrastructure/Services/MigrationEngineService.cs`
- Modify: `src/Infrastructure/Persistence/RegionHRDbContext.cs`
- Create: `src/Web/Components/Pages/Admin/Migration/*.razor` (4 pages)

- [ ] **Step 1: EF configurations + DbContext registration**

Schema: `migration`. Tables: `jobs`, `mappings`, `templates`, `validation_errors`, `logs`.

- [ ] **Step 2: MigrationEngine service**

Orchestrates: parse → map → validate → dry-run → import → report. Uses `IMigrationAdapter` for format-specific parsing. Wraps import in transaction per batch (500 records). Creates MigrationLog entries per imported record.

- [ ] **Step 3: Create wizard UI pages**

4 pages: `/admin/migration` (dashboard), `/admin/migration/ny` (6-step wizard), `/admin/migration/{id}` (detail), `/admin/migration/mallar` (templates).

The wizard uses MudStepper or manual step tracking.

- [ ] **Step 4: Build, test all, commit**

---

## Task 12: Remaining Adapters + API Endpoints

**Files:**
- Create: Remaining adapter files (Personec, Hogia, Fortnox, SIE4i, Workday, SAP, Oracle)
- Create: `src/Api/Endpoints/MigrationEndpoints.cs`
- Create: `src/Api/Endpoints/AutomationEndpoints.cs`
- Create: `src/Api/Endpoints/AgreementEndpoints.cs`

- [ ] **Step 1: Create remaining adapters**

Each follows the IMigrationAdapter pattern. Adapters for systems without publicly documented formats (Personec, Hogia, Fortnox) use GenericCSV with pre-built mapping templates.

- [ ] **Step 2: Create API endpoints**

Follow existing pattern: `MapGroup("/api/v1/...")`, async lambdas, Results.Ok/NotFound/BadRequest.

- [ ] **Step 3: Register endpoints in `src/Api/Program.cs`**

Add to `src/Api/Program.cs` (where other `app.MapXxxEndpoints()` calls are):
```csharp
app.MapMigrationEndpoints();
app.MapAutomationEndpoints();
app.MapAgreementEndpoints();
```

- [ ] **Step 4: Add nav links to `src/Web/Components/Layout/NavMenu.razor`**

Add navigation items for the 3 new admin sections under the existing Admin group:
```razor
<MudNavLink Href="/admin/automation" Icon="@Icons.Material.Filled.SmartToy">Automatisering</MudNavLink>
<MudNavLink Href="/admin/avtal" Icon="@Icons.Material.Filled.Gavel">Kollektivavtal</MudNavLink>
<MudNavLink Href="/admin/migration" Icon="@Icons.Material.Filled.CloudUpload">Dataimport</MudNavLink>
```

- [ ] **Step 5: Build all, run all tests, commit**

---

## Task 13: Collective Agreements — Admin UI

**Files:**
- Create: `src/Web/Components/Pages/Admin/Avtal/AvtalLista.razor`
- Create: `src/Web/Components/Pages/Admin/Avtal/AvtalDetalj.razor`
- Create: `src/Web/Components/Pages/Admin/Avtal/AvtalRedigera.razor`

- [ ] **Step 1: Agreement list page**

`/admin/avtal` — MudTable with all agreements, status, sector, validity.

- [ ] **Step 2: Agreement detail page**

`/admin/avtal/{id}` — tabs for OB rates, overtime rules, vacation rules, pension rules, insurance. Read-only view with edit button.

- [ ] **Step 3: Agreement edit page**

`/admin/avtal/{id}/redigera` — forms for modifying rates per sub-entity.

- [ ] **Step 4: Build, commit**

---

## Task 14: Final Integration + Full Test Suite

- [ ] **Step 1: Run full build**

```bash
dotnet build RegionHR.sln
```
Expected: 0 errors.

- [ ] **Step 2: Run full test suite**

```bash
dotnet test RegionHR.sln
```
Expected: All existing 494+ tests pass + all new tests pass.

- [ ] **Step 3: Apply migration to dev database**

```bash
cd src/Web
dotnet ef database update --project ../Infrastructure/RegionHR.Infrastructure.csproj
```

- [ ] **Step 4: Smoke test**

Start the app and verify:
- `/admin/automation` shows 6 categories with 22 rules
- `/admin/avtal` shows 10 collective agreements
- `/admin/migration` shows empty dashboard ready for imports
- Existing functionality (login, dashboard, leave, payroll) still works

- [ ] **Step 5: Final commit**

```bash
git add -A
git commit -m "feat: complete Phase A — adoption layer

Domain event dispatch, automation framework (6 categories, 22 rules,
3 automation levels), migration engine (10 format adapters, 6-step wizard),
pluggable collective agreements (10 seeded, DB-backed rules engine).

21 new entities, 11 new routes, 3 new modules."
```

---

## Parallel Execution Guide

Tasks can be parallelized as follows:

```
Task 1: Domain Event Dispatch (prerequisite)
    ↓
    ├─→ Tasks 2-5: Automation Framework (sequential within track)
    ├─→ Tasks 9-11: Migration Engine (sequential within track)
    └─→ Tasks 6-8: Collective Agreements (sequential within track)

Task 12: API endpoints (after all three tracks)
Task 13: Agreement UI (after Task 8)
Task 14: Integration testing (after all tasks)
```

**Maximum parallelism: 3 agents** after Task 1 completes, each executing one track.

---

## Notes for Phase B/C Plans

This plan covers Phase A only. Subsequent plans:
- `2026-XX-XX-phase-b1-analytics.md` — Analytics & BI module
- `2026-XX-XX-phase-b2-compensation.md` — Compensation Suite
- `2026-XX-XX-phase-b3-benefits.md` — Benefits Engine
- `2026-XX-XX-phase-b4-vms.md` — VMS / Contingent Workforce
- `2026-XX-XX-phase-b5-wfm.md` — Advanced WFM
- `2026-XX-XX-phase-b6-talent.md` — Talent Marketplace
- `2026-XX-XX-phase-c-extensibility.md` — Platform / Extensibility

Each Phase B module depends on Phase A being complete (especially Automation Framework and Collective Agreements).
