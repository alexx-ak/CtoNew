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
    
    public EfRepository(DbSet<T> dbSet)
    {
        _dbSet = dbSet;
    }
    
    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }
    
    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }
    
    public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }
    
    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }
    
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }
}
