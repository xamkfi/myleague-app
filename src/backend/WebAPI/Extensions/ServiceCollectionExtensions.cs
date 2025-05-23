using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace WebAPI.Extensions;

/// <summary>
/// Extension methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Swagger configuration
    /// </summary>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MyLeague Club API",
                Version = "v1",
                Description = "API for MyLeague application",
                Contact = new OpenApiContact
                {
                    Name = "MyLeague Team",
                    Email = "support@myleague.com"
                }
            });

            // Include XML comments
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // Add security definition if needed in the future
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
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
