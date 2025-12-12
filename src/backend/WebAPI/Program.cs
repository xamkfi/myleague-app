using Application.Configuration;
using Application.DependencyInjections;
using MyLeague.Infrastructure.DependencyInjections;
using MyLeague.Infrastructure.SignalR;
using WebAPI.Middlewares;
using WebAPI.DependencyInjections;
using Serilog;
using Scalar.AspNetCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using System.Text.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Add API Explorer services for OpenAPI
builder.Services.AddEndpointsApiExplorer();

// Add OpenAPI and Scalar configuration using thread-safe extension method
builder.Services.AddOpenApiConfiguration();

// Add CORS configuration using extension method
builder.Services.AddCorsConfiguration();

// Configure pagination options
builder.Services.Configure<PaginationConfiguration>(
    builder.Configuration.GetSection(PaginationConfiguration.SectionName));

// Configure period durations
builder.Services.Configure<PeriodDurationConfiguration>(
    builder.Configuration.GetSection(PeriodDurationConfiguration.SectionName));

// Register application services
builder.Services.AddApplication();

// Register infrastructure services 
builder.Services.AddInfrastructure(builder.Configuration);

// Add Health Check UI configuration using extension method
builder.Services.AddHealthCheckUIConfiguration(builder.Configuration);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Map OpenAPI endpoint at the traditional Swagger location for compatibility
    app.MapOpenApi("/swagger/v1/swagger.json");

    // Configure Scalar UI
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("MyLeague Club API Documentation")
               .WithTheme(ScalarTheme.Purple)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
               .WithOpenApiRoutePattern("/swagger/v1/swagger.json");
    });
}

// Use custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Use built-in middleware
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map SignalR hub
app.MapHub<DomainEventHub>("/api/hubs/domainevent");

// Map health check endpoints with detailed responses
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration.TotalMilliseconds,
            CheckedAt = DateTime.UtcNow,
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description,
                Duration = entry.Value.Duration.TotalMilliseconds,
                Data = entry.Value.Data,
                Tags = entry.Value.Tags
            })
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
});

// Map simple health check for load balancers
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "text/plain";
        await context.Response.WriteAsync(report.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy");
    }
});

// Map liveness check
app.MapGet("/health/live", () => "Alive");

// Map Health Check UI
app.MapHealthChecksUI(options =>
{
    options.UIPath = "/health-ui";
    options.ApiPath = "/health-ui-api";
});

// Log application startup
app.Logger.LogInformation("MyLeague Club API started successfully");
app.Logger.LogInformation("API Documentation available at: /scalar/v1");
app.Logger.LogInformation("OpenAPI JSON available at: /swagger/v1/swagger.json");
app.Logger.LogInformation("Health Check UI available at: /health-ui");
app.Logger.LogInformation("Health Check endpoints:");
app.Logger.LogInformation("  - Detailed: /health");
app.Logger.LogInformation("  - Ready: /health/ready");
app.Logger.LogInformation("  - Live: /health/live");

app.Run();
