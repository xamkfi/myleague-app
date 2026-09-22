using Application.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// HTTP status and body chosen for a failed application result.
/// </summary>
/// <param name="StatusCode">HTTP status code.</param>
/// <param name="Message">Top-level message safe to return to the client.</param>
/// <param name="Errors">Detailed messages safe to return to the client.</param>
public readonly record struct ApiErrorHttpDecision(int StatusCode, string Message, List<string> Errors);

/// <summary>
/// Maps a failed <see cref="Result"/> or <see cref="Result{T}"/> to an HTTP status and body.
/// Not-found uses <see cref="ResultErrorKind"/>. Infrastructure exception text from
/// <c>Exception.Flatten()</c> is replaced with a generic 500 outside Development.
/// </summary>
public static class ApiErrorHttpMapper
{
    /// <summary>
    /// Client-facing message when an infrastructure exception must not be returned.
    /// </summary>
    public const string InternalServerErrorMessage = "An internal server error occurred";

    private static readonly string[] InfrastructureExceptionMarkers =
    [
        "DbUpdateConcurrencyException:",
        "DbUpdateException:",
        "PostgresException:",
        "NpgsqlException:"
    ];

    /// <summary>
    /// Decides the HTTP status and the messages that may be written to the response body.
    /// </summary>
    /// <param name="errorKind">Failure classification from the application result.</param>
    /// <param name="error">Top-level error message, if any.</param>
    /// <param name="detailedErrors">Messages from <c>GetAllErrors()</c>.</param>
    /// <param name="defaultMessage">Fallback when <paramref name="error"/> is null.</param>
    /// <param name="isDevelopment">When true, infrastructure exception text is preserved for local seeding.</param>
    public static ApiErrorHttpDecision Map(
        ResultErrorKind errorKind,
        string? error,
        IEnumerable<string> detailedErrors,
        string defaultMessage,
        bool isDevelopment)
    {
        string topMessage = error ?? defaultMessage;
        List<string> errors = detailedErrors.ToList();
        if (errors.Count == 0)
        {
            errors.Add(topMessage);
        }

        if (errorKind == ResultErrorKind.NotFound)
        {
            return new ApiErrorHttpDecision(StatusCodes.Status404NotFound, topMessage, errors);
        }

        if (errorKind == ResultErrorKind.Validation)
        {
            return new ApiErrorHttpDecision(StatusCodes.Status400BadRequest, topMessage, errors);
        }

        if (IsNotFoundMessage(topMessage))
        {
            return new ApiErrorHttpDecision(StatusCodes.Status404NotFound, topMessage, errors);
        }

        if (!isDevelopment && ContainsInfrastructureException(topMessage, errors))
        {
            List<string> hidden = new List<string> { InternalServerErrorMessage };
            return new ApiErrorHttpDecision(
                StatusCodes.Status500InternalServerError,
                InternalServerErrorMessage,
                hidden);
        }

        return new ApiErrorHttpDecision(StatusCodes.Status400BadRequest, topMessage, errors);
    }

    private static bool IsNotFoundMessage(string message) =>
        message.Contains("not found", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsInfrastructureException(string topMessage, IReadOnlyList<string> errors)
    {
        if (HasInfrastructureMarker(topMessage))
        {
            return true;
        }

        foreach (string error in errors)
        {
            if (HasInfrastructureMarker(error))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasInfrastructureMarker(string message)
    {
        foreach (string marker in InfrastructureExceptionMarkers)
        {
            if (message.Contains(marker, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
