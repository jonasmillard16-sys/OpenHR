# HEROMA Datamigreringsanalys

## 1. Bakgrund

HEROMA (CGI) är det befintliga HR-systemet. Migrering till RegionHR kräver överföring av alla personuppgifter, anställningshistorik, lönedata, scheman, och frånvarohistorik. Denna analys beskriver strategi, datamappning, och risker.

## 2. Migreringsstrategi

### Fas 1: Initial laddning (Månad 5-8)
- **Full export** från HEROMA:s Oracle-databas via CGI:s rapportverktyg
- Transformering till RegionHR:s PostgreSQL-schema
- Parallellkörning: Bidirektionell synkbrygga under övergångsperioden

### Fas 2: Inkrementell synk (Månad 8-20)
- Ändringar i HEROMA replikeras till RegionHR via nattliga ETL-jobb
- Ändringar i RegionHR (nya moduler) propageras tillbaka via integrationsbryggan
- Konfliktlösning: HEROMA är master tills modul-för-modul-cutover

### Fas 3: Cutover per modul (Månad 12-30)
- Core HR först (efter parallellvalidering)
- Lön sist (efter 6 månaders parallellkörning med öresavstemning)

### Fas 4: HEROMA-avveckling (Månad 30-36)
- Historisk data arkiveras (7+ år per bokföringslagen)
- HEROMA sätts i read-only, sedan avvecklas

## 3. Datamappning

### 3.1 Personal (HEROMA → core_hr.employees)

| HEROMA-fält | RegionHR-fält | Transformation |
|-------------|---------------|----------------|
| PERSNR | personnummer_encrypted | Validering + Luhn-check + pgcrypto-kryptering |
| FNAMN | fornamn | Trim, capitalize |
| ENAMN | efternamn | Trim, capitalize |
| GATUADR | gatuadress | Standardisering via adressregister |
| POSTNR | postnummer | Format: "NNN NN" |
| POSTORT | ort | Standardisering |
| BANKCLR | clearingnummer_encrypted | Kryptering |
| BANKKONTONR | kontonummer_encrypted | Kryptering |
| SKATTETAB | skattetabell | Direktmappning (30-36) |
| SKATTEKOL | skattekolumn | Direktmappning (1-6) |
| KOMMUN_KOD | kommun | Uppslag mot kommunregister |
| KYRKOAVG | har_kyrkoavgift / kyrkoavgiftssats | Boolean + sats |

### 3.2 Anställningar (HEROMA → core_hr.employments)

| HEROMA-fält | RegionHR-fält | Transformation |
|-------------|---------------|----------------|
| ANST_ID | id | Ny UUID, mappningstabell behålls |
| PERSNR | anstalld_id | Uppslag via personnummer_hash |
| ENHET_KOD | enhet_id | Mappning HEROMA-enhets-ID → RegionHR OrganizationUnit |
| ANST_FORM | anstallningsform | Kodkonvertering: 1→Tillsvidare, 2→Vikariat, 3→SAVA, etc. |
| KOL_AVTAL | kollektivavtal | Kodkonvertering: AB/HOK/MBA |
| MANLON | manadslon | Decimalkonvertering, valideringscheck |
| SYSS_GRAD | sysselsattningsgrad | Procentvalidering (0-100) |
| STARTDAT | start_datum | Datumformat YYYYMMDD → DATE |
| SLUTDAT | slut_datum | Datumformat, NULL för tillsvidare |
| BESTA | besta_kod | Direktmappning |

### 3.3 Lönehistorik (HEROMA → payroll.payroll_results)

| HEROMA-fält | RegionHR-fält | Transformation |
|-------------|---------------|----------------|
| LONEPER_AR + LONEPER_MAN | ar + manad | Split period |
| BRUTTOLON | brutto | Decimal |
| SKATTEBELOP | skatt | Decimal |
| NETTOLON | netto | Decimal |
| AG_AVG | arbetsgivaravgifter | Decimal |
| LONEART_* | payroll_result_lines | En rad per löneart |

### 3.4 Organisation (HEROMA → core_hr.organization_units)

| HEROMA-fält | RegionHR-fält | Transformation |
|-------------|---------------|----------------|
| ENHET_KOD | id (ny UUID) | Mappningstabell |
| ENHET_NAMN | namn | Trim |
| ENHET_TYP | typ | Kodkonvertering till enum |
| OVERENHET | overordnad_enhet_id | Rekursiv mappning |
| KSTALLE | kostnadsstalle | Direktmappning |
| CFAR | cfar_kod | Direktmappning |

### 3.5 Schema/tid (HEROMA → scheduling.*)

| HEROMA-fält | RegionHR-fält | Transformation |
|-------------|---------------|----------------|
| SCHEMA_ID | schedules.id | Ny UUID |
| PASS_DATUM | scheduled_shifts.datum | Datumkonvertering |
| PASS_START | scheduled_shifts.planerad_start | Tidkonvertering |
| PASS_SLUT | scheduled_shifts.planerad_slut | Tidkonvertering |
| STAMPL_IN | time_clock_events (In) | Separata händelser |
| STAMPL_UT | time_clock_events (Ut) | Separata händelser |

