using FluentValidation.Results;

namespace Application.Common;

/// <summary>
/// Represents the result of an operation with success/failure state
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class Result<T>
{
    private Result(bool isSuccess, T? data, string? error, IEnumerable<string>? errors = null, IEnumerable<ValidationFailure>? validationFailures = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        Errors = errors ?? Enumerable.Empty<string>();
        ValidationFailures = validationFailures ?? Enumerable.Empty<ValidationFailure>();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Data { get; }
    public string? Error { get; }
    public IEnumerable<string> Errors { get; }
    public IEnumerable<ValidationFailure> ValidationFailures { get; }

    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string error) => new(false, default, error);
    public static Result<T> Failure(IEnumerable<string> errors) => new(false, default, null, errors);
    
    /// <summary>
    /// Creates a failure result from validation errors (string messages)
    /// </summary>
    public static Result<T> ValidationFailure(IEnumerable<string> validationErrors) => 
        new(false, default, "Validation failed", validationErrors);
    
    /// <summary>
    /// Creates a failure result from validation failures with full context
    /// </summary>
    public static Result<T> ValidationFailure(IEnumerable<ValidationFailure> validationFailures) => 
        new(false, default, "Validation failed", null, validationFailures);
    
    /// <summary>
    /// Creates a not found failure result
    /// </summary>
    public static Result<T> NotFound(string entityName, object key) => 
        new(false, default, $"{entityName} with key '{key}' was not found.");
    
    /// <summary>
    /// Implicit conversion from T to Result<T>
    /// </summary>
    public static implicit operator Result<T>(T data) => Success(data);
    
    /// <summary>
    /// Gets all error messages combined
    /// </summary>
    public string GetErrorsString() => 
        string.IsNullOrEmpty(Error) ? string.Join("; ", Errors) : Error;
}

/// <summary>
/// Represents the result of an operation without return data
/// </summary>
public class Result
{
    private Result(bool isSuccess, string? error, IEnumerable<string>? errors = null, IEnumerable<ValidationFailure>? validationFailures = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors ?? Enumerable.Empty<string>();
        ValidationFailures = validationFailures ?? Enumerable.Empty<ValidationFailure>();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public IEnumerable<string> Errors { get; }
    public IEnumerable<ValidationFailure> ValidationFailures { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(IEnumerable<string> errors) => new(false, null, errors);
    
    /// <summary>
    /// Creates a failure result from validation errors (string messages)
    /// </summary>
    public static Result ValidationFailure(IEnumerable<string> validationErrors) => 
        new(false, "Validation failed", validationErrors);
    
    /// <summary>
    /// Creates a failure result from validation failures with full context
    /// </summary>
    public static Result ValidationFailure(IEnumerable<ValidationFailure> validationFailures) => 
        new(false, "Validation failed", null, validationFailures);
    
    /// <summary>
    /// Creates a not found failure result
    /// </summary>
    public static Result NotFound(string entityName, object key) => 
        new(false, $"{entityName} with key '{key}' was not found.");
    
    /// <summary>
    /// Gets all error messages combined
    /// </summary>
    public string GetErrorsString() => 
        string.IsNullOrEmpty(Error) ? string.Join("; ", Errors) : Error;
} 