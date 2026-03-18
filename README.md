# OpenHR

**Open-source HR-system för svenska regioner och kommuner.**

OpenHR är ett komplett personalhanteringssystem byggt för att ersätta HEROMA och andra proprietära HR-system inom svensk offentlig sektor. Systemet är byggt med öppen källkod (AGPL-3.0) och följer svensk arbetsrätt, kollektivavtal och GDPR.

## Funktioner

### Personalhantering
- Personalregister med anställningar, befattningar och organisationsträd
- LAS-uppföljning med automatisk ackumulering och konverteringsflöde
- Kompetensregister med gap-analys per roll
- Successionsplanering

### Lön & Ersättning
- Svensk löneberäkning (kommunalskatt, statlig skatt, arbetsgivaravgift)
- Kollektivavtal AB/HOK (OB-tillägg, övertid, viloRegler)
- Löneöversyn och lönekartering (diskrimineringslagen)
- Integrationer: AGI-XML (Skatteverket), pain.001 (Nordea)

### Schema & Tid
- Schemaplanering med optimeringsalgoritm
- Instämpling och tidrapportering
- ATL-efterlevnad (Arbetstidslagen)

### Ledighet & Frånvaro
- Semester, VAB, föräldraledighet, tjänstledighet
- Sjukanmälan med automatisk FK-anmälan dag 15
- Interaktiv månadskalender

### Rekrytering
- Vakanser med ansökningspipeline
- Onboarding och offboarding workflows
- Intern jobbmarknad

### Self-service
- Min sida (schema, lön, ledighet, profil)
- AI-assistent med personliga svar och åtgärdsknappar
- Godkännandeflöden

### Administration
- GDPR-efterlevnad (registerutdrag, anonymisering)
- Granskningslogg med ändringshistorik per anställd
- Pulsundersökningar, peer recognition
- Dokumenthantering med mallgenerering

## Tech Stack

| Komponent | Teknologi |
|-----------|-----------|
| Backend | .NET 9, ASP.NET Core |
| Frontend | Blazor Server, MudBlazor 9.1 |
| Databas | PostgreSQL 17 |
| Arkitektur | Modular Monolith (25 moduler) |
| Tema | Nordic Refined (light/dark) |
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
# Bygga
dotnet build RegionHR.sln

# Testa (482+ tester)
dotnet test RegionHR.sln

# Köra
dotnet run --project src/Web/RegionHR.Web.csproj
```

## Arkitektur

```
src/
├── SharedKernel/          # Domänprimitiver
├── Modules/               # 25 domänmoduler
│   ├── Core/              # Personalregister
│   ├── Payroll/           # Löneberäkning
│   ├── Scheduling/        # Schema
│   ├── CaseManagement/    # Ärenden
│   ├── LAS/               # LAS-uppföljning
│   └── ...
├── Infrastructure/        # EF Core, export, integrationer
├── Web/                   # Blazor Server (96 sidor)
├── Api/                   # REST API
└── DesignSystem/          # Komponentbibliotek
```

## Licens

AGPL-3.0 — Alla forks måste hålla koden öppen.
