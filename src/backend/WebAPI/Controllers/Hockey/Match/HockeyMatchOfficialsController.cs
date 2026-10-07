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
/// Assigns and removes hockey match officials.
/// Every hockey match controller shares the <c>api/HockeyMatch</c> route prefix.
/// </summary>
[Route("api/HockeyMatch")]
public class HockeyMatchOfficialsController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyMatchOfficialsController"/>.
    /// </summary>
    public HockeyMatchOfficialsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Assigns an official to the match.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpPost("{matchId:guid}/officials")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> AddOfficial(
        Guid matchId,
        [FromBody] AddHockeyMatchOfficialRequest request,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(new AddHockeyMatchOfficialCommand(
            matchId,
            request.OfficialId,
            request.Role,
            request.IsMainOfficial), cancellationToken);
        return HandleResult(result, "Official added to hockey match successfully", "Failed to add official");
    }

    /// <summary>
    /// Removes an official from the match.
    /// </summary>
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [HttpDelete("{matchId:guid}/officials/{officialId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyMatchDto>>> RemoveOfficial(
        Guid matchId,
        Guid officialId,
        CancellationToken cancellationToken = default)
    {
        Result<HockeyMatchDto> result = await _mediator.Send(
            new RemoveHockeyMatchOfficialCommand(matchId, officialId), cancellationToken);
        return HandleResult(result, "Official removed from hockey match successfully", "Failed to remove official");
    }
}
