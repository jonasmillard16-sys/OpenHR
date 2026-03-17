# Plan 0: Infrastructure & Foundation

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Set up the foundation that all other plans depend on: rename to OpenHR, install MudBlazor, create role-based layouts, add i18n support, and replace QuestPDF with PdfSharpCore.

**Architecture:** This plan modifies the outermost shell of the application — project names stay as RegionHR.* internally (rename is branding only), MudBlazor replaces the custom design system components for complex widgets while keeping OpenHR-specific wrappers, and ASP.NET Core localization provides i18n. Three distinct layout components replace the single MainLayout.

**Tech Stack:** .NET 9, Blazor Server, MudBlazor (MIT), PdfSharpCore (MIT), ASP.NET Core Localization

**Design Spec:** `docs/superpowers/specs/2026-03-17-regionhr-production-ready-design.md`

---

## File Structure

### New files

```
src/Web/
├── Components/
│   ├── Layout/
│   │   ├── EmployeeLayout.razor          # Stina's 6-card layout (no sidebar)
│   │   ├── ManagerLayout.razor           # Chef layout (approval queue + cards)
│   │   ├── AdminLayout.razor             # HR/Admin layout (sidebar + content)
│   │   └── Shared/
│   │       ├── TopBar.razor              # Shared topbar (search, notifications, user, language)
│   │       └── NotificationBell.razor    # Notification bell with unread count
│   ├── Shared/
│   │   ├── OhrBigCard.razor              # Stina-sized card for employee view
│   │   ├── OhrConversationFlow.razor     # Step-by-step conversation component
│   │   ├── OhrSuggestionCard.razor       # Intelligent suggestion display
│   │   └── OhrBackButton.razor           # Universal back button
│   └── App.razor                         # Modified: add MudBlazor providers
├── Resources/
│   ├── SharedResources.sv.resx           # Swedish strings
│   └── SharedResources.en.resx           # English strings
├── Localization/
│   └── SharedResources.cs                # Marker class for localization
└── Program.cs                            # Modified: add MudBlazor + i18n services

src/DesignSystem/
├── RegionHR.DesignSystem.csproj          # Modified: add MudBlazor dependency
└── Components/
    └── (existing components kept as-is)

tests/Web.Tests/
├── RegionHR.Web.Tests.csproj             # New test project
├── Layout/
│   ├── EmployeeLayoutTests.cs
│   ├── ManagerLayoutTests.cs
│   └── AdminLayoutTests.cs
└── Shared/
    ├── OhrBigCardTests.cs
    └── OhrConversationFlowTests.cs
```

### Modified files

```
src/Web/Components/App.razor              # Add MudBlazor providers
src/Web/Components/Layout/MainLayout.razor # Update branding
src/Web/Components/Layout/NavMenu.razor    # Restructure for admin view
src/Web/Program.cs                         # Add MudBlazor + i18n + auth role routing
src/Web/RegionHR.Web.csproj               # Add MudBlazor + bUnit packages
src/DesignSystem/RegionHR.DesignSystem.csproj # Add MudBlazor dependency
src/Infrastructure/Export/PdfPayslipGenerator.cs # Replace QuestPDF with PdfSharpCore
src/Infrastructure/RegionHR.Infrastructure.csproj # Replace QuestPDF with PdfSharpCore
RegionHR.sln                              # Add Web.Tests project
```

---

## Chunk 1: MudBlazor Installation & OpenHR Branding

### Task 1: Add MudBlazor NuGet packages

**Files:**
- Modify: `src/Web/RegionHR.Web.csproj`
- Modify: `src/DesignSystem/RegionHR.DesignSystem.csproj`

- [ ] **Step 1: Add MudBlazor to Web project**

```bash
cd C:/Users/Admin/regionhr
dotnet add src/Web/RegionHR.Web.csproj package MudBlazor
```

- [ ] **Step 2: Add MudBlazor to DesignSystem project**

```bash
dotnet add src/DesignSystem/RegionHR.DesignSystem.csproj package MudBlazor
```

- [ ] **Step 3: Add bUnit test package**

```bash
dotnet new xunit -n RegionHR.Web.Tests -o tests/Web.Tests
dotnet sln RegionHR.sln add tests/Web.Tests/RegionHR.Web.Tests.csproj
dotnet add tests/Web.Tests/RegionHR.Web.Tests.csproj package bunit
dotnet add tests/Web.Tests/RegionHR.Web.Tests.csproj reference src/Web/RegionHR.Web.csproj
dotnet add tests/Web.Tests/RegionHR.Web.Tests.csproj reference src/DesignSystem/RegionHR.DesignSystem.csproj
```

- [ ] **Step 4: Verify build succeeds**

