using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebAPI.Controllers.Health
{
    /// <summary>
    /// Builds the JSON body for the detailed health endpoints (/health and /api/health).
    /// </summary>
    /// <remarks>
    /// The endpoints stay anonymous because CI, the Docker healthcheck, and deploy smoke tests call them.
    /// Check descriptions and data can contain row counts, memory figures, and exception messages,
    /// so only site admins see them. Everyone else gets the overall status and each check's name and status.
    /// </remarks>
    public static class HealthReportResponse
    {
        /// <summary>
        /// Creates the response body for <paramref name="report"/>.
        /// </summary>
        /// <param name="report">The health report to describe.</param>
        /// <param name="includeDetails">True for site admins: include each check's description, duration, data, and tags.</param>
        /// <param name="tag">The tag the report was filtered by, if any. It is echoed back in the body.</param>
        public static object Create(HealthReport report, bool includeDetails, string? tag = null)
        {
            List<object> checks = report.Entries
                .Select(entry => includeDetails
                    ? (object)new
                    {
                        Name = entry.Key,
                        Status = entry.Value.Status.ToString(),
                        entry.Value.Description,
                        Duration = entry.Value.Duration.TotalMilliseconds,
                        entry.Value.Data,
                        entry.Value.Tags
                    }
                    : new
                    {
                        Name = entry.Key,
                        Status = entry.Value.Status.ToString()
                    })
                .ToList();

            if (tag is null)
            {
                return new
                {
                    Status = report.Status.ToString(),
                    Duration = report.TotalDuration.TotalMilliseconds,
                    CheckedAt = DateTime.UtcNow,
                    Checks = checks
                };
            }

            return new
            {
                Tag = tag,
                Status = report.Status.ToString(),
                Duration = report.TotalDuration.TotalMilliseconds,
                CheckedAt = DateTime.UtcNow,
                Checks = checks
            };
        }
    }
}
