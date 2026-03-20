using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Domain;

/// <summary>
/// Fältmappning mellan källsystem och OpenHR-domänmodell.
/// </summary>
public sealed class MigrationMapping
{
    public Guid Id { get; private set; }
    public MigrationJobId MigrationJobId { get; private set; }
    public string KallFalt { get; private set; } = string.Empty;
    public string MalFalt { get; private set; } = string.Empty;
    public string? TransformationsRegel { get; private set; }

    private MigrationMapping() { }

    public static MigrationMapping Skapa(
        MigrationJobId migrationJobId,
        string kallFalt,
        string malFalt,
        string? transformationsRegel = null)
    {
        return new MigrationMapping
        {
            Id = Guid.NewGuid(),
            MigrationJobId = migrationJobId,
            KallFalt = kallFalt,
            MalFalt = malFalt,
            TransformationsRegel = transformationsRegel
        };
    }
}
