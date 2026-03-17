# RegionHR Gap Analysis: Enterprise HR System Features

**Date:** 2026-03-16
**Scope:** Swedish regional healthcare organization replacing HEROMA (CGI)
**Benchmarked against:** Workday, SAP SuccessFactors, Oracle HCM, Visma HR+, HEROMA, BambooHR, Personio

---

## Executive Summary

RegionHR has strong foundations in payroll calculation (Swedish tax law, collective agreements), scheduling (ATL compliance), and several HR modules (LAS, HalsoSAM, salary review, travel, recruitment). However, significant gaps remain in employee self-service depth, performance management, learning/development, document management, notifications, reporting/analytics, mobile access, and overall UX maturity. The system currently lacks edit/delete capabilities on most entities, has no notification system, no document storage, no performance management module, and limited role-based views in the frontend.

**Gap counts by severity:**
- Critical: 24
- Important: 31
- Nice-to-have: 18

---

## 1. Employee Lifecycle (Hire to Retire)

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Employee Lifecycle | Edit employee personal data (UI) | API exists (UppdateraKontaktuppgifter) but no edit form in UI | **Critical** | Add edit forms on Anstallda/Detalj.razor for contact info, bank details, tax data |
| Employee Lifecycle | Employee detail view with full history | Detalj.razor exists but minimal | **Critical** | Build comprehensive detail view: all employments, salary history, absence history, LAS data, certifications |
| Employee Lifecycle | Employment changes (transfer, promotion) | Employment entity supports AvslutaAnstallning and AndraLon but no UI for internal transfer | **Important** | Add workflows for internal transfer (new employment + end old), promotion, role change with audit trail |
| Employee Lifecycle | Change employment rate (sysselsattningsgrad) | Domain method AndraSysselsattningsgrad exists | **Important** | Build UI for changing employment rate with effective date, salary recalculation, and notification to payroll |
| Employee Lifecycle | End of employment / termination workflow | AvslutaAnstallning exists in domain | **Critical** | Build complete offboarding workflow: final pay calculation, vacation payout (semesterersattning 12%), asset return checklist, access revocation, certificate of employment generation |
| Employee Lifecycle | Offboarding checklist | Not implemented | **Important** | Create offboarding checklist similar to OnboardingChecklist: IT access revocation, asset return, knowledge transfer, exit interview, final salary items |
| Employee Lifecycle | Probation period tracking | Not tracked | **Important** | Track provanstallning with end-date alerts (max 6 months per LAS), reminder before expiry for manager decision |
| Employee Lifecycle | Employment contract generation | Not implemented | **Important** | Template-based contract generation (PDF) with merge fields from employee/employment data, digital signature support |
| Employee Lifecycle | Certificate of employment (tjanstgoringsbevis) | Not implemented | **Important** | Auto-generate employment certificates on request with employment history, roles, dates |
| Employee Lifecycle | Salary history tracking | PayrollResult stores results but no dedicated salary change log | **Important** | Create salary history log per employee showing all changes with effective dates, reasons, and who approved |
| Employee Lifecycle | Re-employment (foretradesratt) workflow | LAS module tracks foretradesratt accumulation | **Important** | When creating new vacancy, system should check and flag employees with active foretradesratt; integrate with recruitment module |

