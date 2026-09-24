using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MyLeague.Infrastructure.Persistence.Contexts;
using System.Runtime.InteropServices;

namespace MyLeague.Infrastructure.HealthChecks
{
    /// <summary>
    /// Extension methods for registering health checks
    /// </summary>
    public static class HealthCheckExtensions
    {
        /// <summary>
        /// Tag for checks that <c>/health/ready</c> runs. Azure App Service and deploy
        /// smoke tests call that path, so only PostgreSQL and the four DbContexts use it.
        /// </summary>
        public const string ReadyTag = "ready";

        /// <summary>
        /// Adds comprehensive health checks to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddMyLeagueHealthChecks(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";

            IHealthChecksBuilder healthChecksBuilder = services.AddHealthChecks()
                // Basic application health check
                .AddCheck("self", () => HealthCheckResult.Healthy("API is running"))
                
                // Database connectivity checks
                .AddNpgSql(
                    connectionString,
                    name: "postgresql-connection",
                    tags: new[] { "database", "postgresql", ReadyTag })

                // Entity Framework context checks. These share one database; each
                // context still has to connect so a broken sport model fails readiness.
                .AddDbContextCheck<CommonDbContext>(
                    name: "common-database",
                    tags: new[] { "database", "ef-core", "common", ReadyTag })

                .AddDbContextCheck<FloorballDbContext>(
                    name: "floorball-database",
                    tags: new[] { "database", "ef-core", "floorball", ReadyTag })

                .AddDbContextCheck<FootballDbContext>(
                    name: "football-database",
                    tags: new[] { "database", "ef-core", "football", ReadyTag })

                .AddDbContextCheck<HockeyDbContext>(
                    name: "hockey-database",
                    tags: new[] { "database", "ef-core", "hockey", ReadyTag })

                // Custom database health check
                .AddCheck<DatabaseHealthCheck>(
                    name: "database-operations",
                    tags: new[] { "database", "custom" })
                
                // Application services health check
                .AddCheck<ApplicationServicesHealthCheck>(
                    name: "application-services",
                    tags: new[] { "services", "dependencies" })
                
                // Memory checks
                .AddProcessAllocatedMemoryHealthCheck(
                    maximumMegabytesAllocated: 1000,
                    name: "memory-usage",
                    tags: new[] { "system", "memory" })
                
                .AddPrivateMemoryHealthCheck(
                    maximumMemoryBytes: 1_500_000_000, // 1.5 GB
                    name: "private-memory",
                    tags: new[] { "system", "memory" });

            // Add disk storage check based on operating system
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                healthChecksBuilder.AddDiskStorageHealthCheck(
                    options => options.AddDrive("C:\\", minimumFreeMegabytes: 1000),
                    name: "disk-storage",
                    tags: new[] { "system", "storage" });
            }
            else
            {
                // For Linux/Docker containers, check root filesystem
                healthChecksBuilder.AddDiskStorageHealthCheck(
                    options => options.AddDrive("/", minimumFreeMegabytes: 1000),
                    name: "disk-storage",
                    tags: new[] { "system", "storage" });
            }

            // Register custom health check services
            services.AddScoped<DatabaseHealthCheck>();
            services.AddScoped<ApplicationServicesHealthCheck>();

            return services;
        }
    }
} 
