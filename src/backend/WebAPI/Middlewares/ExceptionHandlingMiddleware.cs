using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using WebAPI.Models.Common;

namespace WebAPI.Middlewares;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the ExceptionHandlingMiddleware class
    /// </summary>
    /// <param name="next">The next middleware in the pipeline</param>
    /// <param name="logger">The logger for this middleware</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to handle the HTTP request and any exceptions
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            IHostEnvironment environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
            await HandleExceptionAsync(context, ex, environment.IsDevelopment());
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, bool isDevelopment)
    {
        context.Response.ContentType = "application/json";

        (int StatusCode, ApiResponse Response) response = exception switch
        {
            ArgumentException => (
                (int)HttpStatusCode.BadRequest,
                ApiResponse.ErrorResponse(exception.Message)),
            InvalidOperationException => (
                (int)HttpStatusCode.BadRequest,
                ApiResponse.ErrorResponse(exception.Message)),
            KeyNotFoundException => (
                (int)HttpStatusCode.NotFound,
                ApiResponse.ErrorResponse(exception.Message)),
            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                ApiResponse.ErrorResponse("Unauthorized access")),
            DbUpdateException dbUpdate => (
                (int)HttpStatusCode.Conflict,
                ApiResponse.ErrorResponse(DescribeDatabaseRejection(dbUpdate))),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                ApiResponse.ErrorResponse(DescribeUnexpected(exception, isDevelopment)))
        };

        context.Response.StatusCode = response.StatusCode;

        string jsonResponse = JsonSerializer.Serialize(response.Response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static string DescribeDatabaseRejection(DbUpdateException exception)
    {
        string text = exception.InnerException?.Message ?? exception.Message;
        Match constraint = Regex.Match(text, "unique constraint \"([^\"]+)\"");
        if (constraint.Success)
        {
            return $"The save conflicted with unique constraint {constraint.Groups[1].Value} (duplicate key 23505).";
        }

        if (text.Contains("23505", StringComparison.Ordinal)
            || text.Contains("duplicate key", StringComparison.OrdinalIgnoreCase))
        {
            return "The save conflicted with an existing unique key (duplicate key 23505).";
        }

        string firstLine = text.Split('\n', 2)[0].Trim();
        if (firstLine.Length > 300)
            firstLine = firstLine[..300];

        return $"The database rejected the save: {firstLine}";
    }

    private static string DescribeUnexpected(Exception exception, bool isDevelopment)
    {
        if (!isDevelopment)
            return "An internal server error occurred";

        string detail = exception.InnerException?.Message ?? exception.Message;
        return $"{exception.GetType().Name}: {detail}";
    }
} 
