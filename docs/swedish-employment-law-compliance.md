# Swedish Employment Law Compliance Requirements for HR System

**Version:** 2025-2026
**Scope:** Swedish regional healthcare organization (kommun/region sector)
**Last researched:** 2026-03-16

---

## 1. LAS - Lagen om anstallningsskydd (SFS 1982:80)

### 1.1 Sarskild visstidsanstallning (SAVA) - replacing ALVA from 2022-10-01

| Rule | Threshold | Legal reference |
|------|-----------|-----------------|
| Conversion SAVA to permanent | > 12 months total during a 5-year reference period | 5 a SS LAS |
| Conversion via "chain" rule | SAVA + other fixed-term (vikariat, seasonal) in sequence without 6+ month gap totaling > 12 months | 5 a SS LAS |
| Vikariat conversion to permanent | > 2 years total during a 5-year period | 5 a SS LAS |
| Foretradesratt (priority re-employment) for SAVA | After > 9 months total during a 3-year period | 25 SS LAS |
| Foretradesratt duration | 9 months from employment end date | 25 SS LAS |
| Foretradesratt for permanent/vikariat | After > 12 months total during a 3-year period | 25 SS LAS |

### 1.2 Turordning (seniority order at redundancy)

| Rule | Detail | Legal reference |
|------|--------|-----------------|
| Principle | "Sist in, forst ut" (last in, first out) by seniority within each driftsenhet (operational unit) and collective agreement area | 22 SS LAS |
| Employer exemptions | Up to 3 employees may be exempted from the seniority list per redundancy round | 22 SS 2nd paragraph LAS (from 2022-10-01) |
| Quarantine on 3-exemption | Cannot use the 3-exemption again within 3 months | 22 SS LAS |
| Sufficient qualifications | Remaining employees must have sufficient qualifications for available positions | 22 SS LAS |

### 1.3 Transition rules (2022 reform)

- ALVA contracts active on 2022-10-01: old 2-year conversion rules still apply to that specific contract
- New SAVA contracts from 2022-10-01: ALVA time from 2022-03-01 onward counts toward the new 12-month threshold

### HR System UI Requirements

- **Employment type tracking**: System must distinguish between tillsvidareanstallning, SAVA, vikariat, provanstallning, and seasonal employment
- **Automatic day counter**: Track accumulated SAVA days, vikariat days, and combined fixed-term chains per employee over rolling 5-year and 3-year windows
- **Gap detection**: Detect gaps of 6+ months that break the "chain" for conversion
- **Conversion alerts**: Warn administrators before 12-month (SAVA) and 24-month (vikariat) thresholds are reached
- **Foretradesratt register**: Track employees with earned priority re-employment rights, with 9-month expiry countdown
- **Turordning calculator**: Generate seniority lists per driftsenhet and collective agreement area, with ability to mark up to 3 exemptions

---

## 2. ATL - Arbetstidslagen (SFS 1982:673)

### 2.1 Core working time limits

| Rule | Limit | Legal reference |
|------|-------|-----------------|
| Ordinarie arbetstid (regular hours) | Max 40 hours per week | 5 SS ATL |
| Dygnsvila (daily rest) | Min 11 consecutive hours per 24-hour period | 13 SS ATL |
| Veckovila (weekly rest) | Min 36 consecutive hours per 7-day period | 14 SS ATL |
| Allman overtid (general overtime) | Max 48 hours per 4-week period OR 50 hours per calendar month, max 200 hours per calendar year | 8 SS ATL |
| Extra overtid (additional overtime) | Max 150 hours per calendar year (on top of general overtime, requires special reasons) | 8 a SS ATL |
| Total overtime cap | 200 + 150 = 350 hours per calendar year maximum | 8, 8 a SS ATL |
| Nattarbete (night work) - high risk | Max 8 hours per 24-hour period for workers doing risky/strenuous work | 13 a SS ATL |
| Night definition | 22:00 - 06:00 | 13 a SS ATL |
| Night worker definition | Normally works >= 3 hours during night per shift, or >= 1/3 of annual hours during night | 13 a SS ATL |
| Maximum weekly working time (EU directive) | Average 48 hours per week including overtime, calculated over a 4-month reference period | EU Working Time Directive 2003/88/EC |

### 2.2 Healthcare sector - dygnsvila rules (from Oct 2023 / Feb 2024)

