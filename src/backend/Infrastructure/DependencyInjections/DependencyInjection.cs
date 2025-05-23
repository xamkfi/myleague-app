using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Domain.EventSourcing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLeague.Infrastructure.Persistence;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories.Floorball;
using MyLeague.Infrastructure.Persistence.Repositories.Common;
using MyLeague.Infrastructure.Persistence.EventStores;

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
            string connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                "Host=localhost;Database=myleague;Username=postgres;Password=postgres";

            services.AddDbContext<CommonDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(CommonDbContext).Assembly.FullName)));

            services.AddDbContext<FloorballDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(FloorballDbContext).Assembly.FullName)));

            // Add repositories
            services.AddScoped<IClubRepository, ClubRepository>();
            services.AddScoped<IFloorballPlayerRepository, FloorballPlayerRepository>();
            services.AddScoped<IFloorballTeamRepository, FloorballTeamRepository>();
            services.AddScoped<IFloorballRefereeRepository, FloorballRefereeRepository>();
            services.AddScoped<IFloorballMatchRepository, FloorballMatchRepository>();
            services.AddScoped<IFloorballSeasonRepository, FloorballSeasonRepository>();
            services.AddScoped<IEventSourcedFloorballMatchRepository, EventSourcedFloorballMatchRepository>();

            // Add event sourcing
            services.AddScoped<IEventStore, FloorballEventStore>();
            services.AddScoped<IEventStore, CommonEventStore>();

            // Add domain events
            services.AddDomainEvents();

            return services;
        }
    }
}
