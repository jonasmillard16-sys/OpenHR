# OpenHR — Implementeringsstatus

**Senast uppdaterad:** 2026-03-17
**Version:** 1.0.0-beta
**Licens:** AGPL-3.0
**Build:** 0 errors | 40 commits | 88 sidor | 25 moduler

---

## 1. Översikt

OpenHR (tidigare RegionHR) är ett komplett, produktionsklart HR-system byggt för svenska regioner med 10 000+ anställda inom sjukvård. Systemet ersätter HEROMA (CGI) och består av 100% fri och öppen källkod (FOSS).

### Nyckeltal

| Mått | Värde |
|------|-------|
| Blazor-sidor (.razor) | 88 |
| Domänmoduler | 25 |
| API-endpoints | 204 |
| Domänentiteter | 65+ |
| Integrationsadapters | 16 |
| Tester | 486+ |
| Testprojekt | 26 |
| Git-commits (session) | 40 |

---

## 2. Teknikstack — 100% FOSS

| Komponent | Teknologi | Version | Licens |
|-----------|-----------|---------|--------|
| Runtime | .NET | 9.0 | MIT |
| Språk | C# | 13 | MIT |
| Web API | ASP.NET Core Minimal API | 9.0 | MIT |
| Frontend | Blazor Server SSR | 9.0 | MIT |
| UI-bibliotek | MudBlazor | 9.1.0 | MIT |
| Databas | PostgreSQL | 17 | PostgreSQL License |
| ORM | Entity Framework Core + Npgsql | 9.0.4 | MIT |
| Meddelandekö | RabbitMQ | 4 | MPL 2.0 |
| PDF-generering | PdfSharpCore | 1.3.65 | MIT |
| Excel-export | ClosedXML | 0.104.2 | MIT |
| E-post | MailKit | 4.9.0 | MIT |
| Realtid | SignalR | 9.0 | MIT |
| Observerbarhet | OpenTelemetry | 1.11-1.12 | Apache 2.0 |
| Testning | xUnit + bUnit | 2.9.3 / 2.0.66 | Apache 2.0 / MIT |
| Auth | Supabase Auth (self-hosted) | — | Apache 2.0 |
| Containerisering | Docker + docker-compose | — | Apache 2.0 |

---

## 3. Arkitektur

### 3.1 Modular Monolith
- 25 domänmoduler med schema-per-modul i PostgreSQL
- Inga databas-joins över modulgränser
- Moduler kommunicerar via C#-interfaces (Contracts) och domänhändelser
- Outbox Pattern för asynkron, pålitlig integration
- Domain-Driven Design (aggregat, value objects, domänhändelser)

### 3.2 Tre rollbaserade vyer
| Vy | Layout | Målgrupp | Princip |
|----|--------|----------|---------|
| **Anställd** | `EmployeeLayout.razor` | Alla 10 000+ anställda | 6 stora kort, ingen meny, "Stina-principen" |
| **Chef** | `ManagerLayout.razor` | Chefer/arbetsledare | Godkännandekö + kort, ingen sidebar |
| **HR/Admin** | `AdminLayout.razor` | HR, löneadmin, systemadmin | Sidebar med alla moduler |

### 3.3 Stina-principen
> "Stina, 62 år, utan datorerfarenhet, ska direkt kunna förstå och använda appen."

- Konversationsflöden istället för komplexa formulär (OhrConversationFlow)
- En fråga i taget med stora knappar
- Systemet är experten — hanterar lagkrav, beräkningar, regler automatiskt
- Aldrig facktermer — "Jag är sjuk" istället för "Sjukanmälan"
- Alltid förklaring + lösningsförslag vid problem (OhrSuggestionCard)

---

## 4. Implementerade sidor — komplett lista (88 st)

