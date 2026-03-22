# OpenHR "Britta 63" Usability & Quality Overhaul

**Date:** 2026-03-22
**Scope:** Hela kodbasen — UX, backend, infrastruktur, dokumentation
**Princip:** Systemet ska vara experten, inte användaren. Britta 63, utan datorvana, ska kunna använda alla funktioner utan förkunskap.

---

## 1. Bakgrund & Sammanfattning

En systematisk granskning av hela OpenHR-kodbasen (160+ sidor, 38 moduler, 207+ entiteter) har identifierat **~400 problem** som försvårar användningen för icke-tekniska användare. Granskningen har också verifierat alla påståenden i README.md och CLAUDE.md mot faktisk kod.

### Identifierade problem per allvarlighet

| Allvarlighet | Antal | Kategori |
|---|---|---|
| Kritisk | ~45 | Blockerar grundläggande arbetsflöden |
| Hög | ~130 | Kräver gissning eller extern hjälp |
| Medel | ~140 | Förvirrande men kringgåbar |
| Låg | ~85 | Polering och konsekvens |

### De 8 systemövergripande problemen

1. **Fackspråk utan förklaring** — HR-jargong, juridiska termer, förkortningar överallt utan tooltips/ordlista
2. **Komplexa formulär utan steg-guidning** — Alla fält på en gång, inga wizards, inga defaults
3. **45+ navigationsval utan prioritering** — Ingen sökning, ingen rollbaserad förenkling
4. **Status/färgkodning utan legend** — Grönt/gult/rött utan förklaring
5. **Tysta fel och saknad feedback** — 280+ hårdkodade felmeddelanden, tysta bakgrundsjobb
6. **Kritiska backend-buggar** — Felaktig skatteberäkning, saknade helgdagar i semesterberäkning
7. **Admin kräver IT-expertis** — JSON-inmatning, SLA i råa minuter, kryptiska koder
8. **Saknade UX-grundmönster** — Ingen onboarding, inga breadcrumbs, ingen ordlista

---

## 2. Dokumentationsavvikelser (README.md vs CLAUDE.md vs Kod)

### CLAUDE.md — Kraftigt föråldrad, kräver omskrivning

| Påstående i CLAUDE.md | Verklighet | Status |
|---|---|---|
| 25 moduler | 38 moduler | FEL — föråldrad |
| 494 tester | ~1 123 tester | FEL — föråldrad |
| 96 Blazor-sidor | 178 routade sidor | FEL — föråldrad |
| 50+ i18n-nycklar | 329 nycklar (sv), 330 (en) | FEL — föråldrad |
| 94 rutter | ~178 rutter | FEL — föråldrad |

### README.md — Mestadels korrekt, men med avvikelser

| Påstående | Verklighet | Status |
|---|---|---|
| 1 116 tester | ~1 123 (nära) | OK |
| 207 domänentiteter | 207 filer, ~228 klasser + 98 enums | OK (räknar filer) |
| 38 moduler | 38 exakt | OK |
| ~177 sidor | 178 routade | OK |
| 328 i18n-nycklar | 329 sv / 330 en | OK |
| Bottom navigation | CSS definierad, inget Razor-komponent | DELVIS — ej funktionell |
| Swipe-gester | CSS definierad, inget JS | DELVIS — ej funktionell |
| QuestPDF | Ej installerat, platshållarkod | DELVIS — "redo" stämmer |
| SignalR | Hubben finns, men "ej aktiverad" | OK (dokumenterat) |

### Moduler — 8 av 17 granskade funktionspåståenden är "Scaffolded"

| Modul/Funktion | Påstående | Verklighet |
|---|---|---|
| Automatiseringsramverk | "22 regler med execution" | 22 regler seedade, men **ingen exekveringsmotor** |
| Prediktiva modeller | "AI-drivna prognoser" | Entiteter finns, **ingen beräkningslogik** |
| Demand forecasting | "Historikbaserad prognos" | Entiteter + API, **ingen genereringsalgoritm** |
| Webhook retry | "Exponential backoff" | Schema beräknas, **ingen bakgrundsjobb kör retries** |
| API-nyckel scope | "Scope-begränsning" | Scope lagras, **aldrig enforced i middleware** |
| Custom Objects | "JSON Schema-validering" | Schema lagras som sträng, **ingen validering** |
| Marketplace plugins | "Installation och konfiguration" | Registrering finns, **ingen plugin-execution** |
| HR Service routing | "Routing-regler" | Manuell tilldelning, **inga automatiska regler** |

