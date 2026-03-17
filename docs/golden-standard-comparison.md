# OpenHR vs Branschstandard — Gap-analys och förbättringsplan

**Datum:** 2026-03-17
**Jämfört med:** HEROMA (CGI), Workday HCM, SAP SuccessFactors, Oracle Fusion HCM
**Princip:** 100% FOSS, systemet är experten (Stina-principen)

---

## 1. HEROMA (CGI) — Vad de har som vi måste matcha eller slå

| HEROMA-funktion | OpenHR status | Gap |
|-----------------|---------------|-----|
| **HR Master** — central portal för alla moduler | Admin-dashboard med KPI:er finns | Finns, men saknar aggregerad vy som sammanfattar alla moduler |
| **Rekrytering** — annons till anställning, mallar, kravprofiler, anonymisering | Vakanser, ansökningar, scorecards finns | Saknar: anonymiserad rekrytering, kravprofiler, annonsmallar |
| **LAS-bevakning** — automatisk tidsberäkning | Komplett med ackumulering, alarmer, konvertering | OpenHR är BÄTTRE — har intelligent förslag vid konvertering |
| **Kompetenshantering** — gap-analys per roll, utbildningsrekommendationer | Certifieringsregister + expiry alerts | Saknar: kompetens-gap-analys per roll, AI-rekommendationer |
| **Mål och utvecklingssamtal** | Medarbetarsamtal med 5-stjärnig rating | Saknar: koppling till kompetensprofil, utvecklingsplan |
| **Löneöversyn** — förhandling till kommunikation | Rundor med förslag finns | Saknar: förhandlingsfas, facklig samverkansspårning |
| **Schemaläggning** — grundschema till 24/7 med AI | Schema med AI-optimering (OR-Tools) | OpenHR BÄTTRE — har intelligent assistans med lösningsförslag |
| **AI Schema** — avvikelsedetektering, riskidentifiering | ATL-efterlevnad + anomalidetektering i lönekörning | Saknar: dedikerad AI-avvikelseanalys i schema |
| **Självservice** — schema, frånvaro, e-signering, mobil | 6-korts dashboard + alla undersidor | Saknar: e-signering (BankID-integration) |
| **Mobilapp** (Android + iOS) | PWA-redo men inte testat/optimerat | Saknar: dedikerad mobiloptimering, offline-läge |
| **Reseräkning** — automatisk löneintegration | Resekrav med godkännandeflöde | Saknar: automatisk integration med lönekörning |
| **Nämndadministration** — förtroendevalda, arvoden | Finns inte | **NYA:** Nämnd/styrelseadministration |
| **SMS-utskick** — vakansnotiser | Notiser via SignalR + MailKit | Saknar: SMS-kanal |
| **E-signering** — anställningsavtal digitalt | Finns inte | **NY:** E-signeringsintegration |
| **DOS-lagen + WCAG 2.1 AA** | Accessibility CSS + aria | Behöver: formell audit och åtgärdsplan |
| **Standardiserade API:er och integrationer** | 16 adapters + outbox pattern | OpenHR BÄTTRE — fler integrationer |

---

## 2. Branschledare (Workday, SAP, Oracle) — Funktioner som OpenHR bör ha

### 2.1 AI & Intelligens (KRITISKT för 2026)