Run: `dotnet build RegionHR.sln`
Expected: Build succeeded. 0 errors.

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "chore: add MudBlazor and bUnit packages"
```

---

### Task 2: Configure MudBlazor in App.razor

**Files:**
- Modify: `src/Web/Components/App.razor`
- Modify: `src/Web/Program.cs`
- Modify: `src/Web/Components/_Imports.razor`

- [ ] **Step 1: Update App.razor with MudBlazor providers and OpenHR branding**

Replace the full content of `src/Web/Components/App.razor` with:

```html
<!DOCTYPE html>
<html lang="sv">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>OpenHR</title>
    <base href="/" />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap" rel="stylesheet" />
    <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
    <link rel="stylesheet" href="_content/RegionHR.DesignSystem/css/regionhr.css" />
    <link rel="stylesheet" href="css/app.css" />
    <HeadOutlet />
</head>
<body>
    <a class="rhr-skip-link" href="#main-content">Hoppa till huvudinnehåll</a>
    <MudThemeProvider Theme="_openHrTheme" />
    <MudPopoverProvider />
    <MudDialogProvider />
    <MudSnackbarProvider />
    <Routes />
    <script src="_content/MudBlazor/MudBlazor.min.js"></script>
    <script src="_framework/blazor.web.js"></script>
</body>
</html>

@code {
    private readonly MudTheme _openHrTheme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1a5276",
            PrimaryDarken = "#154360",
            PrimaryLighten = "#d4e6f1",
            Secondary = "#2e86c1",
            Success = "#1e8449",
            Warning = "#b7950b",
            Error = "#c0392b",
            Info = "#2471a3",
            Background = "#ffffff",
            Surface = "#ffffff",
            AppbarBackground = "#1a5276",
            DrawerBackground = "#f8f9fa",
            TextPrimary = "#1c2833",
            TextSecondary = "#5d6d7e",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Inter", "Segoe UI", "system-ui", "sans-serif"],
                FontSize = "1.125rem",  // 18px base — Stina-principle
            },
            H1 = new H1Typography { FontSize = "2rem", FontWeight = 700 },
            H2 = new H2Typography { FontSize = "1.5rem", FontWeight = 600 },
            Button = new ButtonTypography { FontSize = "1rem", FontWeight = 600 },
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "8px",
        }
    };
}
```

- [ ] **Step 2: Add MudBlazor using to _Imports.razor**

Add these lines to the top of `src/Web/Components/_Imports.razor`:

```razor
@using MudBlazor
```

- [ ] **Step 3: Add MudBlazor services to Program.cs**

In `src/Web/Program.cs`, add after `builder.Services.AddRazorComponents()`:

```csharp
// MudBlazor
builder.Services.AddMudServices();
```

Add the using at the top:
```csharp
using MudBlazor.Services;
```

- [ ] **Step 4: Build and verify**

Run: `dotnet build src/Web/RegionHR.Web.csproj`
Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add src/Web/Components/App.razor src/Web/Components/_Imports.razor src/Web/Program.cs
git commit -m "feat: configure MudBlazor with OpenHR theme (Stina-principle: 18px base)"
```

---

### Task 3: Update branding from RegionHR to OpenHR in UI

**Files:**
- Modify: `src/Web/Components/Layout/MainLayout.razor`

- [ ] **Step 1: Update MainLayout branding**

In `src/Web/Components/Layout/MainLayout.razor`, replace:
```razor
<h2>RegionHR</h2>
<span class="rhr-sidebar__subtitle">HR-system</span>
```
With:
```razor
<h2>OpenHR</h2>
<span class="rhr-sidebar__subtitle">Fritt HR-system</span>
```

- [ ] **Step 2: Update page title in App.razor**

Already done in Task 2 (`<title>OpenHR</title>`).

- [ ] **Step 3: Build and verify**

Run: `dotnet build src/Web/RegionHR.Web.csproj`
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add src/Web/Components/Layout/MainLayout.razor
git commit -m "feat: rebrand UI from RegionHR to OpenHR"
```

---

## Chunk 2: Role-Based Layouts

### Task 4: Create Employee Layout (Stina's view — no sidebar, just cards)

**Files:**
- Create: `src/Web/Components/Layout/EmployeeLayout.razor`
- Create: `src/Web/Components/Layout/Shared/TopBar.razor`

- [ ] **Step 1: Create shared TopBar component**

Create `src/Web/Components/Layout/Shared/TopBar.razor`:

```razor
@namespace RegionHR.Web.Components.Layout.Shared