**Fullt implementerade (9):** Migration (10 adaptrar), 10 kollektivavtal, VMS F-skatt, Fatigue scoring + constraint solver, Talent matching, AI-assistent + 20 artiklar, Shift bidding (4 algoritmer), Grievance (full state machine), EU Pay Transparency (kohortanalys + regression), ONA (Brandes betweenness centrality).

---

## 3. Åtgärdsplan — Översikt

Planen är indelad i **6 faser** som bygger på varandra. Varje fas har tydliga leverabler.

| Fas | Fokus | Uppskattad omfattning |
|---|---|---|
| **Fas 0** | Dokumentation & kritiska buggar | CLAUDE.md, README.md, skatteberäkning, semesterhelgdagar |
| **Fas 1** | Global UX-infrastruktur | Tooltip-system, ordlista, wizard-ramverk, felhantering |
| **Fas 2** | Min Sida & Självservice | De sidor Britta använder dagligen |
| **Fas 3** | HR & Admin-arbetsflöden | Personal, lön, ledighet, schema, ärenden |
| **Fas 4** | Avancerade moduler & admin | Rapporter, GDPR, rekrytering, VMS, admin-config |
| **Fas 5** | Backend-komplettering & infrastruktur | Scaffolded features, paginering, PDF, i18n |

---

## 4. Fas 0 — Dokumentation & Kritiska Buggar

### 0.1 Uppdatera CLAUDE.md

CLAUDE.md speglar en version med 25 moduler och 494 tester. Hela filen måste skrivas om för att matcha nuvarande kodbas (38 moduler, 1 123 tester, 178 sidor, 329 i18n-nycklar). Alla modulbeskrivningar, konventioner och kommandon ska verifieras.

### 0.2 Korrigera README.md

- Ta bort påståenden om "bottom navigation" och "swipe-gester" (eller markera som "planned")
- Förtydliga "QuestPDF-redo" → "PDF-generering via PdfSharpCore (QuestPDF ej installerat)"
- Uppdatera avvikande siffror

### 0.3 Kritisk bugg: Hårdkodad 30% skatt i NyKorning.razor

**Fil:** `src/Web/Components/Pages/Lon/NyKorning.razor` rad 78-82
**Problem:** `result.Skatt = Money.SEK(Math.Round(brutto * 0.30m, 0))` — ignorerar fullständigt PayrollCalculationEngine med riktiga skattetabeller.
**Åtgärd:** Integrera PayrollCalculationEngine.BeraknaLon() istället för hårdkodad multiplikator.

### 0.4 Kritisk bugg: Semesterdagar ignorerar svenska helgdagar

**Fil:** `src/Modules/Leave/Domain/LeaveRequest.cs` rad 136
**Problem:** RaknaArbetsdagar() räknar mån-fre utan att exkludera röda dagar (midsommar, jul, etc.).
**Åtgärd:** Integrera svensk helgdagskalender (SvenskaHelgdagar) i DateRange.WorkDays-beräkning. Gör kalendern konfigurerbar per region.

### 0.5 Kritisk bugg: IBB-konstanter kräver manuell årsuppdatering

**Fil:** `src/Modules/Payroll/Engine/PayrollCalculationEngine.cs` rad 51-52
**Problem:** `IBB_2025 = 80600, IBB_2026 = 83400` hårdkodade. Ingen varning vid årsskifte.
**Åtgärd:** Flytta IBB till konfigurationstabell. Bakgrundsjobb varnar admin 30 dagar före årsskifte om IBB ej uppdaterats.

### 0.6 Kritisk bugg: Lönekörning fortsätter tyst vid beräkningsfel

**Fil:** `src/Modules/Payroll/Services/PayrollBatchService.cs` rad 77-78
**Problem:** Vid fel loggas det och nästa anställd bearbetas. Lönekörning kan slutföras med anställda som saknar korrekt beräkning.
**Åtgärd:** Samla alla fel, visa felrapport, kräv explicit bekräftelse/korrigering före godkännande.

### 0.7 Kritisk bugg: LAS-beräkning tidszonsosäker

**Fil:** `src/Modules/LAS/Domain/LASAccumulation.cs` rad 85
**Problem:** `DateOnly.FromDateTime(DateTime.Today)` är lokal tid — kan vara 1 dag fel i UTC-miljö.
**Åtgärd:** Injicera klocktjänst (IClock) för testbarhet och konsekvens.