| Funktion | Branschstandard | OpenHR | Åtgärd |
|----------|----------------|--------|--------|
| **Skills Intelligence** — AI identifierar kompetenser automatiskt | Workday Skills Cloud | Manuellt kompetensregister | **NY:** AI-driven kompetensanalys som analyserar anställdas erfarenheter och föreslår kompetenser |
| **Prediktiv attrition** — förutser uppsägningar | Workday, Oracle AI | Finns inte | **NY:** Omsättningsrisk-modell baserad på frånvaromönster, anställningstid, lönenivå |
| **AI Recruiting** — CV-parsing, kandidatrankning | SAP SmartRecruiters | Manuellt scorecards | **NY:** Automatisk CV-screening med kompetensmatching |
| **AI HR-assistent/chatbot** — svarar på policyfrågor 24/7 | Workday Assistant | Finns inte | **NY:** Inbyggd chatbot som svarar på "Hur många semesterdagar har jag?" etc. |
| **Prediktiv bemanning** — prognostiserar behov | Workday Adaptive Planning | Bemanningsöversikt (nuläge) | **NY:** Bemanningsprognos baserad på historik, säsongsvariation, sjukfrånvarotrender |
| **Anomalidetektering** — automatiskt i alla moduler | Oracle AI | Bara i lönekörning | **FÖRBÄTTRA:** Utöka till schema, frånvaro, kompetens |
| **Lönekartering** — pay equity AI-analys | Workday Pay Equity | Finns inte | **NY:** Automatisk lönekartläggning per Diskrimineringslagen (var 3:e år) |

### 2.2 Employee Experience (EX)

| Funktion | Branschstandard | OpenHR | Åtgärd |
|----------|----------------|--------|--------|
| **Personaliserad startsida** — baserat på roll, aktivitet | Workday Home | Samma vy för alla i samma roll | **FÖRBÄTTRA:** Personalisera kort baserat på användarens faktiska aktivitet |
| **Nudges/påminnelser** — proaktiva tips | SAP SuccessFactors | Bakgrundsjobb-triggers | **FÖRBÄTTRA:** Kontextuella tips på startsidan ("Du har 5 dagar semester kvar att planera") |
| **Peer recognition** — kollegor uppmärksammar varandra | Workday Peakon | Finns inte | **NY:** "Ge beröm"-funktion — synlig på teamöversikten |
| **Välbefinnande/wellness-tracker** | Workday Peakon surveys | Friskvårdsbidrag finns | **NY:** Pulsundersökningar (anonyma) för medarbetarnöjdhet |
| **Onboarding-upplevelse** — personlig välkomstportal | Workday, SAP | Onboarding-checklista | **FÖRBÄTTRA:** Dedikerad nyanställd-vy med steg-för-steg-guide |
| **Intern jobbmarknad** — interna lediga tjänster | Workday Talent Marketplace | Vakanser (externt fokuserade) | **NY:** Intern jobbanslagstavla med enkel "intresseanmälan" |

### 2.3 Workforce Planning & Analytics

| Funktion | Branschstandard | OpenHR | Åtgärd |
|----------|----------------|--------|--------|
| **Scenario-modellering** — "vad händer om" | Workday Adaptive | Kostnadssimulering (enkel) | **FÖRBÄTTRA:** Utöka med personal-scenarios (vad om 10% slutar?) |
| **Realtids-dashboards med drill-down** | Alla tre | KPI-kort utan drill-down | **FÖRBÄTTRA:** Klickbara KPI-kort som visar detaljdata |
| **Headcount-budgetering** — kopplat till ekonomi | SAP, Workday | Headcount-planering finns | **FÖRBÄTTRA:** Koppling till lönekostnadsberäkning |
| **Turnover-analys** — varför slutar folk? | Oracle AI | Finns inte som dedikerad vy | **NY:** Omsättningsdashboard med orsaksanalys (exit-samtal-data) |
| **Diversity & Inclusion-analytics** | Workday, SAP | Finns inte | **NY:** Mångfaldsstatistik — kön, ålder, etnisk bakgrund per enhet |
| **Benchmark-jämförelse** — mot bransch/region | Workday Benchmarking | Finns inte | **NY:** Jämför sjukfrånvaro, omsättning, lönespridning mot SKR-snitt |

### 2.4 Avancerad funktionalitet

