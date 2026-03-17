# OpenHR — Produktionsberedskapsdesign

**Datum:** 2026-03-17
**Scope:** Göra OpenHR till ett 100% funktionsdugligt, produktionsklart HR-system
**Målgrupp:** Svenska regioner med 10 000+ anställda (sjukvård)
**Ersätter:** HEROMA (CGI)
**Licens:** AGPL-3.0 — alla forks måste förbli öppen källkod

---

## 1. Kärnprinciper

### 1.1 FOSS — 100% fri och öppen källkod

Varje komponent i systemet ska vara licensierad under en FOSS-licens (MIT, Apache 2.0, BSD, MPL, AGPL, GPL). Inga kommersiella beroenden, inga dual-license-fällor.

| Komponent | Nuvarande | Ändring | Licens |
|-----------|-----------|---------|--------|
| Runtime | .NET 9 | Behåll | MIT |
| Frontend | Blazor Server | Behåll | MIT |
| UI-komponenter | Eget (7 st) | **Byt till MudBlazor** | MIT |
| Databas | PostgreSQL 17 | Behåll | PostgreSQL License |
| ORM | EF Core 9 | Behåll | MIT |
| Cache | In-memory | Behåll (ingen Redis/Valkey behövs) | MIT |
| Meddelandekö | RabbitMQ 4 | Behåll | MPL 2.0 |
| PDF | QuestPDF | **Byt till PdfSharpCore** | MIT |
| Schemaoptimering | — | **Lägg till Google OR-Tools** | Apache 2.0 |
| Excel | ClosedXML | Behåll | MIT |
| E-post | MailKit | Behåll | MIT |
| Observerbarhet | OpenTelemetry | Behåll | Apache 2.0 |
| Realtid | — | **Lägg till SignalR** | MIT |
| Komponenttester | — | **Lägg till bUnit** | MIT |
| E2E-tester | — | **Lägg till Playwright** | Apache 2.0 |
| Tillgänglighetstest | — | **Lägg till axe-core** | MPL 2.0 |
| Prestandatest | — | **Lägg till k6** | AGPL |
| Auth | Supabase Auth | Behåll (self-hosted) | Apache 2.0 |
| Fillagring | LocalFileStorageService | Behåll befintlig abstraktion (S3-kompatibel backend i produktion) | MIT |

### 1.2 Stina-principen — systemet är experten, inte användaren

> "Stina, 62 år, utan datorerfarenhet, ska direkt kunna förstå och använda appen."

Detta innebär:

**a) Systemet fattar de svåra besluten**
Användaren beskriver vad de vill. Systemet hanterar lagkrav, kollektivavtal, beräkningar och regler automatiskt. Om något inte går föreslår systemet tydliga lösningar.

**b) En fråga i taget**
Inga komplexa formulär. Konversationsflöden med stora knappar. Vanligaste valet störst.

**c) Stinas språk, inte systemspråk**
"Jag är sjuk" istället för "Sjukanmälan". "Jag vill ha ledigt" istället för "Ledighetsansökan".

**d) Förklara, aldrig straffa**
Felmeddelanden förklarar vad som hände och vad Stina ska göra. Systemet förhindrar fel innan de uppstår genom att dölja ogiltiga val.

**e) Visa vad som händer och vad som kommer hända**
"Vi meddelar din chef Erik." "Om du är sjuk mer än 7 dagar behöver du ett läkarintyg — vi påminner dig."

### 1.3 Kraftfull under huven

Stina klickar på 2 knappar. Systemet utför 10 operationer. All komplexitet — skatteberäkning, ATL-validering, LAS-ackumulering, kollektivavtalsregler, GDPR-loggning — sker osynligt i bakgrunden.

### 1.4 Flerspråkigt

- Alla UI-texter via ASP.NET Core Localization (`.resx`-resursfiler)
- Svenska och engelska som startspråk
- RTL-stöd (Right-to-Left) för arabiska, farsi
- Språkväljare i topbar, val sparas i användarprofil
- Inga hårdkodade strängar i `.razor`-filer

---

## 2. UX-arkitektur — tre världar

### 2.1 Principen

Istället för en komplex app med 130 menyval ser varje roll **bara sin värld.** Samma backend, tre helt olika upplevelser:

| Roll | Upplevelse | Komplexitet |
|------|-----------|-------------|
| Anställd (Stina) | 6 stora kort, inga menyer | Minimal |
| Chef | Godkännandekö + 4 kort + Min sida | Låg |
| HR/Admin/Löne | Sidopanel med modullista | Medel |

### 2.2 Anställd-vyn — "Min sida"

```
┌──────────────────────────────────────────┐
│  OpenHR        🌐 SV  🔔  Anna ▼      │
├──────────────────────────────────────────┤
│                                          │
│  ┌─────────────────┐ ┌─────────────────┐ │
│  │                 │ │                 │ │
│  │  📅             │ │  🌴 23 dagar    │ │
│  │                 │ │  kvar           │ │
│  │  Mitt schema    │ │  Jag vill ha   │ │
│  │                 │ │  ledigt         │ │
│  │  Se när du      │ │                 │ │
│  │  jobbar         │ │  Ansök om       │ │
│  │                 │ │  semester       │ │
│  └─────────────────┘ └─────────────────┘ │
│                                          │
│  ┌─────────────────┐ ┌─────────────────┐ │
│  │                 │ │                 │ │
│  │  💰             │ │  😷            │ │
│  │                 │ │                 │ │
│  │  Min lön        │ │  Jag är sjuk   │ │
│  │                 │ │                 │ │
│  │  Se vad du      │ │  Meddela att   │ │
│  │  fått i lön     │ │  du inte kan   │ │
│  │                 │ │  jobba          │ │
│  └─────────────────┘ └─────────────────┘ │
│                                          │
│  ┌─────────────────┐ ┌─────────────────┐ │
│  │                 │ │                 │ │
│  │  📋 2 saker     │ │  👤            │ │
│  │  pågår          │ │                 │ │
│  │  Mina pågående  │ │  Om mig        │ │
│  │  saker          │ │                 │ │
│  │  Se status på   │ │  Min adress,   │ │
│  │  dina ärenden   │ │  telefon och   │ │
│  │                 │ │  uppgifter     │ │
│  └─────────────────┘ └─────────────────┘ │
│                                          │
└──────────────────────────────────────────┘
```

**6 kort. Ingen meny. Ingen sidebar.** Klicka på ett kort → en enkel sida. Tillbaka-pil → hem.

Varje kort:
- Stor ikon/emoji
- Rubrik i Stinas språk
- Kort hjälptext som förklarar vad som händer
- Visar relevant siffra direkt (23 dagar kvar, 2 pågående)

### 2.3 Chef-vyn

