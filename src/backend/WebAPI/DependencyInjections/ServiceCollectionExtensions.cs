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
            // Build list of allowed origins
            List<string> allowedOrigins = new List<string>
            {
                "http://localhost:3000",
                "http://localhost:5173",
                "http://localhost:4200",
                "http://127.0.0.1:5173"
            };

            // Add configured origins from appsettings or environment variables
            if (configuration != null)
            {
                string? corsOrigins = configuration["Cors:AllowedOrigins"];
                Console.WriteLine($"[CORS] Checking for Cors:AllowedOrigins. Found: {(string.IsNullOrEmpty(corsOrigins) ? "NOT FOUND" : corsOrigins)}");

                if (!string.IsNullOrEmpty(corsOrigins))
                {
                    string[] origins = corsOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries);
                    foreach (string origin in origins)
                    {
                        string trimmedOrigin = origin.Trim();
                        if (!string.IsNullOrEmpty(trimmedOrigin) && !allowedOrigins.Contains(trimmedOrigin))
                        {
                            allowedOrigins.Add(trimmedOrigin);
                            Console.WriteLine($"[CORS] Added origin: {trimmedOrigin}");
                        }
                    }
                }
            }

            Console.WriteLine($"[CORS] Total allowed origins: {allowedOrigins.Count}");
            foreach (string origin in allowedOrigins)
            {
                Console.WriteLine($"[CORS]   - {origin}");
            }

            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins(allowedOrigins.ToArray())
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials(); // Required for SignalR
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
