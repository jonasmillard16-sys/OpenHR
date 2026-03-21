# Enterprise HR Features Research: Detailed Analysis for OpenHR FOSS Equivalents

**Date:** 2026-03-20
**Purpose:** Deep-dive into enterprise HR features identified as gaps vs. Workday/SAP/Oracle, to inform OpenHR module design
**Status:** Research only -- no code changes

---

## 1. Contingent Workforce / Vendor Management System (VMS)

### What Workday VNDLY Actually Does

Workday VNDLY is a dedicated Vendor Management System that manages the entire lifecycle of contingent (non-permanent) workers sourced through staffing vendors. It pairs with Workday HCM to provide "total workforce" visibility across both employees and contractors.

### Core Entities

| Entity | Description |
|--------|-------------|
| **Vendor** | Staffing agency or consulting firm. Has profile, performance scores, preferred/approved status, geographic coverage, specializations. |
| **Vendor Contact** | Named representatives at each vendor who receive requisitions and submit candidates. |
| **Requisition / Job Request** | A request for contingent labor: role description, required skills, location, duration, budget, bill rate range. |
| **Rate Card** | Predefined billing rates per job category/skill/location. Managers set rate thresholds; VNDLY enforces them during requisition creation. Rate cards can be adjusted per market. |
| **Statement of Work (SOW)** | A project-based contract with defined deliverables, milestones, acceptance criteria, and fixed/T&M pricing. SOWs support redlining, comments, change orders, and approval workflows directly in the system. |
| **Contract** | Master Service Agreement (MSA) or specific engagement terms with a vendor. Tracks start/end dates, renewal terms, insurance requirements, compliance documents. |
| **Contingent Worker** | The actual person working. Linked to vendor, requisition, rate, assignment dates, location, manager, cost center. |
| **Assignment** | A contingent worker's placement: start date, end date, extension options, work location, bill rate, pay rate, overtime rules. |
| **Timesheet** | Contingent worker submits hours; manager approves; system calculates invoice amount based on rate card. |
| **Invoice** | Consolidated or per-worker invoicing from vendor, matched against approved timesheets and rate cards. |

### Key Workflows

1. **Requisition-to-Fill:** Hiring manager creates requisition -> routed to vendor(s) based on distribution rules -> vendors submit candidates -> manager reviews/interviews -> selects candidate -> system generates assignment/contract.
2. **SOW Management:** SOW created with deliverables/milestones -> vendor redlines -> negotiation in-system -> approval chain -> work begins -> milestone tracking -> deliverable acceptance -> payment.
3. **Onboarding/Offboarding:** Automated provisioning of system access when assignment starts; automatic revocation when it ends. Background check triggers, compliance document collection.
4. **Time & Invoice:** Worker submits time -> manager approves -> invoice auto-generated or vendor submits invoice -> 3-way match (timesheet vs. rate card vs. invoice) -> payment approval.
5. **Vendor Performance:** Track fill rate, time-to-fill, quality of candidates, invoice accuracy, compliance adherence. Scorecards drive future vendor selection.
6. **Worker Classification:** Rules engine to help determine whether a worker should be classified as contractor vs. employee (compliance with labor laws like IR35, AB5).

### AI Features (2025-2026)

- **Contingent Sourcing Agent**: Combines VNDLY with HiredScore AI to find temp staff faster via AI-powered candidate matching.
- **Control Plane Vision**: VNDLY evolving from traditional VMS into an AI-enabled orchestration layer that interprets data, applies policies, and automates actions across HR/payroll/ATS systems.

### OpenHR FOSS Equivalent Considerations

- Most Swedish public sector orgs use "bemanningsforetag" (staffing agencies) for healthcare temps (ssk, usk, lakare)
- Key entities needed: Bemanningsforetag (vendor), Ramavtal (framework agreement), Inhyrd personal (contingent worker), Timrapport (timesheet), Fakturamatchning (invoice matching)
- Rate cards are critical in healthcare where regional framework agreements set max hourly rates
- Must track AB/HOK compliance for contingent workers too (working time rules apply)

---

## 2. Strategic Compensation Management

### What a Full Compensation Cycle Looks Like

In Workday/SAP, compensation management is divided into **base compensation** (ongoing salary administration) and **Advanced Compensation** (annual review cycles).

### Core Entities

| Entity | Description |
|--------|-------------|
| **Compensation Plan** | A named plan type: Salary, Hourly, Merit, Bonus, Commission, Stock, Long-term Cash, Allowance, One-time Payment. Each has eligibility rules, effective dates, and currency. |
| **Compensation Grade / Band** | Salary ranges (min/mid/max) per grade level. Used to position employees within bands and identify outliers. |
| **Compensation Package** | Bundle of plans assigned to a worker (e.g., "Senior Engineer Package" = salary plan + annual bonus + stock). |
| **Eligibility Rule** | Criteria determining who participates in a plan: job level, employment type, hire date, location, performance rating, custom criteria. |
| **Merit Plan** | Annual base pay increase program. Defines target % increase, budget pool calculation method, performance-to-merit mapping matrix. |
| **Bonus Plan** | Variable pay program. Defines target % of salary, bonus pool calculation, individual/team/org performance factors, payout schedule. |
| **Equity/Stock Plan** | Stock grant program. Grant types (RSU, ISO, NSO), vesting schedule, cliff period, grant value calculation. |
| **Compensation Review Process** | The container for an annual cycle encompassing merit, bonus, and/or stock awards. |
| **Budget/Pool** | Total compensation budget allocated to an organization. Can be calculated bottom-up (sum of individual targets) or top-down (fixed amount distributed). |
| **Total Rewards Statement** | A per-employee document showing total value: base salary, bonus paid, equity value, benefits value, employer tax contributions, pension, PTO value, perks. |

### Salary Review Cycle Workflow (Workday)

