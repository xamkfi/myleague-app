using Application.Common;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

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
    /// Maps a failed <see cref="Result{T}"/> to an HTTP error response.
    /// </summary>
    /// <remarks>
    /// 404 when <see cref="Result{T}.ErrorKind"/> is <see cref="ResultErrorKind.NotFound"/>.
    /// Validation failures stay 400 even if a message contains "not found". Other failures
    /// still use the "not found" text fallback. Infrastructure exception text becomes a
    /// generic 500 outside Development. Detailed messages from <see cref="Result{T}.GetAllErrors"/>
    /// are preserved so the frontend can show a specific reason. When the result has no
    /// detailed errors, the top-level message is duplicated into the <c>errors</c> array.
    /// </remarks>
    protected ActionResult<ApiResponse<T>> ToErrorResponse<T>(Result<T> result, string defaultMessage)
    {
        ApiErrorHttpDecision decision = MapFailure(result.ErrorKind, result.Error, result.GetAllErrors(), defaultMessage);
        ApiResponse<T> body = ApiResponse<T>.ErrorResponse(decision.Message, decision.Errors);
        return ToErrorActionResult(decision.StatusCode, body);
    }

    /// <summary>
    /// Returns 200 OK with a paginated success envelope when <paramref name="result"/> succeeded,
    /// otherwise maps the failure with the same status rules as <see cref="ToErrorResponse{T}"/>.
    /// </summary>
    protected ActionResult<PaginatedApiResponse<T>> HandlePaginatedResult<T>(
        Result<PagedResult<T>> result,
        string successMessage,
        string defaultErrorMessage)
    {
        if (result.IsSuccess && result.Data is not null)
        {
            return Ok(PaginatedApiResponse<T>.SuccessResponse(result.Data, successMessage));
        }

        return ToPaginatedErrorResponse(result, defaultErrorMessage);
    }

    /// <summary>
    /// Returns 200 OK with a list success envelope when <paramref name="result"/> succeeded,
    /// materialising the sequence to a <see cref="List{T}"/> for the response body.
    /// </summary>
    protected ActionResult<ApiResponse<List<T>>> HandleListResult<T>(
        Result<IEnumerable<T>> result,
        string successMessage,
        string defaultErrorMessage)
    {
        if (result.IsSuccess && result.Data is not null)
        {
            return Ok(ApiResponse<List<T>>.SuccessResponse(result.Data.ToList(), successMessage));
        }

        return ToListErrorResponse(result, defaultErrorMessage);
    }

    /// <summary>
    /// For mutation handlers that return <see cref="Result{T}"/> but expose a payload-free
    /// <see cref="ApiResponse"/> on success (e.g. delete endpoints).
    /// </summary>
    protected ActionResult<ApiResponse> HandleVoidResult<T>(
        Result<T> result,
        string successMessage,
        string defaultErrorMessage)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse(successMessage));
        }

        return ToVoidErrorResponse(result, defaultErrorMessage);
    }

    /// <summary>
    /// Non-generic overload for handlers that return <see cref="Result"/> without a payload.
    /// </summary>
    protected ActionResult<ApiResponse> HandleVoidResult(
        Result result,
        string successMessage,
        string defaultErrorMessage)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse(successMessage));
        }

        return ToErrorResponse(result, defaultErrorMessage);
    }

    /// <summary>
    /// Non-generic counterpart for handlers that return <see cref="Result"/> without a payload.
    /// </summary>
    protected ActionResult<ApiResponse> ToErrorResponse(Result result, string defaultMessage)
    {
        ApiErrorHttpDecision decision = MapFailure(result.ErrorKind, result.Error, result.GetAllErrors(), defaultMessage);
        ApiResponse body = new ApiResponse
        {
            Success = false,
            Message = decision.Message,
            Errors = decision.Errors,
        };

        return ToErrorActionResult(decision.StatusCode, body);
    }

    private ActionResult<PaginatedApiResponse<T>> ToPaginatedErrorResponse<T>(
        Result<PagedResult<T>> result,
        string defaultMessage)
    {
        ApiErrorHttpDecision decision = MapFailure(result.ErrorKind, result.Error, result.GetAllErrors(), defaultMessage);
        PaginatedApiResponse<T> body = new PaginatedApiResponse<T>
        {
            Success = false,
            Message = decision.Message,
            Errors = decision.Errors,
        };

        return ToErrorActionResult(decision.StatusCode, body);
    }

    private ActionResult<ApiResponse<List<T>>> ToListErrorResponse<T>(
        Result<IEnumerable<T>> result,
        string defaultMessage)
    {
        ApiErrorHttpDecision decision = MapFailure(result.ErrorKind, result.Error, result.GetAllErrors(), defaultMessage);
        ApiResponse<List<T>> body = ApiResponse<List<T>>.ErrorResponse(decision.Message, decision.Errors);
        return ToErrorActionResult(decision.StatusCode, body);
    }

    private ActionResult<ApiResponse> ToVoidErrorResponse<T>(Result<T> result, string defaultMessage)
    {
        ApiErrorHttpDecision decision = MapFailure(result.ErrorKind, result.Error, result.GetAllErrors(), defaultMessage);
        ApiResponse body = new ApiResponse
        {
            Success = false,
            Message = decision.Message,
            Errors = decision.Errors,
        };

        return ToErrorActionResult(decision.StatusCode, body);
    }

    private ApiErrorHttpDecision MapFailure(
        ResultErrorKind errorKind,
        string? error,
        IEnumerable<string> detailedErrors,
        string defaultMessage)
    {
        return ApiErrorHttpMapper.Map(
            errorKind,
            error,
            detailedErrors,
            defaultMessage,
            IsDevelopmentEnvironment());
    }

    private bool IsDevelopmentEnvironment()
    {
        IHostEnvironment? environment = HttpContext?.RequestServices.GetService<IHostEnvironment>();
        return environment?.IsDevelopment() == true;
    }

    private ActionResult<TBody> ToErrorActionResult<TBody>(int statusCode, TBody body)
    {
        if (statusCode == StatusCodes.Status404NotFound)
        {
            return NotFound(body);
        }

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, body);
        }

        return BadRequest(body);
    }

    /// <summary>
    /// Strips newline characters from a user-provided value before writing it to logs.
    /// Mitigates log-forging when request fields are included in structured log parameters.
    /// </summary>
    protected static string SanitizeForLog(object? value)
    {
        if (value is null)
        {
            return "null";
        }

        // Use the two-argument Replace overloads so CodeQL models this as a
        // log-forging sanitizer (the StringComparison overload is not modeled).
        return value.ToString()!
            .Replace(Environment.NewLine, string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty);
    }
}
