using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Infrastructure.Storage;
using RegionHR.Documents.Domain;

namespace RegionHR.Api.Endpoints;

public static class DocumentEndpoints
{
    public static WebApplication MapDocumentEndpoints(this WebApplication app)
    {
        var dokument = app.MapGroup("/api/v1/dokument").WithTags("Dokument").RequireAuthorization();

        // ============================================================
        // Lista dokument för anställd
        // ============================================================

        dokument.MapGet("/", async (Guid anstallId, RegionHRDbContext db, CancellationToken ct) =>
        {
            var documents = await db.Documents
                .Where(d => d.AnstallId == anstallId)
                .OrderByDescending(d => d.UppladdadVid)
                .ToListAsync(ct);

            return Results.Ok(documents);
        }).WithName("ListDocuments");

        // ============================================================
        // Skapa dokumentpost
        // ============================================================

        dokument.MapPost("/", async (CreateDocumentRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<DocumentCategory>(req.Kategori, true, out var kategori))
                return Results.BadRequest(new { error = $"Ogiltig kategori: {req.Kategori}. Giltiga värden: {string.Join(", ", Enum.GetNames<DocumentCategory>())}" });

            var document = Document.Skapa(
                req.AnstallId,
                kategori,
                req.FileName,
                req.StoragePath,
                req.FileSizeBytes,
                req.ContentType,
                req.UppladdadAv);

            await db.Documents.AddAsync(document, ct);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/dokument/{document.Id}", new
            {
                document.Id,
                document.AnstallId,
                Kategori = document.Kategori.ToString(),
                document.FileName,
                document.UppladdadVid
            });
        }).WithName("CreateDocument");

        // ============================================================
        // Hämta dokument
        // ============================================================

        dokument.MapGet("/{id:guid}", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var document = await db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);
            return document is not null ? Results.Ok(document) : Results.NotFound();
        }).WithName("GetDocument");

        // ============================================================
        // Arkivera dokument
        // ============================================================

        dokument.MapPost("/{id:guid}/arkivera", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var document = await db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);
            if (document is null) return Results.NotFound();

            document.Archive();
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { document.Id, document.IsArchived });
        }).WithName("ArchiveDocument");

        // ============================================================
        // Ladda upp fil
        // ============================================================

        dokument.MapPost("/upload", async (HttpRequest request, RegionHRDbContext db, IFileStorageService storage, CancellationToken ct) =>
        {
            var form = await request.ReadFormAsync(ct);
            var file = form.Files.FirstOrDefault();
            if (file is null) return Results.BadRequest(new { error = "Ingen fil bifogad" });

            var anstallIdStr = form["anstallId"].FirstOrDefault();
            var kategoriStr = form["kategori"].FirstOrDefault();
            var uppladdadAv = form["uppladdadAv"].FirstOrDefault() ?? "system";

            if (!Guid.TryParse(anstallIdStr, out var anstallId))
                return Results.BadRequest(new { error = "Ogiltigt anstallId" });
            if (!Enum.TryParse<DocumentCategory>(kategoriStr, true, out var kategori))
                return Results.BadRequest(new { error = "Ogiltig kategori" });

            using var stream = file.OpenReadStream();
            var storagePath = await storage.UploadAsync(kategori.ToString(), file.FileName, stream, ct);

            var doc = Document.Skapa(anstallId, kategori, file.FileName, storagePath, file.Length, file.ContentType, uppladdadAv);
            var retention = RetentionPolicy.CalculateRetention(kategori, DateTime.UtcNow);
            doc.SetRetention(retention);

            await db.Documents.AddAsync(doc, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/dokument/{doc.Id}", new { doc.Id, doc.FileName, doc.StoragePath });
        }).WithName("UploadDocument").DisableAntiforgery();

        // ============================================================
        // Ladda ner fil
        // ============================================================

        dokument.MapGet("/{id:guid}/download", async (Guid id, RegionHRDbContext db, IFileStorageService storage, CancellationToken ct) =>
        {
            var doc = await db.Documents.FirstOrDefaultAsync(d => d.Id == id, ct);
            if (doc is null) return Results.NotFound();

            var stream = await storage.DownloadAsync(doc.StoragePath, ct);
            if (stream is null) return Results.NotFound(new { error = "Fil hittades inte i lagring" });

            return Results.File(stream, doc.ContentType, doc.FileName);
        }).WithName("DownloadDocument");

        // ============================================================
        // Dokumentmallar (Templates)
        // ============================================================

        dokument.MapGet("/mallar", async (RegionHRDbContext db, CancellationToken ct) =>
        {
            var mallar = await db.DocumentTemplates
                .OrderByDescending(m => m.SkapadVid)
                .ToListAsync(ct);
            return Results.Ok(mallar.Select(m => new
            {
                m.Id, m.Namn, Kategori = m.Kategori.ToString(), m.MergeFields, m.SkapadVid
            }));
        }).WithName("ListDocumentTemplates");

        dokument.MapPost("/mall", async (CreateDocumentTemplateRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<DocumentCategory>(req.Kategori, true, out var kategori))
                return Results.BadRequest(new { error = $"Ogiltig kategori: {req.Kategori}" });

            var mall = DocumentTemplate.Skapa(req.Namn, kategori, req.MallInnehall, req.MergeFields);
            await db.DocumentTemplates.AddAsync(mall, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/dokument/mallar", new
            {
                mall.Id, mall.Namn, Kategori = mall.Kategori.ToString(), mall.MergeFields
            });
        }).WithName("CreateDocumentTemplate");

        dokument.MapPost("/mall/{id:guid}/generera", async (Guid id, GenerateFromTemplateRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var mall = await db.DocumentTemplates.FirstOrDefaultAsync(m => m.Id == id, ct);
            if (mall is null) return Results.NotFound();

            var content = mall.GenerateContent(req.Values);
            return Results.Ok(new { mall.Id, mall.Namn, GeneratedContent = content });
        }).WithName("GenerateFromTemplate");

        // ============================================================
        // E-signaturer
        // ============================================================

        dokument.MapGet("/{id:guid}/signaturer", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var signaturer = await db.DocumentSignatures
                .Where(s => s.DocumentId == id)
                .OrderBy(s => s.Ordning)
                .ToListAsync(ct);
            return Results.Ok(signaturer.Select(s => new
            {
                s.Id, s.DocumentId, s.SignerarId, s.Ordning,
                Status = s.Status.ToString(), s.SigneradVid, s.SkapadVid
            }));
        }).WithName("ListDocumentSignatures");

        dokument.MapPost("/{id:guid}/signatur", async (Guid id, AddSignatureRequest req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var docExists = await db.Documents.AnyAsync(d => d.Id == id, ct);
            if (!docExists) return Results.NotFound();

            var signatur = DocumentSignature.Skapa(id, req.SignerarId, req.Ordning);
            await db.DocumentSignatures.AddAsync(signatur, ct);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/dokument/{id}/signaturer", new
            {
                signatur.Id, signatur.DocumentId, signatur.SignerarId, signatur.Ordning,
                Status = signatur.Status.ToString()
            });
        }).WithName("AddDocumentSignature");

        dokument.MapPost("/signatur/{id:guid}/signera", async (Guid id, SignRequest? req, RegionHRDbContext db, CancellationToken ct) =>
        {
            var signatur = await db.DocumentSignatures.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (signatur is null) return Results.NotFound();

            try
            {
                signatur.Signera(req?.IPAdress);
                await db.SaveChangesAsync(ct);
                return Results.Ok(new
                {
                    signatur.Id, Status = signatur.Status.ToString(), signatur.SigneradVid
                });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("SignDocument");

        // ============================================================
        // Dokumentversioner
        // ============================================================

        dokument.MapGet("/{id:guid}/versioner", async (Guid id, RegionHRDbContext db, CancellationToken ct) =>
        {
            var versioner = await db.DocumentVersions
                .Where(v => v.DocumentId == id)
                .OrderByDescending(v => v.VersionNummer)
                .ToListAsync(ct);
            return Results.Ok(versioner.Select(v => new
            {
                v.Id, v.DocumentId, v.VersionNummer, v.StoragePath,
                v.FileSizeBytes, v.SkapadAv, v.AndringsBeskrivning, v.SkapadVid
            }));
        }).WithName("ListDocumentVersions");

        return app;
    }
}

// Request DTOs
record CreateDocumentRequest(Guid AnstallId, string Kategori, string FileName, string StoragePath, long FileSizeBytes, string ContentType, string UppladdadAv);
record CreateDocumentTemplateRequest(string Namn, string Kategori, string MallInnehall, List<string>? MergeFields = null);
record GenerateFromTemplateRequest(Dictionary<string, string> Values);
record AddSignatureRequest(Guid SignerarId, int Ordning);
record SignRequest(string? IPAdress = null);
