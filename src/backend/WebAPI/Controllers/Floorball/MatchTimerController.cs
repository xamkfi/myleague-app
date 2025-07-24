using Application.Commands.Floorball.Match;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Services.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing match timers
    /// </summary>
    [ApiController]
    [Route("api/floorball/match-timer")]
    [Produces("application/json")]
    public class MatchTimerController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MatchTimerController> _logger;
        private readonly IMatchClockManager _clockManager;

        public MatchTimerController(IMediator mediator, ILogger<MatchTimerController> logger, IMatchClockManager clockManager)
        {
            _mediator = mediator;
            _logger = logger;
            _clockManager = clockManager;
        }

        /// <summary>
        /// Starts the timer for a match
        /// </summary>
        [HttpPost("start/{matchId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamManagerDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StartTimer(Guid matchId)
        {
            _logger.LogInformation("Request to start timer for match {MatchId}", matchId);
            Result result = await _mediator.Send(new StartMatchTimerCommand(matchId));
            if (result.IsSuccess)
                return Ok(ApiResponse.SuccessResponse("Timer started successfully"));
            return BadRequest(ApiResponse.ErrorResponse(result.Error ?? "Failed to start timer"));
        }

        /// <summary>
        /// Stops the timer for a match
        /// </summary>
        [HttpPost("stop/{matchId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamManagerDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> StopTimer(Guid matchId)
        {
            _logger.LogInformation("Request to stop timer for match {MatchId}", matchId);
            Result result = await _mediator.Send(new StopMatchTimerCommand(matchId));
            if (result.IsSuccess)
                return Ok(ApiResponse.SuccessResponse("Timer stopped successfully"));
            return BadRequest(ApiResponse.ErrorResponse(result.Error ?? "Failed to stop timer"));
        }

        /// <summary>
        /// Gets the current elapsed time for a match timer
        /// </summary>
        [HttpGet("elapsed/{matchId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamManagerDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetElapsedTime(Guid matchId)
        {
            Result<TimeSpan> result = await _mediator.Send(new Application.Queries.Floorball.Match.GetMatchElapsedTimeQuery(matchId));
            if (!result.IsSuccess)
            {
                return NotFound(ApiResponse.ErrorResponse(result.Error ?? $"Timer does not exist for match {matchId}"));
            }
            return Ok(ApiResponse<string>.SuccessResponse(result.Data.ToString(), "Elapsed time retrieved successfully"));
        }
    }
} 
