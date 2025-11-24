using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace WebAPI.DependencyInjections;

/// <summary>
/// Extension methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add OpenAPI and Scalar configuration with thread-safety improvements
    /// </summary>
    public static IServiceCollection AddOpenApiConfiguration(this IServiceCollection services)
    {
        // Configure OpenAPI with minimal transformers to avoid concurrency issues
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

            // Use a simple document transformer that doesn't access complex validation attributes
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
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration? configuration = null)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                // Local development origins
                string[] localOrigins = [
                    "http://localhost:3000",
                    "http://localhost:5173",
                    "http://localhost:4200",
                    "http://127.0.0.1:5173"
                ];

                // Get additional origins from configuration
                string[]? configOrigins = null;
                if (configuration != null)
                {
                    configOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                }

                // Build list of explicit origins
                List<string> explicitOrigins = new List<string>(localOrigins);
                if (configOrigins != null)
                {
                    explicitOrigins.AddRange(configOrigins);
                }

                // Use SetIsOriginAllowed to allow pattern matching for Azure domains
                // This allows all azurestaticapps.net and azurewebsites.net domains
                policy.SetIsOriginAllowed(origin =>
                {
                    // Handle null origin
                    if (string.IsNullOrWhiteSpace(origin))
                    {
                        return false;
                    }

                    // Allow explicit origins (localhost and from config)
                    if (explicitOrigins.Contains(origin))
                    {
                        return true;
                    }

                    // Allow Azure Static Web Apps domains (pattern matching)
                    if (origin.Contains("azurestaticapps.net", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    // Allow Azure Websites domains (for testing)
                    if (origin.Contains("azurewebsites.net", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    return false;
                })
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // Required for SignalR
            });

            // Production policy for specific domains
            options.AddPolicy("Production", policy =>
            {
                policy.SetIsOriginAllowed(origin =>
                {
                    // Allow Azure Static Web Apps domains
                    if (origin.Contains("azurestaticapps.net", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    // Allow specific production domains from configuration
                    if (configuration != null)
                    {
                        string[]? allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                        if (allowedOrigins != null && allowedOrigins.Contains(origin))
                        {
                            return true;
                        }
                    }

                    return false;
                })
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
