using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Domain;

/// <summary>
/// Loggpost per importerad (eller misslyckad) rad i en migrering.
/// </summary>
public sealed class MigrationLog
{
    public Guid Id { get; private set; }
    public MigrationJobId MigrationJobId { get; private set; }
    public string EntityTyp { get; private set; } = string.Empty;
    public Guid? ImporteradPostId { get; private set; }
    public MigrationLogStatus Status { get; private set; }
    public string? FelMeddelande { get; private set; }

    private MigrationLog() { }

    public static MigrationLog Skapa(
        MigrationJobId migrationJobId,
        string entityTyp,
        MigrationLogStatus status,
        Guid? importeradPostId = null,
        string? felMeddelande = null)
    {
        return new MigrationLog
        {
            Id = Guid.NewGuid(),
            MigrationJobId = migrationJobId,
            EntityTyp = entityTyp,
            Status = status,
            ImporteradPostId = importeradPostId,
            FelMeddelande = felMeddelande
        };
    }
}