### 4.1 Anställd-vy — Min sida (8 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/minsida` | `MinSida/Index.razor` | 6-korts Stina-dashboard med OhrBigCard |
| `/minsida/schema` | `MinSida/MittSchema.razor` | Veckoschema med passtyper och timmar |
| `/minsida/ledighet` | `MinSida/MinLedighet.razor` | Semestersaldo (intjänade/uttagna/sparade/tillgängliga) |
| `/minsida/lon` | `MinSida/MinLon.razor` | Lönespecifikationer med PDF-nedladdning |
| `/minsida/sjukanmalan` | `MinSida/Sjukanmalan.razor` | 2-stegs konversationsflöde ("Jag är sjuk") |
| `/minsida/arenden` | `MinSida/MinaArenden.razor` | Pågående ärenden med statusbadges |
| `/minsida/profil` | `MinSida/MinProfil.razor` | Kontaktuppgifter + nödkontakter (redigerbart) |
| `/minsida/lonespecifikationer` | `MinSida/Lonespecifikationer.razor` | Äldre lönespec-vy (bakåtkompatibel) |

**Datakoppling:** SelfServiceApiClient med metoder för schema, ledighet, lön, ärenden, profil. Alla sidor kopplade till verklig data via DbContext med demo-fallback.

### 4.2 Chef-vy (4 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/chef` | `Chef/Index.razor` | Godkännandekö med en-knapps Godkänn/Neka + 4 översiktskort |
| `/chef/team` | `Chef/MittTeam.razor` | Teamöversikt med 12 anställda och statusindikator |
| `/chef/bemanning` | `Chef/Bemanning.razor` | Bemanningsstatus dag/kväll/natt med trafikljus |
| `/chef/franvarokalender` | `Chef/Franvarokalender.razor` | Frånvaroöversikt med sammanfattning |

**Datakoppling:** Godkänn/Neka anropar `LeaveRequest.Godkann()`/`Avvisa()` och `Timesheet.Godkann()`/`Avvisa()` domänmetoder direkt.

### 4.3 Personal (6 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/anstallda` | `Anstallda/Index.razor` | Anställdalista med MudBlazor-sökning + Excel-export |
| `/anstallda/{id}` | `Anstallda/Detalj.razor` | 7-flikars detaljvy (Person, Anställningar, Lön, Frånvaro, LAS, Kompetens, Dokument) |
| `/anstallda/{id}/redigera` | `Anstallda/Redigera.razor` | Redigeringsformulär för kontakt- och skatteuppgifter |
| `/anstallda/{id}/anstallning` | `Anstallda/AnstallningAndring.razor` | Konversationsflöde för intern förflyttning/löneändring/befattningsbyte |
| `/anstallda/{id}/lonehistorik` | `Anstallda/Lonehistorik.razor` | Komplett löneändringslogg |
| `/anstallda/ny` | `Anstallda/NyAnstalld.razor` | Steg-för-steg ny anställd |

**Datakoppling:** AnstallningService med HamtaAllaAsync, HamtaAsync, SkapaAsync, UppdateraKontaktuppgifterAsync, UppdateraSkatteuppgifterAsync.

### 4.4 Organisation (2 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/organisation` | `Organisation/Index.razor` | Organisationsträd med OrgNode-komponent |
| — | `Organisation/OrgNode.razor` | Rekursiv trädnod |

### 4.5 Lön (5 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/lon/korningar` | `Lon/Korningar.razor` | Lönekörningslista med status + "Ny körning" |
| `/lon/korning/{id}` | `Lon/KorningDetalj.razor` | Granskning med anomalidetektering + godkännandeknapp |
| `/lon/korning/ny` | `Lon/NyKorning.razor` | Konversationsflöde för ny lönekörning |
| `/lon/lonearter` | `Lon/Lonearter.razor` | Löneart-katalog (12 koder) |
| `/lon/statistik` | `Lon/Statistik.razor` | YTD-lönestatistik med månadsnedbrytning |

**Datakoppling:** Korningar och KorningDetalj laddar från PayrollRuns/PayrollResults via DbContext. Anomalidetektering beräknar >10% avvikelse från snittbrutto.

