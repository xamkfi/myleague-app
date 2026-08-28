using Domain.Constants;
using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Domain.Enums.Floorball;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;
using WebAPI.Services;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Endpoints for recording, deleting and progressing in-match events (goals, penalties,
    /// saves, overtime, shootout, period start / end). The matchId comes from the URL so the
    /// request body's optional <c>MatchId</c> is ignored when present.
    /// </summary>
    [Route("api/floorball-matches/{matchId:guid}/events")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    public class FloorballMatchEventsController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballMatchEventsController> _logger;
        private readonly IMatchEventRateLimiter _rateLimiter;

        /// <summary>
        /// Creates a new <see cref="FloorballMatchEventsController"/>.
        /// </summary>
        public FloorballMatchEventsController(
            IMediator mediator,
            ILogger<FloorballMatchEventsController> logger,
            IMatchEventRateLimiter rateLimiter)
        {
            _mediator = mediator;
            _logger = logger;
            _rateLimiter = rateLimiter;
        }

        /// <summary>
        /// Records a goal in a floorball match. Subject to a per-(match, scoring player) rate
        /// limit (see <see cref="MatchEventRateLimits.GoalWindow"/>) to swallow accidental
        /// double-clicks from the live match management UI.
        /// </summary>
        [HttpPost("goal")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordGoal(
            Guid matchId,
            [FromBody] RecordGoalRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recording goal for match ID: {matchId}", matchId);

            string rateKey = $"{matchId}:goal:{request.ScoringTeamId}:{request.ScoringPlayerId}";
            if (!request.SkipRateLimit && _rateLimiter.IsRateLimited(rateKey, MatchEventRateLimits.GoalWindow))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<FloorballMatchDto>.ErrorResponse("Too many goal events; please wait a moment."));
            }

            RecordGoalCommand command = new RecordGoalCommand(
                matchId,
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
        [HttpPost("penalty")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordPenalty(
            Guid matchId,
            [FromBody] RecordPenaltyEventRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Recording penalty for match ID: {matchId}", matchId);

            if (!Enum.TryParse(request.PenaltyType, ignoreCase: true, out FloorballPenaltyType penaltyType))
            {
                return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(
                    $"Invalid penalty type '{request.PenaltyType}'. Valid values: {string.Join(", ", Enum.GetNames<FloorballPenaltyType>())}"));
            }

            string rateKey = $"{matchId}:penalty:{request.TeamId}:{request.PlayerId}";
            if (!request.SkipRateLimit && _rateLimiter.IsRateLimited(rateKey, MatchEventRateLimits.PenaltyWindow))
            {
                return StatusCode(StatusCodes.Status429TooManyRequests,
                    ApiResponse<FloorballMatchDto>.ErrorResponse("Too many penalty events; please wait a moment."));
            }

            RecordPenaltyCommand command = new RecordPenaltyCommand(
                matchId,
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
        [HttpPost("save")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status429TooManyRequests)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RecordSave(
            Guid matchId,
            [FromBody] RecordSaveEventRequest request,
            CancellationToken cancellationToken)
        {
            int saveCount = request.Count < 1 ? 1 : request.Count;
            _logger.LogInformation(
                "Recording {Count} save(s) for match ID: {matchId}", saveCount, matchId);

            if (saveCount == 1)
            {
                string rateKey = $"{matchId}:save:{request.TeamId}:{request.PlayerId}";
                if (_rateLimiter.IsRateLimited(rateKey, MatchEventRateLimits.SaveWindow))
                {
                    return StatusCode(StatusCodes.Status429TooManyRequests,
                        ApiResponse<FloorballMatchDto>.ErrorResponse("Too many save events; please wait a moment."));
                }
            }

            RecordSaveCommand command = new RecordSaveCommand(
                matchId,
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
        /// Deletes a goal event from a floorball match
        /// </summary>
        [HttpDelete("goal/{goalEventId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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
        [HttpDelete("penalty/{penaltyEventId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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
        [HttpDelete("save/{saveEventId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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
        [HttpPost("overtime")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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
        [HttpPost("shootout")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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
        /// Starts a period in a floorball match
        /// </summary>
        [HttpPost("periods/{periodNumber:int}/start")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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
        [HttpPost("periods/{periodNumber:int}/end")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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
        /// Imports a batch of historical goals and penalties onto an already-started match
        /// in one request. Skips the live double-click rate limiter. Failed individual
        /// events are listed in <c>eventErrors</c>; successful events are still saved.
        /// </summary>
        [HttpPost("import")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchEventsImportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchEventsImportDto>>> ImportEvents(
            Guid matchId,
            [FromBody] ImportFloorballMatchEventsRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Importing {Count} events for floorball match {MatchId}",
                request.Events.Count,
                matchId);

            List<ImportFloorballMatchEventItem> events = request.Events
                .Select(item => new ImportFloorballMatchEventItem(
                    item.EventType,
                    item.TeamId,
                    item.PlayerId,
                    item.AssistingPlayerId,
                    item.SecondaryAssistingPlayerId,
                    item.PeriodNumber,
                    item.TimeInSeconds,
                    item.GoalType,
                    item.Description,
                    item.PenaltyMinutes,
                    item.PenaltyType))
                .ToList();

            Result<FloorballMatchEventsImportDto> result = await _mediator.Send(
                new ImportFloorballMatchEventsCommand(matchId, events), cancellationToken);

            return HandleResult(result, "Match events imported", "Failed to import match events");
        }
    }
}
