> **ARCHIVED:** This document reflects the state as of 2026-03-17 and is superseded by README.md.
> At the time of writing, OpenHR had 25 modules and 65+ entities. The system has since expanded
> to 36 modules and 160+ entities (OpenHR 2.0). See README.md for current status.

---

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

*Dokumentet genererat 2026-03-17. Se README.md för aktuell status.*
