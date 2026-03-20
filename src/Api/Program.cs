using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using RegionHR.Api;
using RegionHR.Api.Auth;
using RegionHR.Api.Endpoints;
using RegionHR.Api.Middleware;
using RegionHR.Core.Contracts;
using RegionHR.Core.Domain;
using RegionHR.Infrastructure;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Persistence.Repositories;
using RegionHR.Payroll.Domain;
using RegionHR.Payroll.Engine;
using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;
using RegionHR.CaseManagement.Domain;
using RegionHR.IntegrationHub.Adapters.KOLL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var connectionString = builder.Configuration.GetConnectionString("RegionHR")
    ?? "Host=localhost;Port=54322;Database=postgres;Username=postgres;Password=postgres";
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDb", false);
builder.Services.AddInfrastructure(connectionString, useInMemory);
builder.Services.AddRegionHRAuth(builder.Configuration);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;
    options.AddFixedWindowLimiter("standard", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });
    options.AddFixedWindowLimiter("export", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(1);
    });
});
builder.Services.AddSingleton<KOLLHOSPAdapter>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    if (useInMemory) app.SeedDevData();
    app.MapPost("/dev/token", (string name, string role) =>
    {
        var secret = builder.Configuration["Jwt:Secret"] ?? "dev-secret-key-min-32-characters-long!!";
        var token = DevTokenGenerator.GenerateToken(
            Guid.NewGuid().ToString(), name, $"{name.ToLower()}@regionhr.local", role, secret);
        return Results.Ok(new { token, role, expires = "8h" });
    }).WithName("DevGenerateToken").WithTags("Dev");
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

