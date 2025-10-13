using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.Services.Floorball;
using Domain.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLeague.Infrastructure.Persistence;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories.Floorball;
using MyLeague.Infrastructure.Persistence.Repositories.Common;
using MyLeague.Infrastructure.Persistence.EventStores;
using MyLeague.Infrastructure.Persistence.UnitOfWork;
using MyLeague.Infrastructure.HealthChecks;
using Application.Interfaces.Common;
using Application.Services.Common;
using Application.Interfaces.Common;
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
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? "";

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
            services.AddScoped<IEventSourcedFloorballMatchRepository, EventSourcedFloorballMatchRepository>();
            services.AddScoped<IFloorballStatisticsRepository, FloorballStatisticsRepository>();
            services.AddScoped<IImageStorageService, AzureBlobImageStorageService>();
            services.AddScoped<ITokenService, JwtTokenService>();
            
            // Add timer services
            services.AddScoped<ITimerRepository, TimerRepository>();
            services.AddScoped<ITimerNotificationService, TimerNotificationService>();
            services.AddSingleton<ITimerStore, InMemoryTimerStore>();
            
            // Register timer background service
            services.AddHostedService<TimerBackgroundService>();

            // Add unit of work
            services.AddScoped<IUnitOfWork, CommonUnitOfWork>();
            services.AddScoped<IFloorballUnitOfWork, FloorballUnitOfWork>();

            // Add event sourcing
            services.AddScoped<IFloorballEventStore, FloorballEventStore>();
            services.AddScoped<ICommonEventStore, CommonEventStore>();

            // Add domain events
            services.AddDomainEvents();

            // Add health checks
            services.AddMyLeagueHealthChecks(configuration);

            return services;
        }
    }
}
