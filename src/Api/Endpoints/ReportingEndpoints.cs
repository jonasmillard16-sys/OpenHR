using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Reporting;
using RegionHR.Reporting.Domain;

namespace RegionHR.Api.Endpoints;

public static class ReportingEndpoints
{
    public static WebApplication MapReportingEndpoints(this WebApplication app)
    {
        var rapporter = app.MapGroup("/api/v1/rapporter").WithTags("Rapporter").RequireAuthorization("ChefEllerHR");

        // ============================================================
        // Lista rapportdefinitioner
        // ============================================================

        rapporter.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var definitions = await db.ReportDefinitions
                .OrderBy(r => r.Namn)
                .ToListAsync(ct);

            return Results.Ok(definitions.Select(r => new
            {
                r.Id, r.Namn, r.Beskrivning,
                Typ = r.Typ.ToString(),
                r.ArSchemalagd, r.CronExpression, r.MottagareEpost
            }));
        }).WithName("ListReportDefinitions");

        // ============================================================
        // Skapa rapportdefinition
        // ============================================================

        rapporter.MapPost("/", async (CreateReportDefinitionRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<ReportType>(req.Typ, true, out var typ))
                return Results.BadRequest(new { error = $"Ogiltig typ: {req.Typ}. Giltiga värden: {string.Join(", ", Enum.GetNames<ReportType>())}" });

            var definition = ReportDefinition.Skapa(req.Namn, req.Beskrivning, typ);
            await db.ReportDefinitions.AddAsync(definition, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/rapporter/{definition.Id}", new
            {
                definition.Id, definition.Namn, definition.Beskrivning,
                Typ = definition.Typ.ToString()
            });
        }).WithName("CreateReportDefinition");

        // ============================================================
        // Schemalägg rapport
        // ============================================================

        rapporter.MapPost("/{id:guid}/schemalagd", async (Guid id, SetScheduleRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var definition = await db.ReportDefinitions.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (definition is null) return Results.NotFound();

            definition.SattSchemalagd(req.CronExpression, req.MottagareEpost);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                definition.Id, definition.Namn,
                definition.ArSchemalagd, definition.CronExpression, definition.MottagareEpost
            });
        }).WithName("SetReportSchedule");

        // ============================================================
        // Lista rapportkörningar
        // ============================================================

        rapporter.MapGet("/korningar", async (Guid reportId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var executions = await db.ReportExecutions
                .Where(e => e.ReportDefinitionId == reportId)
                .OrderByDescending(e => e.StartadVid)
                .ToListAsync(ct);

            return Results.Ok(executions.Select(e => new
            {
                e.Id, e.ReportDefinitionId,
                Status = e.Status.ToString(),
                e.StartadVid, e.SlutfordVid, e.FelMeddelande
            }));
        }).WithName("ListReportExecutions");

        // ============================================================
        // Starta rapportkörning (genererar rapport med data)
        // ============================================================

        rapporter.MapPost("/{id:guid}/kor", async (Guid id, RegionHRDbContext db, ReportGenerator generator, CancellationToken ct) =>
        {
            var definition = await db.ReportDefinitions.FirstOrDefaultAsync(r => r.Id == id, ct);
            if (definition is null) return Results.NotFound();

            var execution = ReportExecution.Starta(id);
            await db.ReportExecutions.AddAsync(execution, ct);
            await db.SaveChangesAsync(ct);

            try
            {
                var bytes = await generator.GenerateAsync(definition.Typ, ct);
                var fileName = $"rapport_{definition.Typ}_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";
                execution.Slutfor($"/reports/{fileName}");
                await db.SaveChangesAsync(ct);

                return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                execution.MarkeraFel(ex.Message);
                await db.SaveChangesAsync(ct);
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("StartReportExecution");

        return app;
    }
}

// Request DTOs
record CreateReportDefinitionRequest(string Namn, string Beskrivning, string Typ);
record SetScheduleRequest(string CronExpression, string MottagareEpost);