## 2. Self-Service (Employee & Manager Portal)

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Self-Service | Employee portal overview | MinSida/Index.razor exists with static demo data | **Critical** | Connect to real data: upcoming shifts, vacation balance, pending requests, pay slips, personal info |
| Self-Service | Update personal contact info | No self-service update form | **Critical** | Add form for employee to update email, phone, address with audit trail; bank details requires stronger auth |
| Self-Service | View and download pay slips | Lonespecifikationer.razor page exists | **Important** | Connect to real PayrollResult data, generate PDF pay slips, enable download/email delivery |
| Self-Service | Apply for leave (vacation, VAB, etc.) | Link to "Ansok om semester" points to arenden/nytt | **Important** | Build proper leave request form with calendar picker, balance display, conflict check against schedule, auto-routing to manager |
| Self-Service | View own schedule | MinSida/Schema.razor exists | **Important** | Connect to real scheduling data, show week/month view with shift details, enable shift swap initiation |
| Self-Service | Request shift swap | ShiftSwapRequest domain entity exists | **Important** | Build UI for browsing available swaps, requesting/accepting swaps, manager approval flow |
| Self-Service | Report sick leave | Not in self-service | **Critical** | Employee should be able to report sick via self-service; auto-creates absence case, notifies manager, triggers karensavdrag logic |
| Self-Service | Submit travel/expense claims | API exists but no self-service UI | **Important** | Build employee-facing travel claim form with receipt photo upload, per diem auto-calc |
| Self-Service | Manager portal overview | ChefsPortalViewModel defined but no dedicated page | **Critical** | Build manager dashboard: pending approvals count, team attendance today, staffing status, LAS alerts, upcoming tasks |
| Self-Service | Manager: approve/reject requests | Godkannanden page shows list but approve/reject buttons only link to detail | **Critical** | Implement one-click approve/reject with comment, batch approval, delegation when manager absent |
| Self-Service | Manager: view team overview | Not implemented | **Important** | Show all direct reports: who is working, absent, on leave; click through to employee details |
| Self-Service | Manager: staffing gap management | StaffingOverviewService exists in backend | **Important** | UI to visualize gaps, send callout requests to available staff, track responses |
| Self-Service | HR admin portal | All pages currently show same view regardless of role | **Critical** | Implement role-based navigation and views: HR sees all units, manager sees own unit, employee sees own data |
| Self-Service | Delegation of authority | Not implemented | **Important** | Allow managers to delegate approval authority during vacation/absence to substitute |

## 3. Time & Attendance

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Time & Attendance | Time clock (in/out) | TimeClockEvent domain entity exists with API | **Important** | Build time clock UI (for both desktop and future mobile/terminal), handle missed punches, late arrivals |
| Time & Attendance | Timesheet / time reporting | Not implemented as separate feature | **Critical** | Build monthly timesheet view where employees confirm worked hours, deviations from schedule auto-populated |
| Time & Attendance | Overtime reporting and approval | Overtime calculation exists in payroll engine | **Important** | Build UI for reporting overtime hours, manager approval workflow, running totals against ATL limits |
| Time & Attendance | Flex time / time bank | Not implemented | **Important** | Track flex time balance (plus/minus hours) for employees with flex agreements, show balance in self-service |
| Time & Attendance | Absence overview calendar | Not implemented | **Important** | Team calendar showing all absences (vacation, sick, parental) with color coding, useful for manager planning |
| Time & Attendance | Working time analysis (ATL compliance) | ArbetstidslagenValidator exists in scheduling domain | **Important** | Build dashboard showing ATL compliance per employee: dygnsvila violations, overtime accumulation, weekly rest gaps |
| Time & Attendance | Jour/beredskap logging | Scheduling supports jour/beredskap shift types | **Important** | Build dedicated logging for on-call/standby hours, track cumulative beredskap threshold (125h for higher rate) |

## 4. Payroll Processing

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Payroll | Full payroll lifecycle | PayrollRun with states: Skapad -> Paborjad -> Beraknad -> Granskad -> Godkand -> Utbetald | Exists | Core functionality built |
| Payroll | Swedish tax calculation | Tax tables, karensavdrag, sjuklon, OB, overtime all implemented | Exists | Continue maintaining with annual rate updates |
| Payroll | Payroll run review/audit UI | No UI for reviewing payroll results | **Critical** | Build payroll review dashboard: summary totals, per-employee breakdown, comparison with previous month, anomaly flagging |
| Payroll | Pay slip generation (PDF) | Not implemented | **Critical** | Generate individual pay slips from PayrollResult, store/email to employees, make available in self-service |
| Payroll | Payroll correction handling | Retroactive engine exists | **Important** | Build UI for initiating corrections: select employee, period, reason; generate adjustment run |
| Payroll | Salary deductions management | Basic support in payroll engine | **Important** | Build management for recurring deductions: loneexekution (wage garnishment from Kronofogden), union fees, voluntary deductions |
| Payroll | AGI XML submission tracking | AGI XML export adapter exists | **Important** | Build UI to track AGI submission status per month, resubmission, confirmation from Skatteverket |
| Payroll | Payment file management | Nordea pain.001 adapter exists | **Important** | Build UI to generate, review, and track bank payment files; reconciliation with bank confirmation |
| Payroll | Payroll statistics and reporting | Not implemented | **Important** | Monthly/annual payroll summaries, cost per unit, employer contribution reports, pension reports |
| Payroll | Vacation pay (semesterlon) calculation | Mentioned in law compliance doc but unclear implementation depth | **Important** | Ensure sammaloneregeln and procentregeln are both implemented; 0.43% semestertillagg, 12% variable pay component |