```
┌──────────────────────────────────────────┐
│  OpenHR        🌐 SV  🔔 3  Erik ▼    │
├──────────────────────────────────────────┤
│                                          │
│  ⚡ Saker som väntar på dig              │
│  ┌────────────────────────────────────┐  │
│  │ Anna vill ha semester 21-25 mars   │  │
│  │                    [Godkänn] [Nej] │  │
│  ├────────────────────────────────────┤  │
│  │ Karl har sjukanmält sig            │  │
│  │                           [Öppna]  │  │
│  ├────────────────────────────────────┤  │
│  │ Lisa har skickat in tidrapport     │  │
│  │                    [Godkänn] [Nej] │  │
│  └────────────────────────────────────┘  │
│                                          │
│  ┌─────────────────┐ ┌─────────────────┐ │
│  │ 👥 12 anställda │ │ 🟢 10 av 12    │ │
│  │                 │ │ jobbar idag     │ │
│  │ Mitt team       │ │ Bemanning       │ │
│  │ Se vilka som    │ │ Se vem som      │ │
│  │ jobbar hos dig  │ │ är här idag     │ │
│  └─────────────────┘ └─────────────────┘ │
│                                          │
│  ┌─────────────────┐ ┌─────────────────┐ │
│  │ 📅              │ │ 📊              │ │
│  │ Schema          │ │ Frånvaro        │ │
│  │ Se och ändra    │ │ Se vem som      │ │
│  │ schemat         │ │ är borta        │ │
│  └─────────────────┘ └─────────────────┘ │
│                                          │
│  ── Min sida ──                          │
│  (samma 6 kort som alla anställda)       │
│                                          │
└──────────────────────────────────────────┘
```

Godkännanden **direkt på startsidan** med en knapptryckning. Inga omvägar.

### 2.4 HR/Admin-vyn

```
┌────────────┬─────────────────────────────┐
│            │                             │
│  🏠 Start  │  [Globalt sökfält        🔍]│
│            │                             │
│  👥 Personal│  5 023     3.2%      14    │
│  💰 Lön    │  anställda sjukfr.   LAS!  │
│  📅 Schema │                             │
│  📋 Ärenden│  Senaste händelserna        │
│  🌴 Ledighet│ ─────────────────────────  │
│  🏥 Rehab  │  09:14 Anna sjukanmälde sig│
│  📄 Dokument│ 09:02 Lönekörning mars    │
│  🎓 Kompetens│       startad            │
│  📢 Rekryt.│  08:45 3 nya ansökningar   │
│  📊 Rapporter│       till vakans #42    │
│             │                            │
│  ── System ──│                           │
│  ⚙️ Inställn.│                           │
│  🔍 GDPR   │                            │
│  📝 Logg   │                            │
│             │                            │
│  ── Mitt ──  │                           │
│  🏠 Min sida│                            │
│             │                            │
└────────────┴─────────────────────────────┘
```

Platt meny. En rad = en modul. Inga undermenygrupper. Klicka → lista → klicka rad → detalj.

---

## 3. Intelligent assistans — "Systemet är experten"

### 3.1 Principen

När en uppgift kräver expertkunskap (lagkrav, kollektivavtal, beräkningar) ska systemet:

1. **Göra det automatiskt** om det finns ett entydigt rätt svar
2. **Föreslå lösningar** om det finns flera möjliga vägar
3. **Förklara tydligt** om något inte går, och varför, och vad som kan göras istället

Användaren ska **aldrig** behöva kunna ATL, LAS, Semesterlagen eller kollektivavtalet. Systemet kan det.

### 3.2 Exempel: Stina skapar schema för hela sjukhuset

**Vad Stina ser:**

```
Steg 1:
┌─────────────────────────────────────┐
│ ← Tillbaka                          │
│                                     │
│  Skapa nytt schema                  │
│                                     │
│  Vilken period?                     │
│                                     │
│  ┌─────────────┐                    │
│  │ April 2026  │  ← nästa månad     │
│  └─────────────┘    (vanligaste)    │
│  ┌─────────────┐                    │
│  │ Annan period│                    │
│  └─────────────┘                    │
│                                     │
└─────────────────────────────────────┘

Steg 2:
┌─────────────────────────────────────┐
│ ← Tillbaka                          │
│                                     │
│  Vilken enhet?                      │
│                                     │
│  ┌─────────────────────────────┐    │
│  │ Medicinkliniken (alla)      │    │
│  └─────────────────────────────┘    │
│  ┌─────────────────────────────┐    │
│  │ Avdelning 32                │    │
│  └─────────────────────────────┘    │
│  ┌─────────────────────────────┐    │
│  │ Avdelning 33                │    │
│  └─────────────────────────────┘    │
│  ┌─────────────────────────────┐    │
│  │ Akutmottagningen            │    │
│  └─────────────────────────────┘    │
│                                     │
└─────────────────────────────────────┘

Steg 3: (Systemet arbetar...)
┌─────────────────────────────────────┐
│                                     │
│  ⏳ Skapar schema...                │
│                                     │
│  Vi kollar:                         │
│  ✅ Vem som kan jobba               │
│  ✅ Att viloregler följs            │
│  ✅ Att det finns tillräckligt      │
│     med personal på varje pass      │
│  ⏳ Optimerar...                    │
│                                     │
└─────────────────────────────────────┘

Steg 4: Resultat
┌─────────────────────────────────────────┐
│ ← Tillbaka                              │
│                                         │
│  Förslag till schema — April 2026       │
│  Medicinkliniken                        │
│                                         │
│  ✅ 847 av 850 pass tillsatta           │
│  ✅ Alla lagkrav uppfyllda              │
│                                         │
│  ⚠️ 3 saker behöver din hjälp:          │
│                                         │
│  1. Avd 32 — nattpass 15-17 april       │
│     Det saknas 1 sjuksköterska.         │
│     ┌────────────────────────────────┐  │
│     │ Förslag: Fråga Anna Svensson  │  │
│     │ eller Karl Berg — de har inga  │  │
│     │ pass de nätterna              │  │
│     │          [Fråga Anna] [Fråga Karl]│
│     └────────────────────────────────┘  │
│                                         │
│  2. Erik Lindberg — vecka 16            │
│     Han har redan jobbat 47 av max      │
│     48 timmar den veckan.               │
│     ┌────────────────────────────────┐  │
│     │ Förslag: Flytta hans           │  │
│     │ lördagspass till vecka 17      │  │
│     │          [Flytta] [Behåll ändå]│  │
│     └────────────────────────────────┘  │
│                                         │
│  3. Semester vecka 28-32                │
│     8 personer vill vara lediga         │
│     samtidigt, men max 4 kan vara det.  │
│     ┌────────────────────────────────┐  │
│     │ Förslag: Prioritera efter      │  │
│     │ turordning (längst anställd    │  │
│     │ först). Se fördelning →        │  │
│     │            [Godkänn turordning]│  │
│     └────────────────────────────────┘  │
│                                         │
│  ┌─────────────────────────────────────┐│
│  │                                     ││
│  │  Publicera schema                   ││
│  │  Alla anställda meddelas            ││
│  │                                     ││
│  └─────────────────────────────────────┘│
│                                         │
└─────────────────────────────────────────┘
```