| Funktion | Branschstandard | OpenHR | Åtgärd |
|----------|----------------|--------|--------|
| **Org chart — interaktiv** | Alla | Trädvy finns | **FÖRBÄTTRA:** Visuellt organisationsschema med foton, drag-and-drop |
| **Dokumentmallar med merge fields** | Alla | Dokumentuppladdning finns | **NY:** Mallmotor för anställningsavtal, intyg, varningsbrev med auto-ifyllning |
| **E-signering** | SAP (DocuSign) | Finns inte | **NY:** Integration med öppen e-signeringstjänst eller BankID |
| **Multi-tenant/multi-org** | SAP, Oracle | Konfigurationssida finns | **FÖRBÄTTRA:** Verklig multi-tenant-support (flera regioner i samma instans) |
| **Workflow engine — visuell** | Alla | Workflow-definitioner som JSON | **FÖRBÄTTRA:** Visuell workflow-editor (drag-and-drop) |
| **Audit trail med förklaring** | Alla | Audit-logg finns | **FÖRBÄTTRA:** Länka audit-poster till affärsbeslut, inte bara tekniska ändringar |

---

## 3. Förbättringar av befintliga OpenHR-funktioner

### 3.1 Funktioner som finns men behöver djup

| Funktion | Nuläge | Förbättring |
|----------|--------|-------------|
| **Sjukanmälan** | 2-stegs konversation | Lägg till: automatisk karensavdragsberäkning i bekräftelsen, visa "Din sjuklön dag 2-14 blir X kr" |
| **Lönekörning** | Visar anomalier | Lägg till: jämförelse med föregående månads totalsummor, avvikelsetrender |
| **Schemaoptimering** | Konversationsflöde med förslag | Koppla till Google OR-Tools backend, visa faktisk optimeringsresultat |
| **Dashboard** | KPI-kort med siffror | Lägg till: sparkline-trender (mini-grafer), klickbar drill-down |
| **Rapporter** | Rapport-motor med förhandsgranskning | Lägg till: schemalagda rapporter (cron), automatisk e-postutsändning |
| **Sökning** | Anställda via namn | Utöka till: sök ärenden, dokument, vakanser — global omnisearch |
| **Notiser** | In-app + SignalR | Lägg till: SMS-kanal, push-notiser (PWA), notispreferenser per händelsetyp |
| **Dokumenthantering** | Uppladdning via MudFileUpload | Lägg till: versionshantering, filförhandsvisning, automatisk GDPR-gallring |
| **Self-service profil** | Kontaktuppgifter + nödkontakter | Lägg till: profilbild, bankuppgifter via BankID-verifiering, preferensinställningar |
| **Chefsportal** | Godkännandekö + 4 kort | Lägg till: teamets kompetensöversikt, medarbetarsamtal-status, budget-vy |
| **Ledighetskalender** | Veckoöversikt med emojis | Utöka till: månadsvy med scrollning, drag-to-select ledighetsperiod |
| **Offboarding** | 5-stegs wizard | Koppla till verklig slutlönsberäkning via PayrollCalculationEngine |
| **Medarbetarsamtal** | 5-stjärnig rating | Lägg till: kompetensprofil-koppling, individuell utvecklingsplan (IUP), historik |
| **Rekrytering** | Vakanser + inline-formulär | Lägg till: pipeline-vy (kanban), kommunikationsmallar, statusflöde per kandidat |

### 3.2 UX-förbättringar

| Område | Nuläge | Förbättring |
|--------|--------|-------------|
| **Laddningstider** | Demo-data = instant, DB = try/catch | Visa skeleton loaders istället för tom sida |
| **Tom-sida-hantering** | Ibland tom tabell | Visa "empty state" med illustration + handlingsknapp |
| **Felhantering** | try/catch med fallback | Visa tydliga felmeddelanden i Stinas språk |
| **Mobilresponsivitet** | MudGrid xs/sm breakpoints | Testa och optimera varje sida på 375px bredd |
| **Tangentbordsnavigering** | aria-labels finns | Testa tab-order på alla formulär |
| **Datepicker i18n** | MudDatePicker default | Konfigurera svensk kalender (måndag först, svenska månadsnamn) |