## 5. Benefits Administration

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Benefits | Pension administration (AKAP-KR) | Pension calculation engine exists in payroll | Partial | Build reporting view for pension contributions per employee, annual pension statement |
| Benefits | Health insurance / sjukforsakring | Forsakringskassan integration adapter exists | Partial | Build FK notification workflow (day 15 sick leave trigger), track sjukpenning coordination |
| Benefits | Employee benefit enrollment | Epassi adapter exists for benefits | **Important** | Build benefit enrollment UI: show available benefits (friskvardsbidrag, etc.), employee selection, budget tracking |
| Benefits | Friskvardsbidrag (wellness benefit) | Not tracked | **Important** | Track annual wellness benefit allowance per employee, receipt handling, payroll integration (tax-free up to 5000 SEK) |
| Benefits | Insurance overview | Skandia adapter exists | **Nice-to-have** | Show employee their insurance coverage: TGL, AGS, AFA |
| Benefits | Parental leave planning | Not implemented | **Important** | Build parental leave planner: 480-day allocation tracker, foraldralon calculation (10% x 180 days per AB), FK coordination |

## 6. Leave Management

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Leave | Vacation balance tracking | SelfService ViewModel has SemesterdagarKvar etc. but static | **Critical** | Implement vacation balance engine: age-based entitlement (25/31/32 days per AB), earned vs used vs saved, 5-year expiry on saved days |
| Leave | Vacation request and approval workflow | Case management handles absence cases | **Important** | Build dedicated vacation request flow: check balance, check schedule conflicts, multi-day selection, manager approval |
| Leave | Sick leave management | Payroll handles sjuklon calculation; HalsoSAM tracks rehab | Partial | Build complete sick leave flow: day 1 notification, karensavdrag auto-calc, day 8 lakarintyg reminder, day 15 FK notification |
| Leave | Parental leave management | Not implemented | **Important** | Track parental leave periods, coordinate with FK, calculate foraldralon supplement, handle return-to-work scheduling |
| Leave | Leave of absence (tjenstledighet) | Not specifically implemented | **Important** | Build leave-of-absence request types: study leave, union work, political office, personal leave; each with different rules |
| Leave | VAB (vard av barn) tracking | Not implemented | **Important** | Track VAB days, coordinate with FK reporting, ensure correct payroll handling |
| Leave | Leave balance dashboard | Static data only | **Critical** | Real-time dashboard per employee: vacation days (earned/used/saved/remaining), comp time, flex balance |
| Leave | Team leave calendar | Not implemented | **Important** | Calendar view for manager showing all team members' approved and pending leave, useful for planning |

## 7. Performance Management

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Performance | Annual performance review (medarbetarsamtal) | Not implemented | **Critical** | Build performance review module: configurable review templates, self-assessment, manager assessment, goal setting, competency rating |
| Performance | Goal setting and tracking | Not implemented | **Important** | Individual and team goals linked to organizational objectives, progress tracking, goal review at medarbetarsamtal |
| Performance | Continuous feedback / 1-on-1 notes | Not implemented | **Nice-to-have** | Manager tool for recording ongoing feedback notes, follow-up items from 1-on-1 meetings |
| Performance | 360-degree feedback | Not implemented | **Nice-to-have** | Multi-rater feedback from peers, subordinates, other stakeholders for leadership development |
| Performance | Competency framework | Not implemented | **Important** | Define competency profiles per role/befattning, assess employees against competencies, identify gaps |
| Performance | Development plan (individuell utvecklingsplan) | Not implemented | **Important** | Create development plans linked to performance review outcomes, track completion of development activities |
| Performance | Salary review integration | SalaryReview module exists with proposals | Partial | Link salary review proposals to performance review outcomes; display performance data alongside salary proposals |

