using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Models.Auth;
using WebAPI.Services;
using BCrypt.Net;
using Domain.Enums.Common;
using MyLeague.Infrastructure.Persistence.Contexts;

namespace WebAPI.Controllers.Auth
{
    /// <summary>
    /// Controller for user authentication operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly CommonDbContext _context;
        private readonly TokenService _tokenService;
        private readonly ILogger<UserController> _logger;

        /// <summary>
        /// Contoller for user
        /// </summary>
        /// <param name="context"></param>
        /// <param name="tokenService"></param>
        /// <param name="logger"></param>
        public UserController(
            CommonDbContext context,
            TokenService tokenService,
            ILogger<UserController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="loginRequest">Login credentials</param>
        /// <returns>JWT token and user information</returns>
        /// <response code="200">Login successful</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="401">Invalid credentials</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid login request received");
                    return BadRequest(ModelState);
                }

                // Find user by username
                Domain.Entities.Common.User? user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == loginRequest.Username);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt with non-existent username: {Username}", loginRequest.Username);
                    return Unauthorized("Invalid username or password");
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Failed login attempt for user: {Username}", loginRequest.Username);
                    return Unauthorized("Invalid username or password");
                }

                // Generate JWT token
                string token = _tokenService.GenerateToken(user);
                DateTime expiresAt = DateTime.UtcNow.AddMinutes(30); // Should match token expiry

                _logger.LogInformation("Successful login for user: {Username}", user.Username);

                var response = new LoginResponse
                {
                    Token = token,
                    ExpiresAt = expiresAt,
                    UserId = user.Id,
                    Username = user.Username,
                    Role = user.Role
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login for username: {Username}", loginRequest.Username);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}