**Vad systemet gör bakom kulissen:**

1. Hämtar alla anställda med aktiva anställningar på vald enhet
2. Hämtar befintligt grundschema som bas
3. Hämtar godkända ledighetsansökningar för perioden
4. Hämtar sjukskrivningar, föräldraledigheter, tjänstledigheter
5. Tillämpar ATL-regler:
   - 11 timmar dygnsvila
   - 36 timmar sammanhängande veckovila
   - Max 48 timmar/vecka (snitt över 4 veckor)
   - Max 200 timmar övertid/år
6. Tillämpar kollektivavtalsregler (AB):
   - OB-schabloner (kväll/natt/helg/storhelg)
   - Jourpass max 24h
   - Beredskap kompensationsregler
7. Tillämpar minimibemanning per pass (konfigurerat per enhet)
8. Kontrollerar kompetenskrav (legitimationer, specialistkompetens)
9. Beaktar LAS-ackumulering (undviker att ge vikarier för många dagar)
10. Optimerar med AI-solver för bästa möjliga fördelning
11. Identifierar olösbara konflikter och genererar lösningsförslag

### 3.3 Exempel: Stina gör lönekörning

```
Steg 1: Klicka "Kör lön"
Steg 2: Välj period → "Mars 2026" (stor knapp)

Steg 3: Systemet arbetar...
┌─────────────────────────────────────┐
│  ⏳ Beräknar löner...               │
│                                     │
│  ✅ 5 023 anställda                 │
│  ✅ Skattetabeller 2026             │
│  ✅ OB-tillägg beräknade            │
│  ✅ Sjuklön beräknad                │
│  ⏳ Kontrollerar...                 │
│                                     │
└─────────────────────────────────────┘

Steg 4: Resultat
┌───────────────────────────────────────┐
│                                       │
│  Lönekörning Mars 2026 — Klar!       │
│                                       │
│  Total brutto:    158 432 100 kr      │
│  Total skatt:      47 529 630 kr      │
│  Total netto:     110 902 470 kr      │
│                                       │
│  ✅ 5 019 löner ser bra ut            │
│                                       │
│  ⚠️ 4 löner behöver din kontroll:     │
│                                       │
│  Anna Svensson — 32 400 kr            │
│  Normalt: 28 500 kr (+14%)            │
│  ┌──────────────────────────────────┐ │
│  │ Förklaring: 3 nattpass extra +   │ │
│  │ OB storhelg påsk = +3 900 kr     │ │
│  │           [OK, stämmer] [Ändra]  │ │
│  └──────────────────────────────────┘ │
│                                       │
│  (... 3 till ...)                     │
│                                       │
│  ┌───────────────────────────────┐    │
│  │ Godkänn och skicka till       │    │
│  │ utbetalning                   │    │
│  └───────────────────────────────┘    │
│                                       │
│  💡 Nästa steg: AGI-filen till        │
│  Skatteverket skapas automatiskt      │
│  den 12:e.                            │
│                                       │
└───────────────────────────────────────┘
```

Systemet flaggar **bara avvikelser** — Stina behöver inte granska 5 023 löner, bara de 4 som ser ovanliga ut.

### 3.4 Mönstret: Intelligent assistans

Samma mönster upprepas i varje modul:

| Modul | Stina gör | Systemet gör |
|-------|-----------|-------------|
| **Schema** | Väljer period + enhet | Optimerar 850 pass, kontrollerar ATL, AB, bemanning, kompetens, LAS |
| **Lön** | Väljer period, godkänner | Beräknar 5 023 löner, flaggar avvikelser, genererar AGI/betalfiler |
| **Ledighet** | Väljer dagar | Kontrollerar saldo, schemakonflikter, bemanningspåverkan, föreslår alternativ |
| **Rekrytering** | Beskriver tjänsten | Kontrollerar företrädesrätt, genererar annons, publicerar Platsbanken |
| **LAS** | Ser dashboard | Automatisk ackumulering, alarmer vid trösklar, konverteringsförslag |
| **HälsoSAM** | Ser signaler | Auto-trigger vid sjukfrånvaromönster, uppföljningspåminnelser |
| **GDPR** | Godkänner registerutdrag | Samlar data från alla moduler, genererar rapport, spårar deadline |
| **Rapporter** | Väljer rapport | Hämtar data, beräknar KPI:er, genererar PDF/Excel |

### 3.5 När något inte går — tydliga lösningsförslag

Systemet ska **aldrig** bara säga "Det går inte." Det ska **alltid** säga varför och föreslå vad som kan göras:

```
┌─────────────────────────────────────────┐
│                                         │
│  ⚠️ Det gick inte att schemalägga       │
│  Erik på nattpasset 15 april            │
│                                         │
│  Erik har redan jobbat 11 timmar den    │
│  dagen. Enligt arbetstidslagen måste    │
│  han ha minst 11 timmars vila mellan    │
│  pass.                                  │
│                                         │
│  Det här kan du göra istället:          │
│                                         │
│  ┌───────────────────────────────────┐  │
│  │ 1. Ge passet till Anna — hon är  │  │
│  │    ledig den natten       [Välj] │  │
│  └───────────────────────────────────┘  │
│  ┌───────────────────────────────────┐  │
│  │ 2. Flytta Eriks dagpass till en  │  │
│  │    annan dag              [Välj] │  │
│  └───────────────────────────────────┘  │
│  ┌───────────────────────────────────┐  │
│  │ 3. Behöver ni verkligen nattpass │  │
│  │    den natten? Ta bort    [Välj] │  │
│  └───────────────────────────────────┘  │
│                                         │
└─────────────────────────────────────────┘
```

Aldrig bara ett felmeddelande. Alltid **förklaring + numrerade alternativ + knappar.**

---

## 4. UI-sidor — komplett lista per modul

### 4.1 Core HR (utökas)

| Sida | Route | Beskrivning |
|------|-------|-------------|
| Anställdalista | `/anstallda` | MudDataGrid med sökning, filtrering, paginering |
| Anställd detalj | `/anstallda/{id}` | Flikar: Uppgifter, Anställningar, Lön, Frånvaro, LAS, Kompetens, Dokument |
| Ny anställd | `/anstallda/ny` | Steg-för-steg: Person → Anställning → Skatt → Bank |
| Redigera anställd | `/anstallda/{id}/redigera` | Redigeringsformulär med validering |
| Organisation | `/organisation` | Trädvy med organisationsenheter |
| Organisationsenhet | `/organisation/{id}` | Enhetsinformation, chef, anställda |