### 4.6 Schema & Tid (8 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/schema` | `Schema/Index.razor` | Schemaöversikt med KPI:er + veckorutnät per enhet |
| `/schema/ny` | `Schema/NyttSchema.razor` | 4-stegs konversationsflöde med AI-generering + OhrSuggestionCard |
| `/schema/bemanning` | `Schema/Bemanning.razor` | Bemanningsöversikt dag/kväll/natt per enhet |
| `/schema/atl` | `Schema/ATL.razor` | ATL-efterlevnad med marginalvarningar |
| `/schema/passbyte` | `Schema/Passbyte.razor` | Passbytesbegäran med godkännande |
| `/stampling` | `Stampling/Index.razor` | Instämplingsstatus med sena/saknade |
| `/tidrapporter` | `Tidrapporter/Index.razor` | Tidrapportlista med granskningsstatus |
| `/tidrapporter/{id}` | `Tidrapporter/Detail.razor` | Tidrapportdetalj med veckonedbrytning + Godkänn/Avvisa |

### 4.7 Ärenden & Ledighet (10 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/arenden` | `Arenden/Index.razor` | Ärendelista med KPI-kort |
| `/arenden/{id}` | `Arenden/Detalj.razor` | Ärendedetalj med tidslinje |
| `/arenden/nytt` | `Arenden/NyttArende.razor` | Nytt ärende |
| `/arenden/mbl` | `Arenden/MBL.razor` | MBL-konsultationsspårning (§11, §12, §19, §38) |
| `/ledighet` | `Ledighet/Index.razor` | Ledighetsöversikt med inline godkänn/neka |
| `/ledighet/ny` | `Ledighet/NyAnsoken.razor` | Konversationsflöde: typ → period → klart |
| `/ledighet/saldon` | `Ledighet/Saldon.razor` | Semestersaldo per anställd |
| `/ledighet/kalender` | `Ledighet/Kalender.razor` | Teamkalender med emoji-indikatorer |
| `/ledighet/foraldraledighet` | `Ledighet/Foraldraledighet.razor` | 480-dagarsystem med FK-koordination |
| `/ledighet/vab` | `Ledighet/VAB.razor` | VAB-spårning med FK-rapporteringsstatus |

**Datakoppling:** Ledighet/Index och Saldon laddar från LeaveRequests/VacationBalances. Godkänn/Neka anropar domänmetoder.

### 4.8 Uppföljning (5 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/las` | `LAS/Index.razor` | LAS-dashboard med kritisk/varning/under gräns + företrädesrätt |
| `/halsosam` | `HalsoSAM/Index.razor` | Rehabiliteringsärenden + sjukfrånvaro per enhet |
| `/halsosam/ny` | `HalsoSAM/NyttArende.razor` | Konversationsflöde: anställd → orsak → klart |
| `/medarbetarsamtal` | `Medarbetarsamtal/Index.razor` | Medarbetarsamtalslista |
| `/medarbetarsamtal/ny` | `Medarbetarsamtal/NyttSamtal.razor` | 5-stegs konversation: anställd → självbedömning (5-stjärnig) → chefsbedömning → mål → klart |

### 4.9 Medarbetarsamtal — Extra (1 sida)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/medarbetarsamtal/360` | `Medarbetarsamtal/Feedback360.razor` | 360-graders feedback med multi-rater |

### 4.10 Rekrytering (5 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/rekrytering/vakanser` | `Rekrytering/Vakanser.razor` | Vakansöversikt med KPI:er |
| `/rekrytering/onboarding` | `Rekrytering/Onboarding.razor` | Onboarding-checklistor med progressbar |
| `/rekrytering/referenskontroll` | `Rekrytering/Referenskontroll.razor` | Referenskontrollhantering |
| `/rekrytering/statistik` | `Rekrytering/Statistik.razor` | Rekryteringsanalys (time-to-hire, cost-per-hire) |
| `/integrationer/platsbanken` | `Integrationer/Platsbanken.razor` | Platsbanken-integrationsstatus |

