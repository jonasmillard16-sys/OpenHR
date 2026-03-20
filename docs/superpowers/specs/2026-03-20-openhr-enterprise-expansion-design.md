# OpenHR 2.0 — Enterprise Expansion Design

**Date:** 2026-03-20
**Status:** Approved (brainstorming complete)
**Author:** Claude Code + Human
**License:** AGPL-3.0 (all new code follows existing license)

## 1. Vision & Principles

### Goal
Expand OpenHR from a Swedish public-sector HR system into a universal HR platform that competes with Workday, SAP SuccessFactors, Oracle HCM, Dayforce, and UKG — while remaining 100% FOSS, self-hosted, and AGPL-3.0 licensed.

### Target Audience
All Swedish employers: regions, municipalities, government agencies, and private companies of all sizes.

### Core Principle: "Systemet är experten"
The system guides the user — not the other way around. Every module has built-in rule engines, automatic suggestions, and contextual help. Three configurable automation levels per rule category:

| Level | Behavior |
|-------|----------|
| **Notify** | System detects situation, sends notification. User acts manually. |
| **Suggest** | System detects, analyzes, proposes concrete action with one-click acceptance. |
| **Autopilot** | System detects, acts automatically, logs, notifies after the fact. |

Organizations choose their level per rule category. Some rules have a legal minimum level that cannot be lowered (e.g., ATL blocking is always enforced, GDPR retention is always Autopilot).

### Deployment Model
Single-tenant, self-hosted. Every organization runs its own instance via Docker Compose. No SaaS dependency, no central registry. AGPL-3.0 ensures all forks remain open.

### Architecture Approach
Modular monolith (existing pattern). New modules follow existing conventions: domain entities, EF Core config, schema-per-module, DbSet registration, seed data, API endpoints, Blazor pages.

## 2. Phase Structure

### Phase A — Adoption (parallel tracks)
Make it possible to switch to OpenHR. Three tracks built simultaneously:
1. Migration Engine (PAXml, HEROMA, SIE, multi-system)
2. Automation Framework (Notify/Suggest/Autopilot)
3. Pluggable Collective Agreements + Private Sector Pay

### Phase B — Enterprise Depth
Deepen existing modules and add new ones. Every module uses the Automation Framework:
4. Compensation Suite
5. Benefits Engine
6. Enterprise Analytics & BI
7. VMS / Contingent Workforce
8. Advanced WFM
9. Talent Marketplace & Skills Intelligence

### Phase C — Platform
Make OpenHR extensible by third parties:
10. Event Bus + Webhooks + API + Custom Objects + Marketplace

## 3. Section 1: Migration Engine

### Problem
No organization can switch to OpenHR without importing their existing data. HEROMA (CGI) dominates Swedish public sector. Personec P (Visma), Hogia, Fortnox cover the rest. Enterprise systems (Workday/SAP/Oracle) for large private companies.

### Architecture

```
MigrationWizard (UI, 6-step)
  → MigrationEngine (core: parse, map, validate, dry-run, import, report)
    → Format Adapters (pluggable per source system)
```

### New Entities (schema: `migration`)

| Entity | Purpose |
|--------|---------|
| **MigrationJob** | Import run: source, status (Created→Validating→DryRun→Importing→Complete/Failed), timestamps, created by |
| **MigrationMapping** | Field mapping per job: source field → OpenHR field, transformation rule |
| **MigrationTemplate** | Saved mapping templates per source system |
| **MigrationValidationError** | Validation error per row: row number, field, error type, original value |
| **MigrationLog** | Detailed log per imported record: entity type, status, error if any |

### Format Adapters

| Adapter | Format | Coverage |
|---------|--------|----------|
| **PAXmlAdapter** | XML (PAXml 2.0) | 75+ Swedish systems. Import + Export. LONIN/LONUT/REGISTER. All 72 time codes. XSD validation. |
| **HeromaAdapter** | CSV (semicolon) | HEROMA Oracle exports: PERSNR, FNAMN, ENAMN, ANST_FORM, KOL_AVTAL, MANLON, ENHET_KOD, FRANV_TYP, LAS_DAGAR |
| **PersonecAdapter** | CSV / SQL Server .bak | Personec P exports via Visma report tool or SQL Server backup |
| **HogiaAdapter** | CSV | Hogia Lön exports |
| **FortnoxAdapter** | CSV / API JSON | Fortnox exports |
| **SIE4iAdapter** | SIE text (type 4i) | Import accounting vouchers. Export: replace current Raindance CSV with universal SIE 4i. |
| **WorkdayAdapter** | XLSX (EIB) / XML (RaaS) | Workday Enterprise Interface Builder exports |
| **SAPAdapter** | CSV | SAP SuccessFactors Employee Export |
| **OracleHCMAdapter** | CSV / XML | Oracle HCM Extract outputs |
| **GenericCSVAdapter** | CSV (configurable) | Any system. User maps columns manually or selects template. |

### Migration Wizard UI Flow (6 steps)

1. **Select source** — dropdown with source-specific help text ("Export from HEROMA: Go to Reports → Personnel Extract → Select CSV...")
2. **Upload file(s)** — drag-and-drop, auto-detect format (XML→PAXml, CSV→analyze separator/columns)
3. **Review mapping** — system pre-fills based on source, user adjusts per field
4. **Validate + Dry-run** — full validation without writing to DB. Error report per row with correction suggestions.
5. **Import** — batch import with transaction. Progress bar. Rollback on critical error.
6. **Verification report** — imported counts per entity type, rejected rows with reasons, PDF export.

### Data Domains Imported