### 4.2 Payroll (helt nytt UI)

| Sida | Route |
|------|-------|
| Lönekörningar | `/lon/korningar` |
| Lönekörning detalj + granskning | `/lon/korning/{id}` |
| Ny lönekörning | `/lon/korning/ny` |
| Lönespecifikationer (admin) | `/lon/specifikationer` |
| Lönearter | `/lon/lonearter` |
| Skattetabeller | `/lon/skattetabeller` |
| AGI-export | `/lon/agi` |
| Betalningsfiler | `/lon/betalningar` |
| Lönestatistik | `/lon/statistik` |

### 4.3 Scheduling (utökas)

| Sida | Route |
|------|-------|
| Schemaöversikt (vecko/månadsvy) | `/schema` |
| Skapa schema (intelligent assistent) | `/schema/ny` |
| Bemanningsöversikt (realtid) | `/schema/bemanning` |
| Instämpling | `/stampling` |
| Tidrapporter | `/tidrapporter` |
| Tidrapport detalj | `/tidrapporter/{id}` |
| Passbyte | `/schema/passbyte` |
| ATL-efterlevnad | `/schema/atl` |

### 4.4 Case Management (utökas)

| Sida | Route |
|------|-------|
| Ärendelista | `/arenden` |
| Nytt ärende | `/arenden/nytt` |
| Ärendedetalj med tidslinje | `/arenden/{id}` |
| Workflow-administration | `/arenden/workflows` |

### 4.5 LAS (utökas)

| Sida | Route |
|------|-------|
| LAS dashboard | `/las` |
| LAS detalj per anställd | `/las/{anstallId}` |
| Turordningslista | `/las/turordning` |
| Företrädesrätt | `/las/foretradesratt` |

### 4.6 HälsoSAM (utökas)

| Sida | Route |
|------|-------|
| Rehabärenden | `/halsosam` |
| Rehabärende detalj | `/halsosam/{id}` |
| Nytt rehabärende | `/halsosam/ny` |
| Sjukfrånvarostatistik | `/halsosam/statistik` |

### 4.7 Salary Review

| Sida | Route |
|------|-------|
| Löneöversynsrundor | `/loneoversyn` |
| Runda detalj + förslag | `/loneoversyn/{id}` |
| Ny runda | `/loneoversyn/ny` |

### 4.8 Travel

| Sida | Route |
|------|-------|
| Resekrav | `/resor` |
| Nytt resekrav | `/resor/ny` |
| Resekrav detalj | `/resor/{id}` |

### 4.9 Recruitment

| Sida | Route |
|------|-------|
| Vakanser | `/rekrytering/vakanser` |
| Vakans detalj | `/rekrytering/vakans/{id}` |
| Ny vakans | `/rekrytering/vakans/ny` |
| Ansökningar per vakans | `/rekrytering/vakans/{id}/ansokningar` |
| Intervjuplanering | `/rekrytering/intervjuer` |
| Talentpool | `/rekrytering/talentpool` |
| Onboarding | `/rekrytering/onboarding` |
| Onboarding detalj | `/rekrytering/onboarding/{id}` |

### 4.10 Integration Hub

| Sida | Route |
|------|-------|
| Integrationsdashboard | `/integrationer` |
| Adapter status | `/integrationer/{adapter}` |
| Outbox-kö | `/integrationer/outbox` |
| Integrationslogg | `/integrationer/logg` |

### 4.11 SelfService (totalrenovering)

| Sida | Route | Stina-etikett |
|------|-------|---------------|
| Min startsida (6 kort) | `/minsida` | — |
| Mitt schema | `/minsida/schema` | "Se när du jobbar" |
| Min ledighet | `/minsida/ledighet` | "Jag vill ha ledigt" |
| Min lön | `/minsida/lon` | "Se vad du fått i lön" |
| Sjukanmälan | `/minsida/sjukanmalan` | "Jag är sjuk" |
| Mina pågående saker | `/minsida/arenden` | "Se status" |
| Om mig | `/minsida/profil` | "Min adress och telefon" |
| Mina utbildningar | `/minsida/utbildningar` | "Mina kurser" |
| Mina dokument | `/minsida/dokument` | "Mina papper" |
| Nödkontakter | `/minsida/nodkontakter` | "Ring om något händer mig" |

### 4.12 Chefsportal

| Sida | Route |
|------|-------|
| Chefsdashboard | `/chef` |
| Mitt team | `/chef/team` |
| Attestkö (godkännanden) | `/chef/attest` |
| Bemanningsöversikt | `/chef/bemanning` |
| Frånvarokalender | `/chef/franvarokalender` |
| Teamets LAS-status | `/chef/las` |

### 4.13 Audit

| Sida | Route |
|------|-------|
| Granskningslogg | `/audit` |
| Ändringsdetalj | `/audit/{id}` |

### 4.14 Notifications

| Sida | Route |
|------|-------|
| Notiscenter | `/notiser` |
| Notisinställningar | `/notiser/installningar` |

### 4.15 Leave

| Sida | Route |
|------|-------|
| Ledighetsöversikt | `/ledighet` |
| Ny ledighetsansökan (konversation) | `/ledighet/ny` |
| Ledighet detalj | `/ledighet/{id}` |
| Semestersaldo | `/ledighet/saldon` |
| Teamkalender | `/ledighet/kalender` |
| Sjukanmälningar | `/ledighet/sjukanmalningar` |

### 4.16 Documents

| Sida | Route |
|------|-------|
| Dokumentöversikt | `/dokument` |
| Ladda upp | `/dokument/ny` |
| Dokument detalj | `/dokument/{id}` |
| Dokumentmallar | `/dokument/mallar` |
| E-signering | `/dokument/{id}/signera` |

### 4.17 Performance

| Sida | Route |
|------|-------|
| Medarbetarsamtal | `/medarbetarsamtal` |
| Nytt samtal | `/medarbetarsamtal/ny` |
| Samtal detalj | `/medarbetarsamtal/{id}` |
| Självbedömning | `/medarbetarsamtal/{id}/sjalvbedomning` |
| Chefsbedömning | `/medarbetarsamtal/{id}/chefsbedomning` |
| Målsättningar | `/medarbetarsamtal/{id}/mal` |

### 4.18 Reporting

| Sida | Route |
|------|-------|
| Rapportbibliotek | `/rapporter` |
| Kör rapport | `/rapporter/{id}/kor` |
| Rapportresultat | `/rapporter/{id}/resultat` |
| Schemalagda rapporter | `/rapporter/schemalagda` |
| Ad hoc-rapportbyggare | `/rapporter/adhoc` |

### 4.19 GDPR

