using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Application.Handlers.Clubs;
using Application.Commands.Clubs;
using Application.Queries.Clubs;
using Application.DTOs.Common;
using MediatR;
using System.Collections.Generic;

namespace Application.Extensions;

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
        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // Explicit handler registrations for clarity (optional since MediatR auto-discovers them)
        RegisterClubHandlers(services);
        
        return services;
    }

    /// <summary>
    /// Registers all club-related handlers
    /// </summary>
    /// <param name="services">The service collection</param>
    private static void RegisterClubHandlers(IServiceCollection services)
    {
        // Command handlers
        services.AddScoped<IRequestHandler<CreateClubCommand, ClubDto>, CreateClubHandler>();
        services.AddScoped<IRequestHandler<UpdateClubCommand, ClubDto>, UpdateClubHandler>();
        services.AddScoped<IRequestHandler<DeleteClubCommand>, DeleteClubHandler>();
        
        // Query handlers
        services.AddScoped<IRequestHandler<GetClubByIdQuery, ClubDto>, GetClubByIdHandler>();
        services.AddScoped<IRequestHandler<GetAllClubsQuery, IEnumerable<ClubDto>>, GetAllClubsHandler>();
    }
} 