<MudAppBar Fixed="true" Color="Color.Primary" Elevation="1">
    <MudText Typo="Typo.h6" Class="ml-3">OpenHR</MudText>
    <MudSpacer />
    <MudIconButton Icon="@Icons.Material.Filled.Language"
                   Color="Color.Inherit"
                   Title="Byt språk"
                   aria-label="Byt språk" />
    <MudBadge Content="@UnreadCount" Overlap="true" Color="Color.Error"
              Visible="@(UnreadCount > 0)">
        <MudIconButton Icon="@Icons.Material.Filled.Notifications"
                       Color="Color.Inherit"
                       Title="Notiser"
                       aria-label="@($"{UnreadCount} olästa notiser")" />
    </MudBadge>
    <MudMenu Icon="@Icons.Material.Filled.Person" Color="Color.Inherit"
             AnchorOrigin="Origin.BottomRight" TransformOrigin="Origin.TopRight"
             aria-label="Användarmeny">
        <MudMenuItem>@UserName</MudMenuItem>
        <MudMenuItem Icon="@Icons.Material.Filled.Settings">Inställningar</MudMenuItem>
        <MudMenuItem Icon="@Icons.Material.Filled.Logout">Logga ut</MudMenuItem>
    </MudMenu>
</MudAppBar>

@code {
    [Parameter] public string UserName { get; set; } = "Användare";
    [Parameter] public int UnreadCount { get; set; }
}
```

- [ ] **Step 2: Create EmployeeLayout**

Create `src/Web/Components/Layout/EmployeeLayout.razor`:

```razor
@inherits LayoutComponentBase
@namespace RegionHR.Web.Components.Layout

<MudLayout>
    <Shared.TopBar UserName="Anna Svensson" UnreadCount="2" />
    <MudMainContent Class="pt-16">
        <MudContainer MaxWidth="MaxWidth.Medium" Class="py-6">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build src/Web/RegionHR.Web.csproj`
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add src/Web/Components/Layout/Shared/TopBar.razor src/Web/Components/Layout/EmployeeLayout.razor
git commit -m "feat: add EmployeeLayout with TopBar (Stina-view, no sidebar)"
```

---

### Task 5: Create Manager Layout (Chef view — approvals on top, then cards)

**Files:**
- Create: `src/Web/Components/Layout/ManagerLayout.razor`

- [ ] **Step 1: Create ManagerLayout**

Create `src/Web/Components/Layout/ManagerLayout.razor`:

```razor
@inherits LayoutComponentBase
@namespace RegionHR.Web.Components.Layout

<MudLayout>
    <Shared.TopBar UserName="Erik Lindberg" UnreadCount="3" />
    <MudMainContent Class="pt-16">
        <MudContainer MaxWidth="MaxWidth.Large" Class="py-6">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>
```

- [ ] **Step 2: Build and verify**

Run: `dotnet build src/Web/RegionHR.Web.csproj`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/Web/Components/Layout/ManagerLayout.razor
git commit -m "feat: add ManagerLayout for chef view"
```

---

### Task 6: Create Admin Layout (sidebar + content area)

**Files:**
- Create: `src/Web/Components/Layout/AdminLayout.razor`
- Modify: `src/Web/Components/Layout/NavMenu.razor`

- [ ] **Step 1: Update NavMenu for admin with OpenHR module structure**

Replace full content of `src/Web/Components/Layout/NavMenu.razor`:

```razor
<MudNavMenu Bordered="true" Class="py-2">
    <MudNavLink Href="/" Icon="@Icons.Material.Filled.Home">Start</MudNavLink>

    <MudNavGroup Title="Personal" Icon="@Icons.Material.Filled.People" Expanded="false">
        <MudNavLink Href="/anstallda">Anställda</MudNavLink>
        <MudNavLink Href="/organisation">Organisation</MudNavLink>
        <MudNavLink Href="/positioner">Positioner</MudNavLink>
        <MudNavLink Href="/kompetens">Kompetensregister</MudNavLink>
    </MudNavGroup>

    <MudNavGroup Title="Lön" Icon="@Icons.Material.Filled.Payments" Expanded="false">
        <MudNavLink Href="/lon/korningar">Lönekörningar</MudNavLink>
        <MudNavLink Href="/lon/lonearter">Lönearter</MudNavLink>
        <MudNavLink Href="/loneoversyn">Löneöversyn</MudNavLink>
        <MudNavLink Href="/lon/statistik">Statistik</MudNavLink>
    </MudNavGroup>

    <MudNavGroup Title="Schema & Tid" Icon="@Icons.Material.Filled.CalendarMonth" Expanded="false">
        <MudNavLink Href="/schema">Schemaöversikt</MudNavLink>
        <MudNavLink Href="/schema/bemanning">Bemanning</MudNavLink>
        <MudNavLink Href="/stampling">Instämpling</MudNavLink>
        <MudNavLink Href="/tidrapporter">Tidrapporter</MudNavLink>
    </MudNavGroup>

    <MudNavLink Href="/arenden" Icon="@Icons.Material.Filled.Assignment">Ärenden</MudNavLink>
    <MudNavLink Href="/ledighet" Icon="@Icons.Material.Filled.BeachAccess">Ledighet</MudNavLink>
    <MudNavLink Href="/halsosam" Icon="@Icons.Material.Filled.LocalHospital">HälsoSAM</MudNavLink>
    <MudNavLink Href="/las" Icon="@Icons.Material.Filled.Warning">LAS</MudNavLink>
    <MudNavLink Href="/dokument" Icon="@Icons.Material.Filled.Description">Dokument</MudNavLink>
    <MudNavLink Href="/medarbetarsamtal" Icon="@Icons.Material.Filled.RecordVoiceOver">Medarbetarsamtal</MudNavLink>
    <MudNavLink Href="/rekrytering/vakanser" Icon="@Icons.Material.Filled.PersonSearch">Rekrytering</MudNavLink>
    <MudNavLink Href="/rapporter" Icon="@Icons.Material.Filled.BarChart">Rapporter</MudNavLink>

    <MudDivider Class="my-2" />
    <MudText Typo="Typo.caption" Class="px-4 py-1" Style="color: var(--mud-palette-text-secondary);">System</MudText>
    <MudNavLink Href="/admin/konfiguration" Icon="@Icons.Material.Filled.Settings">Inställningar</MudNavLink>
    <MudNavLink Href="/gdpr" Icon="@Icons.Material.Filled.Security">GDPR</MudNavLink>
    <MudNavLink Href="/audit" Icon="@Icons.Material.Filled.History">Granskningslogg</MudNavLink>

    <MudDivider Class="my-2" />
    <MudNavLink Href="/minsida" Icon="@Icons.Material.Filled.AccountCircle">Min sida</MudNavLink>
