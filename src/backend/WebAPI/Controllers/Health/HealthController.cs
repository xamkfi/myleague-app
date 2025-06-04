using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;

namespace WebAPI.Controllers.Health
{
    /// <summary>
    /// Controller for health check endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;

        /// <summary>
        /// Initializes a new instance of the HealthController class
        /// </summary>
        /// <param name="healthCheckService">The health check service</param>
        /// <param name="logger">The logger instance</param>
        public HealthController(
            HealthCheckService healthCheckService,
            ILogger<HealthController> logger)
        {
            _healthCheckService = healthCheckService ?? throw new ArgumentNullException(nameof(healthCheckService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets the overall health status of the application
        /// </summary>
        /// <returns>The health status</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> GetHealthAsync()
        {
            try
            {
                HealthReport healthReport = await _healthCheckService.CheckHealthAsync();
                
                var response = new
                {
                    Status = healthReport.Status.ToString(),
                    Duration = healthReport.TotalDuration.TotalMilliseconds,
                    CheckedAt = DateTime.UtcNow,
                    Checks = healthReport.Entries.Select(entry => new
                    {
                        Name = entry.Key,
                        Status = entry.Value.Status.ToString(),
                        entry.Value.Description,
                        Duration = entry.Value.Duration.TotalMilliseconds,
                        entry.Value.Data,
                        entry.Value.Tags
                    })
                };

                return healthReport.Status == HealthStatus.Healthy 
                    ? Ok(response) 
                    : StatusCode((int)HttpStatusCode.ServiceUnavailable, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed with exception");
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Status = "Unhealthy",
                    Error = "Health check failed",
                    CheckedAt = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Gets the health status for a specific tag group
        /// </summary>
        /// <param name="tag">The tag to filter health checks by</param>
        /// <returns>The health status for the specified tag</returns>
        [HttpGet("tag/{tag}")]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(object), (int)HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> GetHealthByTagAsync(string tag)
        {
            try
            {
                HealthReport healthReport = await _healthCheckService.CheckHealthAsync(
                    check => check.Tags.Contains(tag));
                
                var response = new
                {
                    Tag = tag,
                    Status = healthReport.Status.ToString(),
                    Duration = healthReport.TotalDuration.TotalMilliseconds,
                    CheckedAt = DateTime.UtcNow,
                    Checks = healthReport.Entries.Select(entry => new
                    {
                        Name = entry.Key,
                        Status = entry.Value.Status.ToString(),
                        entry.Value.Description,
                        Duration = entry.Value.Duration.TotalMilliseconds,
                        entry.Value.Data,
                        entry.Value.Tags
                    })
                };

                return healthReport.Status == HealthStatus.Healthy 
                    ? Ok(response) 
                    : StatusCode((int)HttpStatusCode.ServiceUnavailable, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check for tag {Tag} failed with exception", tag);
                return StatusCode((int)HttpStatusCode.InternalServerError, new
                {
                    Tag = tag,
                    Status = "Unhealthy",
                    Error = "Health check failed",
                    CheckedAt = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Gets a simple health status for load balancers and monitoring tools
        /// </summary>
        /// <returns>Simple health status</returns>
        [HttpGet("ready")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.ServiceUnavailable)]
        public async Task<IActionResult> GetReadinessAsync()
        {
            try
            {
                HealthReport healthReport = await _healthCheckService.CheckHealthAsync();
                
                return healthReport.Status == HealthStatus.Healthy 
                    ? Ok("Healthy") 
                    : StatusCode((int)HttpStatusCode.ServiceUnavailable, "Unhealthy");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed with exception");
                return StatusCode((int)HttpStatusCode.ServiceUnavailable, "Unhealthy");
            }
        }

        /// <summary>
        /// Gets a simple liveness check
        /// </summary>
        /// <returns>Liveness status</returns>
        [HttpGet("live")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public IActionResult GetLivenessAsync()
        {
            return Ok("Alive");
        }
    }
} 