| Domain | PAXml | HEROMA CSV | SIE 4i | Generic CSV |
|--------|-------|------------|--------|-------------|
| Employee | `<personal>` | PERSNR/FNAMN/ENAMN | — | Yes |
| Employment | `<person>` dates/salary | ANST_FORM/STARTDAT | — | Yes |
| Organization | `<resultatenheter>` | ENHET_KOD/ENHET_NAMN | `#DIM/#OBJEKT` | Yes |
| Payroll | `<lonetransaktioner>` | LONEART_*/BRUTTOLON | `#VER/#TRANS` | Yes |
| Time/Schedule | `<tidtransaktioner>` | PASS_DATUM/STAMPL_* | — | Yes |
| Leave | `<tidtrans>` tid codes | FRANV_TYP/FRANV_FRAN | — | Yes |
| Competence | — | — | — | Yes |
| Accounting | — | — | All `#VER` | — |

### "System is the expert" features
- Auto-detect file format
- Smart mapping from pre-built templates per source system
- Validation suggestions: "Personnummer 19850115-1234 missing check digit — did you mean 19850115-1235?"
- Duplicate handling: "Employee with pnr already exists — update or skip?"
- Progressive import: import domain by domain (employees first, then employments, then payroll)

### New Routes

| Route | Purpose |
|-------|---------|
| `/admin/migration` | Migration dashboard: history, active jobs |
| `/admin/migration/ny` | Migration wizard (6 steps) |
| `/admin/migration/{id}` | Job detail: status, log, errors |
| `/admin/migration/mallar` | Mapping template library |

## 4. Section 2: Pluggable Collective Agreements

### Problem
`KollektivavtalEngine` has AB/HÖK rules hardcoded in C#. OB rates, overtime multipliers, vacation rules are constants. This locks out all non-public-sector employers.

### Design Principle
Agreements are **data, not code**. Each collective agreement is a configuration in the database. The calculation engine reads rules and applies them — same engine, different agreements.

### New Entities (schema: `agreements`)

| Entity | Purpose |
|--------|---------|
| **CollectiveAgreement** | Master: name, parties, validity period, industry sector, status |
| **AgreementOBRate** | OB rates per period: time type (evening/night/weekend/holiday), amount or percentage, valid from/to |
| **AgreementOvertimeRule** | Overtime rules: threshold, multiplier, max per week/month/year |
| **AgreementVacationRule** | Vacation rules: base days, age-based additions, earning period |
| **AgreementRestRule** | Rest rules beyond ATL: min daily rest, weekly rest, break per shift |
| **AgreementSalaryStructure** | Salary structure: salary code mapping, minimum rates per job category, salary steps |
| **AgreementWorkingHours** | Working hours: normal hours/week, flex rules, compressed schedules |
| **AgreementNoticePeriod** | Notice periods per employment duration (beyond LAS minimum) |
| **AgreementPensionRule** | Pension agreement: SAF-LO/ITP1/ITP2/KAP-KL/AKAP-KR/PA16, contribution rates per salary bracket |
| **AgreementInsurancePackage** | Insurance package: TGL, AGS, TFA, AFA, PSA — different per agreement area |
| **PrivateCompensationPlan** | Private sector: bonus, commission, stock/options, company car — rules and calculation models |

### Pre-configured Agreements (seed)

| Agreement | Parties | Sector |
|-----------|---------|--------|
| AB (Allmänna Bestämmelser) | SKR + unions | Region/municipality |
| HÖK | SKR + unions | Municipality specific |
| Teknikavtalet | Teknikföretagen + IF Metall | Manufacturing |
| Handelsavtalet | Svensk Handel + Handels | Retail |
| IT/Telekomavtalet | Almega + Unionen | IT/telecom |
| Vårdföretagaravtalet | Vårdföretagarna + Kommunal | Private healthcare |
| Transportavtalet | Biltrafikens Arbetsgivareförbund + Transport | Transport/logistics |
| HRF-avtalet | Visita + HRF | Hotel/restaurant |
| Tjänstemannaavtalet | Almega + Unionen/Akademikerförbunden | Services |
| Avtalslöst | — | Private without agreement |

### Existing Entity Changes

| Entity | Change |
|--------|--------|
| **Employment** | New FK: `CollectiveAgreementId` |
| **OrganizationUnit** | New FK: `DefaultCollectiveAgreementId` |
| **KollektivavtalEngine** | Reads rules from DB instead of hardcoded constants |
| **SalaryCode** | Linked to agreement — different agreements can have different salary codes |

### Pension Agreements

| Agreement | Covers | Rate below ceiling | Rate above ceiling | Ceiling |
|-----------|--------|--------------------|--------------------|---------|
| SAF-LO | LO workers private | 4.5% | 30% | 7.5 IBB |
| ITP1 | White-collar private (born 1979+) | 4.5% | 30% | 7.5 IBB |
| ITP2 | White-collar private (born pre-1979) | DB plan | DB plan | — |
| KAP-KL | Municipality/region (older) | 4.5% | — | — |
| AKAP-KR | Municipality/region (newer) | 6% | 31.5% | 7.5 IBB |
| PA 16 | Government | 4.5% + 2.5% | 30% | 7.5 IBB |

## 5. Section 3: Automation Framework

### Architecture

```
Domain Events → AutomationEngine → Evaluate Rules → Check Level → Execute
  NOTIFY:    Create Notification
  SUGGEST:   Create Notification + AutomationSuggestion (one-click action)
  AUTOPILOT: Execute action directly + log + notify after the fact
```

### New Entities (schema: `automation`)