| Sida | Route |
|------|-------|
| GDPR dashboard | `/gdpr` |
| Registerutdrag | `/gdpr/registerutdrag` |
| Begäran detalj | `/gdpr/{id}` |
| Retention & anonymisering | `/gdpr/retention` |

### 4.20 Competence

| Sida | Route |
|------|-------|
| Kompetensregister | `/kompetens` |
| Ny certifiering | `/kompetens/ny` |
| Utgående certifieringar | `/kompetens/utgaende` |
| Obligatoriska utbildningar | `/kompetens/obligatoriska` |

### 4.21 Positions

| Sida | Route |
|------|-------|
| Positionsöversikt | `/positioner` |
| Position detalj | `/positioner/{id}` |
| Headcount-planering | `/positioner/headcount` |

### 4.22 Offboarding

| Sida | Route |
|------|-------|
| Offboarding-ärenden | `/offboarding` |
| Nytt ärende | `/offboarding/ny` |
| Offboarding detalj | `/offboarding/{id}` |
| Exit-samtal | `/offboarding/{id}/exitsamtal` |

### 4.23 Benefits

| Sida | Route |
|------|-------|
| Förmånsöversikt | `/formaner` |
| Förmånsanmälan | `/formaner/anmalan` |
| Friskvårdsbidrag | `/formaner/friskvard` |

### 4.24 LMS

| Sida | Route |
|------|-------|
| Kurskatalog | `/utbildning` |
| Kurs detalj | `/utbildning/{id}` |
| Ny kurs | `/utbildning/ny` |
| Lärstigar | `/utbildning/larstigar` |

### 4.25 Configuration

| Sida | Route |
|------|-------|
| Systemkonfiguration | `/admin/konfiguration` |
| Custom fields | `/admin/custom-fields` |
| Workflow-definitioner | `/admin/workflows` |
| Rollhantering | `/admin/roller` |
| Lokaliseringshantering | `/admin/sprak` |

**Totalt: ~130 sidor**

---

## 5. Konversationsflöden (Stina-mönstret)

Alla användarhandlingar som kräver input följer samma mönster:

### 5.1 Mönstret

```
Steg 1: En tydlig fråga med stora knappar (vanligaste valet störst)
   ↓
Steg 2: Nästa fråga (bara om det behövs)
   ↓
Steg 3: Sammanfattning — "Stämmer det här?"
   ↓
Steg 4: Bekräftelse — "Klart! Här är vad som händer nu."
```

### 5.2 Flöden som ska byggas som konversation

| Flöde | Steg |
|-------|------|
| Sjukanmälan | Vilken dag? → Vet du när du är tillbaka? → Klart! |
| Ledighetsansökan | Vilken typ? → Vilka dagar? (kalender) → Visa saldo → Klart! |
| Nytt resekrav | Vart? → När? → Utlägg (foto) → Skicka in |
| Ny anställd | Personnummer → Namn → Anställningsform → Enhet → Lön → Klart! |
| Nytt schema | Period? → Enhet? → Systemet genererar → Granska → Publicera |
| Lönekörning | Period? → Systemet beräknar → Granska avvikelser → Godkänn |
| Medarbetarsamtal | Välj anställd → Fyll i (steg för steg) → Signera |
| GDPR-registerutdrag | Vilken anställd? → Systemet samlar data → Granska → Skicka |

### 5.3 Aldrig formulär med mer än 3 fält synliga samtidigt

Om ett formulär behöver 10 fält, delas det i steg med 2-3 fält per steg. Progressindikator visar var Stina är:

```
● Steg 1    ○ Steg 2    ○ Steg 3    ○ Klart
  Person      Jobb        Lön
```

---

## 6. Funktionsfördjupning — kritiska luckor

### 6.1 De 29 kritiska luckorna

Varje lucka löses med verklig data, fullständig affärslogik, och Stina-anpassat UI:

| # | Lucka | Lösning |
|---|-------|---------|
| 1 | Redigera anställd i UI | Steg-för-steg-redigering med validering, audit trail |
| 2 | Komplett anställd-detaljvy | Flikar med all data, progressiv avslöjning |
| 3 | Offboarding/avslut-workflow | Konversationsflöde: Slutlön → Semester → Tillgångar → Åtkomst → Intyg |
| 4 | Självservice med riktig data | Alla 6 kort kopplade till API-endpoints via SelfServiceService |
| 5 | Uppdatera egna kontaktuppgifter | Enkelt formulär i "Om mig" med bekräftelse |
| 6 | Sjukanmälan via självservice | Konversationsflöde: 2 frågor → auto-ärende, chef-notis, karensavdrag |
| 7 | Chefsportal med riktig data | ChefPortalService aggregerar team, attestkö, bemanning |
| 8 | Godkänn/neka workflow | En-knapps-godkännande på startsidan + batch-godkännande |
| 9 | Rollbaserade vyer | Tre separata layouter: Anställd/Chef/Admin |
| 10 | Semestersaldomotor | Beräkningsmotor: åldersbaserat (25/31/32 dagar per AB), intjänat/uttaget/sparat |
| 11 | Ledighetsdashboard | Saldo synligt direkt på "Jag vill ha ledigt"-kortet |
| 12 | Medarbetarsamtal | Konversationsflöde: självbedömning → chefsbedömning → mål → signering |
| 13 | Obligatorisk utbildning | Certifieringsregister med auto-påminnelser |
| 14 | Lönegranskningsdashboard | Summor + avvikelsemarkering ("4 löner behöver kontroll") |
| 15 | Lönespecifikation PDF | PdfSharpCore, tillgänglig i "Min lön", e-post vid ny spec |
| 16 | Tidrapportering | Månadstidrapport: schema auto-populerat, avvikelser markeras |
| 17 | Executive HR-dashboard | Realtids-KPI:er med trendpilar och trafikljus |
| 18 | Standardrapporter | Förbyggt bibliotek med en-klicks-körning |
| 19 | Granskningslogg | Alla ändringar loggas, sökbar logg i admin-vyn |
| 20 | GDPR registerutdrag | Konversationsflöde: välj anställd → system samlar → granska → leverera |
| 21 | Dokumentlagring | Filuppladdning per anställd via befintlig IFileStorageService (S3-kompatibel) |
| 22 | Automatiserade workflow-triggers | Domänhändelser → bakgrundsjobb → notiser/åtgärder |
| 23 | E-postnotiser | MailKit, konfigurerbara per händelsetyp |
| 24 | In-app-notiser | SignalR realtid + notiscenter i topbar |
| 25 | Automatiska påminnelser | NotificationReminderService utökad med alla triggers |
| 26 | Enhetsbaserad åtkomstkontroll | RLS i PostgreSQL + middleware i API |
| 27 | Sökfunktion | Globalt sökfält: MudAutocomplete med pg_trgm fuzzy search |
| 28 | WCAG 2.1 AA | axe-core-audit + åtgärder: kontrast, skärmläsare, tangentbord, fokus |
| 29 | Dataexport | Exportknapp på alla listor → Excel/CSV via ClosedXML |

