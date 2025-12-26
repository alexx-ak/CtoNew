using VoxBox.Core.Common;

namespace VoxBox.Core.Interfaces.Persistence;

/// <summary>
/// Generic repository interface following SOLID principles:
/// - Single Responsibility: Only persistence operations
/// - Interface Segregation: Focused methods
/// - Dependency Inversion: Abstractions over implementations
/// Following KISS - simple, minimal CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Methods that bypass soft-delete filter
    Task<T?> GetByIdAsync(int id, bool includeDeleted, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(bool includeDeleted, CancellationToken cancellationToken = default);
}
