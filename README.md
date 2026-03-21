# OpenHR

**Open-source HR-system för svenska regioner och kommuner.**

OpenHR är ett personalhanteringssystem byggt för att ersätta HEROMA och andra proprietära HR-system inom svensk offentlig sektor. Byggt med öppen källkod (AGPL-3.0), svensk arbetsrätt, kollektivavtal och GDPR.

> **OpenHR 2.0** — 36 moduler, ~157 sidor, 160+ domänentities, 976 tester. Se [2.0 Expansion](#20-expansion) nedan.

## Funktionsstatus

### Beräkningsmotorer (production-ready)
Riktiga beräkningar med korrekt svensk lagstiftning:

- **SwedishTaxCalculator** — kommunalskatt, statlig skatt, arbetsgivaravgift (31,42%), reducerad avgift 66+
- **KollektivavtalEngine** — OB-tillägg (kväll/natt/helg/storhelg), viloregler, övertid, semester per ålder (AB/HOK)
- **TraktamentsCalculator** — inrikes/utrikes traktamente enligt Skatteverkets regler 2026
- **SchemaOptimizer** — round-robin-tilldelning med balansindex
- **Integrationsformat** — AGI-XML (Skatteverket), pain.001 (Nordea ISO 20022), CSV-export

### Kärnmoduler (DB-backed, domänlogik)
Fullständig datamodell med entities, domänmetoder, EF Core-konfiguration, migrationer och seed:

| Modul | Entities | Domänflöde | Routes |
|-------|----------|------------|--------|
| **Personalregister** | Employee, Employment, OrganizationUnit | Skapa, anställ, organisationsträd | `/anstallda`, `/anstallda/ny`, `/organisation` |
| **Ledighet** | LeaveRequest, VacationBalance, SickLeaveNotification | Skapa→SkickaIn→Godkänn/Avvisa | `/ledighet/*` (7 routes) |
| **Lön/Payroll** | PayrollRun, PayrollResult, PayrollResultLine, SalaryCode | Skapa→Paborja→LaggTillResultat→Beraknad→Godkand→Utbetald | `/lon/*` (5 routes) |
| **Schema/Tid** | Schedule, ScheduledShift, Timesheet, TimeClockEvent, ShiftSwapRequest, StaffingTemplate | Schema→Pass, Stämpla In/Ut, Tidrapport→Godkänn | `/schema/*`, `/tidrapporter/*`, `/stampling` |
| **Ärenden** | Case, CaseApproval, CaseComment | SkapaFranvaroarende→Godkänn | `/arenden/*`, `/godkannanden` |
| **MBL** | MBLNegotiation | Skapa→Paborja→Avsluta→RegistreraProtokoll | `/arenden/mbl` |
| **LAS** | LASAccumulation, LASPeriod, LASEvent | Perioder, statusberäkning, företrädesrätt | `/las` |
| **HälsoSAM/Rehab** | RehabCase, RehabUppfoljning | Skapa, milstolpar (dag 14/90/180/365) | `/halsosam/*` |
| **Kompetens** | Skill, EmployeeSkill, PositionSkillRequirement, Certification, MandatoryTraining | Gap-analys, certifieringsstatus | `/kompetens/*` |
| **Medarbetarsamtal** | PerformanceReview | Skapa→Självbedömning→Chefsbedömning→Genomförd | `/medarbetarsamtal/*` |
| **360-feedback** | FeedbackRound, FeedbackResponse | Skapa→Oppna→Stang, betyg 1–5 | `/medarbetarsamtal/360` |
| **Pulsundersökning** | PulseSurvey, PulseSurveyQuestion, PulseSurveyResponse, PulseSurveyAnswer | Skapa→Oppna→Svara→Stang. Anonyma svar. | `/admin/pulsundersokning/*` |
| **Policyer** | Policy, PolicyConfirmation | Skapa→Publicera→Arkivera, bekräftelse per anställd | `/dokument/policyer/*` |
| **Dokument** | Document, DocumentTemplate | Filuppladdning, metadata, mallgenerering | `/dokument/*` |
| **Notiser** | Notification, NotificationTemplate, NotificationPreference | Skapa, MarkAsRead, personliga preferenser i DB | `/notiser/*` |
| **Positioner** | Position, PositionHistorik, HeadcountPlan | Skapa→Tillsatt/Vakant/Frys | `/positioner` |
| **Successionsplanering** | SuccessionPlan | Position→Kandidat, beredskapsnivå | `/admin/succession` |
| **Rekrytering** | Vacancy, Application, OnboardingChecklist, Scorecard, InterviewSchedule, ReferenceCheck | Publicera→TaEmotAnsokan→Pipeline→Tillsatt | `/rekrytering/*` |
| **Resor** | TravelClaim, ExpenseItem | Skapa→SattTraktamente→SkickaIn→Attestera | `/resor` |
| **Offboarding** | OffboardingCase, OffboardingItem | Skapa (auto 8 steg)→MarkeraSomPagar→Slutfor | `/offboarding/*` |
| **Löneöversyn** | SalaryReviewRound, SalaryProposal | Skapa→FackligAvstemning→Godkand→Genomford | `/loneoversyn` |
| **Benefits** | Benefit, EmployeeBenefit | Anmala→Godkann | `/formaner/*` |
| **Friskvård** | WellnessClaim | Skapa→Godkann/Avvisa, max 5000 kr/år | `/formaner/friskvard` |
| **Försäkringar** | InsuranceCoverage | TGL, AGS, TFA, AFA, PSA | `/formaner/forsakringar` |
| **Anslagstavla** | Announcement | Skapa→Publicera→Arkivera, prioritetsnivåer | `/admin/anslagstavla` |
| **Peer Recognition** | Recognition | Ge beröm till kollega med kategori | `/admin/berom` |
| **Delegering** | DelegatedAccess | Skapa→ArGiltig→Avsluta | `/admin/delegering` |
| **E-learning** | Course, CourseEnrollment, LearningPath | Anmala→Paborja→Genomford | `/utbildning/*` |
| **GDPR** | DataSubjectRequest, RetentionRecord | Skapa→Tilldela→Slutfor, registerutdrag | `/gdpr` |
| **Audit** | AuditEntry | Create/Update/Delete-logg | `/audit` |
| **Talangpool** | TalentPoolEntry | Kandidater för framtida rekrytering | `/rekrytering/talangpool` |
| **Flight Risk** | (beräknad tjänst) | 4 signaler: tenure, anställningsform, bristyrke, deltid | `/rapporter/flight-risk` |
| **Workforce Planning** | HeadcountPlan | Budget per enhet per år | `/rapporter/workforce-plan` |
| **Provisionering** | ProvisioningRule, ProvisioningEvent | Lokal registrering (ej extern AD/SCIM) | `/admin/provisionering` |
| **Journeys** | JourneyTemplate, JourneyInstance | Onboarding/offboarding-mallar med steg | `/journeys/*` |
| **Migreringsmotor** | MigrationProject, MigrationMapping, MigrationRun, m.fl. | PAXml 2.0, HEROMA, Personec P, Hogia, Fortnox, SIE 4i, Workday, SAP, Oracle, generisk CSV | `/admin/migrering/*` |
| **Automatiseringsramverk** | AutomationRule, AutomationExecution, AutomationSchedule | Notify/Suggest/Autopilot, 22 regler, konfigurerbar per kategori | `/admin/automatisering/*` |
| **Pluggbara kollektivavtal** | CollectiveAgreement, AgreementRule, AgreementVersion | 10 avtal (AB, HÖK, Teknikavtalet, m.fl.), DB-driven | `/admin/avtal/*` |
| **Compensation Suite** | SalaryBand, BonusProgram, TotalRewardsStatement, CompensationModel | Löneband, bonus, total rewards, modellering | `/kompensation/*` |
| **Benefits Engine** | BenefitPlan, EligibilityRule, LifeEvent, EnrollmentWindow, BenefitStatement | Eligibility rules, life events, enrollment, statements | `/formaner/engine/*` |
| **Enterprise Analytics** | KpiDefinition, PredictiveModel, AnalyticsDashboard | 10 KPIer, prediktiva modeller, self-service rapportbyggare | `/analytics/*` |
| **VMS/Inhyrd personal** | Vendor, FrameworkAgreement, RateCard, ContingentWorker, SpendAnalytics | Leverantörer, ramavtal, rate cards, spend analytics | `/vms/*` |
| **Avancerad WFM** | DemandForecast, FatigueScore, OptimizationRun, ShiftBid | Demand forecasting, fatigue scoring, optimering | `/schema/wfm/*` |
| **Talent Marketplace** | CareerPath, InternalPosting, Mentorship, SkillIntelligence | Karriärvägar, intern mobilitet, mentorskap, skills intelligence | `/talang/*` |
| **Plattform** | WebhookSubscription, WebhookDelivery, ApiKey, CustomObjectDefinition, MarketplacePlugin | Webhooks (HMAC-SHA256), API-nycklar, custom objects, marketplace | `/admin/plattform/*` |

### Rapporter & Analytics (DB-backed)
Alla rapportvyer läser från verklig DB-data:
- **Workforce Analytics** — headcount, anställningsformer, snittålder, per-enhet-breakdown
- **Lönekartering** — löneskillnadsanalys per befattning (diskrimineringslagen)
- **Kostnadssimulering** — total lönekostnad + AG-avgifter per enhet
- **SCB-export** — personalstatistik i KLR-format (lokal förhandsvy)
- **Lönestatistik** — PayrollRun-aggregering per månad
- **Rekryteringsstatistik** — Vacancy/Application-aggregering
- **Standardrapporter** — Personalförteckning, löneregister från DB

### Auth & personalisering
- **Demo-auth** med EmployeeId i session (4 profiler: Anna/Anställd, Eva/Chef, Karl/HR, Admin)
- **MinSida** (6 personliga vyer) — schema, lön, ledighet, ärenden, profil, lönespecifikationer
- **Chefsportal** — teamvy filtrerad på chefens enhet, frånvarokalender, godkännanden
- **Auth-guards** — personalbundna actions (godkänn, avvisa, skapa) blockeras utan EmployeeId
- **0 Guid.Empty** i personalbundna flöden

### Infrastruktur
- **CI/CD** — GitHub Actions (build + test + publish)
- **Docker Compose** — PostgreSQL + app
- **PWA** — service worker, offline-sida
- **Säkerhet** — CSP headers, rate limiting, X-Frame-Options, CSRF
- **Bakgrundsjobb** — NotificationReminder, RetentionCleanup, CertificationReminder, LASAlert

### 2.0 Expansion

OpenHR 2.0 Enterprise Expansion lägger till ~85 nya domänentities, 10 nya moduler och ~60 nya sidor i tre faser:

**Fas A — Automation, Migrering & Avtal**
- **Migreringsmotor** med 10 adaptrar: PAXml 2.0, HEROMA, Personec P, Hogia, Fortnox, SIE 4i, Workday, SAP, Oracle HCM och generisk CSV. Stöder fältmappning, validering, dry-run och rollback.
- **Automatiseringsramverk** med tre åtgärdsnivåer (Notify, Suggest, Autopilot) och 22 fördefinierade regler (sjukfrånvaro-eskalering, LAS-varningar, certifiering-påminnelser, m.fl.). Konfigurerbar per kategori.
- **Pluggbara kollektivavtal** — 10 seedade avtal (AB, HÖK 24, Teknikavtalet, Vårdförbundets avtal, m.fl.) med DB-driven regelmotor. Varje anställning knyts till ett avtal.

**Fas B — Analytics, Compensation, Benefits, VMS, WFM & Talent**
- **Compensation Suite** — löneband, bonusprogram, total rewards-utlåtanden och scenariomodellering.
- **Benefits Engine** — planhantering, eligibility rules, life events, enrollment windows och statements.
- **Enterprise Analytics** — 10 KPI-definitioner, prediktiva modeller (turnover, sjukfrånvaro), self-service rapportbyggare med drag-and-drop kolumner.
- **VMS (Vendor Management System)** — leverantörsregister, ramavtal, rate cards, inhyrd personal och spend analytics.
- **Avancerad WFM** — demand forecasting baserat på historisk data, fatigue scoring (EU-arbetstidsdirektivet), optimeringsalgoritm och skiftbudgivning.
- **Talent Marketplace** — karriärvägar, interna utlysningar med matchningspoäng, mentorskapsprogram och skills intelligence.

**Fas C — Plattform & Ekosystem**
- **Webhooks** med HMAC-SHA256-signering, retry med exponential backoff och leveranslogg.
- **API-nycklar** med scope-begränsning, hash-lagring och utgångsdatum.
- **Custom Objects** — dynamiska entiteter med JSON Schema-validering.
- **Marketplace** — pluginregister med installation, konfiguration och versionshantering.

### Uttryckligen utanför nuvarande scope
Dessa kräver extern infrastruktur eller livekopplingar och är medvetet inte implementerade:
- Riktig BankID/SITHS-inloggning (nuvarande auth är demo-simulering)
- Externa integrationer: Försäkringskassan, AD/Entra, Platsbanken, SCB live, banker
- Native mobilapp
- Realtidspush via SignalR (infrastrukturen finns men ej aktiverad)

### Kvarvarande begränsningar
- **Seeddata-beroende** — många vyer förlitar sig på seed för att visa data; i produktion behövs riktiga arbetsflöden
- **Demo-auth** — namnbaserad matchning mot seedade anställda, inte en riktig identity provider

## Tech Stack

| Komponent | Teknologi |
|-----------|-----------|
| Backend | .NET 9, ASP.NET Core |
| Frontend | Blazor Server, MudBlazor 9.1 |
| Databas | PostgreSQL 17 |
| ORM | EF Core 9 med migrationer |
| Arkitektur | Modular Monolith (36 moduler) |
| Tema | Nordic Refined (light/dark mode) |
| Auth | Demo-auth med EmployeeId i session |
| CI/CD | GitHub Actions |
| Container | Docker Compose |
| Licens | AGPL-3.0 |

## Snabbstart

### Med Docker (rekommenderat)
```bash
docker compose up -d
```
Öppna http://localhost:5076

### Utan Docker
```bash
dotnet build RegionHR.sln
dotnet run --project src/Web/RegionHR.Web.csproj
```
Öppna http://localhost:5076/login

### Demo-användare
| Användare | Roll | Ser |
|-----------|------|-----|
| Admin | Admin | Allt (read-only för personalbundna actions) |
| Karl Berg | HR | Personal, Lön, Admin |
| Eva Nilsson | Chef | Team, Godkännanden |
| Anna Svensson | Anställd | Min sida, Ledighet |

## Datamodell

**160+ domänentities** fördelade på 36 moduler. Alla med EF Core-konfiguration, migrationer och seeddata.

Nyckelentities (urval): Employee, Employment, OrganizationUnit, PayrollRun, PayrollResult, LeaveRequest, VacationBalance, Case, ScheduledShift, Timesheet, Position, Vacancy, Policy, PulseSurvey, WellnessClaim, Announcement, Recognition, SuccessionPlan, FeedbackRound, MBLNegotiation, ReferenceCheck, InsuranceCoverage, DelegatedAccess, TravelClaim, OffboardingCase, RehabCase, LASAccumulation, Certification, Skill, Course, Notification, AuditEntry, Document, CollectiveAgreement, AutomationRule, MigrationProject, SalaryBand, BonusProgram, BenefitPlan, KpiDefinition, PredictiveModel, Vendor, FrameworkAgreement, DemandForecast, CareerPath, WebhookSubscription, ApiKey, CustomObjectDefinition, MarketplacePlugin.

## Utveckling

```bash
dotnet build RegionHR.sln       # 0 errors
dotnet test RegionHR.sln        # 976 tester, 0 failures
dotnet run --project src/Web/RegionHR.Web.csproj
```

## Licens

AGPL-3.0 — Alla forks måste hålla koden öppen.
