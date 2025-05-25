using Microsoft.OpenApi.Models;
using System.Reflection;

namespace WebAPI.Extensions;

/// <summary>
/// Extension methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add OpenAPI configuration for Scalar
    /// </summary>
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "MyLeague Club API",
                    Version = "v1",
                    Description = "API for MyLeague application",
                    Contact = new OpenApiContact
                    {
                        Name = "MyLeague Team",
                        Email = "support@myleague.com"
                    }
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
} 