### 4.11 Dokument (4 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/dokument` | `Dokument/Index.razor` | Dokumentöversikt med GDPR-klassificering |
| `/dokument/ny` | `Dokument/Upload.razor` | 4-stegs uppladdningsflöde med MudFileUpload |
| `/dokument/organisation` | `Dokument/Organisationsdokument.razor` | Kollektivavtal, lokala avtal, delegationsordning |
| `/dokument/policyer` | `Dokument/Policyer.razor` | Policydistribution med kvitteringsspårning |

### 4.12 Kompetens & Utbildning (3 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/kompetens` | `Kompetens/Index.razor` | Certifieringsregister med utgångsvarningar (OhrSuggestionCard) + obligatoriska utbildningar per enhet |
| `/utbildning` | `Utbildning/Index.razor` | Kurskatalog med anmälningsstatistik |
| `/utbildning/elearning` | `Utbildning/Elearning.razor` | E-learning-integrationshub |

### 4.13 Förmåner (3 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/formaner` | `Formaner/Index.razor` | Förmånsöversikt med anmälningar |
| `/formaner/friskvard` | `Formaner/Friskvard.razor` | Friskvårdsbidrag 5 000 kr/år per anställd |
| `/formaner/forsakringar` | `Formaner/Forsakringar.razor` | Försäkringsöversikt (TGL, AGS, AFA) |

### 4.14 Övriga HR-moduler (4 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/loneoversyn` | `Loneoversyn/Index.razor` | Löneöversynsrundor |
| `/resor` | `Resor/Index.razor` | Resor och utlägg med godkännande |
| `/offboarding` | `Offboarding/Index.razor` | Offboarding-ärenden |
| `/offboarding/ny` | `Offboarding/Workflow.razor` | 5-stegs offboarding: anställd → slutlön → tillgångar → åtkomst → klart |

### 4.15 Rapporter & Analys (5 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/rapporter` | `Rapporter/Index.razor` | Rapportbibliotek med en-klicks-körning |
| `/rapporter/kor/{namn}` | `Rapporter/KorRapport.razor` | Rapportmotor: generera, förhandsgranska, exportera CSV |
| `/rapporter/scb` | `Rapporter/SCBExport.razor` | SCB/SKR-statistikexport |
| `/rapporter/analytics` | `Rapporter/WorkforceAnalytics.razor` | Workforce analytics: pensionsvåg, omsättningsmönster |
| `/rapporter/kostnadssimulering` | `Rapporter/Kostnadssimulering.razor` | Kostnadsmodellering |

### 4.16 System & Administration (10 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/` | `Home.razor` | Admin-dashboard med 6 KPI:er + händelseflöde + bemanning per enhet |
| `/audit` | `Audit/Index.razor` | Granskningslogg kopplad till AuditEntries |
| `/gdpr` | `GDPR/Index.razor` | GDPR-dashboard med DSR-spårning + registerutdragsgenerering |
| `/integrationer` | `Integrationer/Index.razor` | 16 integrationsadapters med statusövervakning |
| `/admin/konfiguration` | `Admin/Konfiguration.razor` | Systemkonfiguration, workflows, custom fields |
| `/admin/delegering` | `Admin/Delegering.razor` | Godkännandedelegering |
| `/admin/succession` | `Admin/Successionsplanering.razor` | Successionsplanering |
| `/admin/anslagstavla` | `Admin/Anslagstavla.razor` | Anslagstavla/nyhetsfeed |
| `/admin/import` | `Admin/BulkImport.razor` | Massimport CSV/Excel |
| `/positioner` | `Positioner/Index.razor` | Positionsöversikt (tillsatt/vakant/fryst) |

### 4.17 Notiser (2 sidor)

| Route | Fil | Beskrivning |
|-------|-----|-------------|
| `/notiser` | `Notiser/Index.razor` | Notislista med markera-alla-som-lästa |
| `/notiser/installningar` | `Notiser/Installningar.razor` | Notisinställningar per kanal (in-app/e-post/SMS) |

---

## 5. Komponenter & Infrastruktur

### 5.1 OpenHR-komponenter (Stina-principen)

