using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Configuration.Domain;

namespace RegionHR.Api.Endpoints;

public static class CustomObjectEndpoints
{
    public static WebApplication MapCustomObjectEndpoints(this WebApplication app)
    {
        // ============================================================
        // Custom Object Records — auto-generated CRUD per object name
        // ============================================================

        var custom = app.MapGroup("/api/v1/custom").WithTags("CustomObjects");

        // GET /api/v1/custom/{objektNamn} — list records
        custom.MapGet("/{objektNamn}", async (string objektNamn, RegionHRDbContext db, CancellationToken ct) =>
        {
            var obj = await db.CustomObjects
                .FirstOrDefaultAsync(o => o.Namn == objektNamn, ct);
            if (obj is null) return Results.NotFound(new { error = $"Custom object '{objektNamn}' hittades inte" });

            var records = await db.CustomObjectRecords
                .Where(r => r.CustomObjectId == obj.Id)
                .OrderByDescending(r => r.SkapadVid)
                .ToListAsync(ct);

            return Results.Ok(records.Select(r => new
            {
                r.Id, r.CustomObjectId, r.Data, r.SkapadAv, r.SkapadVid, r.UppdateradVid
            }));
        }).WithName("ListCustomObjectRecords");

        // POST /api/v1/custom/{objektNamn} — create record (validates against schema)
        custom.MapPost("/{objektNamn}", async (string objektNamn, CreateCustomObjectRecordRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var obj = await db.CustomObjects
                .FirstOrDefaultAsync(o => o.Namn == objektNamn, ct);
            if (obj is null) return Results.NotFound(new { error = $"Custom object '{objektNamn}' hittades inte" });

            var validationError = ValidateDataAgainstSchema(req.Data, obj.FaltSchema);
            if (validationError != null) return Results.BadRequest(new { error = validationError });

            var record = CustomObjectRecord.Skapa(obj.Id, req.Data, req.SkapadAv ?? "System");
            await db.CustomObjectRecords.AddAsync(record, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/custom/{objektNamn}/{record.Id}", new
            {
                record.Id, record.CustomObjectId, record.Data, record.SkapadAv, record.SkapadVid
            });
        }).WithName("CreateCustomObjectRecord");

        // PUT /api/v1/custom/{objektNamn}/{id} — update record
        custom.MapPut("/{objektNamn}/{id:guid}", async (string objektNamn, Guid id, UpdateCustomObjectRecordRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var obj = await db.CustomObjects
                .FirstOrDefaultAsync(o => o.Namn == objektNamn, ct);
            if (obj is null) return Results.NotFound(new { error = $"Custom object '{objektNamn}' hittades inte" });

            var record = await db.CustomObjectRecords
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomObjectId == obj.Id, ct);
            if (record is null) return Results.NotFound(new { error = $"Record {id} hittades inte" });

            var validationError = ValidateDataAgainstSchema(req.Data, obj.FaltSchema);
            if (validationError != null) return Results.BadRequest(new { error = validationError });

            record.UppdateraData(req.Data);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                record.Id, record.CustomObjectId, record.Data, record.SkapadAv, record.SkapadVid, record.UppdateradVid
            });
        }).WithName("UpdateCustomObjectRecord");

        // DELETE /api/v1/custom/{objektNamn}/{id} — delete record
        custom.MapDelete("/{objektNamn}/{id:guid}", async (string objektNamn, Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var obj = await db.CustomObjects
                .FirstOrDefaultAsync(o => o.Namn == objektNamn, ct);
            if (obj is null) return Results.NotFound(new { error = $"Custom object '{objektNamn}' hittades inte" });

            var record = await db.CustomObjectRecords
                .FirstOrDefaultAsync(r => r.Id == id && r.CustomObjectId == obj.Id, ct);
            if (record is null) return Results.NotFound(new { error = $"Record {id} hittades inte" });

            db.CustomObjectRecords.Remove(record);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        }).WithName("DeleteCustomObjectRecord");

        // ============================================================
        // Custom Object Definitions — admin API
        // ============================================================

        var platform = app.MapGroup("/api/v1/platform").WithTags("Platform");

        // GET /api/v1/platform/custom-objects — list definitions
        platform.MapGet("/custom-objects", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var objects = await db.CustomObjects
                .OrderBy(o => o.Namn)
                .ToListAsync(ct);

            return Results.Ok(objects.Select(o => new
            {
                o.Id, o.Namn, o.PluralNamn, o.Beskrivning,
                o.FaltSchema, o.Relationer, o.Ikon, o.SkapadVid
            }));
        }).WithName("ListCustomObjectDefinitions");

        return app;
    }

    /// <summary>
    /// Validates record data JSON against the custom object's field schema.
    /// Returns null if valid, or an error message if invalid.
    /// </summary>
    public static string? ValidateDataAgainstSchema(string dataJson, string schemaJson)
    {
        try
        {
            using var schemaDoc = JsonDocument.Parse(schemaJson);
            using var dataDoc = JsonDocument.Parse(dataJson);
            var dataRoot = dataDoc.RootElement;

            foreach (var field in schemaDoc.RootElement.EnumerateArray())
            {
                var fieldName = field.GetProperty("name").GetString()!;
                var fieldType = field.GetProperty("type").GetString()!;
                var required = field.TryGetProperty("required", out var reqProp) && reqProp.GetBoolean();

                if (!dataRoot.TryGetProperty(fieldName, out var value) || value.ValueKind == JsonValueKind.Null)
                {
                    if (required)
                        return $"Obligatoriskt falt '{fieldName}' saknas";
                    continue;
                }

                var valueStr = value.ToString();
                if (required && string.IsNullOrWhiteSpace(valueStr))
                    return $"Obligatoriskt falt '{fieldName}' far inte vara tomt";

                // Type-specific validation
                switch (fieldType)
                {
                    case "Number":
                        if (!decimal.TryParse(valueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _))
                            return $"Falt '{fieldName}' maste vara ett nummer";
                        break;
                    case "Date":
                        if (!DateTime.TryParse(valueStr, out _))
                            return $"Falt '{fieldName}' maste vara ett giltigt datum";
                        break;
                    case "Email":
                        if (!string.IsNullOrEmpty(valueStr) && !valueStr.Contains('@'))
                            return $"Falt '{fieldName}' maste vara en giltig e-postadress";
                        break;
                    case "YesNo":
                        if (valueStr != "true" && valueStr != "false" && valueStr != "True" && valueStr != "False")
                            return $"Falt '{fieldName}' maste vara true eller false";
                        break;
                    case "Dropdown":
                        if (field.TryGetProperty("options", out var optionsProp) && optionsProp.ValueKind == JsonValueKind.Array)
                        {
                            var options = optionsProp.EnumerateArray().Select(o => o.GetString()).ToList();
                            if (!options.Contains(valueStr))
                                return $"Falt '{fieldName}' har ogiltigt varde '{valueStr}'. Giltiga: {string.Join(", ", options)}";
                        }
                        break;
                }
            }

            return null; // Valid
        }
        catch (JsonException ex)
        {
            return $"Ogiltig JSON: {ex.Message}";
        }
    }
}

// Request DTOs
record CreateCustomObjectRecordRequest(string Data, string? SkapadAv);
record UpdateCustomObjectRecordRequest(string Data);
