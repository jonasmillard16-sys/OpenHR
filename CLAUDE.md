# OpenHR — Komplett HR-system för svensk region

## Projektbeskrivning
Modular monolith HR-system (AGPL-3.0) som ersätter HEROMA. 25 moduler, 482 tester, 94 Blazor-sidor, 93 rutter, 61 infrastrukturfiler. Hanterar hela HR-livscykeln: personalregister, lön/payroll (svensk skattelagstiftning + kollektivavtal AB/HOK), schemaplanering (24/7 sjukvård), självservice, ärendehantering, LAS-uppföljning med konverteringsflöde, rehabilitering (HälsoSAM), löneöversyn, resor/utlägg, rekrytering med pipeline, ledighetshantering, dokumenthantering med mallgenerering, medarbetarsamtal, kompetensregister med gap-analys, rapportering, GDPR-efterlevnad, granskningslogg, notifieringar (InApp + Email + SMS), pulsundersökningar, peer recognition, offboarding, onboarding, förmåner, utbildning/e-learning, MBL-förhandlingar, samt integrationer mot ~15 externa system.

## Bygga och köra
```bash
dotnet build RegionHR.sln          # 0 errors, 0 warnings
dotnet test RegionHR.sln           # 482 tester, 0 failures
dotnet run --project src/Web/RegionHR.Web.csproj  # http://localhost:5076
```

## Arkitektur
- **Modular Monolith** med schema-per-modul i PostgreSQL
- **Frontend**: Blazor Server SSR med MudBlazor 9.1 (global InteractiveServer via Routes.razor)
- **Tema**: Nordic Refined — Plus Jakarta Sans, teal primary, dark mode via `.ohr-dark`/`.ohr-light` wrapper
- Moduler kommunicerar via publika C# interfaces (Contracts) och domänhändelser
- Aldrig databas-joins över modulgränser
- Export: CSV/Excel (ClosedXML), PDF (QuestPDF)
- Notifieringar: InApp + Email (MailKit) + SMS (HTTP gateway)
- Bakgrundsjobb: NotificationReminderService, RetentionCleanupService, ScheduledReportService
- All teknologi är 100% FOSS/open source, inga kommersiella beroenden

## Modulstruktur (25 moduler)

### Kärnmoduler
- `src/SharedKernel/` — Domänprimitiver (Personnummer, Money, DateRange, EmployeeId)
- `src/Modules/Core/` — Personalregister, organisation, anställningar
- `src/Modules/Payroll/` — Löneberäkning, skatt, kollektivavtal, OB-tillägg
- `src/Modules/Scheduling/` — Schema, instämpling, ATL-efterlevnad, passbyte
- `src/Modules/CaseManagement/` — Ärenden, workflows, frånvaro, MBL-förhandlingar
- `src/Modules/LAS/` — LAS-ackumulering, konvertering till tillsvidare, företrädesrätt
- `src/Modules/HalsoSAM/` — Rehabilitering, sjukfrånvarobevakning, FK-anmälan
- `src/Modules/SalaryReview/` — Löneöversynsrundor, lönekartering (diskrimineringslagen)
- `src/Modules/Travel/` — Resor, utlägg, traktamente, resekrav
- `src/Modules/Recruitment/` — Vakanser, ansökningar, pipeline, onboarding, referenskontroll
- `src/Modules/IntegrationHub/` — AGI-XML, Nordea pain.001, outbox pattern
- `src/Modules/SelfService/` — Självserviceportal (Min sida)

### Expansionsmoduler
- `src/Modules/Audit/` — Granskningslogg (alla ändringar loggas)
- `src/Modules/Notifications/` — Notiseringar (InApp, Email, SMS)
- `src/Modules/Leave/` — Ledighetshantering (semester, sjukfrånvaro, VAB, föräldraledighet)
- `src/Modules/Documents/` — Dokumenthantering med retention, GDPR-klassificering, mallgenerering
- `src/Modules/Performance/` — Medarbetarsamtal, 360-feedback
- `src/Modules/Reporting/` — Rapportering (standard + schemalagda + lönekartering + analytics)
- `src/Modules/GDPR/` — GDPR-efterlevnad (registerutdrag, anonymisering, retention)
- `src/Modules/Competence/` — Kompetens/certifieringar, gap-analys per roll
- `src/Modules/Benefits/` — Förmåner (friskvårdsbidrag, försäkringar)
- `src/Modules/Offboarding/` — Offboarding-workflow (AD, passerkort, utrustning, slutlön)
- `src/Modules/LMS/` — Utbildning, e-learning
- `src/Modules/Analytics/` — Workforce analytics, kostnadssimulering, SCB-rapporter
- `src/Modules/Positions/` — Positionsregister, successionsplanering

### Presentation
- `src/Api/` — ASP.NET Core Web API
- `src/Web/` — Blazor Server frontend (94 sidor, MudBlazor 9.1)
- `src/DesignSystem/` — Blazor komponentbibliotek (OhrConversationFlow, OhrSuggestionCard, OhrEmptyState, OhrUndoSnackbar)
- `src/Infrastructure/` — EF Core, repositories, export, SMS, schemalagda rapporter

