using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace Application.Behaviors;

/// <summary>
/// Pipeline behavior that provides comprehensive logging for all MediatR requests
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the LoggingBehavior class
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Handles the request with comprehensive logging
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="next">The next handler in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response</returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;
        string requestId = Guid.NewGuid().ToString();
        
        // Log request start with structured data
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["RequestName"] = requestName,
            ["RequestType"] = typeof(TRequest).FullName ?? "UnknownRequestType",
            ["ResponseType"] = typeof(TResponse).FullName ?? "UnknownResponseType"
        }))
        {
            _logger.LogInformation("Starting request {RequestName} with ID {RequestId}", requestName, requestId);
            
            // Log request details in debug mode
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                try
                {
                    string requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions 
                    { 
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });
                    _logger.LogDebug("Request {RequestName} details: {RequestData}", requestName, requestJson);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to serialize request {RequestName} for logging", requestName);
                }
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            
            try
            {
                TResponse response = await next();
                stopwatch.Stop();

                // Log successful completion with performance metrics
                _logger.LogInformation(
                    "Completed request {RequestName} with ID {RequestId} in {ElapsedMs}ms", 
                    requestName, requestId, stopwatch.ElapsedMilliseconds);

                // Log response details in debug mode
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    try
                    {
                        string responseJson = JsonSerializer.Serialize(response, new JsonSerializerOptions 
                        { 
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                        _logger.LogDebug("Response for {RequestName}: {ResponseData}", requestName, responseJson);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to serialize response for {RequestName} for logging", requestName);
                    }
                }

                // Log performance warning for slow requests
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    _logger.LogWarning(
                        "Slow request detected: {RequestName} with ID {RequestId} took {ElapsedMs}ms", 
                        requestName, requestId, stopwatch.ElapsedMilliseconds);
                }

                return response;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Log error with full context
                _logger.LogError(ex, 
                    "Request {RequestName} with ID {RequestId} failed after {ElapsedMs}ms. Error: {ErrorMessage}", 
                    requestName, requestId, stopwatch.ElapsedMilliseconds, ex.Message);
                
                throw;
            }
        }
    }
} 