### 6.2 De 31 viktiga luckorna (andra omgången)

Inkluderar: anställningsändringar, provanställning, kontraktgenerering, tjänstgöringsintyg, lönehistorik, företrädesrätt-integration, passbyten, resekrav-UI, ledighetsworkflow, frånvarokalender, föräldraledighet, VAB, ATL-dashboard, jour/beredskap, lönekorrigeringar, avdragshantering, AGI-spårning, betalfilshantering, pensionsrapport, förmånsanmälan, friskvårdsbidrag, Platsbanken-integration, intervjuplanering, erbjudandebrev, onboarding-portal, MBL-workflow, SCB-rapportering, SKR-rapportering, PowerBI-integration, dokumentmallar, e-signering, multi-level approval, delegation, batch-approval, SMS-notiser, mobilresponsivt, bulk-import, integrationsövervakning, hälsodata-separation, 2FA.

### 6.3 De 18 nice-to-have (tredje omgången)

Inkluderar: 360-feedback, succession planning, e-learning-integration, workforce analytics, kostnads-simulation, intern meddelandehantering, anslagstavla, dark mode, utskriftsvänliga vyer, kontextuell hjälp/tooltips, referenskontroll, rekryteringsanalys, försäkringsöversikt, organisationsdokument, policy-distribution, API rate limiting, prestandatest/security audit.

---

## 7. Databasändringar

### 7.1 Nya migrationer

13 nya SQL-migrationer för Wave 1-3-moduler som saknar databastabeller:

| Migration | Schema | Tabeller |
|-----------|--------|----------|
| `000011_leave.sql` | `leave` | vacation_balances, requests, sick_notifications |
| `000012_documents.sql` | `documents` | documents, templates, signatures, versions |
| `000013_performance.sql` | `performance` | reviews, goals, assessments |
| `000014_reporting.sql` | `reporting` | definitions, executions, schedules |
| `000015_gdpr.sql` | `gdpr` | data_subject_requests, retention_records |
| `000016_competence.sql` | `competence` | certifications, mandatory_trainings |
| `000017_positions.sql` | `positions` | positions, headcount_plans |
| `000018_offboarding.sql` | `offboarding` | cases, exit_interviews |
| `000019_benefits.sql` | `benefits` | benefits, enrollments |
| `000020_lms.sql` | `lms` | courses, enrollments, learning_paths |
| `000021_configuration.sql` | `config` | tenants, custom_fields, workflows |
| `000022_analytics.sql` | `analytics` | saved_reports, dashboards |
| `000023_notifications.sql` | `notifications` | notifications, templates, preferences |

Varje migration inkluderar: tabeller, index, RLS-policyer, audit triggers.

### 7.2 Lokaliseringstabeller

Ny tabell `config.localization_overrides` för admin-redigerbara textsträngar. Statiska UI-texter hanteras via `.resx`-kompilerade resursfiler.

---

## 8. Service-lager

### 8.1 Nya services (Blazor → API)

| Service | Ansvar |
|---------|--------|
| `SelfServiceService` | Aggregerar data för anställd-vyn (6 kort) |
| `ChefPortalService` | Aggregerar data för chef-vyn (attestkö, team, bemanning) |
| `PayrollService` | Lönekörningar, granskning, PDF |
| `SchedulingService` | Schema, instämpling, tidrapporter, AI-optimering |
| `LeaveService` | Ledighet, saldon, sjukanmälan |
| `LASService` | Ackumuleringar, alarmeringar |
| `HalsoSAMService` | Rehabärenden, sjukfrånvarostatistik |
| `DocumentService` | Uppladdning, nedladdning, signering |
| `PerformanceService` | Medarbetarsamtal |
| `CompetenceService` | Certifieringar, obligatoriska utbildningar |
| `RecruitmentService` | Vakanser, ansökningar, onboarding |
| `ReportingService` | Rapportbibliotek, körning |
| `NotificationService` | Hämta/skicka notiser |
| `GDPRService` | DSR, registerutdrag, anonymisering |
| `SearchService` | Global sökning |
| `ExportService` | CSV/Excel-export |
| `IntelligentAssistantService` | Lösningsförslag vid regelkonflikter |

### 8.2 IntelligentAssistantService

Ny central service som anropas när systemet upptäcker problem:

```csharp
public interface IIntelligentAssistantService
{
    Task<List<Suggestion>> GetScheduleSuggestions(ScheduleConflict conflict);
    Task<List<Suggestion>> GetPayrollAnomalySuggestions(PayrollAnomaly anomaly);
    Task<List<Suggestion>> GetLeaveDenialAlternatives(LeaveConflict conflict);
    Task<List<Suggestion>> GetATLViolationResolutions(ATLViolation violation);
    Task<List<Suggestion>> GetLASActionSuggestions(LASAlert alert);
}
```

Varje `Suggestion` innehåller:
- Rubrik (Stinas språk)
- Förklaring (varför)
- Åtgärdsknapp (vad Stina kan klicka)
- Automatisk åtgärd (om Stina godkänner)

---

## 9. Realtid & notiser

### 9.1 SignalR Hub

Ny `NotificationHub` för push-notiser till klienten:
- Ny notis → uppdatera klockan i topbar
- Schemaändring → uppdatera bemanningsöversikt
- Godkännande klart → uppdatera chefsvy

### 9.2 Notiseringskanaler

| Kanal | Användning | Teknik |
|-------|-----------|--------|
| In-app | Allt | SignalR |
| E-post | Godkännanden, påminnelser, lönespec | MailKit |
| Push (PWA) | Schemaändringar, brådskande godkännanden | Web Push API |

### 9.3 Automatiska triggers

| Trigger | Åtgärd |
|---------|--------|
| Sjukdag 7 | Påminnelse: läkarintyg behövs |
| Sjukdag 14 | Auto-notifiering till FK |
| LAS > 300 dagar | Alarm till HR + chef |
| Certifiering utgår 90 dagar | Påminnelse till anställd + chef |
| Ny godkännandebegäran | Notis till godkännare |
| Lönekörning klar | Lönespec tillgänglig i "Min lön" |
| Schemaändring | Notis till berörda anställda |
| GDPR-begäran 7 dagar kvar | Alarm till handläggare |

---

## 10. PWA & mobil

### 10.1 PWA-konfiguration
- Service worker för offline-cachning (schema, profil, senaste lönespec)
- Web App Manifest (installering på hemskärmen)
- Push notification-stöd

### 10.2 Mobilanpassning
- Responsiv layout: 6 kort → 2 per rad (tablet) → 1 per rad (mobil)
- Stor instämplingsknapp (hela skärmens bredd)
- Swipe för godkännande (chef)
- Stora touch targets (56px minimum)

