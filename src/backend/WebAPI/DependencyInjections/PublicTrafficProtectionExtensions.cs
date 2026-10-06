using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;

namespace WebAPI.DependencyInjections;

/// <summary>
/// Per-client rate limits and short-lived output caching for anonymous endpoints that are
/// cheap to call but expensive or sensitive to serve (login, all-time statistics).
/// </summary>
public static class PublicTrafficProtectionExtensions
{
    /// <summary>
    /// Policy for login, code verification, token refresh, and invitation verification.
    /// </summary>
    public const string AuthPolicy = "auth";

    /// <summary>
    /// Policy for public all-time statistics endpoints.
    /// </summary>
    public const string PublicStatsPolicy = "public-stats";

    /// <summary>
    /// Output cache policy for public all-time statistics responses.
    /// </summary>
    public const string PublicStatsCachePolicy = "public-stats-cache";

    /// <summary>
    /// Registers forwarded-header handling, the rate limiter policies, and output caching.
    /// </summary>
    public static IServiceCollection AddPublicTrafficProtection(this IServiceCollection services)
    {
        // App Service appends the real client address as the last X-Forwarded-For entry;
        // ForwardLimit = 1 reads only that entry so a client cannot spoof its partition key.
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = 1;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthPolicy, context => FixedWindowByClient(context, permitLimit: 10));
            options.AddPolicy(PublicStatsPolicy, context => FixedWindowByClient(context, permitLimit: 60));
        });

        services.AddOutputCache(options =>
        {
            options.AddPolicy(PublicStatsCachePolicy, policy => policy
                .Expire(TimeSpan.FromSeconds(60))
                .SetVaryByQuery("*"));
        });

        return services;
    }

    private static RateLimitPartition<string> FixedWindowByClient(HttpContext context, int permitLimit)
    {
        IPAddress? address = context.Connection.RemoteIpAddress;
        string partitionKey = address?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true
        });
    }
}
