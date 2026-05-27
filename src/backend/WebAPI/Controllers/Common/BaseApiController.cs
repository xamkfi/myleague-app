using Application.Common;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Base controller that centralises the success/error mapping every concrete API controller
/// in this project repeats. Concrete controllers should inherit from this and use the
/// <c>HandleResult</c> / <c>ToErrorResponse</c> helpers instead of re-implementing the
/// "if not found return 404, else return 400" branching inline.
/// </summary>
[ApiController]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Returns 200 OK with a success envelope when <paramref name="result"/> succeeded and
    /// carries a non-null payload, otherwise delegates to <see cref="ToErrorResponse{T}"/>.
    /// </summary>
    /// <param name="result">Application/MediatR result to translate into HTTP.</param>
    /// <param name="successMessage">Message embedded in the success envelope.</param>
    /// <param name="defaultErrorMessage">Fallback message when the result has no <see cref="Result{T}.Error"/>.</param>
    protected ActionResult<ApiResponse<T>> HandleResult<T>(
        Result<T> result,
        string successMessage,
        string defaultErrorMessage)
    {
        if (result.IsSuccess && result.Data is not null)
        {
            return Ok(ApiResponse<T>.SuccessResponse(result.Data, successMessage));
        }

        return ToErrorResponse(result, defaultErrorMessage);
    }

    /// <summary>
    /// Maps a failed <see cref="Result{T}"/> to an HTTP error response. Returns 404 NotFound
    /// when the top-level error message contains "not found", otherwise 400 BadRequest.
    /// </summary>
    /// <remarks>
    /// Preserves detailed messages from <see cref="Result{T}.GetAllErrors"/> so the frontend
    /// can show a specific reason instead of just "Validation failed". When the result has
    /// no detailed errors, the top-level message is duplicated into the <c>errors</c> array
    /// so callers can rely on a non-empty list.
    /// </remarks>
    protected ActionResult<ApiResponse<T>> ToErrorResponse<T>(Result<T> result, string defaultMessage)
    {
        string topMessage = result.Error ?? defaultMessage;
        List<string> errors = result.GetAllErrors().ToList();
        if (errors.Count == 0)
        {
            errors.Add(topMessage);
        }

        ApiResponse<T> body = ApiResponse<T>.ErrorResponse(topMessage, errors);

        if (IsNotFoundMessage(topMessage))
        {
            return NotFound(body);
        }

        return BadRequest(body);
    }

    /// <summary>
    /// Non-generic counterpart for handlers that return <see cref="Result"/> without a payload.
    /// </summary>
    protected ActionResult<ApiResponse> ToErrorResponse(Result result, string defaultMessage)
    {
        string topMessage = result.Error ?? defaultMessage;
        List<string> errors = result.GetAllErrors().ToList();
        if (errors.Count == 0)
        {
            errors.Add(topMessage);
        }

        ApiResponse body = new ApiResponse
        {
            Success = false,
            Message = topMessage,
            Errors = errors,
        };

        if (IsNotFoundMessage(topMessage))
        {
            return NotFound(body);
        }

        return BadRequest(body);
    }

    private static bool IsNotFoundMessage(string message) =>
        message.Contains("not found", StringComparison.OrdinalIgnoreCase);
}
