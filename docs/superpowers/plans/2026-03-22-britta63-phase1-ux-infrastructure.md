# Phase 1: Global UX Infrastructure — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Create 8 reusable UX components/services that form the foundation for all subsequent usability improvements across OpenHR's 178 pages.

**Architecture:** New shared Blazor components in `src/Web/Components/Shared/` following existing Ohr* pattern (MudBlazor-based, accessible, i18n-ready). Services in `src/Web/Services/`. Glossary data as embedded JSON resource. All components follow existing namespace conventions.

**Tech Stack:** .NET 9, Blazor Server, MudBlazor 9.1, IStringLocalizer, xUnit

**Spec:** `docs/superpowers/specs/2026-03-22-britta63-usability-overhaul-design.md` (section 5)

---

## File Structure

| File | Responsibility |
|------|---------------|
| `src/Web/Services/GlossaryService.cs` | Loads and serves glossary terms from JSON |
| `src/Web/wwwroot/data/glossary-sv.json` | Swedish HR term definitions (~100 terms) |
| `src/Web/Components/Shared/OhrTerm.razor` | Inline tooltip for HR jargon terms |
| `src/Web/Components/Shared/OhrWizard.razor` | Step-by-step wizard with progress and labels |
| `src/Web/Services/ErrorDisplayService.cs` | User-friendly error message mapper |
| `src/Web/Components/Shared/OhrHelpButton.razor` | Contextual help drawer per page |
| `src/Web/Components/Shared/OhrStatusLegend.razor` | Color legend for status chips |
| `src/Web/Components/Shared/OhrConfirmDialog.razor` | Confirmation dialog for destructive actions |
| `src/Web/Components/Shared/OhrOnboarding.razor` | First-login guided tour |
| `src/Web/Components/Shared/OhrBreadcrumb.razor` | User-friendly breadcrumb navigation |
| `src/Web/Components/Pages/Hjalp/Ordlista.razor` | Full glossary page at `/hjalp/ordlista` |

---

### Task 1: GlossaryService + OhrTerm tooltip component

**Files:**
- Create: `src/Web/wwwroot/data/glossary-sv.json`
- Create: `src/Web/Services/GlossaryService.cs`
- Create: `src/Web/Components/Shared/OhrTerm.razor`
- Create: `src/Web/Components/Pages/Hjalp/Ordlista.razor`
- Modify: `src/Web/Program.cs` (register GlossaryService)

- [ ] **Step 1: Create glossary JSON with ~50 initial terms**