### 3.6 Frånvaro (HEROMA → case_mgmt.cases)

| HEROMA-fält | RegionHR-fält | Transformation |
|-------------|---------------|----------------|
| FRANV_TYP | cases.typ = 'Franvaro' + franvaro_data.franvaro_typ | Kodkonvertering |
| FRANV_FRAN | franvaro_data.fran_datum | Datumkonvertering |
| FRANV_TILL | franvaro_data.till_datum | Datumkonvertering |
| FRANV_OMFATT | franvaro_data.omfattning | Procentkonvertering |

### 3.7 LAS-data (HEROMA → las.*)

| HEROMA-fält | RegionHR-fält | Transformation |
|-------------|---------------|----------------|
| LAS_DAGAR | accumulations.ackumulerade_dagar | Direktmappning + omberäkning |
| LAS_PERIODER | periods.* | En rad per period |

## 4. Datakvalitetsregler

### Valideringar vid import
1. **Personnummer**: Luhn-validering, sekelhantering, dubblettcheck
2. **Lönedata**: Netto <= Brutto, Skatt >= 0, AG-avgifter = Brutto * förväntad sats (±1%)
3. **Datum**: Slutdatum >= Startdatum, inga framtida startdatum i historisk data
4. **Organisationsträd**: Ingen cirkulär referens, alla enheter har överordnad (utom Region)
5. **LAS-dagar**: Omberäkna från perioder, jämför med HEROMA:s ackumulerade dagar

### Datarensning
- Inaktiva anställningar >7 år: Arkiveras direkt (ej migreras till aktiv databas)
- Dubbla personnummer: Manuell granskning och sammanslagning
- Felaktiga organisationskoder: Mappning mot aktuellt organisationsträd

## 5. Teknisk implementation

### ETL-pipeline
```
HEROMA Oracle → CSV/XML Export → Transform (C# ETL-jobb) → Validering → PostgreSQL Import
```

### Verktyg
- **Export**: CGI:s rapportmotor / Oracle SQL*Plus
- **Transform**: C# konsolapplikation med FluentValidation
- **Load**: Npgsql COPY för bulk-import
- **Validering**: Automatiserade jämförelserapporter

### Parallellkörningsramverk
```
                    ┌──────────┐
Indata ────────────►│ HEROMA   │──► Löneresultat A
(samma anställda)   └──────────┘
                    ┌──────────┐
                ────►│ RegionHR │──► Löneresultat B
                    └──────────┘
                    ┌──────────┐
                    │ Jämför   │──► Avvikelserapport (per anställd, per löneart, örenivå)
                    └──────────┘
```

## 6. Risker

| Risk | Sannolikhet | Konsekvens | Åtgärd |
|------|-------------|------------|--------|
| Felaktig datamappning | Hög | Hög | Stegvis validering, parallellkörning |
| HEROMA-databrister (dirty data) | Hög | Medel | Fördefinierade rensningsregler, manuell granskning |
| Organisationsförändringar under migrering | Medel | Medel | Integrationsbrygga med konflikthantering |
| Löneberäkningsavvikelser | Medel | Mycket hög | 6 mån parallellkörning, öresavstemning |
| Prestanda vid initial last | Låg | Medel | Bulk COPY, partitionerade tabeller |
| GDPR-överträdelse vid dataöverföring | Låg | Mycket hög | Kryptering under transport, accessloggning |

## 7. Tidsplan

| Aktivitet | Period | Beroenden |
|-----------|--------|-----------|
| Datamappningsanalys (denna doc) | Fas 0, Mån 3-4 | - |
| ETL-utveckling (export + transform) | Fas 1, Mån 5-7 | Datamappning klar |
| Testmigrering (subset) | Fas 1, Mån 7-8 | ETL klart |
| Full testmigrering + validering | Fas 1, Mån 9-10 | Core HR klart |
| Parallellkörning (lön) | Fas 2, Mån 13-18 | Lönemotor klar |
| Modul-för-modul cutover | Fas 2-3, Mån 15-28 | Per modul |
| Historisk dataarkivering | Fas 4, Mån 30-34 | Alla moduler cutover |
| HEROMA-avveckling | Fas 4, Mån 34-36 | Arkivering klar |

## 8. Acceptanskriterier

1. **100% personalregister** migrerat med validerat personnummer
2. **≥99.9% lönehistorik** matchad på örenivå mot HEROMA
3. **Alla aktiva anställningar** korrekt mappade till rätt organisationsenhet
4. **LAS-dagar** avviker max ±1 dag från HEROMA:s ackumulering
5. **Inga GDPR-överträdelser** vid granskning av åtkomstloggar
6. **Parallell lönekörning** avviker max ±1 kr per anställd i 3 konsekutiva månader
