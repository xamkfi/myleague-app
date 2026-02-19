using Application.Common;
using Application.DTOs.Common;
using Application.Features.Auth.Commands;
using Application.Features.Auth.DTOs;
using Application.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Auth;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Auth;

/// <summary>
/// Controller for passwordless email authentication
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the AuthController class
    /// </summary>
    /// <param name="mediator">The mediator</param>
    /// <param name="logger">The logger</param>
    /// <param name="environment">The web host environment</param>
    public AuthController(IMediator mediator, ILogger<AuthController> logger, IWebHostEnvironment environment)
    {
        _mediator = mediator;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Request a login code to be sent to the specified email
    /// </summary>
    /// <param name="request">The login request containing the email</param>
    /// <returns>Success response (always returns 200 to prevent email enumeration)</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginRequest request)
    {
        _logger.LogInformation("Login code requested for email: {Email}", request.Email);

        RequestLoginCodeCommand command = new(request.Email);
        Result<string?> result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            // In development, include the code in the response for auto-fill convenience
            if (_environment.IsDevelopment() && result.Data != null)
            {
                return Ok(ApiResponse<object>.SuccessResponse(
                    new { devCode = result.Data },
                    "If an account exists with this email, a login code has been sent."));
            }

            return Ok(ApiResponse.SuccessResponse("If an account exists with this email, a login code has been sent."));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return BadRequest(ApiResponse.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Verify a login code and receive authentication tokens
    /// </summary>
    /// <param name="request">The verify request containing email and code</param>
    /// <returns>Authentication tokens if the code is valid</returns>
    [HttpPost("verify")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthTokenDto>>> Verify([FromBody] VerifyCodeRequest request)
    {
        _logger.LogInformation("Login code verification for email: {Email}", request.Email);

        VerifyLoginCodeCommand command = new(request.Email, request.Code);
        Result<AuthTokenDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<AuthTokenDto>.SuccessResponse(result.Data, "Login successful."));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return Unauthorized(ApiResponse<AuthTokenDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Refresh authentication tokens using a valid refresh token
    /// </summary>
    /// <param name="request">The refresh request containing the refresh token</param>
    /// <returns>New authentication tokens</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<AuthTokenDto>>> Refresh([FromBody] RefreshTokenRequest request)
    {
        RefreshTokenCommand command = new(request.RefreshToken);
        Result<AuthTokenDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<AuthTokenDto>.SuccessResponse(result.Data, "Tokens refreshed successfully."));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return Unauthorized(ApiResponse<AuthTokenDto>.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Logout by revoking the refresh token
    /// </summary>
    /// <param name="request">The logout request containing the refresh token to revoke</param>
    /// <returns>Success response</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse>> Logout([FromBody] LogoutRequest request)
    {
        RevokeTokenCommand command = new(request.RefreshToken);
        Result<bool> result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse("Logged out successfully."));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return BadRequest(ApiResponse.ErrorResponse(errorMessage));
    }

    /// <summary>
    /// Get the current authenticated user's information
    /// </summary>
    /// <returns>The current user's information</returns>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me()
    {
        string? userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
        {
            return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Invalid token."));
        }

        GetUserByIdQuery query = new(userId);
        Result<UserDto> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<UserDto>.SuccessResponse(result.Data, "User retrieved successfully."));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return Unauthorized(ApiResponse<UserDto>.ErrorResponse(errorMessage));
    }
}