Create `src/Web/wwwroot/data/glossary-sv.json`:
```json
{
  "LAS": "Lagen om anställningsskydd — reglerar hur länge visstidsanställningar får pågå (max 365 dagar per 5 år) och när de automatiskt övergår till tillsvidare.",
  "MBL": "Medbestämmandelagen — kräver att arbetsgivaren förhandlar med facket innan större beslut som omorganisation eller uppsägning.",
  "HälsoSAM": "Hälso- och sjukvårdssamverkan — processen för att stödja medarbetare tillbaka till arbete efter sjukfrånvaro, med uppföljning vid dag 14, 90, 180 och 365.",
  "VAB": "Vård av barn — ledighet när ditt barn är sjukt. Du får ersättning från Försäkringskassan (max 120 dagar per barn och år).",
  "OB-tillägg": "Obekväm arbetstid-tillägg — extra ersättning för arbete kvällar (46 kr/h), nätter (113 kr/h), helger (55 kr/h) och storhelger (130 kr/h).",
  "SAVA": "Särskild visstidsanställning — en tidsbegränsad anställningsform med LAS-regler om max antal dagar.",
  "AG-avgifter": "Arbetsgivaravgifter — skatt som arbetsgivaren betalar utöver lönen (31,42% av bruttolönen). Går till pension, sjukförsäkring och arbetslöshet.",
  "Bruttolön": "Lönen före skatt — det totala beloppet innan kommunalskatt och eventuell statlig skatt dras av.",
  "Nettolön": "Lönen efter skatt — det du faktiskt får utbetalt till ditt bankkonto.",
  "Sysselsättningsgrad": "Hur stor del av en heltidstjänst du arbetar, i procent. 100% = heltid, 75% = deltid tre fjärdedelar.",
  "Befattning": "Din jobbtitel och yrkesroll, till exempel Sjuksköterska, Undersköterska eller Administratör.",
  "Skattetabell": "En tabell som bestämmer hur mycket skatt som dras varje månad. Beror på vilken kommun du bor i.",
  "Lönearter": "Kodade poster som beskriver olika delar av din lön: grundlön, tillägg, avdrag, förmåner.",
  "Traktamente": "Ersättning för mat och logi vid tjänsteresa. Beloppet bestäms av Skatteverket (260 kr/heldag inrikes 2026).",
  "Karensavdrag": "Avdrag på 20% av en genomsnittlig veckoersättning som görs första sjukdagen.",
  "Tillsvidare": "Fast anställning utan slutdatum — den vanligaste och tryggaste anställningsformen.",
  "Visstid": "Tidsbegränsad anställning med ett bestämt slutdatum.",
  "Kollektivavtal": "Avtal mellan arbetsgivare och fackförbund som reglerar löner, arbetstider, semester och andra villkor.",
  "ATL": "Arbetstidslagen — reglerar max arbetstid (48h/vecka), vila mellan pass (minst 11h) och veckovila (minst 36h sammanhängande).",
  "FK": "Försäkringskassan — myndigheten som betalar ut sjukpenning, föräldrapenning och andra socialförsäkringar.",
  "IBB": "Inkomstbasbelopp — ett belopp som Skatteverket fastställer varje år och som används för att beräkna pensioner.",
  "PBB": "Prisbasbelopp — ett belopp som baseras på konsumentprisindex och påverkar socialförsäkringsförmåner.",
  "Föräldraledighet": "Rätten att vara ledig från arbetet för att ta hand om ditt barn. 480 dagar per barn, varav 390 dagar på sjukpenningnivå.",
  "Semester": "Betald ledighet — du har rätt till minst 25 semesterdagar per år enligt semesterlagen.",
  "Sjuklön": "Lön du får från arbetsgivaren när du är sjuk (dag 2-14, 80% av lönen). Från dag 15 tar Försäkringskassan över.",
  "Löneöversyn": "Årlig process där arbetsgivaren och facket förhandlar om löneökningar för alla anställda.",
  "Lönekartering": "Kartläggning av löner för att identifiera osakliga löneskillnader, enligt diskrimineringslagen.",
  "GDPR": "Dataskyddsförordningen — EU-lag som skyddar dina personuppgifter. Du har rätt att se, ändra och radera data om dig.",
  "Registerutdrag": "En sammanställning av alla personuppgifter som organisationen har om dig. Du har rätt att begära detta enligt GDPR.",
  "KLR": "Klassificering av löner — standardformat för att rapportera lönestatistik till SCB.",
  "SCB": "Statistiska centralbyrån — myndigheten som samlar in och publicerar statistik om Sverige.",
  "TGL": "Tjänstegrupplivförsäkring — försäkring som ger ersättning till familjen om du skulle avlida.",
  "AGS": "Avtalsgruppsjukförsäkring — kompletterande sjukförsäkring utöver Försäkringskassans ersättning.",
  "Friskvårdsbidrag": "Bidrag från arbetsgivaren för att du ska kunna träna och må bra. Max 5 000 kr per år för motion och hälsa.",
  "SLA": "Servicenivåavtal — anger hur snabbt ett ärende ska besvaras och lösas (till exempel: svar inom 4 timmar).",
  "KPI": "Nyckeltal — mätvärden som visar hur bra organisationen presterar (till exempel: sjukfrånvaro i procent).",
  "F-skatt": "Godkännande som visar att en person eller företag själv betalar sin skatt. Krävs för inhyrda konsulter.",
  "Ramavtal": "Långsiktigt avtal med en leverantör som reglerar priser och villkor för en bestämd period.",
  "Onboarding": "Introduktionsprocessen för nya medarbetare — allt som behöver göras innan och under de första veckorna.",
  "Offboarding": "Avslutningsprocessen när en medarbetare slutar — returnera utrustning, avsluta konton, slutlön.",
  "Pulsundersökning": "Kort enkät som skickas ut regelbundet för att mäta medarbetarnas trivsel och arbetsmiljö.",
  "360-feedback": "Återkoppling från flera håll — kollegor, chef, underställda — för att ge en helhetsbild av en medarbetares prestation.",
  "Gap-analys": "Jämförelse mellan vilka kompetenser som behövs och vilka som finns — visar var utbildning behövs.",
  "Delegation": "Att tillfälligt ge någon annan rätt att godkänna eller utföra uppgifter i ditt ställe.",
  "Certifiering": "Formellt bevis på att du har en viss kompetens eller utbildning (till exempel: sjukvårdscertifikat).",
  "Successionsplanering": "Att identifiera och förbereda medarbetare som kan ta över nyckelroller i framtiden."
}
```

