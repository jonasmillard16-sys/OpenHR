namespace RegionHR.Documents.Domain;

public enum SignatureStatus { Vantar, Signerad, Nekad, Utgangen }

public class DocumentSignature
{
    public Guid Id { get; private set; }
    public Guid DocumentId { get; private set; }
    public Guid SignerarId { get; private set; }
    public int Ordning { get; private set; }
    public SignatureStatus Status { get; private set; }
    public DateTime? SigneradVid { get; private set; }
    public string? IPAdress { get; private set; }
    public DateTime SkapadVid { get; private set; }

    private DocumentSignature() { }

    public static DocumentSignature Skapa(Guid documentId, Guid signerarId, int ordning)
    {
        return new DocumentSignature
        {
            Id = Guid.NewGuid(), DocumentId = documentId, SignerarId = signerarId,
            Ordning = ordning, Status = SignatureStatus.Vantar, SkapadVid = DateTime.UtcNow
        };
    }

    public void Signera(string? ipAdress = null)
    {
        if (Status != SignatureStatus.Vantar) throw new InvalidOperationException("Kan bara signera väntande signatur");
        Status = SignatureStatus.Signerad; SigneradVid = DateTime.UtcNow; IPAdress = ipAdress;
    }

    public void Neka() { Status = SignatureStatus.Nekad; }
}