## 8. Learning & Development

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Learning | Training/course management | Not implemented | **Important** | Build training catalog, course scheduling, enrollment, completion tracking |
| Learning | Mandatory training tracking | Onboarding has "Brandskyddsutbildning" etc. but not a general system | **Critical** | Track mandatory certifications per role (e.g., HLR, brandskydd, hygien for healthcare), expiry dates, renewal reminders |
| Learning | Competence register | MinKompetens integration adapter exists | **Important** | Build internal competence tracking: certifications, licenses (sjukskoterskeleg, lakareleg), specializations, language skills |
| Learning | Training request and approval | Not implemented | **Nice-to-have** | Employee requests training, manager approves, budget tracking per unit |
| Learning | E-learning integration | Not implemented | **Nice-to-have** | Integration with LMS (e.g., Kompetensportalen used by many regions) |
| Learning | Succession planning | Not implemented | **Nice-to-have** | Identify key positions, potential successors, readiness levels, development gaps |

## 9. Recruitment & Onboarding

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Recruitment | Vacancy management | Vacancy entity with full lifecycle (Utkast -> Publicerad -> Stangd -> Tillsatt) | Exists | Core functionality built |
| Recruitment | Application handling | Application entity with status flow, scoring, interview scheduling | Exists | Core functionality built |
| Recruitment | Platsbanken integration | PubliceradPlatsbanken flag exists but no actual integration | **Important** | Build actual integration to publish vacancies to Arbetsformedlingen Platsbanken |
| Recruitment | Applicant communication | Only email stored, no communication features | **Important** | Build email templates and automated communication: confirmation, interview invitation, rejection, offer |
| Recruitment | Interview scheduling | IntervjuTidpunkt field exists | **Important** | Build scheduling tool with calendar integration, multiple interviewers, evaluation forms |
| Recruitment | Reference check management | Not implemented | **Nice-to-have** | Track reference contacts, reference check notes, structured evaluation |
| Recruitment | Offer letter generation | Not implemented | **Important** | Generate offer letters from template with employment terms, require acceptance/rejection |
| Recruitment | Onboarding checklist | OnboardingChecklist with standard tasks exists | Exists | Expand with customizable templates per role, progress tracking dashboard, notifications to task owners |
| Recruitment | Onboarding portal for new hires | Not implemented | **Important** | Dedicated portal for new hires: welcome info, documents to sign, pre-employment forms, first-day instructions |
| Recruitment | MBL/union notification for recruitment | Not implemented | **Important** | Workflow to ensure MBL consultation (samverkan) before publishing vacancy, with documentation |
| Recruitment | Recruitment analytics | Not implemented | **Nice-to-have** | Time-to-hire, cost-per-hire, source effectiveness, diversity statistics |
| Recruitment | Foretradesratt check | LAS module tracks this but not integrated with recruitment | **Important** | Auto-check foretradesratt register when opening vacancy; warn if employees have priority rights |

## 10. Reporting & Analytics

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Reporting | Executive HR dashboard | Home.razor shows 4 KPI cards with hardcoded demo data | **Critical** | Build real-time dashboard: headcount, turnover rate, sick leave %, vacancy rate, overtime hours, cost per FTE |
| Reporting | Operational reports (pre-built) | Not implemented | **Critical** | Build standard report library: personnel roster, salary register, absence statistics, overtime report, LAS status, staffing analysis |
| Reporting | Ad-hoc reporting / report builder | Not implemented | **Important** | Build report builder allowing HR to combine data fields, filter by unit/period/employment type, export to Excel/PDF |
| Reporting | SCB statistics reporting | SCB integration adapter exists | **Important** | Build UI for generating and submitting statutory statistics reports to SCB (konjunkturstatistik, lonestatistik) |
| Reporting | SKR reporting | SKR adapter exists | **Important** | Implement actual data export for SKR salary statistics (KPR) and other required submissions |
| Reporting | Sick leave statistics | SickLeaveStatisticsService exists | Partial | Build dashboard with sick leave trends: short/long term, per unit, per age group, gender; benchmark against national averages |
| Reporting | Workforce planning analytics | Not implemented | **Nice-to-have** | Predict retirement waves, forecast staffing needs, analyze turnover patterns, scenario modeling |
| Reporting | PowerBI integration | PowerBI adapter exists | **Important** | Implement actual data feed to PowerBI for advanced analytics; or embed PowerBI reports in RegionHR |
| Reporting | Cost simulation | Not implemented | **Nice-to-have** | Simulate total cost of employment including salary, AG-avgifter, pension, OB, for budget planning |
| Reporting | Audit reports | Not implemented | **Important** | Who accessed what data, payroll approval chain, changes to sensitive fields, GDPR compliance reports |

