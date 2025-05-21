using Domain.Repositories.Floorball;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyLeague.Infrastructure.DependencyInjections;
using MyLeague.Infrastructure.Persistence;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories.Floorball;

namespace MyLeague.Infrastructure
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
            // Add database context
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
            
            // Add repositories
            services.AddScoped<IFloorballPlayerRepository, FloorballPlayerRepository>();
            services.AddScoped<IFloorballTeamRepository, FloorballTeamRepository>();
            services.AddScoped<IFloorballRefereeRepository, FloorballRefereeRepository>();
            services.AddScoped<IFloorballMatchRepository, FloorballMatchRepository>();
            services.AddScoped<IFloorballSeasonRepository, FloorballSeasonRepository>();
            
            // Add domain events
            services.AddDomainEvents();
            
            return services;
        }
    }
} 
