using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Services;
using RegionHR.Platform.Domain;

namespace RegionHR.Api.Endpoints;

public static class MarketplaceEndpoints
{
    public static WebApplication MapMarketplaceEndpoints(this WebApplication app)
    {
        var marketplace = app.MapGroup("/api/v1/platform/tillagg").WithTags("Marknadsplats").RequireAuthorization();

        // ============================================================
        // Lista tillagg
        // ============================================================

        marketplace.MapGet("/", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var extensions = await db.Extensions.OrderBy(e => e.Namn).ToListAsync(ct);
            var installations = await db.ExtensionInstallations.ToListAsync(ct);

            return Results.Ok(extensions.Select(e =>
            {
                var installation = installations.FirstOrDefault(i => i.ExtensionId == e.Id);
                return new
                {
                    e.Id,
                    e.Namn,
                    e.Version,
                    e.Forfattare,
                    e.Beskrivning,
                    typ = e.Typ.ToString(),
                    e.Licens,
                    e.Kompatibilitet,
                    installerad = installation != null,
                    installationsStatus = installation?.Status.ToString()
                };
            }));
        }).WithName("ListExtensions");

        // ============================================================
        // Importera .openhr-paket
        // ============================================================

        marketplace.MapPost("/importera", async (HttpRequest request, ExtensionPackageService svc, CancellationToken ct) =>
        {
            if (!request.HasFormContentType || request.Form.Files.Count == 0)
                return Results.BadRequest(new { error = "Ingen fil bifogad. Ladda upp en .openhr-fil." });

            var file = request.Form.Files[0];
            using var stream = file.OpenReadStream();

            try
            {
                var extension = await svc.ImportAsync(stream, ct);
                return Results.Ok(new
                {
                    extension.Id,
                    extension.Namn,
                    extension.Version,
                    typ = extension.Typ.ToString(),
                    meddelande = $"Tillagg '{extension.Namn}' v{extension.Version} importerat."
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("ImportExtension").DisableAntiforgery();

        // ============================================================
        // Installera tillagg
        // ============================================================

        marketplace.MapPost("/{id:guid}/installera", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var extension = await db.Extensions.FirstOrDefaultAsync(e => e.Id == id, ct);
            if (extension is null)
                return Results.NotFound(new { error = "Tillagget hittades inte" });

            var existing = await db.ExtensionInstallations.FirstOrDefaultAsync(i => i.ExtensionId == id, ct);
            if (existing is not null)
                return Results.BadRequest(new { error = "Tillagget ar redan installerat" });

            var installation = ExtensionInstallation.Installera(extension.Id, extension.Version);
            await db.ExtensionInstallations.AddAsync(installation, ct);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                installation.Id,
                extensionId = extension.Id,
                extension.Namn,
                status = installation.Status.ToString(),
                meddelande = $"Tillagg '{extension.Namn}' installerat."
            });
        }).WithName("InstallExtension");

        // ============================================================
        // Inaktivera tillagg
        // ============================================================

        marketplace.MapPost("/{id:guid}/inaktivera", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var installation = await db.ExtensionInstallations.FirstOrDefaultAsync(i => i.ExtensionId == id, ct);
            if (installation is null)
                return Results.NotFound(new { error = "Ingen installation hittades for detta tillagg" });

            try
            {
                installation.Inaktivera();
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { installation.Id, status = installation.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("DisableExtension");

        // ============================================================
        // Aktivera tillagg
        // ============================================================

        marketplace.MapPost("/{id:guid}/aktivera", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var installation = await db.ExtensionInstallations.FirstOrDefaultAsync(i => i.ExtensionId == id, ct);
            if (installation is null)
                return Results.NotFound(new { error = "Ingen installation hittades for detta tillagg" });

            try
            {
                installation.Aktivera();
                await db.SaveChangesAsync(ct);
                return Results.Ok(new { installation.Id, status = installation.Status.ToString() });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("EnableExtension");

        return app;
    }
}