## 11. Compliance & Audit

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Compliance | Audit trail / change log | Entity base has UpdatedAt/UpdatedBy but no comprehensive audit log | **Critical** | Implement full audit logging: every create/update/delete with user, timestamp, old/new values for all entities |
| Compliance | GDPR data access request (registerutdrag) | Not implemented | **Critical** | Build workflow to handle Art 15 requests: collect all data about an employee, generate report, track 1-month deadline |
| Compliance | GDPR data deletion / anonymization | Not implemented | **Important** | Build retention engine: auto-flag data past retention period, anonymize/delete with legal basis check (7yr payroll, 2yr recruitment, etc.) |
| Compliance | GDPR data export (portability) | Not implemented | **Important** | Export all employee data in machine-readable format (JSON/CSV) per Art 20 |
| Compliance | Data retention policy enforcement | Not implemented | **Important** | Automated checks for data past retention period per the documented retention schedule |
| Compliance | MBL compliance (co-determination) | Not implemented | **Important** | Track MBL consultation requirements for significant decisions: redundancies, organizational changes, policy changes; document union negotiations |
| Compliance | ATL compliance dashboard | ArbetstidslagenValidator exists but no dashboard | **Important** | Build compliance dashboard showing ATL violations across organization: dygnsvila, veckovila, overtime limits |
| Compliance | Diskrimineringslag compliance | Not implemented | **Nice-to-have** | Track and report on diversity metrics, pay equity analysis (lonekarlaggning required every 3 years per Diskrimineringslagen) |
| Compliance | Working environment (AFS compliance) | Not tracked | **Nice-to-have** | Systematiskt arbetsmiljoarbete (SAM) tracking: risk assessments, incident reporting, action plans |

## 12. Document Management

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Documents | Employee document storage | No document management at all | **Critical** | Build document storage per employee: contracts, certificates, lakarintyg, performance reviews, with access control |
| Documents | Document templates | Not implemented | **Important** | Template library for: employment contracts, offer letters, warning letters, certificates, reference letters |
| Documents | Digital signature | Not implemented | **Important** | Integrate e-signing (e.g., BankID or equivalent) for employment contracts, policy acknowledgments |
| Documents | Receipt/attachment handling | TravelClaim has KvittoId field but no file storage | **Important** | Build file upload and storage (blob storage): receipts for travel, CV files for recruitment, lakarintyg for sick leave |
| Documents | Policy/handbook distribution | Not implemented | **Nice-to-have** | Distribute policies to employees, track read/acknowledgment |
| Documents | Organizational documents | Not implemented | **Nice-to-have** | Store and manage organizational documents: collective agreements, local agreements, delegationsordning |

## 13. Workflow & Approvals

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Workflow | Generic approval workflow | Case entity has basic status flow (Oppen -> VantarGodkannande -> Godkand -> Nekad) | Partial | Build configurable multi-step approval: route to correct approver based on type and amount, escalation rules, delegation |
| Workflow | Multi-level approval | Not implemented | **Important** | Some items need multiple approvals (e.g., salary changes need both manager and HR); build approval chains |
| Workflow | Approval delegation | Not implemented | **Important** | When manager is absent, auto-delegate to substitute; configurable delegation rules |
| Workflow | Automated workflow triggers | Domain events exist but not connected to notifications/workflows | **Critical** | Build event-driven workflows: sick day 8 -> lakarintyg reminder, LAS threshold -> alert, overtime limit approaching -> warning |
| Workflow | Task/to-do management | Not implemented | **Important** | Build task system: onboarding tasks, follow-up reminders, compliance deadlines, assigned to specific users |
| Workflow | Batch approvals | Not implemented | **Important** | Allow managers to approve multiple items at once (e.g., 10 vacation requests) |
| Workflow | Workflow status tracking | No workflow history/timeline | **Important** | Show timeline of workflow steps: submitted -> approved by -> processed by -> completed, with timestamps |