---

## 11. Teststrategi

### 11.1 Målbild: ~1500+ tester

| Testtyp | Nuvarande | Mål | Verktyg |
|---------|-----------|-----|---------|
| Enhetstest (domänlogik) | 475 | 800+ | xUnit |
| Integrationstest (service → DB) | 0 | 200+ | xUnit + WebApplicationFactory |
| Komponenttest (Blazor) | 0 | 300+ | bUnit (MIT) |
| E2E-test (Stina-flöden) | 0 | 100+ | Playwright (Apache 2.0) |
| Tillgänglighetstest | 0 | 50+ | axe-core (MPL 2.0) |
| Prestandatest | 0 | 20+ | k6 (AGPL) |

### 11.2 Stina-tester (E2E)

Varje konversationsflöde testas som Stina skulle använda det:
- Sjukanmälan: öppna → klicka "Jag är sjuk" → klicka "Idag" → klicka "Nej vet inte" → verifiera bekräftelse
- Ledighetsansökan: öppna → klicka "Jag vill ha ledigt" → välj dagar → skicka → verifiera
- Se lönespec: öppna → klicka "Min lön" → verifiera PDF-länk

---

## 12. Säkerhet

### 12.1 Behörighetsstyrning

| Lager | Mekanism |
|-------|----------|
| Frontend | Rollbaserad layout (3 vyer) |
| API | `[Authorize("RollNamn")]` på alla endpoints |
| Data | RLS i PostgreSQL (enhet-baserad scoping) |
| Känslig data | pgcrypto (personnummer, bankuppgifter) |
| Hälsodata | Striktare RLS (HälsoSAM: bara ärendeägare + HR) |
| Audit | Alla ändringar loggas med användare, tidpunkt, gamla/nya värden |

### 12.2 WCAG 2.1 AA

Lagkrav per DOS-lagen. Implementeras via:
- axe-core-validering i CI/CD
- Stor text (18px bas), högt kontrast-tema
- Tangentbordsnavigering på alla element
- Skärmläsarstöd (aria-attribut)
- Skip-to-content-länk
- Fokushantering vid navigering

---

## 13. Driftsättning

### 13.1 Docker Compose (utveckling)

```yaml
services:
  postgres:
    image: postgres:17-alpine
  rabbitmq:
    image: rabbitmq:4-management-alpine
```

### 13.2 Produktion

- Kubernetes eller Docker Swarm
- Supabase self-hosted (databas + auth)
- RabbitMQ för meddelandekö
- Reverse proxy (Caddy, Apache 2.0)
- TLS-terminering

### 13.3 Driftsäkerhet och katastrofhantering

| Mått | Mål |
|------|-----|
| **RTO** (Recovery Time Objective) | Max 4 timmar |
| **RPO** (Recovery Point Objective) | Max 15 minuter (WAL-arkivering) |

**Backup-strategi:**
- PostgreSQL WAL-arkivering kontinuerligt (15-minutersintervall)
- Daglig fullständig backup (pg_dump) kl 02:00
- Veckovis backuptest: automatisk restore till testmiljö + verifiering
- Backup lagras krypterat på separat lagringsplats

**Failover:**
- PostgreSQL streaming replication (primär + standby)
- Automatisk failover vid primärfel
- Applikationsinstanser bakom load balancer (minst 2)

**Övervakningn:**
- Liveness/readiness-probes på alla tjänster
- Alerting vid: disk >80%, CPU >90%, replikeringslag >30s, backup-misslyckande

---

## 14. Prestandamål

| Operation | Mål |
|-----------|-----|
| Sidladdning (alla sidor) | < 500ms |
| API-svar (CRUD) | < 200ms |
| Global sökning | < 300ms |
| Lönekörning (5 000 anställda) | < 5 minuter |
| Schemaoptimering (850 pass) | < 2 minuter |
| PDF-generering (lönespec) | < 2 sekunder |
| Rapporthämtning | < 10 sekunder |
| Samtidiga användare | 500+ |
| Topplast (skiftbyte 07:00) | 200 samtidiga anrop/sekund |

---

## 15. Schemaoptimering — Google OR-Tools

Schemaläggning för 24/7 sjukvård är ett NP-svårt optimeringsproblem. Lösning:

**Verktyg:** Google OR-Tools (Apache 2.0, 100% FOSS)

**Approach:** Constraint Programming (CP-SAT solver)

**Constraints som modelleras:**
- ATL: 11h dygnsvila, 36h veckovila, max 48h/vecka
- Kollektivavtal (AB): OB-schabloner, jourpass max 24h
- Minimibemanning per pass per enhet (konfigurerat)
- Kompetenskrav per pass (legitimation, specialisering)
- Anställdas sysselsättningsgrad
- Godkända ledigheter och sjukskrivningar
- LAS-hänsyn (undvik att ge vikarier för många dagar)

**Optimeringsmål (viktad):**
1. Uppfyll minimibemanning (högsta prioritet)
2. Minimera lagöverträdelser (0 tolerans)
3. Rättvis fördelning av obekväma pass
4. Minimera övertidskostnader
5. Respektera personalönskemål (lägsta prioritet)

**Integration:**
- Anropas via `IScheduleOptimizer` interface
- Körs asynkront (bakgrundsjobb)
- Returnerar `ScheduleProposal` med pass + konflikter + förslag

---

## 16. MudBlazor-migrationsstrategi

### Fas 1: Installation och tema
- Installera MudBlazor NuGet-paket
- Skapa OpenHR MudTheme som matchar Stina-principen:
  - Primärfärg: `#1a5276`, 18px bastext, 56px knappöjd
  - Högt kontrast-tema (WCAG AAA-nivå)
- Lägg till MudDialogProvider, MudSnackbarProvider i App.razor

### Fas 2: Befintliga sidor
- Skriv om 17 befintliga sidor att använda MudBlazor-komponenter
- Byt RhrDataTable → MudDataGrid (med sortering, filtrering, paginering)
- Byt RhrButton → MudButton (stylade via tema)
- Byt RhrInput → MudTextField (med inbyggd validering)

### Fas 3: DesignSystem-projektet
- Behåll som wrapper-lager för OpenHR-specifika komponenter:
  - `RhrConversationFlow` — steg-för-steg-konversationsvy
  - `RhrBigCard` — de stora korten på startsidan
  - `RhrSuggestionCard` — intelligenta lösningsförslag
  - `RhrTrafficLight` — bemanningsstatus
- Dessa använder MudBlazor-komponenter internt men exponerar Stina-anpassade API:er

### WCAG-notering
MudBlazor DataGrid har kända tillgänglighetsbrister. Åtgärder:
- Lägg till aria-labels via MudBlazor's Accessibility-attribut
- Testa med axe-core i CI/CD
- Fallback till HTML-tabell om DataGrid inte uppfyller WCAG AA

