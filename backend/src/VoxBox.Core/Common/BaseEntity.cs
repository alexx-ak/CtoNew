namespace VoxBox.Core.Common;

/// <summary>
/// Base entity class following KISS principle - simple, minimal base for all entities
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
