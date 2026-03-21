# OpenHR GDPR Compliance Guide

**Version:** 2026-03-21
**Applicable regulation:** GDPR (EU 2016/679) as implemented in Swedish law
**Supervisory authority:** Integritetsskyddsmyndigheten (IMY)
**Organization type:** Swedish region/municipality (offentlig sektor)

---

## 1. Data Categories Processed

OpenHR processes the following categories of personal data (personuppgifter):

### 1.1 Ordinary Personal Data (Artikel 6)

| Category | Examples | Legal Basis | Retention |
|----------|----------|-------------|-----------|
| **Identitetsuppgifter** | Namn, personnummer, foto | Art. 6(1)(b) — employment contract | Employment + 10 years |
| **Kontaktuppgifter** | Adress, telefon, e-post, nödkontakt | Art. 6(1)(b) — employment contract | Employment + 2 years |
| **Anställningsuppgifter** | Befattning, enhet, anställningsform, tillträde, avslut | Art. 6(1)(b) — contract + Art. 6(1)(c) — LAS | Employment + 10 years |
| **Löneuppgifter** | Grundlön, tillägg, OB, skatt, bankkontonummer | Art. 6(1)(b) — contract + Art. 6(1)(c) — skattelagstiftning | 7 years (bokforingslagen) |
| **Arbetstidsuppgifter** | Schema, tidrapporter, stämplingar, övertid | Art. 6(1)(c) — ATL (arbetstidslagen) | 7 years |
| **Ledighetsuppgifter** | Semester, föräldraledighet, VAB, tjänstledighet | Art. 6(1)(b) — contract + Art. 6(1)(c) — semesterlagen | 7 years |
| **Kompetensuppgifter** | Utbildning, certifieringar, kurser, legitimation | Art. 6(1)(c) — krav på legitimation i vården | Employment + 5 years |
| **Organisationsuppgifter** | Enhetstillhörighet, chef, delegering | Art. 6(1)(f) — legitimate interest (verksamhetsstyrning) | Employment duration |
| **Granskningslogg** | Användaraktioner, ändringar, tidsstämplar | Art. 6(1)(c) — GDPR Art. 5(2) accountability | 2 years |

### 1.2 Special Categories of Personal Data (Artikel 9 — Känsliga uppgifter)

| Category | Examples | Legal Basis | Extra Protection |
|----------|----------|-------------|------------------|
| **Sjukfrånvaro** | Sjukanmälan, sjukperioder, karensdag | Art. 9(2)(b) — employment law obligations | RLS: only employee + manager + HR |
| **Rehabilitering (HälsoSAM)** | Rehabärenden, uppföljningar, dag 14/90/180/365 | Art. 9(2)(b) — employer's rehab duty (AML, SFB) | RLS: case owner + HR only |
| **Facklig tillhörighet** | MBL-förhandlingar, facklig representant-flagga | Art. 9(2)(b) — MBL obligations | Restricted to HR + union rep |

**Important:** OpenHR does NOT process health diagnoses (diagnoser). Sick leave records contain only dates and return-to-work status, not medical information. If diagnosis information is needed for Försäkringskassan reporting, it is handled directly by the employee and FK, not through OpenHR.

---

## 2. Legal Basis per Processing Activity

### 2.1 Article 6 — Lawfulness of Processing

