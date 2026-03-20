namespace RegionHR.Migration.Domain;

/// <summary>
/// Sparad migreringsmall med förkonfigurerade fältmappningar per källsystem.
/// </summary>
public sealed class MigrationTemplate
{
    public Guid Id { get; private set; }
    public string Namn { get; private set; } = string.Empty;
    public SourceSystem KallSystem { get; private set; }
    public string Mappningar { get; private set; } = "{}"; // JSON

    private MigrationTemplate() { }

    public static MigrationTemplate Skapa(string namn, SourceSystem kallSystem, string mappningarJson)
    {
        return new MigrationTemplate
        {
            Id = Guid.NewGuid(),
            Namn = namn,
            KallSystem = kallSystem,
            Mappningar = mappningarJson
        };
    }
}
