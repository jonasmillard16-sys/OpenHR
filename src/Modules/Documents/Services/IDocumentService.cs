using RegionHR.Documents.Domain;

namespace RegionHR.Documents.Services;

public interface IDocumentService
{
    Task<Document> UploadAsync(
        Guid anstallId,
        DocumentCategory kategori,
        string fileName,
        Stream content,
        string contentType,
        string uppladdadAv,
        CancellationToken ct = default);

    Task<(Stream Content, string ContentType, string FileName)?> DownloadAsync(
        Guid documentId,
        CancellationToken ct = default);

    Task<IReadOnlyList<Document>> GetByEmployeeAsync(
        Guid anstallId,
        CancellationToken ct = default);

    Task DeleteExpiredAsync(CancellationToken ct = default);
}
