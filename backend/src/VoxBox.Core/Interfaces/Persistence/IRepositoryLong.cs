using VoxBox.Core.Common;

namespace VoxBox.Core.Interfaces.Persistence;

/// <summary>
/// Generic repository interface for entities with Guid IDs using UUID v7 following SOLID principles:
/// - Single Responsibility: Only persistence operations
/// - Interface Segregation: Focused methods
/// - Dependency Inversion: Abstractions over implementations
/// Following KISS - simple, minimal CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepositoryLong<T> where T : BaseEntityLong
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // Methods that bypass soft-delete filter
    Task<T?> GetByIdAsync(Guid id, bool includeDeleted, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> GetAllAsync(bool includeDeleted, CancellationToken cancellationToken = default);
}