### 0.8 Kritisk bugg: Rehabmilstolpar beräknas från skapandedatum

**Fil:** `src/Modules/HalsoSAM/Domain/RehabCase.cs` rad 49-52
**Problem:** Milstolpar (dag 14/90/180/365) räknas från `DateTime.UtcNow` vid skapande, inte från rehabstart.
**Åtgärd:** Lägg till RehabStartDatum-fält. Beräkna milstolpar först när rehab aktiveras.

### 0.9 Privacy-bugg: TotalRewards fallback till första anställd

**Fil:** `src/Web/Components/Pages/MinSida/TotalRewards.razor` rad 119-125
**Problem:** Om auth misslyckas visas första anställds data — allvarligt integritetsbrott.
**Åtgärd:** Visa felmeddelande istället. Aldrig fallback till annan persons data.

### 0.10 Kritisk bugg: GDPR-registerutdrag kräver GUID-inmatning

**Fil:** `src/Web/Components/Pages/GDPR/Index.razor` rad 94
**Problem:** Britta måste skriva in ett GUID manuellt. Omöjligt arbetsflöde.
**Åtgärd:** Ersätt med MudAutocomplete som söker anställda på namn/personnummer.

---

## 5. Fas 1 — Global UX-infrastruktur

Dessa åtgärder skapar infrastruktur som alla sidor sedan använder.

### 1.1 Tooltip-system för fackspråk (OhrGlossaryTooltip)

Skapa en Blazor-komponent `<OhrTerm term="LAS" />` som renderar termen med en tooltip-förklaring. Alla HR-termer (LAS, MBL, HälsoSAM, VAB, OB-tillägg, SAVA, AG-avgifter, bruttolön, nettolön, sysselsättningsgrad, befattning, lönearter, traktamente, karensavdrag, etc.) definieras i en central ordlista (JSON eller .resx) med klartext på svenska.

**Implementering:**
- `src/DesignSystem/Components/OhrTerm.razor` — renderar `<MudTooltip Text="@_definition">@Term</MudTooltip>`
- `src/Web/Services/GlossaryService.cs` — laddar och tillhandahåller ordlista
- Ordlistan bör innehålla ~100 termer initialt
- Tillgänglig via `/hjalp/ordlista` som fullständig sida

### 1.2 Wizard/steg-för-steg-ramverk (OhrWizard)

Utöka befintliga OhrConversationFlow med tydligare steg-indikering:
- Visa "Steg 2 av 5: Kontaktuppgifter"
- Visa "Nästa: Granska och bekräfta"
- Stöd för tillbaka-navigering
- Stöd för att spara utkast

### 1.3 Global felhanteringsmall

Ersätt alla 280+ `catch (Exception ex) { _error = $"Kunde inte...: {ex.Message}"; }` med:
- `ErrorDisplayService` som loggar tekniskt och visar användarvänligt meddelande
- Standardmall: "Vi kunde inte [åtgärd]. Försök igen eller kontakta HR på [telefon]."
- Aldrig exponera `ex.Message` direkt till användaren

### 1.4 Hjälp-knapp och kontextuell hjälp

Lägg till `<OhrHelpButton />` i varje sidans header som öppnar en panel med:
- Kort förklaring av vad sidan gör
- Vanliga frågor för just den sidan
- Länk till ordlistan
- Kontaktinfo till HR/IT

### 1.5 Statuschips-legend (OhrStatusLegend)

Komponent som visar förklaring av alla färgkodade statusar på en sida:
- Grön = Godkänd/OK
- Gul = Väntar/Varning
- Röd = Avvisad/Problem
- Ska användas konsekvent i alla tabeller med statuschipsar

### 1.6 Bekräftelsedialog för destruktiva åtgärder

Skapa `<OhrConfirmDialog>` som används före:
- Godkännande av lönekörning (nuvarande = ett klick utan bekräftelse)
- Avslag av ansökningar
- Radering av data
- Alla irreversibla åtgärder

### 1.7 Onboarding/guidad tur

Skapa en första-gången-upplevelse som visas vid första inloggning:
- 4-5 steg som visar huvuddelarna av systemet
- Anpassad per roll (Anställd ser "Min Sida", Chef ser "Mitt Team", HR ser "Personal")
- Kan visas igen via "Visa guiden igen" i hjälpmenyn

### 1.8 Breadcrumb-navigation

Ersätt nuvarande breadcrumb (som visar komponentnamn som "PersonalList") med användarvänliga sökvägar: "Start > Personal > Anna Svensson > Detaljer". Använd sidtitlar från `@page`-attribut.

