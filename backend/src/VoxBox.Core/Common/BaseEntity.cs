namespace VoxBox.Core.Common;

/// <summary>
/// Base entity class following KISS principle - simple, minimal base for all entities
/// Includes multitenancy, soft delete, and auditing support
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }

    // Multitenancy support
    public Guid? TenantId { get; set; }

    // Audit columns
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public long? ModifiedBy { get; set; }

    // Soft delete support
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public long? DeletedBy { get; set; }
}