---

## 17. HEROMA-migreringsstrategi

Detaljerad migreringsplan finns i `docs/heroma-migrering-analys.md`. Här sammanfattas nyckelpunkterna:

### Cutover-strategi: Fasad övergång

| Fas | Tidsram | Omfattning |
|-----|---------|------------|
| 1. Parallellkörning | 3 månader | OpenHR körs parallellt med HEROMA, data synkas dagligen |
| 2. Pilotgrupp | 2 månader | En enhet (50-100 anställda) går över helt till OpenHR |
| 3. Stegvis utrullning | 4 månader | Förvaltning för förvaltning |
| 4. HEROMA avstängning | 1 månad | Slutgiltig datamigrering, HEROMA avvecklas |

### Datamigrering
- ETL-pipeline byggt som engångsjobb
- Validering: varje migrerad post jämförs med HEROMA-original
- Lönedata: minst 24 månaders historik migreras
- Semestersaldo: verifieras per anställd

### Rollback-plan
- HEROMA hålls aktivt under hela parallellkörningen
- Vid kritiskt fel: återgå till HEROMA inom 24h
- Data som skapats i OpenHR exporteras och matas in manuellt

### Framgångskriterier innan HEROMA stängs
- 100% av lönekörningar producerar identiska resultat i båda system (3 månader i rad)
- 0 kritiska buggar i 30 dagar
- 95% av användare har loggat in och genomfört minst en handling
- Alla obligatoriska integrationer (Skatteverket, FK, Nordea) verifierade

---

## 18. Konfigurationshantering

### Miljöer
| Miljö | Syfte | Databas |
|-------|-------|---------|
| `Development` | Lokal utveckling | PostgreSQL via docker-compose |
| `Staging` | Testning, UAT | Separat PostgreSQL-instans |
| `Production` | Drift | PostgreSQL med streaming replication |

### Hemligheter (secrets)
- **Utveckling:** `appsettings.Development.json` (gitignored) eller environment variables
- **Staging/Produktion:** Environment variables via container orchestrator (Kubernetes Secrets / Docker Secrets)
- Inga hemligheter i git, inga `.env`-filer i produktion
- Roteras kvartalsvis: JWT-nycklar, databaspassord, SMTP-credentials

### Konfiguration per modul
- Varje modul har en `[ModuleName]Options`-klass
- Bindas via `IOptions<T>` pattern från `appsettings.json`
- Överskridbar via environment variables i produktion

---

## 19. Kanonisk referens — löneberäkningssatser 2026

Enda källan till sanning för alla löneberäkningar. Vid konflikt mellan dokument gäller denna tabell:

| Parameter | Värde | Källa |
|-----------|-------|-------|
| Prisbasbelopp (PBB) 2026 | 59 200 kr | SCB |
| Inkomstbasbelopp (IBB) 2026 | 80 600 kr | Pensionsmyndigheten |
| Arbetsgivaravgift (standard) | 31.42% | Skatteverket |
| Arbetsgivaravgift (ungdom <23) | 20.81% | Skatteverket |
| Arbetsgivaravgift (senior 67+) | 10.21% | Skatteverket |
| Semesterdagstillägg per dag | 0.43% av månadslön | AB 25 §27 |
| Sammalöneregeln per dag | 0.80% av månadslön | Semesterlagen + AB |
| Sjuklön dag 2-14 | 80% av lön | Sjuklönelagen |
| Karensavdrag | 20% av genomsnittlig veckoarbetsinkomst | Sjuklönelagen |
| Pension AKAP-KR under 7.5 IBB | 6% | AKAP-KR-avtalet |
| Pension AKAP-KR över 7.5 IBB | 31.5% | AKAP-KR-avtalet |
| OB vardag kväll (19-22) | 46.02 kr/h | AB 25 |
| OB vardag natt (22-06) | 112.52 kr/h | AB 25 |
| OB helg (fre 19 - mån 06) | 54.70 kr/h | AB 25 |
| OB storhelg | 130.21 kr/h | AB 25 |
| Övertid enkel | 180% | AB 25 |
| Övertid kvalificerad | 240% | AB 25 |
| Max övertid per år | 200 timmar | ATL |
| Max arbetstid per vecka (snitt 4v) | 48 timmar | ATL |
| Dygnsvila minimum | 11 timmar | ATL |
| Veckovila minimum | 36 timmar sammanhängande | ATL |

**Observera:** Satserna laddas från konfiguration, inte hårdkodade i kod. Uppdateras årligen i `config.payroll_rates`-tabell.

---

## 20. Sammanfattning

| Aspekt | Beslut |
|--------|--------|
| **Princip** | FOSS + Stina-testet + kraftfull under huven |
| **Stack** | .NET 9, Blazor, MudBlazor, PostgreSQL, RabbitMQ — 100% MIT/Apache/MPL |
| **PDF** | PdfSharpCore (MIT) |
| **Schemaoptimering** | Google OR-Tools (Apache 2.0) |
| **UX** | Tre rollvyer, konversationsflöden, intelligent assistans |
| **Språk** | i18n, svenska + engelska, RTL-redo |
| **Mobil** | PWA med offline, push, installering |
| **Sidor** | ~130 sidor / 25 moduler |
| **Intelligens** | Systemet är experten — auto-genererar, validerar, föreslår |
| **Kritiska luckor** | 29 st löses i fördjupningen |
| **Tester** | 475 → 1500+ |
| **Databas** | 13 nya migrationer |
| **Säkerhet** | RLS, kryptering, WCAG 2.1 AA, rollbaserad åtkomst |
| **Driftsäkerhet** | RTO 4h, RPO 15min, streaming replication, veckovis backuptest |
| **Migration** | Fasad övergång med 3 månaders parallellkörning |
| **Prestanda** | <500ms sidladdning, <5min lönekörning, 500+ samtidiga användare |

| Aspekt | Beslut |
|--------|--------|
| **Princip** | FOSS + Stina-testet + kraftfull under huven |
| **Stack** | .NET 9, Blazor, MudBlazor, PostgreSQL, Valkey, RabbitMQ |
| **UX** | Tre rollvyer, konversationsflöden, intelligent assistans |
| **Språk** | i18n, svenska + engelska, RTL-redo |
| **Mobil** | PWA med offline, push, installering |
| **Sidor** | ~130 sidor / 25 moduler |
| **Intelligens** | Systemet är experten — auto-genererar, validerar, föreslår |
| **Kritiska luckor** | 29 st löses i fördjupningen |
| **Tester** | 475 → 1500+ |
| **Databas** | 13 nya migrationer |
| **Säkerhet** | RLS, kryptering, WCAG 2.1 AA, rollbaserad åtkomst |
