using System.Globalization;
using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Queries;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Common;
using Domain.Entities.Floorball;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using System.Collections.Concurrent;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball matches
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FloorballMatchController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballMatchController> _logger;
        private static readonly ConcurrentDictionary<string, DateTime> _eventRateLimits = new();
        private static readonly TimeSpan _rateLimitTtl = TimeSpan.FromHours(24);

        /// <summary>
        /// Initializes new instance of FloorballMatchController class
        /// </summary>
        /// <param name="mediator"></param>
        /// <param name="logger"></param>
        public FloorballMatchController(IMediator mediator, ILogger<FloorballMatchController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }


        /// <summary>
        /// Checks if the provided rate-limit window has been exceeded for a given key
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <param name="window">The window of time to check</param>
        private bool IsRateLimited(string key, TimeSpan window)
        {
            DateTime now = DateTime.UtcNow;
            if (_eventRateLimits.TryGetValue(key, out DateTime last) && now - last < window)
            {
                return true;
            }
            _eventRateLimits[key] = now;
            CleanupRateLimitCache(now);
            return false;
        }

        /// <summary>
        /// Cleans up the rate limit cache by removing entries that are older than 24 hours
        /// </summary>
        /// <param name="now">The current time</param>
        private void CleanupRateLimitCache(DateTime now)
        {
            foreach (KeyValuePair<string, DateTime> entry in _eventRateLimits)
            {
                if (now - entry.Value > _rateLimitTtl)
                {
                    _eventRateLimits.TryRemove(entry.Key, out _);
                }
            }
        }

        /// <summary>
        /// Get all floorball matches with pagination and filtering
        /// </summary>
        /// <param name="request">Query parameters for pagination and filtering</param>
        /// <returns>Paginated list of floorball matches</returns>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballMatchDto>>> GetAllMatches([FromQuery] GetFloorballMatchesRequest request)
        {
            _logger.LogInformation("Getting all floorball matches with pagination - Page: {Page}, PageSize: {PageSize}, SortOrder: {SortOrder}", request.Page, request.PageSize, request.SortOrder);

            GetAllFloorballMatchesQuery query = new GetAllFloorballMatchesQuery(
                request.Page,
                request.PageSize,
                request.SeasonId,
                request.TeamId,
                request.StartDate,
                request.EndDate,
                request.SortOrder,
                request.SearchQuery,
                request.Status
            );

            Result<PagedResult<FloorballMatchDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(PaginatedApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Floorball matches retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, PaginatedApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get a floorball match by ID
        /// </summary>
        /// <param name="id">Match ID</param>
        /// <returns></returns>
        [HttpGet("by-id/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> GetMatchById(Guid id)
        {
            _logger.LogInformation("Getting floorball match with ID of: {id}", id);

            GetFloorballMatchByIdQuery query = new GetFloorballMatchByIdQuery(id);

            Result<FloorballMatchDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Floorball match retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Floorball match not found";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get floorball matches with season ID
        /// </summary>
        /// <param name="seasonId">Season ID</param>
        /// <returns></returns>
        [HttpGet("by-seasonId/{seasonId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTeamDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballMatchDto>>>> GetMatchBySeason(Guid seasonId)
        {
            _logger.LogInformation("Getting floorball matches with season ID of: {seasonId}", seasonId);

            GetFloorballMatchesBySeasonQuery query = new GetFloorballMatchesBySeasonQuery(seasonId);

            Result<IEnumerable<FloorballMatchDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                List<FloorballMatchDto> matchList = result.Data.ToList();
                return Ok(ApiResponse<List<FloorballMatchDto>>.SuccessResponse(matchList, "Retrieved floorball matches with season ID successfully"));
            }
            string errorMessage = result.Error ?? "Failed to retrieve floorball matches";
            return BadRequest(ApiResponse<List<FloorballMatchDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get floorball matches with team ID
        /// </summary>
        /// <param name="teamId">Team ID to filter matches</param>
        /// <param name="request">Query parameters for pagination and date filtering</param>
        /// <returns></returns>
        [HttpGet("by-team/{teamId:guid}")]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballMatchDto>>> GetMatchByTeam(Guid teamId, [FromQuery] GetTeamMatchesRequest request)
        {
            _logger.LogInformation("Getting floorball match with team ID of: {teamId}", teamId);

            GetFloorballMatchesByTeamQuery query = new GetFloorballMatchesByTeamQuery(
                Page: request.Page,
                PageSize: request.PageSize,
                TeamId: teamId,
                StartDate: request.StartDate,
                EndDate: request.EndDate
                );

            Result<PagedResult<FloorballMatchDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(PaginatedApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Retrieved floorball matches with team ID successfully"));
            }
            string errorMessage = result.Error ?? "Failed to retrieve floorball matches with team ID";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<List<FloorballMatchDto>>.ErrorResponse(errorMessage));
            }
            return StatusCode(500, ApiResponse<List<FloorballMatchDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get today's floorball matches with team ID
        /// </summary>
        /// <param name="teamId">Team ID to filter matches</param>
        /// <returns></returns>
        [HttpGet("by-team/{teamId:guid}/today")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballMatchDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballMatchDto>>>> GetTodaysMatchesByTeam(Guid teamId)
        {
            _logger.LogInformation("Getting today's floorball match with team ID of: {teamId}", teamId);

            GetTodaysMatchesByTeamQuery query = new GetTodaysMatchesByTeamQuery(teamId);

            Result<IEnumerable<FloorballMatchDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                var matches = result.Data.ToList();
                return Ok(ApiResponse<List<FloorballMatchDto>>.SuccessResponse(matches, "Retrieved today's floorball matches with team ID successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve today's floorball matches with team ID";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<List<FloorballMatchDto>>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<List<FloorballMatchDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Creates new floorball match
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CreateMatch([FromBody] CreateFloorballMatchRequest request)
        {
            _logger.LogInformation("Creating floorball match {home} vs {away}", request.HomeTeamId, request.AwayTeamId);

            if (!DateTime.TryParse(request.ScheduledDateTime, out DateTime scheduledDateTime))
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse("Invalid scheduled date and time format"));

            CreateFloorballMatchCommand command = new CreateFloorballMatchCommand(
                request.SeasonId,
                request.HomeTeamId,
                request.AwayTeamId,
                request.RefereeId,
                scheduledDateTime,
                request.Venue
            );

            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetMatchById),
                    new { id = result.Data.Id },
                    ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Floorball match created successfully")
                    );
            }

            string errorMessage = result.Error ?? "Failed to create floorball match";
            List<string> errorList = result.ValidationFailures.Select(err => err.ErrorMessage).ToList();

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage, errorList));
        }

        /// <summary>
        /// Completes given match
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPut("complete-match/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CompleteMatch(Guid id)
        {
            _logger.LogInformation("Completing floorball match with ID: {id}", id);

            CompleteFloorballMatchCommand command = new CompleteFloorballMatchCommand(id);

            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if(result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Completed floorball match successfully"));
            }

            string errorMessage = result.Error ?? "Failed to complete floorball match";

            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Updates an existing floorball match
        /// </summary>
        /// <param name="request">Update match request</param>
        /// <returns>Updated match details</returns>
		[HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> UpdateMatch([FromBody] UpdateFloorballMatchRequest request)
        {
            _logger.LogInformation("Updating floorball match with ID: {id}", request.Id);

            if (!DateTimeOffset.TryParse(
                request.ScheduledDateTime,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out DateTimeOffset dto))
            {
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse("Invalid scheduled date and time format"));
            }

            DateTime scheduledUtc = dto.UtcDateTime;

            UpdateFloorballMatchCommand command = new UpdateFloorballMatchCommand(
                request.Id,
                scheduledUtc,
                request.Venue
            );

            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Floorball match updated successfully"));
            }

            string errorMessage = result.Error ?? "Failed to update floorball match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Starts a floorball match
        /// </summary>
        /// <param name="id">Match ID</param>
        /// <returns>Started match details</returns>
        [HttpPut("start-match/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> StartMatch(Guid id)
        {
            _logger.LogInformation("Starting floorball match with ID: {id}", id);

            StartFloorballMatchCommand command = new StartFloorballMatchCommand(id);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Floorball match started successfully"));
            }

            string errorMessage = result.Error ?? "Failed to start floorball match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }


        /// <summary>
        /// Records a goal in a floorball match, with 1 second rate limit
        /// </summary>
        /// <param name="request">Goal recording request</param>
        /// <returns>Updated match details</returns>
        [HttpPost("record-goal")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordGoal([FromBody] RecordGoalRequest request)
        {
            _logger.LogInformation("Recording goal for match ID: {matchId}", request.MatchId);

            string rateKey = $"{request.MatchId}:goal:{request.ScoringTeamId}:{request.ScoringPlayerId}";
            if (IsRateLimited(rateKey, TimeSpan.FromMilliseconds(50)))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<FloorballMatchDto>.ErrorResponse("Too many goal events; please wait a moment."));
            }

            RecordGoalCommand command = new RecordGoalCommand(
                request.MatchId,
                request.ScoringTeamId,
                request.ScoringPlayerId,
                request.AssistingPlayerId,
                request.SecondaryAssistingPlayerIs,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.Description,
                request.GoalType
            );

            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Goal recorded successfully"));
            }

            string errorMessage = result.Error ?? "Failed to record goal";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Records a penalty in a floorball match, with 1 second rate limit
        /// </summary>
        [HttpPost("record-penalty")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordPenalty([FromBody] RecordPenaltyEventRequest request)
        {
            _logger.LogInformation("Recording penalty for match ID: {matchId}", request.MatchId);

            string rateKey = $"{request.MatchId}:penalty:{request.TeamId}:{request.PlayerId}";
            if (IsRateLimited(rateKey, TimeSpan.FromMilliseconds(50)))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<FloorballMatchDto>.ErrorResponse("Too many penalty events; please wait a moment."));
            }

            RecordPenaltyCommand command = new RecordPenaltyCommand(
                request.MatchId,
                request.TeamId,
                request.PlayerId,
                (Domain.Enums.Floorball.FloorballPenaltyType)Enum.Parse(typeof(Domain.Enums.Floorball.FloorballPenaltyType), request.PenaltyType),
                request.DurationMinutes,
                request.PeriodNumber,
                request.TimeInSeconds,
                string.Empty);

            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Penalty recorded successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to record penalty";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Records a save in a floorball match, with 250ms rate limit
        /// </summary>
        [HttpPost("record-save")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordSave([FromBody] RecordSaveEventRequest request)
        {
            _logger.LogInformation("Recording save for match ID: {matchId}", request.MatchId);

            string rateKey = $"{request.MatchId}:save:{request.TeamId}:{request.PlayerId}";
            if (IsRateLimited(rateKey, TimeSpan.FromMilliseconds(250)))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<FloorballMatchDto>.ErrorResponse("Too many save events; please wait a moment."));
            }

            RecordSaveCommand command = new RecordSaveCommand(
                request.MatchId,
                request.TeamId,
                request.PlayerId,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.WasInOvertime,
                request.WasInShootout);

            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Save recorded successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to record save";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Starts a period in a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/period/{periodNumber:int}/start")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> StartPeriod(Guid matchId, int periodNumber)
        {
            _logger.LogInformation("Starting period {period} for match ID: {matchId}", periodNumber, matchId);

            StartPeriodCommand command = new StartPeriodCommand(matchId, periodNumber);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Period started successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to start period";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Ends a period in a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/period/{periodNumber:int}/end")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> EndPeriod(Guid matchId, int periodNumber)
        {
            _logger.LogInformation("Ending period {period} for match ID: {matchId}", periodNumber, matchId);

            EndPeriodCommand command = new EndPeriodCommand(matchId, periodNumber);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Period ended successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to end period";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deletes a goal event from a floorball match
        /// </summary>
        [HttpDelete("{matchId:guid}/goal/{goalEventId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> DeleteGoal(Guid matchId, Guid goalEventId)
        {
            _logger.LogInformation("Deleting goal {goalEventId} for match ID: {matchId}", goalEventId, matchId);

            DeleteGoalCommand command = new DeleteGoalCommand(matchId, goalEventId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Goal deleted successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to delete goal";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deletes a penalty event from a floorball match
        /// </summary>
        [HttpDelete("{matchId:guid}/penalty/{penaltyEventId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> DeletePenalty(Guid matchId, Guid penaltyEventId)
        {
            _logger.LogInformation("Deleting penalty {penaltyEventId} for match ID: {matchId}", penaltyEventId, matchId);

            DeletePenaltyCommand command = new DeletePenaltyCommand(matchId, penaltyEventId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Penalty deleted successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to delete penalty";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Deletes a save event from a floorball match
        /// </summary>
        [HttpDelete("{matchId:guid}/save/{saveEventId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> DeleteSave(Guid matchId, Guid saveEventId)
        {
            _logger.LogInformation("Deleting save {saveEventId} for match ID: {matchId}", saveEventId, matchId);

            DeleteSaveCommand command = new DeleteSaveCommand(matchId, saveEventId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Save deleted successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to delete save";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Records overtime for a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/overtime")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordOvertime(Guid matchId)
        {
            _logger.LogInformation("Recording overtime for match ID: {matchId}", matchId);

            RecordOvertimeCommand command = new RecordOvertimeCommand(matchId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Overtime recorded successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to record overtime";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Records shootout for a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/shootout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordShootout(Guid matchId)
        {
            _logger.LogInformation("Recording shootout for match ID: {matchId}", matchId);

            RecordShootoutCommand command = new RecordShootoutCommand(matchId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Shootout recorded successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to record shootout";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Postpones a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/postpone")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> Postpone(Guid matchId)
        {
            _logger.LogInformation("Postponing match ID: {matchId}", matchId);

            PostponeMatchCommand command = new PostponeMatchCommand(matchId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match postponed successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to postpone match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Cancels a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/cancel")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> Cancel(Guid matchId)
        {
            _logger.LogInformation("Canceling match ID: {matchId}", matchId);

            CancelMatchCommand command = new CancelMatchCommand(matchId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match canceled successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to cancel match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Reactivates a cancelled floorball match back to Scheduled status
        /// </summary>
        [HttpPost("{matchId:guid}/reactivate")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> Reactivate(Guid matchId)
        {
            _logger.LogInformation("Reactivating match ID: {matchId}", matchId);

            ReactivateMatchCommand command = new ReactivateMatchCommand(matchId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Match reactivated successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to reactivate match";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            if (errorMessage.Contains("Can only reactivate", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Adds an official (referee) to a floorball match (append semantics).
        /// </summary>
        [HttpPost("{matchId:guid}/officials")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> AddOfficial(Guid matchId, [FromBody] AddOfficialToMatchRequest request)
        {
            _logger.LogInformation("Adding official {refereeId} to match ID: {matchId}", request.RefereeId, matchId);

            // Fetch current match to append
            Result<FloorballMatchDto> match = await _mediator.Send(new GetFloorballMatchByIdQuery(matchId));
            if (!match.IsSuccess || match.Data == null)
            {
                string err = match.Error ?? "Match not found";
                if (err.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(err));
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(err));
            }

            List<Guid> currentOfficials = match.Data.Officials?.ToList() ?? new List<Guid>();
            if (!currentOfficials.Contains(request.RefereeId))
            {
                currentOfficials.Add(request.RefereeId);
            }

            UpdateMatchOfficialsCommand command = new UpdateMatchOfficialsCommand(matchId, currentOfficials);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Official added successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to add official";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Replaces officials for a match (requires at least one).
        /// </summary>
        [HttpPut("{matchId:guid}/officials")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> UpdateOfficials(Guid matchId, [FromBody] FloorballMatchOfficialsRequest request)
        {
            _logger.LogInformation("Updating officials for match ID: {matchId}", matchId);

            UpdateMatchOfficialsCommand command = new UpdateMatchOfficialsCommand(matchId, request.Officials ?? Array.Empty<Guid>());
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Officials updated successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to update officials";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Removes an official from a match (must leave at least one official).
        /// </summary>
        [HttpDelete("{matchId:guid}/officials/{refereeId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RemoveOfficial(Guid matchId, Guid refereeId)
        {
            _logger.LogInformation("Removing official {refereeId} from match ID: {matchId}", refereeId, matchId);

            RemoveOfficialFromMatchCommand command = new RemoveOfficialFromMatchCommand(matchId, refereeId);
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Official removed successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to remove official";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Sets a single referee for the match (PUT semantic).
        /// </summary>
        [HttpPut("{matchId:guid}/referee/{refereeId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> SetReferee(Guid matchId, Guid refereeId)
        {
            _logger.LogInformation("Setting referee {refereeId} for match ID: {matchId}", refereeId, matchId);

            UpdateMatchOfficialsCommand command = new UpdateMatchOfficialsCommand(matchId, new[] { refereeId });
            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Referee set successfully"));
            }

            string? errorMessage = result.Error ?? "Failed to set referee";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Changes the active goalie for a team in a match
        /// </summary>
        /// <param name="matchId">The ID of the match</param>
        /// <param name="teamId">The ID of the team</param>
        /// <param name="goalieId">The ID of the new goalie</param>
        /// <returns>Updated match details</returns>
        [HttpPut("{matchId:guid}/team/{teamId:guid}/goalie/{goalieId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> ChangeGoalie(Guid matchId, Guid teamId, Guid goalieId)
        {
            _logger.LogInformation("Changing goalie for match {matchId}, team {teamId} to {goalieId}", matchId, teamId, goalieId);

            ChangeGoalieCommand command = new ChangeGoalieCommand
            {
                MatchId = matchId,
                TeamId = teamId,
                GoalieId = goalieId
            };

            Result<FloorballMatchDto> result = await _mediator.Send(command);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Goalie changed successfully"));
            }
            
            string errorMessage = result.Error ?? "Failed to change goalie";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
            }

            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }
    }
}