</MudNavMenu>
```

- [ ] **Step 2: Create AdminLayout**

Create `src/Web/Components/Layout/AdminLayout.razor`:

```razor
@inherits LayoutComponentBase
@namespace RegionHR.Web.Components.Layout

<MudLayout>
    <Shared.TopBar UserName="Sara Andersson" UnreadCount="5" />
    <MudDrawer @bind-Open="_drawerOpen" Variant="DrawerVariant.Mini"
               OpenMiniOnHover="true" Elevation="1"
               ClipMode="DrawerClipMode.Always">
        <NavMenu />
    </MudDrawer>
    <MudMainContent Class="pt-16">
        <MudContainer MaxWidth="MaxWidth.ExtraLarge" Class="py-6">
            @Body
        </MudContainer>
    </MudMainContent>
</MudLayout>

@code {
    private bool _drawerOpen = true;
}
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build src/Web/RegionHR.Web.csproj`
Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add src/Web/Components/Layout/AdminLayout.razor src/Web/Components/Layout/NavMenu.razor
git commit -m "feat: add AdminLayout with MudBlazor sidebar navigation"
```

---

## Chunk 3: OpenHR Custom Components (Stina-principle)

### Task 7: Create OhrBigCard — the large touchable card for employee view

**Files:**
- Create: `src/Web/Components/Shared/OhrBigCard.razor`
- Create: `tests/Web.Tests/Shared/OhrBigCardTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/Web.Tests/Shared/OhrBigCardTests.cs`:

```csharp
using Bunit;
using RegionHR.Web.Components.Shared;
using Xunit;

namespace RegionHR.Web.Tests.Shared;

public class OhrBigCardTests : TestContext
{
    [Fact]
    public void Renders_title_and_description()
    {
        var cut = RenderComponent<OhrBigCard>(p => p
            .Add(c => c.Icon, "📅")
            .Add(c => c.Title, "Mitt schema")
            .Add(c => c.Description, "Se när du jobbar")
            .Add(c => c.Href, "/minsida/schema"));

        cut.Find("h3").TextContent.ShouldContain("Mitt schema");
        cut.Find("p").TextContent.ShouldContain("Se när du jobbar");
    }

    [Fact]
    public void Renders_badge_value_when_provided()
    {
        var cut = RenderComponent<OhrBigCard>(p => p
            .Add(c => c.Icon, "🌴")
            .Add(c => c.Title, "Jag vill ha ledigt")
            .Add(c => c.BadgeValue, "23 dagar kvar")
            .Add(c => c.Href, "/minsida/ledighet"));

        cut.Markup.ShouldContain("23 dagar kvar");
    }

    [Fact]
    public void Has_correct_aria_label()
    {
        var cut = RenderComponent<OhrBigCard>(p => p
            .Add(c => c.Icon, "😷")
            .Add(c => c.Title, "Jag är sjuk")
            .Add(c => c.Href, "/minsida/sjukanmalan"));

        var link = cut.Find("a");
        link.GetAttribute("aria-label").ShouldBe("Jag är sjuk");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Web.Tests/ --filter "OhrBigCardTests" -v n`
Expected: FAIL — OhrBigCard component not found.

- [ ] **Step 3: Create OhrBigCard component**

Create `src/Web/Components/Shared/OhrBigCard.razor`:

