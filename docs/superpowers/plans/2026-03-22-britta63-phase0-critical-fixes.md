# Phase 0: Critical Bugfixes & Documentation — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fix 10 critical bugs (tax calculation, holiday calendar, privacy, GDPR UX) and bring documentation in sync with actual code.

**Architecture:** Direct fixes to existing domain models, Razor pages, and shared kernel. Introduces `IClock` abstraction and `SvenskaHelgdagar` service. No new modules — surgical fixes to existing code.

**Tech Stack:** .NET 9, Blazor Server, MudBlazor, xUnit, PostgreSQL

**Spec:** `docs/superpowers/specs/2026-03-22-britta63-usability-overhaul-design.md`

---

### Task 1: Introduce IClock abstraction

**Files:**
- Create: `src/SharedKernel/Abstractions/IClock.cs`
- Create: `src/SharedKernel/Domain/SystemClock.cs`
- Modify: `src/Web/Program.cs` (add DI registration)
- Create: `tests/SharedKernel.Tests/SystemClockTests.cs`

- [ ] **Step 1: Create IClock interface**

```csharp
// src/SharedKernel/Abstractions/IClock.cs
namespace RegionHR.SharedKernel.Abstractions;

public interface IClock
{
    DateTime UtcNow { get; }
    DateOnly TodaySweden { get; }
}
```

- [ ] **Step 2: Create SystemClock implementation**

```csharp
// src/SharedKernel/Domain/SystemClock.cs
namespace RegionHR.SharedKernel.Domain;

public sealed class SystemClock : IClock
{
    private static readonly TimeZoneInfo _swedenTz =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Stockholm");

    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly TodaySweden =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _swedenTz));
}
```

- [ ] **Step 3: Write test**

```csharp
// tests/SharedKernel.Tests/SystemClockTests.cs
public class SystemClockTests
{
    [Fact]
    public void TodaySweden_ReturnsDateOnly()
    {
        var clock = new SystemClock();
        var today = clock.TodaySweden;
        Assert.True(today >= DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-2)));
        Assert.True(today <= DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2)));
    }
}
```

- [ ] **Step 4: Register in DI**

In `src/Web/Program.cs`, add:
```csharp
builder.Services.AddSingleton<IClock, SystemClock>();
```

- [ ] **Step 5: Build and run tests**

```bash
cd /home/jonas/OpenHR && dotnet build RegionHR.sln && dotnet test tests/SharedKernel.Tests/
```

- [ ] **Step 6: Commit**

```bash
git add src/SharedKernel/Abstractions/IClock.cs src/SharedKernel/Domain/SystemClock.cs src/Web/Program.cs tests/SharedKernel.Tests/SystemClockTests.cs
git commit -m "feat: add IClock abstraction for timezone-safe date handling"
```

---

### Task 2: Create Swedish Holiday Calendar service

**Files:**
- Create: `src/SharedKernel/Domain/SvenskaHelgdagar.cs`
- Modify: `src/SharedKernel/Domain/DateRange.cs` (add holiday-aware WorkDays)
- Create: `tests/SharedKernel.Tests/SvenskaHelgdagarTests.cs`
- Create: `tests/SharedKernel.Tests/DateRangeHolidayTests.cs`

- [ ] **Step 1: Write failing tests for holiday calendar**