| Processing Activity | Legal Basis | Justification |
|---------------------|-------------|---------------|
| Manage employment records | Art. 6(1)(b) | Necessary for employment contract |
| Calculate and pay salary | Art. 6(1)(b) + (c) | Contract + tax law (SFL, SAL) |
| Report to Skatteverket (AGI) | Art. 6(1)(c) | Skatteforfarandelagen |
| Report to Försäkringskassan | Art. 6(1)(c) | Socialforsakringsbalken |
| Track LAS accumulation | Art. 6(1)(c) | LAS (anstallningsskyddslagen) |
| Manage work schedules | Art. 6(1)(c) | ATL (arbetstidslagen) |
| Handle leave requests | Art. 6(1)(b) + (c) | Contract + Semesterlagen |
| Competence/certification tracking | Art. 6(1)(c) | Patientsäkerhetslagen (legitimation) |
| Audit logging | Art. 6(1)(c) | GDPR Art. 5(2) accountability |
| Performance reviews | Art. 6(1)(f) | Legitimate interest (employer's duty to develop employees) |
| Onboarding/offboarding | Art. 6(1)(b) | Necessary steps for employment relationship |
| Recruitment | Art. 6(1)(b) | Pre-contractual measures at data subject's request |
| Emergency contacts | Art. 6(1)(f) | Legitimate interest (workplace safety, AML) |

### 2.2 Article 9 — Processing of Special Categories

| Processing Activity | Legal Basis | Specific Law |
|---------------------|-------------|--------------|
| Sick leave tracking | Art. 9(2)(b) | SFB kap. 27, AML 3 kap. 2a § |
| Rehabilitation cases | Art. 9(2)(b) | AML 3 kap. 2a §, SFB kap. 30 |
| MBL negotiations | Art. 9(2)(b) | MBL (medbestämmandelagen) |

---

## 3. Data Subject Rights Implementation

OpenHR implements the following GDPR data subject rights:

### 3.1 Right of Access (Art. 15) — Registerutdrag

**Implementation:** `/gdpr` page with "Generera registerutdrag" function.

- Employee can request their own data via Min sida
- HR can generate on behalf of any employee
- Generated as PDF containing all personal data across all modules
- Response time: immediately (automated) or within 30 days per GDPR
- Audit logged

### 3.2 Right to Rectification (Art. 16) — Rättelse

**Implementation:** Employees can update their own contact details via `/minsida/profil`. HR can update any field via `/anstallda/{id}/redigera`.

- All changes are audit logged with old and new values
- Changed-by user and timestamp recorded

### 3.3 Right to Erasure (Art. 17) — Radering

**Implementation:** Partial — limited by legal retention requirements.

- Employee data cannot be fully deleted while legally required retention periods are active
- After retention period expires, `RetentionCleanupService` background job automatically anonymizes data
- Anonymization replaces personal identifiers with hashed values, preserving statistical utility
- Manual anonymization available via `/gdpr` for HR

**Exceptions to erasure (Art. 17(3)):**
- Bokforingslagen requires 7-year retention for financial records
- LAS requires employment period records
- Patientsäkerhetslagen requires competence/legitimation records

### 3.4 Right to Restriction (Art. 18) — Begränsning

**Implementation:** HR can flag an employee record as "restricted" via GDPR dashboard. Restricted records are excluded from bulk operations and reports but remain accessible for legal obligations.

### 3.5 Right to Data Portability (Art. 20) — Dataportabilitet

**Implementation:** Export to CSV/Excel available from most list views. Full employee data export (JSON) available via `/gdpr`.

### 3.6 Right to Object (Art. 21) — Invändning

**Implementation:** Not directly applicable for most processing (legal basis is contract/legal obligation, not consent or legitimate interest). For performance reviews (legitimate interest), employee can object via HR case.

---

## 4. Data Retention Periods

| Entity Type | Retention Period | Legal Basis | Cleanup Method |
|-------------|-----------------|-------------|----------------|
| Employee (core) | Employment + 10 years | LAS, preskriptionslagen | Anonymize |
| Employment records | Employment + 10 years | LAS | Anonymize |
| Salary/payroll data | 7 years from financial year | Bokforingslagen 7:2 | Delete |
| Tax reports (AGI) | 7 years | SFL | Delete |
| Timesheets | 7 years | ATL | Delete |
| Leave requests | 7 years | Semesterlagen, SFB | Delete |
| Sick leave records | 7 years | SFB, AML | Anonymize |
| Rehabilitation cases | 7 years after case closure | AML, SFB | Anonymize |
| Competence/certs | Employment + 5 years | Patientsäkerhetslagen | Delete |
| Performance reviews | 3 years | Internal policy | Delete |
| Audit log entries | 2 years | GDPR Art. 5(2) | Delete |
| Recruitment (hired) | Employment + 2 years | Diskrimineringslagen | Anonymize |
| Recruitment (not hired) | 2 years from decision | Diskrimineringslagen 6:3 | Delete |
| Documents (general) | Per document GDPR classification | Varies | Per classification |
| Login/session logs | 90 days | GDPR Art. 5(1)(e) | Delete |

The `RetentionCleanupService` background job runs daily and processes records that have exceeded their retention period.

---

## 5. Sub-Processor Documentation

OpenHR is self-hosted. In a standard deployment, there are no sub-processors (personuppgiftsbitraden) because all data remains within the organization's own infrastructure.

If the organization uses external services in conjunction with OpenHR, these must be documented:

| Sub-Processor | Purpose | Data Transferred | Agreement |
|---------------|---------|------------------|-----------|
| (hosting provider, if any) | Server infrastructure | All data at rest | DPA required |
| (SMTP relay, if external) | Email delivery | Recipient address + email content | DPA required |
| (backup storage, if external) | Off-site backups | Encrypted database dumps | DPA required |

**Recommendation:** Keep all sub-processors within EU/EEA to avoid Schrems II complications. OpenHR's design as a self-hosted application specifically avoids mandatory cloud sub-processors.

---

## 6. DPIA — Data Protection Impact Assessment

A DPIA (Konsekvensbedömning) is **required** for OpenHR because:
- Large-scale processing of employee data (Art. 35(3)(b))
- Processing of special categories (health data — sick leave, rehabilitation)
- Systematic monitoring of employees (audit logging, time tracking)

### 6.1 DPIA Template Reference

The organization should complete a DPIA using IMY's recommended template:
- **IMY DPIA Guide:** https://www.imy.se/verksamhet/dataskydd/det-har-galler-enligt-gdpr/konsekvensbedomning/
- **EDPB Guidelines on DPIA (wp248rev.01)**

### 6.2 Key DPIA Considerations for OpenHR

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Unauthorized access to salary data | Medium | High | Role-based access, RLS, audit logging |
| Exposure of health data (sick leave) | Medium | Very High | Separate RLS policies, HälsoSAM restricted access |
| Data breach (database compromise) | Low | Very High | Encryption at rest (pgcrypto), TLS in transit, network isolation |
| Excessive data retention | Medium | Medium | Automated retention cleanup, configurable periods |
| Employee surveillance concern | Medium | Medium | Transparent audit log, employee can see own data |
| Cross-border data transfer | Low | High | Self-hosted within EU/EEA, no mandatory cloud services |

---

## 7. Technical and Organizational Measures (TOMs)

### 7.1 Technical Measures

| Measure | Implementation |
|---------|---------------|
| Encryption at rest | pgcrypto for personnummer, bankuppgifter |
| Encryption in transit | TLS 1.2+ (database, HTTPS, SignalR) |
| Access control | Role-based (7 roles), unit-based (UnitAccessScopeService) |
| Row-level security | PostgreSQL RLS policies per module/schema |
| Audit logging | All CRUD operations logged with old/new values |
| Session management | 30-minute inactivity timeout |
| Input validation | Server-side validation, parameterized queries (EF Core) |
| CSP headers | Content Security Policy restricting script/style sources |
| Rate limiting | 100 requests/minute per IP |
| Backup encryption | GPG-encrypted database backups |

### 7.2 Organizational Measures

| Measure | Responsibility |
|---------|---------------|
| Data Protection Officer (DPO) appointed | Organization |
| Staff training on GDPR and data handling | Organization |
| Incident response plan documented | Organization + IT |
| Regular access reviews (quarterly) | HR + IT |
| Sub-processor agreements maintained | Legal/DPO |
| DPIA completed and reviewed annually | DPO |
| Register of processing activities (Art. 30) | DPO |

---

## 8. Data Breach Procedures

### 8.1 Detection

OpenHR assists breach detection through:
- Audit logging of all data access
- Rate limiting to prevent bulk extraction
- Health monitoring for service availability

### 8.2 Response Timeline (Art. 33, 34)

| Action | Deadline |
|--------|----------|
| Internal assessment of breach severity | Within 4 hours |
| Notify IMY (if risk to data subjects) | Within 72 hours |
| Notify affected data subjects (if high risk) | Without undue delay |
| Document breach in internal register | Immediately |

### 8.3 Breach Classification

| Classification | Criteria | IMY Notification | Data Subject Notification |
|----------------|----------|------------------|---------------------------|
| **Low** | Encrypted data, limited scope, no sensitive data | No | No |
| **Medium** | Personal data exposed, limited scope | Yes (72h) | Case-by-case |
| **High** | Sensitive data (health, salary), large scope | Yes (72h) | Yes |
| **Critical** | Mass exposure of sensitive data, identity theft risk | Yes (72h) | Yes (immediately) |
