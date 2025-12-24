using VoxBox.Core.Common;

namespace VoxBox.Core.Entities;

/// <summary>
/// Tenant entity representing a tenant in the multi-tenant system.
/// Host is a special instance of Tenant marked with IsHost = true.
/// </summary>
public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Subdomain { get; set; } = string.Empty;
    public bool IsHost { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