---

## 6. Fas 2 — Min Sida & Självservice

Dessa sidor är Brittas dagliga kontaktyta med systemet.

### 2.1 MinSida Dashboard (`MinSida/Index.razor`)

- Byt "Semesterdagar kvar" → "Du har X dagar ledigt kvar i år" med `<OhrTerm>`
- Byt "Öppna ärenden" → "Dina ansökningar och förfrågningar"
- Lägg till "Vad vill du göra?"-sektion med 3-4 stora knappar: "Sjukanmäl", "Begär ledighet", "Se mitt schema", "Se min lön"

### 2.2 Sjukanmälan (`MinSida/Sjukanmalan.razor`)

- Redan bra (använder OhrConversationFlow) — behåll mönstret
- Lägg till förklaring i steg 0: "Vi meddelar din chef automatiskt"
- Lägg till datumvalidering (slutdatum kan inte vara före startdatum)
- Byt "Läkarintyg" → "Läkarintyg (intyg från din läkare)" med `<OhrTerm>`
- Byt tekniska felmeddelanden till användarvänliga

### 2.3 Min Lön (`MinSida/MinLon.razor` + `Lonespecifikationer.razor`)

- Slå ihop till en sida (nu duplicerat)
- Byt "Bruttolön" → "Lön före skatt" med `<OhrTerm term="Bruttolön">`
- Byt "Nettolön" → "Det du får utbetalt" med `<OhrTerm>`
- Ta bort "AG-avgifter" från anställdvy (irrelevant för anställd)
- Lägg till jämförelse: "+250 kr mer än förra månaden"
- Fixa teckenkodning ("Forsakringar" → "Försäkringar")

### 2.4 Mitt Schema (`MinSida/MittSchema.razor`)

- Lägg till veckonavigering (föregående/nästa vecka)
- Byt "7.5" → "7 tim 30 min"
- Lägg till förklaring vid lediga dagar: "Du är ledig denna dag"
- Visa felmeddelande istället för Debug.WriteLine vid laddningsfel

### 2.5 Min Ledighet (`MinSida/MinLedighet.razor`)

- Byt "Tillgängliga" → "Dagar du kan ta ledigt"
- Byt "Intjänade" → "Dagar du har tjänat in"
- Byt "Sparade" → "Sparade dagar från förra året"
- Lägg till sammanfattning: "Du har 18 dagar. Du är planerad för 15. Kvar: 3."

### 2.6 Min Profil (`MinSida/MinProfil.razor`)

- Ta bort hårdkodad demo-data för nödkontakter (Karl Svensson, Maria Lindgren)
- Markera read-only-fält visuellt (grått + lås-ikon + "Kan bara ändras av HR")
- Lägg till relation-dropdown istället för fritext
- Varna vid osparade ändringar om användaren navigerar bort

### 2.7 Total Rewards (`MinSida/TotalRewards.razor`)

- Ta bort fallback till första anställd (privacy-bugg, se 0.9)
- Byt "Din totala kompensation" → "Vad din anställning är värd totalt"
- Lägg till tooltips för varje komponent (pension, försäkringar, förmåner)
- Fixa teckenkodning
- Lägg till "Ladda ner som PDF"-knapp

### 2.8 Mina Ärenden (`MinSida/MinaArenden.razor`)

- Byt "Mina pågående saker" → "Mina ansökningar och förfrågningar"
- Översätt statuskoder: "VantarGodkannande" → "Väntar på din chefs godkännande"
- Lägg till klickhantering för att se detaljer
- Lägg till historikvy: "Se tidigare ärenden (senaste 6 månader)"

---

## 7. Fas 3 — HR & Admin-arbetsflöden

### 3.1 Personal — Ny Anställd (`Anstallda/NyAnstalld.razor`)

- Konvertera till 3-stegs wizard: (1) Grundinfo, (2) Kontakt, (3) Sammanfattning
- Lägg till fälthjälp: "Personnummer: Svenskt ID-nummer, format ÅÅÅÅMMDD-NNNN"
- Markera valfria fält tydligt: "Telefon (valfritt)"
- Visa vad auto-genererad e-post blir innan sparning
- Byt tekniska felmeddelanden

### 3.2 Personal — Detalj (`Anstallda/Detalj.razor`)

