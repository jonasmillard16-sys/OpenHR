using Microsoft.Extensions.Caching.Memory;
using RegionHR.Payroll.Domain;

namespace RegionHR.Payroll.Services;

/// <summary>
/// Konkret implementation av ITaxTableProvider.
/// Laddar skattetabeller från databas via repository och cachar
/// in-memory per år. Trådsäker via IMemoryCache.
/// </summary>
public sealed class TaxTableProviderImpl : ITaxTableProvider
{
    private readonly ITaxTableRepository _repository;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Cachetid för skattetabeller. Skattetabeller ändras en gång per år
    /// men vi vill kunna ladda om vid behov.
    /// </summary>
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    /// <summary>
    /// Lock-objekt per år för att förhindra thundering herd vid cache-miss.
    /// </summary>
    private static readonly SemaphoreSlim _loadLock = new(1, 1);

    public TaxTableProviderImpl(ITaxTableRepository repository, IMemoryCache cache)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// Hämtar en specifik skattetabell med tabellnummer och kolumn.
    /// Cachar alla tabeller för året vid första åtkomst.
    /// </summary>
    public async Task<TaxTable?> GetTableAsync(int year, int tableNumber, int column, CancellationToken ct = default)
    {
        var tables = await GetOrLoadTablesForYearAsync(year, ct);
        return tables.FirstOrDefault(t => t.Tabellnummer == tableNumber && t.Kolumn == column);
    }

    /// <summary>
    /// Returnerar alla skattetabeller för ett givet år.
    /// </summary>
    public async Task<IReadOnlyList<TaxTable>> GetAllTablesForYearAsync(int year, CancellationToken ct = default)
    {
        return await GetOrLoadTablesForYearAsync(year, ct);
    }

    /// <summary>
    /// Laddar skattetabeller från cache eller databas.
    /// Trådsäkert med SemaphoreSlim för att undvika duplicerade laddningar.
    /// </summary>
    private async Task<IReadOnlyList<TaxTable>> GetOrLoadTablesForYearAsync(int year, CancellationToken ct)
    {
        var cacheKey = BuildCacheKey(year);

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<TaxTable>? cachedTables) && cachedTables is not null)
        {
            return cachedTables;
        }

        await _loadLock.WaitAsync(ct);
        try
        {
            // Dubbelkolla efter att vi fått låset (double-check locking)
            if (_cache.TryGetValue(cacheKey, out cachedTables) && cachedTables is not null)
            {
                return cachedTables;
            }

            var tables = await _repository.GetAllTablesForYearAsync(year, ct);

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheDuration,
                Priority = CacheItemPriority.High
            };

            _cache.Set(cacheKey, tables, cacheOptions);
            return tables;
        }
        finally
        {
            _loadLock.Release();
        }
    }

    private static string BuildCacheKey(int year) => $"TaxTables_{year}";
}

/// <summary>
/// Repository-interface för skattetabeller.
/// Separerar databasåtkomst från cachning.
/// </summary>
public interface ITaxTableRepository
{
    Task<TaxTable?> GetTableAsync(int year, int tableNumber, int column, CancellationToken ct = default);
    Task<IReadOnlyList<TaxTable>> GetAllTablesForYearAsync(int year, CancellationToken ct = default);
}