1. **Initiation:** HR configures the compensation review process: selects plans (merit, bonus, stock), sets eligibility rules, defines timeline milestones, assigns roles.
2. **Budget Calculation:**
   - **Bottom-up:** System calculates individual target amounts based on plan rules (e.g., 3% merit target x each employee's salary), then sums to org-level pools.
   - **Top-down:** Executive sets total budget per division; system distributes down the hierarchy.
3. **Manager Proposal Phase:** Managers see their team with current salary, compa-ratio (salary vs. midpoint), performance rating, tenure, and budget remaining. They propose individual awards.
4. **Collaboration:** Workday uses "shared participation" -- multiple managers can propose simultaneously. A first-line manager proposes; the next-level manager can adjust and also propose for indirect reports.
5. **Rollup & Approval:** Proposals roll up the org hierarchy. Each level can review, adjust, and approve. Budget guardrails flag over-budget proposals.
6. **Pay Equity Check:** Built-in analytics flag pay anomalies -- e.g., gender pay gaps, outliers below band minimum. Managers see "pay vs. peers" comparisons.
7. **Finalization:** HR reviews all proposals, runs compliance checks, and finalizes. System generates effective-date salary changes and bonus payments.
8. **Communication:** Total Rewards Statements generated for each employee. Manager notification templates for comp conversations.

### Total Rewards Statement Sections

1. **Employee Info**: Name, role, department, employment date
2. **Base Compensation**: Annual salary, hourly rate, pay frequency
3. **Variable Pay**: Bonus target and actual payout, commission
4. **Equity**: Stock grants, vesting schedule, estimated current value
5. **Benefits**: Employer-paid health insurance premium, dental, vision, life insurance, disability
6. **Retirement/Pension**: Employer pension contribution (in Sweden: AKAP-KR ~4.5%+), matching
7. **Paid Time Off**: Monetary value of vacation days, sick leave
8. **Employer Taxes**: Arbetsgivaravgifter (31.42%), special payroll tax on pension
9. **Other Perks**: Wellness benefit (friskvardsbidrag), phone allowance, education budget, transit
10. **Total Package Value**: Sum of all above

### OpenHR FOSS Equivalent Considerations

- Swedish salary review ("lonerevision") follows collective agreements (AB/HOK) with specific rules
- Union negotiation phase ("forhandling") is legally required before individual distribution
- Lonekartering (pay equity mapping per Diskrimineringslagen) is mandatory every 3 years
- OpenHR already has SalaryReview module with rounds/proposals -- needs: budget allocation, compa-ratio display, pay equity integration, total rewards statement generation

---

## 3. Advanced Benefits Administration

### Eligibility Rules Engine

Enterprise HR systems use a **rules engine** to automatically determine which benefits an employee is eligible for, based on multiple criteria evaluated in combination:

| Criteria Type | Examples |
|---------------|----------|
| **Employment Type** | Full-time, part-time, temporary, seasonal, contractor |
| **Work Schedule** | Minimum hours/week (e.g., 20+ hours for benefits eligibility) |
| **Employment Duration** | Waiting period (e.g., eligible after 30/60/90 days) |
| **Job Level/Grade** | Executive plans only for certain grades |
| **Location/Country** | Country-specific plans, state-specific requirements |
| **Age** | Age-based eligibility (e.g., pension at 25+, Medicare at 65) |
| **Union Membership** | Different plans per collective agreement |
| **Dependents** | Dependent coverage rules, age limits for children (typically 26) |
| **Compensation Level** | Higher-tier plans for employees above salary threshold |
| **Custom Fields** | Organization-specific criteria |

Rules are combined with AND/OR logic. An employee can be in multiple **benefit groups** simultaneously. The system re-evaluates eligibility whenever employee data changes (promotion, transfer, status change).

### Life Events (Qualifying Events)

A **life event** is a significant change that triggers the right to modify benefit elections outside the annual open enrollment window. Enterprise systems define these as configurable event types:

| Life Event | Typical Benefit Changes Allowed |
|------------|-------------------------------|
| **Marriage** | Add spouse to health/dental/vision, change coverage level |
| **Divorce** | Remove ex-spouse, change coverage level, change beneficiaries |
| **Birth/Adoption of Child** | Add dependent, increase life insurance, change FSA |
| **Death of Dependent** | Remove dependent, change coverage, change beneficiaries |
| **Loss of Other Coverage** | Enroll in employer plan (e.g., spouse lost their employer coverage) |
| **Gain of Other Coverage** | Drop employer plan or reduce coverage |
| **Employment Status Change** | FT->PT or PT->FT changes eligibility |
| **Job Change/Transfer** | May change eligible plans based on new location/grade |
| **Relocation** | Country/region-specific plans |
| **Turning a Milestone Age** | Medicare eligibility at 65, dependent aging out at 26 |
| **Return from Leave** | Re-enrollment after extended leave |
| **Disability/Medical Event** | Change coverage during disability |

Key rules:
- Changes must be **consistent** with the life event (can't add dental because of a marriage)
- Typically a **30-60 day window** after the event to make changes
- System requires **documentation** (marriage certificate, birth certificate, etc.)
- Missed deadline = wait until next annual enrollment

### End-to-End Enrollment Workflow

1. **Trigger**: New hire, annual open enrollment period, or qualifying life event
2. **Eligibility Determination**: Rules engine evaluates employee against all plan eligibility rules; presents only eligible plans
3. **Plan Presentation**: Employee sees available plans grouped by category (medical, dental, vision, life, disability, FSA/HSA, retirement)
4. **Coverage Selection**: Employee chooses plan tier (employee-only, employee+spouse, employee+children, family), selects specific plan option within tier
5. **Dependent Management**: Add/verify dependents, upload documentation (SSN/birth cert), designate beneficiaries
6. **Cost Display**: System shows employee premium cost per pay period, employer contribution, total plan cost
7. **Confirmation & E-signature**: Employee reviews all elections, confirms, signs electronically
8. **Effective Date Calculation**: System determines coverage start date based on event type rules
9. **Payroll Integration**: Deduction records automatically created for payroll processing
10. **Carrier Feed**: Enrollment data sent to insurance carriers via EDI/API (carrier file feeds)
11. **Benefits Statement**: Annual statement showing total benefits value (part of Total Rewards Statement)

### Benefits Statement Contents

- Plan name and coverage level for each enrolled benefit
- Employee premium contribution (per pay period and annual)
- Employer premium contribution (per pay period and annual)
- Covered dependents per plan
- Beneficiary designations
- Coverage effective/end dates
- Claims summary (if integrated with carrier)
- Total employer investment in employee benefits

### OpenHR FOSS Equivalent Considerations (Swedish Context)

Swedish benefits are different from US-model -- fewer plan choices, more mandated by law/collective agreement:
- **Pension (AKAP-KR/KAP-KL)**: Mandatory employer contributions, not employee-choice
- **Friskvardsbidrag**: Wellness benefit (5,000 SEK tax-free), employee submits receipts
- **Insurance (AFA)**: TGL (group life), AGS (health insurance) -- mandated by collective agreement
- **Parental Leave Supplement (foraldralon)**: 10% salary supplement for 180 days per AB
- **Semester**: 25-32 days per AB based on age -- not a "benefit election" but a legal right
- Life events in Swedish context: birth/adoption triggers foraldraledighet planning, FK coordination
- Eligibility rules simpler but still needed: employment type (tillsvidare vs visstid), working hours (heltid/deltid), age brackets for pension

---

## 4. Internal Talent Marketplace / Skills Intelligence

### What Workday Skills Cloud Does

Skills Cloud is a **universal skills ontology** -- a machine-learning-powered database of 50,000+ skills with relationships mapped between them using graph technology.

### Core Capabilities

**a) Skills Ontology & Graph**
- 50,000+ standardized skills, constantly updated
- Graph technology maps relationships: "Excel" relates to "Data Analysis," "Reporting," "Financial Modeling"
- Skills are decomposed into components and connected to other categories (job families, industries, certifications)
- Deduplication: "MS Excel," "Microsoft Excel," "Excel" all resolve to one skill

**b) Skills Inference (LLM-Powered)**
- Uses a Large Language Model to infer skills from:
  - Job titles (e.g., "Senior Nurse" -> clinical assessment, patient care, medication administration, triage)
  - Resume text / CV parsing
  - Job descriptions
  - Learning history (completed courses imply skills)
  - Job history (previous roles imply skills gained)
  - Public feedback/endorsements from colleagues
- Suggestions are surfaced to employees: "Based on your role as Sjukskoterska, you likely have these skills..."
- Employees can accept, reject, or add skills to their profile

