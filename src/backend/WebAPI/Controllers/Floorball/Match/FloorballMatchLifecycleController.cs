using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball.Match
{
    /// <summary>
    /// Endpoints that change a single floorball match's lifecycle status (Scheduled,
    /// InProgress, Completed, Postponed, Cancelled) or assign the participating teams.
    /// All endpoints require authentication and address a specific match through
    /// <c>{matchId}</c>.
    /// </summary>
    [Route("api/floorball-matches/{matchId:guid}")]
    [Authorize]
    public class FloorballMatchLifecycleController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballMatchLifecycleController> _logger;

        /// <summary>
        /// Creates a new <see cref="FloorballMatchLifecycleController"/>.
        /// </summary>
        public FloorballMatchLifecycleController(
            IMediator mediator,
            ILogger<FloorballMatchLifecycleController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Starts a floorball match
        /// </summary>
        [HttpPut("start")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> StartMatch(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting floorball match with ID: {id}", matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new StartFloorballMatchCommand(matchId), cancellationToken);

            return HandleResult(result, "Floorball match started successfully", "Failed to start floorball match");
        }

        /// <summary>
        /// Completes a floorball match
        /// </summary>
        [HttpPut("complete")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> CompleteMatch(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Completing floorball match with ID: {id}", matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new CompleteFloorballMatchCommand(matchId), cancellationToken);

            return HandleResult(result, "Completed floorball match successfully", "Failed to complete floorball match");
        }

        /// <summary>
        /// Reopens a previously completed floorball match back to InProgress so the operator can
        /// correct mistakes or continue recording events. Per-match aggregates that were applied
        /// at completion time (team / player / goalie season stats) are reverted in the handler.
        /// Playoff matches are rejected because bracket propagation rollback is not supported.
        /// </summary>
        [HttpPut("reopen")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> ReopenMatch(
            Guid matchId,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reopening floorball match with ID: {id}", matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new ReopenFloorballMatchCommand(matchId), cancellationToken);

            return HandleResult(result, "Reopened floorball match successfully", "Failed to reopen floorball match");
        }

        /// <summary>
        /// Postpones a floorball match
        /// </summary>
        [HttpPost("postpone")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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
        [HttpPost("cancel")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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
        /// Reactivates a cancelled floorball match back to Scheduled status
        /// </summary>
        [HttpPost("reactivate")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
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
        /// Assigns (or clears) the home and away team slots on a scheduled or postponed match.
        /// </summary>
        /// <remarks>
        /// Pass <c>null</c> for either side to leave the slot as "to be determined". When the
        /// match is a playoff bracket match, the change is automatically propagated forward into
        /// the next bracket slot (provided that next match hasn't started yet). The endpoint
        /// rejects any attempt to change teams on a match that is already InProgress or Completed.
        /// </remarks>
        [HttpPut("teams")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> AssignTeams(
            Guid matchId,
            [FromBody] AssignMatchTeamsRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Assigning teams on floorball match {MatchId}: home={HomeTeamId}, away={AwayTeamId}",
                matchId, SanitizeForLog(request.HomeTeamId), SanitizeForLog(request.AwayTeamId));

            AssignMatchTeamsCommand command = new AssignMatchTeamsCommand(
                matchId,
                request.HomeTeamId,
                request.AwayTeamId);

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            return HandleResult(result, "Match teams updated successfully", "Failed to update match teams");
        }
    }
}