| Rule | Detail | Reference |
|------|--------|-----------|
| Standard dygnsvila | 11 hours consecutive rest per 24-hour period | AB SS 13 / ATL 13 SS |
| Exception for life/health/safety | Can be reduced to 9 hours dygnsvila in planned scheduling, if work cannot be organized differently | Collective agreement SKR/Sobona (2023) |
| Dispensation (jour + work) | Total combined work + jour may exceed 20 hours, up to max 24 hours | Dispensoverenskommelse Feb 2024 |
| Compensatory rest | Shortened dygnsvila must be compensated with equivalent extended rest within a defined period | EU Working Time Directive |

**Background:** EU Commission investigated Sweden for non-compliance with the Working Time Directive (prompted by an ambulance nurse complaint in 2021). New collective agreement rules from Oct 2023/Feb 2024 resolved the issue; EU closed its case.

### HR System UI Requirements

- **Working hours tracker**: Real-time tracking of regular hours, overtime (allman + extra), per week, 4-week period, calendar month, and calendar year
- **Overtime ceiling warnings**: Alert when approaching 48h/4-week, 50h/month, 200h/year (allman), 150h/year (extra) limits
- **Rest period validation**: In scheduling module, validate 11-hour dygnsvila gaps (or 9-hour with documented exception for healthcare)
- **Weekly rest validation**: Ensure 36 consecutive hours off per 7-day period in schedules
- **Night worker classification**: Auto-flag employees meeting night worker criteria
- **EU 48-hour average calculator**: Rolling 4-month average of total weekly working time
- **Jour/work combination limiter**: Cap combined jour + active work at 20h (24h with dispens)

---

## 3. Semesterlagen (SFS 1977:480)

### 3.1 Statutory minimum

| Rule | Value | Legal reference |
|------|-------|-----------------|
| Statutory vacation days | 25 days per year | 4 SS Semesterlagen |
| Intjanandearet (earning year) | April 1 - March 31 | 3 SS Semesterlagen |
| Semesteraret (vacation year) | April 1 - March 31 (the year following the earning year) | 3 SS Semesterlagen |
| Note: AB collective agreement | Uses calendar year (Jan 1 - Dec 31) for both earning and vacation year | AB SS 27 |

### 3.2 AB collective agreement vacation days (by age)

| Age | Days per year | Reference |
|-----|---------------|-----------|
| Up to 39 | 25 days | AB 25 SS 27 |
| 40-49 | 31 days | AB 25 SS 27 |
| 50+ | 32 days | AB 25 SS 27 |

### 3.3 Semesterlon calculation

**Sammaloneregeln (same-pay rule)** - most common for monthly salaried employees:
- Semesterlon = current monthly salary + fixed pay supplements (unchanged during vacation)
- Plus: semestertillagg of **0.43%** of monthly salary per vacation day taken (for monthly employees)
- Plus: **12%** of total variable pay components (overtime, OB, jour, beredskap, provision, bonus) earned during the earning year

**Procentregeln (percentage rule)** - used when:
- Variable pay > 10% of total pay
- Employee changed sysselsattningsgrad (employment rate) between earning and vacation year
- Formula: **12%** of total gross pay (fixed + variable) during the earning year

**Semesterersattning (vacation pay for hourly/departing employees):**
- **12%** of total gross pay during employment period

### 3.4 Sparade dagar (saved vacation days)

| Rule | Detail | Legal reference |
|------|--------|-----------------|
| Right to save | Days above 20 paid days per year may be saved | 18 SS Semesterlagen |
| Maximum saving period | Must be used within 5 years from the end of the vacation year when saved | 18 SS Semesterlagen |
| Saved days payout | At current salary level when taken (not earning-year salary) | 18 SS Semesterlagen |

### HR System UI Requirements

- **Age-based entitlement engine**: Auto-calculate vacation days based on age (AB thresholds) and update on birthdays
- **Dual-period tracking**: Support both statutory (Apr-Mar) and AB (Jan-Dec) earning/vacation year definitions
- **Semestertillagg calculator**: 0.43% x monthly salary x days taken
- **Variable pay integration**: 12% calculation on aggregated variable pay components
- **Sparade dagar ledger**: Track saved days with 5-year expiry dates and alerts before expiration
- **Semesterersattning**: Auto-calculate 12% payout at employment termination

---