## 14. Communication & Notifications

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Notifications | Email notifications | Not implemented at all | **Critical** | Build email notification system: approval requests, status changes, reminders, payslip availability |
| Notifications | In-app notifications | Not implemented | **Critical** | Build notification center in UI: unread count badge, notification list, click-through to relevant page |
| Notifications | SMS notifications | Not implemented | **Important** | SMS for urgent items: shift callouts, schedule changes, time-sensitive approvals |
| Notifications | Notification preferences | Not implemented | **Important** | Let users configure which notifications they receive and via which channel (email, in-app, SMS) |
| Notifications | Automated reminders | Not implemented | **Critical** | Scheduled reminders: lakarintyg day 8, LAS threshold approaching, vacation balance before year-end, mandatory training expiring |
| Communication | Internal messaging | Not implemented | **Nice-to-have** | Manager-to-employee messaging within the system for HR-related communication |
| Communication | Announcement/news feed | Not implemented | **Nice-to-have** | Organization-wide or unit-specific announcements on dashboard |

## 15. Mobile Access

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Mobile | Responsive web design | Blazor Server app; likely basic responsiveness | **Important** | Ensure all pages work well on mobile: responsive layouts, touch-friendly buttons, mobile navigation |
| Mobile | Mobile app (or PWA) | Not implemented | **Important** | Build Progressive Web App or native app for: schedule viewing, time clock, leave requests, approvals, notifications |
| Mobile | Push notifications | Not implemented | **Important** | Push notifications for mobile: shift changes, approval requests, schedule updates |
| Mobile | Mobile time clock | Not implemented | **Important** | Allow clock in/out from mobile with location validation (geofencing for on-site requirement) |
| Mobile | Manager mobile approvals | Not implemented | **Important** | One-tap approve/reject from mobile notification; essential for healthcare managers who are not at desks |

## 16. Data Management (Import/Export)

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Data | HEROMA data migration | Detailed migration plan exists (heroma-migrering-analys.md) with data mapping | Exists | Execute per plan; build ETL pipeline, validation, parallel run |
| Data | Bulk data import | Not implemented | **Important** | Admin UI for bulk importing employee data, organizational changes, salary updates (CSV/Excel) |
| Data | Data export (Excel/CSV) | Not implemented | **Critical** | Add export buttons on all list views; export to Excel/CSV for ad-hoc analysis |
| Data | Integration monitoring dashboard | IntegrationHub has 16 adapters but no monitoring | **Important** | Build dashboard showing integration status: last sync time, error count, queue depth per adapter |
| Data | Raindance accounting integration | Raindance adapter exists | Partial | Ensure complete implementation: accounting dimensions, posting rules, period-end reconciliation |
| Data | Data backup and recovery | No application-level backup management | **Nice-to-have** | Database backup strategy documentation, point-in-time recovery capability, disaster recovery plan |

## 17. Security & Access Control

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| Security | Role-based access control (RBAC) | 7 roles defined (Anstalld, Chef, HRAdmin, HRSpecialist, Loneadmin, Systemadmin, FackligRepresentant) with auth policies | Partial | Implement RBAC enforcement on all API endpoints and UI pages; many endpoints currently lack [Authorize] attributes |
| Security | Unit-based access scoping | Not implemented | **Critical** | Manager should only see employees in their unit; HR specialist scoped to their division; currently no data-level access control |
| Security | Sensitive data encryption | Employee entity has bank/personnummer fields; noted as "krypteras i databas" | Partial | Verify pgcrypto encryption is actually implemented; add encryption for health data (HalsoSAM), salary data |
| Security | Health data separation | HalsoSAM data in same database | **Important** | Implement stricter access controls on health/rehab data; only HR and direct manager; separate audit log |
| Security | Session management | JWT-based auth configured | Partial | Implement session timeout, forced re-auth for sensitive operations (salary changes, bank details) |
| Security | Two-factor authentication | Not implemented | **Important** | 2FA for admin roles (HR-admin, Loneadmin, Systemadmin); BankID integration for employee self-service login |
| Security | API rate limiting | Not implemented | **Nice-to-have** | Rate limiting on API endpoints to prevent abuse |
| Security | Penetration testing / security audit | No evidence | **Important** | Conduct security audit before production deployment |

