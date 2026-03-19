# OpenHR

**Open-source HR-system för svenska regioner och kommuner.**

OpenHR är ett personalhanteringssystem byggt för att ersätta HEROMA och andra proprietära HR-system inom svensk offentlig sektor. Byggt med öppen källkod (AGPL-3.0), svensk arbetsrätt, kollektivavtal och GDPR.

## Funktionsstatus

### Production-ready
Riktig backend-logik, beräkningar och/eller databaslagring:

- **Personalregister** — anställda, organisationsträd, anställningar (PostgreSQL, EF Core migrationer)
- **Svensk löneberäkning** — SwedishTaxCalculator: kommunalskatt, statlig skatt, arbetsgivaravgift
- **Kollektivavtal AB/HOK** — KollektivavtalEngine: OB-tillägg, viloregler, övertid, semester per ålder
- **Traktamentsberäkning** — TraktamentsCalculator: inrikes/utrikes enligt Skatteverkets regler
- **LAS-uppföljning** — konverteringsflöde med fullständig konsekvensåterkoppling
- **Integrationsformat** — AGI-XML (Skatteverket), pain.001 (Nordea) — genererar riktig XML/ISO 20022
- **Schemaoptimering** — SchemaOptimizer med round-robin och balansindex
- **Health checks** — /health endpoint, request logging middleware
- **Säkerhet** — CSP headers, rate limiting (100 req/min), X-Frame-Options, CSRF

### Delvis implementerad
Fungerande UI med viss backend-logik. Seeddata eller lokal state — inte production-ready:

- **Uppsägningsrisk (Flight Risk v1)** — regelbaserad heuristik, beräknad från riktig personaldata via FlightRiskService. 4 signaler: tenure, anställningsform, bristyrke (heuristik), deltid. Inte prediktiv AI. Begränsning: seeddata i DB.
- **Skills/Kompetens (v1)** — normaliserad skills-katalog (Skill, EmployeeSkill, PositionSkillRequirement) med riktig EF-migration. Gap-analys via Position.InnehavareAnstallId → PositionSkillRequirement. Begränsning: seeddata, inget registrerings-UI.
- **Identitetshantering (v1)** — lokal provisioneringslogg (ProvisioningEvent) och regelkonfiguration (ProvisioningRule) i DB. ProvisioningService beräknar åtgärder baserat på regler. LocalRecordingProvider registrerar i DB utan externa anrop. Ingen SCIM/AD/Entra-integration.
- **Arbetsmiljö SAM (v1)** — tillbud, skyddsronder, riskbedömningar i DB med CRUD. RiskVärde beräknas on-the-fly. Seeddata.
- **Medarbetarresor (Journeys v1)** — JourneyTemplate + JourneyInstance med owned step-entities. Steg kopieras som snapshot. Persisterad progress. Seeddata.
- **Förmånsval (v1)** — förmånskatalog (Benefit) + anställds val (EmployeeBenefit) i DB. Registrering via domänens Anmala(), godkännande via Godkann(). Inte full open enrollment. Seeddata.
- **HälsoSAM/Rehab (v1.5)** — rehabärenden från DB. Milstolpar från lagrade uppföljningsdatum. Seeddata.
- **Arbetsmiljö SAM (v1)** — tillbud (Incident), skyddsronder (SafetyRound), riskbedömningar (RiskAssessment) i DB. CRUD via formulär. RiskVärde beräknas on-the-fly (Sannolikhet × Konsekvens), lagras ej. EnhetId är logisk koppling (inget FK). Seeddata.
- **Rekrytering** — pipeline med statusflöde, men lokal state (ej DB)
- **Lönekörningar (v2)** — PayrollRun + PayrollResult med riktig domänlogik (Skapa, Paborja, LaggTillResultat, MarkeraSomBeraknad, Godkann, MarkeraSomUtbetald). Körningslista och detalj med anomalidetektering (10% avvikelse) från DB. Lönehistorik per anställd från PayrollResult. Seeddata: 2 körningar, 4 resultat vardera. Inga demo-fallbacks.
- **Rapporter** — Löneregister hämtar från DB, övriga 3 rapporter har realistiska beräkningar. CSV-export fungerar.
- **Ledighetshantering (v2)** — LeaveRequest med riktig domänlogik (Skapa, SkickaIn, Godkann, Avvisa). VacationBalance med semestersaldo. Översikt, saldon, kalender, föräldraledighet och VAB alla DB-baserade. Ny-dialoger skapar riktiga LeaveRequests. Seeddata inkl. VAB + föräldraledighet. Kolumner som saknar modellstöd (barnnamn, FK-rapporteringsstatus, omfattning%) är borttagna — ej simulerade.
- **HälsoSAM/Rehab (v1.5)** — rehabärenden läses från DB (RehabCase). Milstolpar (dag 14/90/180/365) lagras i domänmodellen, beräknade från ärendets skapandedatum. Nästa milstolpe visas baserat på dagens datum. Begränsning: seeddata, milstolpar är planerade uppföljningsdatum — inte verifierad sjukfallsstart. SickLeaveNotification ej kopplad i v1.5.
- **Godkännanden (v1.5)** — väntande ärenden från DB (Case med owned CaseApproval). Godkänn via Case.Godkann(). Avslå via direkt property-set (domänmetod saknas). Seeddata.
- **Ärenden (v2)** — ärendelista helt från DB (Case). KPI:er beräknade per CaseStatus. Employee name join. Seeddata: 3 ärenden.
- **Granskningslogg (v2)** — audit trail från DB (AuditEntry). KPI:er (idag: skapa/ändra/ta bort) beräknade från riktiga poster. Seeddata: 5 audit entries.
- **GDPR (v2)** — DSR-lista från DB (DataSubjectRequest). Försenade/väntande/efterlevnad-KPI:er beräknade. Registerutdrag-dialog hämtar riktig Employee-data. Anonymiseringskö visar tomt (ingen backing entity). Seeddata: 2 DSR.
- **Positioner (v2)** — positionslista från DB (Position). Employee+OrgUnit name joins. KPI:er beräknade per PositionStatus. Riktig ny-dialog med OrgUnit-dropdown skapar via Position.Skapa(). Seeddata: 7 positioner (5 tillsatta, 1 vakant, 1 frusen).
- **Notiser (v1.5)** — in-app-notiser från DB via Notification-entity. Read/unread via MarkAsRead(). Testnotis skapar riktig DB-post. Ingen realtidspush. Seeddata.
- **Dokumenthantering (v1.5)** — riktig filuppladdning via IFileStorageService + Document-metadata i DB. Kategori, anställd-koppling, storlek. Mallgenerering (read-only). Seeddata.
- **Dashboard (v1.5)** — 5 KPI-kort helt från DB (headcount, LAS-alarm, vakanta positioner, godkännanden, bemanningsgrad). Bemanning per enhet från DB. Inga demo-fallbacks, ingen DemoDataModel. Sjukfrånvaro och händelselista borttagna (semantiskt ej möjliga utan ny entity).
- **Autentisering** — rollbaserad med persistent session, SITHS/BankID-simulering (inte riktig eID)