```razor
@namespace RegionHR.Web.Components.Shared

<MudPaper Class="ohr-bigcard pa-6" Elevation="2" Style="cursor: pointer; min-height: 180px;"
          @onclick="Navigate" role="link" aria-label="@Title" tabindex="0"
          @onkeydown="HandleKeyDown">
    <div style="font-size: 2.5rem; margin-bottom: 0.5rem;">@Icon</div>
    @if (!string.IsNullOrEmpty(BadgeValue))
    {
        <MudText Typo="Typo.h5" Color="Color.Primary" Style="font-weight: 700;">@BadgeValue</MudText>
    }
    <MudText Typo="Typo.h6" tag="h3" Style="font-weight: 600; margin-top: 0.5rem;">@Title</MudText>
    @if (!string.IsNullOrEmpty(Description))
    {
        <MudText Typo="Typo.body1" Style="color: var(--mud-palette-text-secondary); margin-top: 0.25rem;">
            @Description
        </MudText>
    }
</MudPaper>

@code {
    [Parameter, EditorRequired] public string Icon { get; set; } = "";
    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter] public string? Description { get; set; }
    [Parameter] public string? BadgeValue { get; set; }
    [Parameter, EditorRequired] public string Href { get; set; } = "";
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private void Navigate() => Nav.NavigateTo(Href);

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key is "Enter" or " ")
            Navigate();
    }
}
```

- [ ] **Step 4: Run tests**

Run: `dotnet test tests/Web.Tests/ --filter "OhrBigCardTests" -v n`
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Web/Components/Shared/OhrBigCard.razor tests/Web.Tests/Shared/OhrBigCardTests.cs
git commit -m "feat: add OhrBigCard component for Stina-principle employee view"
```

---

### Task 8: Create OhrConversationFlow — step-by-step wizard

**Files:**
- Create: `src/Web/Components/Shared/OhrConversationFlow.razor`
- Create: `tests/Web.Tests/Shared/OhrConversationFlowTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/Web.Tests/Shared/OhrConversationFlowTests.cs`:

```csharp
using Bunit;
using RegionHR.Web.Components.Shared;
using Xunit;

namespace RegionHR.Web.Tests.Shared;

public class OhrConversationFlowTests : TestContext
{
    [Fact]
    public void Shows_first_step_initially()
    {
        var cut = RenderComponent<OhrConversationFlow>(p => p
            .Add(c => c.Steps, new List<string> { "Steg 1", "Steg 2", "Klart" })
            .Add(c => c.CurrentStep, 0)
            .Add(c => c.ChildContent, builder =>
                builder.AddContent(0, "Första steget")));

        cut.Markup.ShouldContain("Steg 1");
        cut.Markup.ShouldContain("Första steget");
    }

    [Fact]
    public void Shows_progress_indicator()
    {
        var cut = RenderComponent<OhrConversationFlow>(p => p
            .Add(c => c.Steps, new List<string> { "Steg 1", "Steg 2", "Klart" })
            .Add(c => c.CurrentStep, 1)
            .Add(c => c.ChildContent, builder =>
                builder.AddContent(0, "Andra steget")));

        // Step 2 of 3 — progress should be visible
        cut.Markup.ShouldContain("Steg 2");
    }