**c) Talent Marketplace / Career Hub**
- Internal job marketplace where open positions are matched to employees based on skill fit
- **Career Hub**: Personalized portal showing:
  - Recommended open roles based on skill match %
  - Recommended learning to close skill gaps for desired roles
  - Recommended mentors/connections based on skill similarity
  - Career path visualization (current role -> possible future roles)
- Employees can express interest in roles with one click
- Managers can search internal talent by skill when filling positions

**d) Mentorship Matching**
- System suggests mentor-mentee pairings based on:
  - Skill similarity/complementarity
  - Career path alignment (mentor has the role mentee aspires to)
  - Location/department preferences
  - Availability
- Built-in mentorship program management: goals, meeting tracking, feedback

**e) Gig/Project Assignments**
- Short-term internal projects posted as "gigs"
- Employees matched based on skills, apply with one click
- Builds experience and skills outside regular role

### SAP SuccessFactors Opportunity Marketplace

SAP's equivalent system adds:
- **Talent Intelligence Hub**: AI-powered foundation with sophisticated skills ontology
- **Experiential Assignments**: Short-term projects, gigs, fellowships
- **Mentorships**: AI-matched mentor connections
- **Internal Job Postings**: Career advancement opportunities
- **Auto-populated Skills**: When new assignments are created, required skills are auto-populated from job role definitions
- **Mobile Access**: Employees and assignment owners can manage opportunities from mobile devices
- **Cross-location Mobility**: Employees can find assignments across multiple locations
- **Priority Matching**: Assignments prioritized in recommendations when required skills match employee's passionate/critical skills

### OpenHR FOSS Equivalent Considerations

- Build a skills taxonomy relevant to Swedish healthcare: medical specializations, certifications (legitimationer), language skills, clinical competencies
- Inference from befattning (position type) + utbildning (education) + previous employments
- Internal job board ("Intern jobbmarknad") already identified as needed feature
- Mentor matching particularly valuable in healthcare for specialist development
- Career path mapping: USK -> SSK -> Specialistsjukskoterska -> Verksamhetschef
- Could use open-source NLP/ML models (Hugging Face, spaCy) for skills extraction from Swedish text

---

## 5. Enterprise Analytics / BI

### Standard HR KPIs

Enterprise HR systems typically provide 40-60 pre-built KPIs organized into categories:

**Workforce Composition**
- Total headcount (by org, location, employment type, gender, age bracket)
- FTE count vs. headcount
- Contingent worker ratio
- New hires this period
- Departures this period
- Net headcount change
- Span of control (direct reports per manager)

**Turnover & Retention**
- Monthly/annual turnover rate (voluntary, involuntary, total)
- Turnover by tenure band, department, job family
- First-year turnover rate (early attrition)
- Retention rate
- Average tenure
- Regretted vs. non-regretted attrition
- Exit reason analysis

**Recruitment**
- Time-to-fill (days from requisition to acceptance)
- Time-to-hire (days from application to hire)
- Cost-per-hire
- Source effectiveness (which channels produce hires)
- Offer acceptance rate
- Quality of hire (performance after 12 months)
- Pipeline conversion rates (application -> screen -> interview -> offer -> hire)

**Compensation**
- Average salary by grade/role/gender
- Compa-ratio distribution (salary vs. midpoint)
- Pay equity ratios (gender, ethnicity)
- Compensation budget utilization
- Bonus payout distribution
- Total rewards cost per employee

**Absence & Wellbeing**
- Sick leave rate (short-term <14 days, long-term >14 days)
- Bradford Factor (frequency x duration^2 per person)
- Absence cost
- Return-to-work rate after long-term sick
- Workers' compensation claims

**Learning & Development**
- Training hours per employee
- Training spend per employee
- Mandatory training compliance rate
- Certification expiry rate
- Internal promotion rate (vs. external hire rate)

**Workforce Planning**
- Retirement eligibility forecast (who can retire in 1/3/5 years)
- Succession pipeline coverage (% of key positions with identified successors)
- Skills gap analysis (required vs. available skills per role)
- Headcount forecast vs. budget

### Predictive Models

**a) Attrition/Flight Risk Prediction**
- Inputs: tenure, performance rating, compensation (compa-ratio), manager effectiveness, engagement survey scores, overtime hours, leave patterns, time since last promotion, commute distance, job market conditions
- Output: Risk score (0-100%) per employee, color-coded (green/yellow/red)
- Models: Gradient boosting (XGBoost/CatBoost), logistic regression, random forest
- Reported accuracy: 85-96% on enterprise datasets
- Key drivers displayed per employee (SHAP explainability): "Top factors: 2+ years since promotion, below-median salary, high overtime"
- Actionable recommendations: "Consider for promotion," "Discuss career development," "Review compensation"

**b) Diversity & Inclusion Analytics**
- Representation dashboard: % by gender, age, ethnicity across org levels
- Hiring funnel equity: conversion rates at each stage by demographic
- Promotion equity: promotion rates by demographic
- Pay equity: regression-adjusted pay gap analysis controlling for job, tenure, performance
- Inclusion index: from engagement survey questions on belonging, fairness, voice

**c) Headcount Forecasting**
- Statistical models project future headcount based on historical hire/attrition rates
- Scenario modeling: "What if attrition increases 5%?" "What if we freeze hiring?"
- Budget impact: projected labor cost under each scenario
- Retirement wave analysis: using age + pension eligibility data

**d) Labor Cost Simulation**
- Model total cost changes: salary increases, new hires, departures, overtime, shift differentials
- "What-if" scenarios: "What does 3% vs. 5% salary increase cost?" "What's the cost of 10 new nurses?"
- Include employer taxes (arbetsgivaravgifter), pension, benefits, OB-tillagg in total cost model

### Self-Service BI

"Self-service BI" means non-technical users (HR business partners, managers) can explore data without IT help:

- **Discovery Boards** (Workday): Drag-and-drop visualization builder. Select dimensions (org, location, time), measures (headcount, turnover), chart type. Drill down from org-level to team to individual.
- **Data Blending** (Workday Prism Analytics): Combine Workday data with external data (budget from finance, patient volume from clinical systems) using no-code transformation pipelines.
- **Saved Views / Dashboards**: Users create personal dashboards with pinned visualizations.
- **Scheduled Reports**: Auto-generate and email reports on a schedule (daily/weekly/monthly).
- **Export**: Always exportable to Excel/PDF for offline analysis.
- **Benchmarking**: Compare own metrics against industry/region averages (Workday benchmarking service).

### OpenHR FOSS Equivalent Considerations

- Open-source BI: Apache Superset, Metabase, or Redash for self-service dashboards
- Predictive models: Python (scikit-learn, XGBoost) microservice or embedded ML
- OpenHR already has Reporting module with analytics -- needs expansion to full KPI library
- Swedish-specific KPIs: sjukfranvarostatistik (sick leave) per Forsakringskassan format, LAS thresholds, ATL compliance rates
- SCB/SKR reporting already has adapters -- need to connect analytics data

---

## 6. Advanced Workforce Management (UKG/Dayforce Depth)

### What Makes UKG/Dayforce Deeper Than Basic Scheduling

**a) Demand Forecasting**
- AI/ML algorithms analyze historical patterns: patient volume (healthcare), customer traffic (retail), production orders (manufacturing)
- Granularity: forecasts down to **15-minute intervals**
- External factors: weather, events, holidays, seasonality
- Output: predicted staffing need per 15-min/30-min/1-hour slot, per location, per role/skill
- Automatic schedule generation: system creates optimal schedule from forecast + staff availability + rules