- Reducera från 10 tabbar till 5 grupper: "Person", "Anställning & Lön", "Frånvaro", "Kompetens", "Dokument"
- Lägg till `<OhrTerm>` på alla facktermer (Tillsvidare, Skattetabell, Kyrkoavgift etc.)
- Översätt audit-log från engelska (Create/Update/Delete) till svenska
- Lägg till färglegend för statuschipsar

### 3.3 Ledighet — Ny Ansökan (`Ledighet/NyAnsoken.razor`)

- Auto-välj inloggad användare (kräv inte att Britta väljer sig själv i dropdown)
- Förklara ledighetstyper inline: "Semester = din semesterledighet", "VAB = vård av sjukt barn"
- Visa saldo i klartext: "Du har 18 dagar kvar. Denna ansökan drar 5 dagar."
- Visa vem som godkänner: "Skickas till din chef Eva Nilsson"

### 3.4 Ledighet — Kalender (`Ledighet/Kalender.razor`)

- Ersätt emojis + 3-bokstavsförkortningar med fulla ord: "Semester", "Sjuk", "VAB"
- Öka fontstorlek från 0.7rem till minst 0.9rem
- Lägg till text på navigeringsknappar: "← Föregående månad" / "Nästa månad →"
- Lägg till legend som förklarar färgkodningen

### 3.5 HälsoSAM (`HalsoSAM/Index.razor` + `NyttArende.razor`)

- Skriv om disclaimer från backend-dokumentation till klartext
- Förklara milstolpar i klartext: "Dag 14: Vi anmäler till Försäkringskassan"
- Lägg till åtgärdsknappar vid varje milstolpe: "Boka möte nu"
- Visa hur lång tid som gått: "Förfallen: 3 dagar sedan — boka möte snart" (röd)

### 3.6 Lön — Ny Körning (`Lon/NyKorning.razor`)

- Integrera riktiga skatteberäkningar (se 0.3)
- Lägg till bekräftelsedialog vid godkännande (se 1.6)
- Visa skatteuppdelning: "Kommunalskatt: X kr, Statlig skatt: Y kr"
- Visa jämförelse med föregående månad
- Visa avvikelserapport tydligt med förklaring

### 3.7 Lön — Lönearter (`Lon/Lonearter.razor`)

- Lägg till `<OhrTerm>` för varje löneart: "OB-tillägg = Extra ersättning för kvällar/nätter/helger"
- Förklara koder: "1310 = Kvällstillägg 46 kr/h", "3001 = Karensavdrag"
- Skapa nedladdningsbar ordlista som PDF

### 3.8 Löneöversyn (`Loneoversyn/Index.razor`)

- Lägg till stepper: "1. Skapa runda → 2. Fördela → 3. Facklig avstämning → 4. Godkännande → 5. Genomförd"
- Visa nästa-steg-knapp baserat på aktuell status
- Förklara processen i klartext på sidan

### 3.9 Schema — Stampling (`Stampling/Index.razor`)

- Lägg till prominent "Stämpla in"-knapp (grön, stor)
- Visa aktuell status: "Du är INTE instämplad" eller "Instämplad sedan 08:00"
- Visa aktuell tid

### 3.10 Schema — ATL (`Schema/ATL.razor`)

- Ersätt liten alert med expanderbar "Vad är ATL?"-sektion
- Klartext: "Max 48 timmars arbete per vecka. Min 11 timmars vila mellan pass."
- Förklara VARFÖR: "Skyddar din hälsa och dina kollegors"

### 3.11 Schema — Optimering (`Schema/Optimering.razor`)

- Förklara vad optimeringsknappen gör: "Systemet skapar det mest rättvisa schemat"
- Visa progressindikator under beräkning
- Visa resultat med ✅/⚠️: "ATL-kompatibelt" eller "Problem: behöver 2 fler anställda kvällar"

### 3.12 Ärenden — MBL (`Arenden/MBL.razor`)

- Lägg till juridisk vägledning i klartext ovanför formuläret
- "§19 Information = arbetsgivaren informerar facket" vs "§11-14 Förhandling = facket förhandlar"
- Visa MBL-process som tidslinje: "Kallelse → Förhandling → Protokoll → Avslut"

### 3.13 LAS (`LAS/Index.razor`)

- Förklara gränsvärden i klartext: "Max 365 dagar visstidsanställning per 5-årsperiod"
- Lägg till "Konvertera till tillsvidare"-knapp i expanded panel
- Visa deadline: "Konvertering måste ske senast [datum]"

### 3.14 Godkännanden (`Godkannanden/Index.razor`)

