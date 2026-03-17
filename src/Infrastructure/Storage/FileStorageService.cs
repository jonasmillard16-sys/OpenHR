using Microsoft.Extensions.Logging;

namespace RegionHR.Infrastructure.Storage;

public class FileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(ILogger<FileStorageService> logger)
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        Directory.CreateDirectory(_basePath);
        _logger = logger;
    }

    public async Task<string> SaveAsync(string fileName, byte[] content, string category = "documents")
    {
        var dir = Path.Combine(_basePath, category);
        Directory.CreateDirectory(dir);
        var safeName = $"{Guid.NewGuid():N}_{Path.GetFileName(fileName)}";
        var path = Path.Combine(dir, safeName);
        await File.WriteAllBytesAsync(path, content);
        _logger.LogInformation("File saved: {Path} ({Size} bytes)", path, content.Length);
        return $"/uploads/{category}/{safeName}";
    }

    public async Task<byte[]?> LoadAsync(string relativePath)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));
        if (!File.Exists(path)) return null;
        return await File.ReadAllBytesAsync(path);
    }

    public bool Delete(string relativePath)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.TrimStart('/'));
        if (!File.Exists(path)) return false;
        File.Delete(path);
        return true;
    }

    public IEnumerable<StoredFile> ListFiles(string category = "documents")
    {
        var dir = Path.Combine(_basePath, category);
        if (!Directory.Exists(dir)) yield break;
        foreach (var file in Directory.GetFiles(dir))
        {
            var info = new FileInfo(file);
            yield return new StoredFile(info.Name, $"/uploads/{category}/{info.Name}", info.Length, info.CreationTime);
        }
    }
}

public record StoredFile(string Name, string Url, long SizeBytes, DateTime Created);
