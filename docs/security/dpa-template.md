# Personuppgiftsbiträdesavtal (DPA)

**Mall för OpenHR-installationer**
**Senast uppdaterad:** 2026-03-21
**Rättslig grund:** Artikel 28 i GDPR (EU 2016/679)

---

## 1. Parter

**Personuppgiftsansvarig (den Ansvarige):**

| Fält | Uppgift |
|------|---------|
| Organisation | [Regionens/kommunens namn] |
| Organisationsnummer | [XXXXXX-XXXX] |
| Adress | [Gatuadress, postnummer, ort] |
| Kontaktperson | [Namn, titel] |
| E-post | [e-postadress] |
| Telefon | [telefonnummer] |
| Dataskyddsombud (DPO) | [Namn, e-post till DPO] |

**Personuppgiftsbiträde (Biträdet):**

| Fält | Uppgift |
|------|---------|
| Organisation | [Driftleverantörens namn, om extern drift] |
| Organisationsnummer | [XXXXXX-XXXX] |
| Adress | [Gatuadress, postnummer, ort] |
| Kontaktperson | [Namn, titel] |
| E-post | [e-postadress] |
| Telefon | [telefonnummer] |

**Anmärkning:** Om organisationen själv driftar OpenHR (self-hosted) utan extern leverantör behövs inget personuppgiftsbiträdesavtal för själva systemet. Detta avtal behövs enbart om en extern part hanterar drift, support eller backup av OpenHR-installationen.

---

## 2. Bakgrund och syfte

2.1 Den Ansvarige använder OpenHR (AGPL-3.0-licensierat HR-system) för hantering av personaladministration, löner, schemaplanering, ledighetshantering, kompetensuppföljning och övriga HR-processer.

2.2 Biträdet tillhandahåller [drift/hosting/support/backup] av OpenHR-installationen på uppdrag av den Ansvarige.

2.3 Detta avtal reglerar Biträdets behandling av personuppgifter i enlighet med artikel 28 i GDPR och kompletterar det underliggande tjänsteavtalet daterat [datum] ("Huvudavtalet").

---

## 3. Behandlingens omfattning

### 3.1 Ändamål

Biträdet behandlar personuppgifter uteslutande för följande ändamål:
- Drift och underhåll av OpenHR-applikationen
- Databasadministration (backup, restore, prestandaoptimering)
- Teknisk support vid felanmälningar
- Uppdateringar och uppgraderingar av OpenHR

### 3.2 Kategorier av registrerade

| Kategori | Uppskattat antal |
|----------|------------------|
| Anställda (nuvarande) | [antal] |
| Anställda (tidigare, inom lagringstid) | [antal] |
| Chefer | [antal] |
| HR-administratörer | [antal] |
| Fackliga representanter | [antal] |
| Arbetssökande (vid rekrytering) | [antal] |

### 3.3 Kategorier av personuppgifter

| Kategori | Exempel |
|----------|---------|
| Identitetsuppgifter | Namn, personnummer, anställningsnummer |
| Kontaktuppgifter | Adress, telefon, e-post, nödkontakt |
| Anställningsuppgifter | Befattning, enhet, anställningsform, tillträde, avslut |
| Löneuppgifter | Grundlön, tillägg, skatteinformation, bankkontonummer |
| Arbetstidsuppgifter | Schema, tidrapporter, stämplingar |
| Ledighetsuppgifter | Semester, föräldraledighet, VAB |
| Kompetensuppgifter | Utbildningar, certifieringar, legitimation |
| **Känsliga uppgifter (Art. 9)** | Sjukfrånvaro (ej diagnos), rehabiliteringsärenden, facklig tillhörighet |

### 3.4 Behandlingens varaktighet

Behandlingen pågår under hela avtalstiden för Huvudavtalet. Vid avtalets upphörande tillämpas avsnitt 11 (Återlämnande och radering).

---

## 4. Biträdets skyldigheter

4.1 **Instruktioner.** Biträdet ska behandla personuppgifter uteslutande i enlighet med den Ansvariges dokumenterade instruktioner. Biträdet ska omedelbart underrätta den Ansvarige om en instruktion enligt Biträdets bedömning strider mot GDPR eller annan dataskyddslagstiftning.

4.2 **Konfidentialitet.** Biträdet ska säkerställa att alla personer som är behöriga att behandla personuppgifterna har åtagit sig att iaktta konfidentialitet eller omfattas av en lämplig lagstadgad tystnadsplikt.

