using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Configuration.Domain;

namespace RegionHR.Api.Endpoints;

public static class ConfigurationEndpoints
{
    public static WebApplication MapConfigurationEndpoints(this WebApplication app)
    {
        var konfig = app.MapGroup("/api/v1/konfiguration").WithTags("Konfiguration").RequireAuthorization("Systemadmin");

        // ============================================================
        // Hämta tenant-konfiguration
        // ============================================================

        konfig.MapGet("/tenant", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var tenant = await db.TenantConfigurations.FirstOrDefaultAsync(t => t.ArAktiv, ct);
            if (tenant is null) return Results.NotFound();

            return Results.Ok(new
            {
                tenant.Id, tenant.TenantNamn, tenant.Organisationsnummer,
                tenant.Land, tenant.Sprak, tenant.Valuta,
                tenant.LogoUrl, tenant.Konfiguration, tenant.ArAktiv
            });
        }).WithName("GetTenantConfiguration");

        // ============================================================
        // Uppdatera tenant-konfiguration
        // ============================================================

        konfig.MapPut("/tenant", async (UpdateTenantRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var tenant = await db.TenantConfigurations.FirstOrDefaultAsync(t => t.ArAktiv, ct);
            if (tenant is null)
            {
                tenant = TenantConfiguration.Skapa(req.TenantNamn, req.Organisationsnummer, req.Land, req.Sprak);
                await db.TenantConfigurations.AddAsync(tenant, ct);
            }
            if (req.Konfiguration is not null)
                tenant.UppdateraKonfiguration(req.Konfiguration);

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                tenant.Id, tenant.TenantNamn, tenant.Organisationsnummer,
                tenant.Land, tenant.Sprak, tenant.Valuta, tenant.Konfiguration
            });
        }).WithName("UpdateTenantConfiguration");

        // ============================================================
        // Lista custom fields (filtrera på target)
        // ============================================================

        konfig.MapGet("/custom-fields", async (string? target, RegionHRDbContext db, CancellationToken ct) =>
        {
            var query = db.CustomFields.Where(f => f.ArAktiv);
            if (!string.IsNullOrWhiteSpace(target) && Enum.TryParse<CustomFieldTarget>(target, true, out var t))
                query = query.Where(f => f.Target == t);

            var fields = await query.OrderBy(f => f.Ordning).ToListAsync(ct);

            return Results.Ok(fields.Select(f => new
            {
                f.Id, f.FieldName, f.DisplayName,
                FieldType = f.FieldType.ToString(),
                Target = f.Target.ToString(),
                f.ArObligatorisk, f.Alternativ, f.Standardvarde, f.Ordning
            }));
        }).WithName("ListCustomFields");

        // ============================================================
        // Skapa custom field
        // ============================================================

        konfig.MapPost("/custom-field", async (CreateCustomFieldRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<CustomFieldType>(req.FieldType, true, out var fieldType))
                return Results.BadRequest(new { error = $"Ogiltig FieldType: {req.FieldType}. Giltiga värden: {string.Join(", ", Enum.GetNames<CustomFieldType>())}" });

            if (!Enum.TryParse<CustomFieldTarget>(req.Target, true, out var target))
                return Results.BadRequest(new { error = $"Ogiltig Target: {req.Target}. Giltiga värden: {string.Join(", ", Enum.GetNames<CustomFieldTarget>())}" });

            var field = CustomField.Skapa(req.FieldName, req.DisplayName, fieldType, target, req.Obligatorisk, req.Alternativ, req.Ordning);
            await db.CustomFields.AddAsync(field, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/konfiguration/custom-fields", new
            {
                field.Id, field.FieldName, field.DisplayName,
                FieldType = field.FieldType.ToString(),
                Target = field.Target.ToString()
            });
        }).WithName("CreateCustomField");

        // ============================================================
        // Hämta custom field values för en entity
        // ============================================================

        konfig.MapGet("/custom-values/{entityId}", async (string entityId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var values = await db.CustomFieldValues
                .Where(v => v.EntityId == entityId)
                .ToListAsync(ct);

            return Results.Ok(values.Select(v => new
            {
                v.Id, v.CustomFieldId, v.EntityId, v.Varde, v.UppdateradVid
            }));
        }).WithName("GetCustomFieldValues");

        // ============================================================
        // Sätt custom field value
        // ============================================================

        konfig.MapPost("/custom-value", async (SetCustomFieldValueRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var existing = await db.CustomFieldValues
                .FirstOrDefaultAsync(v => v.CustomFieldId == req.CustomFieldId && v.EntityId == req.EntityId, ct);

            if (existing is not null)
            {
                existing.UppdateraVarde(req.Varde);
            }
            else
            {
                existing = CustomFieldValue.Skapa(req.CustomFieldId, req.EntityId, req.Varde);
                await db.CustomFieldValues.AddAsync(existing, ct);
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                existing.Id, existing.CustomFieldId, existing.EntityId, existing.Varde, existing.UppdateradVid
            });
        }).WithName("SetCustomFieldValue");

        // ============================================================
        // Lista workflow definitions
        // ============================================================

        konfig.MapGet("/workflows", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var workflows = await db.WorkflowDefinitions
                .OrderBy(w => w.Namn)
                .ToListAsync(ct);

            return Results.Ok(workflows.Select(w => new
            {
                w.Id, w.Namn, w.TargetEntityType, w.StegDefinition, w.ArAktiv, w.SkapadVid
            }));
        }).WithName("ListWorkflowDefinitions");

        // ============================================================
        // Skapa/uppdatera workflow definition
        // ============================================================

        konfig.MapPost("/workflow", async (CreateWorkflowRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            WorkflowDefinition workflow;
            if (req.Id.HasValue)
            {
                workflow = await db.WorkflowDefinitions.FirstOrDefaultAsync(w => w.Id == req.Id.Value, ct)
                    ?? throw new InvalidOperationException("Workflow hittades inte");
                workflow.UppdateraSteg(req.StegDefinition);
            }
            else
            {
                workflow = WorkflowDefinition.Skapa(req.Namn, req.TargetEntityType, req.StegDefinition);
                await db.WorkflowDefinitions.AddAsync(workflow, ct);
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                workflow.Id, workflow.Namn, workflow.TargetEntityType,
                workflow.StegDefinition, workflow.ArAktiv
            });
        }).WithName("CreateOrUpdateWorkflow");

        return app;
    }
}

// Request DTOs
record UpdateTenantRequest(string TenantNamn, string Organisationsnummer, string? Land, string? Sprak, string? Konfiguration);
record CreateCustomFieldRequest(string FieldName, string DisplayName, string FieldType, string Target, bool Obligatorisk = false, string? Alternativ = null, int Ordning = 0);
record SetCustomFieldValueRequest(Guid CustomFieldId, string EntityId, string Varde);
record CreateWorkflowRequest(Guid? Id, string Namn, string TargetEntityType, string StegDefinition);
