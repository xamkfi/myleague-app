using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Endpoints for managing per-match team rosters: the active goalie and the active field
    /// player lineup. Both operate within a specific match-team pair.
    /// </summary>
    [Route("api/floorball-matches/{matchId:guid}/teams/{teamId:guid}")]
    [Authorize]
    public class FloorballMatchRosterController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballMatchRosterController> _logger;

        /// <summary>
        /// Creates a new <see cref="FloorballMatchRosterController"/>.
        /// </summary>
        public FloorballMatchRosterController(
            IMediator mediator,
            ILogger<FloorballMatchRosterController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Changes the active goalie for a team in a match
        /// </summary>
        [HttpPut("goalie/{goalieId:guid}")]
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
                GoalieId = goalieId,
            };

            Result<FloorballMatchDto> result = await _mediator.Send(command, cancellationToken);

            return HandleResult(result, "Goalie changed successfully", "Failed to change goalie");
        }

        /// <summary>
        /// Replaces the active field player lineup (and optionally the active goalie) for a single
        /// team in a match. Used by the match-management UI's "Edit lineup" dialog.
        /// </summary>
        [HttpPut("active-roster")]
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