    [Fact]
    public void Has_back_button_on_step_2()
    {
        var cut = RenderComponent<OhrConversationFlow>(p => p
            .Add(c => c.Steps, new List<string> { "Steg 1", "Steg 2" })
            .Add(c => c.CurrentStep, 1)
            .Add(c => c.ChildContent, builder =>
                builder.AddContent(0, "Content")));

        cut.Find("[aria-label='Tillbaka']").ShouldNotBeNull();
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test tests/Web.Tests/ --filter "OhrConversationFlowTests" -v n`
Expected: FAIL — component not found.

- [ ] **Step 3: Create OhrConversationFlow component**

Create `src/Web/Components/Shared/OhrConversationFlow.razor`:

```razor
@namespace RegionHR.Web.Components.Shared

<div class="ohr-conversation" role="form" aria-label="@Steps.ElementAtOrDefault(CurrentStep)">
    @if (CurrentStep > 0)
    {
        <MudButton StartIcon="@Icons.Material.Filled.ArrowBack"
                   Variant="Variant.Text" Color="Color.Primary"
                   OnClick="OnBack" aria-label="Tillbaka"
                   Style="margin-bottom: 1rem; font-size: 1.125rem;">
            Tillbaka
        </MudButton>
    }

    <div class="ohr-conversation__progress" style="margin-bottom: 1.5rem;">
        <MudProgressLinear Value="@ProgressPercent" Color="Color.Primary"
                           Rounded="true" Size="Size.Medium" />
        <MudText Typo="Typo.caption" Align="Align.Center" Class="mt-1">
            @Steps.ElementAtOrDefault(CurrentStep)
        </MudText>
    </div>

    <div class="ohr-conversation__content">
        @ChildContent
    </div>
</div>

@code {
    [Parameter, EditorRequired] public List<string> Steps { get; set; } = [];
    [Parameter] public int CurrentStep { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public EventCallback OnBack { get; set; }

    private int ProgressPercent => Steps.Count > 1
        ? (int)((double)CurrentStep / (Steps.Count - 1) * 100)
        : 100;
}
```

- [ ] **Step 4: Run tests**

Run: `dotnet test tests/Web.Tests/ --filter "OhrConversationFlowTests" -v n`
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Web/Components/Shared/OhrConversationFlow.razor tests/Web.Tests/Shared/OhrConversationFlowTests.cs
git commit -m "feat: add OhrConversationFlow for step-by-step Stina interactions"
```

---

### Task 9: Create OhrSuggestionCard — intelligent assistant suggestions

**Files:**
- Create: `src/Web/Components/Shared/OhrSuggestionCard.razor`

- [ ] **Step 1: Create OhrSuggestionCard component**

Create `src/Web/Components/Shared/OhrSuggestionCard.razor`:

```razor
@namespace RegionHR.Web.Components.Shared

<MudAlert Severity="@AlertSeverity" Variant="Variant.Outlined" Class="mb-3"
          NoIcon="false" Dense="false">
    <MudText Typo="Typo.subtitle1" Style="font-weight: 600;">@Title</MudText>
    <MudText Typo="Typo.body1" Class="mt-1">@Explanation</MudText>
    @if (Actions.Count > 0)
    {
        <div class="mt-3 d-flex gap-2 flex-wrap">
            @foreach (var action in Actions)
            {
                <MudButton Variant="Variant.Filled" Color="Color.Primary"
                           Size="Size.Large" OnClick="@(() => OnActionClicked.InvokeAsync(action.Key))"
                           Style="min-height: 48px;">
                    @action.Value
                </MudButton>
            }
        </div>
    }
</MudAlert>

@code {
    [Parameter, EditorRequired] public string Title { get; set; } = "";
    [Parameter] public string? Explanation { get; set; }
    [Parameter] public Dictionary<string, string> Actions { get; set; } = [];
    [Parameter] public SuggestionLevel Level { get; set; } = SuggestionLevel.Info;
    [Parameter] public EventCallback<string> OnActionClicked { get; set; }

    private Severity AlertSeverity => Level switch
    {
        SuggestionLevel.Warning => Severity.Warning,
        SuggestionLevel.Error => Severity.Error,
        SuggestionLevel.Success => Severity.Success,
        _ => Severity.Info
    };

    public enum SuggestionLevel { Info, Warning, Error, Success }
}
```

- [ ] **Step 2: Build and verify**

Run: `dotnet build src/Web/RegionHR.Web.csproj`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/Web/Components/Shared/OhrSuggestionCard.razor
git commit -m "feat: add OhrSuggestionCard for intelligent assistant suggestions"
```

---

## Chunk 4: Internationalization (i18n)

### Task 10: Set up ASP.NET Core Localization

**Files:**
- Create: `src/Web/Localization/SharedResources.cs`
- Create: `src/Web/Resources/SharedResources.sv.resx`
- Create: `src/Web/Resources/SharedResources.en.resx`
- Modify: `src/Web/Program.cs`

- [ ] **Step 1: Create marker class for shared resources**

Create `src/Web/Localization/SharedResources.cs`:

```csharp
namespace RegionHR.Web.Localization;

/// <summary>
/// Marker class for shared localization resources.
/// Resource files: Resources/SharedResources.{culture}.resx
/// </summary>
public class SharedResources { }
```

- [ ] **Step 2: Create Swedish resource file**

Create `src/Web/Resources/SharedResources.sv.resx`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <data name="AppName"><value>OpenHR</value></data>
  <data name="MySchedule"><value>Mitt schema</value></data>
  <data name="MyScheduleDesc"><value>Se när du jobbar</value></data>
  <data name="MyLeave"><value>Jag vill ha ledigt</value></data>
  <data name="MyLeaveDesc"><value>Ansök om semester</value></data>
  <data name="MySalary"><value>Min lön</value></data>
  <data name="MySalaryDesc"><value>Se vad du fått i lön</value></data>
  <data name="ImSick"><value>Jag är sjuk</value></data>
  <data name="ImSickDesc"><value>Meddela att du inte kan jobba</value></data>
  <data name="MyThings"><value>Mina pågående saker</value></data>
  <data name="MyThingsDesc"><value>Se status på dina ärenden</value></data>
  <data name="AboutMe"><value>Om mig</value></data>
  <data name="AboutMeDesc"><value>Min adress, telefon och uppgifter</value></data>
  <data name="Back"><value>Tillbaka</value></data>
  <data name="Done"><value>Klart!</value></data>
  <data name="Approve"><value>Godkänn</value></data>
  <data name="Reject"><value>Nej</value></data>
  <data name="DaysLeft"><value>dagar kvar</value></data>
  <data name="ThingsWaiting"><value>Saker som väntar på dig</value></data>
  <data name="LogOut"><value>Logga ut</value></data>
  <data name="Settings"><value>Inställningar</value></data>
  <data name="ChangeLanguage"><value>Byt språk</value></data>
</root>
```

- [ ] **Step 3: Create English resource file**

Create `src/Web/Resources/SharedResources.en.resx`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <data name="AppName"><value>OpenHR</value></data>
  <data name="MySchedule"><value>My schedule</value></data>
  <data name="MyScheduleDesc"><value>See when you work</value></data>
  <data name="MyLeave"><value>I want time off</value></data>
  <data name="MyLeaveDesc"><value>Apply for leave</value></data>
  <data name="MySalary"><value>My pay</value></data>
  <data name="MySalaryDesc"><value>See your salary</value></data>
  <data name="ImSick"><value>I'm sick</value></data>
  <data name="ImSickDesc"><value>Report that you can't work</value></data>
  <data name="MyThings"><value>My ongoing things</value></data>
  <data name="MyThingsDesc"><value>Check the status of your requests</value></data>
  <data name="AboutMe"><value>About me</value></data>
  <data name="AboutMeDesc"><value>My address, phone and details</value></data>
  <data name="Back"><value>Back</value></data>
  <data name="Done"><value>Done!</value></data>
  <data name="Approve"><value>Approve</value></data>
  <data name="Reject"><value>No</value></data>
  <data name="DaysLeft"><value>days left</value></data>
  <data name="ThingsWaiting"><value>Things waiting for you</value></data>
  <data name="LogOut"><value>Log out</value></data>
  <data name="Settings"><value>Settings</value></data>
  <data name="ChangeLanguage"><value>Change language</value></data>
</root>
```

- [ ] **Step 4: Add localization services to Program.cs**

In `src/Web/Program.cs`, add after the MudBlazor line:

```csharp
// Localization (i18n)
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "sv", "en" };
    options.SetDefaultCulture("sv");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
});
```

And after `app.UseAntiforgery();`:

```csharp
app.UseRequestLocalization();
```

Add using:
```csharp
using Microsoft.AspNetCore.Localization;
```

- [ ] **Step 5: Build and verify**

Run: `dotnet build src/Web/RegionHR.Web.csproj`
Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add src/Web/Localization/ src/Web/Resources/ src/Web/Program.cs
git commit -m "feat: add i18n support with Swedish and English resource files"
```

