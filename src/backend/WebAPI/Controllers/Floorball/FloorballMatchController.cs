using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Queries;
using Application.Features.Floorball.Teams.DTOs;
using Domain.Common;
using Domain.Enums.Floorball;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Floorball;
using WebAPI.Services;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball matches
    /// </summary>
    [Route("api/[controller]")]
    public class FloorballMatchController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballMatchController> _logger;
        private readonly IMatchEventRateLimiter _rateLimiter;

        /// <summary>
        /// Initializes new instance of FloorballMatchController class
        /// </summary>
        public FloorballMatchController(
            IMediator mediator,
            ILogger<FloorballMatchController> logger,
            IMatchEventRateLimiter rateLimiter)
        {
            _mediator = mediator;
            _logger = logger;
            _rateLimiter = rateLimiter;
        }

        /// <summary>
        /// Get all floorball matches with pagination and filtering
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballMatchDto>>> GetAllMatches(
            [FromQuery] GetFloorballMatchesRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Getting all floorball matches with pagination - Page: {Page}, PageSize: {PageSize}, SortOrder: {SortOrder}",
                request.Page, request.PageSize, request.SortOrder);

            GetAllFloorballMatchesQuery query = new GetAllFloorballMatchesQuery(
                request.Page,
                request.PageSize,
                request.CompetitionId,
                request.TeamId,
                request.StartDate,
                request.EndDate,
                request.SortOrder,
                request.SearchQuery,
                request.Status,
                request.TournamentGroupId,
                request.CompetitionType
            );

            Result<PagedResult<FloorballMatchDto>> result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(PaginatedApiResponse<FloorballMatchDto>.SuccessResponse(
                    result.Data, "Floorball matches retrieved successfully"));
            }

            string errorMessage = result.Error ?? result.GetErrorsString();
            return StatusCode(500, PaginatedApiResponse<FloorballMatchDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get a floorball match by ID
        /// </summary>
        [HttpGet("by-id/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> GetMatchById(
            Guid id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting floorball match with ID of: {id}", id);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new GetFloorballMatchByIdQuery(id), cancellationToken);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballMatchDto>.SuccessResponse(
                    result.Data, "Floorball match retrieved successfully"));
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
        [HttpGet("by-competitionId/{competitionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTeamDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballMatchDto>>>> GetMatchBySeason(
            Guid competitionId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting floorball matches with season ID of: {competitionId}", competitionId);

            Result<IEnumerable<FloorballMatchDto>> result = await _mediator.Send(
                new GetFloorballMatchesBySeasonQuery(competitionId), cancellationToken);

            if (result.IsSuccess && result.Data != null)
            {
                List<FloorballMatchDto> matchList = result.Data.ToList();
                return Ok(ApiResponse<List<FloorballMatchDto>>.SuccessResponse(
                    matchList, "Retrieved floorball matches with season ID successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve floorball matches";
            return BadRequest(ApiResponse<List<FloorballMatchDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Get floorball matches with team ID
        /// </summary>
        [HttpGet("by-team/{teamId:guid}")]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballMatchDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballMatchDto>>> GetMatchByTeam(
            Guid teamId,
            [FromQuery] GetTeamMatchesRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting floorball match with team ID of: {teamId}", teamId);

            GetFloorballMatchesByTeamQuery query = new GetFloorballMatchesByTeamQuery(
                Page: request.Page,
                PageSize: request.PageSize,
                TeamId: teamId,
                StartDate: request.StartDate,
                EndDate: request.EndDate
            );

            Result<PagedResult<FloorballMatchDto>> result = await _mediator.Send(query, cancellationToken);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(PaginatedApiResponse<FloorballMatchDto>.SuccessResponse(
                    result.Data, "Retrieved floorball matches with team ID successfully"));
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
        [HttpGet("by-team/{teamId:guid}/today")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballMatchDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballMatchDto>>>> GetTodaysMatchesByTeam(
            Guid teamId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting today's floorball match with team ID of: {teamId}", teamId);

            Result<IEnumerable<FloorballMatchDto>> result = await _mediator.Send(
                new GetTodaysMatchesByTeamQuery(teamId), cancellationToken);

            if (result.IsSuccess && result.Data != null)
            {
                List<FloorballMatchDto> matches = result.Data.ToList();
                return Ok(ApiResponse<List<FloorballMatchDto>>.SuccessResponse(
                    matches, "Retrieved today's floorball matches with team ID successfully"));
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
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CreateMatch(
            [FromBody] CreateFloorballMatchRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Creating floorball match {home} vs {away}", request.HomeTeamId, request.AwayTeamId);

            if (!DateTime.TryParse(request.ScheduledDateTime, out DateTime scheduledDateTime))
            {
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse("Invalid scheduled date and time format"));
            }

            FloorballTournamentStage? stage = null;
            if (!string.IsNullOrWhiteSpace(request.TournamentStage))
            {
                if (!Enum.TryParse(request.TournamentStage, true, out FloorballTournamentStage parsedStage))
                {
                    return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(
                        $"Invalid tournament stage '{request.TournamentStage}'. Valid values: {string.Join(", ", Enum.GetNames<FloorballTournamentStage>())}"));
                }
                stage = parsedStage;
            }

            CreateFloorballMatchCommand command = new CreateFloorballMatchCommand(
                request.CompetitionId,
                request.HomeTeamId,
                request.AwayTeamId,
                request.RefereeId,
                scheduledDateTime,
                request.Venue,
                request.TournamentGroupId,
                stage
            );

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            if (result.IsSuccess && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetMatchById),
                    new { id = result.Data.Id },
                    ApiResponse<FloorballMatchDto>.SuccessResponse(result.Data, "Floorball match created successfully")
                );
            }

            return ToErrorResponse(result, "Failed to create floorball match");
        }

        /// <summary>
        /// Completes given match
        /// </summary>
        [HttpPut("complete-match/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CompleteMatch(
            Guid id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Completing floorball match with ID: {id}", id);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new CompleteFloorballMatchCommand(id), cancellationToken);

            return HandleResult(result, "Completed floorball match successfully", "Failed to complete floorball match");
        }

        /// <summary>
        /// Reopens a previously completed floorball match back to InProgress so the operator can
        /// correct mistakes or continue recording events. Per-match aggregates that were applied
        /// at completion time (team / player / goalie season stats) are reverted in the handler.
        /// Playoff matches are rejected because bracket propagation rollback is not supported.
        /// </summary>
        [HttpPut("reopen-match/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> ReopenMatch(
            Guid id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reopening floorball match with ID: {id}", id);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new ReopenFloorballMatchCommand(id), cancellationToken);

            return HandleResult(result, "Reopened floorball match successfully", "Failed to reopen floorball match");
        }

        /// <summary>
        /// Updates an existing floorball match
        /// </summary>
        [HttpPut]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> UpdateMatch(
            [FromBody] UpdateFloorballMatchRequest request,
            CancellationToken cancellationToken)
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

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            return HandleResult(result, "Floorball match updated successfully", "Failed to update floorball match");
        }

        /// <summary>
        /// Starts a floorball match
        /// </summary>
        [HttpPut("start-match/{id:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> StartMatch(
            Guid id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting floorball match with ID: {id}", id);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new StartFloorballMatchCommand(id), cancellationToken);

            // ToErrorResponse already surfaces validation failures and the domain error message
            // in the `errors` list so the frontend can show a single clear banner (e.g.
            // "Ottelua ei voi aloittaa: molempien joukkueiden tulee olla valittuina.") without
            // having to parse `message`.
            return HandleResult(result, "Floorball match started successfully", "Failed to start floorball match");
        }

        /// <summary>
        /// Assigns (or clears) the home and away team slots on a scheduled or postponed match.
        /// </summary>
        /// <remarks>
        /// Pass <c>null</c> for either side to leave the slot as "to be determined". When the
        /// match is a playoff bracket match, the change is automatically propagated forward into
        /// the next bracket slot (provided that next match hasn't started yet). The endpoint
        /// rejects any attempt to change teams on a match that is already InProgress or Completed.
        /// </remarks>
        [HttpPut("{matchId:guid}/teams")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> AssignTeams(
            Guid matchId,
            [FromBody] AssignMatchTeamsRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Assigning teams on floorball match {MatchId}: home={HomeTeamId}, away={AwayTeamId}",
                matchId, request.HomeTeamId, request.AwayTeamId);

            AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(
                matchId,
                request.HomeTeamId,
                request.AwayTeamId);

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            return HandleResult(result, "Match teams updated successfully", "Failed to update match teams");
        }

        /// <summary>
        /// Records a goal in a floorball match. Subject to a per-(match, scoring player) rate
        /// limit (see <see cref="MatchEventRateLimits.GoalWindow"/>) to swallow accidental
        /// double-clicks from the live match management UI.
        /// </summary>
        [HttpPost("record-goal")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordGoal(
            [FromBody] RecordGoalRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recording goal for match ID: {matchId}", request.MatchId);

            string rateKey = $"{request.MatchId}:goal:{request.ScoringTeamId}:{request.ScoringPlayerId}";
            if (_rateLimiter.IsRateLimited(rateKey, MatchEventRateLimits.GoalWindow))
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

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            return HandleResult(result, "Goal recorded successfully", "Failed to record goal");
        }

        /// <summary>
        /// Records a penalty in a floorball match. Subject to the same kind of per-event rate
        /// limit as <see cref="RecordGoal"/>.
        /// </summary>
        [HttpPost("record-penalty")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordPenalty(
            [FromBody] RecordPenaltyEventRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recording penalty for match ID: {matchId}", request.MatchId);

            // Validate enum input up front so a bad value yields a clean 400 instead of a 500
            // from a thrown ArgumentException inside the controller body.
            if (!Enum.TryParse(request.PenaltyType, ignoreCase: true, out FloorballPenaltyType penaltyType))
            {
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(
                    $"Invalid penalty type '{request.PenaltyType}'. Valid values: {string.Join(", ", Enum.GetNames<FloorballPenaltyType>())}"));
            }

            string rateKey = $"{request.MatchId}:penalty:{request.TeamId}:{request.PlayerId}";
            if (_rateLimiter.IsRateLimited(rateKey, MatchEventRateLimits.PenaltyWindow))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<FloorballMatchDto>.ErrorResponse("Too many penalty events; please wait a moment."));
            }

            RecordPenaltyCommand command = new RecordPenaltyCommand(
                request.MatchId,
                request.TeamId,
                request.PlayerId,
                penaltyType,
                request.DurationMinutes,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.Description ?? string.Empty);

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            return HandleResult(result, "Penalty recorded successfully", "Failed to record penalty");
        }

        /// <summary>
        /// Records one or more saves in a floorball match. The single-save flow is rate-limited
        /// (see <see cref="MatchEventRateLimits.SaveWindow"/>); bulk backfills (count &gt; 1)
        /// bypass the limiter because they are an explicit operator action via the bulk dialog.
        /// </summary>
        [HttpPost("record-save")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordSave(
            [FromBody] RecordSaveEventRequest request,
            CancellationToken cancellationToken)
        {
            int saveCount = request.Count < 1 ? 1 : request.Count;
            _logger.LogInformation(
                "Recording {Count} save(s) for match ID: {matchId}", saveCount, request.MatchId);

            if (saveCount == 1)
            {
                string rateKey = $"{request.MatchId}:save:{request.TeamId}:{request.PlayerId}";
                if (_rateLimiter.IsRateLimited(rateKey, MatchEventRateLimits.SaveWindow))
                {
                    return StatusCode(StatusCodes.Status429TooManyRequests,
                        ApiResponse<FloorballMatchDto>.ErrorResponse("Too many save events; please wait a moment."));
                }
            }

            RecordSaveCommand command = new RecordSaveCommand(
                request.MatchId,
                request.TeamId,
                request.PlayerId,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.WasInOvertime,
                request.WasInShootout,
                saveCount);

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            return HandleResult(result, "Save recorded successfully", "Failed to record save");
        }

        /// <summary>
        /// Starts a period in a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/period/{periodNumber:int}/start")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> StartPeriod(
            Guid matchId,
            int periodNumber,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting period {period} for match ID: {matchId}", periodNumber, matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new StartPeriodCommand(matchId, periodNumber), cancellationToken);

            return HandleResult(result, "Period started successfully", "Failed to start period");
        }

        /// <summary>
        /// Ends a period in a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/period/{periodNumber:int}/end")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> EndPeriod(
            Guid matchId,
            int periodNumber,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Ending period {period} for match ID: {matchId}", periodNumber, matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new EndPeriodCommand(matchId, periodNumber), cancellationToken);

            return HandleResult(result, "Period ended successfully", "Failed to end period");
        }

        /// <summary>
        /// Deletes a goal event from a floorball match
        /// </summary>
        [HttpDelete("{matchId:guid}/goal/{goalEventId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> DeleteGoal(
            Guid matchId,
            Guid goalEventId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting goal {goalEventId} for match ID: {matchId}", goalEventId, matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new DeleteGoalCommand(matchId, goalEventId), cancellationToken);

            return HandleResult(result, "Goal deleted successfully", "Failed to delete goal");
        }

        /// <summary>
        /// Deletes a penalty event from a floorball match
        /// </summary>
        [HttpDelete("{matchId:guid}/penalty/{penaltyEventId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> DeletePenalty(
            Guid matchId,
            Guid penaltyEventId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting penalty {penaltyEventId} for match ID: {matchId}", penaltyEventId, matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new DeletePenaltyCommand(matchId, penaltyEventId), cancellationToken);

            return HandleResult(result, "Penalty deleted successfully", "Failed to delete penalty");
        }

        /// <summary>
        /// Deletes a save event from a floorball match
        /// </summary>
        [HttpDelete("{matchId:guid}/save/{saveEventId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> DeleteSave(
            Guid matchId,
            Guid saveEventId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting save {saveEventId} for match ID: {matchId}", saveEventId, matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new DeleteSaveCommand(matchId, saveEventId), cancellationToken);

            return HandleResult(result, "Save deleted successfully", "Failed to delete save");
        }

        /// <summary>
        /// Records overtime for a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/overtime")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordOvertime(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recording overtime for match ID: {matchId}", matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new RecordOvertimeCommand(matchId), cancellationToken);

            return HandleResult(result, "Overtime recorded successfully", "Failed to record overtime");
        }

        /// <summary>
        /// Records shootout for a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/shootout")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordShootout(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recording shootout for match ID: {matchId}", matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new RecordShootoutCommand(matchId), cancellationToken);

            return HandleResult(result, "Shootout recorded successfully", "Failed to record shootout");
        }

        /// <summary>
        /// Postpones a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/postpone")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> Postpone(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Postponing match ID: {matchId}", matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new PostponeMatchCommand(matchId), cancellationToken);

            return HandleResult(result, "Match postponed successfully", "Failed to postpone match");
        }

        /// <summary>
        /// Cancels a floorball match
        /// </summary>
        [HttpPost("{matchId:guid}/cancel")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> Cancel(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Canceling match ID: {matchId}", matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new CancelMatchCommand(matchId), cancellationToken);

            return HandleResult(result, "Match canceled successfully", "Failed to cancel match");
        }

        /// <summary>
        /// Permanently deletes a floorball match. Only allowed for matches still in the
        /// <see cref="Domain.Enums.Floorball.FloorballMatchStatus.Scheduled"/> state — matches
        /// that have started, finished, or been cancelled cannot be deleted because they
        /// carry recorded events and statistics. Used by the tournament JSON import revert flow.
        /// </summary>
        [HttpDelete("{matchId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteMatch(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting match ID: {matchId}", matchId);

            Result result = await _mediator.Send(
                new DeleteFloorballMatchCommand(matchId), cancellationToken);

            if (result.IsSuccess)
            {
                return Ok(ApiResponse.SuccessResponse("Match deleted successfully"));
            }

            return ToErrorResponse(result, "Failed to delete match");
        }

        /// <summary>
        /// Reactivates a cancelled floorball match back to Scheduled status
        /// </summary>
        [HttpPost("{matchId:guid}/reactivate")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> Reactivate(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reactivating match ID: {matchId}", matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new ReactivateMatchCommand(matchId), cancellationToken);

            return HandleResult(result, "Match reactivated successfully", "Failed to reactivate match");
        }

        /// <summary>
        /// Adds an official (referee) to a floorball match (append semantics).
        /// </summary>
        [HttpPost("{matchId:guid}/officials")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> AddOfficial(
            Guid matchId,
            [FromBody] AddOfficialToMatchRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding official {refereeId} to match ID: {matchId}", request.RefereeId, matchId);

            // Fetch current match to compute the appended officials list. NOTE: ideally this
            // should live behind a single AddOfficialToMatchCommand handler so the controller
            // doesn't issue two mediator calls — that's planned for Phase 2.
            Result<FloorballMatchDto> match = await _mediator.Send(
                new GetFloorballMatchByIdQuery(matchId), cancellationToken);
            if (!match.IsSuccess || match.Data == null)
            {
                return ToErrorResponse(match, "Match not found");
            }

            List<Guid> currentOfficials = match.Data.Officials?.ToList() ?? new List<Guid>();
            if (!currentOfficials.Contains(request.RefereeId))
            {
                currentOfficials.Add(request.RefereeId);
            }

            Result<FloorballMatchDto> result = await _mediator.Send(
                new UpdateMatchOfficialsCommand(matchId, currentOfficials), cancellationToken);

            return HandleResult(result, "Official added successfully", "Failed to add official");
        }

        /// <summary>
        /// Replaces officials for a match (requires at least one).
        /// </summary>
        [HttpPut("{matchId:guid}/officials")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> UpdateOfficials(
            Guid matchId,
            [FromBody] FloorballMatchOfficialsRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating officials for match ID: {matchId}", matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new UpdateMatchOfficialsCommand(matchId, request.Officials ?? Array.Empty<Guid>()),
                cancellationToken);

            return HandleResult(result, "Officials updated successfully", "Failed to update officials");
        }

        /// <summary>
        /// Removes an official from a match (must leave at least one official).
        /// </summary>
        [HttpDelete("{matchId:guid}/officials/{refereeId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RemoveOfficial(
            Guid matchId,
            Guid refereeId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Removing official {refereeId} from match ID: {matchId}", refereeId, matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new RemoveOfficialFromMatchCommand(matchId, refereeId), cancellationToken);

            return HandleResult(result, "Official removed successfully", "Failed to remove official");
        }

        /// <summary>
        /// Sets a single referee for the match (PUT semantic).
        /// </summary>
        [HttpPut("{matchId:guid}/referee/{refereeId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> SetReferee(
            Guid matchId,
            Guid refereeId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Setting referee {refereeId} for match ID: {matchId}", refereeId, matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new UpdateMatchOfficialsCommand(matchId, new[] { refereeId }), cancellationToken);

            return HandleResult(result, "Referee set successfully", "Failed to set referee");
        }

        /// <summary>
        /// Changes the active goalie for a team in a match
        /// </summary>
        [HttpPut("{matchId:guid}/team/{teamId:guid}/goalie/{goalieId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> ChangeGoalie(
            Guid matchId,
            Guid teamId,
            Guid goalieId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Changing goalie for match {matchId}, team {teamId} to {goalieId}", matchId, teamId, goalieId);

            ChangeGoalieCommand command = new ChangeGoalieCommand
            {
                MatchId = matchId,
                TeamId = teamId,
                GoalieId = goalieId
            };

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            return HandleResult(result, "Goalie changed successfully", "Failed to change goalie");
        }

        /// <summary>
        /// Replaces the active field player lineup (and optionally the active goalie) for a single
        /// team in a match. Used by the match-management UI's "Edit lineup" dialog.
        /// </summary>
        [HttpPut("{matchId:guid}/team/{teamId:guid}/active-roster")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> SetActiveRoster(
            Guid matchId,
            Guid teamId,
            [FromBody] SetMatchActiveRosterRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse("Request body is required."));
            }

            string? sanitizedGoalieIdForLog = request.GoalieId?.ToString()
                .Replace("\r", string.Empty, StringComparison.Ordinal)
                .Replace("\n", string.Empty, StringComparison.Ordinal);

            _logger.LogInformation(
                "Updating active roster for match {matchId}, team {teamId} ({playerCount} players, goalie={goalieId})",
                matchId, teamId, request.Players?.Count ?? 0, sanitizedGoalieIdForLog);

            SetMatchActiveRosterCommand command = new SetMatchActiveRosterCommand
            {
                MatchId = matchId,
                TeamId = teamId,
                Players = request.Players?.Select(p => new ActivePlayerInput
                {
                    PlayerId = p.PlayerId,
                    Position = p.Position,
                }).ToList() ?? new List<ActivePlayerInput>(),
                GoalieId = request.GoalieId,
            };

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            return HandleResult(result, "Active roster updated successfully", "Failed to update active roster");
        }
    }
}