- [ ] **Step 2: Create GlossaryService**

Create `src/Web/Services/GlossaryService.cs`:
```csharp
using System.Text.Json;

namespace RegionHR.Web.Services;

public sealed class GlossaryService
{
    private Dictionary<string, string> _terms = new(StringComparer.OrdinalIgnoreCase);
    private bool _loaded;

    public async Task EnsureLoadedAsync(HttpClient http)
    {
        if (_loaded) return;
        try
        {
            var json = await http.GetStringAsync("data/glossary-sv.json");
            _terms = JsonSerializer.Deserialize<Dictionary<string, string>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? new();
        }
        catch { _terms = new(); }
        _loaded = true;
    }

    public string? GetDefinition(string term)
    {
        return _terms.TryGetValue(term, out var def) ? def : null;
    }

    public IReadOnlyDictionary<string, string> GetAllTerms() => _terms;
}
```

- [ ] **Step 3: Register GlossaryService in Program.cs**

Add to `src/Web/Program.cs` after other `AddScoped` registrations:
```csharp
builder.Services.AddSingleton<GlossaryService>();
```

- [ ] **Step 4: Create OhrTerm component**

Create `src/Web/Components/Shared/OhrTerm.razor`:
```razor
@namespace RegionHR.Web.Components.Shared
@inject GlossaryService Glossary
@inject HttpClient? Http

<MudTooltip Text="@_definition" Arrow="true" Placement="Placement.Top"
            Style="max-width: 400px;">
    <span style="border-bottom: 1px dotted var(--mud-palette-primary); cursor: help;"
          aria-describedby="ohr-term-@_id">
        @(DisplayText ?? Term)
    </span>
</MudTooltip>

@code {
    private string _id = Guid.NewGuid().ToString("N")[..8];
    private string _definition = "";

    /// <summary>The glossary key to look up (e.g. "LAS", "VAB").</summary>
    [Parameter, EditorRequired] public string Term { get; set; } = "";

    /// <summary>Optional display text (defaults to Term if not set).</summary>
    [Parameter] public string? DisplayText { get; set; }

    protected override async Task OnInitializedAsync()
    {
        if (Http is not null)
            await Glossary.EnsureLoadedAsync(Http);
        _definition = Glossary.GetDefinition(Term) ?? $"{Term} — ingen förklaring tillgänglig";
    }
}
```

- [ ] **Step 5: Register HttpClient for Blazor Server**

In `src/Web/Program.cs`, add (if not already present):
```csharp
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5076/")
});
```
Note: Check if HttpClient is already registered. If so, skip this step.

- [ ] **Step 6: Create glossary page**

Create `src/Web/Components/Pages/Hjalp/Ordlista.razor`:
```razor
@page "/hjalp/ordlista"
@namespace RegionHR.Web.Components.Pages.Hjalp
@layout RegionHR.Web.Components.Layout.AdminLayout
@inject GlossaryService Glossary
@inject HttpClient? Http

<PageTitle>OpenHR - Ordlista</PageTitle>
<MudText Typo="Typo.h4" Class="mb-2">Ordlista</MudText>
<MudText Typo="Typo.body1" Class="mb-4" Style="color: var(--mud-palette-text-secondary);">
    Förklaringar av termer och förkortningar som används i systemet.
</MudText>

<MudTextField @bind-Value="_search" Label="Sök term..." Variant="Variant.Outlined"
              Adornment="Adornment.Start" AdornmentIcon="@Icons.Material.Filled.Search"
              Immediate="true" Class="mb-4" />

@foreach (var term in _filtered)
{
    <MudPaper Class="pa-4 mb-3" Elevation="1">
        <MudText Typo="Typo.subtitle1" Style="font-weight: 700;">@term.Key</MudText>
        <MudText Typo="Typo.body1" Class="mt-1">@term.Value</MudText>
    </MudPaper>
}

@if (!_filtered.Any())
{
    <OhrEmptyState Title="Ingen matchande term" Description="Prova att söka med en annan term." />
}

@code {
    private string _search = "";
    private IEnumerable<KeyValuePair<string, string>> _filtered =>
        Glossary.GetAllTerms()
            .Where(t => string.IsNullOrEmpty(_search)
                || t.Key.Contains(_search, StringComparison.OrdinalIgnoreCase)
                || t.Value.Contains(_search, StringComparison.OrdinalIgnoreCase))
            .OrderBy(t => t.Key);

    protected override async Task OnInitializedAsync()
    {
        if (Http is not null)
            await Glossary.EnsureLoadedAsync(Http);
    }
}
```

