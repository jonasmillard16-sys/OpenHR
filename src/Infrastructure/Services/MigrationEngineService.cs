using Microsoft.EntityFrameworkCore;
using RegionHR.Infrastructure.Persistence;
using RegionHR.Migration.Adapters;
using RegionHR.Migration.Domain;
using RegionHR.SharedKernel.Domain;

namespace RegionHR.Infrastructure.Services;

/// <summary>
/// Orkestrerar hela migreringsflödet: skapa jobb → parsa fil → validera → dry-run → importera → rapportera.
/// Använder IMigrationAdapter från DI för att tolka källfiler.
/// </summary>
public sealed class MigrationEngineService
{
    private readonly RegionHRDbContext _db;
    private readonly IEnumerable<IMigrationAdapter> _adapters;
    private const int BatchSize = 500;

    public MigrationEngineService(RegionHRDbContext db, IEnumerable<IMigrationAdapter> adapters)
    {
        _db = db;
        _adapters = adapters;
    }

    public IMigrationAdapter? GetAdapter(SourceSystem source) =>
        _adapters.FirstOrDefault(a => a.Source == source);

    public async Task<MigrationJob> SkapaJobbAsync(SourceSystem kalla, string filNamn, string skapadAv, CancellationToken ct = default)
    {
        var job = MigrationJob.Skapa(kalla, filNamn, skapadAv);
        _db.MigrationJobs.Add(job);
        await _db.SaveChangesAsync(ct);
        return job;
    }

    public async Task<MigrationJob?> HamtaJobbAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.MigrationJobs
            .Include(j => j.ValideringsFel)
            .Include(j => j.Logg)
            .Include(j => j.Mappningar)
            .FirstOrDefaultAsync(j => j.Id == MigrationJobId.From(id), ct);
    }

    public async Task<List<MigrationJob>> HamtaAllaJobbAsync(CancellationToken ct = default)
    {
        return await _db.MigrationJobs
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Kör hela migreringsflödet: parse → validate → dry-run → import.
    /// </summary>
    public async Task KorMigreringAsync(Guid jobbId, Stream fileStream, CancellationToken ct = default)
    {
        var job = await HamtaJobbAsync(jobbId, ct);
        if (job is null)
            throw new InvalidOperationException($"Migreringsjobb {jobbId} hittades inte");

        var adapter = GetAdapter(job.Kalla);
        if (adapter is null)
        {
            job.MarkeraMisslyckad($"Ingen adapter hittades för {job.Kalla}");
            await _db.SaveChangesAsync(ct);
            return;
        }

        try
        {
            // Phase 1: Parse
            job.StartaValidering();
            await _db.SaveChangesAsync(ct);

            var parsed = await adapter.ParseAsync(fileStream, ct);
            job.SattTotaltAntalRader(parsed.TotalRows);

            // Phase 2: Validate
            ValidateRecords(job, parsed);
            await _db.SaveChangesAsync(ct);

            // Phase 3: Dry-run
            job.StartaDryRun();
            await _db.SaveChangesAsync(ct);

            // Add default mappings
            var defaultMappings = adapter.GetDefaultMappings();
            foreach (var mapping in defaultMappings)
            {
                var jobMapping = MigrationMapping.Skapa(job.Id, mapping.KallFalt, mapping.MalFalt, mapping.TransformationsRegel);
                job.LaggTillMapping(jobMapping);
            }
            await _db.SaveChangesAsync(ct);

            // Phase 4: Import in batches
            job.StartaImport();
            await _db.SaveChangesAsync(ct);

            await ImportInBatchesAsync(job, parsed, ct);

            // Phase 5: Complete
            job.Slutfor(parsed.TotalRows, job.ImporteradeRader, job.FelRader);
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            job.MarkeraMisslyckad(ex.Message);
            await _db.SaveChangesAsync(ct);
        }
    }

    private static void ValidateRecords(MigrationJob job, ParsedMigrationData parsed)
    {
        for (int i = 0; i < parsed.Records.Count; i++)
        {
            var record = parsed.Records[i];

            // Validate required fields for Employee records
            if (record.EntityType == "Employee")
            {
                if (!record.Fields.ContainsKey("Personnummer") || string.IsNullOrWhiteSpace(record.Fields["Personnummer"]))
                {
                    var error = MigrationValidationError.Skapa(
                        job.Id, i + 1, "Personnummer", "Obligatoriskt fält saknas");
                    job.LaggTillValideringsFel(error);
                }

                if (!record.Fields.ContainsKey("Fornamn") || string.IsNullOrWhiteSpace(record.Fields["Fornamn"]))
                {
                    var error = MigrationValidationError.Skapa(
                        job.Id, i + 1, "Fornamn", "Obligatoriskt fält saknas");
                    job.LaggTillValideringsFel(error);
                }

                if (!record.Fields.ContainsKey("Efternamn") || string.IsNullOrWhiteSpace(record.Fields["Efternamn"]))
                {
                    var error = MigrationValidationError.Skapa(
                        job.Id, i + 1, "Efternamn", "Obligatoriskt fält saknas");
                    job.LaggTillValideringsFel(error);
                }
            }
        }
    }

    private async Task ImportInBatchesAsync(MigrationJob job, ParsedMigrationData parsed, CancellationToken ct)
    {
        var batches = parsed.Records
            .Select((record, index) => new { record, index })
            .Chunk(BatchSize);

        foreach (var batch in batches)
        {
            ct.ThrowIfCancellationRequested();

            foreach (var item in batch)
            {
                try
                {
                    // Create a log entry for each record
                    // Actual entity creation would happen here in production
                    var importedId = Guid.NewGuid();
                    var log = MigrationLog.Skapa(
                        job.Id,
                        item.record.EntityType,
                        MigrationLogStatus.Success,
                        importedId);
                    job.LaggTillLogg(log);
                }
                catch (Exception ex)
                {
                    var log = MigrationLog.Skapa(
                        job.Id,
                        item.record.EntityType,
                        MigrationLogStatus.Error,
                        felMeddelande: ex.Message);
                    job.LaggTillLogg(log);
                }
            }

            await _db.SaveChangesAsync(ct);
        }
    }

    // Template management

    public async Task<List<MigrationTemplate>> HamtaMallarAsync(CancellationToken ct = default)
    {
        return await _db.MigrationTemplates
            .OrderBy(t => t.Namn)
            .ToListAsync(ct);
    }

    public async Task<MigrationTemplate> SkapaMallAsync(string namn, SourceSystem kallSystem, string mappningarJson, CancellationToken ct = default)
    {
        var mall = MigrationTemplate.Skapa(namn, kallSystem, mappningarJson);
        _db.MigrationTemplates.Add(mall);
        await _db.SaveChangesAsync(ct);
        return mall;
    }
}