### Prototyp/mock
UI med fungerande navigation och demo-data. Ingen backend-persistens:

- **Medarbetarresor (Journeys)** — se "Delvis implementerad" ovan
- **Arbetsmiljö SAM** — se "Delvis implementerad" ovan
- **Strategisk Workforce Planning** — se "Delvis implementerad" ovan
- **AD/SCIM Provisionering** — lokal provisioneringslogg och regelkonfiguration. Se "Delvis implementerad" nedan.
- **Benefits Enrollment** — se "Delvis implementerad" ovan
- **Talangpool** — se "Delvis implementerad" ovan
- **Pulsundersökningar** — enkätverktyg, demo-resultat
- **E-learning (v1.5)** — kurskatalog (Course) + kursanmälningar (CourseEnrollment) från DB. Anmälan via domänens Anmala(). Progress/status från modellen. Seeddata.
- **Medarbetarsamtal (v1.5)** — PerformanceReview med domänlogik (Skapa, SattSjalvbedomning, SattChefsbedomning, Genomfor). Persisteras i DB. Seeddata i olika statusar.
- **Förmåner** — friskvård, försäkringar, formulär utan DB-koppling

## Tech Stack

| Komponent | Teknologi |
|-----------|-----------|
| Backend | .NET 9, ASP.NET Core |
| Frontend | Blazor Server, MudBlazor 9.1 |
| Databas | PostgreSQL 17 |
| Arkitektur | Modular Monolith (25 moduler) |
| Tema | Nordic Refined (light/dark mode) |
| Auth | SITHS/BankID (simulerad), rollbaserad |
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
| Admin | Admin | Allt |
| Karl Berg | HR | Personal, Lön, Admin |
| Eva Nilsson | Chef | Team, Godkännanden |
| Sara Andersson | Anställd | Min sida, Ledighet |

## Utveckling

```bash
dotnet build RegionHR.sln       # 0 errors
dotnet test RegionHR.sln        # 494+ tester
dotnet run --project src/Web/RegionHR.Web.csproj
```

## Arkitektur

```
src/
├── SharedKernel/          # Domänprimitiver (Personnummer, Money, EmployeeId)
├── Modules/               # 25 domänmoduler
│   ├── Core/              # Personalregister, organisation
│   ├── Payroll/           # Löneberäkning, skatt
│   ├── Scheduling/        # Schema, instämpling
│   ├── CaseManagement/    # Ärenden, MBL
│   ├── LAS/               # LAS-ackumulering
│   ├── Competence/        # Skills, certifieringar, gap-analys
│   ├── Recruitment/       # Vakanser, ansökningar
│   └── ...                # 18 moduler till
├── Infrastructure/        # EF Core, beräkningsmotorer, integrationer
├── Web/                   # Blazor Server (106 sidor)
├── Api/                   # REST API
└── DesignSystem/          # Komponentbibliotek
```

## Licens

AGPL-3.0 — Alla forks måste hålla koden öppen.