- [ ] **Step 7: Build and verify**

```bash
dotnet build RegionHR.sln
```

- [ ] **Step 8: Commit**

```bash
git add src/Web/wwwroot/data/glossary-sv.json src/Web/Services/GlossaryService.cs src/Web/Components/Shared/OhrTerm.razor src/Web/Components/Pages/Hjalp/Ordlista.razor src/Web/Program.cs
git commit -m "feat: add glossary service, OhrTerm tooltip, and /hjalp/ordlista page

GlossaryService loads ~50 HR terms from JSON. OhrTerm renders inline
tooltips for jargon. Full glossary searchable at /hjalp/ordlista.

Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>"
```

---

### Task 2: OhrWizard step-by-step component

**Files:**
- Create: `src/Web/Components/Shared/OhrWizard.razor`

- [ ] **Step 1: Create OhrWizard component**

Create `src/Web/Components/Shared/OhrWizard.razor`:
```razor
@namespace RegionHR.Web.Components.Shared

<div class="ohr-wizard" role="form" aria-label="@(Steps.ElementAtOrDefault(CurrentStep) ?? "Wizard")">
    @if (Steps.Count > 1)
    {
        <MudStepper @bind-ActiveIndex="CurrentStep" Linear="true" Variant="Variant.Text"
                    Color="Color.Primary" Class="mb-4"
                    PreventStepChange="@(new Func<StepChangeDirection, bool>(_ => !AllowNavigation))">
            @for (var i = 0; i < Steps.Count; i++)
            {
                <MudStep Title="@Steps[i]" />
            }
        </MudStepper>
    }

    <MudText Typo="Typo.h5" Class="mb-1" Style="font-weight: 700;">
        Steg @(CurrentStep + 1) av @Steps.Count: @Steps.ElementAtOrDefault(CurrentStep)
    </MudText>

    @if (CurrentStep < Steps.Count - 1)
    {
        <MudText Typo="Typo.body2" Class="mb-4" Style="color: var(--mud-palette-text-secondary);">
            Nästa: @Steps.ElementAtOrDefault(CurrentStep + 1)
        </MudText>
    }

    <div class="ohr-wizard__content">
        @ChildContent
    </div>

    <div class="d-flex justify-space-between mt-4">
        @if (CurrentStep > 0)
        {
            <MudButton StartIcon="@Icons.Material.Filled.ArrowBack"
                       Variant="Variant.Text" Color="Color.Primary"
                       OnClick="PreviousStep" Style="min-height: 48px;">
                Tillbaka
            </MudButton>
        }
        else
        {
            <div></div>
        }

        @if (CurrentStep < Steps.Count - 1)
        {
            <MudButton EndIcon="@Icons.Material.Filled.ArrowForward"
                       Variant="Variant.Filled" Color="Color.Primary"
                       OnClick="NextStep" Disabled="!CanAdvance"
                       Style="min-height: 48px;">
                Nästa
            </MudButton>
        }
    </div>
</div>

@code {
    [Parameter, EditorRequired] public List<string> Steps { get; set; } = [];
    [Parameter] public int CurrentStep { get; set; }
    [Parameter] public EventCallback<int> CurrentStepChanged { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public bool CanAdvance { get; set; } = true;
    [Parameter] public bool AllowNavigation { get; set; } = true;
    [Parameter] public EventCallback OnCompleted { get; set; }

    private async Task NextStep()
    {
        if (CurrentStep < Steps.Count - 1)
        {
            CurrentStep++;
            await CurrentStepChanged.InvokeAsync(CurrentStep);
        }
        else
        {
            await OnCompleted.InvokeAsync();
        }
    }

    private async Task PreviousStep()
    {
        if (CurrentStep > 0)
        {
            CurrentStep--;
            await CurrentStepChanged.InvokeAsync(CurrentStep);
        }
    }
}
```

- [ ] **Step 2: Build and verify**

```bash
dotnet build src/Web/RegionHR.Web.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/Web/Components/Shared/OhrWizard.razor
git commit -m "feat: add OhrWizard step-by-step component with progress indication

Shows step X of Y with labels, next step preview, back/forward navigation,
and CanAdvance gating. Built on MudStepper for visual progress.

Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>"
```

---

### Task 3: ErrorDisplayService for user-friendly errors

**Files:**
- Create: `src/Web/Services/ErrorDisplayService.cs`

