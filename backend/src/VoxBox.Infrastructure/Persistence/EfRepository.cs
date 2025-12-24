using Microsoft.EntityFrameworkCore;
using VoxBox.Core.Common;
using VoxBox.Core.Interfaces.Persistence;

namespace VoxBox.Infrastructure.Persistence;

/// <summary>
/// Generic EF Repository implementation following SOLID:
/// - Single Responsibility: Only data access
/// - Open/Closed: Extensible via base class
/// KISS: Simple CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class EfRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly DbSet<T> _dbSet;
    private readonly ITenantContext _tenantContext;

    public EfRepository(DbSet<T> dbSet, ITenantContext tenantContext)
    {
        _dbSet = dbSet;
        _tenantContext = tenantContext;
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await GetQuery(includeDeleted: false)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<T?> GetByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken = default)
    {
        return await GetQuery(includeDeleted)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetQuery(includeDeleted: false).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<T>> GetAllAsync(bool includeDeleted, CancellationToken cancellationToken = default)
    {
        return await GetQuery(includeDeleted).ToListAsync(cancellationToken);
    }

    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.TenantId = _tenantContext.TenantId;

        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        entity.ModifiedBy = "System"; // Will be replaced with actual user when auth is implemented

        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, includeDeleted: false, cancellationToken);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = "System"; // Will be replaced with actual user when auth is implemented

            _dbSet.Update(entity);
        }
    }

    private IQueryable<T> GetQuery(bool includeDeleted)
    {
        var query = _dbSet.AsQueryable();

        // Apply global query filters are handled by EF Core automatically
        // When includeDeleted is true, bypass the soft-delete filter
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return query;
    }
}
