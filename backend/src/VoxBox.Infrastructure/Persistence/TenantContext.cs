using VoxBox.Core.Interfaces.Persistence;

namespace VoxBox.Infrastructure.Persistence;

/// <summary>
/// Scoped service to hold tenant context for the current request.
/// </summary>
public class TenantContext : ITenantContext
{
    private int? _tenantId;
    private bool _isHost;
    private string? _subdomain;

    public int? TenantId => _tenantId;
    public bool IsHost => _isHost;
    public string? Subdomain => _subdomain;

    public void SetTenant(int? tenantId, bool isHost, string? subdomain)
    {
        _tenantId = tenantId;
        _isHost = isHost;
        _subdomain = subdomain;
    }

    public void Clear()
    {
        _tenantId = null;
        _isHost = false;
        _subdomain = null;
    }
}