---

## Chunk 5: Replace QuestPDF with PdfSharpCore

### Task 11: Swap PDF library

**Files:**
- Modify: `src/Infrastructure/RegionHR.Infrastructure.csproj`
- Modify: `src/Infrastructure/Export/PdfPayslipGenerator.cs`

- [ ] **Step 1: Replace NuGet package**

```bash
cd C:/Users/Admin/regionhr
dotnet remove src/Infrastructure/RegionHR.Infrastructure.csproj package QuestPDF
dotnet add src/Infrastructure/RegionHR.Infrastructure.csproj package PdfSharpCore
```

- [ ] **Step 2: Rewrite PdfPayslipGenerator to use PdfSharpCore**

Replace the full content of `src/Infrastructure/Export/PdfPayslipGenerator.cs`:

```csharp
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using RegionHR.Payroll.Domain;

namespace RegionHR.Infrastructure.Export;

public class PdfPayslipGenerator
{
    public byte[] Generate(PayrollResult result, string employeeName, string employer)
    {
        using var document = new PdfDocument();
        document.Info.Title = $"Lönespecifikation {result.Year}-{result.Month:D2}";
        document.Info.Author = employer;

        var page = document.AddPage();
        page.Width = XUnit.FromMillimeter(210);
        page.Height = XUnit.FromMillimeter(297);

        using var gfx = XGraphics.FromPdfPage(page);

        var fontTitle = new XFont("Arial", 18, XFontStyleEx.Bold);
        var fontHeader = new XFont("Arial", 12, XFontStyleEx.Bold);
        var fontNormal = new XFont("Arial", 10, XFontStyleEx.Regular);
        var fontSmall = new XFont("Arial", 8, XFontStyleEx.Regular);

        double y = 40;
        double marginLeft = 40;
        double pageWidth = page.Width.Point - 80;

        // Header
        gfx.DrawString("OpenHR", fontTitle, XBrushes.DarkBlue, marginLeft, y);
        y += 25;
        gfx.DrawString($"Lönespecifikation — {result.Year}-{result.Month:D2}", fontHeader, XBrushes.Black, marginLeft, y);
        y += 20;

        // Employee info
        gfx.DrawString($"Anställd: {employeeName}", fontNormal, XBrushes.Black, marginLeft, y);
        y += 15;
        gfx.DrawString($"Arbetsgivare: {employer}", fontNormal, XBrushes.Black, marginLeft, y);
        y += 25;

        // Separator
        gfx.DrawLine(XPens.LightGray, marginLeft, y, marginLeft + pageWidth, y);
        y += 15;

        // Summary
        DrawRow(gfx, fontHeader, marginLeft, pageWidth, ref y, "Bruttolön", result.Brutto.Amount.ToString("N2") + " kr");
        DrawRow(gfx, fontNormal, marginLeft, pageWidth, ref y, "Skatt", $"-{result.Skatt.Amount:N2} kr");

        if (result.OBTillagg.Amount > 0)
            DrawRow(gfx, fontNormal, marginLeft, pageWidth, ref y, "OB-tillägg", $"{result.OBTillagg.Amount:N2} kr");

        if (result.Semesterlon.Amount > 0)
            DrawRow(gfx, fontNormal, marginLeft, pageWidth, ref y, "Semesterlön", $"{result.Semesterlon.Amount:N2} kr");

        y += 10;
        gfx.DrawLine(XPens.LightGray, marginLeft, y, marginLeft + pageWidth, y);
        y += 15;

        DrawRow(gfx, fontHeader, marginLeft, pageWidth, ref y, "Nettolön", $"{result.Netto.Amount:N2} kr");

        y += 20;

        // Employer costs
        gfx.DrawString("Arbetsgivarkostnader", fontHeader, XBrushes.Gray, marginLeft, y);
        y += 15;
        DrawRow(gfx, fontSmall, marginLeft, pageWidth, ref y, "Arbetsgivaravgifter", $"{result.Arbetsgivaravgifter.Amount:N2} kr");
        DrawRow(gfx, fontSmall, marginLeft, pageWidth, ref y, "Pension (AKAP-KR)", $"{result.PensionsgrundandeBelopp.Amount:N2} kr");

        // Footer
        gfx.DrawString($"Genererad av OpenHR — {DateTime.Now:yyyy-MM-dd HH:mm}", fontSmall, XBrushes.Gray, marginLeft, page.Height.Point - 30);

        using var stream = new MemoryStream();
        document.Save(stream, false);
        return stream.ToArray();
    }

    private static void DrawRow(XGraphics gfx, XFont font, double x, double width, ref double y, string label, string value)
    {
        gfx.DrawString(label, font, XBrushes.Black, x, y);
        gfx.DrawString(value, font, XBrushes.Black, x + width - gfx.MeasureString(value, font).Width, y);
        y += font.Height + 4;
    }
}
```

