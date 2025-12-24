using Microsoft.EntityFrameworkCore;
using VoxBox.Core.Entities;
using VoxBox.Core.Interfaces.Persistence;

namespace VoxBox.Infrastructure.Middleware;

/// <summary>
/// Middleware to extract tenant information from the request subdomain.
/// The subdomain is used to identify which tenant the request belongs to.
/// </summary>
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantContextMiddleware> _logger;

    public TenantContextMiddleware(RequestDelegate next, ILogger<TenantContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, IVoxBoxDbContextFactory dbContextFactory)
    {
        var tenancyName = ExtractSubdomain(context.Request.Host);

        if (!string.IsNullOrEmpty(tenancyName))
        {
            _logger.LogDebug("Extracted tenancy name: {TenancyName}", tenancyName);

            var dbContext = (VoxBoxDbContext)dbContextFactory.CreateDbContext();

            var tenant = await dbContext.Set<Tenant>()
                .IgnoreQueryFilters()
                .Where(t => t.TenancyName == tenancyName)
                .Select(t => new { t.Id, t.TenancyName, t.IsHost })
                .FirstOrDefaultAsync();

            if (tenant != null)
            {
                tenantContext.SetTenant(tenant.Id, tenant.IsHost, tenant.TenancyName);
                _logger.LogDebug("Tenant identified: {TenantId}, IsHost: {IsHost}", tenant.Id, tenant.IsHost);
            }
            else
            {
                _logger.LogWarning("No tenant found for tenancy name: {TenancyName}", tenancyName);
            }
        }
        else
        {
            _logger.LogDebug("No tenancy name found in request host: {Host}", context.Request.Host);
        }

        await _next(context);

        tenantContext.Clear();
    }

    private static string? ExtractSubdomain(HostString host)
    {
        var hostString = host.Value;

        // Handle localhost development scenarios
        if (hostString.Contains("localhost") || hostString.Contains("127.0.0.1"))
        {
            // For development, return "host" as default
            return "host";
        }

        // Expected format: tenancyname.domain.com
        var parts = hostString.Split('.');

        if (parts.Length >= 3)
        {
            // Return everything before the last two parts (domain and TLD)
            return string.Join(".", parts.Take(parts.Length - 2));
        }

        return null;
    }
}

/// <summary>
/// Extension methods to add tenant context middleware
/// </summary>
public static class TenantContextMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TenantContextMiddleware>();
    }
}
