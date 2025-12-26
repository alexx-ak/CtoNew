namespace VoxBox.Core.Interfaces.Persistence;

/// <summary>
/// Interface for accessing the current tenant context.
/// Used by middleware to store tenant information per request.
/// </summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
    bool IsHost { get; }
    string? Subdomain { get; }
    void SetTenant(Guid? tenantId, bool isHost, string? subdomain);
    void Clear();
}