## Frontend (Blazor Web, 94 sidor)

### Layout & Tema
- `AdminLayout.razor` — Huvudlayout med sidebar, topbar, dark mode, MudTheme
- `NavMenu.razor` — Sidmeny med expanderbara grupper (Personal, Lön, Schema och Tid)
- `TopBar.razor` — AppBar med sökning, dark mode, språkväxling (sv/en), notiser, användarmeny
- `openhr-theme.css` — Nordic Refined tema med light/dark mode via `.ohr-dark`/`.ohr-light`

### Sidor per område
| Område | Sidor | Rutter |
|--------|-------|--------|
| Dashboard | 1 | `/` |
| Personal | 6 | `/anstallda`, `/anstallda/ny`, `/anstallda/{id}`, redigera, lönehistorik, anställning |
| Organisation | 2 | `/organisation`, `/positioner` |
| Lön | 5 | `/lon/korningar`, `/lon/korning/ny`, `/lon/lonearter`, `/lon/statistik`, `/loneoversyn` |
| Schema & Tid | 5 | `/schema`, `/schema/ny`, `/schema/bemanning`, `/stampling`, `/tidrapporter` |
| Ärenden | 3 | `/arenden`, `/arenden/nytt`, `/arenden/mbl` |
| Ledighet | 5 | `/ledighet`, `/ledighet/ny`, `/ledighet/kalender`, `/ledighet/vab`, `/ledighet/foraldraledighet` |
| HälsoSAM | 2 | `/halsosam`, `/halsosam/ny` |
| LAS | 1 | `/las` (med konverteringsflöde) |
| Dokument | 4 | `/dokument`, `/dokument/ny`, `/dokument/organisation`, `/dokument/policyer`, `/dokument/mall/generera` |
| Medarbetarsamtal | 3 | `/medarbetarsamtal`, `/medarbetarsamtal/ny`, `/medarbetarsamtal/360` |
| Rekrytering | 6 | `/rekrytering/vakanser`, `/rekrytering/pipeline`, `/rekrytering/intern`, `/rekrytering/onboarding`, `/rekrytering/referenskontroll`, `/rekrytering/statistik` |
| Rapporter | 5 | `/rapporter`, `/rapporter/lonekartering`, `/rapporter/analytics`, `/rapporter/kostnadssimulering`, `/rapporter/scb` |
| Min sida | 7 | `/minsida`, `/minsida/sjukanmalan`, `/minsida/lon`, `/minsida/schema`, `/minsida/ledighet`, `/minsida/profil`, `/minsida/arenden` |
| Chef | 4 | `/chef`, `/chef/team`, `/chef/bemanning`, `/chef/franvarokalender` |
| Admin | 7 | `/admin/konfiguration`, `/admin/import`, `/admin/berom`, `/admin/pulsundersokning`, `/admin/anslagstavla`, `/admin/delegering`, `/admin/succession` |
| Övrigt | 10 | `/gdpr`, `/audit`, `/notiser`, `/notiser/installningar`, `/offboarding`, `/offboarding/ny`, `/formaner`, `/formaner/friskvard`, `/utbildning`, `/utbildning/elearning` |

### Interaktiva komponenter
- **OhrAssistant** — AI-chatbot (regelbaserad) med personliga svar, åtgärdsknappar, navigering
- **OhrConversationFlow** — Steg-för-steg-wizard med tillbaka-knapp
- **OhrSuggestionCard** — Åtgärdsförslag med förklaringar och knappar
- **OhrEmptyState** — Tom-tillståndsvy med illustration och CTA
- **OhrUndoSnackbar** — 10-sekunders ångra-snackbar

### Tema & Dark Mode
- Light: Varm beige bakgrund (#f6f5f2), teal primary (#0e7490), Plus Jakarta Sans
- Dark: Midnattssvart (#0f1419), himmelsblå accent (#38bdf8), navy kort (#1a2332)
- Styrs via `.ohr-dark`/`.ohr-light` klass på wrapper i AdminLayout
- MudThemeProvider med IsDarkMode + PaletteDark/PaletteLight

## Konventioner
- Alla monetära värden använder `Money` (decimal-baserad, SEK)
- Personnummer hanteras via `Personnummer` value object med Luhn-validering
- Svenskt språk i domänmodell (metoder, egenskaper), engelskt för infrastruktur
- Varje modul exponerar ett publikt `Contracts/` interface för användning av andra moduler
- Nya moduler följer samma mönster: Domain/ + Contracts/ + Services/ + tester
- UI-text på svenska, multi-language förberett via IStringLocalizer + .resx-filer
- Alla formulär adderar till synlig lista med bekräftelse-alert (ingen "under utveckling" kvar)
- Workflows visar detaljerade resultatpaneler (ärendenummer, notifieringar, nästa steg)
- Copyleft-licens: AGPL-3.0 (forks måste hålla koden öppen)
