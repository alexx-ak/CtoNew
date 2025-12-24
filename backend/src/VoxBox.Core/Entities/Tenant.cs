using VoxBox.Core.Common;

namespace VoxBox.Core.Entities;

/// <summary>
/// Tenant entity representing a tenant in the multi-tenant system.
/// Host is a special instance of Tenant marked with IsHost = true.
/// </summary>
public class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string TenancyName { get; set; } = string.Empty;
    public bool IsPrivate { get; set; } = false;
    public int VoteWeightMode { get; set; } = 0;
    public string AdminIdentifiers { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Host tenant is identified by TenancyName "host"
    public bool IsHost => TenancyName.Equals("host", StringComparison.OrdinalIgnoreCase);
}
