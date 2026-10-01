using Domain.Constants;
using Application.Common;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Floorball.Match
{
    /// <summary>
    /// Endpoints for managing the optional scorekeepers (toimitsijat) of a single floorball match.
    /// </summary>
    [Route("api/floorball-matches/{matchId:guid}/scorekeepers")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    public class FloorballMatchScorekeepersController : BaseApiController
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Creates a new <see cref="FloorballMatchScorekeepersController"/>.
        /// </summary>
        public FloorballMatchScorekeepersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Adds a person as a scorekeeper to a floorball match.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> AddScorekeeper(
            Guid matchId,
            [FromBody] AddMatchScorekeeperRequest request,
            CancellationToken cancellationToken)
        {
            Result<FloorballMatchDto> result = await _mediator.Send(
                new AddFloorballScorekeeperToMatchCommand(matchId, request.PersonId), cancellationToken);

            return HandleResult(result, "Scorekeeper added successfully", "Failed to add scorekeeper");
        }

        /// <summary>
        /// Removes a scorekeeper from a floorball match.
        /// </summary>
        [HttpDelete("{personId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> RemoveScorekeeper(
            Guid matchId,
            Guid personId,
            CancellationToken cancellationToken)
        {
            Result<FloorballMatchDto> result = await _mediator.Send(
                new RemoveFloorballScorekeeperFromMatchCommand(matchId, personId), cancellationToken);

            return HandleResult(result, "Scorekeeper removed successfully", "Failed to remove scorekeeper");
        }
    }
}
