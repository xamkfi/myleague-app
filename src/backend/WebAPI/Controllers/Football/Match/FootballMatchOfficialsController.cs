using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Football;

namespace WebAPI.Controllers.Football.Match;

[Route("api/football-matches/{matchId:guid}/officials")]
[Authorize]
public class FootballMatchOfficialsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FootballMatchOfficialsController> _logger;

    public FootballMatchOfficialsController(
        IMediator mediator,
        ILogger<FootballMatchOfficialsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> AddOfficial(
        Guid matchId,
        [FromBody] AddOfficialToMatchRequest request,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result =
            await _mediator.Send(new AddOfficialToMatchCommand(matchId, request.RefereeId), cancellationToken);
        return HandleResult(result, "Official added successfully", "Failed to add official");
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> UpdateOfficials(
        Guid matchId,
        [FromBody] FootballMatchOfficialsRequest request,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(
            new UpdateMatchOfficialsCommand(matchId, request.Officials ?? Array.Empty<Guid>()),
            cancellationToken);
        return HandleResult(result, "Officials updated successfully", "Failed to update officials");
    }

    [HttpDelete("{refereeId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> RemoveOfficial(
        Guid matchId,
        Guid refereeId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result =
            await _mediator.Send(new RemoveOfficialFromMatchCommand(matchId, refereeId), cancellationToken);
        return HandleResult(result, "Official removed successfully", "Failed to remove official");
    }

    [HttpPut("referee/{refereeId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> SetReferee(
        Guid matchId,
        Guid refereeId,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result =
            await _mediator.Send(new UpdateMatchOfficialsCommand(matchId, new[] { refereeId }), cancellationToken);
        return HandleResult(result, "Referee set successfully", "Failed to set referee");
    }
}
