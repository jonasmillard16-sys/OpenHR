namespace RegionHR.Configuration.Domain;

public class CustomObjectRecord
{
    public Guid Id { get; private set; }
    public Guid CustomObjectId { get; private set; }
    public string Data { get; private set; } = "{}"; // JSONB — actual field values
    public string SkapadAv { get; private set; } = "";
    public DateTime SkapadVid { get; private set; }
    public DateTime? UppdateradVid { get; private set; }

    private CustomObjectRecord() { }

    public static CustomObjectRecord Skapa(Guid customObjectId, string data, string skapadAv = "System")
    {
        return new CustomObjectRecord
        {
            Id = Guid.NewGuid(),
            CustomObjectId = customObjectId,
            Data = data,
            SkapadAv = skapadAv,
            SkapadVid = DateTime.UtcNow
        };
    }

    public void UppdateraData(string data)
    {
        Data = data;
        UppdateradVid = DateTime.UtcNow;
    }
}