## 4. Sjuklonelagen (SFS 1991:1047)

### 4.1 Employer sick pay period (dag 1-14)

| Rule | Value | Legal reference |
|------|-------|-----------------|
| Sjukloneperiod | Day 1-14 of sick period | 7 SS Sjuklonelagen |
| Karensavdrag (deduction) | 20% of average weekly sick pay | 6 SS Sjuklonelagen |
| Sjuklon day 2-14 | 80% of lost earnings | 6 SS Sjuklonelagen |
| Lakarintyg (medical certificate) required | From day 8 (calendar day 8 of absence) | 10 a SS Sjuklonelagen |
| Max karensavdrag per 12 months | 10 deductions; the 11th sick period has no karensavdrag | 6 SS Sjuklonelagen |

### 4.2 Karensavdrag calculation formula

```
Hourly rate = (Monthly salary x 12) / (52 x weekly working hours)
Sick pay per hour = Hourly rate x 80%
Average weekly sick pay = Sick pay per hour x weekly working hours
Karensavdrag = Average weekly sick pay x 20%
```

**Example:** Monthly salary 30,000 SEK, 40h/week:
- Hourly rate = (30,000 x 12) / (52 x 40) = 173.08 SEK
- Sick pay/hour = 173.08 x 0.80 = 138.46 SEK
- Weekly sick pay = 138.46 x 40 = 5,538.46 SEK
- Karensavdrag = 5,538.46 x 0.20 = **1,107.69 SEK**

### 4.3 Forsakringskassan takes over from day 15

| Rule | Value | Reference |
|------|-------|-----------|
| Sjukpenning from FK | From day 15 onwards | SFB 27 kap |
| Sjukpenning level | Approximately 80% of SGI (sjukpenninggrundande inkomst) | SFB 28 kap |
| SGI maximum 2025 | 588,000 SEK/year (10 x prisbasbelopp 58,800) | SFB |
| SGI maximum 2026 | 592,000 SEK/year (10 x prisbasbelopp 59,200) | SFB |
| Max sjukpenning per day 2026 | Approximately 1,259 SEK/day before tax | Forsakringskassan |
| Employer notification to FK | Within 7 days after sjukloneperiod ends | SFB |

### 4.4 AB collective agreement supplement

| Rule | Value | Reference |
|------|-------|-----------|
| Employer supplement day 15-90 | 10% of pay (so employee gets ~90% total) | AB |
| Employer supplement day 91-365 | Reduced to approximately 0% (only FK pays) | AB |

> **FLAG:** The exact AB supplement percentages for day 15+ may vary; verify against current AB 25 text.

### HR System UI Requirements

- **Sick period tracker**: Auto-track consecutive sick days, distinguish karensdag, sjukloneperiod (day 1-14), and FK period (day 15+)
- **Karensavdrag calculator**: Implement the 20%-of-weekly-sick-pay formula, handle part-time and irregular schedules
- **10-karens rule**: Count karensavdrag in rolling 12-month window; suppress deduction on 11th period
- **Lakarintyg reminder**: Alert employee and manager that medical certificate required from day 8
- **FK notification trigger**: Alert payroll to submit sick notice to Forsakringskassan by day 21 (day 14 + 7 days)
- **Sjukpenning integration**: Interface with Forsakringskassan for sjukpenning coordination from day 15

---

## 5. Foraldraledighetslag (SFS 1995:584)

### 5.1 Foraldrapenning structure

| Component | Days | Level | Reference |
|-----------|------|-------|-----------|
| Total foraldrapenning days per child | 480 days | - | SFB 12 kap |
| Sjukpenningniva days | 390 days | ~80% of SGI (up to ceiling) | SFB 12 kap |
| Lagstaniva days | 90 days | 180 SEK/day (2025) | SFB 12 kap |
| Reserved days per parent | 90 days (sjukpenningniva) | Cannot be transferred | SFB 12 kap 17 SS |
| Transferable days | Remaining 300 days can be shared | Flexible | SFB 12 kap |
| Transfer to non-parent | Up to 45 days can be transferred to non-partner/non-guardian | From July 2024 | SFB 12 kap |
| Dubbeldagar (both parents off simultaneously) | Up to 60 days for children under 15 months | From July 2024 | SFB 12 kap |

### 5.2 Allocation between two parents (standard)

