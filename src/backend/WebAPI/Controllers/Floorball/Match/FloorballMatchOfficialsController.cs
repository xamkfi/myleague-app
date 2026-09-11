using Domain.Constants;
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
    /// Endpoints for managing the officials (referees) attached to a single floorball match.
    /// Append, replace, and remove operations are exposed; "set the single referee" is offered
    /// as a convenience PUT under <c>referee/{refereeId}</c> for callers that don't keep track
    /// of the existing list.
    /// </summary>
    [Route("api/floorball-matches/{matchId:guid}/officials")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    public class FloorballMatchOfficialsController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballMatchOfficialsController> _logger;

        /// <summary>
        /// Creates a new <see cref="FloorballMatchOfficialsController"/>.
        /// </summary>
        public FloorballMatchOfficialsController(
            IMediator mediator,
            ILogger<FloorballMatchOfficialsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Adds an official (referee) to a floorball match (append semantics). Routed through
        /// the dedicated <see cref="AddOfficialToMatchCommand"/> so the append logic happens
        /// inside one transactional handler instead of two consecutive mediator calls.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> AddOfficial(
            Guid matchId,
            [FromBody] AddOfficialToMatchRequest request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Adding official {refereeId} to match ID: {matchId}", request.RefereeId, matchId);

            Result<FloorballMatchDto> result = await _mediator.Send(
                new AddOfficialToMatchCommand(matchId, request.RefereeId), cancellationToken);

            return HandleResult(result, "Official added successfully", "Failed to add official");
        }

        /// <summary>
        /// Replaces the entire officials list for a match (requires at least one).
        /// </summary>
        [HttpPut]
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
        [HttpDelete("{refereeId:guid}")]
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
        /// Replaces the officials list with a single referee (PUT semantic). Convenience
        /// endpoint for callers that don't track the existing list.
        /// </summary>
        [HttpPut("referee/{refereeId:guid}")]
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
    }
}
