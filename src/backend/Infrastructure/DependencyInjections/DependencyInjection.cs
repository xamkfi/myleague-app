// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Configuration;
using Application.Features.Common.MatchTimer.Services;
using Application.Interfaces.Auth;
using Application.Interfaces.Common;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyLeague.Infrastructure.HealthChecks;
using MyLeague.Infrastructure.Persistence.Contexts;
using MyLeague.Infrastructure.Persistence.Repositories.Common;
using MyLeague.Infrastructure.Persistence.Repositories.Floorball;
using MyLeague.Infrastructure.Persistence.UnitOfWork;
using MyLeague.Infrastructure.Services.Auth;
using MyLeague.Infrastructure.Services.Common;
using MyLeague.Infrastructure.Services.ImageStorage;
using MyLeague.Infrastructure.Services.Seeding;

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

            services.AddScoped<ICommonDbContext>(sp =>
                sp.GetRequiredService<CommonDbContext>());

            services.AddDbContext<FloorballDbContext>(options =>
                options.UseNpgsql(
                    connectionString,
                    b => b.MigrationsAssembly(typeof(FloorballDbContext).Assembly.FullName)));

            // Auto-apply migrations and seed data
            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                using (IServiceScope scope = serviceProvider.CreateScope())
                {
                    CommonDbContext commonDbContext = scope.ServiceProvider.GetRequiredService<CommonDbContext>();
                    commonDbContext.Database.Migrate();

                    FloorballDbContext floorballDbContext = scope.ServiceProvider.GetRequiredService<FloorballDbContext>();
                    floorballDbContext.Database.Migrate();

                    // Seed default users after migrations
                    DatabaseSeeder seeder = new();
                    IWebHostEnvironment env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                    seeder.SeedAsync(scope.ServiceProvider, env, configuration).GetAwaiter().GetResult();
                }
            }

            // Add repositories
            services.AddScoped<IClubRepository, ClubRepository>();
            services.AddScoped<IPersonRepository, PersonRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<INewsArticleRepository, NewsArticleRepository>();
            services.AddScoped<IDivisionRepository, DivisionRepository>();
            services.AddScoped<IFloorballPlayerRepository, FloorballPlayerRepository>();
            services.AddScoped<IFloorballTeamRepository, FloorballTeamRepository>();
            services.AddScoped<IFloorballTeamManagerRepository, FloorballTeamManagerRepository>();
            services.AddScoped<IFloorballRefereeRepository, FloorballRefereeRepository>();
            services.AddScoped<IFloorballMatchRepository, FloorballMatchRepository>();
            services.AddScoped<IFloorballCompetitionRepository, FloorballCompetitionRepository>();
            services.AddScoped<IFloorballTournamentRepository, FloorballTournamentRepository>();
            services.AddScoped<IFloorballCompetitionDivisionRepository, FloorballCompetitionDivisionRepository>();
            services.AddScoped<IFloorballStatisticsRepository, FloorballStatisticsRepository>();
            services.AddScoped<IImageStorageService>(sp =>
            {
                IConfiguration config = sp.GetRequiredService<IConfiguration>();
                IWebHostEnvironment env = sp.GetRequiredService<IWebHostEnvironment>();

                // Use Azure Blob Storage when ConnectionStrings:AzureBlobStorage is configured
                // Otherwise use local file storage in Development
                bool hasAzureConfig = !string.IsNullOrWhiteSpace(config.GetConnectionString("AzureBlobStorage"));
                bool useLocalStorage = env.IsDevelopment() && !hasAzureConfig;

                if (useLocalStorage)
                {
                    return new LocalFileImageStorageService(
                        env,
                        sp.GetRequiredService<IHttpContextAccessor>(),
                        config,
                        sp.GetRequiredService<ILogger<LocalFileImageStorageService>>());
                }
                return new AzureBlobImageStorageService(
                    config,
                    sp.GetRequiredService<ILogger<AzureBlobImageStorageService>>());
            });
            services.AddScoped<IPersonNameProvider, PersonNameProvider>();

            // Add authentication services
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IEmailService>(sp =>
            {
                IWebHostEnvironment env = sp.GetRequiredService<IWebHostEnvironment>();
                IConfiguration config = sp.GetRequiredService<IConfiguration>();

                // Use console logging in development when Azure Communication Services is not configured
                bool hasAzureConfig = !string.IsNullOrWhiteSpace(
                    config.GetSection("AzureCommunicationServices:ConnectionString").Value);
                bool useConsole = env.IsDevelopment() && !hasAzureConfig;

                if (useConsole)
                {
                    return new ConsoleLoginCodeEmailService(
                        sp.GetRequiredService<ILogger<ConsoleLoginCodeEmailService>>());
                }
                return new AzureCommunicationEmailService(
                    sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureCommunicationServicesConfiguration>>(),
                    sp.GetRequiredService<ILogger<AzureCommunicationEmailService>>());
            });

            // Add timer services
            services.AddScoped<ITimerRepository, TimerRepository>();
            services.AddScoped<ITimerNotificationService, TimerNotificationService>();
            services.AddSingleton<ITimerStore, InMemoryTimerStore>();

            // Register timer background service
            // No need for it now so disabled by default
            // services.AddHostedService<TimerBackgroundService>();

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