- [ ] **Step 1: Create ErrorDisplayService**

Create `src/Web/Services/ErrorDisplayService.cs`:
```csharp
using Microsoft.Extensions.Logging;

namespace RegionHR.Web.Services;

/// <summary>
/// Maps technical exceptions to user-friendly Swedish error messages.
/// Logs the technical details server-side. Never exposes ex.Message to UI.
/// </summary>
public sealed class ErrorDisplayService
{
    private readonly ILogger<ErrorDisplayService> _logger;

    public ErrorDisplayService(ILogger<ErrorDisplayService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Returns a user-friendly error message and logs the exception.
    /// </summary>
    public string HandleError(Exception ex, string operation)
    {
        _logger.LogError(ex, "Error during {Operation}", operation);

        return ex switch
        {
            TimeoutException => $"Systemet svarar långsamt. Försök igen om en stund.",
            InvalidOperationException ioe when ioe.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
                => "Posten finns redan i systemet. Kontrollera uppgifterna.",
            InvalidOperationException ioe when ioe.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                => "Posten kunde inte hittas. Den kan ha tagits bort.",
            UnauthorizedAccessException => "Du har inte behörighet för denna åtgärd. Kontakta din administratör.",
            ArgumentException => "Felaktiga uppgifter. Kontrollera formuläret och försök igen.",
            _ => $"Något gick fel. Försök igen eller kontakta support."
        };
    }

    /// <summary>
    /// Convenience: wraps an async operation with error handling.
    /// Returns (success, errorMessage).
    /// </summary>
    public async Task<(bool Success, string? Error)> TryAsync(Func<Task> action, string operation)
    {
        try
        {
            await action();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, HandleError(ex, operation));
        }
    }
}
```

- [ ] **Step 2: Register in Program.cs**

Add to `src/Web/Program.cs`:
```csharp
builder.Services.AddScoped<ErrorDisplayService>();
```

- [ ] **Step 3: Build**

```bash
dotnet build src/Web/RegionHR.Web.csproj
```

- [ ] **Step 4: Commit**

```bash
git add src/Web/Services/ErrorDisplayService.cs src/Web/Program.cs
git commit -m "feat: add ErrorDisplayService for user-friendly error messages

Maps technical exceptions to Swedish plain-language messages.
Logs full exception server-side, shows friendly text to user.
Includes TryAsync convenience wrapper.

Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>"
```

---

### Task 4: OhrHelpButton contextual help

**Files:**
- Create: `src/Web/Components/Shared/OhrHelpButton.razor`

- [ ] **Step 1: Create OhrHelpButton component**

Create `src/Web/Components/Shared/OhrHelpButton.razor`:
```razor
@namespace RegionHR.Web.Components.Shared

<MudTooltip Text="Hjälp">
    <MudIconButton Icon="@Icons.Material.Filled.HelpOutline"
                   Color="Color.Primary" Size="Size.Medium"
                   OnClick="@(() => _open = !_open)"
                   aria-label="Visa hjälp" />
</MudTooltip>

@if (_open)
{
    <MudDrawer @bind-Open="_open" Anchor="Anchor.End" Elevation="2"
               Width="400px" Variant="DrawerVariant.Temporary"
               Style="z-index: 1300;">
        <MudDrawerHeader>
            <div class="d-flex justify-space-between align-center" style="width: 100%;">
                <MudText Typo="Typo.h6">Hjälp</MudText>
                <MudIconButton Icon="@Icons.Material.Filled.Close"
                               OnClick="@(() => _open = false)"
                               aria-label="Stäng hjälp" />
            </div>
        </MudDrawerHeader>
        <div class="pa-4">
            @if (!string.IsNullOrEmpty(PageDescription))
            {
                <MudText Typo="Typo.body1" Class="mb-4">@PageDescription</MudText>
            }

            @if (Faq?.Count > 0)
            {
                <MudText Typo="Typo.subtitle1" Class="mb-2" Style="font-weight: 700;">Vanliga frågor</MudText>
                <MudExpansionPanels MultiExpansion="true" Elevation="0">
                    @foreach (var qa in Faq)
                    {
                        <MudExpansionPanel Text="@qa.Key">
                            <MudText Typo="Typo.body1">@qa.Value</MudText>
                        </MudExpansionPanel>
                    }
                </MudExpansionPanels>
            }

            <MudDivider Class="my-4" />
            <MudButton Variant="Variant.Outlined" Color="Color.Primary"
                       Href="/hjalp/ordlista" FullWidth="true"
                       StartIcon="@Icons.Material.Filled.MenuBook"
                       Style="min-height: 48px;">
                Öppna ordlistan
            </MudButton>

            @if (!string.IsNullOrEmpty(ContactInfo))
            {
                <MudAlert Severity="Severity.Info" Variant="Variant.Text" Class="mt-4">
                    @ContactInfo
                </MudAlert>
            }
        </div>
    </MudDrawer>
}

@code {
    private bool _open;

    [Parameter] public string? PageDescription { get; set; }
    [Parameter] public Dictionary<string, string>? Faq { get; set; }
    [Parameter] public string? ContactInfo { get; set; } = "Behöver du hjälp? Kontakta HR-avdelningen.";
}
```

