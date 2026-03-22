namespace RegionHR.Infrastructure.Storage;

public interface IFileStorageService
{
    Task<string> UploadAsync(string category, string fileName, Stream content, CancellationToken ct = default);
    Task<Stream?> DownloadAsync(string storagePath, CancellationToken ct = default);
    Task DeleteAsync(string storagePath, CancellationToken ct = default);
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(string basePath = "uploads")
    {
        _basePath = Path.GetFullPath(basePath);
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(string category, string fileName, Stream content, CancellationToken ct = default)
    {
        var dir = Path.Combine(_basePath, category, DateTime.UtcNow.ToString("yyyy-MM"));
        Directory.CreateDirectory(dir);
        var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(dir, safeName);
        using var fs = File.Create(fullPath);
        await content.CopyToAsync(fs, ct);
        return Path.GetRelativePath(_basePath, fullPath);
    }

    /// <summary>
    /// Returns an open FileStream. CALLER OWNS THE STREAM and must dispose it (use 'using' or 'await using').
    /// Returns null if file does not exist.
    /// </summary>
    public Task<Stream?> DownloadAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        if (!File.Exists(fullPath)) return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(File.OpenRead(fullPath));
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(_basePath, storagePath);
        if (File.Exists(fullPath)) File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