**b) Labor Optimization**
- Balances simultaneously:
  - Coverage requirements (minimum staff per shift per skill)
  - Labor budget (cost targets per period)
  - Employee preferences (preferred shifts, days off)
  - Fairness (equitable distribution of desirable/undesirable shifts)
  - Skills/certifications (right-qualified staff on each shift)
  - Overtime minimization
  - Compliance rules (max hours, rest periods)
- Uses mathematical optimization (constraint programming, linear programming)
- Can run "what-if" simulations: "What if we add a night shift?" "What if 3 nurses call in sick?"

**c) Real-Time Compliance Checking**
- **Before scheduling**: System prevents creation of non-compliant schedules (e.g., less than 11h rest between shifts per ATL/EU Working Time Directive)
- **During shift**: Real-time alerts when approaching limits (overtime threshold, consecutive days worked)
- **After shift**: Audit trail of all compliance events, violations flagged for review
- **Jurisdiction-aware**: Configurable rules per country/state/city:
  - Sweden: ATL (dygnsvila 11h, veckovila 36h, max overtime 200h/year)
  - US: FLSA overtime, state meal break rules, predictive scheduling laws (Oregon, Seattle, NYC)
  - EU: Working Time Directive 48h/week average
- **Predictive scheduling laws**: Fair workweek rules requiring advance schedule posting (typically 14 days), good-faith shift estimates, "predictability pay" for last-minute changes

**d) Fatigue Management**
- Rules engine tracks cumulative fatigue indicators:
  - Hours worked in rolling 24h/48h/7-day/28-day periods
  - Rest time between shifts (minimum gap enforcement)
  - Night shift limits (consecutive nights, total night shifts per period)
  - On-call hours as partial work hours
- Healthcare-specific: Nurse fatigue rules based on patient safety research
  - Max 12-hour shifts
  - Minimum 8-10 hours between shifts
  - No more than 3 consecutive night shifts without extended rest
  - Mandatory rest after extended on-call periods
- Alerts to managers before assigning fatigued workers
- Reporting: fatigue risk scores per employee, unit-level fatigue heatmaps

**e) Real-Time Staffing**
- Live dashboard showing: who's clocked in, who's late, who's absent, coverage gaps
- Automated callout: system contacts available staff (by preference order) when gap detected
- Shift marketplace: employees can pick up available shifts from a pool
- Self-scheduling: within parameters, employees choose their own shifts (increases satisfaction)

### Dayforce Continuous Calculation Engine

Dayforce's unique differentiator: a single continuous calculation engine that processes changes in real-time across HR, payroll, time, scheduling, and benefits simultaneously. When an employee clocks in, the system immediately understands payroll implications, overtime calculations, and compliance requirements -- no batch processing delays.

### OpenHR FOSS Equivalent Considerations

- OpenHR already has scheduling with OR-Tools optimization and ATL compliance checking
- Missing: demand forecasting (need patient volume or historical staffing data integration), fatigue scoring, real-time staffing dashboard with callout automation
- Swedish healthcare specifics: Socialstyrelsen guidelines on staffing ratios, jour/beredskap rules, AB section 13 working time rules
- Consider open-source optimization: Google OR-Tools (already used), OptaPlanner (Java-based)
- 15-minute granularity forecasting relevant for vardcentral/akutmottagning staffing

---

## 7. Employee Experience Platform (Oracle ME)

### Oracle ME Components

Oracle ME (My Experience) is a suite of interconnected tools within Oracle HCM:

**a) Oracle Journeys (Guided Workflows)**
- Step-by-step guided processes for any employee lifecycle event
- 30+ pre-built journey templates including:
  - New hire onboarding
  - Return from parental leave
  - Return from sick leave
  - Internal transfer/promotion
  - Relocation
  - Retirement preparation
  - Open enrollment
  - Performance review preparation
  - Manager onboarding (becoming a new manager)
- Each journey = sequence of **tasks** with:
  - Task description and instructions
  - Embedded documents, videos, FAQs
  - Links to system actions (fill out form, complete training, sign document)
  - Conditional logic (if relocating internationally -> add visa tasks)
  - Due dates and reminders
  - Progress tracking (% complete)
- **Trigger mechanisms**:
  - Event-driven: Specific data change triggers a journey (e.g., "Hire Date" set -> onboarding journey starts)
  - Manual: HR or manager assigns a journey
  - Self-service: Employee initiates a journey (e.g., "I'm planning parental leave")
  - Scheduled: Time-based triggers (e.g., 30 days before retirement eligibility)
- HR teams can create **custom journeys** with a visual editor

**b) HCM Communicate (Communication Campaigns)**
- Built into HCM, connected to workforce data for targeting
- **Targeting**: Send messages to highly specific employee segments:
  - "All nurses in Region Vast with 2+ years tenure enrolled in AKAP-KR"
  - "All managers who haven't completed performance reviews"
  - Uses any combination of HR data fields for targeting
- **Campaign types**: One-time announcements, recurring newsletters, drip campaigns, event-triggered
- **Engagement analytics**: Open rates, click rates, response rates
- **Follow-up automation**: If someone didn't open -> resend with different subject
- **Personalization**: Merge fields from employee data ("Hej {FirstName}, din nasta medarbetarsamtal ar {ReviewDate}")
- **Manager communications**: Managers can send targeted messages to their team members

**c) Oracle Touchpoints (Sentiment & Check-ins)**
- Continuous employee listening tool for managers
- **Pulse Surveys**: Short (1-5 question) frequent surveys measuring:
  - Engagement level
  - Wellbeing / stress level
  - Manager relationship quality
  - Workload satisfaction
  - Growth opportunity satisfaction
- **Sentiment Tracking**: Dashboard showing sentiment trends per employee/team over time
- **Check-in Scheduling**: Prompts for regular 1-on-1 meetings
- **Recommended Actions**: Based on sentiment data, system suggests:
  - "Schedule a check-in with Anna -- her engagement dropped this month"
  - "Recognize Erik -- his sentiment improved after project completion"
  - "Discuss career development with Maria -- growth satisfaction is low"

**d) Oracle Celebrate (Recognition)**
- Peer-to-peer and manager-to-employee recognition
- Recognition tied to company values
- Milestone celebrations (work anniversary, birthday, achievement)
- Social feed of recognitions visible to team/org
- Points/badges system

**e) Oracle Digital Assistant (AI Chatbot)**
- Conversational AI for employee questions
- Natural language: "How many vacation days do I have left?"
- Completes transactions: "Submit a sick leave for today"
- Available via text or voice
- Integrates with all HCM modules

### OpenHR FOSS Equivalent Considerations

- OhrAssistant chatbot already exists -- expand with more intents and transaction capability
- Journeys concept maps well to OpenHR's existing OhrConversationFlow component
- Communication targeting could use existing employee data + notification system
- Pulse surveys (pulsundersokningar) already identified as a needed feature
- Peer recognition (berom) already on the admin page
- Key missing piece: the journey authoring tool (visual editor for step-by-step guides)

---

## 8. Platform Extensibility

### Workday Extend

Workday Extend is Workday's low-code/pro-code platform for building custom applications on the Workday technology stack:

**Core Capabilities:**
| Capability | Description |
|------------|-------------|
| **Custom Business Objects** | Create new data entities with fields, relationships, validation rules. System auto-generates REST APIs, indexed data sources, and security domains. |
| **Custom Business Processes** | Define approval workflows with steps, conditions, routing rules, notifications. Use the same Business Process Framework as core Workday. |
| **Custom UI Pages** | Build screens using Workday's UI framework. Pages can read/write both custom and core Workday data. |
| **Orchestrations** | Drag-and-drop workflow builder connecting multiple business processes, API calls, data transformations, notifications. Runtimes up to 48 hours for batch processes. |
| **Custom Reports** | Build reports combining custom and core data. Composite reports aggregate multiple data sources. |
| **Custom Security** | Define security domains and policies for custom objects, following Workday's role-based security model. |
| **REST APIs** | Every custom object gets auto-generated CRUD APIs. Can also call external APIs from orchestrations. |
| **Marketplace** | Workday Marketplace: pre-built apps and integrations from partners. Customers can publish internal apps. |

**2025-2026 Enhancements:**
- **Workday Build**: Unified developer platform pulling all tools together
- **Flowise Agent Builder**: AI agent builder for Workday Extend (available H1 2026)
- **Copilot for Orchestrate**: AI-assisted orchestration flow creation from natural language prompts
- **Expanded API Ecosystem**: Thousands of public APIs + prepackaged connectors

### SAP BTP (Business Technology Platform) Extensions

SAP offers three extensibility tiers:

| Tier | Description | Tools |
|------|-------------|-------|
| **Key User (In-App)** | No-code: custom fields, custom forms, email templates, custom analytics, business rules | Admin UI, no coding |
| **Developer (On-Stack)** | Low-code/pro-code: custom ABAP code, enhanced validations, complex business logic within S/4HANA | ABAP Cloud, RAP (RESTful ABAP Programming) |
| **Side-by-Side (BTP)** | Full-stack: independent apps on BTP connected via APIs. Decoupled lifecycle. Any language (Java, Node.js, Python) | SAP Build Apps, SAP Build Process Automation, CAP (Cloud Application Programming) |

Key SAP extensibility features:
- Custom business objects (MDF - Meta Data Framework in SuccessFactors)
- Configurable business rules engine
- Event-driven architecture (SAP Event Mesh)
- API Hub with 4000+ APIs
- Integration Suite for connecting to any system

### Typical Extension Model for an HR Platform (Generalized)

A well-designed extensible HR platform should support:

1. **Custom Fields**: Add fields to existing entities without modifying core schema. Stored in EAV (Entity-Attribute-Value) pattern or JSON columns.
2. **Custom Objects**: Define entirely new entities with relationships to core objects. Auto-generate CRUD APIs.
3. **Webhooks/Events**: Publish events when data changes (employee.created, leave.approved, payroll.completed). External systems subscribe.
4. **Workflow Hooks**: Insert custom steps into standard workflows (before/after approval, on status change).
5. **Custom Reports**: Report builder combining any fields from any objects.
6. **Integration APIs**: REST/GraphQL APIs with authentication, rate limiting, pagination, filtering.
7. **Plugin/App Framework**: Installable packages that add UI pages, API endpoints, background jobs, and data objects.
8. **Marketplace**: Repository of pre-built integrations and extensions from community/partners.
9. **Custom UI Components**: Embed custom UI within the standard application shell.
10. **Configuration as Code**: Export/import configuration, enable version control and CI/CD.

### OpenHR FOSS Equivalent Considerations

- OpenHR's modular monolith already supports adding new modules
- Need: custom fields infrastructure (JSON columns or EAV pattern on core entities)
- Need: webhook/event publishing system (outbox pattern already exists -- expose as public webhooks)
- Need: report builder allowing ad-hoc field selection and filtering
- Integration Hub already has adapter pattern -- formalize as plugin architecture
- Consider: OpenAPI spec generation for all endpoints, enabling third-party integrations
- AGPL-3.0 ensures all extensions stay open source

---

## 9. Swedish Public Procurement Requirements (LOU)

### Legal Framework

Swedish public procurement is governed by:
- **LOU** (Lagen om offentlig upphandling, 2016:1145) -- implements EU Directive 2014/24/EU
- **LUF** (Lagen om upphandling inom forsarjningssektorerna) -- utilities sectors
- **GDPR** + Swedish supplementary data protection law
- **DOS-lagen** (Lagen om tillganglighet till digital offentlig service) -- digital accessibility
- **Sakerhetsskyddslagen** -- security protection for classified/sensitive systems

### Fundamental Principles

All IT procurements must follow:
1. **Non-discrimination**: Cannot favor domestic suppliers
2. **Equal treatment**: Same information and criteria for all bidders
3. **Transparency**: Published criteria, predictable process
4. **Proportionality**: Requirements must be proportional to the contract
5. **Mutual recognition**: Accept equivalent certifications from other EU countries

### Typical Requirement Categories for HR System Procurement

Based on HEROMA's market positioning and typical Swedish region kravspecifikationer:

**A. Functional Requirements (Funktionella krav)**

| Category | Typical Requirements |
|----------|---------------------|
| **Personnel Register** | Complete employee lifecycle management, organizational structure, position management |
| **Payroll** | Swedish tax tables, collective agreements (AB, HOK, BHT), AGI reporting to Skatteverket, pension (AKAP-KR), all employer contributions |
| **Scheduling** | 24/7 healthcare scheduling, ATL compliance, shift planning, on-call (jour/beredskap), staffing overview |
| **Time & Attendance** | Time clock, deviation reporting, timesheet, overtime tracking, flex time |
| **Leave Management** | Vacation (Semesterlagen), sick leave (sjuklon 80% day 2-14, then FK), parental leave, VAB, study leave |
| **Self-Service** | Employee portal (schedule, pay slips, leave requests, personal data), Manager portal (approvals, team overview, staffing) |
| **Recruitment** | Vacancy management, application handling, Platsbanken integration, anonymized recruitment |
| **Competence** | Certification tracking with expiry alerts, competence profiles per role, gap analysis |
| **LAS Compliance** | Automatic LAS day calculation, threshold alerts, foretradesratt tracking, conversion triggers |
| **Salary Review** | Annual salary review process per collective agreement, union negotiation support |
| **Rehabilitation** | Rehab case management (HalsoSAM), FK notification at day 15, return-to-work planning |
| **Reporting** | Standard reports (headcount, absence, overtime, LAS, salary), ad-hoc reporting, SCB/SKR reporting |
| **Travel/Expense** | Travel claims with per diem (traktamente), receipt handling, approval workflow |
| **MBL** | MBL/samverkan documentation and tracking for significant decisions |

**B. Non-Functional Requirements (Icke-funktionella krav)**

| Category | Typical Requirements |
|----------|---------------------|
| **Availability** | 99.5-99.9% uptime, max planned downtime windows |
| **Performance** | Response time <2s for 95% of transactions, page load <3s |
| **Scalability** | Support 5,000-50,000 employees depending on region size |
| **Backup & Recovery** | RPO <1 hour, RTO <4 hours, point-in-time recovery |
| **Disaster Recovery** | Geographically separated backup, documented DR plan, annual DR test |