- Visa konsekvenser vid godkännande: "Godkännande minskar X's saldo från 20 till 15 dagar"
- Avkommentera avslå-knapp (för närvarande utkommenterad)
- Visa vem som tar över efter godkännande

---

## 8. Fas 4 — Avancerade moduler & Admin

### 4.1 Dokument — Mallgenerator (`Dokument/MallGenerator.razor`)

- Installera QuestPDF (eller fullgör PdfSharpCore-generering) — nuvarande ger textfiler
- Lägg till riktig PDF-förhandsgranskning
- Konvertera till 3-stegs wizard: "Välj mall → Fyll i detaljer → Granska och ladda ned"

### 4.2 Rapporter — Lönekartering (`Rapporter/Lonekartering.razor`)

- Förklara "Lönekartering" i klartext: "Visar löner per befattning för att hitta diskriminering"
- Förklara 15%-gränsen: "Om skillnaden är >15% bör ni utreda"
- Lägg till `<OhrTerm>` på alla termer

### 4.3 Rapporter — KPI Dashboard (`Rapporter/KPIDashboard.razor`)

- Lägg till legend: "Grön = Bra, Gul = Varning, Röd = Åtgärd krävs"
- Visa tröskelvärden: "Sjukfrånvaro: Grönt under 4%, Gult 4-6%, Rött över 6%"
- Förklara "Riktning" i klartext: "Högre är bättre" / "Lägre är bättre"

### 4.4 Rapporter — Lönetransparens (`Rapporter/Lonetransparens.razor`)

- Skapa guidad arbetsflöde (stepper): "Vad är lönegranskning? → Förstå resultaten → Vad ska du göra?"
- Förklara "ojusterat gap" vs "justerat gap" i klartext
- Lägg till åtgärdsförslag: "Omedelbar åtgärd", "Gradvis", "Via nyanställningar"

### 4.5 Rapporter — ReportBuilder (`Rapporter/ReportBuilder.razor`)

- Dokumentera datakällor med kolumnbeskrivningar
- Ersätt hårdkodade filteralternativ med dynamiska (från DB)
- Lägg till data-preview (visa 5 första raderna) innan sparning
- Förklara gruppering och visualiseringsalternativ

### 4.6 GDPR (`GDPR/Index.razor`)

- Ersätt GUID-inmatning med sökbar anställdlista (se 0.10)
- Lägg till steg-för-steg-guide: "Registrera → Verifiera identitet → Samla data → Skicka svar → Dokumentera"
- Förklara DSR-typer i klartext: "Registerutdrag = ge personen kopia av all deras data"
- Visa 30-dagars deadline tydligt med varningsfärg

### 4.7 Admin — Automatisering (`Admin/Automation.razor` etc.)

- Förklara varje automatiseringsregel i klartext
- Visa "Dessa regler körs automatiskt. Du behöver inte göra något."
- Lägg till "Testa regel"-knapp som visar vilka anställda som matchar

### 4.8 Admin — Avtal (`Admin/Avtal/`)

- Aktivera "Nytt avtal"-knapp med förklaring (eller tooltip som förklarar varför den är inaktiv)
- Gör detaljsidan redigerbar (nu read-only)
- Förklara försäkringskoder: "TGL = Gruppliv", "AGS = Arbetsskada", etc.
- Visa OB-kategorier med tider: "Vardagkväll (18:00-22:00) — 46 kr/h"

### 4.9 Admin — Benefits/Eligibility Rules (`Admin/Formaner/Regler.razor`)

- Ersätt JSON-inmatning med strukturerat formulär
- Ersätt kryptiska operatorer: "GE" → "är minst (≥)"
- Ersätt tekniska fältnamn: "Sysselsattningsgrad" → "Arbetstid i procent (t.ex. 75%)"
- Lägg till "Testa regel"-knapp som visar matchande anställda
- Validera regler i realtid

### 4.10 Admin — SLA Management (`Admin/Helpdesk/SLAManagement.razor`)

- Ersätt minuter med läsbar input: "[4] timmar [0] minuter"
- Visa förvalt alternativ: "4 timmar", "8 timmar", "1 dag"
- Förklara SLA i klartext

### 4.11 Admin — Kunskapsbas (`Admin/Kunskapsbas/NyArtikel.razor`)

- Lägg till markdown-förhandsgranskning (side-by-side editor)
- Visa "Förhandsgranska som anställd"-knapp

### 4.12 Admin — Migration (`Admin/Migration/`)