## 18. User Experience

| Category | Feature | Current Status | Gap Level | What needs to be done |
|----------|---------|----------------|-----------|----------------------|
| UX | Design system consistency | RhrCard, RhrBadge, RhrButton, RhrAlert, RhrTable, RhrTrafficLight components exist | Partial | Expand design system: form components, date pickers, modal dialogs, dropdowns, search, pagination |
| UX | Delete/archive functionality | No delete on any entity | **Important** | Add soft-delete/archive on all entities with confirmation dialog; cascading archive logic |
| UX | Search functionality | No search on any page | **Critical** | Build global search (employees by name/personnummer) and per-page search/filter on all list views |
| UX | Pagination | Not implemented on lists | **Important** | Add pagination on all list endpoints and UI tables; currently all queries use .Take(50) |
| UX | Sorting and filtering | Not implemented | **Important** | Add column sorting and filtering on all tables |
| UX | Breadcrumb navigation | Not implemented | **Nice-to-have** | Add breadcrumb trail for deep navigation (e.g., Organization > Unit > Employee > Employment) |
| UX | Loading states and error handling | Basic loading states exist | **Important** | Consistent loading spinners, error boundaries, retry logic, user-friendly error messages in Swedish |
| UX | Form validation | Minimal client-side validation | **Important** | Add comprehensive form validation: required fields, format validation, real-time feedback in Swedish |
| UX | Keyboard navigation | Not considered | **Nice-to-have** | Ensure full keyboard accessibility (WCAG 2.1 compliance required for public sector) |
| UX | Accessibility (WCAG 2.1 AA) | Not audited | **Critical** | Swedish public sector must meet WCAG 2.1 AA (DOS-lagen); audit and fix: screen reader support, contrast, focus management |
| UX | Multi-language support | Swedish only (appropriate) | **Nice-to-have** | Consider English as secondary language for non-Swedish-speaking staff in healthcare |
| UX | Print-friendly views | Not implemented | **Nice-to-have** | Print-optimized CSS for schedules, payslips, reports |
| UX | Dark mode | Not implemented | **Nice-to-have** | Optional dark mode for night shift users |
| UX | Contextual help / tooltips | Swedish law info boxes exist on some pages | Partial | Expand help system: tooltips on all fields, link to relevant law text, FAQ section |

---

## Summary by Priority

### Must Fix Before Production (Critical - 24 items)

1. **Edit employee data in UI** - cannot manage employees without editing
2. **Employee detail view** - core daily HR work requires comprehensive view
3. **Offboarding/termination workflow** - legal requirement for correct final pay
4. **Employee self-service portal (real data)** - replaces HEROMA self-service
5. **Update own contact info** - basic self-service expectation
6. **Sick leave self-reporting** - daily need for 10,000+ employees
7. **Manager portal with real data** - managers need operational overview
8. **Approve/reject workflow** - core manager function
9. **Role-based views** - security and usability requirement
10. **Vacation balance engine** - legal requirement (Semesterlagen)
11. **Leave balance dashboard** - employees must see their balances
12. **Performance reviews (medarbetarsamtal)** - annual requirement per AB
13. **Mandatory training tracking** - patient safety requirement in healthcare
14. **Payroll review dashboard** - cannot approve payroll without reviewing
15. **Pay slip generation** - legal requirement per Lonelagen
16. **Timesheet/time reporting** - foundation for correct pay
17. **Executive HR dashboard (real data)** - leadership requires operational data
18. **Standard report library** - daily HR operations depend on reports
19. **Audit trail** - GDPR and internal audit requirement
20. **GDPR registerutdrag** - legal requirement, 1-month deadline
21. **Employee document storage** - contracts, certificates must be stored
22. **Automated workflow triggers** - legal deadlines (lakarintyg, FK notification)
23. **Email notifications** - system is unusable without notifications
24. **In-app notifications** - users need to know about pending actions
25. **Automated reminders** - legal compliance depends on timely reminders
26. **Unit-based access scoping** - security requirement
27. **Search functionality** - cannot find employees in 10,000+ organization
28. **WCAG 2.1 AA accessibility** - legal requirement (DOS-lagen)
29. **Data export (Excel/CSV)** - basic operational need