**C. Security Requirements (Sakerhetskrav)**

| Requirement | Detail |
|-------------|--------|
| **ISO 27001** | Vendor should be ISO 27001 certified or demonstrate equivalent |
| **GDPR compliance** | Full Article 28 processor agreement, DPIA support, registerutdrag within 30 days |
| **Data residency** | Data must be stored within EU/EEA (often Sweden preferred) |
| **Encryption** | Data encrypted at rest (AES-256) and in transit (TLS 1.2+) |
| **Access control** | Role-based access, unit-based scoping, sensitive data separation (health data) |
| **Authentication** | SITHS card support (healthcare), BankID for self-service, MFA for admin |
| **Audit logging** | All access and changes logged with user, timestamp, before/after values |
| **Penetration testing** | Annual penetration test by independent party, results shared with customer |
| **Incident management** | Documented incident response process, notification within 24h for security incidents |

**D. Integration Requirements (Integrationskrav)**

| Integration | Typical Requirement |
|-------------|-------------------|
| **Skatteverket** | AGI XML (employer declaration) |
| **Forsakringskassan** | Sick leave notifications, parental leave coordination |
| **SCB** | Statistical reporting (salary statistics, employment statistics) |
| **SKR** | KPR salary statistics |
| **Arbetsformedlingen** | Platsbanken vacancy publication |
| **Finance system** | Raindance/Agresso/Unit4 for accounting postings |
| **Identity management** | AD/LDAP/SAML/OIDC for SSO |
| **Bank** | Payment files (ISO 20022 pain.001 for salaries) |
| **Pension provider** | Pension reporting (e.g., KPA Pension) |
| **E-signing** | SITHS/BankID-based digital signatures |
| **Open APIs** | REST APIs with OpenAPI specification for custom integrations |

**E. Accessibility Requirements (Tillganglighetskrav)**

| Requirement | Detail |
|-------------|--------|
| **WCAG 2.1 AA** | Mandatory per DOS-lagen for public sector digital services |
| **EN 301 549** | European harmonized standard for ICT accessibility |
| **Screen reader** | Full compatibility with JAWS, NVDA, VoiceOver |
| **Keyboard navigation** | All functions accessible via keyboard |
| **Color contrast** | Minimum 4.5:1 for normal text, 3:1 for large text |
| **Responsive design** | Functional on mobile devices (375px+) |

**F. Vendor/Delivery Requirements**

| Requirement | Detail |
|-------------|--------|
| **Deployment** | Cloud SaaS, on-premise, or hybrid options |
| **Data portability** | Export all data in open formats upon contract termination |
| **Migration support** | Data migration from current system (typically HEROMA) with parallel run |
| **Training** | Administrator training, key user training, end-user training materials |
| **Support** | Swedish-speaking support, defined SLAs, 24/7 for critical issues |
| **Documentation** | Complete technical documentation, API documentation, user manuals in Swedish |
| **Roadmap** | Published product roadmap, customer influence on priorities |
| **Source code escrow** | Access to source code if vendor ceases operations (for proprietary systems) |

### OpenHR FOSS Advantage

OpenHR being AGPL-3.0 open source eliminates several procurement concerns:
- No vendor lock-in (source code is available, not just escrowed)
- No license fees (significant cost advantage over HEROMA/Workday)
- Data portability guaranteed (self-hosted, full database access)
- Swedish data residency (self-hosted within region's own infrastructure)
- Community-driven roadmap rather than single-vendor dependency
- Still needs: formal security audit, WCAG audit, documented SLAs for support organization

---

## 10. GDPR Advanced Features

### Beyond Basic DSR (Data Subject Requests)

Enterprise HR systems provide these advanced GDPR capabilities:

**a) Privacy Impact Assessments (DPIA)**

Article 35 requires DPIAs before processing likely to result in high risk. Enterprise systems offer:

| Feature | Description |
|---------|-------------|
| **DPIA Templates** | Pre-built assessment templates for common HR processing (recruitment screening, employee monitoring, health data) |
| **Risk Scoring** | Automated risk scoring based on data types, volume, processing activities, recipients |
| **Mitigation Tracking** | Document safeguards and track implementation of risk mitigation measures |
| **Review Cycle** | Automatic reminders for periodic DPIA reviews (annually or when processing changes) |
| **Audit Trail** | Complete history of assessments, decisions, and approvals |
| **DPO Sign-off** | Workflow for Data Protection Officer review and approval |

**b) Data Lineage & Data Mapping**

| Feature | Description |
|---------|-------------|
| **Processing Activities Register (RoPA)** | Article 30 register: what data, what purpose, what legal basis, who processes, retention period, security measures, transfers to third countries |
| **Data Flow Maps** | Visual diagrams showing how personal data flows: collection point -> processing system -> storage -> recipients -> deletion |
| **Field-Level Classification** | Every data field tagged: personal data, sensitive/special category, PII, financial, health. Classification drives access control and retention. |
| **Cross-System Tracking** | Track where employee data is sent (payroll provider, pension company, FK, Skatteverket) and legal basis for each transfer |
| **Column-Level Lineage** | Trace individual data points through transformations (e.g., personnummer -> encrypted hash in analytics) |
| **Impact Assessment** | When a field or system changes, identify all downstream impacts on data subjects |

**c) Consent Management**

Important nuance for HR: **consent is rarely the appropriate legal basis for employee data** because of the power imbalance (employees may feel they can't refuse). Most HR processing uses:
- **Contract performance** (Article 6(1)(b)): Processing necessary for employment contract
- **Legal obligation** (Article 6(1)(c)): Tax reporting, pension contributions, workplace safety
- **Legitimate interest** (Article 6(1)(f)): Performance management, workforce planning (must pass balancing test)

However, consent IS needed for:
- Processing photos for internal directory (beyond strict necessity)
- Wellness program participation data
- Employee satisfaction surveys (when not anonymized)
- Social media monitoring
- Marketing use of employee likeness

Enterprise consent management features:
| Feature | Description |
|---------|-------------|
| **Granular Consent Records** | Per-purpose consent tracking: what was consented to, when, how, withdrawal date |
| **Consent Withdrawal** | One-click withdrawal, automatic downstream processing stops |
| **Purpose Limitation** | Enforce that data collected for purpose A cannot be used for purpose B without new consent/basis |
| **Consent Audit** | Complete history of consent given, modified, withdrawn with timestamps |
| **Re-consent Campaigns** | When processing changes, mass re-consent request to affected employees |

**d) Automated Retention & Deletion**

| Feature | Description |
|---------|-------------|
| **Retention Schedule** | Per-data-category retention periods configured: payroll 7 years (Bokforingslagen), recruitment applications 2 years, sick leave 10 years, tax data 7 years |
| **Automatic Flagging** | System identifies data past retention period, flags for review |
| **Anonymization Engine** | Replace personal data with anonymous identifiers while preserving statistical value |
| **Selective Deletion** | Delete specific data categories while retaining others (e.g., delete health data after 2 years, keep employment record for 10 years) |
| **Legal Hold** | Prevent deletion when data is subject to legal proceedings or audit |
| **Deletion Audit** | Log what was deleted, when, by whom, under what authority |

**e) Data Subject Rights Beyond Access**