```csharp
// tests/SharedKernel.Tests/SvenskaHelgdagarTests.cs
public class SvenskaHelgdagarTests
{
    [Theory]
    [InlineData(2026, 1, 1, true)]    // Nyårsdagen
    [InlineData(2026, 1, 6, true)]    // Trettondedag jul
    [InlineData(2026, 4, 3, true)]    // Långfredagen 2026
    [InlineData(2026, 4, 6, true)]    // Annandag påsk 2026
    [InlineData(2026, 5, 1, true)]    // 1 maj
    [InlineData(2026, 5, 14, true)]   // Kristi Himmelsfärdsdag 2026
    [InlineData(2026, 6, 6, true)]    // Sveriges nationaldag
    [InlineData(2026, 6, 19, true)]   // Midsommardagen 2026 (fredag closest to Jun 19-25)
    [InlineData(2026, 12, 25, true)]  // Juldagen
    [InlineData(2026, 12, 26, true)]  // Annandag jul
    [InlineData(2026, 3, 22, false)]  // Vanlig söndag — no, it's a Sunday actually. Let's use a weekday
    [InlineData(2026, 3, 23, false)]  // Vanlig måndag
    public void ArHelgdag_ReturnsExpected(int year, int month, int day, bool expected)
    {
        var date = new DateOnly(year, month, day);
        Assert.Equal(expected, SvenskaHelgdagar.ArHelgdag(date));
    }

    [Fact]
    public void HelgdagarForAr_Returns_AllHolidaysFor2026()
    {
        var holidays = SvenskaHelgdagar.HelgdagarForAr(2026);
        Assert.True(holidays.Count >= 13); // At least 13 public holidays
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

```bash
dotnet test tests/SharedKernel.Tests/ --filter "SvenskaHelgdagarTests"
```
Expected: Compilation error — `SvenskaHelgdagar` not defined.

- [ ] **Step 3: Implement SvenskaHelgdagar**

```csharp
// src/SharedKernel/Domain/SvenskaHelgdagar.cs
namespace RegionHR.SharedKernel.Domain;

public static class SvenskaHelgdagar
{
    public static bool ArHelgdag(DateOnly date) => HelgdagarForAr(date.Year).Contains(date);

    public static HashSet<DateOnly> HelgdagarForAr(int year)
    {
        var holidays = new HashSet<DateOnly>
        {
            new(year, 1, 1),   // Nyårsdagen
            new(year, 1, 6),   // Trettondedag jul
            new(year, 5, 1),   // 1 maj
            new(year, 6, 6),   // Nationaldag
            new(year, 12, 24), // Julafton (de facto)
            new(year, 12, 25), // Juldagen
            new(year, 12, 26), // Annandag jul
            new(year, 12, 31), // Nyårsafton (de facto)
        };

        // Påsk-baserade helgdagar (rörliga)
        var easter = CalculateEaster(year);
        holidays.Add(easter.AddDays(-2));  // Långfredagen
        holidays.Add(easter.AddDays(-1));  // Påskafton
        holidays.Add(easter);               // Påskdagen
        holidays.Add(easter.AddDays(1));   // Annandag påsk
        holidays.Add(easter.AddDays(39));  // Kristi himmelsfärdsdag
        holidays.Add(easter.AddDays(49));  // Pingstdagen

        // Midsommar (fredag mellan 19-25 juni)
        var jun19 = new DateOnly(year, 6, 19);
        var midsommarAfton = jun19;
        while (midsommarAfton.DayOfWeek != DayOfWeek.Friday)
            midsommarAfton = midsommarAfton.AddDays(1);
        holidays.Add(midsommarAfton);          // Midsommarafton
        holidays.Add(midsommarAfton.AddDays(1)); // Midsommardagen

        // Alla helgons dag (lördag 31 okt - 6 nov)
        var oct31 = new DateOnly(year, 10, 31);
        var allaHelgon = oct31;
        while (allaHelgon.DayOfWeek != DayOfWeek.Saturday)
            allaHelgon = allaHelgon.AddDays(1);
        holidays.Add(allaHelgon);

        return holidays;
    }