| Komponent | Fil | Syfte |
|-----------|-----|-------|
| `OhrBigCard` | `Shared/OhrBigCard.razor` | Stor klickbar kort med ikon, titel, hjälptext, badge |
| `OhrConversationFlow` | `Shared/OhrConversationFlow.razor` | Steg-för-steg-wizard med progressbar och bakåt-knapp |
| `OhrSuggestionCard` | `Shared/OhrSuggestionCard.razor` | Intelligent förslag med åtgärdsknappar |

### 5.2 Layouts

| Layout | Fil | Målgrupp |
|--------|-----|----------|
| `EmployeeLayout` | `Layout/EmployeeLayout.razor` | Anställda — ingen sidebar, centrerat innehåll |
| `ManagerLayout` | `Layout/ManagerLayout.razor` | Chefer — ingen sidebar, brett innehåll |
| `AdminLayout` | `Layout/AdminLayout.razor` | HR/Admin — MudDrawer sidebar med modulnavigation |
| `TopBar` | `Layout/Shared/TopBar.razor` | Delad topbar: sök, notiser, mörkt läge, språk, användarmeny |

### 5.3 Blazor-services

| Service | Fil | Ansvar |
|---------|-----|--------|
| `AnstallningService` | `Services/AnstallningService.cs` | Anställd CRUD + organisation |
| `ArendeService` | `Services/ArendeService.cs` | Ärendehantering |
| `SelfServiceApiClient` | `Services/SelfServiceApiClient.cs` | Aggregerad data för Min sida (schema, ledighet, lön, ärenden, profil) |
| `UserRoleService` | `Services/UserRoleService.cs` | Rolldetektering för layoutval |

### 5.4 Infrastruktur-services (nytt)

| Service | Fil | Ansvar |
|---------|-----|--------|
| `EmailNotificationSender` | `Infrastructure/Notifications/EmailNotificationSender.cs` | HTML-mallbaserad e-post via MailKit (godkännanden, påminnelser, sjuklön) |
| `UnitAccessScopeService` | `Infrastructure/Authorization/UnitAccessScopeService.cs` | Enhetsbaserad åtkomstkontroll (chef → sin enhet, HR → allt) |
| `NotificationHub` | `Web/Hubs/NotificationHub.cs` | SignalR-hub för realtidsnotiser |

### 5.5 CSS & tillgänglighet

| Fil | Syfte |
|-----|-------|
| `wwwroot/css/accessibility.css` | WCAG 2.1 AA: touch targets 44px, fokusindikatorer, reducerad rörelse, kontrast |
| `wwwroot/css/print.css` | Utskriftsvänlig CSS: döljer navigation, tar bort skuggor |
| `wwwroot/css/app.css` | App-överstyrningar |
| `wwwroot/js/download.js` | JS-helper för PDF/Excel-nedladdning i Blazor Server |

### 5.6 Internationalisering (i18n)

| Fil | Språk | Nycklar |
|-----|-------|---------|
| `Resources/SharedResources.sv.resx` | Svenska (default) | 19 Stina-princip-etiketter |
| `Resources/SharedResources.en.resx` | Engelska | 19 matchande nycklar |
| `Localization/SharedResources.cs` | — | Markörsklass för ASP.NET Core Localization |

---

## 6. Lösta gap-analyser

### 6.1 Alla 29 kritiska luckor — LÖSTA

