using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace WebAPI.DependencyInjections;

/// <summary>
/// Extension methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add OpenAPI and Scalar configuration
    /// </summary>
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            var info = new OpenApiInfo
            {
                Title = "MyLeague Club API",
                Version = "v1",
                Description = "API for MyLeague application with comprehensive documentation",
                Contact = new OpenApiContact
                {
                    Name = "MyLeague Team",
                    Email = "support@myleague.com"
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            };

            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = info;
                return Task.CompletedTask;
            });

            // Add security scheme for future JWT implementation
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };
                return Task.CompletedTask;
            });
        });

        return services;
    }

    /// <summary>
    /// Add CORS configuration
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });

            // You can add more specific policies here for production
            options.AddPolicy("Production", policy =>
            {
                policy.WithOrigins("https://yourdomain.com")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Add Health Check UI configuration
    /// </summary>
    public static IServiceCollection AddHealthCheckUIConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecksUI(options =>
        {
            options.SetEvaluationTimeInSeconds(30); // Check every 30 seconds
            options.MaximumHistoryEntriesPerEndpoint(50);
            options.SetApiMaxActiveRequests(1);
            options.SetMinimumSecondsBetweenFailureNotifications(60);

            // Add health check endpoint from configuration
            string healthCheckEndpoint = configuration.GetValue<string>("HealthChecks:UI:Endpoint") ?? "http://localhost:8080/health";
            options.AddHealthCheckEndpoint("MyLeague API", healthCheckEndpoint);
        })
        .AddInMemoryStorage();

        return services;
    }
} 