| Parent | Sjukpenningniva | Lagstaniva | Total |
|--------|-----------------|------------|-------|
| Parent 1 | 195 days | 45 days | 240 days |
| Parent 2 | 195 days | 45 days | 240 days |
| Of which reserved (non-transferable) | 90 days each | - | 180 days total |

### 5.3 AB collective agreement supplement (Foraldralon)

| Rule | Value | Condition | Reference |
|------|-------|-----------|-----------|
| Foraldrapenningtillagg | 10% of salary per calendar day | Max 180 calendar days | AB 25 |
| Qualification | Must have been employed >= 180 consecutive calendar days before leave | - | AB 25 |
| Result for employee | ~90% of salary during first 180 days (80% FK + 10% employer) | Up to SGI ceiling | AB 25 |

### 5.4 Right to leave (employer obligations)

| Type of leave | Duration | Reference |
|---------------|----------|-----------|
| Full leave | Until child is 18 months | 5 SS FL |
| Reduced hours (75%, 50%, 25%) | Until child turns 8 or finishes year 1 of school | 7 SS FL |
| Breastfeeding leave | As needed | 4 SS FL |

### HR System UI Requirements

- **Parental leave planner**: Track 480 days allocation between parents, show reserved vs transferable
- **Qualification checker**: Verify 180-day employment prerequisite for foraldrapenningtillagg
- **Employer supplement calculator**: 10% of salary x calendar days, max 180 days
- **Return-to-work scheduling**: Support reduced hours (75/50/25%) with corresponding salary adjustment
- **FK integration**: Interface for reporting parental leave periods to Forsakringskassan
- **Leave balance dashboard**: Show remaining days per parent with visual breakdown

---

## 6. Kollektivavtal AB (Allmanna bestammelser) for regions

### 6.1 OB-tillagg (unsocial hours supplements) - AB 25, Bilaga R

Rates effective from 2025-04-01 (reference amounts from AB/Nacka kommun publication, representative for the agreement):

| Category | Time period | Rate (approx. SEK/hour) | Reference |
|----------|-------------|--------------------------|-----------|
| O-tillagg D (weekday evening) | Mon-Fri 19:00-22:00 (Fri from 17:00 per AB 25) | ~26.50 | Bilaga R AB 25 |
| O-tillagg C (weekday night) | Mon-Thu 22:00-24:00, Tue-Fri 00:00-06:00 | ~58.60 | Bilaga R AB 25 |
| O-tillagg B (weekend) | Fri 22:00 - Mon 06:00 | ~68.30 | Bilaga R AB 25 |
| O-tillagg A (helgdag/storhelg) | Helgdag 06:00 - day after helgdag 06:00 | ~131.20 | Bilaga R AB 25 |
| Night before Sat/Sun/helgdag (from 2026-04-01) | 22:00-06:00 | +30% increase on top | Bilaga R AB 25 |

> **FLAG:** The exact kronor amounts above are approximate based on published municipal examples (pre-2025-04-01 rates + 3.4% increase). Exact amounts should be verified against the official AB 25 Bilaga R from SKR. The rates are centrally negotiated and apply uniformly across all regions. All OB rates increase by 3.4% from 2025-04-01 and 3.0% from 2026-04-01.

**Key change in AB 25:** Friday evening OB (O-tillagg B) now starts at 17:00 instead of 19:00 from 2025-04-01.

### 6.2 Overtidsersattning (overtime compensation)

| Type | Rate | When |
|------|------|------|
| Enkel overtid (simple overtime) | 180% of hourly rate | First 2 hours before/after regular working time |
| Kvalificerad overtid (qualified overtime) | 240% of hourly rate | After the first 2 overtime hours, weekends, public holidays |

**AB 25 change:** From 2025-04-01, part-time employees get the same overtime compensation as full-time when working beyond their scheduled hours.

### 6.3 Jour and beredskap (on-call and standby)

| Type | Base compensation | Reference |
|------|-------------------|-----------|
| Jour (on-site on-call) | Percentage of hourly rate (varies by time) | AB 25 SS 23 |
| Beredskap (off-site standby) | Percentage of hourly rate; higher rate triggered after 125 hours cumulative (lowered from 130h) | AB 25 SS 23 |
| Beredskap higher rate | 28% of hourly rate (triggered at 125h threshold) | AB 25 SS 23 |
| Jour/beredskap minimum | Compensation for at least 8 hours per 24-hour period minus active work hours | AB 25 SS 23 |