| Entity | Purpose |
|--------|---------|
| **AutomationRule** | Rule definition: name, category, trigger type, conditions (JSON), action (JSON), active/inactive, MinimumLevel (for legal requirements), system rule vs custom |
| **AutomationCategory** | Grouping: name, description, icon |
| **AutomationLevelConfig** | Per organization + category: chosen level (Notify/Suggest/Autopilot) |
| **AutomationExecution** | Execution log: rule, event, result, level used, action taken, timestamp, audit link |
| **AutomationSuggestion** | Pending suggestion (Suggest level): rule, proposed action, created for (EmployeeId/ManagerId), status (Pending/Accepted/Dismissed), valid until |

### Built-in Rules (38 rules across 6 categories)

**Compliance / Labor Law (5 rules)**
- LAS warning at 10/11 months, conversion at 12 months
- ATL 11-hour rest violation (always blocks — legal requirement)
- ATL weekly rest violation

**Absence / Sickness (5 rules)**
- FK notification day 15
- Rehab trigger at 6 incidents/12 months
- Rehab trigger at 14 consecutive days
- Rehab milestone day 90
- Simple leave auto-approval (≤3 days, balance sufficient)

**Payroll / Finance (4 rules)**
- AGI generation (monthly cron)
- Payroll run reminder (day 20)
- SIE export on payroll completion
- Minimum salary warning per agreement

**Competence / Certification (3 rules)**
- Certification expiry 90/30 day reminders
- Overdue mandatory training reminder

**Recruitment / Onboarding (3 rules)**
- Auto-start onboarding journey on new employment
- Probation expiry reminder (30 days before)
- Auto-start offboarding on termination date set

**GDPR / Retention (2 rules)**
- Retention cleanup (nightly cron, always Autopilot)
- DSR deadline escalation (25 days without response)

### Legal minimum levels (cannot be lowered)
- ATL violations: always BLOCK
- FK notification day 15: minimum SUGGEST
- GDPR retention cleanup: always AUTOPILOT
- LAS conversion day 365: minimum NOTIFY

### Replaces existing background services
| Current Service | Becomes |
|-----------------|---------|
| `LASAlertService` | Compliance → LAS rules |
| `CertificationReminderService` | Competence → Certification rules |
| `NotificationReminderService` | Absence → Sickness rules |
| `RetentionCleanupService` | GDPR → Retention rules |

### New Routes

| Route | Purpose |
|-------|---------|
| `/admin/automation` | Category list with level selector per category |
| `/admin/automation/{category}` | Rules in category with individual level adjustment |
| `/admin/automation/logg` | Execution log filtered by category, level, time |
| `/admin/automation/forslag` | Pending suggestions (Suggest level) |

## 6. Section 4: Compensation Suite

### New Entities (schema: `compensation`)

| Entity | Purpose |
|--------|---------|
| **CompensationPlan** | Annual plan: name, validity, total budget, status (Draft→Active→Closed) |
| **CompensationBand** | Salary band per job category: min/target/max, steps, experience-based |
| **CompensationBudget** | Budget allocation per org unit: total allowance, distributed, remaining |
| **CompensationGuideline** | Guidelines per plan: recommended raise % per performance level, max raise, priority groups |
| **BonusPlan** | Bonus program: name, type (individual/group/company), calculation model, payout timing |
| **BonusTarget** | Bonus target per employee/group: KPI target, weight, threshold, cap |
| **BonusOutcome** | Actual outcome per target: result value, calculated amount, status (Pending→Calculated→Approved→Paid) |
| **VariablePayComponent** | Commission, on-call, standby: calculation rule, link to time data |
| **TotalRewardsStatement** | Generated annual summary per employee: all components totaled |
| **CompensationSimulation** | Saved modeling: parameters, calculated result, comparison vs current |

### Compensation Cycle Workflow
1. HR creates CompensationPlan with total budget and guidelines
2. Budget distributed per org unit (top-down or bottom-up)
3. Managers propose individual raises (system shows band position, performance, market comparison)
4. Union review (SalaryReviewRound.FackligAvstemning)
5. HR approves (validates total budget, no band violations, discrimination analysis)
6. Implementation (salary changes applied, retroactive calculation if needed, AGI updated)
7. Total Rewards Statements generated automatically

### Compensation Modeling (`/lon/modellering`)
Scenario simulation: "What does it cost to raise all nurses to band target?"
- Shows: affected count, average raise, total annual cost + AG contributions, budget remaining, wage mapping impact

### New Routes

| Route | Purpose |
|-------|---------|
| `/lon/modellering` | Compensation modeling/simulation |
| `/lon/band` | Salary band management per job category |
| `/lon/bonus` | Bonus plans and outcomes |
| `/minsida/totalrewards` | Employee's Total Rewards Statement |

## 7. Section 5: Benefits Engine

### New Entities (schema: `benefits`)

| Entity | Purpose |
|--------|---------|
| **BenefitPlan** | Benefit offering: name, type, validity, budget per person, tax rules, provider |
| **EligibilityRule** | Eligibility rules per plan: conditions that must be met |
| **EligibilityCondition** | Individual condition (owned by rule): field, operator, value |
| **LifeEvent** | Life event definition: type, allowed benefit changes, time window |
| **LifeEventOccurrence** | Registered event per employee: type, date, status, linked actions |
| **EnrollmentPeriod** | Open enrollment window: name, start, end, included plans |
| **BenefitEnrollment** | Enrollment per employee per plan: status (Pending→Active→Cancelled), start date, chosen level |
| **BenefitStatement** | Annual summary per employee: generated PDF, all active benefits with value |
| **BenefitTransaction** | Transaction: claim per employee per plan (generalizes WellnessClaim) |