### Should Have for Competitive Parity (Important - 31 items)

Employment changes, probation tracking, contract generation, employment certificates, shift swap UI, travel claims UI, leave request workflow, absence calendar, parental leave, VAB, ATL dashboard, MBL compliance, document templates, digital signatures, file uploads, multi-level approval, delegation, batch approvals, SMS notifications, mobile responsive, mobile app/PWA, bulk import, integration monitoring, health data separation, 2FA, and more.

### Nice-to-Have for Excellence (18 items)

360 feedback, succession planning, e-learning integration, workforce analytics, cost simulation, internal messaging, dark mode, multi-language, and more.

---

## Comparison with HEROMA Specific Features

HEROMA (CGI) provides these features that RegionHR must match to be a valid replacement:

| HEROMA Feature | RegionHR Status | Gap |
|----------------|-----------------|-----|
| Employee self-service (schedule view, absence registration, pay slips) | ViewModels defined, UI partially built | Must complete with real data |
| Manager self-service (approve absence, schedule management) | Basic approval page exists | Must build manager-specific views |
| Schedule planning (24/7 healthcare) | Domain model and ATL validator exist | Must complete UI with drag-and-drop scheduling |
| Staffing overview | StaffingOverviewService exists | Must build UI |
| Salary/payroll processing | Comprehensive engine built | Must build review/approval UI |
| Travel/expense management | Domain and API exist | Must build self-service UI |
| Competence management | MinKompetens adapter exists | Must build internal module |
| Recruitment | Basic flow implemented | Must add communication and Platsbanken |
| Rehabilitation tracking (like HalsoSAM) | RehabCase and follow-ups exist | Must enhance with more structured workflow |
| LAS tracking | Full LAS accumulation engine | Dashboard exists; continue maintaining |
| Report generation | Minimal | Must build comprehensive report library |
| Mobile app | Not started | HEROMA has Android app; must provide mobile access |

---

*This analysis should be revisited quarterly as development progresses. Critical items should be addressed in priority order before go-live.*

Sources:
- [Workday vs Oracle HCM vs SAP SuccessFactors Comparison](https://www.outsail.co/post/workday-vs-oracle-hcm-vs-sap-successfactors)
- [HRIS vs HRMS vs HCM: 2025 Features Guide](https://harmonyhr.org/blog/what-hris-21-must-have-features-benefits-2025-guide.html)
- [HEROMA - CGI](https://www.cgi.com/se/sv/heroma)
- [Visma HRM-system](https://www.visma.se/hrm-system/)
- [Visma HRM and payroll for municipalities](https://www.visma.com/public-sector/hrm-and-payroll)
- [BambooHR vs Personio Comparison](https://www.selecthub.com/hr-management-software/bamboohr-vs-personio/)
- [HR-system Sverige - Headcount HR](https://headcounthr.se/hr-system/)
- [HRIS Requirements Checklist - HiBob](https://www.hibob.com/hr-tools/hris-requirements-template/)
- [Compliance-Ready HRIS Features](https://www.outsail.co/post/compliance-ready-hris-key-features-for-regulated-industries)
- [HR Mobile App Features 2026](https://payrun.app/blog/hr-mobile-app-features)
- [Sweden Employment Protection Act (LAS)](https://www.revea.se/en/news/swedens-updated-employment-law---what-you-need-to-know)
- [Employment Laws in Sweden - ICLG](https://iclg.com/practice-areas/employment-and-labour-laws-and-regulations/sweden)
- [HRIS Data Analytics - Executive Dashboards](https://www.outsail.co/post/hris-data-analytics-turning-hr-metrics-into-executive-dashboards)
- [16 Most Common HRMS Modules](https://www.hrmsworld.com/16-most-common-hrms-modules.html)