- [ ] **Step 2: Build**

```bash
dotnet build src/Web/RegionHR.Web.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/Web/Components/Shared/OhrHelpButton.razor
git commit -m "feat: add OhrHelpButton with contextual help drawer

Slide-out drawer with page description, FAQ expandable panels,
glossary link, and contact info. 400px wide, temporary variant.

Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>"
```

---

### Task 5: OhrStatusLegend for color-coded chips

**Files:**
- Create: `src/Web/Components/Shared/OhrStatusLegend.razor`

- [ ] **Step 1: Create OhrStatusLegend component**

Create `src/Web/Components/Shared/OhrStatusLegend.razor`:
```razor
@namespace RegionHR.Web.Components.Shared

<MudPaper Class="pa-3 mb-3" Elevation="0"
          Style="background: var(--mud-palette-background-gray); border-radius: 8px;">
    <div class="d-flex gap-4 flex-wrap align-center">
        <MudText Typo="Typo.caption" Style="font-weight: 600;">Förklaring:</MudText>
        @foreach (var item in Items)
        {
            <div class="d-flex align-center gap-1">
                <MudChip T="string" Size="Size.Small" Color="@item.Color"
                         Style="pointer-events: none;">@item.Label</MudChip>
                @if (!string.IsNullOrEmpty(item.Description))
                {
                    <MudText Typo="Typo.caption">@item.Description</MudText>
                }
            </div>
        }
    </div>
</MudPaper>

@code {
    [Parameter, EditorRequired]
    public List<StatusLegendItem> Items { get; set; } = [];

    public record StatusLegendItem(string Label, Color Color, string? Description = null);
}
```

- [ ] **Step 2: Build**

```bash
dotnet build src/Web/RegionHR.Web.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/Web/Components/Shared/OhrStatusLegend.razor
git commit -m "feat: add OhrStatusLegend component for color-coded chip explanation

Displays a horizontal legend mapping chip colors to meanings.
Used on pages with status indicators (approvals, leave, LAS, etc).

Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>"
```

---

### Task 6: OhrConfirmDialog for destructive actions

**Files:**
- Create: `src/Web/Components/Shared/OhrConfirmDialog.razor`

- [ ] **Step 1: Create OhrConfirmDialog component**

Create `src/Web/Components/Shared/OhrConfirmDialog.razor`:
```razor
@namespace RegionHR.Web.Components.Shared

<MudDialog>
    <TitleContent>
        <div class="d-flex align-center gap-2">
            <MudIcon Icon="@Icons.Material.Filled.Warning" Color="Color.Warning" />
            <MudText Typo="Typo.h6">@Title</MudText>
        </div>
    </TitleContent>
    <DialogContent>
        <MudText Typo="Typo.body1" Class="mb-3">@Message</MudText>
        @if (!string.IsNullOrEmpty(Consequences))
        {
            <MudAlert Severity="Severity.Warning" Variant="Variant.Text" Dense="true">
                @Consequences
            </MudAlert>
        }
        @if (RequireConfirmation)
        {
            <MudCheckBox T="bool" @bind-Value="_confirmed" Class="mt-3"
                         Label="@ConfirmationLabel" />
        }
    </DialogContent>
    <DialogActions>
        <MudButton OnClick="Cancel" Variant="Variant.Text" Style="min-height: 44px;">
            Avbryt
        </MudButton>
        <MudButton OnClick="Confirm" Variant="Variant.Filled"
                   Color="@(IsDangerous ? Color.Error : Color.Primary)"
                   Disabled="@(RequireConfirmation && !_confirmed)"
                   Style="min-height: 44px;">
            @ConfirmText
        </MudButton>
    </DialogActions>
</MudDialog>

@code {
    private bool _confirmed;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public string Title { get; set; } = "Bekräfta åtgärd";
    [Parameter] public string Message { get; set; } = "Är du säker?";
    [Parameter] public string? Consequences { get; set; }
    [Parameter] public string ConfirmText { get; set; } = "Bekräfta";
    [Parameter] public bool IsDangerous { get; set; }
    [Parameter] public bool RequireConfirmation { get; set; }
    [Parameter] public string ConfirmationLabel { get; set; } = "Jag förstår konsekvenserna";

    private void Cancel() => MudDialog.Cancel();
    private void Confirm() => MudDialog.Close(DialogResult.Ok(true));
}
```

