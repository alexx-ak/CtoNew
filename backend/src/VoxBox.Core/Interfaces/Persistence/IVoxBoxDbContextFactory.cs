namespace VoxBox.Core.Interfaces.Persistence;

/// <summary>
/// Marker interface for DbContext factory.
/// The actual implementation is in Infrastructure layer.
/// </summary>
public interface IVoxBoxDbContextFactory
{
    /// <summary>
    /// Creates a new DbContext instance
    /// </summary>
    object CreateDbContext();
}
