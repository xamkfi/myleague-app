using Domain.Constants;
using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Football.Match;

/// <summary>
/// Endpoints for managing the optional scorekeepers (toimitsijat) of a single football match.
/// </summary>
[Route("api/football-matches/{matchId:guid}/scorekeepers")]
[Authorize(Roles = AuthRoles.AdminOnly)]
public class FootballMatchScorekeepersController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="FootballMatchScorekeepersController"/>.
    /// </summary>
    public FootballMatchScorekeepersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Adds a person as a scorekeeper to a football match.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> AddScorekeeper(
        Guid matchId,
        [FromBody] AddMatchScorekeeperRequest request,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(
            new AddFootballScorekeeperToMatchCommand(matchId, request.PersonId), cancellationToken);
        return HandleResult(result, "Scorekeeper added successfully", "Failed to add scorekeeper");
    }

    /// <summary>
    /// Removes a scorekeeper from a football match.
    /// </summary>
    [HttpDelete("{personId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> RemoveScorekeeper(
        Guid matchId,
        Guid personId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(
            new RemoveFootballScorekeeperFromMatchCommand(matchId, personId), cancellationToken);
        return HandleResult(result, "Scorekeeper removed successfully", "Failed to remove scorekeeper");
    }
}