> **FLAG:** Exact jour/beredskap kronor amounts depend on individual salary. The AB defines them as percentages of the employee's hourly rate. Verify exact percentages in official AB 25 text.

### 6.4 Semester (vacation) per AB 25

| Age bracket | Vacation days | Reference |
|-------------|---------------|-----------|
| Up to 39 years | 25 days | AB 25 SS 27 |
| 40-49 years | 31 days | AB 25 SS 27 |
| 50+ years | 32 days | AB 25 SS 27 |

### 6.5 AKAP-KR Pension (Avgiftsbestamd KollektivAvtalad Pension)

| Component | Rate | Reference |
|-----------|------|-----------|
| Employer contribution on salary up to 7.5 IBB | 6.0% | AKAP-KR |
| Employer contribution on salary above 7.5 IBB (up to 30 IBB) | 31.5% | AKAP-KR |
| Extra age-based contribution (above 7.5 IBB) | 0.5% - 8.5% depending on birth year | AKAP-KR |
| Inkomstbasbelopp (IBB) 2025 | 80,600 SEK | Government decree |
| Inkomstbasbelopp (IBB) 2026 | 83,400 SEK | Forordning 2025:1002 |
| 7.5 IBB threshold (monthly) 2025 | 50,375 SEK/month | Calculated |
| 7.5 IBB threshold (monthly) 2026 | 52,125 SEK/month | Calculated |

### HR System UI Requirements

- **OB calculation engine**: Apply correct O-tillagg category based on exact time of work (D/C/B/A)
- **OB rate table**: Configurable rate table to update with annual increases (3.4% from 2025-04-01, 3.0% from 2026-04-01)
- **Overtime calculator**: Distinguish enkel (180%) vs kvalificerad (240%) based on overtime duration and day type
- **Jour/beredskap tracker**: Log hours, calculate compensation based on salary percentage, track cumulative threshold (125h for higher rate)
- **Pension engine**: Split salary at 7.5 IBB threshold, apply 6% below and 31.5% above; update IBB annually
- **Age-based vacation**: Auto-adjust entitlement when employee turns 40 and 50

---

## 7. Arbetsgivaravgifter (Employer social contributions) 2025-2026

### 7.1 Standard rates

| Year | Full rate | Composition |
|------|-----------|-------------|
| 2025 | 31.42% | Alderspensionsavgift 10.21% + sjukforsakringsavgift 3.55% + foraldraforsakringsavgift 2.60% + efterlevandepensionsavgift 0.60% + arbetsmarknadsavgift 2.64% + arbetsskadeavgift 0.20% + allman loneavgift 11.62% |
| 2026 | 31.42% | Same composition |

### 7.2 Reduced rates

| Category | Rate | Condition | Period |
|----------|------|-----------|--------|
| Young employees (19-22) | 20.81% (2/3 of full) | Born after Dec 31 of year - 23; on salary up to 25,000 SEK/month. Full rate on amounts above 25,000 | 2026-04-01 to 2027-09-30 |
| Elderly employees (67+) | 10.21% (only alderspensionsavgift) | Born in year of assessment - 67 or earlier (i.e., turns 68+ during the year). From 2026: age 67+ at year start | Ongoing |

> **Note on age thresholds:**
> - 2025: Reduced rate for those born 1957 or earlier (turning 68+ during 2025) -- only alderspensionsavgift 10.21%
> - 2026: Reduced rate for those born 1959 or earlier (turning 67+ during 2026, threshold lowered to 67) -- only alderspensionsavgift 10.21%
> - Youth discount: Applies from 2026-04-01; for employees who turn 19-22 during the calendar year

### 7.3 Key base amounts for reference

| Amount | 2025 | 2026 |
|--------|------|------|
| Prisbasbelopp (PBB) | 58,800 SEK | 59,200 SEK |
| Inkomstbasbelopp (IBB) | 80,600 SEK | 83,400 SEK |
| SGI ceiling (10 x PBB) | 588,000 SEK | 592,000 SEK |

### HR System UI Requirements