### Eligibility Rule Engine
Condition types: AnstallningsForm, Sysselsattningsgrad, Anstallningstid, Alder, Befattningskategori, CollectiveAgreement, OrganizationUnit, CustomField.
Operators: IN, NOT_IN, >=, <=, =, BETWEEN.
Combination: AND (all must be met).

### Life Events
Nyanställning (30d window), Marriage/cohabitation (30d), Child born/adopted (60d), Divorce (30d), Role change (30d), Employment % change (immediate), Agreement change (30d), Age 65 (automatic), Termination (immediate).

### Open Enrollment
Annual window. System notifies all eligible. Shows current selections + available alternatives with real-time net salary effect. Unchanged selections roll over. Autopilot: reminders at 7/3/1 days before close.

### New Routes

| Route | Purpose |
|-------|---------|
| `/formaner/val` | Open enrollment / benefit selection |
| `/formaner/livshandelse` | Register life event |
| `/formaner/sammanstallning` | Benefit statement |
| `/admin/formaner/planer` | Manage benefit plans |
| `/admin/formaner/regler` | Manage eligibility rules |
| `/admin/formaner/perioder` | Manage enrollment periods |

## 8. Section 6: Enterprise Analytics & BI

### New Entities (schema: `analytics`)

| Entity | Purpose |
|--------|---------|
| **KPIDefinition** | KPI definition: name, category, calculation formula, unit, direction, thresholds |
| **KPISnapshot** | Calculated value at point in time: KPI, period, value, comparison, trend |
| **KPIAlert** | KPI alarm: KPI, threshold, recipients, linked to AutomationEngine |
| **PredictionModel** | Predictive model: name, type, input parameters, last training date, accuracy |
| **PredictionResult** | Prediction per employee/unit: model, score, risk level, contributing factors |
| **ReportTemplate** | Report template for self-service: columns, filters, grouping, visualization type |
| **ScheduledReport** | Scheduled report: template, frequency (daily/weekly/monthly), recipients, format |
| **BenchmarkDataset** | Benchmark data per industry/region: KPI, value, source, period |

### KPI Library (60+ metrics across 8 categories)
- **Workforce** (10): Headcount, FTE, vacancy rate, internal recruitment rate, avg tenure, age distribution, gender ratio, part-time ratio, fixed-term ratio, manager density
- **Turnover** (8): Total turnover, voluntary attrition, unwanted attrition, new hire rate, 90-day retention, 1-year retention, time to fill, flight risk score
- **Absence/Health** (8): Sick leave %, short-term, long-term, frequency, rehab cases/100, wellness utilization, incidents/100, avg rehab duration
- **Compensation** (10): Total labor cost, cost/FTE, salary spread P10/P50/P90, gender gap, overtime cost, OB cost, cost vs budget, cost per hire, AG contributions, pension cost
- **Competence** (6): Skills gap %, certification coverage, expiring certs, training hours/employee, mandatory training completion, LMS completion rate
- **Recruitment** (6): Time to fill, applications/vacancy, interview-to-offer, offer acceptance, talent pool size, cost per hire
- **Engagement** (6): eNPS, pulse response rate, avg pulse score, recognition frequency, review completion, 360 participation
- **Compliance** (6): LAS risk count, ATL violations, open GDPR cases, avg GDPR handling time, policy acknowledgment coverage, active MBL cases

### Predictive Models (4, all local — no external AI)
1. **Attrition Prediction**: Logistic regression on tenure, salary band position, absence trend, pulse score, time since promotion, manager change. Score 0-100 + top 3 factors.
2. **Headcount Forecast**: Time series (exponential smoothing + known events) on 24-month history + known retirements + active recruitments + budget.
3. **Sick Leave Forecast**: Seasonal regression on historical absence + season + pulse stress indicators + incidents + staffing level.
4. **Labor Cost Forecast**: Bottom-up calculation from current salaries + agreement increases + planned hires + known departures + OB/overtime trend.

### Self-Service Report Builder (`/rapporter/bygg`)
Data sources: Employees, Employments, Payroll, Absence, Competence, Time, Recruitment, Benefits.
User selects columns, filters, grouping, sorting, visualization (table/bar/line/pie). Save as template, export CSV/Excel/PDF.

### Dashboard Drill-Down
Click KPI card → per unit → per job category → per individual.

### New Routes

| Route | Purpose |
|-------|---------|
| `/rapporter/bygg` | Self-service report builder |
| `/rapporter/kpi` | KPI library with snapshots and trends |
| `/rapporter/prediktion` | Predictive analytics dashboard |
| `/rapporter/benchmark` | Benchmark comparison |
| `/admin/rapporter/schema` | Scheduled report management |

## 9. Section 7: VMS / Contingent Workforce

### New Entities (schema: `vms`)

| Entity | Purpose |
|--------|---------|
| **Vendor** | Staffing agency: name, org number, contact, category, status (Active/Inactive/Blocked) |
| **FrameworkAgreement** | Framework agreement: vendor, validity, terms, notice period, extension clause |
| **RateCard** | Price list per agreement: job category, hourly rate, OB markup, overtime markup, VAT |
| **StaffingRequest** | Order for contingent staff: unit, role, period, count, requirements, status (Draft→Submitted→Approved→Filled→Closed) |
| **ContingentWorker** | Contingent person: name, vendor, request, start/end date, hourly cost, unit |
| **ContingentTimeReport** | Time report per contingent: period, hours, OB hours, overtime, attested by, status |
| **VendorInvoice** | Vendor invoice: vendor, period, amount, matched against time reports, status (Received→Matched→Approved→Paid) |
| **VendorPerformance** | Vendor rating: vendor, period, delivery time, quality, availability, score 1-5 |
| **SpendCategory** | Cost category: staffing nurse, IT consultant, construction, etc. |

