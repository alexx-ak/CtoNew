namespace VoxBox.Core.Common;

/// <summary>
/// Base entity class with long ID for entities that need bigint primary keys
/// Includes multitenancy, soft delete, and auditing support
/// </summary>
public abstract class BaseEntityLong
{
    public long Id { get; set; }

    // Multitenancy support
    public int? TenantId { get; set; }

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