- Visa status i klartext: "DryRun" → "Testkörning — ingen data importerad ännu"
- Visa nästa-steg-knapp baserat på status
- Lägg till felrapport för misslyckade rader

### 4.13 Admin — ONA (`Admin/ONA/`)

- Ersätt JSON-frågor med strukturerad frågebyggare
- Lägg till nätverksvisualisering (D3.js force-directed graph)
- Visa förhandsgranskning av enkät innan den öppnas

### 4.14 Rekrytering — Pipeline (`Rekrytering/Pipeline.razor`)

- Gör pipeline interaktiv (flytta kandidater mellan steg)
- Visa visuella steg-kolumner: "Ansökningar | Intervjuer | Erbjudande | Anställd"
- Lägg till statusantal per steg

### 4.15 Chef-portal (`Chef/Index.razor` etc.)

- Byt emojis mot MudBlazor-ikoner
- Expandera kortlabels: "Anställda" → "Visa alla anställda"
- Förklara coaching-nudges i klartext
- Öka fontstorlek i tabeller (body1 minimum)

### 4.16 VMS (`VMS/`)

- Dölj tekniska kolumner (F-skatt, betyg) för icke-inköpsroller
- Förklara begrepp: "Ramavtal = långsiktigt avtal med leverantör"
- Lägg till arbetsflödesvisualisering: "Utkast → Inskickad → Attesterad"

### 4.17 Formaner (`Formaner/`)

- Förklara varje förmån med en mening: "Friskvårdsbidrag = pengar för träning/motion"
- Ersätt fritext "Aktivitet" med dropdown (Gym, Yoga, Simning, Massage, Annat)
- Visa arbetsgivarandel i klartext: "Du betalar 20%, regionen betalar 80%"

---

## 9. Fas 5 — Backend-komplettering & Infrastruktur

### 5.1 Fullgör scaffolded features

| Feature | Åtgärd |
|---|---|
| Automation execution engine | Implementera `IAutomationEngine` med regelexekvering baserad på triggers |
| Predictive models | Implementera enkla heuristik-baserade prognoser (ej ML) — eller ta bort "AI-drivna" från README |
| Demand forecasting | Implementera prognos baserad på historisk data (medeltal per veckodag) |
| Webhook retry | Skapa `WebhookRetryBackgroundService` som pollar och kör retries |
| API key scope enforcement | Skapa `ApiKeyMiddleware` som validerar scope mot request |
| Custom Objects validation | Integrera JSON Schema-validering (NJsonSchema) |
| Marketplace plugin execution | Implementera grundläggande plugin-applicering (skapa custom objects från manifest) |
| HR Service auto-routing | Implementera regelbaserad routing (kategori → kö-mappning) |

### 5.2 Fullgör bakgrundsjobb

- `CertificationReminderService` — Implementera faktisk DB-query och notifieringsskapande (nu stub)
- `LASAlertService` — Implementera faktisk DB-query och alert-skapande (nu stub)
- Skapa `/admin/background-jobs` dashboard som visar senaste körning, status, och fel

### 5.3 Databasfrågor — Paginering och filtrering

Repositories laddar ALL data (`GetAllAsync()` → `ToListAsync()`). Vid 10 000+ anställda blir detta ohållbart.
- Lägg till `GetPaginatedAsync(page, pageSize, searchTerm?, filter?)` i alla repositories
- Implementera server-side sortering och filtrering
- Lägg till sökning som söker på namn, befattning, enhet (inte bara förnamn/efternamn)

### 5.4 PDF-generering

`PdfGenerator.cs` genererar text-baserade UTF-8-strängar, inte riktiga PDF:er. `PdfPayslipGenerator.cs` använder PdfSharpCore och genererar professionella PDF:er.
- Applicera PdfPayslipGenerator-mönstret på alla dokumenttyper (tjänstgöringsintyg, anställningsavtal)
- Alternativt: installera QuestPDF och migrera till det

### 5.5 CSV-export

- Lägg till BOM (Byte Order Mark) i CSV-filer för korrekt Excel-hantering av svenska tecken
- Lägg till metadata (exportdatum, antal rader)

### 5.6 E-post/SMS-felhantering

- Returnera `EmailDeliveryResult` med success/failure
- Visa varning i UI: "E-post till din chef kunde inte skickas"
- Implementera retry-logik

### 5.7 i18n-komplettering

