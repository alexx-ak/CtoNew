using VoxBox.Core.Common;

namespace VoxBox.Core.Interfaces.Persistence;

/// <summary>
/// Unit of Work pattern following SOLID - ensures data consistency
/// Single Responsibility: Manages transaction scope
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<T> GetRepository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
