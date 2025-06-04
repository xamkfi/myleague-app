using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using MyLeague.Infrastructure.Persistence.UnitOfWork;

namespace MyLeague.Infrastructure.HealthChecks
{
    /// <summary>
    /// Health check for verifying critical application services are properly registered and functional
    /// </summary>
    public class ApplicationServicesHealthCheck : IHealthCheck
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ApplicationServicesHealthCheck> _logger;

        /// <summary>
        /// Initializes a new instance of the ApplicationServicesHealthCheck class
        /// </summary>
        /// <param name="serviceProvider">The service provider to check service registrations</param>
        /// <param name="logger">The logger instance</param>
        public ApplicationServicesHealthCheck(
            IServiceProvider serviceProvider,
            ILogger<ApplicationServicesHealthCheck> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs the health check by verifying critical services are registered and accessible
        /// </summary>
        /// <param name="context">The health check context</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the health check result</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Starting application services health check");

                var serviceChecks = new Dictionary<string, bool>();
                var errors = new List<string>();

                // Check critical repository services
                await CheckServiceAsync<IClubRepository>("ClubRepository", serviceChecks, errors);
                await CheckServiceAsync<IPersonRepository>("PersonRepository", serviceChecks, errors);
                await CheckServiceAsync<IFloorballPlayerRepository>("FloorballPlayerRepository", serviceChecks, errors);
                await CheckServiceAsync<IFloorballTeamRepository>("FloorballTeamRepository", serviceChecks, errors);
                await CheckServiceAsync<IFloorballMatchRepository>("FloorballMatchRepository", serviceChecks, errors);
                await CheckServiceAsync<IFloorballSeasonRepository>("FloorballSeasonRepository", serviceChecks, errors);

                // Check Unit of Work
                await CheckServiceAsync<IUnitOfWork>("UnitOfWork", serviceChecks, errors);

                var data = new Dictionary<string, object>
                {
                    { "ServicesChecked", serviceChecks.Count },
                    { "ServicesHealthy", serviceChecks.Count(s => s.Value) },
                    { "CheckedAt", DateTime.UtcNow }
                };

                // Add individual service status to data
                foreach (KeyValuePair<string, bool> serviceCheck in serviceChecks)
                {
                    data.Add(serviceCheck.Key, serviceCheck.Value ? "Healthy" : "Unhealthy");
                }

                if (errors.Any())
                {
                    _logger.LogWarning("Application services health check found issues: {Errors}", string.Join(", ", errors));
                    data.Add("Errors", errors);
                    return HealthCheckResult.Degraded("Some application services have issues", null, data);
                }

                _logger.LogDebug("Application services health check completed successfully");
                return HealthCheckResult.Healthy("All critical application services are healthy", data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Application services health check failed with exception");
                return HealthCheckResult.Unhealthy("Application services health check failed", ex);
            }
        }

        /// <summary>
        /// Checks if a specific service is registered and can be resolved
        /// </summary>
        /// <typeparam name="T">The service type to check</typeparam>
        /// <param name="serviceName">The name of the service for logging</param>
        /// <param name="serviceChecks">Dictionary to store check results</param>
        /// <param name="errors">List to store any errors</param>
        private async Task CheckServiceAsync<T>(
            string serviceName,
            Dictionary<string, bool> serviceChecks,
            List<string> errors) where T : class
        {
            try
            {
                using IServiceScope scope = _serviceProvider.CreateScope();
                T? service = scope.ServiceProvider.GetService<T>();
                
                if (service == null)
                {
                    serviceChecks[serviceName] = false;
                    errors.Add($"{serviceName} is not registered");
                    _logger.LogWarning("Service {ServiceName} is not registered", serviceName);
                }
                else
                {
                    serviceChecks[serviceName] = true;
                    _logger.LogDebug("Service {ServiceName} is healthy", serviceName);
                }
            }
            catch (Exception ex)
            {
                serviceChecks[serviceName] = false;
                errors.Add($"{serviceName} failed to resolve: {ex.Message}");
                _logger.LogWarning(ex, "Failed to resolve service {ServiceName}", serviceName);
            }

            await Task.CompletedTask;
        }
    }
} 
