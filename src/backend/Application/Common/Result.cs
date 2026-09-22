using FluentValidation.Results;

namespace Application.Common;

/// <summary>
/// Distinguishes why a <see cref="Result"/> or <see cref="Result{T}"/> failed so HTTP
/// status mapping does not depend on the wording of the error message.
/// </summary>
public enum ResultErrorKind
{
    /// <summary>
    /// The operation succeeded.
    /// </summary>
    None = 0,

    /// <summary>
    /// A business or request failure that is not a missing entity or a validation error.
    /// </summary>
    Failure = 1,

    /// <summary>
    /// The requested entity does not exist.
    /// </summary>
    NotFound = 2,

    /// <summary>
    /// Input failed FluentValidation.
    /// </summary>
    Validation = 3
}

/// <summary>
/// Represents the result of an operation with success/failure state
/// </summary>
/// <typeparam name="T">The type of data returned on success</typeparam>
public class Result<T>
{
    private Result(
        bool isSuccess,
        T? data,
        string? error,
        ResultErrorKind errorKind,
        IEnumerable<string>? errors = null,
        IEnumerable<ValidationFailure>? validationFailures = null)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
        ErrorKind = errorKind;
        Errors = errors ?? Enumerable.Empty<string>();
        ValidationFailures = validationFailures ?? Enumerable.Empty<ValidationFailure>();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Data { get; }
    public string? Error { get; }

    /// <summary>
    /// Why the operation failed. <see cref="ResultErrorKind.None"/> when <see cref="IsSuccess"/> is true.
    /// </summary>
    public ResultErrorKind ErrorKind { get; }

    public IEnumerable<string> Errors { get; }
    public IEnumerable<ValidationFailure> ValidationFailures { get; }

    public static Result<T> Success(T data) => new(true, data, null, ResultErrorKind.None);
    public static Result<T> Failure(string error) => new(false, default, error, ResultErrorKind.Failure);
    public static Result<T> Failure(string error, IEnumerable<string> errors) =>
        new(false, default, error, ResultErrorKind.Failure, errors);

    /// <summary>
    /// Creates a failure result from validation errors (string messages)
    /// </summary>
    public static Result<T> ValidationFailure(IEnumerable<string> validationErrors) =>
        new(false, default, "Validation failed", ResultErrorKind.Validation, validationErrors);

    /// <summary>
    /// Creates a failure result from validation failures with full context
    /// </summary>
    public static Result<T> ValidationFailure(IEnumerable<ValidationFailure> validationFailures) =>
        new(false, default, "Validation failed", ResultErrorKind.Validation, null, validationFailures);

    /// <summary>
    /// Creates a not found failure result
    /// </summary>
    public static Result<T> NotFound(string entityName, object key) =>
        new(false, default, $"{entityName} with key '{key}' was not found.", ResultErrorKind.NotFound);

    /// <summary>
    /// Implicit conversion from T to Result<T>
    /// </summary>
    public static implicit operator Result<T>(T data) => Success(data);

    /// <summary>
    /// Gets all error messages combined
    /// </summary>
    public string GetErrorsString() =>
        string.IsNullOrEmpty(Error) ? string.Join("; ", Errors) : Error;

    /// <summary>
    /// Returns every available detailed error string from this result, combining
    /// <see cref="Errors"/> (typically populated from caught exceptions via
    /// <c>ex.Flatten()</c>) with <see cref="ValidationFailures"/> messages (from
    /// FluentValidation). The high-level <see cref="Error"/> message is intentionally
    /// excluded so callers can keep using it for the response's top-level <c>message</c>
    /// while still surfacing the individual error details in the <c>errors</c> array.
    /// </summary>
    public IEnumerable<string> GetAllErrors() =>
        Errors.Concat(ValidationFailures.Select(vf => vf.ErrorMessage));
}

/// <summary>
/// Represents the result of an operation without return data
/// </summary>
public class Result
{
    private Result(
        bool isSuccess,
        string? error,
        ResultErrorKind errorKind,
        IEnumerable<string>? errors = null,
        IEnumerable<ValidationFailure>? validationFailures = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorKind = errorKind;
        Errors = errors ?? Enumerable.Empty<string>();
        ValidationFailures = validationFailures ?? Enumerable.Empty<ValidationFailure>();
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }

    /// <summary>
    /// Why the operation failed. <see cref="ResultErrorKind.None"/> when <see cref="IsSuccess"/> is true.
    /// </summary>
    public ResultErrorKind ErrorKind { get; }

    public IEnumerable<string> Errors { get; }
    public IEnumerable<ValidationFailure> ValidationFailures { get; }

    public static Result Success() => new(true, null, ResultErrorKind.None);
    public static Result Failure(string error) => new(false, error, ResultErrorKind.Failure);
    public static Result Failure(IEnumerable<string> errors) => new(false, null, ResultErrorKind.Failure, errors);
    public static Result Failure(string error, IEnumerable<string> errors) =>
        new(false, error, ResultErrorKind.Failure, errors);

    /// <summary>
    /// Creates a failure result from validation errors (string messages)
    /// </summary>
    public static Result ValidationFailure(IEnumerable<string> validationErrors) =>
        new(false, "Validation failed", ResultErrorKind.Validation, validationErrors);

    /// <summary>
    /// Creates a failure result from validation failures with full context
    /// </summary>
    public static Result ValidationFailure(IEnumerable<ValidationFailure> validationFailures) =>
        new(false, "Validation failed", ResultErrorKind.Validation, null, validationFailures);

    /// <summary>
    /// Creates a not found failure result
    /// </summary>
    public static Result NotFound(string entityName, object key) =>
        new(false, $"{entityName} with key '{key}' was not found.", ResultErrorKind.NotFound);

    /// <summary>
    /// Gets all error messages combined
    /// </summary>
    public string GetErrorsString() =>
        string.IsNullOrEmpty(Error) ? string.Join("; ", Errors) : Error;

    /// <summary>
    /// Returns every available detailed error string from this result, combining
    /// <see cref="Errors"/> (typically populated from caught exceptions via
    /// <c>ex.Flatten()</c>) with <see cref="ValidationFailures"/> messages (from
    /// FluentValidation). The high-level <see cref="Error"/> message is intentionally
    /// excluded so callers can keep using it for the response's top-level <c>message</c>
    /// while still surfacing the individual error details in the <c>errors</c> array.
    /// </summary>
    public IEnumerable<string> GetAllErrors() =>
        Errors.Concat(ValidationFailures.Select(vf => vf.ErrorMessage));
}
