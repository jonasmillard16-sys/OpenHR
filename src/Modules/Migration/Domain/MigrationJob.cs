using RegionHR.SharedKernel.Abstractions;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Migration.Domain;

/// <summary>
/// Aggregatrot för ett migreringsjobb. Hanterar hela livscykeln
/// från skapande, validering, dry-run, import till slutförande.
/// </summary>
public sealed class MigrationJob : AggregateRoot<MigrationJobId>
{
    public SourceSystem Kalla { get; private set; }
    public MigrationJobStatus Status { get; private set; }
    public string FilNamn { get; private set; } = string.Empty;
    public int TotaltAntalRader { get; private set; }
    public int ImporteradeRader { get; private set; }
    public int FelRader { get; private set; }
    public string SkapadAv { get; private set; } = string.Empty;
    public string? FelMeddelande { get; private set; }

    private readonly List<MigrationValidationError> _valideringsfel = [];
    public IReadOnlyList<MigrationValidationError> ValideringsFel => _valideringsfel.AsReadOnly();

    private readonly List<MigrationLog> _logg = [];
    public IReadOnlyList<MigrationLog> Logg => _logg.AsReadOnly();

    private readonly List<MigrationMapping> _mappningar = [];
    public IReadOnlyList<MigrationMapping> Mappningar => _mappningar.AsReadOnly();

    private MigrationJob() { }

    public static MigrationJob Skapa(SourceSystem kalla, string filNamn, string skapadAv)
    {
        if (string.IsNullOrWhiteSpace(filNamn))
            throw new ArgumentException("Filnamn krävs", nameof(filNamn));
        if (string.IsNullOrWhiteSpace(skapadAv))
            throw new ArgumentException("Skapad av krävs", nameof(skapadAv));

        return new MigrationJob
        {
            Id = MigrationJobId.New(),
            Kalla = kalla,
            Status = MigrationJobStatus.Created,
            FilNamn = filNamn,
            SkapadAv = skapadAv
        };
    }

    public void StartaValidering()
    {
        if (Status != MigrationJobStatus.Created)
            throw new InvalidOperationException($"Kan inte starta validering från status {Status}");
        Status = MigrationJobStatus.Validating;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartaDryRun()
    {
        if (Status != MigrationJobStatus.Validating)
            throw new InvalidOperationException($"Kan inte starta dry-run från status {Status}");
        Status = MigrationJobStatus.DryRun;
        UpdatedAt = DateTime.UtcNow;
    }

    public void StartaImport()
    {
        if (Status != MigrationJobStatus.DryRun)
            throw new InvalidOperationException($"Kan inte starta import från status {Status}");
        Status = MigrationJobStatus.Importing;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Slutfor(int totaltAntalRader, int importeradeRader, int felRader)
    {
        if (Status != MigrationJobStatus.Importing)
            throw new InvalidOperationException($"Kan inte slutföra från status {Status}");
        Status = MigrationJobStatus.Complete;
        TotaltAntalRader = totaltAntalRader;
        ImporteradeRader = importeradeRader;
        FelRader = felRader;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkeraMisslyckad(string felmeddelande)
    {
        Status = MigrationJobStatus.Failed;
        FelMeddelande = felmeddelande;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SattTotaltAntalRader(int antal)
    {
        TotaltAntalRader = antal;
    }

    public void LaggTillValideringsFel(MigrationValidationError fel)
    {
        _valideringsfel.Add(fel);
        FelRader = _valideringsfel.Count;
    }

    public void LaggTillLogg(MigrationLog logg)
    {
        _logg.Add(logg);
        if (logg.Status == MigrationLogStatus.Success)
            ImporteradeRader++;
        else
            FelRader++;
    }

    public void LaggTillMapping(MigrationMapping mapping)
    {
        _mappningar.Add(mapping);
    }
}
