# RegionHR - Komplett HR-system for svensk region

## Projektbeskrivning
Modular monolith HR-system som ersatter HEROMA. 19 moduler, 388 tester, 131 API-endpoints, 16 integrationsadapters. Hanterar personalregister, lon/payroll (svensk skattelagstiftning + kollektivavtal AB/HOK), schemaplaggning (24/7 sjukvard), sjalvservice, arendehantering, LAS-uppfoljning, rehabilitering (HalsoSAM), loneversyn, resor/utlagg, rekrytering, ledighetshantering, dokumenthantering, medarbetarsamtal, kompetensregister, rapportering, GDPR-efterlevnad, granskningslogg, notifieringar, samt integrationer mot ~15 externa system.

## Bygga och kora
```bash
dotnet build RegionHR.sln
dotnet test RegionHR.sln
dotnet run --project src/Api/RegionHR.Api.csproj
```

## Arkitektur
- **Modular Monolith** med schema-per-modul i PostgreSQL
- Moduler kommunicerar via publika C# interfaces (Contracts) och domanhendelser
- Aldrig databas-joins over modulgranser
- Export: CSV/Excel (ClosedXML), PDF (QuestPDF)
- Notifieringar: InApp + Email (MailKit)

## Modulstruktur (19 moduler)

### Karnmoduler (Fas 0-3)
- `src/SharedKernel/` - Domanprimitiver (Personnummer, Money, DateRange, enums)
- `src/Modules/Core/` - Personalregister, organisation, anstallningar
- `src/Modules/Payroll/` - Loneberakning, skatt, kollektivavtal
- `src/Modules/Scheduling/` - Schema, instampling, AI-optimering
- `src/Modules/CaseManagement/` - Arenden, workflows, franvaro
- `src/Modules/LAS/` - LAS-ackumulering, konvertering, turordning
- `src/Modules/HalsoSAM/` - Rehabilitering, sjukfranvarobevakning
- `src/Modules/SalaryReview/` - Loneoversynsrundor
- `src/Modules/Travel/` - Resor, utlagg, traktamente
- `src/Modules/Recruitment/` - Vakanser, ansokngar
- `src/Modules/IntegrationHub/` - AGI-XML, Nordea pain.001, outbox pattern
- `src/Modules/SelfService/` - Sjalvserviceportal

### Expansion (Fas 5 - Wave 1-3)
- `src/Modules/Audit/` - Granskningslogg (alla andringar loggas)
- `src/Modules/Notifications/` - Notiseringar (InApp, Email, SMS)
- `src/Modules/Leave/` - Ledighetshantering (semester, sjukfranvaro, VAB, foraldraledighet)
- `src/Modules/Documents/` - Dokumenthantering med retention och GDPR-klassificering
- `src/Modules/Performance/` - Medarbetarsamtal (sjalvbedomning + chefsbedomning)
- `src/Modules/Reporting/` - Rapportering (standardrapporter + schemalagda)
- `src/Modules/GDPR/` - GDPR-efterlevnad (registerutdrag, anonymisering, retention)
- `src/Modules/Competence/` - Kompetens/certifieringar (legitimationer, obligatoriska utbildningar)

### Presentation
- `src/Api/` - ASP.NET Core Web API + HTML SPA
- `src/Web/` - Blazor Server SSR frontend
- `src/DesignSystem/` - Blazor komponentbibliotek
- `src/Infrastructure/` - EF Core, repositories, export (CSV/Excel/PDF)

## Konventioner
- Alla monetara varden anvander `Money` (decimal-baserad, SEK)
- Personnummer hanteras via `Personnummer` value object med Luhn-validering
- Svenskt spraak i domanmodell (metoder, egenskaper), engelskt for infrastruktur
- Varje modul exponerar ett publikt `Contracts/` interface for anvandning av andra moduler
- Nya moduler foljer samma monster: Domain/ + Contracts/ + Services/ + tester
