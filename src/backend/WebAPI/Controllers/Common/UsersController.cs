using Domain.Constants;
using Application.Common;
using Application.Features.Common.Users.Commands;
using Application.Features.Common.Users.DTOs;
using Application.Features.Common.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using Microsoft.AspNetCore.Http;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Controller for managing users
/// </summary>
[Authorize(Roles = AuthRoles.AdminOnly)]
[Route("api/[controller]")]
public class UsersController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    /// <summary>
    /// Initializes a new instance of the UsersController class
    /// </summary>
    /// <param name="mediator">The mediator</param>
    /// <param name="logger">The logger</param>
    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all users
    /// </summary>
    /// <returns>List of all users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers()
    {
        _logger.LogInformation("Getting all users");

        GetAllUsersQuery query = new();
        Result<IEnumerable<UserDto>> result = await _mediator.Send(query);

        return HandleListResult(result, "Users retrieved successfully", "Failed to retrieve users");
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(Guid id)
    {
        _logger.LogInformation("Getting user by ID: {Id}", id);

        GetUserByIdQuery query = new(id);
        Result<UserDto> result = await _mediator.Send(query);

        return HandleResult(result, "User retrieved successfully", "User not found");
    }

    /// <summary>
    /// Get user by email
    /// </summary>
    /// <param name="email">The email to search for</param>
    /// <returns>The user</returns>
    [HttpGet("by-email")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse("Email parameter is required"));
        }

        _logger.LogInformation("Getting user by email: {Email}", SanitizeForLog(email));

        GetUserByEmailQuery query = new(email);
        Result<UserDto> result = await _mediator.Send(query);

        return HandleResult(result, "User retrieved successfully", "User not found");
    }

    /// <summary>
    /// Get user by person ID
    /// </summary>
    /// <param name="personId">The person ID to search for</param>
    /// <returns>The user</returns>
    [HttpGet("by-person/{personId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByPersonId(Guid personId)
    {
        _logger.LogInformation("Getting user by person ID: {PersonId}", personId);

        GetUserByPersonIdQuery query = new(personId);
        Result<UserDto> result = await _mediator.Send(query);

        return HandleResult(result, "User retrieved successfully", "User not found");
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">The user creation request</param>
    /// <returns>The created user</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserRequest request)
    {
        _logger.LogInformation("Creating new user: {Email}", SanitizeForLog(request.Email));

        CreateUserCommand command = new(
            request.Email,
            request.PersonId,
            request.Role,
            request.ClubAssignments);
        Result<UserDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetUserById),
                new { id = result.Data.Id },
                ApiResponse<UserDto>.SuccessResponse(result.Data, "User created successfully")
            );
        }

        return ToErrorResponse(result, "Failed to create user");
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <param name="request">The user update request</param>
    /// <returns>The updated user</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        _logger.LogInformation("Updating user: {Id}", id);

        UpdateUserCommand command = new(id, request.Email, request.Role, request.IsActive);
        Result<UserDto> result = await _mediator.Send(command);

        return HandleResult(result, "User updated successfully", "Failed to update user");
    }

    /// <summary>
    /// Resend the admin invitation email to a user who has not yet verified their email.
    /// Generates a fresh 48-hour verification token and sends a new invitation.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/resend-invitation")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> ResendInvitation(Guid id)
    {
        _logger.LogInformation("Resending admin invitation for user: {Id}", id);

        ResendAdminInvitationCommand command = new(id);
        Result<bool> result = await _mediator.Send(command);

        return HandleVoidResult(result, "Invitation email resent successfully.", "Failed to resend invitation");
    }

    /// <summary>
    /// Delete a user
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> DeleteUser(Guid id)
    {
        _logger.LogInformation("Deleting user: {Id}", id);

        DeleteUserCommand command = new(id);
        Result<bool> result = await _mediator.Send(command);

        return HandleVoidResult(result, "User deleted successfully", "Failed to delete user");
    }
}
