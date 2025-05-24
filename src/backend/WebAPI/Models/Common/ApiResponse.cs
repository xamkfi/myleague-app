namespace WebAPI.Models.Common;

/// <summary>
/// Standard API response wrapper for consistent response format
/// </summary>
/// <typeparam name="T">Type of the response data</typeparam>
public record ApiResponse<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Gets the response data if the operation was successful
    /// </summary>
    public T? Data { get; init; }
    
    /// <summary>
    /// Gets the success or informational message
    /// </summary>
    public string? Message { get; init; }
    
    /// <summary>
    /// Gets the list of error messages if the operation failed
    /// </summary>
    public List<string>? Errors { get; init; }

    /// <summary>
    /// Creates a successful response with data and optional message
    /// </summary>
    /// <param name="data">The response data</param>
    /// <param name="message">Optional success message</param>
    /// <returns>A successful API response</returns>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    /// <summary>
    /// Creates an error response with a single error message
    /// </summary>
    /// <param name="error">The error message</param>
    /// <returns>An error API response</returns>
    public static ApiResponse<T> ErrorResponse(string error)
        => new() { Success = false, Errors = [error] };

    /// <summary>
    /// Creates an error response with multiple error messages
    /// </summary>
    /// <param name="errors">The list of error messages</param>
    /// <returns>An error API response</returns>
    public static ApiResponse<T> ErrorResponse(List<string> errors)
        => new() { Success = false, Errors = errors };
}

/// <summary>
/// Standard API response wrapper for operations without data
/// </summary>
public record ApiResponse
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    public bool Success { get; init; }
    
    /// <summary>
    /// Gets the success or informational message
    /// </summary>
    public string? Message { get; init; }
    
    /// <summary>
    /// Gets the list of error messages if the operation failed
    /// </summary>
    public List<string>? Errors { get; init; }

    /// <summary>
    /// Creates a successful response with optional message
    /// </summary>
    /// <param name="message">Optional success message</param>
    /// <returns>A successful API response</returns>
    public static ApiResponse SuccessResponse(string? message = null)
        => new() { Success = true, Message = message };

    /// <summary>
    /// Creates an error response with a single error message
    /// </summary>
    /// <param name="error">The error message</param>
    /// <returns>An error API response</returns>
    public static ApiResponse ErrorResponse(string error)
        => new() { Success = false, Errors = [error] };

    /// <summary>
    /// Creates an error response with multiple error messages
    /// </summary>
    /// <param name="errors">The list of error messages</param>
    /// <returns>An error API response</returns>
    public static ApiResponse ErrorResponse(List<string> errors)
        => new() { Success = false, Errors = errors };
} 
