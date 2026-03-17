namespace RegionHR.Documents.Domain;

public class DocumentVersion
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public int VersionNummer { get; private set; }
    public string StoragePath { get; private set; } = "";
    public long FileSizeBytes { get; private set; }
    public string SkapadAv { get; private set; } = "";
    public string? AndringsBeskrivning { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private DocumentVersion() { }

    public static DocumentVersion Skapa(Guid documentId, int versionNummer, string storagePath, long fileSize, string skapadAv, string? beskrivning = null)
    {
        return new DocumentVersion
        {
            Id = Guid.NewGuid(), DocumentId = documentId, VersionNummer = versionNummer,
            StoragePath = storagePath, FileSizeBytes = fileSize, SkapadAv = skapadAv,
            AndringsBeskrivning = beskrivning, SkapadVid = DateTime.UtcNow
        };
    }
}
