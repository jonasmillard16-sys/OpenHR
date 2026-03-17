# Plan 1: Employee Self-Service ("Min sida")

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the old static MinSida/Index.razor with the Stina-principle 6-card dashboard, and build the pages behind each card: schedule, leave, salary, sick report, cases, and profile.

**Architecture:** The new `/minsida` page uses EmployeeLayout (no sidebar) with 6 OhrBigCard components. Each card links to a dedicated sub-page. The sjukanmälan page uses OhrConversationFlow for a 2-question wizard. All pages connect to existing API endpoints via new Blazor services.

**Tech Stack:** Blazor Server, MudBlazor 9.1.0, OhrBigCard, OhrConversationFlow, existing Leave/SelfService APIs

**Depends on:** Plan 0 (completed)

---

## File Structure

### New files
```
src/Web/
├── Components/Pages/MinSida/
│   ├── Index.razor                    # REWRITE: 6-card Stina dashboard
│   ├── MittSchema.razor               # NEW: weekly schedule from API
│   ├── MinLedighet.razor              # NEW: leave balance + apply button
│   ├── MinLon.razor                   # REWRITE: salary specs from API
│   ├── Sjukanmalan.razor             # NEW: conversation-flow sick report
│   ├── MinaArenden.razor             # NEW: case tracking
│   └── MinProfil.razor               # NEW: edit contact info
├── Services/
│   └── SelfServiceApiClient.cs        # NEW: HTTP client to API endpoints
```

### Modified files
```
src/Web/Program.cs                     # Register SelfServiceApiClient + HttpClient
```

---

## Task 1: Create SelfServiceApiClient

**Files:**
- Create: `src/Web/Services/SelfServiceApiClient.cs`
- Modify: `src/Web/Program.cs`

The client wraps calls to `/api/v1/minsida/*` and `/api/v1/ledighet/*` endpoints. For now it uses the DbContext directly (same process), but the service interface is HTTP-shaped for future API separation.

---

## Task 2: Rewrite MinSida/Index.razor — 6-card Stina dashboard

**Files:**
- Modify: `src/Web/Components/Pages/MinSida/Index.razor`

Replace the current 3-column RhrCard layout with 6 OhrBigCard components using EmployeeLayout. Uses localized strings from SharedResources.

---

## Task 3: Build MinSida/MittSchema.razor

**Files:**
- Create: `src/Web/Components/Pages/MinSida/MittSchema.razor`

Weekly schedule view using MudSimpleTable showing shifts from the ScheduledShifts DbSet.

---

## Task 4: Build MinSida/MinLedighet.razor

**Files:**
- Create: `src/Web/Components/Pages/MinSida/MinLedighet.razor`

Shows vacation balance (intjänade/uttagna/sparade/tillgängliga) from VacationBalances, plus a "Jag vill ha ledigt" button that navigates to the leave application flow.

---

## Task 5: Build MinSida/Sjukanmalan.razor — conversation flow

**Files:**
- Create: `src/Web/Components/Pages/MinSida/Sjukanmalan.razor`

The core Stina-principle page. Uses OhrConversationFlow with 2 steps:
1. "Från och med vilken dag är du sjuk?" → Idag / Igår / Annat datum
2. "Vet du när du kan jobba igen?" → Nej / Ja + datum
→ Confirmation: "Klart! Vi har meddelat din chef."

Creates a SickLeaveNotification + LeaveRequest in the backend.

---

## Task 6: Build MinSida/MinLon.razor

**Files:**
- Modify: `src/Web/Components/Pages/MinSida/Lonespecifikationer.razor` → rename to MinLon concept

Salary specs page connected to real PayrollResults data via SelfServiceApiClient.

---

## Task 7: Build MinSida/MinaArenden.razor

**Files:**
- Create: `src/Web/Components/Pages/MinSida/MinaArenden.razor`

Simple list of the employee's cases with status badges.

---

## Task 8: Build MinSida/MinProfil.razor

**Files:**
- Create: `src/Web/Components/Pages/MinSida/MinProfil.razor`

Profile page with editable contact info (email, phone, address) and emergency contacts.

---
