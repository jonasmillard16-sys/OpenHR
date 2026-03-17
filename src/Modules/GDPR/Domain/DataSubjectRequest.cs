namespace RegionHR.GDPR.Domain;

public enum RequestType
{
    Registerutdrag,
    Radering,
    Dataportabilitet,
    Rattelse
}

public enum RequestStatus
{
    Mottagen,
    UnderBehandling,
    Klar,
    Avslagen
}

public class DataSubjectRequest
{
    public Guid Id { get; private set; }
    public Guid AnstallId { get; private set; }
    public RequestType Typ { get; private set; }
    public RequestStatus Status { get; private set; }
    public DateTime Mottagen { get; private set; }
    public DateTime Deadline { get; private set; }
    public DateTime? SlutfordVid { get; private set; }
    public string? HandlaggarId { get; private set; }
    public string? Kommentar { get; private set; }
    public string? ResultatFilSokvag { get; private set; }

    private DataSubjectRequest() { }

    public static DataSubjectRequest Skapa(Guid anstallId, RequestType typ)
    {
        var mottagen = DateTime.UtcNow;
        return new DataSubjectRequest
        {
            Id = Guid.NewGuid(),
            AnstallId = anstallId,
            Typ = typ,
            Status = RequestStatus.Mottagen,
            Mottagen = mottagen,
            Deadline = mottagen.AddDays(30)
        };
    }

    public void Tilldela(string handlaggarId)
    {
        HandlaggarId = handlaggarId;
        Status = RequestStatus.UnderBehandling;
    }

    public void Slutfor(string? filSokvag)
    {
        if (Status == RequestStatus.Klar)
            throw new InvalidOperationException("Begäran är redan slutförd.");

        Status = RequestStatus.Klar;
        SlutfordVid = DateTime.UtcNow;
        ResultatFilSokvag = filSokvag;
    }

    public bool ArForsenad => Status != RequestStatus.Klar && DateTime.UtcNow > Deadline;
}
