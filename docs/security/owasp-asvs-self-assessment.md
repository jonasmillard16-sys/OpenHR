# OpenHR OWASP ASVS Self-Assessment

**Version:** 2026-03-21
**ASVS Version:** 4.0.3
**Target Level:** Level 1 (Opportunistic)
**Application:** OpenHR — Blazor Server HR System

---

## Overview

This document maps OpenHR's security controls to the OWASP Application Security Verification Standard (ASVS) Level 1 requirements. Level 1 is appropriate for an initial self-assessment. The organization should aim for Level 2 before production deployment with sensitive data.

**Legend:**
- PASS — Requirement met
- PARTIAL — Partially met, with documented gap
- FAIL — Not met, remediation planned
- N/A — Not applicable to this application type

---

## V1: Architecture, Design and Threat Modeling

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 1.1.1 | Defined security architecture | PASS | Modular monolith with schema-per-module isolation, RLS, role-based access |
| 1.1.2 | Verified deployment architecture | PASS | Docker Compose with network isolation, see `docs/security/deployment-guide.md` |
| 1.1.3 | All application components identified | PASS | Blazor Server, PostgreSQL, SignalR — no external SaaS dependencies |
| 1.1.4 | Threat model for the application | PARTIAL | Informal threat assessment completed; formal threat model (STRIDE/DREAD) recommended |

---

## V2: Authentication

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 2.1.1 | Passwords stored with approved hash | N/A | Current demo-auth uses session storage without passwords. Production should use BankID/SITHS (external identity provider) |
| 2.1.2 | Password minimum length ≥ 8 | N/A | No password-based auth. Uses BankID/SITHS simulation |
| 2.1.5 | Users can change credentials | N/A | No local credentials — delegated to external IdP |
| 2.2.1 | Anti-automation controls on auth | PASS | Rate limiting: 100 req/min per IP address |
| 2.3.1 | Session tokens generated on login | PASS | ASP.NET Core ProtectedSessionStorage generates per-session tokens |
| 2.5.1 | Application uses centralized auth | PASS | Single AuthService for all authentication |
| 2.7.1 | MFA available for sensitive operations | PARTIAL | BankID/SITHS are inherently 2FA (something you have + something you know). Demo mode lacks this. |

**Gap:** Demo authentication mode does not provide real security. Production deployment MUST integrate with BankID or SITHS for healthcare environments. This is the single most critical security gap.

---

## V3: Session Management

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 3.1.1 | Application uses framework session management | PASS | ASP.NET Core ProtectedSessionStorage with Data Protection API |
| 3.2.1 | Sessions invalidated on logout | PASS | `AuthService.LogoutAsync()` clears all session keys |
| 3.2.2 | Sessions timeout after inactivity | PASS | 30-minute idle timeout via `SessionTimeoutMiddleware` |
| 3.2.3 | Sessions invalidated after password change | N/A | No local passwords |
| 3.3.1 | Session tokens are sufficiently random | PASS | ASP.NET Core Data Protection generates cryptographically random tokens |
| 3.3.2 | Session tokens have HttpOnly flag | PASS | ProtectedSessionStorage uses encrypted cookies via Blazor Server |
| 3.4.1 | Cookie-based session tokens have Secure flag | PARTIAL | Set when running over HTTPS (production); not enforced in development |
| 3.4.2 | Cookie-based session tokens have SameSite attribute | PASS | ASP.NET Core default: SameSite=Lax |

---

## V4: Access Control

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 4.1.1 | Application enforces access control at server | PASS | All access checks in server-side Blazor (no client-side bypass possible) |
| 4.1.2 | Access control on every request | PASS | AuthService checked in layout components; unauthorized users redirected to /login |
| 4.1.3 | Principle of least privilege | PASS | 7 roles: Anställd sees only own data, Chef sees own unit, HR sees all |
| 4.2.1 | Sensitive data access requires authorization | PASS | Role checks on all pages; unit-based access via UnitAccessScopeService |
| 4.2.2 | Vertical access control (role escalation) | PASS | Roles stored server-side in ProtectedSessionStorage; cannot be modified by client |
| 4.3.1 | Admin functions restricted to admins | PASS | `/admin/*` routes check `Auth.IsAdmin` or `Auth.IsHR` |
| 4.3.2 | Directory browsing disabled | PASS | ASP.NET Core default: UseStaticFiles() serves only explicitly placed files |

### Database-Level Access Control

