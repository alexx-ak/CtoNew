using VoxBox.Core.Common;

namespace VoxBox.Core.Entities;

/// <summary>
/// Tenant entity representing a tenant in the multi-tenant system.
/// Host is a special instance of Tenant marked with IsHost = true.
/// </summary>
public class Tenant : BaseEntity
{
    // Properties will be defined based on the SQL CREATE TABLE statement
    // TenantId and IsHost are inherited from BaseEntity and configuration
}
