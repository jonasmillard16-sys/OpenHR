using System.IO.Compression;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Platform.Domain;

namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Hanterar import och export av .openhr tillggspaket.
/// Paketet ar ett ZIP-arkiv med manifest.json och tillhande definitioner.
/// </summary>
public class ExtensionPackageService
{
    private readonly RegionHRDbContext _db;

    public ExtensionPackageService(RegionHRDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Importerar ett .openhr-paket fran en ZIP-strom.
    /// Validerar manifest.json och skapar en Extension-post.
    /// </summary>
    public async Task<Extension> ImportAsync(Stream zipStream, CancellationToken ct = default)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        var manifestEntry = archive.GetEntry("manifest.json")
            ?? throw new InvalidOperationException("Paketet saknar manifest.json");

        using var manifestStream = manifestEntry.Open();
        var manifest = await JsonSerializer.DeserializeAsync<PackageManifest>(manifestStream, ManifestJsonOptions, ct)
            ?? throw new InvalidOperationException("Kunde inte tolka manifest.json");

        ValidateManifest(manifest);

        var typ = ParseExtensionTyp(manifest);

        var extension = Extension.Skapa(
            manifest.Name,
            manifest.Version,
            manifest.Author,
            manifest.Description,
            typ,
            manifest.License,
            manifest.Compatibility,
            JsonSerializer.Serialize(manifest.Contents, ManifestJsonOptions));

        await _db.Extensions.AddAsync(extension, ct);
        await _db.SaveChangesAsync(ct);

        return extension;
    }

    /// <summary>
    /// Exporterar ett tillagg som .openhr ZIP-paket.
    /// </summary>
    public async Task<byte[]> ExportAsync(Guid extensionId, CancellationToken ct = default)
    {
        var extension = await _db.Extensions.FirstOrDefaultAsync(e => e.Id == extensionId, ct)
            ?? throw new InvalidOperationException($"Tillagg med id {extensionId} hittades inte");

        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            var manifest = new PackageManifest
            {
                Name = extension.Namn,
                Version = extension.Version,
                Description = extension.Beskrivning,
                Author = extension.Forfattare,
                License = extension.Licens,
                Compatibility = extension.Kompatibilitet,
                Contents = JsonSerializer.Deserialize<PackageContents>(extension.Innehall, ManifestJsonOptions)
                    ?? new PackageContents()
            };

            var manifestEntry = archive.CreateEntry("manifest.json");
            using var writer = new StreamWriter(manifestEntry.Open());
            await writer.WriteAsync(JsonSerializer.Serialize(manifest, ManifestJsonOptions));
        }

        return ms.ToArray();
    }

    private static void ValidateManifest(PackageManifest manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.Name))
            throw new InvalidOperationException("manifest.json: 'name' saknas");
        if (string.IsNullOrWhiteSpace(manifest.Version))
            throw new InvalidOperationException("manifest.json: 'version' saknas");
        if (string.IsNullOrWhiteSpace(manifest.Author))
            throw new InvalidOperationException("manifest.json: 'author' saknas");
    }

    private static ExtensionTyp ParseExtensionTyp(PackageManifest manifest)
    {
        if (manifest.Contents.CustomObjects.Count > 0)
            return ExtensionTyp.CustomObject;
        if (manifest.Contents.Workflows.Count > 0)
            return ExtensionTyp.Workflow;
        if (manifest.Contents.Reports.Count > 0)
            return ExtensionTyp.Report;
        return ExtensionTyp.Integration;
    }

    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}

/// <summary>
/// Representerar manifest.json i ett .openhr-paket.
/// </summary>
public class PackageManifest
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string License { get; set; } = string.Empty;
    public string Compatibility { get; set; } = string.Empty;
    public PackageContents Contents { get; set; } = new();
}

public class PackageContents
{
    public List<string> CustomObjects { get; set; } = [];
    public List<string> Workflows { get; set; } = [];
    public List<string> Reports { get; set; } = [];
}
