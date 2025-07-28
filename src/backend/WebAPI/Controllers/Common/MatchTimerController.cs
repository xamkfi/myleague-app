using System;
using System.Threading.Tasks;
using Application.Services.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Common
{
    /// <summary>
    /// Controller for managing match timers with RESTful routes
    /// </summary>
    [ApiController]
    [Route("api/matches/{matchId:guid}/timer")]
    [Produces("application/json")]
    public class MatchTimerController : ControllerBase
    {
        private readonly IMatchTimerService _timerService;
        private readonly ILogger<MatchTimerController> _logger;

        /// <summary>
        /// Initializes a new instance of the MatchTimerController class
        /// </summary>
        /// <param name="timerService">The timer service</param>
        /// <param name="logger">The logger</param>
        public MatchTimerController(
            IMatchTimerService timerService,
            ILogger<MatchTimerController> logger)
        {
            _timerService = timerService;
            _logger = logger;
        }

        /// <summary>
        /// Starts the timer for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <param name="periodNumber">Optional period number</param>
        /// <returns>Success response</returns>
        [HttpPost("start")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StartTimer(Guid matchId, [FromQuery] int? periodNumber = null)
        {
            try
            {
                _logger.LogInformation("Request to start timer for match {MatchId} with period {PeriodNumber}", matchId, periodNumber);
                
                await _timerService.StartTimerAsync(matchId, periodNumber);
                
                return Ok(ApiResponse.SuccessResponse("Timer started successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting timer for match {MatchId}", matchId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to start timer"));
            }
        }

        /// <summary>
        /// Stops the timer for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>Success response</returns>
        [HttpPost("stop")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StopTimer(Guid matchId)
        {
            try
            {
                _logger.LogInformation("Request to stop timer for match {MatchId}", matchId);
                
                await _timerService.StopTimerAsync(matchId);
                
                return Ok(ApiResponse.SuccessResponse("Timer stopped successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping timer for match {MatchId}", matchId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to stop timer"));
            }
        }

        /// <summary>
        /// Resets the timer for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>Success response</returns>
        [HttpPost("reset")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResetTimer(Guid matchId)
        {
            try
            {
                _logger.LogInformation("Request to reset timer for match {MatchId}", matchId);
                
                await _timerService.ResetTimerAsync(matchId);
                
                return Ok(ApiResponse.SuccessResponse("Timer reset successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting timer for match {MatchId}", matchId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to reset timer"));
            }
        }

        /// <summary>
        /// Gets the elapsed time for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>The elapsed time</returns>
        [HttpGet("elapsed")]
        [ProducesResponseType(typeof(ApiResponse<TimeSpan>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetElapsedTime(Guid matchId)
        {
            try
            {
                _logger.LogDebug("Request to get elapsed time for match {MatchId}", matchId);
                
                TimeSpan elapsedTime = await _timerService.GetElapsedTimeAsync(matchId);
                
                return Ok(ApiResponse<TimeSpan>.SuccessResponse(elapsedTime, "Elapsed time retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting elapsed time for match {MatchId}", matchId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to get elapsed time"));
            }
        }

        /// <summary>
        /// Gets the timer status for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>The timer status</returns>
        [HttpGet("status")]
        [ProducesResponseType(typeof(ApiResponse<TimerStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTimerStatus(Guid matchId)
        {
            try
            {
                _logger.LogDebug("Request to get timer status for match {MatchId}", matchId);
                
                bool exists = await _timerService.ExistsAsync(matchId);
                if (!exists)
                {
                    return Ok(ApiResponse<TimerStatusResponse>.SuccessResponse(
                        new TimerStatusResponse { Exists = false }, 
                        "Timer does not exist"));
                }
                
                bool isRunning = await _timerService.IsRunningAsync(matchId);
                TimeSpan elapsedTime = await _timerService.GetElapsedTimeAsync(matchId);
                
                TimerStatusResponse status = new TimerStatusResponse
                {
                    Exists = true,
                    IsRunning = isRunning,
                    ElapsedTime = elapsedTime
                };
                
                return Ok(ApiResponse<TimerStatusResponse>.SuccessResponse(status, "Timer status retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timer status for match {MatchId}", matchId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to get timer status"));
            }
        }

        /// <summary>
        /// Creates a timer for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>Success response</returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateTimer(Guid matchId)
        {
            try
            {
                _logger.LogInformation("Request to create timer for match {MatchId}", matchId);
                
                await _timerService.CreateTimerAsync(matchId);
                
                return Ok(ApiResponse.SuccessResponse("Timer created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating timer for match {MatchId}", matchId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to create timer"));
            }
        }

        /// <summary>
        /// Destroys the timer for a match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>Success response</returns>
        [HttpDelete("destroy")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DestroyTimer(Guid matchId)
        {
            try
            {
                _logger.LogInformation("Request to destroy timer for match {MatchId}", matchId);
                
                await _timerService.DestroyTimerAsync(matchId);
                
                return Ok(ApiResponse.SuccessResponse("Timer destroyed successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error destroying timer for match {MatchId}", matchId);
                return StatusCode(500, ApiResponse.ErrorResponse("Failed to destroy timer"));
            }
        }
    }

    /// <summary>
    /// Response model for timer status
    /// </summary>
    public class TimerStatusResponse
    {
        /// <summary>
        /// Whether the timer exists
        /// </summary>
        public bool Exists { get; set; }

        /// <summary>
        /// Whether the timer is running
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// The elapsed time
        /// </summary>
        public TimeSpan ElapsedTime { get; set; }
    }
} 