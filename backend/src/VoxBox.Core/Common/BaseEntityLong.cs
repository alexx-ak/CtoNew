namespace VoxBox.Core.Common;

/// <summary>
/// Base entity class with Guid ID using UUID v7 for time-sortable primary keys
/// Includes multitenancy, soft delete, and auditing support
/// </summary>
public abstract class BaseEntityLong
{
    public Guid Id { get; set; } = GuidGenerator.GenerateNewGuid();

    // Multitenancy support
    public Guid? TenantId { get; set; }

    // Audit columns
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
    public Guid? ModifiedBy { get; set; }

    // Soft delete support
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
}