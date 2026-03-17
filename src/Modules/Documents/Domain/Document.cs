namespace RegionHR.Documents.Domain;

public enum DocumentCategory
{
    Anstallningsavtal,
    Lakarintyg,
    Betyg,
    Legitimation,
    Policy,
    Lonespecifikation,
    Tjanstgoringsbevis,
    Ovrigt
}

public enum DataClassification
{
    Normal,
    Kansllig,
    SarskildKategori
}

public sealed class Document
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public DocumentCategory Kategori { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string StoragePath { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public string ContentType { get; private set; } = string.Empty;
    public string? Beskrivning { get; private set; }
    public DateTime UppladdadVid { get; private set; }
    public string UppladdadAv { get; private set; } = string.Empty;
    public DateTime? RetentionUntil { get; private set; }
    public bool IsArchived { get; private set; }
    public DataClassification Klassificering { get; private set; }

    private Document() { } // EF Core

    public static Document Skapa(
        Guid anstallId,
        DocumentCategory kategori,
        string fileName,
        string storagePath,
        long fileSize,
        string contentType,
        string uppladdadAv,
        DataClassification klassificering = DataClassification.Normal,
        string? beskrivning = null)
    {
        return new Document
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Kategori = kategori,
            FileName = fileName,
            StoragePath = storagePath,
            FileSizeBytes = fileSize,
            ContentType = contentType,
            UppladdadAv = uppladdadAv,
            Klassificering = klassificering,
            Beskrivning = beskrivning,
            UppladdadVid = DateTime.UtcNow,
            IsArchived = false
        };
    }

    public void SetRetention(DateTime until)
    {
        RetentionUntil = until;
    }

    public void Archive()
    {
        IsArchived = true;
    }

    public bool ShouldBeRetained()
    {
        return RetentionUntil == null || RetentionUntil > DateTime.UtcNow;
    }
}
