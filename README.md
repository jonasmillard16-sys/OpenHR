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
- **Arbetsmiljö SAM (v1)** — tillbud (Incident), skyddsronder (SafetyRound), riskbedömningar (RiskAssessment) i DB. CRUD via formulär. RiskVärde beräknas on-the-fly (Sannolikhet × Konsekvens), lagras ej. EnhetId är logisk koppling (inget FK). Seeddata.
- **Rekrytering** — pipeline med statusflöde, men lokal state (ej DB)
- **Rapporter** — Löneregister hämtar från DB, övriga 3 rapporter har realistiska beräkningar. CSV-export fungerar.
- **Ledighetshantering** — wizard med ärendenummer, persisterar ej till DB
- **HälsoSAM/Rehab (v1.5)** — rehabärenden läses från DB (RehabCase). Milstolpar (dag 14/90/180/365) lagras i domänmodellen, beräknade från ärendets skapandedatum. Nästa milstolpe visas baserat på dagens datum. Begränsning: seeddata, milstolpar är planerade uppföljningsdatum — inte verifierad sjukfallsstart. SickLeaveNotification ej kopplad i v1.5.
- **Godkännanden** — approve/reject med batch-operationer, lokal state
- **Notiser** — InApp med batch-radera och undo, SignalR hub finns men pushar ej
- **Dokumenthantering** — upload UI, mallgenerering, PDF-preview (text-baserad)
- **Dashboard** — KPI:er beräknade från modell, klickbara kort
- **Autentisering** — rollbaserad med persistent session, SITHS/BankID-simulering (inte riktig eID)

### Prototyp/mock
UI med fungerande navigation och demo-data. Ingen backend-persistens:

- **Medarbetarresor (Journeys)** — 5 journey-typer med checklistor, ingen domänmodell/DB
- **Arbetsmiljö SAM** — se "Delvis implementerad" ovan
- **Strategisk Workforce Planning** — what-if-scenarier, ingen beräkningsmotor
- **AD/SCIM Provisionering** — lokal provisioneringslogg och regelkonfiguration. Se "Delvis implementerad" nedan.
- **Benefits Enrollment** — livshändelser, open enrollment, ej kopplad till DB
- **Talangpool/Recruitment CRM** — kandidatpool, CV-parsning simulerad
- **Pulsundersökningar** — enkätverktyg, demo-resultat
- **E-learning** — kurskatalog, inga riktiga SCORM-paket
- **Medarbetarsamtal** — wizard med dokumentation, 360-feedback, ej persisterad
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
