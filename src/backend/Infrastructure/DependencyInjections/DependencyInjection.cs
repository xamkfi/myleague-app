using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Services.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using MyLeague.Infrastructure.Persistence;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories.Floorball;
using MyLeague.Infrastructure.Persistence.Repositories.Common;
using MyLeague.Infrastructure.Persistence.UnitOfWork;
using MyLeague.Infrastructure.HealthChecks;
using Application.Interfaces.Common;
using Application.Services.Common;
using MyLeague.Infrastructure.Services.ImageStorage;
using MyLeague.Infrastructure.Services.Common;

namespace MyLeague.Infrastructure.DependencyInjections
{
    /// <summary>
    /// Static class for registering infrastructure services
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds infrastructure services to the specified IServiceCollection
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <param name="configuration">The configuration</param>
        /// <returns>The IServiceCollection so that additional calls can be chained</returns>
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Try multiple ways to get the connection string
            // Priority order:
            // 1. Azure App Service connection strings (POSTGRESQLCONNSTR_ prefix for PostgreSQL type)
            // 2. Standard connection string environment variables
            // 3. Configuration API (GetConnectionString)
            // 4. Configuration API (direct key access)
            string connectionString = Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_DefaultConnection")
                ?? Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection")
                ?? Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? configuration["ConnectionStrings:DefaultConnection"]
                ?? "";
            
            if (string.IsNullOrEmpty(connectionString))
            {
                // Log all attempted sources for debugging
                (string, string?)[] attemptedSources = new[]
                {
                    ("POSTGRESQLCONNSTR_DefaultConnection", Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_DefaultConnection")),
                    ("CUSTOMCONNSTR_DefaultConnection", Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection")),
                    ("SQLCONNSTR_DefaultConnection", Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection")),
                    ("GetConnectionString", configuration.GetConnectionString("DefaultConnection")),
                    ("ConnectionStrings:DefaultConnection", configuration["ConnectionStrings:DefaultConnection"])
                };
                
                string sourceInfo = string.Join(", ", attemptedSources.Select(s => $"{s.Item1}: {(string.IsNullOrEmpty(s.Item2) ? "NOT FOUND" : "FOUND")}"));
                
                throw new InvalidOperationException(
                    $"Connection string 'DefaultConnection' is not configured. " +
                    $"Please set it in App Service connection strings with type 'PostgreSQL'. " +
                    $"Checked sources: {sourceInfo}");
            }

            services.AddDbContext<CommonDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(CommonDbContext).Assembly.FullName)));

            services.AddDbContext<FloorballDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(FloorballDbContext).Assembly.FullName)));

            // Auto-apply migrations
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                using (IServiceScope scope = serviceProvider.CreateScope())
                {
                    CommonDbContext commonDbContext = scope.ServiceProvider.GetRequiredService<CommonDbContext>();
                    commonDbContext.Database.Migrate();

                    FloorballDbContext floorballDbContext = scope.ServiceProvider.GetRequiredService<FloorballDbContext>();
                    floorballDbContext.Database.Migrate();
                }
            }

            // Add repositories
            services.AddScoped<IClubRepository, ClubRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
            services.AddScoped<IDivisionRepository, DivisionRepository>();
            services.AddScoped<IFloorballPlayerRepository, FloorballPlayerRepository>();
            services.AddScoped<IFloorballTeamRepository, FloorballTeamRepository>();
            services.AddScoped<IFloorballTeamManagerRepository, FloorballTeamManagerRepository>();
            services.AddScoped<IFloorballRefereeRepository, FloorballRefereeRepository>();
            services.AddScoped<IFloorballMatchRepository, FloorballMatchRepository>();
            services.AddScoped<IFloorballSeasonRepository, FloorballSeasonRepository>();
            services.AddScoped<IFloorballSeasonDivisionRepository, FloorballSeasonDivisionRepository>();
            services.AddScoped<IFloorballStatisticsRepository, FloorballStatisticsRepository>();
            services.AddScoped<IImageStorageService, AzureBlobImageStorageService>();
            services.AddScoped<IPersonNameProvider, PersonNameProvider>();
            
            // Add timer services
            services.AddScoped<ITimerRepository, TimerRepository>();
            services.AddScoped<ITimerNotificationService, TimerNotificationService>();
            services.AddSingleton<ITimerStore, InMemoryTimerStore>();
            
            // Register timer background service
            services.AddHostedService<TimerBackgroundService>();

            // Add unit of work
            services.AddScoped<IUnitOfWork, CommonUnitOfWork>();
            services.AddScoped<IFloorballUnitOfWork, FloorballUnitOfWork>();


            // Add domain events / SignalR
            services.AddDomainEvents();

            // Add health checks
            services.AddMyLeagueHealthChecks(configuration);

            return services;
        }
    }
}