| Control | Status | Notes |
|---------|--------|-------|
| PostgreSQL RLS | PASS | Row-level security policies per module schema |
| Schema isolation | PASS | Each module has its own PostgreSQL schema |
| HälsoSAM restricted | PASS | Health data accessible only to case owner + HR |
| pgcrypto encryption | PASS | Personnummer and bank details encrypted at column level |

---

## V5: Validation, Sanitization and Encoding

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 5.1.1 | HTTP parameter pollution protection | PASS | ASP.NET Core model binding handles parameter deduplication |
| 5.1.3 | All input validated server-side | PASS | Blazor Server renders on server; all form validation is server-side |
| 5.2.1 | All HTML output encoded | PASS | Blazor automatically HTML-encodes all `@` expressions. `MarkupString` used only for trusted content |
| 5.2.2 | Unstructured data sanitized | PASS | No raw HTML input accepted. MudBlazor components handle encoding |
| 5.3.1 | SQL injection prevented | PASS | Entity Framework Core uses parameterized queries exclusively |
| 5.3.4 | OS command injection prevented | PASS | No `Process.Start()` or shell execution in application code |
| 5.5.1 | Deserialization of untrusted data prevented | PASS | System.Text.Json with typed models; no BinaryFormatter usage |

---

## V6: Stored Cryptography

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 6.1.1 | Regulated data encrypted at rest | PASS | pgcrypto for personnummer, bank account numbers |
| 6.2.1 | Industry-proven cryptographic algorithms | PASS | pgcrypto uses AES-256; ASP.NET Data Protection uses AES-256-GCM |
| 6.2.2 | Encryption keys managed securely | PARTIAL | Data Protection keys stored on filesystem by default. Production should use key vault |
| 6.3.1 | Random values from CSPRNG | PASS | Uses `System.Security.Cryptography.RandomNumberGenerator` via ASP.NET Core |
| 6.4.1 | Cryptographic architecture documented | PARTIAL | Documented in this file; dedicated cryptographic architecture document recommended |

**Gap:** Data Protection key management should use a dedicated key store (e.g., HashiCorp Vault or file system with restricted permissions) rather than default file-based storage.

---

## V7: Error Handling and Logging

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 7.1.1 | Generic error messages to users | PASS | Production mode uses `/Error` page. No stack traces shown |
| 7.1.2 | Exception handling covers all code paths | PASS | Global exception handler + try/catch on DB operations |
| 7.2.1 | All auth decisions logged | PASS | Login/logout logged via structured JSON logging |
| 7.2.2 | All access control failures logged | PARTIAL | Auth redirects logged; explicit "access denied" events should be more detailed |
| 7.3.1 | No sensitive data in logs | PASS | Structured logging does not include personnummer, passwords, or PII |
| 7.3.2 | Logging includes timestamp, event, user | PASS | JSON logging with `TimestampFormat`, event level, and correlation |
| 7.4.1 | Error handling prevents info leakage | PASS | Development error page disabled in Production environment |

### Audit Logging Detail

| Event Type | Logged | Details |
|------------|--------|---------|
| Create entity | Yes | Table name, entity ID, user, timestamp, new values |
| Update entity | Yes | Table name, entity ID, user, timestamp, old values, new values |
| Delete entity | Yes | Table name, entity ID, user, timestamp |
| Login | Yes | Username, role, method, timestamp |
| Logout | Yes | Username, timestamp |
| Access denied | Partial | Redirect to login logged; specific denial events need improvement |

---

## V8: Data Protection

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 8.1.1 | Sensitive data not stored in URL parameters | PASS | Blazor Server uses SignalR; no sensitive URL parameters |
| 8.1.2 | Sensitive data not in browser storage unprotected | PASS | ProtectedSessionStorage encrypts data client-side |
| 8.2.1 | Sensitive data not cached in responses | PASS | Security headers include no-cache directives for API responses |
| 8.3.1 | Sensitive data removed when no longer needed | PASS | RetentionCleanupService automatically removes expired data |
| 8.3.2 | PII has defined retention periods | PASS | See `docs/security/gdpr-compliance-guide.md` retention table |

---

## V9: Communications

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 9.1.1 | TLS used for all connections | PASS (production) | Caddy/nginx handles TLS termination. Development uses HTTP |
| 9.1.2 | TLS 1.2 or higher enforced | PASS | Configured in reverse proxy and PostgreSQL |
| 9.1.3 | Up-to-date TLS configuration | PASS | Caddy auto-configures modern TLS |
| 9.2.1 | Connections to external services use TLS | N/A | No runtime external service connections (integrations are export-based) |

