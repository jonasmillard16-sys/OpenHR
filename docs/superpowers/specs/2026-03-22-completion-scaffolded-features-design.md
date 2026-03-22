# OpenHR Completion — Scaffolded Features, TODOs & Infrastructure

**Date:** 2026-03-22
**Scope:** 8 scaffolded features, 3 domain TODOs, PDF, bottom nav + swipe, repository paginering

---

## 1. Scaffolded Features

### 1.1 Automation Engine
`AutomationEngine : IAutomationEngine` — evaluerar seedade regler mot triggers. Bakgrundsjobb `AutomationBackgroundService` pollar var 5:e minut. Tre nivåer: Notify (skapa notifikation), Suggest (skapa förslag), Autopilot (utför automatiskt).

### 1.2 Predictive Models
`PredictionCalculationService` — heuristikbaserade prognoser: attrition (tenure + frånvaro + anställningsform), sjukfrånvaro (12-mån trend), headcount (linjär extrapolering), lönekostnad (snittökning).

### 1.3 Demand Forecasting
`DemandForecastGenerator` — genomsnittligt bemanningsbehov per veckodag/skift baserat på historisk data (12 veckor). Sparar till DemandForecast-entitet.

### 1.4 Webhook Retry
`WebhookRetryBackgroundService` — pollar EventDelivery med KanRetry(), kör HTTP POST med exponential backoff. Max 5 retries.

### 1.5 API Key Scope Enforcement
`ApiKeyScopeMiddleware` — matchar request-path mot API-nyckelns Scope-JSON. 403 vid mismatch. Registreras för `/api/`-rutter.

### 1.6 Custom Objects JSON Validation
NJsonSchema NuGet. `CustomObjectValidator` validerar record-data mot FaltSchema innan sparning. Returnerar valideringsfel i klartext.

### 1.7 Marketplace Plugin Execution
`PluginApplicator` — läser Extension.Content (manifest JSON), skapar CustomObjectDefinitions och registrerar EventSubscriptions. Activate/deactivate via ExtensionInstallation-status.

### 1.8 HR Service Auto-Routing
`AutoRoutingService` — mappar ServiceCategory.DefaultKoId → HRQueue, tilldelar agent via round-robin. Anropas från ServiceRequestRouter.RouteAsync().

## 2. Domänmodell-TODOs

### 2.1 Employment.BeraknaTimlon()
Hämta VeckotimmarHeltid från CollectiveAgreement via anställningens Avtalsomrade. Fallback 38.25 om avtal saknas.

### 2.2 LeaveRequest överlappningsvalidering
I Skapa(): query befintliga ej-avslagna requests för samma EmployeeId + överlappande datumperiod. Kasta om överlapp.

### 2.3 BenefitEnrollment enum-migration
Skapa BenefitEnrollmentStatus enum (Pending, Active, Cancelled). Byt string-property. Uppdatera EF-config.

## 3. PDF-generering
Applicera PdfSharpCore-mönstret (från PdfPayslipGenerator) på: Tjänstgöringsintyg, Anställningsavtal, Offboarding-sammanfattning.

## 4. Bottom Navigation + Swipe
- BottomNav.razor: MudBottomNavBar med 4 val, visas < 600px
- swipe.js: Touch event handler, JSInterop till Blazor

## 5. Repository-paginering
PaginatedResult<T> record. GetPaginatedAsync i alla repositories. MudTable ServerData-integration.
