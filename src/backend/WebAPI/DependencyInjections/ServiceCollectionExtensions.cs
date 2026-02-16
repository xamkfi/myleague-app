using Microsoft.AspNetCore.Authorization;
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

            // Add JWT Bearer security scheme definition
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

            // Mark operations that require authentication based on [Authorize] attributes
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                IList<object> metadata = context.Description.ActionDescriptor.EndpointMetadata;

                bool hasAuthorize = metadata.OfType<AuthorizeAttribute>().Any();
                bool hasAllowAnonymous = metadata.OfType<AllowAnonymousAttribute>().Any();

                if (hasAuthorize && !hasAllowAnonymous)
                {
                    // Add security requirement referencing the Bearer scheme
                    operation.Security ??= new List<OpenApiSecurityRequirement>();
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });

                    // Add 401 Unauthorized response
                    operation.Responses.TryAdd("401", new OpenApiResponse
                    {
                        Description = "Unauthorized — authentication required"
                    });
                }

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
                policy.WithOrigins(
                        // Local development
                        "http://localhost:3000",
                        "http://localhost:5173",
                        "http://localhost:4200",
                        "http://127.0.0.1:5173",
                        // Azure Static Web Apps (development azure static web app)
                        "https://calm-tree-06b4ac003.2.azurestaticapps.net")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials(); // Required for SignalR
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
