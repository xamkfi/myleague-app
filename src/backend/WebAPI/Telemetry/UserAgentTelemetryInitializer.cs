using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Net.Http.Headers;

namespace WebAPI.Telemetry;

/// <summary>
/// Stores the caller's User-Agent on request telemetry. Application Insights masks client
/// IPs, so the User-Agent is the only way to tell crawlers apart from real visitors
/// (<c>AppRequests | summarize count() by tostring(Properties.UserAgent)</c>).
/// </summary>
public class UserAgentTelemetryInitializer : ITelemetryInitializer
{
    /// <summary>
    /// The custom property name written to request telemetry.
    /// </summary>
    public const string PropertyName = "UserAgent";

    private const int MaxLength = 256;

    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Creates the initializer.
    /// </summary>
    /// <param name="httpContextAccessor">Accessor for the current HTTP context.</param>
    public UserAgentTelemetryInitializer(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is not RequestTelemetry request)
        {
            return;
        }

        if (request.Properties.ContainsKey(PropertyName))
        {
            return;
        }

        HttpContext? context = _httpContextAccessor.HttpContext;
        string? userAgent = context?.Request.Headers[HeaderNames.UserAgent].ToString();
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return;
        }

        if (userAgent.Length > MaxLength)
        {
            userAgent = userAgent[..MaxLength];
        }

        request.Properties[PropertyName] = userAgent;
    }
}