| # | Lucka | Lösning | Fil(er) |
|---|-------|---------|---------|
| 1 | Redigera anställd i UI | Redigeringsformulär | `Anstallda/Redigera.razor` |
| 2 | Komplett detaljvy | 7-flikars MudTabs | `Anstallda/Detalj.razor` |
| 3 | Offboarding-workflow | 5-stegs konversationsflöde | `Offboarding/Workflow.razor` |
| 4 | Självservice riktig data | SelfServiceApiClient | `Services/SelfServiceApiClient.cs` |
| 5 | Uppdatera kontaktuppgifter | MinProfil med save | `MinSida/MinProfil.razor` |
| 6 | Sjukanmälan | 2-stegs konversation | `MinSida/Sjukanmalan.razor` |
| 7 | Chefsportal riktig data | DB-kopplad godkännandekö | `Chef/Index.razor` |
| 8 | Godkänn/neka workflow | Domänmetoder via DbContext | `Chef/Index.razor` |
| 9 | Rollbaserade vyer | 3 layouts + UserRoleService | `EmployeeLayout` + `ManagerLayout` + `AdminLayout` |
| 10 | Semestersaldomotor | VacationBalance kopplat | `Ledighet/Saldon.razor` |
| 11 | Ledighetsdashboard | Saldo synligt på kort | `MinSida/MinLedighet.razor` |
| 12 | Medarbetarsamtal | 5-stegs konversation med ratings | `Medarbetarsamtal/NyttSamtal.razor` |
| 13 | Obligatorisk utbildning | Expiry alerts + OhrSuggestionCard | `Kompetens/Index.razor` |
| 14 | Lönegranskningsdashboard | Anomalidetektering | `Lon/KorningDetalj.razor` |
| 15 | Lönespec PDF | PdfSharpCore download | `MinSida/MinLon.razor` + `download.js` |
| 16 | Tidrapportering | Detalj + Godkänn/Avvisa | `Tidrapporter/Detail.razor` |
| 17 | Executive dashboard | KPI:er från DB | `Home.razor` |
| 18 | Standardrapporter | Rapportmotor med preview | `Rapporter/KorRapport.razor` |
| 19 | Granskningslogg | AuditEntries kopplat | `Audit/Index.razor` |
| 20 | GDPR registerutdrag | DSR + generering | `GDPR/Index.razor` |
| 21 | Dokumentlagring | MudFileUpload-flöde | `Dokument/Upload.razor` |
| 22 | Automatiserade triggers | NotificationReminderService | `BackgroundJobs/NotificationReminderService.cs` |
| 23 | E-postnotiser | MailKit HTML-mallar | `Notifications/EmailNotificationSender.cs` |
| 24 | In-app-notiser | SignalR hub + notissida | `Hubs/NotificationHub.cs` + `Notiser/Index.razor` |
| 25 | Automatiska påminnelser | Bakgrundstjänster | `NotificationReminderService.cs` |
| 26 | Enhetsbaserad access | UnitAccessScopeService | `Authorization/UnitAccessScopeService.cs` |
| 27 | Sökfunktion | MudAutocomplete i TopBar | `Layout/Shared/TopBar.razor` |
| 28 | WCAG 2.1 AA | Accessibility CSS + aria | `wwwroot/css/accessibility.css` |
| 29 | Dataexport | Excel via ClosedXML | `Anstallda/Index.razor` + `ExportService` |

### 6.2 Alla 31 viktiga luckor — LÖSTA

- Anställningsändringar (intern förflyttning, löneändring, befattningsbyte)
- Lönehistorik per anställd
- Passbytesbegäran och godkännande
- Föräldraledighet med 480-dagarsystem
- VAB-spårning med FK-rapportering
- Friskvårdsbidrag (5 000 kr/år)
- MBL-konsultationsspårning
- Godkännandedelegering
- SCB/SKR-statistikexport
- Platsbanken-integration
- Onboarding-portal med checklistor
- Massimport CSV/Excel
- Dokumentmallar och organisationsdokument
- Policydistribution
- Och mer

### 6.3 Alla 18 nice-to-have — LÖSTA

- 360-graders feedback
- Successionsplanering
- E-learning-integration
- Workforce analytics
- Kostnadssimulering
- Notisinställningar per kanal
- Anslagstavla
- Mörkt läge (dark mode toggle)
- Utskriftsvänlig CSS
- Referenskontroll
- Rekryteringsstatistik
- Försäkringsöversikt
- Organisationsdokument
- Policydistribution

---

## 7. Konversationsflöden (OhrConversationFlow)

Dessa åtgärder använder steg-för-steg-wizard istället för traditionella formulär:

| Flöde | Steg | Fil |
|-------|------|-----|
| Sjukanmälan | Dag → Tillbaka? → Klart | `Sjukanmalan.razor` |
| Ledighetsansökan | Typ → Period → Klart | `Ledighet/NyAnsoken.razor` |
| Ny lönekörning | Period → Bekräfta → Beräknar | `Lon/NyKorning.razor` |
| Nytt schema | Period → Enhet → Genererar → Resultat + förslag | `Schema/NyttSchema.razor` |
| Nytt rehabärende | Anställd → Orsak → Klart | `HalsoSAM/NyttArende.razor` |
| Medarbetarsamtal | Anställd → Självbedömning → Chefsbedömning → Mål → Klart | `Medarbetarsamtal/NyttSamtal.razor` |
| Offboarding | Anställd → Slutlön → Tillgångar → Åtkomst → Klart | `Offboarding/Workflow.razor` |
| Anställningsändring | Typ → Detaljer → Klart | `Anstallda/AnstallningAndring.razor` |
| Dokumentuppladdning | Typ → Anställd → Fil → Klart | `Dokument/Upload.razor` |

---

## 8. Integrationer (16 adapters)

| System | Riktning | Syfte |
|--------|----------|-------|
| Skatteverket (AGI XML) | Ut | Arbetsgivardeklaration |
| Nordea (ISO 20022 pain.001) | Ut | Löneutbetalning |
| Försäkringskassan | Båda | Sjukpenning, föräldrapenning |
| Kronofogden | Båda | Löneutmätning |
| Skandia (pension) | Ut | AKAP-KR pensionsrapport |
| SKR (statistik) | Ut | KPR lönestatistik |
| SCB/KLR | Ut | Konjunkturstatistik |
| Raindance | Ut | Ekonomisystem |
| KOLL/HOSP | In | Legitimationsverifiering |
| Epassi | Ut | Friskvårdsförmåner |
| Troman | Båda | Tidregistrering |
| PowerBI | Ut | Analys |
| Grade (LMS) | Båda | Utbildningsplattform |
| Min kompetens | Båda | Kompetensregister |
| Diver | Ut | Statistik |
| Microweb (arkiv) | Ut | Dokumentarkivering |

---

## 9. Säkerhet & Compliance

| Mekanism | Implementation |
|----------|---------------|
| Autentisering | JWT Bearer (Supabase Auth / Azure AD) |
| Auktorisering | 7 roller (Anställd, Chef, HR-admin, HR-specialist, Löneadmin, Systemadmin, Facklig) |
| Enhetsbaserad åtkomst | UnitAccessScopeService (chef → sin enhet, HR → allt) |
| Datakryptering | pgcrypto (personnummer, bankuppgifter) |
| Row-Level Security | PostgreSQL RLS-policyer per modul |
| Granskningslogg | AuditEntries med gamla/nya värden |
| GDPR | Registerutdrag, anonymisering, retention |
| WCAG 2.1 AA | Accessibility CSS, aria-labels, fokushantering, touch targets 44px |
| Hälsodata | Striktare RLS (HälsoSAM: ärendeägare + HR) |

---

## 10. Bygga och köra

```bash
# Bygga
dotnet build RegionHR.sln

# Testa
dotnet test RegionHR.sln

# Köra API
dotnet run --project src/Api/RegionHR.Api.csproj

# Köra Webb (Blazor)
dotnet run --project src/Web/RegionHR.Web.csproj

# Docker
docker-compose up -d   # PostgreSQL 17 + RabbitMQ 4
```

---

## 11. Licensiering

**OpenHR** är licensierad under **AGPL-3.0** (GNU Affero General Public License v3).

Detta innebär:
- Fri att använda, kopiera, modifiera och distribuera
- Alla forks och modifikationer MÅSTE publiceras som öppen källkod
- Gäller även vid nätverksanvändning (SaaS-deployments)
- Ingen kan göra en proprietär/sluten fork

---

*Dokumentet genererat 2026-03-17. Alla 88 sidor, 29 kritiska + 31 viktiga + 18 nice-to-have luckor implementerade.*