4.3 **Säkerhetsåtgärder.** Biträdet ska vidta de tekniska och organisatoriska åtgärder som anges i Bilaga 1 (avsnitt 8) för att säkerställa en säkerhetsnivå som är lämplig i förhållande till risken.

4.4 **Underbiträden.** Se avsnitt 5.

4.5 **Registrerades rättigheter.** Biträdet ska bistå den Ansvarige med att fullgöra skyldigheten att svara på begäranden om utövande av de registrerades rättigheter enligt kapitel III i GDPR, inklusive registerutdrag, rättelse och radering.

4.6 **Bistånd vid säkerhetsincidenter.** Biträdet ska utan onödigt dröjsmål underrätta den Ansvarige vid personuppgiftsincidenter (se avsnitt 7).

4.7 **Bistånd vid konsekvensbedömning.** Biträdet ska bistå den Ansvarige med konsekvensbedömningar (DPIA) och förhandssamråd med IMY i den mån det behövs.

4.8 **Granskning.** Biträdet ska ge den Ansvarige tillgång till all information som krävs för att visa att skyldigheterna i artikel 28 GDPR uppfylls, samt möjliggöra och bidra till granskningar och inspektioner som genomförs av den Ansvarige eller av en revisor som den Ansvarige utsett.

---

## 5. Underbiträden (Sub-processors)

5.1 Den Ansvarige ger härmed ett **allmänt förhandsgodkännande** till att Biträdet anlitar underbiträden, under förutsättning att:
- Biträdet informerar den Ansvarige skriftligen minst **30 dagar** innan ett nytt underbiträde anlitas
- Den Ansvarige har rätt att invända mot anlitandet inom 14 dagar
- Biträdet ingår skriftligt avtal med underbiträdet som ålägger samma dataskyddsskyldigheter som detta avtal

5.2 **Aktuell lista över underbiträden:**

| Underbiträde | Org.nr | Land | Tjänst | Uppgifter som behandlas |
|--------------|--------|------|--------|-------------------------|
| [Namn] | [Org.nr] | [Land] | [T.ex. serverhosting] | [T.ex. alla uppgifter vid hosting] |
| [Namn] | [Org.nr] | [Land] | [T.ex. backup-lagring] | [T.ex. krypterade databasdumpar] |

5.3 Samtliga underbiträden ska vara lokaliserade inom **EU/EEA**. Överföring till tredjeland kräver den Ansvariges uttryckliga skriftliga godkännande och lämpliga skyddsåtgärder enligt kapitel V i GDPR.

---

## 6. Tredjelandsöverföring

6.1 Biträdet ska inte överföra personuppgifter till länder utanför EU/EEA utan den Ansvariges föregående skriftliga godkännande.

6.2 Vid godkänd tredjelandsöverföring ska lämpliga skyddsåtgärder tillämpas:
- Standardavtalsklausuler (EU-kommissionens beslut 2021/914)
- Eller annat godkänt skydd enligt kapitel V GDPR

6.3 OpenHR är designat för self-hosting inom EU/EEA och kräver inga molntjänster utanför EU.

---

## 7. Personuppgiftsincidenter

7.1 Biträdet ska utan onödigt dröjsmål, och senast inom **24 timmar** efter att ha fått kännedom om en personuppgiftsincident, underrätta den Ansvarige.

7.2 Underrättelsen ska innehålla:
- Beskrivning av incidentens art, inklusive kategorier och ungefärligt antal berörda registrerade
- Namn och kontaktuppgifter till Biträdets kontaktperson
- Beskrivning av sannolika konsekvenser
- Beskrivning av åtgärder som vidtagits eller föreslås

7.3 Biträdet ska dokumentera alla personuppgiftsincidenter, inklusive omständigheter, effekter och vidtagna korrigerande åtgärder.

7.4 Biträdet ska bistå den Ansvarige med att uppfylla anmälningsskyldigheten till IMY (72 timmar) och, vid behov, underrätta berörda registrerade.

---

## 8. Tekniska och organisatoriska åtgärder (Bilaga 1)

### 8.1 Åtkomstkontroll

| Åtgärd | Beskrivning |
|--------|-------------|
| Fysisk åtkomst | Servrar i låst serverrum/datacenter med passerkontroll |
| Logisk åtkomst | SSH-nyckelbaserad inloggning, inga delade konton |
| Behörighetshantering | Namngivna konton, loggade sessioner, minsta-privilegier-princip |
| Granskning | Kvartalsvis genomgång av åtkomstbehörigheter |

### 8.2 Kryptering

