using Application.Commands.Users;
using Application.Common;
using Application.DTOs.Common;
using Application.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Controller for managing users
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
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

        if (result.IsSuccess && result.Data != null)
        {
            List<UserDto> userList = result.Data.ToList();
            return Ok(ApiResponse<List<UserDto>>.SuccessResponse(userList, "Users retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        return StatusCode(500, ApiResponse<List<UserDto>>.ErrorResponse(errorMessage));
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

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<UserDto>.SuccessResponse(result.Data, "User retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<UserDto>.ErrorResponse(errorMessage));
        }

        return StatusCode(500, ApiResponse<UserDto>.ErrorResponse(errorMessage));
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

        _logger.LogInformation("Getting user by email: {Email}", email);

        GetUserByEmailQuery query = new(email);
        Result<UserDto> result = await _mediator.Send(query);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<UserDto>.SuccessResponse(result.Data, "User retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<UserDto>.ErrorResponse(errorMessage));
        }

        return StatusCode(500, ApiResponse<UserDto>.ErrorResponse(errorMessage));
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

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<UserDto>.SuccessResponse(result.Data, "User retrieved successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<UserDto>.ErrorResponse(errorMessage));
        }

        return StatusCode(500, ApiResponse<UserDto>.ErrorResponse(errorMessage));
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
        _logger.LogInformation("Creating new user: {Email}", request.Email);

        CreateUserCommand command = new(request.Email, request.PersonId);
        Result<UserDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetUserById),
                new { id = result.Data.Id },
                ApiResponse<UserDto>.SuccessResponse(result.Data, "User created successfully")
            );
        }

        string errorMessage = result.Error ?? result.GetErrorsString();
        List<string> errorList = result.ValidationFailures.Select(x => x.ErrorMessage).ToList();

        return BadRequest(ApiResponse<UserDto>.ErrorResponse(errorMessage, errorList));
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

        UpdateUserCommand command = new(id, request.Email);
        Result<UserDto> result = await _mediator.Send(command);

        if (result.IsSuccess && result.Data != null)
        {
            return Ok(ApiResponse<UserDto>.SuccessResponse(result.Data, "User updated successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse<UserDto>.ErrorResponse(errorMessage));
        }

        return BadRequest(ApiResponse<UserDto>.ErrorResponse(errorMessage));
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

        if (result.IsSuccess)
        {
            return Ok(ApiResponse.SuccessResponse("User deleted successfully"));
        }

        string errorMessage = result.Error ?? result.GetErrorsString();

        if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase) ||
            errorMessage.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(ApiResponse.ErrorResponse(errorMessage));
        }

        return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
    }
}
