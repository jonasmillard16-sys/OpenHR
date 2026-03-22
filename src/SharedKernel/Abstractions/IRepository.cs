namespace RegionHR.SharedKernel.Abstractions;

public interface IRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : struct
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);
    Task<PaginatedResult<TEntity>> GetPaginatedAsync(int page, int pageSize, string? searchTerm = null, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task UpdateAsync(TEntity entity, CancellationToken ct = default);
    Task DeleteAsync(TId id, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
