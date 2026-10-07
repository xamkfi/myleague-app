using Application.Common;
using Application.Features.Hockey.Matches.Commands;
using Application.Features.Hockey.Matches.DTOs;
using Application.Features.Hockey.Matches.Queries;
using Domain.Common;
using Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

/// <summary>
/// Assigns and removes hockey match scorekeepers.
/// Every hockey match controller shares the <c>api/HockeyMatch</c> route prefix.
/// </summary>
[Route("api/HockeyMatch")]
public class HockeyMatchScorekeepersController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyMatchScorekeepersController"/>.
    /// </summary>
    public HockeyMatchScorekeepersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Adds a person as a scorekeeper (toimitsija) to the match.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/scorekeepers")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddScorekeeper(
        Guid matchId,
        [FromBody] AddMatchScorekeeperRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new AddHockeyMatchScorekeeperCommand(matchId, request.PersonId), cancellationToken);
        return HandleResult(result, "Scorekeeper added to hockey match successfully", "Failed to add scorekeeper");
    }

    /// <summary>
    /// Removes a scorekeeper (toimitsija) from the match.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{matchId:guid}/scorekeepers/{personId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RemoveScorekeeper(
        Guid matchId,
        Guid personId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new RemoveHockeyMatchScorekeeperCommand(matchId, personId), cancellationToken);
        return HandleResult(result, "Scorekeeper removed from hockey match successfully", "Failed to remove scorekeeper");
    }
}
