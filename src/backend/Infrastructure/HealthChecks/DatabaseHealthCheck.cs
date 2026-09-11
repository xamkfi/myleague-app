using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace MyLeague.Infrastructure.HealthChecks
{
    /// <summary>
    /// Custom health check for database connectivity and basic operations
    /// </summary>
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly CommonDbContext _commonDbContext;
        private readonly FloorballDbContext _floorballDbContext;
        private readonly HockeyDbContext _hockeyDbContext;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        /// <summary>
        /// Initializes a new instance of the DatabaseHealthCheck class
        /// </summary>
        /// <param name="commonDbContext">The common database context</param>
        /// <param name="floorballDbContext">The floorball database context</param>
        /// <param name="hockeyDbContext">The hockey database context</param>
        /// <param name="logger">The logger instance</param>
        public DatabaseHealthCheck(
            CommonDbContext commonDbContext,
            FloorballDbContext floorballDbContext,
            HockeyDbContext hockeyDbContext,
            ILogger<DatabaseHealthCheck> logger)
        {
            _commonDbContext = commonDbContext ?? throw new ArgumentNullException(nameof(commonDbContext));
            _floorballDbContext = floorballDbContext ?? throw new ArgumentNullException(nameof(floorballDbContext));
            _hockeyDbContext = hockeyDbContext ?? throw new ArgumentNullException(nameof(hockeyDbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Performs the health check by testing database connectivity and basic operations
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
                _logger.LogDebug("Starting database health check");

                // Check Common database connectivity
                bool commonDbCanConnect = await _commonDbContext.Database.CanConnectAsync(cancellationToken);
                if (!commonDbCanConnect)
                {
                    _logger.LogWarning("Common database connection failed");
                    return HealthCheckResult.Unhealthy("Common database is not accessible");
                }

                // Check Floorball database connectivity
                bool floorballDbCanConnect = await _floorballDbContext.Database.CanConnectAsync(cancellationToken);
                if (!floorballDbCanConnect)
                {
                    _logger.LogWarning("Floorball database connection failed");
                    return HealthCheckResult.Unhealthy("Floorball database is not accessible");
                }

                // Check Hockey database connectivity
                bool hockeyDbCanConnect = await _hockeyDbContext.Database.CanConnectAsync(cancellationToken);
                if (!hockeyDbCanConnect)
                {
                    _logger.LogWarning("Hockey database connection failed");
                    return HealthCheckResult.Unhealthy("Hockey database is not accessible");
                }

                // Perform basic query operations to ensure databases are functional
                int commonClubCount = 0;
                int floorballPlayerCount = 0;
                int hockeyTeamCount = 0;
                var warnings = new List<string>();

                try
                {
                    // Test if we can execute a simple query
                    commonClubCount = await _commonDbContext.Clubs.CountAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to count clubs in common database - this may be normal if migrations haven't run yet");
                    warnings.Add("Common database tables may not exist yet");
                }

                try
                {
                    // Test if we can execute a simple query
                    floorballPlayerCount = await _floorballDbContext.FloorballPlayers.CountAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to count players in floorball database - this may be normal if migrations haven't run yet");
                    warnings.Add("Floorball database tables may not exist yet");
                }

                try
                {
                    hockeyTeamCount = await _hockeyDbContext.HockeyTeams.CountAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to count teams in hockey database - this may be normal if migrations haven't run yet");
                    warnings.Add("Hockey database tables may not exist yet");
                }

                var data = new Dictionary<string, object>
                {
                    { "CommonDatabase", "Connected" },
                    { "FloorballDatabase", "Connected" },
                    { "HockeyDatabase", "Connected" },
                    { "ClubCount", commonClubCount },
                    { "PlayerCount", floorballPlayerCount },
                    { "HockeyTeamCount", hockeyTeamCount },
                    { "CheckedAt", DateTime.UtcNow }
                };

                if (warnings.Any())
                {
                    data.Add("Warnings", warnings);
                }

                _logger.LogDebug("Database health check completed successfully");

                // Return healthy even if tables don't exist yet, as long as we can connect
                string description = warnings.Any() 
                    ? $"Databases are accessible but some tables may not exist yet. Warnings: {string.Join(", ", warnings)}"
                    : "All databases are accessible and functional";

                return HealthCheckResult.Healthy(description, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed with exception");
                return HealthCheckResult.Unhealthy("Database health check failed", ex);
            }
        }
    }
} 