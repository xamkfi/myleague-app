using Application.Commands.Users;
using Application.Common;
using Application.DTOs.Common;
using Application.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common
{
    /// <summary>
    /// Controller for managing users
    /// </summary>
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

            GetAllUsersQuery query = new GetAllUsersQuery();
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

            GetUserByIdQuery query = new GetUserByIdQuery(id);
            Result<UserDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                UserDto user = result.Data;
                return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            
            // Check if it's a not found error
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<UserDto>.ErrorResponse(errorMessage));
            }
            
            return StatusCode(500, ApiResponse<UserDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="username">The username to search for</param>
        /// <returns>The user</returns>
        [HttpGet("by-username")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetUserByUsername([FromQuery] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Username parameter is required"));
            }

            _logger.LogInformation("Getting user by username: {Username}", username);

            GetUserByUsernameQuery query = new GetUserByUsernameQuery(username);
            Result<UserDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                UserDto user = result.Data;
                return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            
            // Check if it's a not found error
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
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserRequest request)
        {
            _logger.LogInformation("Creating new user: {Username}", request.Username);

            CreateUserCommand command = new CreateUserCommand(
                request.Username,
                request.Password);

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
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Update an existing user
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <param name="request">The user update request</param>
        /// <returns>The updated user</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
        {
            _logger.LogInformation("Updating user: {Id}", id);

            UpdateUserCommand command = new UpdateUserCommand(
                id,
                request.Username,
                request.Password);

            Result<UserDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<UserDto>.SuccessResponse(result.Data, "User updated successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            
            // Check if it's a not found error
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase) || 
                errorMessage.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<UserDto>.ErrorResponse(errorMessage));
            }
            
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Update user password
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <param name="request">The password update request</param>
        /// <returns>Success status</returns>
        [HttpPatch("{id:guid}/password")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> UpdateUserPassword(Guid id, [FromBody] UpdateUserPasswordRequest request)
        {
            _logger.LogInformation("Updating password for user: {Id}", id);

            // Check if user is updating their own password or if they're an admin
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (currentUserId != id.ToString() && userRole != "Admin" && userRole != "SuperAdmin")
            {
                _logger.LogWarning("User {CurrentUserId} attempted to update password for user {TargetUserId} without permission", currentUserId, id);
                return Forbid();
            }

            UpdateUserPasswordCommand command = new UpdateUserPasswordCommand(
                id,
                request.CurrentPassword,
                request.NewPassword);

            Result<bool> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("Password updated successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            
            // Check if it's a not found error
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase) || 
                errorMessage.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }
            
            return BadRequest(ApiResponse.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Delete a user
        /// </summary>
        /// <param name="id">The user ID</param>
        /// <returns>Success status</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SuperAdmin")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteUser(Guid id)
        {
            _logger.LogInformation("Deleting user: {Id}", id);

            DeleteUserCommand command = new DeleteUserCommand(id);
            Result<bool> result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("User deleted successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            
            // Check if it's a not found error
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase) || 
                errorMessage.Contains("does not exist", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse.ErrorResponse(errorMessage));
            }
            
            return StatusCode(500, ApiResponse.ErrorResponse(errorMessage));
        }
    }
} 