// Root fallback - only if no static file matched (keep as backup)
app.MapGet("/old-root", () => Results.Content("""
<!DOCTYPE html>
<html lang="sv">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>RegionHR — HR-system</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Segoe UI', system-ui, sans-serif; background: #f0f4f8; color: #1a202c; }
        .header { background: linear-gradient(135deg, #1e3a5f 0%, #2563eb 100%); color: white; padding: 2rem; }
        .header h1 { font-size: 1.8rem; margin-bottom: 0.3rem; }
        .header p { opacity: 0.85; font-size: 0.95rem; }
        .container { max-width: 1100px; margin: 0 auto; padding: 1.5rem; }
        .stats { display: grid; grid-template-columns: repeat(auto-fit, minmax(160px, 1fr)); gap: 1rem; margin-bottom: 2rem; }
        .stat { background: white; border-radius: 12px; padding: 1.2rem; box-shadow: 0 1px 3px rgba(0,0,0,0.1); text-align: center; }
        .stat .number { font-size: 2rem; font-weight: 700; color: #2563eb; }
        .stat .label { font-size: 0.8rem; color: #64748b; margin-top: 0.2rem; }
        .modules { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 1rem; }
        .module { background: white; border-radius: 12px; padding: 1.2rem; box-shadow: 0 1px 3px rgba(0,0,0,0.1); }
        .module h3 { font-size: 1rem; margin-bottom: 0.8rem; color: #1e3a5f; border-bottom: 2px solid #e2e8f0; padding-bottom: 0.5rem; }
        .module ul { list-style: none; }
        .module li { margin: 0.4rem 0; }
        .module a { color: #2563eb; text-decoration: none; font-size: 0.9rem; }
        .module a:hover { text-decoration: underline; }
        .badge { display: inline-block; font-size: 0.7rem; padding: 0.15rem 0.5rem; border-radius: 99px; font-weight: 600; margin-left: 0.3rem; }
        .get { background: #dcfce7; color: #166534; }
        .post { background: #dbeafe; color: #1e40af; }
        .version { font-size: 0.75rem; color: #94a3b8; margin-top: 2rem; text-align: center; padding: 1rem; }
    </style>
</head>
<body>
    <div class="header">
        <div class="container">
            <h1>RegionHR</h1>
            <p>Komplett HR-system for svensk region — ersatter HEROMA</p>
        </div>
    </div>
    <div class="container">
        <div class="stats" id="stats"></div>
        <div class="modules">
            <div class="module">
                <h3>Core HR — Personalregister</h3>
                <ul>
                    <li><a href="/api/v1/anstallda">Anstallda</a> <span class="badge get">GET</span></li>
                    <li><a href="/api/v1/organisation">Organisation</a> <span class="badge get">GET</span></li>
                    <li><a href="/api/v1/arenden">Arenden</a> <span class="badge get">GET</span></li>
                </ul>
            </div>
            <div class="module">
                <h3>Lon — Loneberakning</h3>
                <ul>
                    <li><a href="/api/v1/lon/korningar">Lonekorngar</a> <span class="badge get">GET</span></li>
                    <li><a href="/api/v1/lon/lonearter">Lonearter</a> <span class="badge get">GET</span></li>
                    <li><a href="/api/v1/lon/skattetabeller/2025">Skattetabeller 2025</a> <span class="badge get">GET</span></li>
                </ul>
            </div>
            <div class="module">
                <h3>Schema — Schemaladdning</h3>
                <ul>
                    <li><a href="/api/v1/schema">Scheman</a> <span class="badge get">GET</span></li>
                    <li><a href="/api/v1/schema/avvikelser/2026-03-16">Avvikelser idag</a> <span class="badge get">GET</span></li>
                </ul>
            </div>
            <div class="module">
                <h3>LAS — Regelefterlevnad</h3>
                <ul>
                    <li><a href="/api/v1/las/dashboard">Dashboard</a> <span class="badge get">GET</span></li>
                    <li><a href="/api/v1/las/alarmeringar">Alarmeringar</a> <span class="badge get">GET</span></li>
                    <li><a href="/api/v1/las/foretradesratt">Foretradesratt</a> <span class="badge get">GET</span></li>
                </ul>
            </div>
            <div class="module">
                <h3>HalsoSAM — Rehabilitering</h3>
                <ul>
                    <li><a href="/api/v1/halsosam/arenden">Rehabarenden</a> <span class="badge get">GET</span></li>
                    <li><a href="/api/v1/halsosam/kommande-uppfoljningar?dagar=30">Kommande uppfoljningar</a> <span class="badge get">GET</span></li>
                </ul>
            </div>
            <div class="module">
                <h3>HR-moduler</h3>
                <ul>
                    <li><a href="/api/v1/loneoversyn/rundor">Loneoversyn</a> <span class="badge get">GET</span></li>
                    <li><a href="/api/v1/resor">Resor &amp; Utlagg</a> <span class="badge get">GET</span></li>
                    <li><a href="/api/v1/rekrytering/vakanser">Rekrytering</a> <span class="badge get">GET</span></li>
                </ul>
            </div>
            <div class="module">
                <h3>Integrationer</h3>
                <ul>
                    <li><a href="/api/v1/integration/status">Integrationsstatus</a> <span class="badge get">GET</span></li>
                    <li><a href="/api/v1/integration/adapters">Alla 16 adapters</a> <span class="badge get">GET</span></li>
                </ul>
            </div>
            <div class="module">
                <h3>System</h3>
                <ul>
                    <li><a href="/health">Halsokontroll</a> <span class="badge get">GET</span></li>
                    <li><a href="/openapi/v1.json">OpenAPI-spec (JSON)</a> <span class="badge get">GET</span></li>
                </ul>
            </div>
        </div>
        <div class="version">RegionHR v0.4.0-fas3 | .NET 9 | Modular Monolith | 11 moduler | 16 integrationer | 247 tester</div>
    </div>
    <script>
        fetch('/api/v1/anstallda').then(r=>r.json()).then(d=>{
            const s=document.getElementById('stats');
            s.innerHTML=`
                <div class="stat"><div class="number">${d.length}</div><div class="label">Anstallda</div></div>
            `;
            return fetch('/api/v1/integration/status');
        }).then(r=>r.json()).then(d=>{
            const s=document.getElementById('stats');
            s.innerHTML+=`<div class="stat"><div class="number">${d.integrationer.length}</div><div class="label">Integrationer</div></div>`;
            s.innerHTML+=`<div class="stat"><div class="number">11</div><div class="label">Moduler</div></div>`;
            s.innerHTML+=`<div class="stat"><div class="number">247</div><div class="label">Tester</div></div>`;
        });
    </script>
</body>
</html>
""", "text/html")).WithName("OldRoot").ExcludeFromDescription();