    private static DateOnly CalculateEaster(int year)
    {
        // Meeus/Jones/Butcher algorithm
        int a = year % 19, b = year / 100, c = year % 100;
        int d = b / 4, e = b % 4, f = (b + 8) / 25;
        int g = (b - f + 1) / 3, h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4, k = c % 4, l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day = (h + l - 7 * m + 114) % 31 + 1;
        return new DateOnly(year, month, day);
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

```bash
dotnet test tests/SharedKernel.Tests/ --filter "SvenskaHelgdagarTests"
```

- [ ] **Step 5: Add holiday-aware WorkDays to DateRange**

Modify `src/SharedKernel/Domain/DateRange.cs` — change `WorkDays` property:

```csharp
/// <summary>Antal arbetsdagar (mån-fre exkl. svenska helgdagar) i perioden.</summary>
public int? WorkDays
{
    get
    {
        if (!End.HasValue) return null;
        var count = 0;
        var holidays = SvenskaHelgdagar.HelgdagarForAr(Start.Year);
        if (End.Value.Year != Start.Year)
            holidays.UnionWith(SvenskaHelgdagar.HelgdagarForAr(End.Value.Year));
        for (var d = Start; d <= End.Value; d = d.AddDays(1))
        {
            if (d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) && !holidays.Contains(d))
                count++;
        }
        return count;
    }
}
```

- [ ] **Step 6: Write DateRange holiday test**

```csharp
// tests/SharedKernel.Tests/DateRangeHolidayTests.cs
public class DateRangeHolidayTests
{
    [Fact]
    public void WorkDays_ExcludesSwedishHolidays()
    {
        // Dec 22 (Mon) to Dec 26 (Fri) 2025 — includes julafton(24), juldagen(25), annandag(26)
        var range = new DateRange(new DateOnly(2025, 12, 22), new DateOnly(2025, 12, 26));
        // Mon 22 = workday, Tue 23 = workday, Wed 24 = holiday, Thu 25 = holiday, Fri 26 = holiday
        Assert.Equal(2, range.WorkDays);
    }

    [Fact]
    public void WorkDays_MidsommarWeek()
    {
        // Midsommar 2026: Jun 19 (Fri) is midsommarafton
        var range = new DateRange(new DateOnly(2026, 6, 15), new DateOnly(2026, 6, 19));
        // Mon 15, Tue 16, Wed 17, Thu 18 = workdays, Fri 19 = midsommarafton (holiday)
        Assert.Equal(4, range.WorkDays);
    }
}
```

- [ ] **Step 7: Run all SharedKernel tests**

```bash
dotnet test tests/SharedKernel.Tests/
```

- [ ] **Step 8: Commit**

```bash
git add src/SharedKernel/Domain/SvenskaHelgdagar.cs src/SharedKernel/Domain/DateRange.cs tests/SharedKernel.Tests/
git commit -m "feat: add Swedish holiday calendar and holiday-aware WorkDays"
```

---

### Task 3: Fix LAS timezone issue

**Files:**
- Modify: `src/Modules/LAS/Domain/LASAccumulation.cs` (inject IClock or accept DateOnly parameter)
- Modify: `tests/LAS.Tests/` (update tests for new signature)

- [ ] **Step 1: Change LaggTillPeriod to accept referensDatum parameter**

In `src/Modules/LAS/Domain/LASAccumulation.cs`, line 86, change:
```csharp
// FROM:
Omberakna(DateOnly.FromDateTime(DateTime.Today));
// TO:
Omberakna(referensDatum ?? DateOnly.FromDateTime(DateTime.UtcNow));
```

And update method signature:
```csharp
public void LaggTillPeriod(DateOnly startDatum, DateOnly slutDatum, string? anstallningsId = null, DateOnly? referensDatum = null)
```

- [ ] **Step 2: Update existing LAS tests to pass referensDatum explicitly**

Search for `LaggTillPeriod` calls in `tests/LAS.Tests/` and ensure they pass explicit dates for deterministic testing.

- [ ] **Step 3: Run LAS tests**

```bash
dotnet test tests/LAS.Tests/
```

- [ ] **Step 4: Commit**

```bash
git add src/Modules/LAS/Domain/LASAccumulation.cs tests/LAS.Tests/
git commit -m "fix: make LAS accumulation timezone-safe with explicit reference date"
```

---

### Task 4: Fix hardcoded 30% tax in NyKorning.razor

**Files:**
- Modify: `src/Web/Components/Pages/Lon/NyKorning.razor` (replace inline calculation with PayrollCalculationEngine)

- [ ] **Step 1: Read PayrollCalculationEngine to understand the CalculateAsync API**

Read `src/Modules/Payroll/Engine/PayrollCalculationEngine.cs` fully to understand the method signature and required input types.

- [ ] **Step 2: Inject PayrollBatchService instead of inline calculation**

Replace the manual loop in NyKorning.razor (lines 71-90) with a call to `PayrollBatchService.KorLoneberakning()`. The batch service already handles proper tax calculation via `PayrollCalculationEngine`.

In `NyKorning.razor` `@code` section, replace the inline calculation with:
```csharp
@inject IPayrollBatchService PayrollBatchService

// In SkapaKorning():
// Replace entire foreach loop with:
await PayrollBatchService.KorLoneberakning(run.Id, _year, _month, default);
```

If `IPayrollBatchService` is not registered, register it in `Program.cs`:
```csharp
builder.Services.AddScoped<IPayrollBatchService, PayrollBatchService>();
```

- [ ] **Step 3: Build and verify**

```bash
dotnet build src/Web/RegionHR.Web.csproj
```

- [ ] **Step 4: Commit**

```bash
git add src/Web/Components/Pages/Lon/NyKorning.razor src/Web/Program.cs
git commit -m "fix: use PayrollCalculationEngine instead of hardcoded 30% tax rate"
```

---

### Task 5: Fix silent payroll batch errors

**Files:**
- Modify: `src/Modules/Payroll/Services/PayrollBatchService.cs`
- Modify: `src/Modules/Payroll/Domain/PayrollRun.cs` (add error tracking)
- Create: `tests/Payroll.Tests/PayrollBatchErrorHandlingTests.cs`

- [ ] **Step 1: Add error collection to PayrollRun domain**

Add to `PayrollRun.cs`:
```csharp
private readonly List<string> _berakningsFel = [];
public IReadOnlyList<string> BerakningsFel => _berakningsFel.AsReadOnly();
public bool HarFel => _berakningsFel.Count > 0;

public void LaggTillFel(EmployeeId anstallId, string felmeddelande)
{
    _berakningsFel.Add($"{anstallId}: {felmeddelande}");
}
```

- [ ] **Step 2: Update PayrollBatchService to collect errors**

In `PayrollBatchService.cs`, replace the silent catch (lines 73-79):
```csharp
catch (Exception ex)
{
    run.LaggTillFel(employee.Id, ex.Message);
    _logger.LogError(ex, "Löneberäkningsfel för {EmployeeId}", employee.Id);
}
```

After the loop, before `MarkeraSomBeraknad()`:
```csharp
if (run.HarFel)
{
    // Don't mark as calculated — mark as has errors
    run.MarkeraSomBeraknadMedFel();
}
else
{
    run.MarkeraSomBeraknad();
}
```

- [ ] **Step 3: Add MarkeraSomBeraknadMedFel to PayrollRun**

```csharp
public void MarkeraSomBeraknadMedFel()
{
    Status = PayrollRunStatus.BeraknadMedFel;
}
```

Add `BeraknadMedFel` to the PayrollRunStatus enum if it doesn't exist.

- [ ] **Step 4: Write test**

```csharp
[Fact]
public void PayrollRun_WithErrors_MarkedAsBeraknadMedFel()
{
    var run = PayrollRun.Skapa(2026, 3, "Test");
    run.LaggTillFel(EmployeeId.From(Guid.NewGuid()), "Skattetabell saknas");
    Assert.True(run.HarFel);
    Assert.Single(run.BerakningsFel);
}
```

- [ ] **Step 5: Run tests**

```bash
dotnet test tests/Payroll.Tests/
```

- [ ] **Step 6: Commit**

```bash
git add src/Modules/Payroll/ tests/Payroll.Tests/
git commit -m "fix: collect payroll batch errors instead of silently continuing"
```

---

### Task 6: Fix RehabCase milestone calculation

**Files:**
- Modify: `src/Modules/HalsoSAM/Domain/RehabCase.cs`
- Modify: `tests/HalsoSAM.Tests/`

- [ ] **Step 1: Add StartaRehab method**

Add to `RehabCase.cs`:
```csharp
public void StartaRehab(DateTime rehabStartDatum)
{
    if (Status != RehabStatus.Signal)
        throw new InvalidOperationException("Rehab kan bara startas från Signal-status");

    Status = RehabStatus.AktivRehab;
    Uppfoljning14Dagar = rehabStartDatum.AddDays(14);
    Uppfoljning90Dagar = rehabStartDatum.AddDays(90);
    Uppfoljning180Dagar = rehabStartDatum.AddDays(180);
    Uppfoljning365Dagar = rehabStartDatum.AddDays(365);
}
```

- [ ] **Step 2: Update Skapa to NOT set milestones**

In `Skapa()`, remove the milestone assignments — set them to null or keep as creation-based defaults with clear comment that `StartaRehab()` recalculates them.

- [ ] **Step 3: Write test**

```csharp
[Fact]
public void StartaRehab_RecalculatesMilestones_FromRehabStartDate()
{
    var rehab = RehabCase.Skapa(EmployeeId.From(Guid.NewGuid()), RehabTrigger.Langtidssjuk);
    var startDate = new DateTime(2026, 4, 1);
    rehab.StartaRehab(startDate);

    Assert.Equal(new DateTime(2026, 4, 15), rehab.Uppfoljning14Dagar);
    Assert.Equal(new DateTime(2026, 6, 30), rehab.Uppfoljning90Dagar);
}
```

- [ ] **Step 4: Run tests**

```bash
dotnet test tests/HalsoSAM.Tests/
```

- [ ] **Step 5: Commit**

```bash
git add src/Modules/HalsoSAM/ tests/HalsoSAM.Tests/
git commit -m "fix: calculate rehab milestones from rehab start date, not creation date"
```

---

### Task 7: Fix TotalRewards privacy fallback

**Files:**
- Modify: `src/Web/Components/Pages/MinSida/TotalRewards.razor`

- [ ] **Step 1: Remove fallback to first employee**

In `TotalRewards.razor`, replace lines 119-130:
```csharp
// FROM:
Guid anstallId;
if (employeeIdClaim != null && Guid.TryParse(employeeIdClaim, out var parsedId))
{
    anstallId = parsedId;
}
else
{
    var first = await db.Employees.FirstOrDefaultAsync();
    if (first == null) { _error = "Ingen anstalld hittad"; return; }
    anstallId = first.Id.Value;
}

// TO:
if (employeeIdClaim == null || !Guid.TryParse(employeeIdClaim, out var anstallId))
{
    _error = "Du måste vara inloggad för att se din kompensationsöversikt. Logga in på nytt.";
    return;
}
```

- [ ] **Step 2: Fix typos in same file**

Replace `"Forsakringar"` → `"Försäkringar"`, `"hamta"` → `"hämta"`, `"Oversikt"` → `"Översikt"` throughout the file.

- [ ] **Step 3: Build and verify**

```bash
dotnet build src/Web/RegionHR.Web.csproj
```

- [ ] **Step 4: Commit**

```bash
git add src/Web/Components/Pages/MinSida/TotalRewards.razor
git commit -m "fix: remove privacy-violating fallback to first employee in TotalRewards"
```

---

### Task 8: Fix GDPR page — replace GUID input with employee search

**Files:**
- Modify: `src/Web/Components/Pages/GDPR/Index.razor`

- [ ] **Step 1: Replace GUID textfield with MudAutocomplete**

In `GDPR/Index.razor`, replace line 94:
```razor
@* FROM: *@
<MudTextField @bind-Value="_registerutdragAnstallId" Label="Anställd-ID (GUID)" Variant="Variant.Outlined" Class="mb-3" />

@* TO: *@
<MudAutocomplete T="string" Label="Sök anställd (namn eller personnummer)"
    @bind-Value="_registerutdragAnstallId"
    SearchFunc="@SokAnstallda"
    Variant="Variant.Outlined" Class="mb-3"
    AdornmentIcon="@Icons.Material.Filled.Search"
    ResetValueOnEmptyText="true" />
```

- [ ] **Step 2: Add search function to code block**

```csharp
private async Task<IEnumerable<string>> SokAnstallda(string value, CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
        return [];

    await using var db = await DbFactory.CreateDbContextAsync();
    var matches = await db.Employees
        .Where(e => EF.Functions.ILike(e.Fornamn + " " + e.Efternamn, $"%{value}%")
                  || EF.Functions.ILike(e.Personnummer.Value, $"%{value}%"))
        .Take(10)
        .Select(e => $"{e.Fornamn} {e.Efternamn} ({e.Personnummer.Value}) [{e.Id.Value}]")
        .ToListAsync(ct);
    return matches;
}
```

- [ ] **Step 3: Update HamtaRegisterutdrag to parse new format**

```csharp
private async Task HamtaRegisterutdrag()
{
    // Extract GUID from format "Anna Svensson (19900101-1234) [guid-here]"
    var match = System.Text.RegularExpressions.Regex.Match(_registerutdragAnstallId, @"\[([0-9a-f-]+)\]$");
    if (!match.Success || !Guid.TryParse(match.Groups[1].Value, out var anstallId))
    {
        _registerutdragResult = "Välj en anställd från listan.";
        return;
    }
    // ... rest of existing logic using anstallId
```

- [ ] **Step 4: Build and verify**

```bash
dotnet build src/Web/RegionHR.Web.csproj
```

- [ ] **Step 5: Commit**

```bash
git add src/Web/Components/Pages/GDPR/Index.razor
git commit -m "fix: replace manual GUID input with employee search in GDPR page"
```

---

### Task 9: Move IBB constants to configuration

**Files:**
- Modify: `src/Modules/Payroll/Engine/PayrollCalculationEngine.cs`
- Create: `src/Modules/Configuration/Domain/SystemConfiguration.cs` (if not exists)
- Modify: seed data to include IBB values

- [ ] **Step 1: Check if Configuration module has a key-value entity**

Read `src/Modules/Configuration/Domain/` to see existing entities.

- [ ] **Step 2: Add IBB configuration entries to seed data**

Add to `SeedData.cs`:
```csharp
// System configuration for annually-updated values
db.Add(new SystemSetting("IBB_2025", "80600", "Inkomstbasbelopp 2025"));
db.Add(new SystemSetting("IBB_2026", "83400", "Inkomstbasbelopp 2026"));
db.Add(new SystemSetting("PBB_2025", "58800", "Prisbasbelopp 2025"));
db.Add(new SystemSetting("PBB_2026", "59200", "Prisbasbelopp 2026"));
```

- [ ] **Step 3: Update PayrollCalculationEngine to read from config**

Replace hardcoded constants with injected configuration lookup. Keep constants as fallback with `[Obsolete]` marker.

- [ ] **Step 4: Build and run payroll tests**

```bash
dotnet test tests/Payroll.Tests/
```

- [ ] **Step 5: Commit**

```bash
git add src/Modules/Payroll/ src/Modules/Configuration/ src/Infrastructure/Persistence/SeedData.cs
git commit -m "feat: move IBB/PBB constants to configurable settings"
```

---

### Task 10: Update CLAUDE.md to match actual codebase

**Files:**
- Modify: `CLAUDE.md`

- [ ] **Step 1: Update all counts**

- 25 moduler → 38 moduler
- 494 tester → ~1 123 tester
- 96 Blazor-sidor → 178 sidor
- 50+ i18n-nycklar → 329 nycklar (sv + en)
- 94 rutter → ~178 rutter
- 75 infrastrukturfiler → count and update

- [ ] **Step 2: Add all missing module descriptions**

Add Fas A-D modules (Agreements, Analytics, Automation, Compensation, Communication, Configuration, Helpdesk, Insurance, Knowledge, Migration, Platform, PolicyManagement, Pulse, VMS, Wellness) to the module structure section.

- [ ] **Step 3: Update conventions section**

Note scaffolded features explicitly: "Automation rules exist as seed data but execution engine is interface-only."

- [ ] **Step 4: Commit**

```bash
git add CLAUDE.md
git commit -m "docs: update CLAUDE.md to reflect actual codebase (38 modules, 1123 tests, 178 pages)"
```

---

### Task 11: Update README.md to fix inaccuracies

**Files:**
- Modify: `README.md`

- [ ] **Step 1: Fix bottom navigation and swipe claims**

Change:
```
- **Deep PWA** — offline data caching, push-notiser, background sync, bottom navigation, swipe-gester
```
To:
```
- **Deep PWA** — offline data caching, push-notiser, background sync (bottom navigation och swipe-gester: CSS förberett, ej aktiverat i UI)
```

- [ ] **Step 2: Clarify QuestPDF**

Change "QuestPDF-redo" to "PDF via PdfSharpCore (lönespecifikationer). Övriga mallar: textbaserad platshållare."

- [ ] **Step 3: Mark scaffolded features**

In the 2.0 Expansion section, add markers like "(entity + seed, exekveringsmotor ej implementerad)" for the 8 scaffolded features.

- [ ] **Step 4: Commit**

```bash
git add README.md
git commit -m "docs: correct README.md claims about bottom nav, swipe, QuestPDF, scaffolded features"
```
