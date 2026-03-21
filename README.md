# OpenHR

**Open-source HR-system for svenska regioner och kommuner.**

OpenHR ar ett personalhanteringssystem byggt for att ersatta HEROMA och andra proprietara HR-system inom svensk offentlig sektor. Byggt med oppen kallkod (AGPL-3.0), svensk arbetsratt, kollektivavtal och GDPR.

> **OpenHR 2.0** — 38 moduler, ~177 sidor, 207 domanentities, 1 116 tester, 328 i18n-nycklar (sv+en). Se [2.0 Expansion](#20-expansion) nedan.

## Funktionsstatus

### Berakningsmotorer (production-ready)
Riktiga berakningar med korrekt svensk lagstiftning:

- **SwedishTaxCalculator** — kommunalskatt, statlig skatt, arbetsgivaravgift (31,42%), reducerad avgift 66+
- **KollektivavtalEngine** — OB-tillagg (kvall/natt/helg/storhelg), viloregler, overtid, semester per alder (AB/HOK)
- **TraktamentsCalculator** — inrikes/utrikes traktamente enligt Skatteverkets regler 2026
- **SchemaOptimizer** — round-robin-tilldelning med balansindex
- **Integrationsformat** — AGI-XML (Skatteverket), pain.001 (Nordea ISO 20022), CSV-export

### Karnmoduler (DB-backed, domanlogik)
Fullstandig datamodell med entities, domanmetoder, EF Core-konfiguration, migrationer och seed:

| Modul | Entities | Domanflode | Routes |
|-------|----------|------------|--------|
| **Personalregister** | Employee, Employment, OrganizationUnit | Skapa, anstall, organisationstrad | `/anstallda`, `/anstallda/ny`, `/organisation` |
| **Ledighet** | LeaveRequest, VacationBalance, SickLeaveNotification | Skapa→SkickaIn→Godkann/Avvisa | `/ledighet/*` (7 routes) |
| **Lon/Payroll** | PayrollRun, PayrollResult, PayrollResultLine, SalaryCode | Skapa→Paborja→LaggTillResultat→Beraknad→Godkand→Utbetald | `/lon/*` (5 routes) |
| **Schema/Tid** | Schedule, ScheduledShift, Timesheet, TimeClockEvent, ShiftSwapRequest, StaffingTemplate | Schema→Pass, Stampla In/Ut, Tidrapport→Godkann. **Shift Bidding**: OpenShift, ShiftBid, ShiftBidResult — budgivning pa oppna pass | `/schema/*`, `/tidrapporter/*`, `/stampling` |
| **Arenden** | Case, CaseApproval, CaseComment | SkapaFranvaroarende→Godkann. **Grievance Management**: Grievance, GrievanceInvestigation, GrievanceHearing, GrievanceAppeal — formell klagomal-process med utredning och overklagan | `/arenden/*`, `/godkannanden` |
| **MBL** | MBLNegotiation | Skapa→Paborja→Avsluta→RegistreraProtokoll | `/arenden/mbl` |
| **LAS** | LASAccumulation, LASPeriod, LASEvent | Perioder, statusberakning, foretradesratt | `/las` |
| **HalsoSAM/Rehab** | RehabCase, RehabUppfoljning | Skapa, milstolpar (dag 14/90/180/365) | `/halsosam/*` |
| **Kompetens** | Skill, EmployeeSkill, PositionSkillRequirement, Certification, MandatoryTraining | Gap-analys, certifieringsstatus | `/kompetens/*` |
| **Medarbetarsamtal** | PerformanceReview | Skapa→Sjalvbedomning→Chefsbedomning→Genomford | `/medarbetarsamtal/*` |
| **360-feedback** | FeedbackRound, FeedbackResponse | Skapa→Oppna→Stang, betyg 1-5 | `/medarbetarsamtal/360` |
| **Pulsundersokning** | PulseSurvey, PulseSurveyQuestion, PulseSurveyResponse, PulseSurveyAnswer | Skapa→Oppna→Svara→Stang. Anonyma svar. | `/admin/pulsundersokning/*` |
| **Policyer** | Policy, PolicyConfirmation | Skapa→Publicera→Arkivera, bekraftelse per anstalld | `/dokument/policyer/*` |
| **Dokument** | Document, DocumentTemplate | Filuppladdning, metadata, mallgenerering | `/dokument/*` |
| **Notiser** | Notification, NotificationTemplate, NotificationPreference, **PushSubscription** | Skapa, MarkAsRead, personliga preferenser, **push-prenumeration** (Web Push) | `/notiser/*` |
| **Positioner** | Position, PositionHistorik, HeadcountPlan | Skapa→Tillsatt/Vakant/Frys | `/positioner` |
| **Successionsplanering** | SuccessionPlan | Position→Kandidat, beredskapsniva | `/admin/succession` |
| **Rekrytering** | Vacancy, Application, OnboardingChecklist, Scorecard, InterviewSchedule, ReferenceCheck | Publicera→TaEmotAnsokan→Pipeline→Tillsatt | `/rekrytering/*` |
| **Resor** | TravelClaim, ExpenseItem | Skapa→SattTraktamente→SkickaIn→Attestera | `/resor` |
| **Offboarding** | OffboardingCase, OffboardingItem | Skapa (auto 8 steg)→MarkeraSomPagar→Slutfor | `/offboarding/*` |
| **Loneoversyn** | SalaryReviewRound, SalaryProposal | Skapa→FackligAvstemning→Godkand→Genomford | `/loneoversyn` |
| **Benefits** | Benefit, EmployeeBenefit | Anmala→Godkann | `/formaner/*` |
| **Friskvard** | WellnessClaim | Skapa→Godkann/Avvisa, max 5000 kr/ar | `/formaner/friskvard` |
| **Forsakringar** | InsuranceCoverage | TGL, AGS, TFA, AFA, PSA | `/formaner/forsakringar` |
| **Anslagstavla** | Announcement | Skapa→Publicera→Arkivera, prioritetsniver | `/admin/anslagstavla` |
| **Peer Recognition** | Recognition | Ge berom till kollega med kategori | `/admin/berom` |
| **Delegering** | DelegatedAccess | Skapa→ArGiltig→Avsluta | `/admin/delegering` |
| **E-learning** | Course, CourseEnrollment, LearningPath | Anmala→Paborja→Genomford | `/utbildning/*` |
| **GDPR** | DataSubjectRequest, RetentionRecord | Skapa→Tilldela→Slutfor, registerutdrag | `/gdpr` |
| **Audit** | AuditEntry | Create/Update/Delete-logg | `/audit` |
| **Talangpool** | TalentPoolEntry | Kandidater for framtida rekrytering | `/rekrytering/talangpool` |
| **Flight Risk** | (beraknad tjanst) | 4 signaler: tenure, anstallningsform, bristyrke, deltid | `/rapporter/flight-risk` |
| **Workforce Planning** | HeadcountPlan | Budget per enhet per ar. **Workforce Planning Scenarios** — scenariomodellering med what-if-analys | `/rapporter/workforce-plan` |
| **Provisionering** | ProvisioningRule, ProvisioningEvent | Lokal registrering (ej extern AD/SCIM) | `/admin/provisionering` |
| **Journeys** | JourneyTemplate, JourneyInstance | Onboarding/offboarding-mallar med steg | `/journeys/*` |
| **Migreringsmotor** | MigrationProject, MigrationMapping, MigrationRun, m.fl. | PAXml 2.0, HEROMA, Personec P, Hogia, Fortnox, SIE 4i, Workday, SAP, Oracle, generisk CSV | `/admin/migrering/*` |
| **Automatiseringsramverk** | AutomationRule, AutomationExecution, AutomationSchedule | Notify/Suggest/Autopilot, 22 regler, konfigurerbar per kategori | `/admin/automatisering/*` |
| **Pluggbara kollektivavtal** | CollectiveAgreement, AgreementRule, AgreementVersion | 10 avtal (AB, HOK, Teknikavtalet, m.fl.), DB-driven | `/admin/avtal/*` |
| **Compensation Suite** | SalaryBand, BonusProgram, TotalRewardsStatement, CompensationModel | Loneband, bonus, total rewards, modellering | `/kompensation/*` |
| **Benefits Engine** | BenefitPlan, EligibilityRule, LifeEvent, EnrollmentWindow, BenefitStatement | Eligibility rules, life events, enrollment, statements | `/formaner/engine/*` |
| **Enterprise Analytics** | KpiDefinition, PredictiveModel, AnalyticsDashboard | 10 KPI:er, prediktiva modeller, self-service rapportbyggare. **Workforce Planning Scenarios** for headcount-prognoser | `/analytics/*` |
| **VMS/Inhyrd personal** | Vendor, FrameworkAgreement, RateCard, ContingentWorker, SpendAnalytics | Leverantorer, ramavtal, rate cards, spend analytics. **F-skatt Compliance** — verifiering av F-skattsedel for inhyrda | `/vms/*` |
| **Avancerad WFM** | DemandForecast, FatigueScore, OptimizationRun, ShiftBid | Demand forecasting, fatigue scoring, optimering | `/schema/wfm/*` |
| **Talent Marketplace** | CareerPath, InternalPosting, Mentorship, SkillIntelligence | Karrarsvagar, intern mobilitet, mentorskap, skills intelligence | `/talang/*` |
| **Plattform** | WebhookSubscription, WebhookDelivery, ApiKey, CustomObjectDefinition, MarketplacePlugin | Webhooks (HMAC-SHA256), API-nycklar, custom objects, marketplace | `/admin/plattform/*` |
| **HR Service Delivery** | ServiceRequest, ServiceCategory, SLADefinition, HRQueue, CaseTemplate, CaseSatisfaction | Arenderutt med SLA, agentarbetsyta, CSAT-matning, mallar | `/helpdesk/*` |
| **AI HR-assistent** | KnowledgeArticle, KnowledgeCategory, ConversationSession, ConversationMessage, AssistantAction | 20 kunskapsartiklar, konversationspersistens, atgardsforslag | `/kunskapsbas/*` |
| **Manager Effectiveness** | (integrerad i chefsportalen) | 1:1-moten, scorecard, coaching-nudges for chefer | `/chef/*` |
| **ONA** | (beraknad tjanst) | Organisational Network Analysis — samarbetsmonster och kommunikationsfloden | `/rapporter/ona` |

### Rapporter & Analytics (DB-backed)
Alla rapportvyer laser fran verklig DB-data:
- **Workforce Analytics** — headcount, anstallningsformer, snittalder, per-enhet-breakdown
- **Lonekartering** — loneskillnadsanalys per befattning (diskrimineringslagen)
- **Kostnadssimulering** — total lonekostnad + AG-avgifter per enhet
- **SCB-export** — personalstatistik i KLR-format (lokal forhandsvy)
- **Lonestatistik** — PayrollRun-aggregering per manad
- **Rekryteringsstatistik** — Vacancy/Application-aggregering
- **Standardrapporter** — Personalforteckning, loneregister fran DB
- **EU Pay Transparency** — loneransparensrapportering enligt EU-direktivet 2023/970, pay gap-analys per kohort
- **Workforce Planning Scenarios** — scenariomodellering for framtida personalbehov

### Auth & personalisering
- **Demo-auth** med EmployeeId i session (4 profiler: Anna/Anstalld, Eva/Chef, Karl/HR, Admin)
- **MinSida** (6 personliga vyer) — schema, lon, ledighet, arenden, profil, lonespecifikationer
- **Chefsportal** — teamvy filtrerad pa chefens enhet, franvarokalender, godkannanden
- **Auth-guards** — personalbundna actions (godkann, avvisa, skapa) blockeras utan EmployeeId
- **0 Guid.Empty** i personalbundna floden

### Internationalisering (i18n)
- **328 nycklar** i sv + en (SharedResources.sv.resx / SharedResources.en.resx)
- NavMenu, TopBar, formularlabels, felmeddelanden — allt via IStringLocalizer
- Sprakvaxling via cookie + page reload
- Forberett for fler sprak (lagg till .resx-fil)

### Infrastruktur
- **CI/CD** — GitHub Actions (build + test + publish)
- **Docker Compose** — PostgreSQL + app
- **PWA** — Enhanced service worker med offline data caching (schema, saldon, notiser), network-first for API, cache-first for statiska resurser, background sync for offline-actions (ledighetsbegaran, stampling), push-notiser (Web Push), bottom navigation, manifest med genvagar
- **Sakerhet** — CSP headers, rate limiting, X-Frame-Options, CSRF
- **Bakgrundsjobb** — NotificationReminder, RetentionCleanup, CertificationReminder, LASAlert

### Trust & Security
OpenHR har en dedikerad [/trust](/trust)-sida med:
- Sakerhetsarkitektur och design-principer
- OWASP ASVS sjalvbedomning
- GDPR-complianceguide
- DPA-mall for personuppgiftsbitradesavtal
- Deployment-guide med hardenings-rekommendationer

Se aven `docs/security/` for detaljerad dokumentation: OWASP ASVS, GDPR-guide, DPA-mall, deployment-guide.

### 2.0 Expansion

OpenHR 2.0 Enterprise Expansion lagger till ~100 nya domanentities, 12+ nya moduler och ~80 nya sidor:

**Fas A — Automation, Migrering & Avtal**
- **Migreringsmotor** med 10 adaptrar: PAXml 2.0, HEROMA, Personec P, Hogia, Fortnox, SIE 4i, Workday, SAP, Oracle HCM och generisk CSV. Stoder faltmappning, validering, dry-run och rollback.
- **Automatiseringsramverk** med tre atgardsniver (Notify, Suggest, Autopilot) och 22 fordefinierade regler (sjukfranvaro-eskalering, LAS-varningar, certifiering-paminnelser, m.fl.). Konfigurerbar per kategori.
- **Pluggbara kollektivavtal** — 10 seedade avtal (AB, HOK 24, Teknikavtalet, Vardforbundets avtal, m.fl.) med DB-driven regelmotor. Varje anstallning knyts till ett avtal.

**Fas B — Analytics, Compensation, Benefits, VMS, WFM & Talent**
- **Compensation Suite** — loneband, bonusprogram, total rewards-utlatanden och scenariomodellering.
- **Benefits Engine** — planhantering, eligibility rules, life events, enrollment windows och statements.
- **Enterprise Analytics** — 10 KPI-definitioner, prediktiva modeller (turnover, sjukfranvaro), self-service rapportbyggare med drag-and-drop kolumner. **Workforce Planning Scenarios** for headcount-prognoser.
- **VMS (Vendor Management System)** — leverantorsregister, ramavtal, rate cards, inhyrd personal, spend analytics och **F-skatt Compliance**.
- **Avancerad WFM** — demand forecasting baserat pa historisk data, fatigue scoring (EU-arbetstidsdirektivet), optimeringsalgoritm och **skiftbudgivning**.
- **Talent Marketplace** — karriarvagar, interna utlysningar med matchningspoang, mentorskapsprogram och skills intelligence.

**Fas C — Plattform & Ekosystem**
- **Webhooks** med HMAC-SHA256-signering, retry med exponential backoff och leveranslogg.
- **API-nycklar** med scope-begransning, hash-lagring och utgangsdatum.
- **Custom Objects** — dynamiska entiteter med JSON Schema-validering.
- **Marketplace** — pluginregister med installation, konfiguration och versionshantering.

**Fas D — Service Delivery, AI & Compliance**
- **HR Service Delivery** — arendehantering med SLA, agentarbetsyta, routing-regler, CSAT-matning, mallar.
- **AI HR-assistent** — kunskapsbas med 20 artiklar, konversationspersistens, atgardsforslag, kontextmedveten.
- **Shift Bidding** — budgivning pa oppna pass med preferenser och automatisk tilldelning.
- **Grievance Management** — formell klagomal-process med utredning, hearing och overklagan.
- **EU Pay Transparency** — lonerapportering enligt EU-direktivet 2023/970, pay gap-analys per kohort.
- **Manager Effectiveness** — 1:1-moten, scorecard, coaching-nudges for chefer.
- **ONA (Organizational Network Analysis)** — samarbetsmonster och kommunikationsfloden.
- **Deep PWA** — offline data caching, push-notiser, background sync, bottom navigation, swipe-gester.

### Uttryckligen utanfor nuvarande scope
Dessa kraver extern infrastruktur eller livekopplingar och ar medvetet inte implementerade:
- Riktig BankID/SITHS-inloggning (nuvarande auth ar demo-simulering)
- Externa integrationer: Forsakringskassan, AD/Entra, Platsbanken, SCB live, banker
- Native mobilapp (PWA med offline-stod och push-notiser anvands istallet)
- Realtidspush via SignalR (infrastrukturen finns men ej aktiverad)

### Kvarvarande begransningar
- **Seeddata-beroende** — manga vyer forlitar sig pa seed for att visa data; i produktion behovs riktiga arbetsfloden
- **Demo-auth** — namnbaserad matchning mot seedade anstallda, inte en riktig identity provider

## Tech Stack

| Komponent | Teknologi |
|-----------|-----------|
| Backend | .NET 9, ASP.NET Core |
| Frontend | Blazor Server, MudBlazor 9.1 |
| Databas | PostgreSQL 17 |
| ORM | EF Core 9 med migrationer |
| Arkitektur | Modular Monolith (38 moduler) |
| Tema | Nordic Refined (light/dark mode) |
| Auth | Demo-auth med EmployeeId i session |
| i18n | 328 nycklar, sv + en (IStringLocalizer) |
| PWA | Offline cache, push-notiser, background sync |
| CI/CD | GitHub Actions |
| Container | Docker Compose |
| Licens | AGPL-3.0 |

## Snabbstart

### Med Docker (rekommenderat)
```bash
docker compose up -d
```
Oppna http://localhost:5076

### Utan Docker
```bash
dotnet build RegionHR.sln
dotnet run --project src/Web/RegionHR.Web.csproj
```
Oppna http://localhost:5076/login

### Demo-anvandare
| Anvandare | Roll | Ser |
|-----------|------|-----|
| Admin | Admin | Allt (read-only for personalbundna actions) |
| Karl Berg | HR | Personal, Lon, Admin |
| Eva Nilsson | Chef | Team, Godkannanden |
| Anna Svensson | Anstalld | Min sida, Ledighet |

## Datamodell

**207 domanentities** fordelade pa 38 moduler. Alla med EF Core-konfiguration, migrationer och seeddata.

Nyckelentities (urval): Employee, Employment, OrganizationUnit, PayrollRun, PayrollResult, LeaveRequest, VacationBalance, Case, ScheduledShift, Timesheet, Position, Vacancy, Policy, PulseSurvey, WellnessClaim, Announcement, Recognition, SuccessionPlan, FeedbackRound, MBLNegotiation, ReferenceCheck, InsuranceCoverage, DelegatedAccess, TravelClaim, OffboardingCase, RehabCase, LASAccumulation, Certification, Skill, Course, Notification, PushSubscription, AuditEntry, Document, CollectiveAgreement, AutomationRule, MigrationProject, SalaryBand, BonusProgram, BenefitPlan, KpiDefinition, PredictiveModel, Vendor, FrameworkAgreement, DemandForecast, CareerPath, WebhookSubscription, ApiKey, CustomObjectDefinition, MarketplacePlugin, ServiceRequest, SLADefinition, KnowledgeArticle, ConversationSession, Grievance, GrievanceInvestigation, OpenShift, ShiftBid, PayTransparencyReport, PayGapAnalysis.

## Utveckling

```bash
dotnet build RegionHR.sln       # 0 errors
dotnet test RegionHR.sln        # 1 116 tester, 0 failures
dotnet run --project src/Web/RegionHR.Web.csproj
```

## Licens

AGPL-3.0 — Alla forks maste halla koden oppen.