- Lägg till saknad "LogOut"-nyckel i sv.resx
- Ersätt alla hårdkodade svenska strängar i OhrAssistant med L["..."]
- Ersätt hårdkodade strängar i NavMenu (rad 74-75, 139) med L["..."]
- Lägg till alla saknade engelska översättningar

### 5.8 Domänmodell-förbättringar

| Problem | Fil | Åtgärd |
|---|---|---|
| BeraknaTimlon() hårdkodar 38.25h/v | Employment.cs | Hämta timmar från kollektivavtal |
| Semester ej uppdaterad vid födelsedag | VacationBalance.cs | Dynamisk beräkning baserad på ålder |
| OBKategori utan default/validering | ScheduledShift.cs | Gör required, sätt default baserat på tid |
| Case Godkann() använder LastOrDefault | Case.cs | Använd FirstOrDefault, validera |
| Överlappande ledighetsansökningar | LeaveRequest.cs | Validera mot existerande ansökningar |
| BenefitEnrollment status som string | BenefitEnrollment.cs | Byt till enum |
| Notification ActionUrl ej validerad | Notification.cs | URI-validering, blockera javascript: |
| OffboardingCase tillåter paserat datum | OffboardingCase.cs | Validera framtida datum |
| TravelClaim utan dagsmaximum | TravelClaim.cs | Validera traktamentedagar ≤ resedagar |
| Vacancy Bedoma() utan poänggräns | Vacancy.cs | Validera 1-5 skala |

### 5.9 Tillgänglighet (Accessibility)

- Säkerställ minst 14px (0.875rem) för ALL text (tabellhuvuden nu 0.75rem)
- Lägg till aria-labels på alla ikonknappar
- Implementera skip-to-content-länk (CSS finns, HTML saknas)
- Testa färgkontrast i dark mode
- Säkerställ 44x44px touchmål för alla interaktiva element på mobil

### 5.10 Bottom navigation & Swipe (ej implementerat)

CSS finns men inget Razor-komponent eller JS existerar.
- **Beslut krävs:** Implementera fullt, eller ta bort från README?
- Om implementera: skapa `BottomNav.razor` för mobil med 4-5 snabbval
- Skapa swipe.js med touch-event-hantering för kortnavigering

---

## 10. Tvärsnittsproblem — Checklista per sida

Följande ska gälla ALLA ~178 sidor:

- [ ] Alla facktermer har `<OhrTerm>` eller tooltip
- [ ] Alla formulärfält har label, hjälptext, och exempel
- [ ] Alla tabeller har sorteringsbara kolumner
- [ ] Alla statuschipsar har textlabel (inte bara färg)
- [ ] Alla felmeddelanden talar om hur man löser problemet
- [ ] Alla destruktiva åtgärder har bekräftelsedialog
- [ ] Keyboard-navigering fungerar (Tab, Enter, Escape)
- [ ] Textkontrast uppfyller WCAG AA (4.5:1)
- [ ] Minsta fontstorlek 14px

---

## 11. Prioriteringsordning

### Vecka 1-2: Fas 0 (Kritiska buggar + Dokumentation)
- Fixa skatteberäkning, semesterhelgdagar, privacy-bugg
- Uppdatera CLAUDE.md och README.md
- Fixa GDPR GUID-inmatning

### Vecka 3-4: Fas 1 (Global UX-infrastruktur)
- Bygg tooltip-system, wizard-ramverk, felhanteringsmall
- Implementera hjälpknappar, statuslegend, bekräftelsedialoger
- Breadcrumbs

### Vecka 5-8: Fas 2 (Min Sida)
- Alla 9 Min Sida-sidor
- Självservice-upplevelsen ska vara felfri för Britta

### Vecka 9-14: Fas 3 (HR-arbetsflöden)
- Personal, lön, ledighet, schema, ärenden, LAS, MBL
- 20+ sidor

### Vecka 15-20: Fas 4 (Avancerade moduler)
- Rapporter, GDPR, rekrytering, VMS, admin, chef
- 50+ sidor

### Vecka 21-26: Fas 5 (Backend & Infrastruktur)
- Scaffolded features, paginering, PDF, i18n
- Domänmodellförbättringar

---

## 12. Definition of Done per fas

Varje fas är klar när:
1. Alla identifierade problem i fasen är åtgärdade
2. Alla tester passerar (`dotnet test RegionHR.sln`)
3. Inga nya varningar vid build (`dotnet build`)
4. Manuell genomgång ur Britta-perspektivet bekräftar att flödet fungerar utan hjälp
5. CLAUDE.md och README.md uppdaterade om förändringar påverkar dem
