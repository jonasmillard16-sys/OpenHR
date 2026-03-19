# OpenHR

**Open-source HR-system för svenska regioner och kommuner.**

OpenHR är ett personalhanteringssystem byggt för att ersätta HEROMA och andra proprietära HR-system inom svensk offentlig sektor. Systemet är byggt med öppen källkod (AGPL-3.0) och följer svensk arbetsrätt, kollektivavtal och GDPR.

## Funktionsstatus

### Production-ready
Dessa funktioner har riktig backend-logik, beräkningar och/eller databaslagring:

- **Personalregister** — anställda, organisationsträd, anställningar (PostgreSQL)
- **Svensk löneberäkning** — SwedishTaxCalculator: kommunalskatt, statlig skatt, arbetsgivaravgift
- **Kollektivavtal AB/HOK** — KollektivavtalEngine: OB-tillägg, viloregler, övertid, semester per ålder
- **Traktamentsberäkning** — TraktamentsCalculator: inrikes/utrikes enligt Skatteverkets regler
- **LAS-uppföljning** — konverteringsflöde med fullständig konsekvensåterkoppling
- **EF Core + PostgreSQL** — migrationer, seed data (10 anställda, 6 enheter), value converters
- **Autentisering** — rollbaserad (Admin/HR/Chef/Anställd), persistent session, SITHS/BankID-simulering
- **PDF-generering** — lönespec, tjänstgöringsintyg, anställningsavtal (text-baserad, QuestPDF-redo)
- **Integrationsformat** — AGI-XML (Skatteverket), pain.001 (Nordea)
- **Schemaoptimering** — SchemaOptimizer med round-robin och balansindex
- **Health checks** — /health endpoint, request logging middleware
- **Säkerhet** — CSP headers, rate limiting, X-Frame-Options

### Delvis implementerade
Dessa har fungerande UI och viss backend-logik men använder delvis demo-data:

- **Rekrytering** — pipeline med 22 ansökande och statusflöde, men inte kopplad till DB fullt
- **Rapporter** — 4 rapporter med realistisk data (Löneregister hämtar från DB), CSV-export fungerar
- **Ledighetshantering** — wizard med ärendenummer, men persisterar ej till DB
- **HälsoSAM/Rehab** — rehabkedjan med lagstadgade milstolpar (dag 14/90/180/365), delvis DB-kopplad
- **Medarbetarsamtal** — wizard med dokumentation, 360-feedback UI, ej persisterad
- **Godkännanden** — approve/reject med batch-operationer, lokal state
- **Notiser** — InApp med batch-radera och undo, SignalR hub finns men pushar ej
- **Förmåner (grund)** — friskvård, försäkringsöversikt, formulär fungerar
- **Dokumenthantering** — upload UI, mallgenerering, PDF-preview
- **Dashboard** — KPI:er beräknade från modell, klickbara kort

### Delvis implementerad (riktig backend, begränsad data)
- **Uppsägningsrisk (Flight Risk v1)** — regelbaserad heuristik, beräknad från riktig personaldata (tenure, anställningsform, befattning, sysselsättningsgrad). Inte prediktiv AI. Begränsningar: ingen sjukfrånvaro, lönehistorik eller samtalsdata.
- **Skills/Kompetens (v1)** — normaliserad skills-katalog i DB (Skill, EmployeeSkill, PositionSkillRequirement). Gap-analys jämför anställds skills mot positionens kravprofil via riktig relation (Position.InnehavareAnstallId). Seeddata i DB — inte production-ready.

### Prototyp/mock (UI utan riktig backend)
Dessa sidor existerar med fungerande navigation och demo-data men saknar backend-persistens:

- **Medarbetarresor (Journeys)** — 5 journeys med checklistor, ingen domänmodell/DB
- **Arbetsmiljö SAM** — tillbud, skyddsronder, riskmatris, ingen DB
- **Strategisk Workforce Planning** — what-if-scenarier, Build/Borrow/Buy, ingen beräkningsmotor
- **AD/SCIM Provisionering** — dashboard med simulerad synk, ingen extern integration
- **Benefits Enrollment** — livshändelser, open enrollment, ej kopplad till DB
- **Talangpool/Recruitment CRM** — kandidatpool, CV-parsning simulerad, ej kopplad till DB
- **Pulsundersökningar** — enkätverktyg, resultat är demo-data
- **E-learning** — kurskatalog, inga riktiga SCORM-paket

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
│   ├── HalsoSAM/          # Rehabilitering
│   ├── Recruitment/       # Vakanser, ansökningar
│   └── ...                # 18 moduler till
├── Infrastructure/        # EF Core, export, beräkningsmotorer, integrationer
├── Web/                   # Blazor Server (106 sidor)
├── Api/                   # REST API
└── DesignSystem/          # Komponentbibliotek
```

## Licens

AGPL-3.0 — Alla forks måste hålla koden öppen.