### Key Design Decision
**ContingentWorker ≠ Employee**. Contingent workers have no personnummer in the system, no Employment, no salary, no leave. They appear in staffing views but in a separate section marked "Inhyrd personal". They are NOT counted in headcount KPIs but ARE counted in total workforce cost.

### Compliance
- **Uthyrningslagen (2012:854)**: Equal conditions after 6 months. System counts days, warns at 5 months.
- **LOU**: Framework agreements must be used in ranked order. System ranks vendors per agreement.
- **Information obligation**: Contingent workers must be informed of vacancies. Vacancy notifications sent to active ContingentWorkers.

### New Routes

| Route | Purpose |
|-------|---------|
| `/vms` | Overview: active contingent, total cost, deviations |
| `/vms/leverantorer` | Vendor registry with performance scores |
| `/vms/ramavtal` | Framework agreements with rate cards |
| `/vms/bestallningar` | Staffing requests with status flow |
| `/vms/tidrapporter` | Contingent time report attestation |
| `/vms/fakturor` | Invoice matching and approval |
| `/vms/statistik` | Spend analytics, vendor comparison |

## 10. Section 8: Advanced WFM

### New Entities (schema: `scheduling`)

| Entity | Purpose |
|--------|---------|
| **DemandForecast** | Demand forecast per unit per day: calculated headcount needed, hours, confidence |
| **DemandPattern** | Historical pattern: unit, weekday, time, average load, seasonal variation |
| **DemandEvent** | Known future event affecting demand: type (holiday, event, flu season), impact |
| **SchedulingConstraint** | Constraint: type (ATL/agreement/competence/preference/cost), weight, hard/soft |
| **ShiftCoverageRequest** | Coverage need on absence: shift to cover, status (Open→Offered→Covered→Uncovered) |
| **EmployeeAvailability** | Availability per employee: day/time, preference (want/can/cannot), repetition |
| **FatigueScore** | Fatigue assessment per employee: score 0-100, contributing factors, last calculated |
| **SchedulingRun** | Optimization run: parameters, result, comparison vs manual schedule, status |

### Five Capability Layers
1. **Demand Forecasting**: Historical patterns + seasonal adjustment + known events + absence buffer → recommended staffing per unit per day
2. **Skill-Based Scheduling**: Match PositionSkillRequirement + Certification to available staff per shift
3. **Cost Optimization**: Minimize total cost (base + OB + overtime). Prioritize regular hours, spread evening/night evenly, use part-timers for gaps, overtime as last resort, contingent (VMS) as final fallback.
4. **Absence Coverage**: On sick call → identify shift → filter qualified available staff (competence + ATL + fatigue) → rank by cost/fairness/fatigue → assign (per automation level).
5. **Real-Time Compliance + Fatigue**: Continuous ATL validation on every schedule change. Fatigue score: consecutive days (+2/day after 5), night shifts (+8), hours >40/week (+1/hour), short rest <13h (+5), weekend work (+3). Thresholds: 0-30 green, 31-60 yellow, 61-80 orange, 81-100 red.

### Healthcare-Specific Rules
- Max 3 consecutive night shifts (patient safety)
- Forward-rotating schedules (day→evening→night, never backwards)
- No solo night for new employees (<6 months)
- ≥48h recovery after night block

### New Routes

| Route | Purpose |
|-------|---------|
| `/schema/realtid` | Real-time operational view per unit |
| `/schema/optimering` | Optimization run: compare auto vs manual |
| `/schema/prognos` | Demand forecast visualization |
| `/schema/tillganglighet` | Employee availability management |

## 11. Section 9: Talent Marketplace & Skills Intelligence

### New Entities (schema: `competence`)

| Entity | Purpose |
|--------|---------|
| **SkillCategory** | Skill category: Clinical, Technical, Leadership, Communication, Regulatory |
| **SkillRelation** | Relation between skills: prerequisite, related, supersedes |
| **InferredSkill** | Inferred skill: employee, skill, source (role/course/cert/experience), confidence 0-100, confirmed yes/no |
| **CareerPath** | Career path: name, steps in order, industry |
| **CareerPathStep** | Step in path (owned): position/title, typical time, required skills, required experience |
| **DevelopmentPlan** | Individual development plan: employee, target role, status (Draft→Active→Completed), timeline |
| **DevelopmentMilestone** | Milestone in plan (owned): description, type (skill/cert/course/experience), target date, status |
| **InternalOpportunity** | Internal opportunity: type (Role/Project/Gig/Mentorship/Rotation), title, unit, period, requirements, status |
| **OpportunityApplication** | Interest application: employee, opportunity, motivation, match score, status |
| **MentorRelation** | Mentorship: mentor, mentee, focus area, start date, status, meeting frequency |
| **SkillEndorsement** | Skill endorsement: skill, employee, endorsed by (colleague/manager), date |

### Skills Intelligence
- **Skill graph**: Skills have relationships (prerequisite, related, supersedes)
- **Skills inference**: Automatically derive skills from job history, completed courses, certifications, tenure. Confidence score 0-100. Manager/employee confirms or rejects.
- **Endorsements**: Colleagues and managers endorse skills, increasing verification.