- **Employer contribution engine**: Auto-calculate correct rate per employee based on age
- **Age threshold logic**: Check birth year against calendar year to determine full/reduced/youth rate
- **Split calculation for youth**: Apply 20.81% on first 25,000 SEK, 31.42% on remainder
- **Annual rate table**: Configurable table updated each January 1
- **Payroll reporting**: Generate AG/A (arbetsgivardeklaration) with correct age-differentiated rates
- **Cost simulation**: Enable managers to see total employment cost including contributions

---

## 8. GDPR and Swedish Data Protection

### 8.1 Legal basis for processing employee data

| Purpose | Legal basis | GDPR Article |
|---------|-------------|--------------|
| Employment contract execution | Performance of contract | Art 6.1(b) |
| Salary, tax, social insurance reporting | Legal obligation | Art 6.1(c) |
| Scheduling, attendance tracking | Legitimate interest | Art 6.1(f) |
| IT security, access logs | Legitimate interest | Art 6.1(f) |
| Health data (sick leave, rehab) | Employment law obligation | Art 9.2(b) |
| Trade union membership | Employment law obligation (for collective agreement administration) | Art 9.2(b) |

**Critical:** Consent (Art 6.1(a)) is generally NOT a valid legal basis for employee data, because the employment relationship creates a power imbalance that undermines "freely given" consent (per IMY guidance and EDPB guidelines).

### 8.2 Retention periods (gallringsfrister)

| Data type | Retention period | Legal basis for retention |
|-----------|-----------------|--------------------------|
| Payroll records, salary specifications | 7 years after financial year | Bokforingslagen (SFS 1999:1078) 7 kap 2 SS |
| Salary claims documentation | Up to 10 years | Preskriptionslagen (10-year claim period) |
| Employment contracts | Duration of employment + 10 years | Preskriptionslagen |
| Sick leave records | Duration + 2 years (for employment law claims) | Preskriptionslagen (labor law 2-year) |
| Misconduct documentation | Duration + 2 years (up to 10 years if pay-related) | Preskriptionslagen |
| Recruitment/application data (non-hired) | 2 years after recruitment decision | Diskrimineringslagen (2-year complaint window) |
| Training/competence records | Duration + 1 year | Proportionality principle |
| Working time records | 7 years (if part of payroll) or 2 years (standalone) | Bokforingslagen / Preskriptionslagen |
| Health/rehabilitation data | Delete when purpose fulfilled, max 2 years after employment ends | GDPR minimization principle |
| Performance reviews | Duration of employment + 1-2 years | Proportionality principle |

### 8.3 Data subject rights for employees

| Right | GDPR Article | Response deadline | Notes for HR system |
|-------|-------------|-------------------|---------------------|
| Right of access (registerutdrag) | Art 15 | 1 month (extendable by 2 months for complex requests) | System must be able to export all data about an employee |
| Right to rectification | Art 16 | Without undue delay | Employees can correct inaccurate personal data |
| Right to erasure ("right to be forgotten") | Art 17 | 1 month | Limited: cannot erase data required by law (e.g., bokforingslagen records) |
| Right to data portability | Art 20 | 1 month | Export employee data in machine-readable format |
| Right to restriction of processing | Art 18 | Without undue delay | Must be able to "freeze" processing while disputes are resolved |
| Right to object | Art 21 | Without undue delay | Limited applicability in employment context |

### 8.4 Special category data (Article 9)

Health data in an HR system includes:
- Sick leave records (dates, durations)
- Medical certificates (lakarintyg)
- Rehabilitation plans
- Work capacity assessments
- Disability accommodations

**Requirements:**
- Must be stored with enhanced security (access restrictions, encryption)
- Access limited to those with legitimate need (HR, direct manager for operational purposes)
- Separate storage from general HR data recommended
- Explicit documentation of why health data is necessary (Art 9.2(b) - employment law obligations)

### 8.5 Public sector specifics (regions are public authorities)

| Requirement | Detail | Reference |
|-------------|--------|-----------|
| Data Protection Officer (DPO) required | Mandatory for public authorities | Art 37.1(a) GDPR |
| DPIA required | For systematic monitoring or large-scale health data processing | Art 35 GDPR |
| Offentlighetsprincipen (principle of public access) | Personnel records may be subject to public access requests under Tryckfrihetsforordningen | TF 2 kap |
| Sekretess (confidentiality) | Health data protected under OSL 21:1, 39:1-2 | Offentlighets- och sekretesslagen |
| Legal basis preference | Public authorities should use "legal obligation" or "task in public interest" rather than "legitimate interest" | Art 6.1(e) GDPR |

