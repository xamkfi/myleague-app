using System.Net;
using System.Text.Json;
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
        // Skip exception handling for OPTIONS requests (CORS preflight)
        // CORS middleware will handle these requests
        if (context.Request.Method == "OPTIONS")
        {
            await _next(context);
            return;
        }

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            ArgumentException => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Response = ApiResponse.ErrorResponse(exception.Message)
            },
            KeyNotFoundException => new
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Response = ApiResponse.ErrorResponse("Resource not found")
            },
            UnauthorizedAccessException => new
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Response = ApiResponse.ErrorResponse("Unauthorized access")
            },
            _ => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Response = ApiResponse.ErrorResponse("An internal server error occurred")
            }
        };

        context.Response.StatusCode = response.StatusCode;

        string jsonResponse = JsonSerializer.Serialize(response.Response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
} 