### Internal Talent Marketplace
Opportunity types: Role (permanent), Project (1-12 months), Gig (1-8 weeks), Mentorship (6-12 months), Rotation (3-6 months).
MatchingEngine scores: Competence match (40%), Interest match (25%), Performance (20%), Availability (15%).

### Career Engine
Pre-configured career paths (seeded per industry). Readiness score 0-100% per employee per target role:
```
ReadinessScore = SkillMatch×0.40 + ExperienceMatch×0.25 + CertMatch×0.25 + PerformanceBonus×0.10
```

### New Routes

| Route | Purpose |
|-------|---------|
| `/karriar` | Internal talent marketplace: all open opportunities with match scores |
| `/karriar/minutveckling` | My career view: readiness, development plan, milestones |
| `/karriar/mentorskap` | Find mentor / my mentorships |
| `/kompetens/endorsements` | Endorse colleagues' skills |
| `/admin/karriarvagar` | Manage career paths |
| `/admin/talangmarknad` | Publish internal opportunities |

## 12. Section 10: Extensibility & Platform

### Lager 1: Event Bus + Webhooks + API (Phase A/B)

**New Entities (schema: `platform`)**

| Entity | Purpose |
|--------|---------|
| **DomainEvent** | Published event: type, aggregate, data (JSON), timestamp, correlation ID |
| **EventSubscription** | Webhook subscription: URL, secret (HMAC), filter (event types), status, retry config |
| **EventDelivery** | Delivery log: subscription, event, status (Pending→Delivered→Failed), HTTP status, attempts, next retry |
| **ApiKey** | API key: name, key (hashed SHA-256), scope (read/write per module), expiry, created by |

**Event catalog**: ~50 event types across all modules (employee.*, employment.*, leave.*, payroll.*, scheduling.*, recruitment.*, case.*, compliance.*, competence.*, benefits.*, vms.*, compensation.*, document.*, automation.*).

**Webhook delivery**: HMAC-SHA256 signed, exponential backoff (1min→5min→30min→2h→12h), max 5 attempts, auto-pause after 10 consecutive failures.

**API expansion**: OpenAPI/Swagger spec at `/api/docs`. API key auth with per-key scope. Rate limiting per key (configurable, default 1000 req/min).

### Lager 2: Custom Objects & Workflows (Phase C)

**New Entities**

| Entity | Purpose |
|--------|---------|
| **CustomObject** | Object definition: name, fields (JSON schema), relations, icon |
| **CustomObjectRecord** | Instance: object definition, data (JSONB), created by, timestamps |
| **CustomObjectRelation** | Relation to core entity: type (one-to-many, many-to-many), source entity |
| **WorkflowStep** | Step in visual workflow (extends existing WorkflowDefinition): type (Approval/Notification/FieldUpdate/ExternalCall/Condition), config |
| **WorkflowExecution** | Workflow execution: definition, current step, status, data, history |

Custom objects get auto-generated CRUD views, API endpoints, webhook events, and search/filter/export.

### Lager 3: Marketplace (Phase C)

**New Entities**

| Entity | Purpose |
|--------|---------|
| **Extension** | Extension package: name, version, author, description, type, package file |
| **ExtensionInstallation** | Installation: extension, version, install date, status (Active/Disabled), config |
| **ExtensionRating** | Community rating: extension, score 1-5, comment |

Package format: `.openhr` ZIP with `manifest.json` + definitions. Distributed via Git (no central registry — FOSS principle). Categories: Custom Objects, Workflows, Report Templates, Integration Adapters.

### New Routes

| Route | Purpose |
|-------|---------|
| `/admin/webhooks` | Webhook subscription management + delivery log |
| `/admin/api` | API key management + usage stats |
| `/admin/custom-objects` | Custom object definitions |
| `/custom/{objectName}` | Auto-generated CRUD view per custom object |
| `/admin/workflows` | Visual workflow editor |
| `/admin/marketplace` | Extension catalog + import/install |

## 13. Summary

### Entity Count

| Phase | New Entities | New Routes | Dependencies |
|-------|-------------|------------|--------------|
| A: Migration | 5 | ~4 | — |
| A: Agreements | 11 | ~3 | — |
| A: Automation | 5 | ~4 | — |
| B: Compensation | 10 | ~4 | A2, A3 |
| B: Benefits | 9 | ~6 | A2, A3 |
| B: Analytics | 8 | ~5 | A3 |
| B: VMS | 9 | ~7 | A3 |
| B: WFM | 8 | ~4 | A2, A3 |
| B: Talent | 11 | ~6 | A3 |
| C: Extensibility | 10 | ~6 | A3 |
| **Total** | **86** | **~49** | |

### Grand Totals After Expansion

| Metric | Current | After |
|--------|---------|-------|
| Entities | ~77 | ~163 |
| Pages | ~98 | ~147 |
| Modules | 32 | ~38 |
| Integration formats | 14 | 24+ |
| KPIs | ~10 | 60+ |
| Automation rules | 0 (hardcoded) | 38+ (configurable) |
| API endpoint groups | 25 | 35+ |
| Collective agreements | 2 (hardcoded) | 10+ (data-driven) |

## 14. Out of Scope

Explicitly not included in this design:
1. **Real BankID/SITHS authentication** — requires external identity provider
2. **Native mobile app** — PWA covers mobile use cases
3. **Live external integrations** — FK, AD/Entra, Platsbanken, SCB live, bank payments
4. **SignalR real-time push** — infrastructure exists but not activated
5. **Global HCM / multi-country** — future expansion, not current priority
6. **AI/LLM assistant (D-level)** — future layer on top of Autopilot
7. **Multi-tenant SaaS** — single-tenant by design, self-hosted