| Right | Enterprise Implementation |
|-------|--------------------------|
| **Right to Access (Art 15)** | Automated report generation collecting all data about an employee across all modules, formatted as readable PDF + machine-readable JSON |
| **Right to Rectification (Art 16)** | Self-service correction of personal data, HR approval workflow for verified data |
| **Right to Erasure (Art 17)** | Automated identification of deletable data, legal basis check before deletion, cascading deletion across modules |
| **Right to Restriction (Art 18)** | Flag records as "restricted processing" -- data retained but not processed |
| **Right to Portability (Art 20)** | Export all data in structured, machine-readable format (JSON, CSV) |
| **Right to Object (Art 21)** | Employee objects to specific processing (e.g., profiling); system stops that processing |

**f) Privacy by Design Features**

| Feature | Description |
|---------|-------------|
| **Data Minimization Enforcement** | System prevents collection of unnecessary fields; configurable per module |
| **Access Logging** | Every view of personal data logged (not just changes) -- "who looked at my salary?" |
| **Purpose Binding** | Data fields tagged with allowed purposes; system enforces purpose limitation in queries |
| **Pseudonymization** | Analytics and reporting use pseudonymized data by default |
| **Encryption at Field Level** | Personnummer, bank account, health data encrypted separately in database |
| **Privacy Dashboard** | Employee-facing: "Here is all data we hold about you, here are the legal bases, here is who accessed it" |

### OpenHR FOSS Equivalent Considerations

- OpenHR already has GDPR module with registerutdrag and retention cleanup background job
- Need to add: DPIA workflow (templates, risk scoring, approval), processing activities register with visual data flow mapping
- Consent management: track per-purpose consent for non-contract-based processing
- Data lineage: document all integration points and data flows (16 adapters = 16 external data recipients)
- Privacy dashboard for employees: expand "Min sida" with "Mina personuppgifter" section
- Retention schedule already defined in docs -- needs to be configurable per data category in system

---

## Summary: Priority Matrix for OpenHR Feature Development

| Feature Area | Enterprise Value | Swedish Public Sector Relevance | Implementation Complexity | Priority |
|-------------|-----------------|-------------------------------|--------------------------|----------|
| **Strategic Compensation** | High | High (lonerevision per AB/HOK) | Medium | **P1** |
| **Advanced Workforce Mgmt** | Very High | Very High (24/7 healthcare) | High | **P1** |
| **Enterprise Analytics/BI** | High | High (SKR/SCB reporting) | Medium | **P1** |
| **GDPR Advanced** | High | Very High (legal requirement) | Medium | **P1** |
| **Benefits Administration** | Medium | Medium (simpler in Sweden) | Low-Medium | **P2** |
| **Employee Experience** | High | Medium-High | Medium | **P2** |
| **Platform Extensibility** | Very High | High (integration requirements) | High | **P2** |
| **Skills Intelligence** | Medium-High | Medium | High | **P3** |
| **Swedish Procurement (LOU)** | N/A (compliance) | Very High | Low (documentation) | **P1** |
| **Contingent Workforce/VMS** | Medium | Medium (bemanningsforetag) | High | **P3** |

---

## Sources

