using Application.Common;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;
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
    /// Returns 200 OK with a paginated success envelope when <paramref name="result"/> succeeded,
    /// otherwise maps the failure to 404 or 400 (never 500 for domain/query failures).
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

    private ActionResult<PaginatedApiResponse<T>> ToPaginatedErrorResponse<T>(
        Result<PagedResult<T>> result,
        string defaultMessage)
    {
        string topMessage = result.Error ?? defaultMessage;
        List<string> errors = result.GetAllErrors().ToList();
        if (errors.Count == 0)
        {
            errors.Add(topMessage);
        }

        PaginatedApiResponse<T> body = new PaginatedApiResponse<T>
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

    private ActionResult<ApiResponse<List<T>>> ToListErrorResponse<T>(
        Result<IEnumerable<T>> result,
        string defaultMessage)
    {
        string topMessage = result.Error ?? defaultMessage;
        List<string> errors = result.GetAllErrors().ToList();
        if (errors.Count == 0)
        {
            errors.Add(topMessage);
        }

        ApiResponse<List<T>> body = ApiResponse<List<T>>.ErrorResponse(topMessage, errors);

        if (IsNotFoundMessage(topMessage))
        {
            return NotFound(body);
        }

        return BadRequest(body);
    }

    private ActionResult<ApiResponse> ToVoidErrorResponse<T>(Result<T> result, string defaultMessage)
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