## 15. Migration from Existing Entities & Conventions

This section addresses how new entities relate to existing ones, resolving naming collisions, duplication, and migration strategies.

### 15.1 Entity Modeling Convention

New entities follow two patterns based on their role:

**Aggregate Roots** (get strongly-typed IDs, private setters, factory methods):
- `CollectiveAgreement` → `CollectiveAgreementId`
- `MigrationJob` → `MigrationJobId`
- `AutomationRule` → `AutomationRuleId`
- `CompensationPlan` → `CompensationPlanId`
- `BenefitPlan` → `BenefitPlanId`
- `Vendor` → `VendorId`
- `CareerPath` → `CareerPathId`
- `DevelopmentPlan` → `DevelopmentPlanId`
- `InternalOpportunity` → `InternalOpportunityId`
- `CustomObject` → `CustomObjectId`
- `Extension` → `ExtensionId`
- `DemandForecast` → `DemandForecastId`
- `SchedulingRun` → `SchedulingRunId`
- `BonusPlan` → `BonusPlanId`
- `FrameworkAgreement` → `FrameworkAgreementId`
- `StaffingRequest` → `StaffingRequestId`

All strongly-typed IDs follow the existing `readonly record struct` pattern with EF Core ValueConverters registered in `ConfigureConventions`.

**Simple entities** (plain Guid Id, used as children or value-like records):
- All `Agreement*` rate/rule entities (owned by CollectiveAgreement)
- `MigrationMapping`, `MigrationValidationError`, `MigrationLog`
- `AutomationExecution`, `AutomationSuggestion`
- `KPISnapshot`, `EventDelivery`
- `BenefitTransaction`, `ContingentTimeReport`
- `SkillEndorsement`, `OpportunityApplication`

### 15.2 Critical: Benefits Entity Migration (C1)

**Existing entities:**
- `Benefit` (schema `benefits`) — name, description, category enum (Friskvard/Pension/Forsakring/etc), `EligibilityRegler` JSON column
- `EmployeeBenefit` — links employee to benefit, has `LivshandardAnledning`, `EnrollmentStatus` enum

**Migration strategy:**
1. `Benefit` is **renamed to `BenefitPlan`** via EF migration (table rename, not drop+create). The existing `EligibilityRegler` JSON column becomes the seed for the new `EligibilityRule` entities — during migration, JSON rules are parsed into proper `EligibilityRule` + `EligibilityCondition` records.
2. `EmployeeBenefit` is **renamed to `BenefitEnrollment`**. The existing `EnrollmentStatus` enum is kept and extended.
3. `WellnessClaim` is **kept as-is** but reclassified as a specific `BenefitTransaction` subtype. Over time, `BenefitTransaction` generalizes the pattern.
4. The `BenefitCategory` enum moves from being on `Benefit` to being on `BenefitPlan`.
5. No data loss — all existing seed data and runtime data survives the rename.

### 15.3 Critical: Analytics/Reporting Entity Reconciliation (C2)

**Existing entities:**
- `analytics` schema: `SavedReport` (named queries with visualization), `Dashboard` (widget layout)
- `reporting` schema: `ReportDefinition` (SQL template with `CronExpression`, `ArSchemalagd`, `MottagareEpost`), `ReportExecution` (cached results)

**Migration strategy:**
1. `ReportTemplate` (from spec) **replaces `ReportDefinition`** — same table, extended with self-service builder metadata (columns, filters, grouping, visualization type). Migration adds new columns.
2. `ScheduledReport` is **extracted from `ReportDefinition`** — the `CronExpression`, `ArSchemalagd`, `MottagareEpost` fields move to a dedicated `ScheduledReport` entity that references a `ReportTemplate`. This is a proper normalization.
3. `SavedReport` and `Dashboard` are **kept as-is** — they serve different purposes (user-saved ad-hoc queries vs. KPI dashboards).
4. New entities (`KPIDefinition`, `KPISnapshot`, `PredictionModel`, `PredictionResult`, `BenchmarkDataset`) are purely additive — no collision.

### 15.4 Critical: Competence SkillCategory Collision (C3)

**Existing:**
- `Skill` entity has a `SkillCategory` enum property (values: Klinisk, Teknisk, Ledarskap, Administration)
- `EmployeeSkill`, `PositionSkillRequirement` reference `Skill`

**Migration strategy:**
1. The `SkillCategory` **enum is replaced by a `SkillCategory` entity** (new table in `competence` schema). Migration: create table, seed rows matching existing enum values (Klinisk, Teknisk, Ledarskap, Administration, plus new: Kommunikation, Regulatorisk), add FK column to `Skill`, populate FK from existing enum, drop enum column.
2. `Skill`, `EmployeeSkill`, `PositionSkillRequirement` are **kept as-is** — all new entities (`InferredSkill`, `SkillRelation`, `SkillEndorsement`) reference the existing `Skill` entity.
3. `InferredSkill` links to `EmployeeSkill` — when confirmed, it creates/updates an `EmployeeSkill` record.

### 15.5 Critical: Employment CollectiveAgreement FK (C4)

**Existing:**
- `Employment` has `Kollektivavtal` property stored as string column (values: "AB", "HOK", "MBA", "PAN", "None")

**Migration strategy:**
1. Create `CollectiveAgreement` seed records matching existing enum values
2. Add `CollectiveAgreementId` FK column to `Employment`
3. Populate FK from existing string values: "AB" → AB record, "HOK" → HÖK record, etc.
4. Keep `Kollektivavtal` string column as read-only computed property (for backward compatibility in reports/exports) for one release cycle, then remove.
5. `OrganizationUnit` gets `DefaultCollectiveAgreementId` FK (new column, nullable).