| Åtgärd | Beskrivning |
|--------|-------------|
| I transit | TLS 1.2+ för all kommunikation (HTTPS, databas, SignalR) |
| I vila | pgcrypto AES-256 för personnummer och bankuppgifter |
| Backup | GPG-krypterade databasdumpar |
| Nycklar | Separata krypteringsnycklar lagras utanför databasen |

### 8.3 Tillgänglighet och motståndskraft

| Åtgärd | Beskrivning |
|--------|-------------|
| Backup | Daglig krypterad backup, 30 dagars retention |
| Återställning | Testad återställning minst en gång per kvartal |
| Övervakning | Hälsokontroll var 30:e sekund, larm vid avbrott |
| Uppdateringar | Säkerhetsuppdateringar inom 7 dagar efter publicering |

### 8.4 Loggning och spårbarhet

| Åtgärd | Beskrivning |
|--------|-------------|
| Applikationslogg | Strukturerad JSON-loggning av alla operationer |
| Granskningslogg | AuditEntry med användare, tidpunkt, gamla/nya värden |
| Databaslogg | PostgreSQL log_connections, log_statement = 'ddl' |
| Lagring | Loggar sparas minst 90 dagar |

### 8.5 Organisatoriska åtgärder

| Åtgärd | Beskrivning |
|--------|-------------|
| Utbildning | All personal utbildad i dataskydd och GDPR |
| Sekretess | Sekretessförbindelser undertecknade av all personal |
| Incidentprocess | Dokumenterad och övad incidenthanteringsprocess |
| Ändringskontroll | Alla systemändringar dokumenterade och godkända |

---

## 9. Granskning och revision

9.1 Den Ansvarige har rätt att genomföra revision av Biträdets behandling, antingen själv eller genom en oberoende tredjepartsrevisor, med **14 dagars** skriftligt förvarning.

9.2 Biträdet ska tillhandahålla all nödvändig information och åtkomst för genomförande av revisionen.

9.3 Revision ska genomföras minst en gång per **kalenderår** under avtalstiden.

9.4 Kostnader för revision bärs av den Ansvarige, om inte revisionen avslöjar väsentliga brister hos Biträdet.

---

## 10. Ansvar och ersättning

10.1 Biträdet ansvarar för skada som orsakats av behandling som strider mot detta avtal, den Ansvariges instruktioner eller tillämplig dataskyddslagstiftning.

10.2 Biträdet ska hålla den Ansvarige skadeslös avseende anspråk, böter eller skadestånd som uppstår till följd av Biträdets brott mot detta avtal.

---

## 11. Återlämnande och radering vid avtalets upphörande

11.1 Vid Huvudavtalets upphörande ska Biträdet, enligt den Ansvariges val:
- **Återlämna** samtliga personuppgifter till den Ansvarige i ett strukturerat, maskinläsbart format (PostgreSQL dump eller CSV), och därefter radera alla kopior; eller
- **Radera** samtliga personuppgifter och befintliga kopior, och skriftligen bekräfta att radering skett

11.2 Radering ska ske inom **30 dagar** från Huvudavtalets upphörande.

11.3 Undantag: Personuppgifter som Biträdet är skyldigt att behålla enligt lag eller myndighetsföreskrift ska lagras separat och skyddas under kvarvarande lagringstid.

---

## 12. Avtalstid och uppsägning

12.1 Detta avtal gäller från och med undertecknandet och löper så länge Biträdet behandlar personuppgifter för den Ansvariges räkning under Huvudavtalet.

12.2 Den Ansvarige har rätt att säga upp detta avtal med omedelbar verkan vid väsentligt brott mot Biträdets skyldigheter.

---

## 13. Tillämplig lag och tvistlösning

13.1 Svensk lag ska tillämpas på detta avtal.

13.2 Tvister som uppstår med anledning av detta avtal ska avgöras av svensk allmän domstol, med [ort] tingsrätt som första instans.

---

## 14. Underskrifter

**Personuppgiftsansvarig:**

| | |
|---|---|
| Datum | ______________________ |
| Namnförtydligande | ______________________ |
| Titel | ______________________ |
| Underskrift | ______________________ |

**Personuppgiftsbiträde:**

| | |
|---|---|
| Datum | ______________________ |
| Namnförtydligande | ______________________ |
| Titel | ______________________ |
| Underskrift | ______________________ |

---

*Denna mall är baserad på artikel 28 GDPR och anpassad for svenska regioners och kommuners användning av OpenHR. Mallen bör granskas av organisationens jurist och dataskyddsombud innan undertecknande.*