---

## V10: Malicious Code

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 10.1.1 | Source code does not contain malicious code | PASS | Open source (AGPL-3.0), fully auditable |
| 10.2.1 | No undocumented features or backdoors | PASS | All features documented. Demo auth is clearly labeled |
| 10.3.1 | Dependency vulnerabilities checked | PARTIAL | `dotnet list package --vulnerable` should be run as part of CI |

**Gap:** Add `dotnet list package --vulnerable` to CI/CD pipeline for automated vulnerability scanning of NuGet dependencies.

---

## V11: Business Logic

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 11.1.1 | Business logic executed server-side | PASS | Blazor Server: all logic runs on server. No client-side business logic |
| 11.1.2 | Business logic cannot be bypassed | PASS | Domain methods enforce rules (e.g., LeaveRequest.Godkann() checks preconditions) |
| 11.1.3 | Business logic handles race conditions | PARTIAL | EF Core optimistic concurrency on key entities. More thorough testing needed |
| 11.1.4 | Rate limiting on business operations | PASS | Global rate limiting at 100 req/min per IP |

---

## V12: Files and Resources

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 12.1.1 | File uploads restricted by type | PASS | MudFileUpload with MIME type validation in Document upload flow |
| 12.1.2 | File uploads scanned for malware | FAIL | No antivirus scanning. Recommended: integrate ClamAV |
| 12.3.1 | User-submitted filenames sanitized | PASS | Files stored with GUID names; original filename stored in metadata |

**Gap:** Integrate ClamAV (FOSS antivirus) for uploaded file scanning. This is important for a healthcare organization handling documents.

---

## V13: API and Web Service

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 13.1.1 | All API endpoints use consistent auth | PASS | Blazor Server routes all through same auth pipeline |
| 13.1.3 | API responses use Content-Type header | PASS | ASP.NET Core sets appropriate Content-Type automatically |
| 13.2.1 | RESTful or consistent API style | PASS | Internal APIs use EF Core directly; external APIs use Minimal API pattern |

---

## V14: Configuration

| # | Requirement | Status | Notes |
|---|-------------|--------|-------|
| 14.1.1 | Server configuration hardened | PASS (production) | Docker containers, non-root user, minimal base images |
| 14.2.1 | Dependency versions pinned | PASS | NuGet packages with specific versions in .csproj files |
| 14.3.1 | Security headers present | PASS | CSP, X-Frame-Options DENY, X-Content-Type-Options, HSTS |
| 14.4.1 | HTTP security headers on all responses | PASS | SecurityHeadersMiddleware applies to all responses |

---

## Summary

### Overall ASVS Level 1 Compliance

| Category | Pass | Partial | Fail | N/A |
|----------|------|---------|------|-----|
| V1: Architecture | 3 | 1 | 0 | 0 |
| V2: Authentication | 3 | 1 | 0 | 4 |
| V3: Session Management | 5 | 1 | 0 | 1 |
| V4: Access Control | 5 | 0 | 0 | 0 |
| V5: Validation | 7 | 0 | 0 | 0 |
| V6: Cryptography | 3 | 2 | 0 | 0 |
| V7: Error Handling | 5 | 1 | 0 | 0 |
| V8: Data Protection | 5 | 0 | 0 | 0 |
| V9: Communications | 3 | 0 | 0 | 1 |
| V10: Malicious Code | 2 | 1 | 0 | 0 |
| V11: Business Logic | 3 | 1 | 0 | 0 |
| V12: Files | 2 | 0 | 1 | 0 |
| V13: API | 3 | 0 | 0 | 0 |
| V14: Configuration | 4 | 0 | 0 | 0 |
| **Total** | **53** | **8** | **1** | **6** |

### Critical Gaps and Remediation Plan

| Priority | Gap | Remediation | Timeline |
|----------|-----|-------------|----------|
| **Critical** | Demo auth in production | Integrate BankID/SITHS identity provider | Before go-live |
| **High** | No file upload virus scanning | Integrate ClamAV container for uploaded documents | Before go-live |
| **High** | Data Protection key management | Configure dedicated key store | Before go-live |
| **Medium** | Dependency vulnerability scanning | Add `dotnet list package --vulnerable` to CI | Next sprint |
| **Medium** | Formal threat model | Conduct STRIDE analysis | Within 3 months |
| **Low** | Detailed access denial logging | Add explicit "access denied" audit events | Next sprint |
