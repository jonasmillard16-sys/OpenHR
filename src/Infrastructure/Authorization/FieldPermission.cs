namespace RegionHR.Infrastructure.Authorization;

public enum FieldAccessLevel { Full, Lasa, Maskerad, Dold }

public class FieldPermission
{
    public Guid Id { get; private set; }
    public string Roll { get; private set; } = "";
    public string EntityType { get; private set; } = "";
    public string FieldName { get; private set; } = "";
    public FieldAccessLevel AccessLevel { get; private set; }

    private FieldPermission() { }

    public static FieldPermission Skapa(string roll, string entityType, string fieldName, FieldAccessLevel level)
    {
        return new FieldPermission
        {
            Id = Guid.NewGuid(), Roll = roll, EntityType = entityType,
            FieldName = fieldName, AccessLevel = level
        };
    }
}
