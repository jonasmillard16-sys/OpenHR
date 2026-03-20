using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Domain;

/// <summary>
/// Valideringsfel som hittats under parsning/validering av migreringsdata.
/// </summary>
public sealed class MigrationValidationError
{
    public Guid Id { get; private set; }
    public MigrationJobId MigrationJobId { get; private set; }
    public int RadNummer { get; private set; }
    public string Falt { get; private set; } = string.Empty;
    public string FelTyp { get; private set; } = string.Empty;
    public string? OriginalVarde { get; private set; }
    public string? ForeslagnKorrektion { get; private set; }

    private MigrationValidationError() { }

    public static MigrationValidationError Skapa(
        MigrationJobId migrationJobId,
        int radNummer,
        string falt,
        string felTyp,
        string? originalVarde = null,
        string? foreslagnKorrektion = null)
    {
        return new MigrationValidationError
        {
            Id = Guid.NewGuid(),
            MigrationJobId = migrationJobId,
            RadNummer = radNummer,
            Falt = falt,
            FelTyp = felTyp,
            OriginalVarde = originalVarde,
            ForeslagnKorrektion = foreslagnKorrektion
        };
    }
}
