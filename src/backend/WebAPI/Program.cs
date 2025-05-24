using Application.DependencyInjections;
using MyLeague.Infrastructure.DependencyInjections;
using WebAPI.Middlewares;
using WebAPI.Extensions;
using Serilog;
using FluentValidation.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddControllers();

// Add FluentValidation (using new non-obsolete methods)
builder.Services.AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

// Add API Explorer services for Swagger
builder.Services.AddEndpointsApiExplorer();

// Add Swagger configuration using extension method
builder.Services.AddSwaggerConfiguration();

// Add CORS configuration using extension method
builder.Services.AddCorsConfiguration();

// Register application services
builder.Services.AddApplication();

// Register infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add health checks
builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyLeague Club API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
}

// Use custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Use built-in middleware
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Map health check endpoint
app.MapHealthChecks("/health");

// Log application startup
app.Logger.LogInformation("MyLeague Club API started successfully");

app.Run(); 