- [ ] **Step 2: Build**

```bash
dotnet build src/Web/RegionHR.Web.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/Web/Components/Shared/OhrConfirmDialog.razor
git commit -m "feat: add OhrConfirmDialog for destructive action confirmation

MudDialog-based confirmation with optional consequences warning,
checkbox confirmation requirement, and dangerous action styling.

Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>"
```

---

### Task 7: OhrOnboarding guided tour

**Files:**
- Create: `src/Web/Components/Shared/OhrOnboarding.razor`

- [ ] **Step 1: Create OhrOnboarding component**

Create `src/Web/Components/Shared/OhrOnboarding.razor`:
```razor
@namespace RegionHR.Web.Components.Shared
@inject NavigationManager Nav
@inject Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedLocalStorage Storage

@if (_showTour)
{
    <MudOverlay Visible="true" DarkBackground="true" ZIndex="1400" />
    <MudPaper Elevation="8" Class="pa-6"
              Style="position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%);
                     z-index: 1500; max-width: 500px; width: 90%; border-radius: 16px;">
        <MudText Typo="Typo.h5" Class="mb-2" Style="font-weight: 700;">
            @_steps[_currentStep].Title
        </MudText>
        <MudText Typo="Typo.body1" Class="mb-4">
            @_steps[_currentStep].Description
        </MudText>

        <MudProgressLinear Value="@((_currentStep + 1) * 100.0 / _steps.Count)"
                           Color="Color.Primary" Rounded="true" Class="mb-4" />

        <div class="d-flex justify-space-between">
            <MudButton Variant="Variant.Text" OnClick="SkipTour"
                       Style="min-height: 44px;">
                Hoppa över
            </MudButton>
            <div class="d-flex gap-2">
                @if (_currentStep > 0)
                {
                    <MudButton Variant="Variant.Text" Color="Color.Primary"
                               OnClick="PreviousStep" Style="min-height: 44px;">
                        Tillbaka
                    </MudButton>
                }
                <MudButton Variant="Variant.Filled" Color="Color.Primary"
                           OnClick="NextStep" Style="min-height: 44px;">
                    @(_currentStep < _steps.Count - 1 ? "Nästa" : "Kom igång!")
                </MudButton>
            </div>
        </div>
    </MudPaper>
}

@code {
    private bool _showTour;
    private int _currentStep;

    /// <summary>Role-specific tour steps.</summary>
    [Parameter] public string UserRole { get; set; } = "Anstalld";

    private List<TourStep> _steps = [];

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        try
        {
            var result = await Storage.GetAsync<bool>("ohr-tour-completed");
            if (!result.Success || !result.Value)
            {
                _steps = GetStepsForRole(UserRole);
                _showTour = true;
                StateHasChanged();
            }
        }
        catch { /* First visit — show tour */ }
    }

    private async Task NextStep()
    {
        if (_currentStep < _steps.Count - 1)
        {
            _currentStep++;
        }
        else
        {
            await CompleteTour();
        }
    }

    private void PreviousStep()
    {
        if (_currentStep > 0) _currentStep--;
    }

    private async Task SkipTour() => await CompleteTour();

    private async Task CompleteTour()
    {
        _showTour = false;
        await Storage.SetAsync("ohr-tour-completed", true);
    }

    private static List<TourStep> GetStepsForRole(string role) => role switch
    {
        "Anstalld" =>
        [
            new("Välkommen till OpenHR!", "Här hanterar du allt kring din anställning — ledighet, lön, schema och mer. Vi visar dig runt!"),
            new("Min Sida", "Under 'Min Sida' hittar du allt som rör dig personligen: ditt schema, din lön, ledighetsansökningar och dina ärenden."),
            new("Sjukanmälan", "Behöver du sjukanmäla dig? Gå till Min Sida → Sjukanmälan. Systemet meddelar din chef automatiskt."),
            new("Ledighet", "Vill du ansöka om semester? Gå till Ledighet → Ny ansökan. Din chef godkänner sedan."),
            new("Behöver du hjälp?", "Tryck på ?-knappen uppe till höger på varje sida för att få hjälp. Du kan också söka i ordlistan.")
        ],
        "Chef" =>
        [
            new("Välkommen till OpenHR!", "Som chef hanterar du ditt team, godkänner ledighetsansökningar och följer upp frånvaro."),
            new("Mitt Team", "Under 'Chef' hittar du ditt team, bemanningsöversikt och frånvarokalender."),
            new("Godkännanden", "Under 'Godkännanden' ser du allt som väntar på ditt beslut — ledighet, tidrapporter, ärenden."),
            new("Schema", "Under 'Schema' planerar du ditt teams arbetstider och hanterar passbyten."),
            new("Behöver du hjälp?", "Tryck på ?-knappen för sidspecifik hjälp, eller sök i ordlistan under Hjälp.")
        ],
        _ =>
        [
            new("Välkommen till OpenHR!", "Här hanterar du hela HR-livscykeln — personal, lön, schema, ärenden och mer."),
            new("Personal", "Under 'Personal' hittar du alla anställda, organisationsstruktur och positioner."),
            new("Lön", "Under 'Lön' kör du lönekörningar, hanterar lönearter och gör löneöversyn."),
            new("Rapporter", "Under 'Rapporter' hittar du analyser, KPI:er och lagstadgade rapporter."),
            new("Behöver du hjälp?", "Varje sida har en ?-knapp med hjälp. Ordlistan förklarar alla termer. Välkommen!")
        ]
    };

    private record TourStep(string Title, string Description);
}
```

