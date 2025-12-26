using Microsoft.EntityFrameworkCore;
using VoxBox.Core.Common;
using VoxBox.Core.Interfaces.Persistence;

namespace VoxBox.Infrastructure.Persistence;

/// <summary>
/// Generic EF Repository implementation for entities with Guid IDs using UUID v7 following SOLID:
/// - Single Responsibility: Only data access
/// - Open/Closed: Extensible via base class
/// KISS: Simple CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public class EfRepositoryLong<T>(DbSet<T> dbSet, ITenantContext tenantContext) : IRepositoryLong<T> where T : BaseEntityLong
{
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await GetQuery(includeDeleted: false)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<T?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken = default)
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
        entity.TenantId = tenantContext.TenantId;

        await dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        entity.ModifiedBy = null; // Set to null to avoid foreign key constraints

        dbSet.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, includeDeleted: false, cancellationToken);
        if (entity != null)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.DeletedBy = null; // Set to null to avoid foreign key constraints

            dbSet.Update(entity);
        }
    }

    private IQueryable<T> GetQuery(bool includeDeleted)
    {
        var query = dbSet.AsQueryable();

        // Apply global query filters are handled by EF Core automatically
        // When includeDeleted is true, bypass the soft-delete filter
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return query;
    }
}