- [ ] **Step 3: Build and verify**

Run: `dotnet build src/Infrastructure/RegionHR.Infrastructure.csproj`
Expected: Build succeeded. 0 errors.

- [ ] **Step 4: Run all tests to ensure nothing broke**

Run: `dotnet test RegionHR.sln -v n`
Expected: All tests pass.

- [ ] **Step 5: Commit**

```bash
git add src/Infrastructure/RegionHR.Infrastructure.csproj src/Infrastructure/Export/PdfPayslipGenerator.cs
git commit -m "feat: replace QuestPDF with PdfSharpCore (MIT) for FOSS compliance"
```

---

## Chunk 6: Add AGPL-3.0 License

### Task 12: Create LICENSE file

**Files:**
- Create: `LICENSE`

- [ ] **Step 1: Create AGPL-3.0 license file**

```bash
cd C:/Users/Admin/regionhr
curl -sL https://www.gnu.org/licenses/agpl-3.0.txt -o LICENSE
```

If curl fails, create `LICENSE` with the AGPL-3.0 standard text header:

```
OpenHR - Free and Open Source HR System
Copyright (C) 2026

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
```

- [ ] **Step 2: Commit**

```bash
git add LICENSE
git commit -m "chore: add AGPL-3.0 license — all forks must remain open source"
```

---

## Verification

After completing all tasks in this plan, verify:

- [ ] `dotnet build RegionHR.sln` succeeds with 0 errors
- [ ] `dotnet test RegionHR.sln` — all tests pass
- [ ] App starts: `dotnet run --project src/Web/RegionHR.Web.csproj` — opens in browser
- [ ] MudBlazor components render (sidebar navigation visible)
- [ ] OpenHR branding shows in header
- [ ] Three layout files exist: EmployeeLayout, ManagerLayout, AdminLayout

---

## Next Plans

After Plan 0 is complete:
- **Plan 1:** Employee Self-Service (6 Stina-cards + pages behind them)
- **Plan 2:** Manager Portal (approval queue + team overview)
- **Plan 3:** HR Admin Core modules UI
- **Plan 4:** HR Admin Support modules UI
- **Plans 5-8:** Functionality deepening, notifications, reporting
