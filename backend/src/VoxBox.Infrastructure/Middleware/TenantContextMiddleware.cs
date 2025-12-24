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
        var subdomain = ExtractSubdomain(context.Request.Host);

        if (!string.IsNullOrEmpty(subdomain))
        {
            _logger.LogDebug("Extracted subdomain: {Subdomain}", subdomain);

            var dbContext = (VoxBoxDbContext)dbContextFactory.CreateDbContext();

            var tenant = await dbContext.Set<Tenant>()
                .IgnoreQueryFilters()
                .Where(t => t.Subdomain == subdomain)
                .Select(t => new { t.Id, t.IsHost, t.Subdomain })
                .FirstOrDefaultAsync();

            if (tenant != null)
            {
                tenantContext.SetTenant(tenant.Id, tenant.IsHost, tenant.Subdomain);
                _logger.LogDebug("Tenant identified: {TenantId}, IsHost: {IsHost}", tenant.Id, tenant.IsHost);
            }
            else
            {
                _logger.LogWarning("No tenant found for subdomain: {Subdomain}", subdomain);
            }
        }
        else
        {
            _logger.LogDebug("No subdomain found in request host: {Host}", context.Request.Host);
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
            // For development, you can use a query parameter or header
            return null;
        }

        // Expected format: subdomain.domain.com
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