---

## 4. HELT NYA funktioner som OpenHR borde ha

### 4.1 Must-have (saknas helt idag)

| # | Funktion | Beskrivning | Prioritet |
|---|----------|-------------|-----------|
| N1 | **AI HR-assistent** | Chatbot som svarar på "Hur många semesterdagar har jag?", "När får jag lön?", "Vad gäller vid VAB?" Byggt på regelmotor, inte LLM. | Hög |
| N2 | **E-signering** | Digital signering av anställningsavtal, policykvitteringar. Integration med öppen e-signeringstjänst. | Hög |
| N3 | **Dokumentmallmotor** | Generera anställningsavtal, tjänstgöringsintyg, varningsbrev automatiskt med merge fields från personaldata. | Hög |
| N4 | **Lönekartering** | Automatisk lönekartläggning per Diskrimineringslagen — jämför lön per kön, ålder, befattning. Obligatoriskt var 3:e år. | Hög |
| N5 | **SMS-notiser** | SMS-kanal för brådskande: schemaändringar, ej instämplad, FK-deadline. Via FOSS SMS-gateway. | Hög |
| N6 | **Pulsundersökningar** | Anonyma medarbetarenkäter (1-5 frågor) med trendanalys. Mäter välbefinnande, engagemang, arbetsmiljö. | Medel |
| N7 | **Intern jobbmarknad** | Interna vakanser synliga för alla anställda. "Intresseanmälan" med ett klick. | Medel |
| N8 | **Peer recognition** | "Ge beröm till en kollega" — synligt på teamöversikten. Uppmuntrar positiv arbetskultur. | Medel |
| N9 | **Nämndadministration** | Styrelser, nämnder, förtroendevalda — arvoden, sammanträdesplanering, ersättningsberäkning. Specifikt för offentlig sektor. | Medel |
| N10 | **Kompetens-gap-analys** | Per roll/befattning: vilka kompetenser krävs vs. vilka har den anställde? Visuell gap-vy. | Medel |
| N11 | **Prediktiv bemanning** | Prognostisera bemanningsbehov baserat på historisk sjukfrånvaro, säsong, kommande ledigheter. | Låg |
| N12 | **Omsättningsprediktion** | Identifiera anställda med förhöjd risk att sluta baserat på mönster (senioritet, löneutveckling, frånvaro). | Låg |

### 4.2 Differentiators (gör OpenHR unikt)

| # | Funktion | Varför det är unikt | Stina-princip |
|---|----------|---------------------|---------------|
| D1 | **"Förklara som för Stina"** | Varje siffra, varje KPI har en förklaringsknapp: "Vad betyder sjukfrånvaro 4.2%?" → "Det betyder att i snitt 4 av 100 anställda är sjuka varje dag." | Ingen jargong, alltid förklaring |
| D2 | **Smart default-val** | Systemet föreslår det vanligaste valet först. T.ex. ledighetsansökan: "Semester" är störst, "Komptid" minst. | Vanligaste valet störst |
| D3 | **Kontextuella tips på startsidan** | "Du har 5 semesterdagar som utgår 2026-12-31 — vill du planera?" | Proaktiv hjälp |
| D4 | **Undo/ångra på allt** | Efter varje handling: "Ångra" länk synlig i 30 sekunder. | Aldrig rädd att klicka fel |
| D5 | **Progressiv komplexitet** | Samma sida visar mer detalj ju mer erfaren användaren är. Första gången: 3 fält. Tionde gången: visa avancerade alternativ. | Växer med användaren |

---

## 5. Sammanfattning: Prioriterad åtgärdslista

