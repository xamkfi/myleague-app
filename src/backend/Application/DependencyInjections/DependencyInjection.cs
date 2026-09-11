using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Application.Behaviors;
using Application.Features.Common.Deletion;
using Application.Features.Common.MatchTimer.Services;
using Application.Services.Common;
using MediatR;
using FluentValidation;

namespace Application.DependencyInjections;

/// <summary>
/// Extension methods for setting up application services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds application services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        
        // Register MediatR with pipeline behaviors
        services.AddMediatR(cfg => 
        {
            cfg.RegisterServicesFromAssembly(assembly);
            
            // Add pipeline behaviors - order matters!
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });
        
        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);
        
        // Register application services
        services.AddScoped<IPaginationService, PaginationService>();
        services.AddScoped<IClubAdminAccessService, ClubAdminAccessService>();
        services.AddScoped<IPersonDeletionGuard, PersonDeletionGuard>();
        
        // Register timer services
        services.AddScoped<IMatchTimerService, PersistentMatchTimerService>();

        return services;
    }
} 
