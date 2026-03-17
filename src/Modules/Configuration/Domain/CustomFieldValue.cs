namespace RegionHR.Configuration.Domain;

public class CustomFieldValue
{
    public Guid Id { get; private set; }
    public Guid CustomFieldId { get; private set; }
    public string EntityId { get; private set; } = "";
    public string Varde { get; private set; } = "";
    public DateTime UppdateradVid { get; private set; }

    private CustomFieldValue() { }

    public static CustomFieldValue Skapa(Guid fieldId, string entityId, string varde)
    {
        return new CustomFieldValue
        {
            Id = Guid.NewGuid(), CustomFieldId = fieldId, EntityId = entityId,
            Varde = varde, UppdateradVid = DateTime.UtcNow
        };
    }

    public void UppdateraVarde(string nyttVarde) { Varde = nyttVarde; UppdateradVid = DateTime.UtcNow; }
}
