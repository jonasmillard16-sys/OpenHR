# RegionHR — Komplett systemkartläggning

> **Version:** 0.6.0-fas6 | **Plattform:** .NET 9 | **Arkitektur:** Modular Monolith
> **Moduler:** 25 | **API-endpoints:** 204 | **Tester:** 475 | **Integrationsadapters:** 16
> **Senast uppdaterad:** 2026-03-17

---

## Innehåll

1. [Systemöversikt](#1-systemöversikt)
2. [Teknikstack](#2-teknikstack)
3. [Moduler — detaljerad kartläggning](#3-moduler)
   - 3.1 [Core HR (Personalregister)](#31-core-hr)
   - 3.2 [Payroll (Löneberäkning)](#32-payroll)
   - 3.3 [Scheduling (Schemaläggning)](#33-scheduling)
   - 3.4 [Case Management (Ärendehantering)](#34-case-management)
   - 3.5 [LAS (Regelefterlevnad)](#35-las)
   - 3.6 [HälsoSAM (Rehabilitering)](#36-hälsosam)
   - 3.7 [Salary Review (Löneöversyn)](#37-salary-review)
   - 3.8 [Travel (Resor & Utlägg)](#38-travel)
   - 3.9 [Recruitment (Rekrytering)](#39-recruitment)
   - 3.10 [Integration Hub](#310-integration-hub)
   - 3.11 [SelfService (Självservice)](#311-selfservice)
   - 3.12 [Audit (Granskningslogg)](#312-audit)
   - 3.13 [Notifications (Notiseringar)](#313-notifications)
   - 3.14 [Leave (Ledighet)](#314-leave)
   - 3.15 [Documents (Dokumenthantering)](#315-documents)
   - 3.16 [Performance (Medarbetarsamtal)](#316-performance)
   - 3.17 [Reporting (Rapportering)](#317-reporting)
   - 3.18 [GDPR](#318-gdpr)
   - 3.19 [Competence (Kompetens)](#319-competence)
   - 3.20 [Positions (Positionshantering)](#320-positions)
   - 3.21 [Offboarding](#321-offboarding)
   - 3.22 [Benefits (Förmåner)](#322-benefits)
   - 3.23 [LMS (Utbildning)](#323-lms)
   - 3.24 [Configuration (Multi-tenant)](#324-configuration)
   - 3.25 [Analytics (Ad hoc-analys)](#325-analytics)
4. [Infrastruktur](#4-infrastruktur)
5. [API-endpoints — komplett lista](#5-api-endpoints)
6. [Databasschema](#6-databasschema)
7. [Säkerhet & behörighet](#7-säkerhet)
8. [Frontend](#8-frontend)
9. [Testinventering](#9-tester)
10. [Driftsättning](#10-driftsättning)

---

## 1. Systemöversikt

RegionHR är ett komplett HR-system byggt för svenska regioner som ersätter HEROMA (CGI). Systemet hanterar hela personallivscykeln: anställning, lön, schema, frånvaro, rehabilitering, LAS-uppföljning, löneöversyn, resor, rekrytering, dokumenthantering, medarbetarsamtal, kompetensregister, rapportering och GDPR-efterlevnad.

### Arkitekturprinciper
- **Modular Monolith** — 19 domänmoduler med schema-per-modul i PostgreSQL
- **Inga databasjoin över modulgränser** — moduler kommunicerar via C#-interfaces och domänhändelser
- **Outbox Pattern** — asynkron, pålitlig integration via meddelandekö
- **Domain-Driven Design** — aggregat, value objects, domänhändelser
- **Svenskt domänspråk** — metoder och egenskaper på svenska, infrastruktur på engelska

### Siffror
| Mått | Värde |
|------|-------|
| Projekt i solution | 55 |
| Domänmoduler | 25 |
| API-endpoints | 204 |
| Domänentiteter | 65+ |
| Integrationsadapters | 16 |
| Tester | 475 (0 fel) |
| Testprojekt | 25 |
| Källfiler (.cs) | 298 |
| NuGet-paket | 18 |

---

## 2. Teknikstack

| Lager | Teknologi | Version |
|-------|-----------|---------|
| Runtime | .NET | 9.0 |
| Språk | C# | 13 |
| Web API | ASP.NET Core Minimal API | 9.0 |
| Frontend (SPA) | HTML/JS/CSS (single-page) | — |
| Frontend (SSR) | Blazor Server | 9.0 |
| Databas | PostgreSQL | 17 |
| ORM | Entity Framework Core + Npgsql | 9.0.4 |
| Cache | Redis | 7 |
| Meddelandekö | RabbitMQ | 4 |
| Autentisering | JWT Bearer (Azure AD / Supabase) | — |
| PDF-generering | QuestPDF | 2024.12.2 |
| Excel-export | ClosedXML | 0.104.2 |
| E-post | MailKit | 4.9.0 |
| Observerbarhet | OpenTelemetry | 1.11–1.12 |
| Testramverk | xUnit | 2.9.3 |
| Containerisering | Docker + docker-compose | — |

### NuGet-paket (komplett)
| Paket | Version | Användning |
|-------|---------|------------|
| Microsoft.EntityFrameworkCore | 9.0.4 | ORM |
| Microsoft.EntityFrameworkCore.Design | 9.0.4 | Migreringsverktyg |
| Microsoft.EntityFrameworkCore.InMemory | 9.0.4 | Testdatabas |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.3 | PostgreSQL-provider |
| Microsoft.AspNetCore.Authentication.JwtBearer | 9.0.4 | JWT-autentisering |
| Microsoft.AspNetCore.Components.Web | 9.0.14 | Blazor SSR |
| Microsoft.AspNetCore.OpenApi | 9.0.14 | OpenAPI/Swagger |
| Microsoft.Extensions.Caching.Memory | 9.0.4 | In-memory cache |
| Microsoft.Extensions.Hosting.Abstractions | 9.0.4 | BackgroundService |
| OpenTelemetry.Extensions.Hosting | 1.11.2 | Telemetri |
| OpenTelemetry.Instrumentation.AspNetCore | 1.12.0 | HTTP-tracing |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.11.2 | OTLP-export |
| QuestPDF | 2024.12.2 | PDF (lönespecifikationer) |
| ClosedXML | 0.104.2 | Excel-export |
| MailKit | 4.9.0 | SMTP-epost |
| Microsoft.NET.Test.Sdk | 17.12.0 | Testharness |
| xunit | 2.9.3 | Enhetstester |
| xunit.runner.visualstudio | 2.8.2 | VS Test Runner |

---

## 3. Moduler

### 3.1 Core HR
**Sökväg:** `src/Modules/Core/` | **Schema:** `core_hr` | **Tester:** 5

Personalregister, organisationsstruktur och anställningshantering.

#### Domänentiteter

**Employee** (Aggregatrot)
| Egenskap | Typ | Beskrivning |
|----------|-----|-------------|
| Id | EmployeeId | Starkt typat ID |
| Personnummer | Personnummer | Luhn-validerat, maskerat vid visning |
| Fornamn, Efternamn, MellanNamn | string | Namnuppgifter |
| Epost, Telefon | string? | Kontaktuppgifter |
| Adress | Address (value object) | Gatuadress, postnummer, ort, land |
| Clearingnummer, Kontonummer | string? | Bankuppgifter (krypterade) |
| Skattetabell, Skattekolumn | int | Skatteuppgifter (tabell 30–36, kolumn 1–6) |
| Kommun | string? | Kommunalkod (290+ kommuner) |
| KommunalSkattesats, Kyrkoavgiftssats | decimal | Skattesatser |
| HarKyrkoavgift, HarJamkning | bool | Skatteinställningar |
| JamkningBelopp | decimal | Jämkningsbelopp |
| Anstallningar | List | Alla anställningar |

Metoder: `Skapa()`, `UppdateraKontaktuppgifter()`, `UppdateraBankuppgifter()`, `UppdateraSkatteuppgifter()`, `LaggTillAnstallning()`, `AktivAnstallning()`, `AktivaAnstallningar()`

Domänhändelser: `EmployeeCreatedEvent`, `EmploymentCreatedEvent`, `EmploymentEndedEvent`, `SalaryChangedEvent`

**Employment** (Entitet)
| Egenskap | Typ | Beskrivning |
|----------|-----|-------------|
| AnstallId | EmployeeId | Koppling till anställd |
| EnhetId | OrganizationId | Organisationsenhet |
| Anstallningsform | EmploymentType | Tillsvidare/Vikariat/SAVA/Säsong |
| Kollektivavtal | CollectiveAgreementType | AB/HOK |
| Manadslon | Money | Månadslön i SEK |
| Sysselsattningsgrad | Percentage | 0–100% |
| Giltighetsperiod | DateRange | Start- och slutdatum |
| BESTAKod, AIDKod | string? | Statistikkoder |
| Befattningstitel | string? | Titel |

Metoder: `Skapa()`, `AndraLon()`, `AndraSysselsattningsgrad()`, `AvslutaAnstallning()`, `BeraknaDaglon()`, `BeraknaTimlon()`

**OrganizationUnit** (Aggregatrot)
| Egenskap | Typ | Beskrivning |
|----------|-----|-------------|
| Namn | string | Enhetsnamn |
| Typ | OrganizationUnitType | Region/Förvaltning/Verksamhet/Enhet |
| OverordnadEnhetId | OrganizationId? | Förälder i trädstruktur |
| Kostnadsstalle | string? | Kostnadskod |
| CFARKod | string? | CFAR-arbetsställekod |
| ChefId | EmployeeId? | Enhetens chef |
| Giltighet | DateRange | Temporal giltighet |

**Kontrakt (ICoreHRModule):**
- `GetEmployeeAsync(EmployeeId)` → EmployeeDto
- `GetActiveEmploymentAsync(EmployeeId, DateOnly)` → EmploymentDto
- `GetActiveEmploymentsAsync(EmployeeId, DateOnly)` → List<EmploymentDto>
- `GetEmployeesByUnitAsync(OrganizationId, DateOnly)` → List<EmployeeDto>
- `GetOrganizationUnitAsync(OrganizationId)` → OrganizationUnitDto

---

### 3.2 Payroll
**Sökväg:** `src/Modules/Payroll/` | **Schema:** `payroll` | **Tester:** 105

Fullständig svensk löneberäkning med skattetabeller, kollektivavtalsregler, arbetsgivaravgifter, pension, OB, övertid, sjuklön, semester och retroaktiva omberäkningar.

#### Beräkningsmotor (PayrollCalculationEngine)

**10-stegs brutto-till-netto-pipeline:**
1. Grundlön (månadslön × sysselsättningsgrad × arbetade dagar)
2. OB-tillägg (vardag kväll/natt, helg, storhelg per AB/HOK)
3. Övertid (enkel 180%, kvalificerad 240%)
4. Jour/beredskap (passiv/aktiv timlön)
5. Sjuklön (karensavdrag 20%, 80% dag 2–14)
6. Semester (sammalöneregeln 0.80%/dag, tillägg 0.605%)
7. Skatt (skattetabeller 290+ kommuner, 6 kolumner)
8. Arbetsgivaravgifter (31.42% standard, 20.81% ungdom <23, 10.21% senior 67+)
9. Pension AKAP-KR (6% under 7.5 IBB, 31.5% över)
10. Avdrag (löneutmätning, fackavgift)

**Konstanter:**
| Konstant | 2025 | 2026 |
|----------|------|------|
| IBB (inkomstbasbelopp) | 80 600 kr | 83 400 kr |
| PBB (prisbasbelopp) | 58 800 kr | 58 800 kr |
| AG-avgift standard | 31.42% | 31.42% |
| AG-avgift ungdom (19–22) | 20.81% | 20.81% |
| AG-avgift senior (67+) | 10.21% | 10.21% |
| AKAP-KR under 7.5 IBB | 6% | 6% |
| AKAP-KR över 7.5 IBB | 31.5% | 31.5% |
| Semester per AB | 0.80%/dag | 0.80%/dag |
| Semestertillägg | 0.605% | 0.605% |

#### Domänentiteter

**PayrollRun** (Aggregatrot)
| Egenskap | Typ |
|----------|-----|
| Year, Month | int |
| Period | string ("202603") |
| Status | PayrollRunStatus |
| TotalBrutto, TotalNetto, TotalSkatt, TotalArbetsgivaravgifter | Money |
| AntalAnstallda | int |
| ArRetroaktiv | bool |
| StartadAv, GodkandAv | string |

Status: `Skapad → Påbörjad → Beräknad → Granskad → Godkänd → Utbetald`

**PayrollResult** — 25+ monetära fält per anställd och månad:
Brutto, SkattepliktBrutto, Skatt, Netto, Arbetsgivaravgifter, Semesterlon, Semestertillagg, Pensionsgrundande, Pensionsavgift, OBTillagg, Overtidstillagg, JourTillagg, BeredskapsTillagg, Sjuklon, Karensavdrag, ForaldraloneUtfyllnad, Loneutmatning, Fackavgift, OvrigaAvdrag + Rader (PayrollResultLine[])

**TaxTable** — Skattetabeller med rader (InkomstFran, InkomstTill, Skattebelopp)

**SalaryCode** — Lönearter: Månadslön, OB-tillägg, Övertid, Semesterlön, Sjuklön, Karensavdrag, Traktamente, Milersättning

**RetroactiveRecalculationEngine** — Omberäknar vilken period som helst, genererar differensrader

**SvenskaHelgdagar** — Beräknar röda dagar, påsk, midsommar, alla helgons dag. Bestämmer OB-kategori per datum/klockslag.

**CollectiveAgreementRulesEngine (ICollectiveAgreementRulesEngine):**
- `GetOBRateAsync()` — OB-satser per tidsfönster
- `GetOvertimeRulesAsync()` — Övertidsfaktorer (enkel 180%, kvalificerad 240%)
- `GetVacationRulesAsync()` — Semesterregler per AB (25/31/32 dagar)
- `GetSickPayRulesAsync()` — Sjuklöneregler (karens, 80% dag 2–14)
- `GetJourReglerAsync()` — Jourregler (passiv/aktiv)
- `GetBeredskapsReglerAsync()` — Beredskapsregler
- `GetForaldraloneReglerAsync()` — Föräldralöne-utfyllnad (180 dagar, 10%)

---

### 3.3 Scheduling
**Sökväg:** `src/Modules/Scheduling/` | **Schema:** `scheduling` | **Tester:** 57

Schemaläggning för 24/7 sjukvård, instämpling, tidrapportering och bemanningsoptimering.

#### Domänentiteter

**Schedule** (Aggregatrot)
| Typ | Beskrivning |
|-----|-------------|
| Grundschema | Cykliskt (t.ex. 4-veckors rotation) |
| Periodschema | Tidsbegränsat (sommar, helger) |
| Operativt | Dagliga justeringar |

Status: `Utkast → Publicerad → Arkiverad`

**ScheduledShift** — Enskilt pass
| Egenskap | Typ |
|----------|-----|
| Datum | DateOnly |
| PassTyp | Dag/Kväll/Natt/Jour/Beredskap/Delat |
| PlaneradStart, PlaneradSlut | TimeOnly |
| FaktiskStart, FaktiskSlut | TimeOnly? |
| Rast | TimeSpan |
| OBKategori | Ingen/VardagKvall/VardagNatt/Helg/Storhelg |
| Status | Planerad/Pågående/Avslutad/Avbokad/Bytt |

Metoder: `StamplaIn()`, `StamplaUt()`, `RegistreraAvvikelse()`

**Timesheet** — Månadsvis tidrapport
| Egenskap | Typ |
|----------|-----|
| AnstallId | Guid |
| Ar, Manad | int |
| PlaneradeTimmar, FaktiskaTimmar, Overtid | decimal |
| Avvikelse | decimal (beräknad) |
| Status | Öppen/Inskickad/Godkänd/Avslagen |

Status: `Öppen → Inskickad → Godkänd/Avslagen (→ Återöppnad)`

**TimeClockEvent** — Instämpling
| Egenskap | Typ |
|----------|-----|
| Typ | In/Ut/Raststart/Rastslut |
| Kalla | Webbterminal/PWA/Manuell/Integration |
| Tidpunkt | DateTime |
| IPAdress, Latitud, Longitud | string?/double? |
| ArOfflineStampling | bool |

**ShiftSwapRequest** — Passbyte
Status: `Begärd → Erbjuden → Accepterad → Godkänd/Avvisad/Makulerad`

**StaffingTemplate** — Bemanningskrav per enhet och veckodag

**ArbetstidslagenValidator** — Validerar mot ATL (SFS 1982:673):
| Regel | Gränsvärde |
|-------|-----------|
| Dygnsvila | Min 11h (9h undantag sjukvård) |
| Veckovila | Min 36h sammanhängande |
| Ordinarie veckoarbetstid | Max 40h |
| Övertid per 4 veckor | Max 48h |
| Övertid per år | Max 200h |
| Nattarbete | Max 8h/24h |
| EU-snitt | Max 48h/vecka över 4 månader |

---

### 3.4 Case Management
**Sökväg:** `src/Modules/CaseManagement/` | **Schema:** `case_mgmt` | **Tester:** 11

Ärendehantering med konfigurerbart godkännandeflöde.

**Case** (Aggregatrot)
| Typ | Beskrivning |
|-----|-------------|
| Frånvaro | Semester, sjuk, föräldraledighet, VAB |
| Anställningsändring | Löneändring, sysselsättningsgrad |
| Löneändring | Individuell lönejustering |
| Omplacering | Byte av enhet |
| Rehabilitering | Kopplas till HälsoSAM |
| LAS | LAS-relaterade ärenden |

Status: `Öppnad → Under behandling → Väntar godkännande → Godkänd → Avslutad`

Innehåller: `CaseApproval[]` (godkännandesteg), `CaseComment[]` (kommentarer), `AbsenceData` (frånvarospecifik data med typ, datum, omfattning)

Domänhändelse: `CaseApprovedEvent`

---

### 3.5 LAS
**Sökväg:** `src/Modules/LAS/` | **Schema:** `las` | **Tester:** 16

Uppföljning av lagen om anställningsskydd (SFS 1982:80, reformerad 2022-10-01).

**LASAccumulation** (Aggregatrot)
| Regel | Gränsvärde |
|-------|-----------|
| SAVA → tillsvidare | 365 dagar i 5-årsperiod (5a§) |
| Vikariat → tillsvidare | 730 dagar i 5-årsperiod |
| Företrädesrätt SAVA | Efter 274 dagar/3 år, varar 9 månader |
| Företrädesrätt vikariat | Efter 365 dagar/3 år, varar 9 månader |
| 3-i-månadsregeln | 3+ avtal samma månad → mellanliggande räknas |
| Kedjeregeln | SAVA + vikariat utan 6 mån uppehåll sammanräknas |
| Turordning | Sist in först ut, max 3 undantag per omgång (22§) |

Status: `Under gräns → Nära gräns (305d) → Kritiskt nära (335d) → Konverterad (365d)`

Alarmtrösklar: 305, 335, 365 dagar

Domänhändelse: `LASConversionTriggeredEvent`

---

### 3.6 HälsoSAM
**Sökväg:** `src/Modules/HalsoSAM/` | **Schema:** `halsosam` | **Tester:** 17

Rehabiliteringshantering enligt SFB + AFS.

**RehabCase** (Aggregatrot)

Triggers: `6+ tillfällen/12 mån`, `14+ sammanhängande dagar`, `Mönster detekterat`, `Chef-initierat`, `Medarbetare-initierat`

Rehabkedja: `Signal → Under utredning → Aktiv rehab → Uppföljning → Avslutad`

Uppföljningspunkter: Dag 14, 90, 180, 365

Innehåller: `RehabNote[]` (anteckningar), `RehabUppfoljning[]` (genomförda uppföljningar)

Regler:
- Läkarintyg krävs från dag 8
- FK-anmälan inom 7 dagar efter dag 14 (senast dag 21)
- Max 10 karensavdrag per 12 månader
- GDPR: hälsodata gallras 2 år efter avslutat ärende

---

### 3.7 Salary Review
**Sökväg:** `src/Modules/SalaryReview/` | **Schema:** `salary_review` | **Tester:** 11

Löneöversynsrundor med budgetfördelning och facklig avstämning.

**SalaryReviewRound** (Aggregatrot)
- Budget fördelas per organisationshierarki
- Chefsförslag per anställd med motivering
- Facklig avstämning (aggregerad statistik per avtalsområde)
- Genomförande med retroaktiv utbetalning

Status: `Planering → Facklig avstämning → Godkänd → Genomförd`

**SalaryProposal** — Löneförslag per anställd:
NuvarandeLon, ForeslagenLon, Okning, OkningProcent, Motivering

---

### 3.8 Travel
**Sökväg:** `src/Modules/Travel/` | **Schema:** `travel` | **Tester:** 11

Resekrav och utlägg per Skatteverkets satser.

**TravelClaim** (Aggregatrot)
| Sats | Belopp |
|------|--------|
| Traktamente heldag inrikes | 260 kr |
| Traktamente halvdag inrikes | 130 kr |
| Milersättning (bil) | 25 kr/mil |

Status: `Utkast → Inskickad → Godkänd → Utbetald/Avslagen`

Innehåller: `ExpenseItem[]` (utlägg med kvitto-ID)

---

### 3.9 Recruitment
**Sökväg:** `src/Modules/Recruitment/` | **Schema:** `recruitment` | **Tester:** 14

Rekrytering med annonsering, ansökningshantering, bedömning, kommunikationsmallar och onboarding.

**Vacancy** (Aggregatrot)
Status: `Utkast → Publicerad → Stängd → Tillsatt`
Flaggor: PubliceradExternt, PubliceradPlatsbanken

**Application** — Ansökan
Status: `Mottagen → Under granskning → Intervju → Erbjudande → Anställd/Avslagen`
Metoder: `Bedoma()`, `BjudInIntervju()`, `ErbjudTjanst()`, `Avsluta()`

**CommunicationTemplate** — Kommunikationsmallar
Typer: `Kallelse`, `Avslag`, `Erbjudande`, `Onboarding`

**OnboardingChecklist** — Onboarding med 6 standardsteg:
1. IT-utrustning beställd
2. Behörigheter uppsatta
3. Arbetsplats förberedd
4. Obligatoriska utbildningar bokade (HLR, brandskydd)
5. Välkomstmöte planerat
6. Mentor/fadder tilldelad

---

### 3.10 Integration Hub
**Sökväg:** `src/Modules/IntegrationHub/` | **Schema:** `integration_hub` | **Tester:** 15

Centraliserad integrationshantering med Adapter Pattern och Outbox Pattern.

#### 16 integrationsadapters

| System | Riktning | Format | Innehåll |
|--------|----------|--------|----------|
| **Skatteverket (AGI)** | Ut | XML v1.1 | Lön, förmåner, skatteavdrag per individ |
| **Nordea (betalning)** | Ut | ISO 20022 pain.001 | Nettolöner, kontoutdrag |
| **Försäkringskassan** | Båda | E-tjänst/API | Sjukanmälan dag 15+, föräldrafrånvaro |
| **Kronofogden** | Båda | E-tjänst | Löneutmätningsbeslut |
| **Skandia (pension)** | Ut | Fil | AKAP-KR premier |
| **Raindance (ekonomi)** | Ut | Fil/API | Konteringar per kostnadsställe |
| **SKR (statistik)** | Ut | Fil | Personalstatistik per BESTA/AID |
| **SCB (KLR)** | Ut | Textfil | Genomsnittslön, timmar, årsarbetare |
| **KOLL/HOSP** | In | REST API | Legitimationsverifiering (Socialstyrelsen) |
| **Epasssi** | Ut | SFTP | Friskvårdsbidrag |
| **Troman** | Båda | REST | Förtroendevaldas arvoden |
| **PowerBI** | Ut | DirectQuery | HR-analytics, KPI:er |
| **Min kompetens** | Båda | REST | Kompetensdata |
| **Diver** | Ut | Fil/DB-vy | Analysrapportdata |
| **Grade (LMS)** | Båda | REST/SCIM | Utbildningsstatus |
| **Microweb (arkiv)** | Ut | API/fil | Personalhandlingar |

**Infrastruktur:** OutboxProcessor med retry, dead-letter, revisionslogg per integration

---

### 3.11 SelfService
**Sökväg:** `src/Modules/SelfService/`

Medarbetarportal (Min Sida) med aggregerad data:

**Dashboard-endpoint** (`/api/v1/minsida/dashboard/{id}`) returnerar:
- Personuppgifter (namn, personnummer, e-post)
- Semesterbalans (tilldelning, uttagna, tillgängliga, sparade)
- Kommande pass (nästa 7 dagar)
- Öppna ledighetsansökningar
- Aktiva ärenden
- Olästa notiser
- Utgående certifikat (90 dagar)

**Lönhistorik** (`/api/v1/minsida/lonhistorik/{id}`) — 24 senaste perioderna

---

### 3.12 Audit
**Sökväg:** `src/Modules/Audit/` | **Schema:** `audit` | **Tester:** 10

Automatisk granskningslogg via EF Core SaveChangesInterceptor.

**AuditEntry**
| Egenskap | Typ | Beskrivning |
|----------|-----|-------------|
| EntityType | string | T.ex. "Employee", "PayrollRun" |
| EntityId | string | Entitetens ID |
| Action | AuditAction | Create/Update/Delete |
| OldValues | string? (JSONB) | Tidigare värden (vid Update/Delete) |
| NewValues | string? (JSONB) | Nya värden (vid Create/Update) |
| UserId | string | Användare som gjorde ändringen |
| Timestamp | DateTime | Tidpunkt |
| IpAddress | string? | IP-adress |

**AuditInterceptor** (SaveChangesInterceptor):
- Fångar automatiskt alla Add/Modify/Delete-ändringar
- Serialiserar gamla och nya värden som JSON
- Hoppar över AuditEntry och OutboxMessage (undviker oändlig loop)
- Extraherar entity-ID via reflektion

---

### 3.13 Notifications
**Sökväg:** `src/Modules/Notifications/` | **Schema:** `notifications` | **Tester:** 15

Notiseringssystem med automatiska påminnelser.

**Notification**
| Egenskap | Typ |
|----------|-----|
| Type | Info/Warning/Action/Reminder |
| Channel | InApp/Email/SMS/Push |
| IsRead | bool |
| ActionUrl | string? (djuplänk) |
| RelatedEntityType/Id | string? (dedup-nyckel) |

**NotificationTemplate** — Mallar med TitleTemplate, MessageTemplate

**NotificationReminderService** (BackgroundService, körs varje timme):
| Kontroll | Trigger | Dedup |
|----------|---------|-------|
| Sjuklön dag 8 | LakarintygKravs && !LakarintygInlamnat | 24h |
| Sjuklön dag 15 | FKAnmalanKravs && !FKAnmalanGjord | 24h |
| Utgående certifikat | GiltigTill inom 90 dagar | Veckovis |
| LAS-gränser | AckumuleradeDagar >= 300 | 24h |
| GDPR-deadlines | Deadline inom 7 dagar | 24h |
| Retention | Utgången, ej anonymiserad | 24h |

**RetentionCleanupService** (BackgroundService, körs dagligen):
- Hittar utgångna RetentionRecord
- Anropar `Anonymize()` på varje
- Loggar antal anonymiserade

---

### 3.14 Leave
**Sökväg:** `src/Modules/Leave/` | **Schema:** `leave` | **Tester:** 35

Ledighetshantering med svensk semesterlag och kollektivavtal.

**VacationBalance** — Semesterbalans per AB 25:
| Ålder | Dagar/år |
|-------|----------|
| Under 40 år | 25 |
| 40–49 år | 31 |
| 50+ år | 32 |

Max 5 års sparande. Auto-beräkning av ålder från personnummer.

**LeaveRequest** — Ledighetsansökan
| Typ | Beskrivning |
|-----|-------------|
| Semester | Ordinarie semester |
| Sjukfrånvaro | Med karensavdrag + sjuklön |
| Föräldraledighet | 480 dagar |
| VAB | Vård av sjukt barn |
| Tjänstledighet | Obetald ledighet |
| Komptid | Uttag av komptid |
| Utbildning | Utbildningsledighet |

Status: `Utkast → Inskickad → Godkänd/Avslagen/Återkallad`

Beräknar arbetsdagar automatiskt (exkluderar helger).

**Konfliktkontroll** — Kontrollerar mot schemalagda pass under ledighetsperioden.

**SickLeaveNotification** — Sjukfrånvarobevakning:
- Dag 8: läkarintyg krävs
- Dag 15: FK-anmälan krävs

---

### 3.15 Documents
**Sökväg:** `src/Modules/Documents/` | **Schema:** `documents` | **Tester:** 12

Dokumenthantering med fillagring, retention och GDPR-klassificering.

**Document**
| Kategori | Retention |
|----------|-----------|
| Lönespecifikation | 7 år (bokföringslagen) |
| Läkarintyg | 2 år efter avslutat ärende |
| Anställningsavtal | 7 år |
| Legitimation | 10 år |
| Betyg (ej anställda) | 2 år (diskrimineringslagen) |
| Övrigt | 5 år |

GDPR-klassificering: `Normal`, `Känslig`, `Särskild kategori`

**Fillagring** via `IFileStorageService`:
- Upload (multipart form data)
- Download (streaming)
- LocalFileStorageService (lokal disk, kan bytas till Azure Blob)

---

### 3.16 Performance
**Sökväg:** `src/Modules/Performance/` | **Schema:** `performance` | **Tester:** 12

Medarbetarsamtal med självbedömning, chefsbedömning och målsättning.

**PerformanceReview**
| Egenskap | Typ |
|----------|-----|
| SjalvBedomning | string (JSONB) |
| ChefsBedomning | string (JSONB) |
| OverallRating | int (1–5) |
| Malsattning | string |

Status: `Planerad → Påbörjad → Självbedömning klar → Chefsbedömning klar → Genomförd → Avslutat`

Validering: Rating 1–5, båda bedömningar krävs före genomförande.

---

### 3.17 Reporting
**Sökväg:** `src/Modules/Reporting/` | **Schema:** `reporting` | **Tester:** 6

Rapportgenerering med 8 standardrapporttyper och schemaläggning.

**Rapporttyper (med faktisk dataaggregerering):**
| Rapporttyp | Datakällor |
|------------|------------|
| Personalrostar | Employees + Employments |
| Löneregister | PayrollResults |
| Frånvarostatistik | LeaveRequests (godkända) |
| Övertidsrapport | Timesheets (övertid) |
| LAS-status | LASAccumulations |
| Bemanningsanalys | ScheduledShifts + StaffingTemplates |
| Sjukfrånvaro-KPI | LeaveRequests (sjukfrånvaro, per månad) |
| Kostnad per enhet | PayrollResults + Employments |

Alla genererar Excel (XLSX) via ReportGenerator + ExportService.

**ReportDefinition** — Schemaläggning med cron-uttryck och mottagare-epost.

**ReportExecution** — Körningshistorik med status: `Köar → Pågår → Klar/Fel`

---

### 3.18 GDPR
**Sökväg:** `src/Modules/GDPR/` | **Schema:** `gdpr` | **Tester:** 7

GDPR-efterlevnad med registerutdrag, anonymisering och retentionshantering.

**DataSubjectRequest**
| Typ | GDPR-artikel | Beskrivning |
|-----|-------------|-------------|
| Registerutdrag | Art 15 | Samlar data från alla moduler |
| Radering | Art 17 | Rätt att bli glömd (med undantag) |
| Dataportabilitet | Art 20 | Maskinläsbart format |
| Rättelse | Art 16 | Korrigering av felaktiga data |

Status: `Mottagen → Under behandling → Klar/Avslagen`
Deadline: 30 dagar från mottagande.
`ArForsenad` — automatisk flaggning vid passerad deadline.

**RegisterutdragGenerator** — Aggregerar data från 10+ moduler:
- Personuppgifter, anställningar
- Ärenden, löneresultat (24 mån)
- Ledighetsansökningar, dokument
- Certifieringar, medarbetarsamtal
- LAS-uppföljning, rehabärenden
- Granskningslogg (100 senaste)

Exporterar som JSON-fil.

**RetentionRecord** — Retentionsspårning med automatisk anonymisering via RetentionCleanupService.

---

### 3.19 Competence
**Sökväg:** `src/Modules/Competence/` | **Schema:** `competence` | **Tester:** 6

Kompetensregister med certifieringar, legitimationer och obligatoriska utbildningar.

**Certification**
| Typ | Exempel |
|-----|---------|
| Legitimation | Sjuksköterskelegitimation (Socialstyrelsen) |
| Specialisering | Anestesisjuksköterska |
| Obligatorisk utbildning | HLR, brandskydd, hygien |
| Frivillig utbildning | Kurs i palliativ vård |
| Certifikat | ISO-certifiering |

Status beräknas dynamiskt:
- `Giltig` — GiltigTill > idag + 90 dagar
- `Utgår snart` — GiltigTill inom 90 dagar
- `Utgången` — GiltigTill < idag
- `Saknas` — Ingen giltig certifiering

**MandatoryTraining** — Obligatoriska utbildningar per roll med giltighetstid i månader.

---

### 3.20 Positions
**Sökväg:** `src/Modules/Positions/` | **Schema:** `positions` | **Tester:** 10

Positionshantering med headcount-styrning, budgetering och efterträdarplanering.

**Position** (Aggregatrot)
| Egenskap | Typ | Beskrivning |
|----------|-----|-------------|
| Titel | string | Befattningstitel |
| EnhetId | Guid | Organisationsenhet |
| BESTAKod, AIDKod | string? | Statistikkoder |
| Status | PositionStatus | Aktiv/Vakant/Frysta/Avvecklad |
| BudgeteradManadslon | decimal | Budgeterad månadslön |
| Sysselsattningsgrad | decimal | FTE 0-100% |
| InnehavareAnstallId | Guid? | Nuvarande innehavare |
| EftertradarePlanerad | Guid? | Planerad efterträdare |
| KravdaKompetenser | List&lt;string&gt; | Kompetenskrav |
| Historik | List&lt;PositionHistorik&gt; | Historik över innehavare |

Metoder: `Skapa()`, `Tillsatt()`, `Vakansatt()`, `Frys()`, `Avveckla()`, `SattEftertrardare()`, `UppdateraBudget()`, `LaggTillKompetenskrav()`

**PositionHistorik** — Spårning av innehavarbyten med anledning och tidpunkt.

**HeadcountPlan** — Budget vs faktisk bemanning per enhet och år:
- BudgeteradePositioner, BudgeteradFTE, BudgeteradKostnad
- FaktiskaPositioner, FaktiskFTE, FaktiskKostnad
- Avvikelse (beräknad)

---

### 3.21 Offboarding
**Sökväg:** `src/Modules/Offboarding/` | **Schema:** `offboarding` | **Tester:** 9

Avslutprocess med checklista, exit-samtal och rehire-markering.

**OffboardingCase** (Aggregatrot)

Avslutanledning: `Egen begäran`, `Uppsägning`, `Pension`, `Vikariat slut`, `Provanställning avbruten`, `Övergång`, `Dödsfall`, `Annat`

Status: `Skapad → Pågår → Slutförd`

**8 standardsteg vid skapande:**
1. Återlämning av utrustning (dator, telefon, nycklar, passerkort)
2. Stängning av IT-behörigheter och systemkonton
3. Slutdokument: tjänstgöringsintyg utfärdat
4. Slutdokument: arbetsgivarintyg (AF) utfärdat
5. Slutlön beräknad (semester, komptid, övertid)
6. Exit-samtal genomfört
7. Kunskapsöverföring genomförd
8. GDPR-gallringsplan upprättad

Metoder: `Skapa()`, `MarkeraSomPagar()`, `MarkeraStegKlart()`, `RegistreraExitSamtal()`, `SattReHireStatus()`, `Slutfor()`

Rehire-funktionalitet: `ArReHireEligible`, `ReHireKommentar`

---

### 3.22 Benefits
**Sökväg:** `src/Modules/Benefits/` | **Schema:** `benefits` | **Tester:** 11

Förmånskatalog med anmälningsflöde och livshändelsehantering.

**Benefit** — Förmånskatalog
| Kategori | Exempel |
|----------|---------|
| Friskvård | Friskvårdsbidrag 5 000 kr/år |
| Pension | Tilläggsuppgörelse |
| Försäkring | Sjukvårdsförsäkring |
| Tjänstebil | Bilförmån |
| Sjukvård | Privat sjukvård |
| Utbildning | Kompetensutveckling |

Egenskaper: MaxBelopp, ArbetsgivarAndel/ArbetstagarAndel (procent), ArSkattepliktig, EligibilityRegler (JSON)

**EmployeeBenefit** — Anmälningsflöde
Status: `Ansökt → Aktiv → Avslutad/Nekad`
Stöd för livshändelse (LivshandardAnledning): giftermål, barn, flytt etc.

---

### 3.23 LMS
**Sökväg:** `src/Modules/LMS/` | **Schema:** `lms` | **Tester:** 17

Utbildnings- och kurshantering med lärstigar och resultatspårning.

**Course** — Kurskatalog
| Format | Beskrivning |
|--------|-------------|
| Klassrum | Fysisk utbildning |
| E-learning | Digital kurs |
| Blandat | Kombination |
| Workshop | Praktisk workshop |

Status: `Utkast → Publicerad → Arkiverad`
Egenskaper: LangdMinuter, ArObligatorisk, GiltighetManader, MaxDeltagare

**CourseEnrollment** — Kursanmälan med resultat
Status: `Anmäld → Påbörjad → Genomförd/Underkänd/Avbruten`
Godkänt: resultat >= 70 (av 100). GiltigTill beräknas automatiskt.

**LearningPath** — Lärstigar per roll
Steg med ordning och obligatorisk/valfri markering. Kopplas till roll (t.ex. "Sjuksköterska").

---

### 3.24 Configuration
**Sökväg:** `src/Modules/Configuration/` | **Schema:** `configuration` | **Tester:** 29

Multi-tenant-konfiguration, custom fields och konfigurerbart arbetsflöde.

**TenantConfiguration** — Organisationsinställningar
Egenskaper: TenantNamn, Organisationsnummer, Land, Sprak, Valuta, LogoUrl, Konfiguration (JSON)

**CustomField** — Användardefinierade fält
| Fälttyp | Beskrivning |
|---------|-------------|
| Text | Fritext |
| Nummer | Numeriskt värde |
| Datum | Datumfält |
| Valval | Enval (dropdown) |
| Flerval | Flerval (checkboxar) |
| JaNej | Boolean |

Target: `Anstalld`, `Anstallning`, `Organisation`, `Arende`, `Vakans`

**CustomFieldValue** — Värden kopplade till entiteter via EntityId.

**WorkflowDefinition** — Konfigurerbart arbetsflöde
StegDefinition som JSON-array. Kopplas till entitetstyp.

---

### 3.25 Analytics
**Sökväg:** `src/Modules/Analytics/` | **Schema:** `analytics` | **Tester:** 11

Ad hoc-rapportbyggare och dashboards.

**SavedReport** — Sparade rapportfrågor
QueryDefinition som JSON: entitet, fält, filter, gruppering, sortering.
Visualisering: tabell, stapel, linje, cirkel.
Delningsfunktion mellan användare.

**Dashboard** — Anpassningsbara dashboards
Layout som JSON (widgetpositioner). Ägare-baserat med delning.

---

### Utökade funktioner i befintliga moduler

#### Documents — Utökat med workflow, e-signering och versioner
- **DocumentTemplate**: Dokumentmallar med merge fields ({{Fornamn}}, {{Efternamn}} etc.), genererar dokument från mall + persondata
- **DocumentSignature**: E-signeringsflöde med ordning, status (Väntar/Signerad/Nekad/Utgången), IP-loggning
- **DocumentVersion**: Versionshistorik per dokument med ändringsbeskrivning

#### Core HR — Utökat med nödkontakter
- **EmergencyContact**: Nödkontakter per anställd med namn, relation, telefon, e-post, primärmarkering

#### Recruitment — Utökat med djupare ATS
- **RequisitionApproval**: Godkännandeflöde för vakanser (VäntarGodkännande/Godkänd/Nekad)
- **InterviewSchedule**: Intervjuplanering med tidpunkt, plats, flera intervjuare, anteckningar
- **Scorecard**: Bedömningsmatris med 4 dimensioner (1-5): kompetens, erfarenhet, personlighet, motivation
- **TalentPoolEntry**: Kandidatpool för framtida rekrytering med kompetensområde och ursprungsansökan

#### Infrastruktur — Utökat med fältnivåbehörighet
- **FieldPermission** (ABAC): Fältnivåbehörighet per roll och entitetstyp. Nivåer: Full, Läsa, Maskerad, Dold
- **DelegatedAccess**: Tidsbegränsad rolldelgering med anledning och giltighetskontroll

---

## 4. Infrastruktur

### 4.1 Export
| Tjänst | Användning |
|--------|------------|
| ExportService | CSV (semikolonseparerad UTF-8) och Excel (XLSX med formatering) |
| PdfPayslipGenerator | Lönespecifikationer (A4, tabeller, sammanställning, arbetsgivarkostnader) |
| ReportGenerator | 8 rapporttyper → Excel |

### 4.2 Fillagring
| Interface | Implementation |
|-----------|---------------|
| IFileStorageService | LocalFileStorageService (lokal disk, kan bytas till Azure Blob) |

Upload: `POST /api/v1/dokument/upload` (multipart form)
Download: `GET /api/v1/dokument/{id}/download` (streaming)

### 4.3 Bakgrundsjobb
| Tjänst | Intervall | Kontroller |
|--------|-----------|------------|
| NotificationReminderService | 1 timme | 5 kontroller (sjuklön, certifikat, LAS, GDPR, retention) |
| RetentionCleanupService | 24 timmar | Anonymiserar utgångna retentionsposter |

### 4.4 Observerbarhet
- **OpenTelemetry** — Tracing och metrics med ASP.NET Core-instrumentering
- **RequestLoggingMiddleware** — Loggar metod, sökväg, statuskod, svarstid för alla API-anrop
- **Rate Limiting** — 100 req/min standard, 10 req/min för export

### 4.5 Granskningslogg
- **AuditInterceptor** (SaveChangesInterceptor) — automatisk loggning av alla Create/Update/Delete
- Serialiserar gamla och nya värden som JSONB
- Undantar AuditEntry och OutboxMessage

---

## 5. API-endpoints

### 5.1 System (4 endpoints)
| Metod | Sökväg | Auth | Beskrivning |
|-------|--------|------|-------------|
| GET | `/health` | Anonym | Hälsokontroll |
| POST | `/dev/token` | Anonym (dev) | Generera JWT-token |
| GET | `/api/v1/integration/status` | Systemadmin | 14 integrationer |
| GET | `/api/v1/integration/koll/verifiera/{pnr}` | Systemadmin | KOLL/HOSP-verifiering |

### 5.2 Core HR (6 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/anstallda?sok&sida&perSida` | Lista anställda (paginerad) |
| GET | `/api/v1/anstalld/{id}` | Hämta anställd |
| POST | `/api/v1/anstalld` | Skapa anställd |
| PUT | `/api/v1/anstalld/{id}` | Uppdatera kontaktuppgifter |
| GET | `/api/v1/organisation` | Lista organisationsenheter |
| GET | `/api/v1/organisation/{id}/anstallda?datum` | Anställda per enhet |

### 5.3 Ärenden (3 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/arenden/` | Lista ärenden |
| POST | `/api/v1/arenden/franvaro` | Skapa frånvaroärende |
| GET | `/api/v1/arenden/{id}` | Hämta ärende med godkännanden |

### 5.4 Lön (13 endpoints, auth: LonOchHR)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/lon/korningar?ar` | Lista lönekörningar |
| GET | `/api/v1/lon/korning/{id}` | Hämta körning |
| POST | `/api/v1/lon/korning` | Skapa körning |
| POST | `/api/v1/lon/korning/{id}/berakna` | Beräkna |
| POST | `/api/v1/lon/korning/{id}/godkann` | Godkänn |
| POST | `/api/v1/lon/korning/{id}/utbetala` | Markera utbetald |
| GET | `/api/v1/lon/korning/{id}/resultat` | Löneresultat per körning |
| GET | `/api/v1/lon/resultat/{anstId}/{ar}/{man}` | Resultat per anställd |
| GET | `/api/v1/lon/resultat/{anstId}/{ar}/{man}/pdf` | PDF-lönespecifikation |
| POST | `/api/v1/lon/korning/{id}/export/agi` | AGI-XML (Skatteverket) |
| POST | `/api/v1/lon/korning/{id}/export/betalning` | pain.001 (Nordea) |
| GET | `/api/v1/lon/skattetabeller/{ar}` | Skattetabeller |
| GET | `/api/v1/lon/lonearter` | Lönearter |

### 5.5 Schema (13 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/schema/` | Lista scheman |
| GET | `/api/v1/schema/{id}` | Hämta schema med pass |
| POST | `/api/v1/schema/grundschema` | Skapa grundschema |
| POST | `/api/v1/schema/periodschema` | Skapa periodschema |
| POST | `/api/v1/schema/{id}/pass` | Lägg till pass |
| POST | `/api/v1/schema/{id}/publicera` | Publicera |
| POST | `/api/v1/schema/optimera` | AI-schemaoptimering |
| POST | `/api/v1/stampling/in` | Stämpla in |
| POST | `/api/v1/stampling/ut` | Stämpla ut |
| GET | `/api/v1/stampling/status/{id}` | Instämplingsstatus |
| GET | `/api/v1/stampling/historik/{id}?fran&till` | Stämplingshistorik |
| GET | `/api/v1/bemanning/{enhetId}/{datum}` | Bemanningsöversikt |
| GET | `/api/v1/schema/avvikelser/{datum}` | Avvikelser |

### 5.6 Tidrapporter (6 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/tidrapporter/?anstallId&ar&manad` | Lista tidrapporter |
| POST | `/api/v1/tidrapporter/` | Skapa tidrapport |
| POST | `/api/v1/tidrapporter/{id}/registrera` | Registrera timmar |
| POST | `/api/v1/tidrapporter/{id}/skickain` | Skicka in |
| POST | `/api/v1/tidrapporter/{id}/godkann` | Godkänn |
| POST | `/api/v1/tidrapporter/{id}/avvisa` | Avvisa |

### 5.7 LAS (4 endpoints, auth: ChefEllerHR)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/las/ackumuleringar?status` | LAS-ackumuleringar |
| GET | `/api/v1/las/alarmeringar` | Alarm |
| GET | `/api/v1/las/foretradesratt` | Företrädesrättsinnehavare |
| GET | `/api/v1/las/dashboard` | Dashboard med KPI:er |

### 5.8 HälsoSAM (8 endpoints, auth: ChefEllerHR)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/halsosam/arenden?status` | Lista rehabärenden |
| GET | `/api/v1/halsosam/arende/{id}` | Hämta ärende |
| POST | `/api/v1/halsosam/arende` | Skapa rehabärende |
| POST | `/api/v1/halsosam/arende/{id}/tilldela` | Tilldela ärendeägare |
| POST | `/api/v1/halsosam/arende/{id}/rehabplan` | Sätt rehabplan |
| POST | `/api/v1/halsosam/arende/{id}/anteckning` | Lägg till anteckning |
| POST | `/api/v1/halsosam/arende/{id}/avsluta` | Avsluta |
| GET | `/api/v1/halsosam/kommande-uppfoljningar?dagar` | Kommande uppföljningar |

### 5.9 Löneöversyn (5 endpoints, auth: ChefEllerHR)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/loneoversyn/rundor?ar` | Lista rundor |
| GET | `/api/v1/loneoversyn/runda/{id}` | Hämta runda |
| POST | `/api/v1/loneoversyn/runda` | Skapa runda |
| POST | `/api/v1/loneoversyn/runda/{id}/forslag` | Lägg till löneförslag |
| POST | `/api/v1/loneoversyn/runda/{id}/genomfor` | Genomför |

### 5.10 Resor (3 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/resor/?anstallId` | Lista resekrav |
| POST | `/api/v1/resor/` | Skapa resekrav |
| POST | `/api/v1/resor/{id}/utlagg` | Lägg till utlägg |
| POST | `/api/v1/resor/{id}/skickain` | Skicka in |

### 5.11 Rekrytering (10 endpoints, auth: ChefEllerHR)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/rekrytering/vakanser?status` | Lista vakanser |
| GET | `/api/v1/rekrytering/vakans/{id}` | Hämta vakans |
| POST | `/api/v1/rekrytering/vakans` | Skapa vakans |
| POST | `/api/v1/rekrytering/vakans/{id}/publicera` | Publicera |
| POST | `/api/v1/rekrytering/vakans/{id}/ansok` | Skicka ansökan |
| GET | `/api/v1/rekrytering/mallar` | Kommunikationsmallar |
| POST | `/api/v1/rekrytering/mall` | Skapa mall |
| GET | `/api/v1/rekrytering/onboarding?anstallId` | Onboarding-checklistor |
| POST | `/api/v1/rekrytering/onboarding` | Skapa checklista |
| POST | `/api/v1/rekrytering/onboarding/{id}/klara/{index}` | Markera steg klart |

### 5.12 Integrationer (1 endpoint, auth: Systemadmin)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/integration/adapters` | Lista alla 16 adapters |

### 5.13 Audit (1 endpoint, auth: Systemadmin)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/audit/?entityType&entityId&take` | Sök i granskningslogg |

### 5.14 Notiser (5 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/notiser/?userId` | Lista notiser |
| GET | `/api/v1/notiser/olasta?userId` | Antal olästa |
| POST | `/api/v1/notiser/` | Skapa notis |
| POST | `/api/v1/notiser/{id}/las` | Markera läst |
| POST | `/api/v1/notiser/las-alla?userId` | Markera alla lästa |

### 5.15 Ledighet (10 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/ledighet/balanser?anstallId&ar` | Semesterbalans |
| POST | `/api/v1/ledighet/balans` | Skapa balans (manuell ålder) |
| POST | `/api/v1/ledighet/balans/auto` | Skapa balans (auto ålder från pnr) |
| GET | `/api/v1/ledighet/ansokngar?anstallId` | Lista ansökningar |
| POST | `/api/v1/ledighet/ansokan` | Skapa ansökan |
| POST | `/api/v1/ledighet/ansokan/{id}/skickain` | Skicka in |
| POST | `/api/v1/ledighet/ansokan/{id}/godkann` | Godkänn |
| POST | `/api/v1/ledighet/ansokan/{id}/avvisa` | Avvisa |
| GET | `/api/v1/ledighet/ansokan/{id}/konflikter` | Schemakonflikter |
| GET | `/api/v1/ledighet/sjukanmalan?anstallId` | Sjukanmälningar |

### 5.16 Dokument (6 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/dokument/?anstallId` | Lista dokument |
| POST | `/api/v1/dokument/` | Skapa dokumentpost |
| GET | `/api/v1/dokument/{id}` | Hämta metadata |
| POST | `/api/v1/dokument/{id}/arkivera` | Arkivera |
| POST | `/api/v1/dokument/upload` | Ladda upp fil (multipart) |
| GET | `/api/v1/dokument/{id}/download` | Ladda ner fil |

### 5.17 Medarbetarsamtal (7 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/medarbetarsamtal/?ar` | Lista samtal |
| GET | `/api/v1/medarbetarsamtal/{id}` | Hämta samtal |
| POST | `/api/v1/medarbetarsamtal/` | Skapa samtal |
| POST | `/api/v1/medarbetarsamtal/{id}/sjalvbedomning` | Självbedömning |
| POST | `/api/v1/medarbetarsamtal/{id}/chefsbedomning` | Chefsbedömning + betyg |
| POST | `/api/v1/medarbetarsamtal/{id}/malsattning` | Målsättning |
| POST | `/api/v1/medarbetarsamtal/{id}/genomfor` | Genomför |

### 5.18 Rapporter (5 endpoints, auth: ChefEllerHR)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/rapporter/` | Lista rapportdefinitioner |
| POST | `/api/v1/rapporter/` | Skapa rapportdefinition |
| POST | `/api/v1/rapporter/{id}/schemalagd` | Schemalägg rapport |
| GET | `/api/v1/rapporter/korningar?reportId` | Körningshistorik |
| POST | `/api/v1/rapporter/{id}/kor` | Kör rapport → Excel |

### 5.19 GDPR (7 endpoints, auth: Systemadmin)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/gdpr/begaran` | Lista begäran |
| POST | `/api/v1/gdpr/begaran` | Skapa begäran |
| POST | `/api/v1/gdpr/begaran/{id}/tilldela` | Tilldela handläggare |
| POST | `/api/v1/gdpr/begaran/{id}/slutfor` | Slutför |
| GET | `/api/v1/gdpr/registerutdrag/{anstallId}` | Generera registerutdrag (Art 15) |
| GET | `/api/v1/gdpr/retention` | Utgången retention |
| POST | `/api/v1/gdpr/retention/{id}/anonymisera` | Anonymisera |

### 5.20 Kompetens (5 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/kompetens/?anstallId` | Certifieringar |
| GET | `/api/v1/kompetens/utgaende?dagar` | Utgående certifikat |
| POST | `/api/v1/kompetens/` | Skapa certifiering |
| GET | `/api/v1/kompetens/obligatoriska` | Obligatoriska utbildningar |
| POST | `/api/v1/kompetens/obligatorisk` | Skapa obligatorisk utbildning |

### 5.21 Export (2 endpoints, auth: ChefEllerHR, rate limit: 10/min)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/export/anstallda?format=csv\|xlsx` | Exportera anställda |
| GET | `/api/v1/export/lonekorngar/{id}?format=csv\|xlsx` | Exportera löneresultat |

### 5.22 Min Sida (2 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/minsida/dashboard/{anstallId}` | Medarbetardashboard |
| GET | `/api/v1/minsida/lonhistorik/{anstallId}?ar` | Lönhistorik |

### 5.23 Chefsportal (3 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/chef/dashboard/{chefId}` | Chefsdashboard |
| GET | `/api/v1/chef/attestko` | Attestkö |
| GET | `/api/v1/chef/team/{enhetId}` | Teamöversikt |

### 5.24 Positioner (9 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/positioner/?enhetId` | Lista positioner |
| GET | `/api/v1/positioner/{id}` | Hämta position |
| POST | `/api/v1/positioner/` | Skapa position |
| POST | `/api/v1/positioner/{id}/tillsatt` | Tillsätt position |
| POST | `/api/v1/positioner/{id}/vakansatt` | Vakansätt |
| POST | `/api/v1/positioner/{id}/frys` | Frys position |
| POST | `/api/v1/positioner/{id}/avveckla` | Avveckla |
| GET | `/api/v1/headcount/?enhetId&ar` | Headcount-planer |
| POST | `/api/v1/headcount/` | Skapa headcount-plan |

### 5.25 Offboarding (6 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/offboarding/?anstallId` | Lista offboarding-ärenden |
| POST | `/api/v1/offboarding/` | Skapa offboarding |
| POST | `/api/v1/offboarding/{id}/steg/{index}/klar` | Markera steg klart |
| POST | `/api/v1/offboarding/{id}/exitsamtal` | Registrera exit-samtal |
| POST | `/api/v1/offboarding/{id}/rehire` | Sätt rehire-status |
| POST | `/api/v1/offboarding/{id}/slutfor` | Slutför offboarding |

### 5.26 Förmåner (6 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/formaner/` | Förmånskatalog |
| POST | `/api/v1/formaner/` | Skapa förmån |
| GET | `/api/v1/formaner/anmalningar?anstallId` | Anmälningar |
| POST | `/api/v1/formaner/anmalan` | Anmäl förmån |
| POST | `/api/v1/formaner/anmalan/{id}/godkann` | Godkänn |
| POST | `/api/v1/formaner/anmalan/{id}/avsluta` | Avsluta |

### 5.27 Utbildning / LMS (9 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/utbildning/kurser` | Kurskatalog |
| POST | `/api/v1/utbildning/kurs` | Skapa kurs |
| POST | `/api/v1/utbildning/kurs/{id}/publicera` | Publicera |
| GET | `/api/v1/utbildning/anmalningar?anstallId` | Kursanmälningar |
| POST | `/api/v1/utbildning/anmalan` | Anmäl till kurs |
| POST | `/api/v1/utbildning/anmalan/{id}/paborja` | Påbörja kurs |
| POST | `/api/v1/utbildning/anmalan/{id}/genomfor` | Genomför med resultat |
| GET | `/api/v1/utbildning/larstigar` | Lärstigar |
| POST | `/api/v1/utbildning/larstig` | Skapa lärstig |

### 5.28 Konfiguration (8 endpoints, auth: Systemadmin)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/konfiguration/tenant` | Tenant-konfiguration |
| PUT | `/api/v1/konfiguration/tenant` | Uppdatera tenant |
| GET | `/api/v1/konfiguration/custom-fields?target` | Custom fields |
| POST | `/api/v1/konfiguration/custom-field` | Skapa custom field |
| GET | `/api/v1/konfiguration/custom-values/{entityId}` | Värden per entitet |
| POST | `/api/v1/konfiguration/custom-value` | Sätt värde |
| GET | `/api/v1/konfiguration/workflows` | Arbetsflöden |
| POST | `/api/v1/konfiguration/workflow` | Skapa arbetsflöde |

### 5.29 Analytics (7 endpoints, auth: ChefEllerHR)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/analytics/rapporter?skapadAv` | Sparade rapporter |
| POST | `/api/v1/analytics/rapport` | Skapa rapport |
| PUT | `/api/v1/analytics/rapport/{id}` | Uppdatera rapport |
| POST | `/api/v1/analytics/rapport/{id}/kor` | Kör ad hoc-fråga |
| GET | `/api/v1/analytics/dashboards` | Dashboards |
| POST | `/api/v1/analytics/dashboard` | Skapa dashboard |
| PUT | `/api/v1/analytics/dashboard/{id}` | Uppdatera layout |

### 5.30 Behörighet (5 endpoints, auth: Systemadmin)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/behorighet/faltregler?roll` | Fältbehörigheter |
| POST | `/api/v1/behorighet/faltregel` | Skapa fältregel |
| GET | `/api/v1/behorighet/delegeringar?delegatId` | Delegeringar |
| POST | `/api/v1/behorighet/delegering` | Skapa delegering |
| POST | `/api/v1/behorighet/delegering/{id}/avsluta` | Avsluta delegering |

### 5.31 Utökade rekryteringsendpoints (9 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| POST | `/api/v1/rekrytering/vakans/{id}/godkannande` | Skapa godkännandebegäran |
| POST | `/api/v1/rekrytering/godkannande/{id}/godkann` | Godkänn vakans |
| POST | `/api/v1/rekrytering/godkannande/{id}/neka` | Neka vakans |
| POST | `/api/v1/rekrytering/intervju` | Boka intervju |
| POST | `/api/v1/rekrytering/intervju/{id}/genomford` | Markera intervju genomförd |
| POST | `/api/v1/rekrytering/scorecard` | Skapa bedömning |
| GET | `/api/v1/rekrytering/vakans/{id}/scorecards` | Bedömningar per vakans |
| GET | `/api/v1/rekrytering/talentpool` | Kandidatpool |
| POST | `/api/v1/rekrytering/talentpool` | Lägg till i kandidatpool |

### 5.32 Utökade dokumentendpoints (7 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/dokument/mallar` | Dokumentmallar |
| POST | `/api/v1/dokument/mall` | Skapa mall |
| POST | `/api/v1/dokument/mall/{id}/generera` | Generera från mall |
| GET | `/api/v1/dokument/{id}/signaturer` | Signaturer per dokument |
| POST | `/api/v1/dokument/{id}/signatur` | Begär signatur |
| POST | `/api/v1/dokument/signatur/{id}/signera` | Signera |
| GET | `/api/v1/dokument/{id}/versioner` | Versionshistorik |

### 5.33 Utökade självserviceendpoints (7 endpoints)
| Metod | Sökväg | Beskrivning |
|-------|--------|-------------|
| GET | `/api/v1/minsida/noddkontakter/{anstallId}` | Nödkontakter |
| POST | `/api/v1/minsida/noddkontakt` | Lägg till nödkontakt |
| PUT | `/api/v1/minsida/noddkontakt/{id}` | Uppdatera nödkontakt |
| DELETE | `/api/v1/minsida/noddkontakt/{id}` | Ta bort nödkontakt |
| PUT | `/api/v1/minsida/kontaktuppgifter/{anstallId}` | Uppdatera egna kontaktuppgifter |
| PUT | `/api/v1/minsida/bankuppgifter/{anstallId}` | Uppdatera egna bankuppgifter |
| GET | `/api/v1/minsida/arenden/{anstallId}` | Spåra egna ärenden |

---

## 6. Databasschema

### Scheman och tabeller

| Schema | Tabeller | Beskrivning |
|--------|----------|-------------|
| `core_hr` | employees, employments, organization_units | Personalregister |
| `payroll` | payroll_runs, payroll_results, payroll_result_lines, tax_tables, tax_table_rows, salary_codes | Löneberäkning |
| `scheduling` | schedules, scheduled_shifts, time_clock_events, staffing_templates, staffing_requirement_lines, shift_swap_requests, timesheets | Schema & tid |
| `case_mgmt` | cases, case_approvals, case_comments | Ärendehantering |
| `las` | accumulations, periods, events | LAS-uppföljning |
| `halsosam` | rehab_cases, rehab_notes, rehab_uppfoljningar | Rehabilitering |
| `salary_review` | salary_review_rounds, salary_proposals | Löneöversyn |
| `travel` | travel_claims, expense_items | Resor & utlägg |
| `recruitment` | vacancies, applications, communication_templates, onboarding_checklists, onboarding_items | Rekrytering |
| `integration_hub` | outbox_messages | Integrationer |
| `audit` | audit_entries | Granskningslogg |
| `notifications` | notifications, notification_templates | Notiseringar |
| `leave` | vacation_balances, leave_requests, sick_leave_notifications | Ledighet |
| `documents` | documents, document_templates, document_signatures, document_versions | Dokumenthantering + workflow |
| `performance` | performance_reviews | Medarbetarsamtal |
| `reporting` | report_definitions, report_executions | Rapportering |
| `gdpr` | data_subject_requests, retention_records | GDPR |
| `positions` | positions, position_historik, headcount_plans | Positionshantering |
| `offboarding` | offboarding_cases, offboarding_items | Avslutprocess |
| `benefits` | benefits, employee_benefits | Förmåner |
| `lms` | courses, course_enrollments, learning_paths, learning_path_steps | Utbildning |
| `configuration` | tenant_configurations, custom_fields, custom_field_values, workflow_definitions | Multi-tenant & konfiguration |
| `analytics` | saved_reports, dashboards | Ad hoc-analys |
| `authorization` | field_permissions, delegated_accesses | Fältnivåbehörighet |
| `competence` | certifications, mandatory_trainings | Kompetens |

### Datatyper
- **JSONB-kolumner:** audit_entries (old/new values), performance_reviews (bedömningar), cases (frånvarodata), report_definitions (parameter schema), report_executions (parametrar)
- **Value objects som owned types:** Address (Employee), DateRange (Employment, OrganizationUnit, StaffingTemplate)
- **Money-konvertering:** Alla monetära värden som `decimal` med automatisk konvertering
- **Krypterade fält:** Personnummer, bankuppgifter

### Index
- Unika: TaxTable (År+Tabell+Kolumn), VacationBalance (AnstallId+År), Timesheet (AnstallId+År+Månad), NotificationTemplate (TemplateKey)
- Sammansatta: AuditEntry (EntityType+EntityId), RetentionRecord (EntityType+EntityId), PerformanceReview (AnstallId+År)
- Enkla: Alla ForeignKey-kolumner, status-kolumner, datumfält

---

## 7. Säkerhet

### Autentisering
- **JWT Bearer** med stöd för Azure AD (Entra ID) och Supabase
- SAML 2.0/OIDC-stöd via Azure AD
- MFA obligatoriskt (konfigurerat i Azure AD)
- Rollextraktion från `role`-claim eller `app_metadata.role` (Supabase)

### Roller (RBAC)
| Roll | Behörighet |
|------|------------|
| Anstalld | Se egen data, Min Sida |
| Chef | Teamöversikt, attestering, ledighetshantering |
| HRAdmin | Full personalhantering |
| HRSpecialist | Specialiserad HR-hantering |
| Loneadmin | Lönehantering, skattetabeller |
| Systemadmin | Systemkonfiguration, audit, GDPR, integrationer |
| FackligRepresentant | Löneöversynsstatistik |

### Auktoriseringspolicies
| Policy | Roller |
|--------|--------|
| RequireAuthorization() | Alla autentiserade |
| ChefEllerHR | Chef, HRAdmin, HRSpecialist |
| LonOchHR | Loneadmin, HRAdmin |
| Systemadmin | Systemadmin |

### Enhetsbegränsning (UnitScopeService)
- Systemadmin/HR/Löneadmin: åtkomst till alla enheter
- Chef: åtkomst till sin enhet och underenheter
- Anställd: åtkomst till egen data

### Rate Limiting
- Standard: 100 requests/minut
- Export: 10 requests/minut

### GDPR
- Dataklass per fält: Normal, Känslig, Särskild kategori
- Ändamålsbegränsning: all åtkomst loggad
- Kryptering: TLS 1.3 i transit, AES-256 i vila
- Automatisk gallring via RetentionCleanupService
- Registerutdrag (Art 15) inom 30 dagar
- Anonymisering vid utgången retention

---

## 8. Frontend

### HTML SPA (`src/Api/wwwroot/index.html`)

**21 sidor:**
Dashboard, Anställda, Organisation, Ärenden, Lönekörningar, Schemaläggning, Tidrapportering, Chefsportal, LAS, HälsoSAM, Löneöversyn, Resor, Rekrytering, Ledighet, Medarbetarsamtal, Kompetens, Dokument, GDPR, Rapporter, Granskningslogg, Notiser, Integrationer

**11 modaler:** Ny anställd, Nytt ärende, Ny lönekörning, Nytt schema, Instämpling, Nytt rehabärende, Ny löneöversynsrunda, Nytt resekrav, Ny vakans, Ny ledighetsansökan, Nytt medarbetarsamtal, Ny certifiering, Nytt dokument, GDPR-begäran, Ny rapport

**PWA:** manifest.json, service worker (cache-first för statiska resurser, network-first för API)

**WCAG 2.1 AA:** Skip-link, ARIA-roller (navigation, main, dialog), tangentbordshantering, semantisk HTML

### Blazor Server (`src/Web/`)

**17 Razor-sidor:**
Home, Anställda (Index/Detalj/Ny), Ärenden (Index/Detalj/Nytt), Godkännanden, HälsoSAM, LAS, MinSida (Index/Lönespecifikationer/Schema), Organisation (Index/OrgNode), Schema

**7 designsystemkomponenter:** RhrAlert, RhrBadge, RhrButton, RhrCard, RhrDataTable, RhrInput, RhrTrafficLight

---

## 9. Tester

### Testinventering (388 tester, 19 projekt)

| Testprojekt | Antal | Fokusområde |
|-------------|-------|-------------|
| Payroll.Tests | 105 | Brutto-netto, skatt, OB, övertid, sjuklön, semester, AG-avgifter, retroaktiv, helgdagar |
| Scheduling.Tests | 57 | ATL-validering, schemaoptimering, instämpling, tidrapporter |
| Leave.Tests | 35 | Semesterbalans (åldersberäkning), ledighetsansökan, statusövergångar, sjukanmälan |
| SharedKernel.Tests | 22 | Personnummer (Luhn), Money, DateRange, Percentage |
| HalsoSAM.Tests | 17 | Rehabärenden, uppföljningspunkter, sjukfrånvarobevakning |
| LAS.Tests | 16 | 5-årsfonster, SAVA/vikariat, 3-i-månadsregeln, företrädesrätt, konvertering |
| IntegrationHub.Tests | 15 | AGI-XML, Nordea pain.001, Skandia pension, FK |
| Notifications.Tests | 15 | Notiseringar, ReminderService (sjuklön, certifikat, LAS, GDPR, retention) |
| Recruitment.Tests | 14 | Vakanser, ansökningar, onboarding, kommunikationsmallar |
| Documents.Tests | 12 | Dokument, retention, GDPR-klassificering |
| Performance.Tests | 12 | Medarbetarsamtal, självbedömning, chefsbedömning, genomförande |
| CaseManagement.Tests | 11 | Frånvaroärenden, godkännandeflöde, kommentarer |
| SalaryReview.Tests | 11 | Löneöversynsrundor, förslag, budget |
| Travel.Tests | 11 | Resekrav, traktamente, milersättning, utlägg |
| Audit.Tests | 10 | AuditEntry, AuditInterceptor (auto-capture) |
| GDPR.Tests | 7 | DataSubjectRequest, deadline, anonymisering |
| Competence.Tests | 6 | Certifieringar, utgångsstatus, obligatoriska utbildningar |
| Reporting.Tests | 6 | Rapportdefinitioner, körningar, schemaläggning |
| Core.Tests | 5 | Employee, anställningar, skatteinställningar, domänhändelser |
| Positions.Tests | 10 | Position livscykel, headcount, kompetenskrav, efterträdare |
| Offboarding.Tests | 9 | Avslutprocess, steg, exit-samtal, rehire, slutförande |
| Benefits.Tests | 11 | Förmåner, anmälan, godkännande, avslut |
| LMS.Tests | 17 | Kurser, anmälningar, resultat (70% godkänt), lärstigar |
| Configuration.Tests | 29 | Tenant, custom fields, värden, arbetsflöden |
| Analytics.Tests | 11 | Sparade rapporter, dashboards, delning |

---

## 10. Driftsättning

### Docker Compose
```yaml
Services:
  postgres: PostgreSQL 17 (port 54322)
  redis: Redis 7 Alpine (port 6379)
  rabbitmq: RabbitMQ 4 Management (port 5672/15672)
```

### Dockerfile
- Build: .NET 9 SDK, multi-stage
- Runtime: ASP.NET 9, non-root user, port 8080
- Miljö: Production

### Konfiguration
- Databas: `Host=localhost;Port=54322;Database=postgres;Username=postgres;Password=postgres`
- InMemory-läge: `UseInMemoryDb=true` (för utveckling/tester)
- JWT: Konfigurerbar via `Jwt:Secret`, `Jwt:Issuer`, `Jwt:Audience`
- OpenAPI: `/openapi/v1.json`

---

*Genererat 2026-03-17. RegionHR v0.6.0-fas6. 25 moduler, 204 endpoints, 475 tester.*