### Contingent Workforce / VMS
- [Workday VNDLY Overview](https://www.workday.com/en-us/products/vndly-vms/overview.html)
- [Workday VNDLY Datasheet (PDF)](https://www.workday.com/content/dam/web/en-us/documents/datasheets/workday-vndly-datasheet-vms.pdf)
- [Workday VNDLY for HR](https://www.workday.com/en-us/products/vndly-vms/vndly-vms-for-hr.html)
- [Workday VNDLY for Procurement](https://www.workday.com/en-us/products/vndly-vms/vndly-vms-for-procurement.html)
- [Workday VNDLY Statement of Work](https://www.workday.com/en-us/products/vndly-vms/statement-of-work.html)
- [Workday VNDLY Extended Workforce Management](https://www.workday.com/en-us/products/vndly-vms/extended-workforce-management.html)
- [PwC: Contingent Workforce with VNDLY](https://www.pwc.com/us/en/technology/alliances/library/contingent-workforce-workday-vndly.html)
- [3Sixty Insights: VNDLY Total Workforce](https://3sixtyinsights.com/workday-vndly-is-defining-the-future-of-total-workforce-management/)
- [Surety Systems: VNDLY Overview](https://www.suretysystems.com/insights/workday-vndly-optimizing-vendor-management-across-teams/)

### Strategic Compensation
- [Workday Compensation Management](https://www.workday.com/en-us/products/human-capital-management/human-resource-management/compensation.html)
- [Commit: Workday Compensation Review Process](https://commitconsulting.com/blog/workday-compensation-review-process)
- [Surety Systems: Workday Advanced Compensation Guide](https://www.suretysystems.com/insights/your-comprehensive-guide-to-workday-advanced-compensation/)
- [Surety Systems: Workday Merit Process](https://www.suretysystems.com/insights/aligning-rewards-and-performance-with-the-workday-merit-process/)
- [Workday Compensation Datasheet (PDF)](https://www.workday.com/content/dam/web/en-us/documents/datasheets/datasheet-workday-compensation.pdf)
- [Commit: Compensation Review Dates](https://commitconsulting.com/blog/workday-advanced-compensation-dates)
- [AIHR: Total Rewards Statement Template](https://www.aihr.com/blog/total-rewards-statement/)
- [Carta: Total Rewards Statement](https://carta.com/learn/startups/compensation/total-rewards-statement/)

### Benefits Administration
- [Workday Benefits Administration](https://www.workday.com/en-us/products/human-capital-management/human-resource-management/benefits.html)
- [SAP: Global Benefits Components](https://learning.sap.com/learning-journeys/configuring-sap-successfactors-employee-central-global-benefits/introducing-global-benefits-and-its-components_ef043800-a7c9-4e5e-bea0-32861f59ae44)
- [HR Path: SuccessFactors Global Benefits vs SAP Benefits](https://hr-path.com/en/blog/successfactors-global-benefits-vs-sap-benefits-management/2025/03/14/)
- [TriNet: Qualifying Life Events](https://www.trinet.com/insights/whats-a-qualifying-life-event)
- [UnitedHealthcare: Qualifying Life Events](https://www.uhc.com/understanding-health-insurance/open-enrollment/qualifying-life-events)

### Skills Intelligence / Talent Marketplace
- [Workday Skills Cloud](https://www.workday.com/en-us/products/human-capital-management/skills-cloud.html)
- [Workday Engineering: Skill Inference with LLM](https://medium.com/workday-engineering/skill-inference-building-an-llm-based-service-in-the-workday-skills-cloud-47c9cce9f7bd)
- [Workday Blog: Foundation of Skills Cloud](https://blog.workday.com/en-us/2020/foundation-workday-skills-cloud.html)
- [Commit: Skills Cloud Explained](https://commitconsulting.com/blog/workday-skills-cloud)
- [GoFigr: Workday Talent Marketplace](https://www.gofigr.ai/blog/workday-talent-marketplace)
- [SAP: Opportunity Marketplace Guide](https://talenteam.com/blog/ultimate-guide-to-sap-successfactors-opportunity-marketplace/)
- [SAP: SuccessFactors H2 2025 Talent Management](https://rizing.com/human-capital-management/sap-successfactors-h2-2025-talent-management/)
- [Surety Systems: SAP Opportunity Marketplace](https://www.suretysystems.com/insights/sap-opportunity-marketplace-surety-systems/)

### Enterprise Analytics / BI
- [Workday Analytics & Reporting](https://www.workday.com/en-us/products/analytics-reporting/overview.html)
- [Workday Core Reporting & Analytics](https://www.workday.com/en-us/products/analytics-reporting/core-reporting-analytics.html)
- [Workday Prism Analytics](https://www.workday.com/en-us/products/analytics-reporting/data-hub.html)
- [Kognitiv: Prism Analytics Implementation](https://kognitivinc.com/blog/workday-prism-analytics-implementation-considerations/)
- [ClearPoint: 50+ HR KPIs](https://www.clearpointstrategy.com/blog/human-capital-kpis-scorecard-measures)
- [AIHR: 19 HR Metrics Examples](https://www.aihr.com/blog/hr-metrics-examples/)
- [Peoplebox: 45+ HR Metrics & KPIs 2026](https://www.peoplebox.ai/blog/hr-metrics/)
- [AIHR: Predictive Analytics in HR](https://www.aihr.com/blog/predictive-analytics-human-resources/)
- [Nature: ML for Employee Attrition Prediction](https://www.nature.com/articles/s41598-026-36424-2)
- [JobsPikr: Workforce Planning Metrics 2025](https://www.jobspikr.com/blog/workforce-planning-metrics-2025/)

### Advanced Workforce Management
- [UKG Workforce Management](https://www.ukg.com/products/workforce-management)
- [UKG Scheduling](https://www.ukg.com/products/features/scheduling)
- [UKG Strategic Workforce Planning](https://www.ukg.com/products/features/strategic-workforce-planning)
- [UKG: Labor Optimization](https://www.ukg.com/blog/hr-leaders/labor-optimization-made-simple-ukg-strategic-workforce-planning)
- [Improv: UKG Pro WFM Complete Guide 2026](https://improvizations.com/insights/ukg-pro-workforce-management-complete-guide-2026)
- [OutSail: UKG vs Dayforce](https://www.outsail.co/post/ukg-vs-dayforce-enterprise-workforce)
- [Dayforce Healthcare Solutions](https://www.ceridian.com/solutions/healthcare-workforce)
- [Dayforce Scheduling](https://www.ceridian.com/products/dayforce/workforce-management/scheduling)

### Employee Experience Platform
- [Oracle ME Overview](https://www.oracle.com/human-capital-management/employee-experience/oracle-me/)
- [Oracle Journeys](https://www.oracle.com/human-capital-management/employee-experience/oracle-me/journeys/)
- [Oracle HCM Communicate](https://www.oracle.com/human-capital-management/employee-experience/oracle-me/communicate/)
- [Oracle: Employee Engagement Communications (March 2025)](https://www.oracle.com/news/announcement/oracle-boosts-employee-engagement-with-communications-and-events-2025-03-20/)
- [Apps2Fusion: Oracle ME](https://apps2fusion.com/oracle-me-employee-experience-in-oracle-hcm/)
- [Surety Systems: Oracle ME Personalized Experiences](https://www.suretysystems.com/insights/utilizing-oracle-me-for-personalized-employee-experiences/)
- [Oracle Journeys Implementation Guide (PDF)](https://docs.oracle.com/en/cloud/saas/human-resources/faijh/implementing-and-using-journeys.pdf)
- [Kovaion: Oracle ME](https://www.kovaion.com/blog/oracle-me-guides-connects-support-employees/)

### Platform Extensibility
- [Workday Extend](https://www.workday.com/en-us/products/platform-product-extensions/app-development.html)
- [Workday Platform & Product Extensions](https://www.workday.com/en-us/products/platform-product-extensions/overview.html)
- [Workday Orchestrate](https://www.workday.com/en-us/products/platform-product-extensions/integrations.html)
- [Workday Build Announcement (Sept 2025)](https://newsroom.workday.com/2025-09-16-Workday-Unveils-Workday-Build,-Giving-Developers-the-Tools-to-Build-the-Future-of-Work)
- [Workday 2026R1 Integrations & Extend](https://coreteamgroup.com/workday-release-2026r1-integrations-extend-updates/)
- [Collaborative Solutions: Workday Extend Demystified (PDF)](https://www.collaborativesolutions.com/hubfs/00_Premium%20Content/eBooks%20and%20Whitepapers/Workday%20Extend%20Demystified%20Whitepaper/Whitepapers%20%7C%20Workday%20Extend%20Demystified.pdf)
- [SAP: Extensibility Types](https://community.sap.com/t5/technology-blog-posts-by-sap/exploring-sap-extensibility-types-of-extensibilities/ba-p/13656258)
- [SAP: BTP Extensions](https://www.geeksforgeeks.org/building-extensions-with-sap-business-technology-platform/)

### Swedish Public Procurement (LOU)
- [Upphandlingsmyndigheten: Requirements Specification](https://www.upphandlingsmyndigheten.se/inkopsprocessen/genomfor-upphandlingen/krav-pa-foremalet-for-upphandlingen/)
- [Upphandlingsmyndigheten: About Public Procurement](https://www.upphandlingsmyndigheten.se/en/about-public-procurement/)
- [Konkurrensverket: Swedish Public Procurement Act](https://www.konkurrensverket.se/en/public-procurement/laws-and-rules/the-swedish-public-procurement-act/)
- [Government.se: Public Procurement](https://www.government.se/government-policy/public-procurement/)
- [CE Sweden: LOU Guide](https://www.ce.se/understanding-the-public-procurement-act-lagen-om-offentlig-upphandling-a-practical-guide/)
- [CGI HEROMA](https://www.cgi.com/se/sv/heroma)
- [HerbertNathan: HEROMA System Guide](https://www.herbertnathan.com/sv/systemguide/heroma-2/)
- [Upphandlingsmyndigheten: Accessibility](https://www.upphandlingsmyndigheten.se/hallbarhet/socialt-ansvarsfull-upphandling/Tillganglighet-och-samtliga-anvandares-behov/)
- [Upphandlingsmyndigheten: ISO 27001 in Procurement](https://www.upphandlingsmyndigheten.se/frageportalen/1555156/iso-27001/)

### GDPR Advanced Features
- [Securiti: GDPR Employee Data](https://securiti.ai/blog/gdpr-employee-data/)
- [DPO Consulting: GDPR Data Governance](https://www.dpo-consulting.com/blog/gdpr-data-governance)
- [DPO Consulting: HR Systems and GDPR Compliance](https://www.dpo-consulting.com/blog/hr-system-and-gdpr)
- [iubenda: GDPR Data Mapping](https://www.iubenda.com/en/blog/gdpr-data-mapping/)
- [GDPR Register: Data Mapping Guide](https://www.gdprregister.eu/gdpr/guide-to-personal-data-mapping/)
- [SecurePrivacy: Enterprise Privacy Dashboard](https://secureprivacy.ai/blog/enterprise-privacy-dashboard)
- [Data Privacy Group: GDPR Employee Consent](https://thedataprivacygroup.com/us/blog/2019-10-29-gdpr-employee-consent-to-process-personal-data/)
- [Atlan: Data Privacy Management Software](https://atlan.com/know/data-governance/best-data-privacy-management-software/)