### DIREKT (högt värde, relativt enkelt)
1. **Klickbara KPI-kort med drill-down** på dashboard
2. **Skeleton loaders** istället för tomma sidor
3. **Empty states** med illustration och handlingsknapp
4. **Kontextuella tips** på Min sida ("5 semesterdagar utgår snart")
5. **Svensk datepicker** (måndag först, svenska namn)
6. **Global omnisearch** (sök ärenden, dokument, vakanser — inte bara anställda)
7. **Sparkline-trender** i KPI-kort (minigrafer)

### SNART (högt värde, medelstor insats)
8. **Dokumentmallmotor** — generera avtal/intyg med merge fields
9. **E-signering** — integrationsabstraktion (FOSS-vänlig)
10. **SMS-notiser** — kanal i NotificationReminderService
11. **Lönekartering** — Diskrimineringslagen-rapport
12. **Kompetens-gap-analys** — visuell per roll
13. **Rekrytering pipeline-vy** — kanban med drag-and-drop
14. **AI HR-assistent** — regelbaserad chatbot (inte LLM)
15. **Pulsundersökningar** — anonyma enkäter
16. **Schemalägga rapporter** — cron + e-postutsändning

### FRAMTID (differentiators)
17. **Prediktiv bemanning** — ML-baserad prognos
18. **Omsättningsprediktion** — attrition risk score
19. **Intern jobbmarknad**
20. **Peer recognition**
21. **Progressiv komplexitet** (visa mer för erfarna)
22. **Nämndadministration**
23. **Undo/ångra på allt**

---

## 6. HEROMA-funktioner som OpenHR redan slår

| Område | HEROMA | OpenHR | Fördel |
|--------|--------|--------|--------|
| **Schemaläggning** | AI Schema (avvikelsedetektering) | AI-optimering med OR-Tools + OhrSuggestionCard med lösningsförslag | OpenHR visar inte bara problem, utan föreslår lösningar |
| **LAS-hantering** | Automatisk tidsberäkning | Ackumulering + konversationsflöde med 3 åtgärdsval | OpenHR guidar användaren genom beslutet |
| **Lönekörning** | Gransknings-UI (okänt) | Anomalidetektering med förklaring per avvikelse | OpenHR förklarar VARFÖR en lön avviker |
| **UX/tillgänglighet** | WCAG-efterlevnad | Stina-principen: konversationsflöden, 18px text, 44px knappar | OpenHR designat för 62-åring utan datorerfarenhet |
| **Kostnad** | Kommersiell licens (miljonbelopp/år) | AGPL-3.0, 100% gratis | OpenHR är gratis för alltid |
| **Vendor lock-in** | CGI-beroende | Öppen källkod, bygg på själv | Ingen vendor lock-in |
| **Integrationer** | Standardiserade API:er | 16 specifika adapters + outbox pattern | OpenHR har fler färdiga integrationer |

---

*Denna analys bör uppdateras kvartalsvis. Källor: CGI HEROMA, Workday, SAP SuccessFactors, Oracle HCM, AIHR, Gartner.*

Sources:
- [CGI HEROMA](https://www.cgi.com/se/sv/heroma)
- [CGI HR-system](https://www.cgi.com/se/sv/hr-system)
- [Workday vs Oracle HCM vs SAP SuccessFactors](https://www.outsail.co/post/workday-vs-oracle-hcm-vs-sap-successfactors)
- [36 Best HCM Software 2026](https://peoplemanagingpeople.com/tools/hcm-software/)
- [AI and Automation in HR 2026](https://corehr.wordpress.com/2026/02/20/ai-and-automation-in-hr-the-ultimate-guide-to-digital-hr-transformation-in-2026/)
- [HCM Trends 2026](https://www.uctoday.com/talent-hcm-platforms/hcm-trends-2026/)
- [Workforce Analytics Trends 2026](https://www.aihr.com/blog/workforce-analytics-trends/)
- [HRIS Requirements Checklist - AIHR](https://www.aihr.com/blog/hris-requirements-checklist/)
- [Gartner Cloud HCM Reviews](https://www.gartner.com/reviews/market/cloud-hcm-suites-for-1000-employees)