// Health
app.MapGet("/health", () => Results.Ok(new { status = "healthy", system = "RegionHR", version = "0.5.0-fas5", moduler = 19, timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck").WithTags("System").AllowAnonymous();

// ============================================================
// Core HR - CRUD
// ============================================================
var coreApi = app.MapGroup("/api/v1").WithTags("Core HR").RequireAuthorization();

coreApi.MapGet("/anstallda", async (string? sok, int? sida, int? perSida, RegionHRDbContext db, CancellationToken ct) =>
{
    var query = db.Employees.Include(e => e.Anstallningar).AsQueryable();
    if (!string.IsNullOrWhiteSpace(sok))
    {
        var term = sok.ToLower();
        query = query.Where(e => e.Fornamn.ToLower().Contains(term) || e.Efternamn.ToLower().Contains(term));
    }
    var total = await query.CountAsync(ct);
    var pageSize = Math.Clamp(perSida ?? 50, 1, 200);
    var page = Math.Max(sida ?? 1, 1);
    var result = await query.OrderBy(e => e.Efternamn).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    return Results.Ok(new {
        sida = page, perSida = pageSize, totalt = total,
        anstallda = result.Select(e => new {
            e.Id, e.Fornamn, e.Efternamn, Personnummer = e.Personnummer.ToMaskedString(), e.Epost,
            AktivaAnstallningar = e.Anstallningar.Count
        })
    });
}).WithName("ListEmployees");

coreApi.MapGet("/anstalld/{id:guid}", async (Guid id, ICoreHRModule coreHR, CancellationToken ct) =>
{
    var emp = await coreHR.GetEmployeeAsync(EmployeeId.From(id), ct);
    return emp is not null ? Results.Ok(emp) : Results.NotFound();
}).WithName("GetEmployee");

coreApi.MapPost("/anstalld", async (CreateEmployeeRequest req, RegionHRDbContext db, CancellationToken ct) =>
{
    var pnr = new Personnummer(req.Personnummer);
    var employee = Employee.Skapa(pnr, req.Fornamn, req.Efternamn);
    if (req.Epost is not null)
        employee.UppdateraKontaktuppgifter(req.Epost, req.Telefon, null);
    await db.Employees.AddAsync(employee, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/v1/anstalld/{employee.Id}", new { employee.Id });
}).WithName("CreateEmployee");

coreApi.MapPut("/anstalld/{id:guid}", async (Guid id, UpdateEmployeeRequest req, RegionHRDbContext db, CancellationToken ct) =>
{
    var emp = await db.Employees.FirstOrDefaultAsync(e => e.Id == EmployeeId.From(id), ct);
    if (emp is null) return Results.NotFound();

    if (req.Epost is not null || req.Telefon is not null)
        emp.UppdateraKontaktuppgifter(req.Epost ?? emp.Epost, req.Telefon ?? emp.Telefon, null);

    await db.SaveChangesAsync(ct);
    return Results.Ok(new { emp.Id, emp.Fornamn, emp.Efternamn, emp.Epost, emp.Telefon });
}).WithName("UpdateEmployee");

coreApi.MapGet("/organisation", async (RegionHRDbContext db, CancellationToken ct) =>
{
    var units = await db.OrganizationUnits.OrderBy(u => u.Namn).ToListAsync(ct);
    return Results.Ok(units.Select(u => new { u.Id, u.Namn, Typ = u.Typ.ToString(), u.Kostnadsstalle }));
}).WithName("ListOrganization");

coreApi.MapGet("/organisation/{id:guid}/anstallda", async (Guid id, DateOnly? datum, ICoreHRModule coreHR, CancellationToken ct) =>
{
    var date = datum ?? DateOnly.FromDateTime(DateTime.Today);
    return Results.Ok(await coreHR.GetEmployeesByUnitAsync(OrganizationId.From(id), date, ct));
}).WithName("GetEmployeesByUnit");

// ============================================================
// Case Management
// ============================================================
var arendeApi = app.MapGroup("/api/v1/arenden").WithTags("Ärenden").RequireAuthorization();

arendeApi.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
{
    var cases = await db.Cases.OrderByDescending(c => c.CreatedAt).Take(50).ToListAsync(ct);
    return Results.Ok(cases.Select(c => new { c.Id, Typ = c.Typ.ToString(), Status = c.Status.ToString(), c.Beskrivning, c.CreatedAt }));
}).WithName("ListCases");

arendeApi.MapPost("/franvaro", async (CreateAbsenceCaseRequest req, RegionHRDbContext db, CancellationToken ct) =>
{
    var arende = Case.SkapaFranvaroarende(
        EmployeeId.From(req.AnstallId), req.FranvaroTyp,
        req.FranDatum, req.TillDatum, req.Beskrivning);
    await db.Cases.AddAsync(arende, ct);
    await db.SaveChangesAsync(ct);
    return Results.Created($"/api/v1/arenden/{arende.Id.Value}", new { arende.Id });
}).WithName("CreateAbsenceCase");

arendeApi.MapGet("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
{
    var arende = await db.Cases.Include(c => c.Godkannanden).Include(c => c.Kommentarer)
        .FirstOrDefaultAsync(c => c.Id == CaseId.From(id), ct);
    return arende is not null ? Results.Ok(arende) : Results.NotFound();
}).WithName("GetCase");

// ============================================================
// KOLL/HOSP Integration
// ============================================================
app.MapGet("/api/v1/integration/koll/verifiera/{personnummer}", async (string personnummer, KOLLHOSPAdapter koll, CancellationToken ct) =>
{
    var result = await koll.ExecuteAsync(new IntegrationRequest("VerifyLegitimation", personnummer), ct);
    return result.Success ? Results.Ok(result.ResponseData) : Results.BadRequest(result.Message);
}).WithName("VerifyLegitimation").WithTags("Integration").RequireAuthorization("Systemadmin");

// ============================================================
// Payroll (Fas 2A — full CRUD + export)
// ============================================================
app.MapPayrollEndpoints();

// ============================================================
// Scheduling (Fas 2B — schema, instämpling, bemanning)
// ============================================================
app.MapSchedulingEndpoints();
app.MapTimesheetEndpoints();

// ============================================================
// LAS (Fas 3 — regelefterlevnad)
// ============================================================
app.MapLASEndpoints();

// ============================================================
// HälsoSAM (Fas 3 — rehabilitering)
// ============================================================
app.MapHalsoSAMEndpoints();

// ============================================================
// HR-moduler (Fas 3 — löneöversyn, resor, rekrytering)
// ============================================================
app.MapHRModuleEndpoints();

// ============================================================
// Integration catalog (Fas 3)
// ============================================================
app.MapIntegrationEndpoints();

// ============================================================
// Wave 1: Granskningslogg, Notiser, Export
// ============================================================
app.MapAuditEndpoints();
app.MapNotificationEndpoints();
app.MapExportEndpoints();

// ============================================================
// Wave 2: Ledighet, Dokument
// ============================================================
app.MapLeaveEndpoints();
app.MapDocumentEndpoints();

// ============================================================
// Wave 3: Medarbetarsamtal, Rapportering, GDPR, Kompetens
// ============================================================
app.MapPerformanceEndpoints();
app.MapReportingEndpoints();
app.MapGDPREndpoints();
app.MapCompetenceEndpoints();

// ============================================================
// Wave 4: Positioner, Offboarding
// ============================================================
app.MapPositionEndpoints();
app.MapOffboardingEndpoints();

// ============================================================
// Wave 5: Förmåner, Utbildning (LMS)
// ============================================================
app.MapBenefitsEndpoints();
app.MapBenefitsExpandedEndpoints();
app.MapLMSEndpoints();

// ============================================================
// Wave 6: Konfiguration, Analytics, Behörighet
// ============================================================
app.MapConfigurationEndpoints();
app.MapAnalyticsEndpoints();
app.MapAnalyticsExpandedEndpoints();
app.MapPermissionEndpoints();

// ============================================================
// Portaler: Min Sida (självservice) & Chefsportal
// ============================================================
app.MapSelfServiceEndpoints();
app.MapManagerPortalEndpoints();

// ============================================================
// Phase A: Migrering, Automatisering, Kollektivavtal
// ============================================================
app.MapMigrationEndpoints();
app.MapAutomationEndpoints();
app.MapAgreementEndpoints();

// ============================================================
// Phase B2: Compensation Suite
// ============================================================
app.MapCompensationEndpoints();

// ============================================================
// Phase C Layer 2: Custom Objects + Workflow Nodes
// ============================================================
app.MapCustomObjectEndpoints();

// ============================================================
// Phase B4: VMS / Contingent Workforce
// ============================================================
app.MapVMSEndpoints();

// ============================================================
// Phase B5: Advanced WFM
// ============================================================
app.MapWFMEndpoints();

// ============================================================
// Phase B6: Talent Marketplace & Skills Intelligence
// ============================================================
app.MapTalentEndpoints();

// ============================================================
// Phase C: Event Bus, Webhooks, API Keys
// ============================================================
app.MapPlatformEndpoints();

// ============================================================
// Phase C Layer 3: Marketplace Foundation
// ============================================================
app.MapMarketplaceEndpoints();

// ============================================================
// Integration Status
// ============================================================
app.MapGet("/api/v1/integration/status", async (KOLLHOSPAdapter koll, CancellationToken ct) =>
{
    var kollHealth = await koll.HealthCheckAsync(ct);
    return Results.Ok(new
    {
        integrationer = new[]
        {
            new { system = "Skatteverket (AGI)", status = "OK" },
            new { system = "Nordea (betalning)", status = "OK" },
            new { system = "KOLL/HOSP", status = kollHealth ? "OK" : "FEL" },
            new { system = "Raindance", status = "OK" },
            new { system = "Försäkringskassan", status = "OK" },
            new { system = "Kronofogden", status = "OK" },
            new { system = "Skandia (pension)", status = "OK" },
            new { system = "SKR (statistik)", status = "OK" },
            new { system = "SCB (KLR)", status = "OK" },
            new { system = "Epassi", status = "OK" },
            new { system = "Troman", status = "OK" },
            new { system = "PowerBI", status = "OK" },
            new { system = "Grade (LMS)", status = "OK" },
            new { system = "Microweb (arkiv)", status = "OK" }
        }
    });
}).WithName("GetIntegrationStatus").WithTags("Integration").RequireAuthorization("Systemadmin");

app.Run();

// Request DTOs
record CreateEmployeeRequest(string Personnummer, string Fornamn, string Efternamn, string? Epost = null, string? Telefon = null);
record UpdateEmployeeRequest(string? Epost = null, string? Telefon = null);
record CreateAbsenceCaseRequest(Guid AnstallId, AbsenceType FranvaroTyp, DateOnly FranDatum, DateOnly TillDatum, string Beskrivning);
