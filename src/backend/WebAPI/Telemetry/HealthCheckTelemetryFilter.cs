using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace WebAPI.Telemetry;

/// <summary>
/// Drops successful health-probe requests from Application Insights. The App Service
/// health check and the availability test call <c>/health/*</c> thousands of times a day,
/// and every stored request is billed by the Log Analytics workspace. Failed probes are kept.
/// </summary>
public class HealthCheckTelemetryFilter : ITelemetryProcessor
{
    private const string HealthPathPrefix = "/health";

    private readonly ITelemetryProcessor _next;

    /// <summary>
    /// Creates the filter. The SDK supplies the next processor in the chain.
    /// </summary>
    /// <param name="next">The next telemetry processor.</param>
    public HealthCheckTelemetryFilter(ITelemetryProcessor next)
    {
        _next = next;
    }

    /// <inheritdoc />
    public void Process(ITelemetry item)
    {
        if (ShouldDrop(item))
        {
            return;
        }

        _next.Process(item);
    }

    /// <summary>
    /// Returns true for a successful request whose path starts with <c>/health</c>.
    /// </summary>
    /// <param name="item">The telemetry item.</param>
    public static bool ShouldDrop(ITelemetry item)
    {
        if (item is not RequestTelemetry request)
        {
            return false;
        }

        if (request.Success == false)
        {
            return false;
        }

        string? path = request.Url?.AbsolutePath;
        return path is not null
            && path.StartsWith(HealthPathPrefix, StringComparison.OrdinalIgnoreCase);
    }
}
