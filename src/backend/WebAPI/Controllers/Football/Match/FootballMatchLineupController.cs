using Domain.Constants;
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

/// <summary>
/// Controller for managing football match lineups
/// </summary>
[Route("api/football-matches/{matchId:guid}/teams/{teamId:guid}")]
[Authorize(Roles = AuthRoles.AdminOnly)]
public class FootballMatchLineupController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FootballMatchLineupController> _logger;

    /// <summary>
    /// Initializes a new instance of the FootballMatchLineupController class
    /// </summary>
    public FootballMatchLineupController(
        IMediator mediator,
        ILogger<FootballMatchLineupController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Set lineup
    /// </summary>
    [HttpPut("lineup")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> SetLineup(
        Guid matchId,
        Guid teamId,
        [FromBody] SetMatchLineupRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<FootballMatchDto>.ErrorResponse("Request body is required."));
        }

        SetMatchLineupCommand command = new()
        {
            MatchId = matchId,
            TeamId = teamId,
            Players = request.Players?.Select(p => new LineupPlayerInput
            {
                PlayerId = p.PlayerId,
                Position = p.Position,
                IsOnField = p.IsOnField
            }).ToList() ?? new List<LineupPlayerInput>()
        };

        Result<FootballMatchDto> result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result, "Lineup updated successfully", "Failed to update lineup");
    }
}