### 15.6 Important: SalaryCode Agreement Linkage (I1)

`SalaryCode` currently uses `Kod` (string) as natural key. Strategy:
- Add `CollectiveAgreementId` FK (nullable — salary codes can be agreement-agnostic)
- Many-to-many not needed: a salary code belongs to one agreement or is universal (null FK)
- `SalaryCode` keeps its current structure; no strongly-typed ID needed (it's a reference entity, not an aggregate)

### 15.7 Important: Domain Event Dispatch Infrastructure (I3)

The automation framework requires domain event dispatch. The existing codebase has `RaiseDomainEvent()` on aggregates and `ClearDomainEvents()` but no dispatcher. Required new infrastructure:

1. `IDomainEventDispatcher` interface in SharedKernel
2. `DomainEventDispatcher` implementation that routes events to `AutomationEngine`
3. `SaveChanges` override in `RegionHRDbContext` (or `SaveChangesInterceptor`) that:
   - Collects domain events from all changed aggregates before save
   - Saves changes
   - Dispatches events after successful save
4. This is a prerequisite for Phase A and must be built first within the Automation Framework track.

### 15.8 Important: WorkflowStep Name Collision (I5)

Existing `WorkflowStep` class exists in `CaseManagement/Workflows/WorkflowEngine.cs` as a simple POCO.
New platform workflow step entity is **renamed to `WorkflowNode`** to avoid collision.
`WorkflowExecution` renamed to `WorkflowRunInstance`.

### 15.9 Important: WFM Overlap with Existing Scheduling (I7)

The existing `ConstraintScheduleSolver` with ATL validation, backtracking, and fairness scoring becomes the **core algorithm behind `SchedulingRun`**. `SchedulingRun` persists the inputs (constraints, demand forecast, availability) and outputs (generated shifts, cost analysis, compliance report) of `Solve()`. The solver is extended with:
- Demand forecast input (from `DemandForecast`)
- Fatigue constraints (from `FatigueScore`)
- Competence matching (from `PositionSkillRequirement`)

`FatigueScore` is recalculated in real-time after each schedule modification, and verified on a nightly batch for drift correction.

### 15.10 Important: Pension Plan Detail (I4)

ITP2 and PA 16 are complex defined-benefit plans that cannot be fully specified in a rate table. These receive separate calculation modules:
- `ITP2Calculator` — implements the actual DB formula: 10% of salary ≤7.5 IBB, 65% between 7.5-20 IBB, 32.5% between 20-30 IBB
- `PA16Calculator` — implements avd. I (DC: 4.5% + individual 2.5%) and avd. II (DB with complex rules)
- Both are seeded as `AgreementPensionRule` entries with `CalculationModel = "ITP2"` / `"PA16"` that route to the dedicated calculators rather than using simple percentage rates.

### 15.11 Suggestion: ML Library

All predictive models use **ML.NET** (MIT license, ships with .NET). No external AI dependencies. This maintains the FOSS principle.

### 15.12 Suggestion: Entity Count Correction

Corrected count for Phase C (Extensibility): 12 entities (not 10). Grand total: **88 new entities** (not 86). Updated totals: ~77 existing + 88 new = **~165 entities**.

### 15.13 Suggestion: Route Naming Convention

User-facing routes use Swedish (consistent with existing codebase). Admin/platform routes may use English for technical terms with no natural Swedish equivalent:
- `/admin/webhooks` (kept — "webbhookar" is awkward)
- `/admin/api-nycklar` (Swedish)
- `/admin/tillagg` (Swedish for marketplace/extensions)
- `/custom/{objectName}` → `/anpassat/{objektNamn}` (Swedish)

### 15.14 Suggestion: YAGNI Removals

- `BenchmarkDataset` — **deferred to Phase C**. Requires external data source with no clear provider for self-hosted.
- `ExtensionRating` — **removed**. No central aggregation point in FOSS/self-hosted model. Re-add if community repo emerges.
- `VendorPerformance` — **kept but simplified**. Manual rating only (no auto-scoring), 1-5 score + comment. Auto-scoring deferred.

## 16. Implementation Order

Phase A tracks can be built in parallel. Phase B modules should be built after Phase A's automation framework is in place (so all new modules use it from day one). Phase C should wait until Phase B modules are mature.

### Phase A (parallel)
1. **Domain Event Dispatch infrastructure** (prerequisite — see 15.7)
2. **Automation Framework** (Section 3) — replaces existing background services
3. **Migration Engine** (Section 1) — PAXml adapter first, then HEROMA, then others
4. **Collective Agreements** (Section 2) — includes Employment FK migration (15.5)

### Phase B (sequential, with dependencies)
1. **Analytics** (Section 6) — provides KPI infrastructure for other modules. Includes ReportDefinition migration (15.3).
2. **Compensation** (Section 4) — builds on agreements module
3. **Benefits** (Section 5) — includes Benefit→BenefitPlan migration (15.2). Builds on agreements + compensation.
4. **VMS** (Section 7) — independent, but built before WFM so WFM can use contingent fallback
5. **WFM** (Section 8) — builds on agreements, analytics, VMS. Includes SchedulingRun integration (15.9).
6. **Talent** (Section 9) — builds on competence (includes SkillCategory migration 15.4), analytics

### Phase C (after Phase B is mature)
1. **Event Bus + Webhooks + API** (Section 10 Layer 1)
2. **Custom Objects + Workflows** (Section 10 Layer 2)
3. **Marketplace** (Section 10 Layer 3)