### HR System UI Requirements

- **Data access request workflow**: Built-in process for handling Art 15 requests with 1-month deadline tracking
- **Erasure request handler**: Evaluate which data can be deleted vs must be retained (with documented legal basis)
- **Data export (portability)**: Export employee data in structured, machine-readable format (JSON/CSV)
- **Role-based access control**: Strict access levels -- health data only visible to authorized HR personnel
- **Retention engine**: Auto-flag data approaching retention deadline; scheduled deletion with audit trail
- **Audit logging**: Track all access to personal data (who, when, what, why)
- **Consent management**: Track any consent given (for non-employment-contract purposes) with withdrawal capability
- **Privacy dashboard**: Show each employee what data is held about them (self-service registerutdrag)
- **DPIA documentation**: Built-in support for documenting Data Protection Impact Assessments
- **Encryption**: Health data and sensitive data encrypted at rest and in transit

---

## Summary of Key Numeric Thresholds for System Configuration

| Parameter | Value |
|-----------|-------|
| SAVA conversion threshold | 12 months in 5 years |
| Vikariat conversion threshold | 24 months in 5 years |
| Chain gap limit | 6 months |
| Foretradesratt SAVA threshold | 9 months in 3 years |
| Foretradesratt expiry | 9 months |
| Turordning exemptions | 3 per round, 3-month quarantine |
| Regular working hours | 40h/week |
| Dygnsvila | 11h (9h healthcare exception) |
| Veckovila | 36h |
| Allman overtime | 200h/year, 48h/4 weeks |
| Extra overtime | 150h/year |
| Statutory vacation | 25 days |
| AB vacation 40-49 | 31 days |
| AB vacation 50+ | 32 days |
| Semestertillagg | 0.43%/day of monthly salary |
| Variable pay vacation | 12% |
| Saved vacation max storage | 5 years |
| Karensavdrag | 20% of weekly sick pay |
| Sjuklon rate | 80% of lost earnings |
| Lakarintyg required from | Day 8 |
| FK takeover | Day 15 |
| Max karensavdrag per year | 10 |
| Foraldrapenning total | 480 days |
| Reserved per parent | 90 days |
| Foraldralon (AB) | 10%, max 180 days |
| Employer contributions | 31.42% (full), 10.21% (67+), 20.81% (19-22 from 2026-04) |
| AKAP-KR below 7.5 IBB | 6% |
| AKAP-KR above 7.5 IBB | 31.5% |
| IBB 2025 | 80,600 SEK |
| IBB 2026 | 83,400 SEK |
| PBB 2025 | 58,800 SEK |
| PBB 2026 | 59,200 SEK |
| GDPR access request deadline | 1 month |
| Recruitment data retention | 2 years |
| Payroll record retention | 7 years |

---

## Items Flagged for Verification

1. **OB-tillagg exact kronor amounts**: The amounts listed in Section 6.1 are approximations based on publicly available municipal examples. The exact AB 25 Bilaga R amounts should be obtained from SKR's official published document.

2. **Jour/beredskap exact percentages**: AB defines these as percentages of hourly rate but exact percentage tiers are not fully confirmed from public web sources. Obtain from AB 25 full text.

3. **AB sick pay supplement day 15-90**: The 10% employer supplement from day 15 is mentioned in multiple sources for the AB agreement but the exact duration and tapering should be verified against AB 25 SS 28.

4. **Youth employer contribution start date**: The 19-22 youth discount was announced but takes effect 2026-04-01. Monitor for any changes before implementation.

5. **Age threshold for reduced employer contributions**: Changed from 65 to 67 effective 2026 -- system needs to handle the transition correctly for the 2025-2026 boundary.

6. **Foraldralon 10% AB supplement**: Some sources indicate 90% of salary (which implies 10% employer + 80% FK), but the exact calculation may differ for income above SGI ceiling. Verify against AB 25.

7. **AKAP-KR extra age-based contribution**: The 0.5-8.5% extra contribution above 7.5 IBB varies by birth year. The exact table by birth year cohort should be obtained from the AKAP-KR agreement text.

---

*Sources compiled from Swedish government databases (riksdagen.se), Skatteverket, Forsakringskassan, SKR, IMY, and authoritative legal commentary sites. All figures should be validated against primary legal texts before implementation in the HR system.*