- [ ] **Step 2: Build**

```bash
dotnet build src/Web/RegionHR.Web.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/Web/Components/Shared/OhrOnboarding.razor
git commit -m "feat: add OhrOnboarding guided tour for first-time users

Role-specific tour (Anställd/Chef/HR+Admin) with 5 steps each.
Shows on first login, persisted via ProtectedLocalStorage.
Skip and back navigation. Overlay-based modal.

Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>"
```

---

### Task 8: OhrBreadcrumb navigation

**Files:**
- Create: `src/Web/Components/Shared/OhrBreadcrumb.razor`

- [ ] **Step 1: Create OhrBreadcrumb component**

Create `src/Web/Components/Shared/OhrBreadcrumb.razor`:
```razor
@namespace RegionHR.Web.Components.Shared
@inject NavigationManager Nav

<MudBreadcrumbs Items="_items" Class="mb-3 pa-0"
                Style="font-size: 0.95rem;"
                Separator=">" />

@code {
    [Parameter, EditorRequired] public string CurrentPage { get; set; } = "";
    [Parameter] public List<BreadcrumbItem>? ExtraItems { get; set; }

    private List<BreadcrumbItem> _items = [];

    protected override void OnParametersSet()
    {
        _items = [new("Start", "/", icon: Icons.Material.Filled.Home)];

        if (ExtraItems is not null)
        {
            _items.AddRange(ExtraItems);
        }

        _items.Add(new(CurrentPage, href: null, disabled: true));
    }
}
```

- [ ] **Step 2: Build**

```bash
dotnet build src/Web/RegionHR.Web.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/Web/Components/Shared/OhrBreadcrumb.razor
git commit -m "feat: add OhrBreadcrumb navigation component

MudBreadcrumbs wrapper with Start > [parents] > current page pattern.
Accepts ExtraItems for intermediate levels. Home icon on Start.

Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>"
```

---

### Task 9: Wire onboarding into AdminLayout

**Files:**
- Modify: `src/Web/Components/Layout/AdminLayout.razor`

- [ ] **Step 1: Add OhrOnboarding to AdminLayout**

In `src/Web/Components/Layout/AdminLayout.razor`, add the onboarding component inside the `else` block (after the `<MudLayout>` closing), just before the closing `</div>`:

```razor
<RegionHR.Web.Components.Shared.OhrOnboarding UserRole="@Auth.Role" />
```

This ensures the tour appears on first login for all authenticated users.

- [ ] **Step 2: Build**

```bash
dotnet build src/Web/RegionHR.Web.csproj
```

- [ ] **Step 3: Commit**

```bash
git add src/Web/Components/Layout/AdminLayout.razor
git commit -m "feat: wire OhrOnboarding into AdminLayout for first-login tour

Co-Authored-By: Claude Opus 4.6 (1M context) <noreply@anthropic.com>"
```
