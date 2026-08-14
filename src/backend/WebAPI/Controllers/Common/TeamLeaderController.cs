using System.Security.Claims;
using Application.Common;
using Application.Features.Common.TeamLeader.DTOs;
using Application.Features.Common.TeamLeader.Queries;
using Application.Features.Floorball.Matches.Commands;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Matches.Queries;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Queries;
using Application.Features.Football.Teams.DTOs;
using Application.Services.Common;
using Domain.Common;
using Domain.Constants;
using Domain.Enums.Floorball;
using Domain.Enums.Football;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;
using WebAPI.Models.Floorball;
using WebAPI.Models.Football;
using FloorballJerseyNumberCommand = Application.Features.Floorball.Teams.Commands.UpdateTeamPlayerJerseyNumberCommand;
using FootballJerseyNumberCommand = Application.Features.Football.Teams.Commands.UpdateTeamPlayerJerseyNumberCommand;

namespace WebAPI.Controllers.Common;

/// <summary>
/// Endpoints for team leaders: listing their own teams, editing jersey numbers on their
/// rosters, and announcing (pre-filling) the match roster/lineup for upcoming matches.
/// Administrators may also call these endpoints; team leaders are restricted to teams
/// they actively manage via the team manager link.
/// </summary>
[Route("api/team-leader")]
[Authorize(Roles = AuthRoles.TeamLeaderOrAdmin)]
public class TeamLeaderController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ITeamLeaderAccessService _accessService;
    private readonly ILogger<TeamLeaderController> _logger;

    /// <summary>
    /// Initializes a new instance of the TeamLeaderController class
    /// </summary>
    public TeamLeaderController(
        IMediator mediator,
        ITeamLeaderAccessService accessService,
        ILogger<TeamLeaderController> logger)
    {
        _mediator = mediator;
        _accessService = accessService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all teams (floorball and football) that the current user actively manages
    /// </summary>
    [HttpGet("my-teams")]
    [ProducesResponseType(typeof(ApiResponse<List<TeamLeaderTeamDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<TeamLeaderTeamDto>>>> GetMyTeams()
    {
        if (!TryGetPersonId(out Guid personId))
        {
            return Unauthorized(ApiResponse<List<TeamLeaderTeamDto>>.ErrorResponse("Invalid token."));
        }

        Result<IEnumerable<TeamLeaderTeamDto>> result = await _mediator.Send(new GetMyTeamsQuery(personId));

        return HandleListResult(result, "Teams retrieved successfully", "Failed to retrieve teams");
    }

    /// <summary>
    /// Gets the upcoming (scheduled) floorball matches for a team the current user manages
    /// </summary>
    [HttpGet("floorball/teams/{teamId:guid}/upcoming-matches")]
    [ProducesResponseType(typeof(ApiResponse<List<FloorballMatchDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<FloorballMatchDto>>>> GetFloorballUpcomingMatches(
        Guid teamId,
        CancellationToken cancellationToken)
    {
        ActionResult? accessError = await CheckFloorballTeamAccessAsync(teamId);
        if (accessError != null)
        {
            return accessError;
        }

        Result<PagedResult<FloorballMatchDto>> result = await _mediator.Send(
            new GetFloorballMatchesByTeamQuery(
                Page: 1,
                PageSize: 100,
                TeamId: teamId,
                StartDate: DateTime.UtcNow.Date),
            cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            return BadRequest(ApiResponse<List<FloorballMatchDto>>.ErrorResponse(
                result.Error ?? "Failed to retrieve upcoming matches"));
        }

        List<FloorballMatchDto> upcoming = result.Data.Items
            .Where(m => m.Status == FloorballMatchStatus.Scheduled)
            .OrderBy(m => m.ScheduledDateTime)
            .ToList();

        return Ok(ApiResponse<List<FloorballMatchDto>>.SuccessResponse(upcoming, "Upcoming matches retrieved successfully"));
    }

    /// <summary>
    /// Gets the upcoming (scheduled) football matches for a team the current user manages
    /// </summary>
    [HttpGet("football/teams/{teamId:guid}/upcoming-matches")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballMatchDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<FootballMatchDto>>>> GetFootballUpcomingMatches(
        Guid teamId,
        CancellationToken cancellationToken)
    {
        ActionResult? accessError = await CheckFootballTeamAccessAsync(teamId);
        if (accessError != null)
        {
            return accessError;
        }

        Result<PagedResult<FootballMatchDto>> result = await _mediator.Send(
            new GetFootballMatchesByTeamQuery(
                Page: 1,
                PageSize: 100,
                TeamId: teamId,
                StartDate: DateTime.UtcNow.Date,
                EndDate: null),
            cancellationToken);

        if (!result.IsSuccess || result.Data is null)
        {
            return BadRequest(ApiResponse<List<FootballMatchDto>>.ErrorResponse(
                result.Error ?? "Failed to retrieve upcoming matches"));
        }

        List<FootballMatchDto> upcoming = result.Data.Items
            .Where(m => m.Status == FootballMatchStatus.Scheduled)
            .OrderBy(m => m.ScheduledDateTime)
            .ToList();

        return Ok(ApiResponse<List<FootballMatchDto>>.SuccessResponse(upcoming, "Upcoming matches retrieved successfully"));
    }

    /// <summary>
    /// Updates the jersey number of a player on a floorball team the current user manages
    /// </summary>
    [HttpPut("floorball/teams/{teamId:guid}/players/{playerId:guid}/jersey-number")]
    [ProducesResponseType(typeof(ApiResponse<FloorballTeamPlayerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<FloorballTeamPlayerDto>>> UpdateFloorballJerseyNumber(
        Guid teamId,
        Guid playerId,
        [FromBody] UpdateJerseyNumberRequest request)
    {
        ActionResult? accessError = await CheckFloorballTeamAccessAsync(teamId);
        if (accessError != null)
        {
            return accessError;
        }

        _logger.LogInformation(
            "Team leader updating jersey number for player {PlayerId} in floorball team {TeamId} to {JerseyNumber}",
            playerId, teamId, request.JerseyNumber);

        Result<FloorballTeamPlayerDto> result = await _mediator.Send(
            new FloorballJerseyNumberCommand(teamId, playerId, request.JerseyNumber));

        return HandleResult(result, "Jersey number updated successfully", "Failed to update jersey number");
    }

    /// <summary>
    /// Updates the jersey number of a player on a football team the current user manages
    /// </summary>
    [HttpPut("football/teams/{teamId:guid}/players/{playerId:guid}/jersey-number")]
    [ProducesResponseType(typeof(ApiResponse<FootballTeamPlayerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<FootballTeamPlayerDto>>> UpdateFootballJerseyNumber(
        Guid teamId,
        Guid playerId,
        [FromBody] UpdateJerseyNumberRequest request)
    {
        ActionResult? accessError = await CheckFootballTeamAccessAsync(teamId);
        if (accessError != null)
        {
            return accessError;
        }

        _logger.LogInformation(
            "Team leader updating jersey number for player {PlayerId} in football team {TeamId} to {JerseyNumber}",
            playerId, teamId, request.JerseyNumber);

        Result<FootballTeamPlayerDto> result = await _mediator.Send(
            new FootballJerseyNumberCommand(teamId, playerId, request.JerseyNumber));

        return HandleResult(result, "Jersey number updated successfully", "Failed to update jersey number");
    }

    /// <summary>
    /// Announces (saves) the active roster for one team in an upcoming floorball match.
    /// Writes into the same match roster structure the admin UI uses, so the announced
    /// roster is immediately visible in the match view. Only allowed while the match is
    /// still scheduled.
    /// </summary>
    [HttpPut("floorball/matches/{matchId:guid}/teams/{teamId:guid}/roster")]
    [ProducesResponseType(typeof(ApiResponse<FloorballMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FloorballMatchDto>>> AnnounceFloorballRoster(
        Guid matchId,
        Guid teamId,
        [FromBody] SetMatchActiveRosterRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse("Request body is required."));
        }

        ActionResult? accessError = await CheckFloorballTeamAccessAsync(teamId);
        if (accessError != null)
        {
            return accessError;
        }

        Result<FloorballMatchDto> matchResult = await _mediator.Send(new GetFloorballMatchByIdQuery(matchId), cancellationToken);
        if (!matchResult.IsSuccess || matchResult.Data is null)
        {
            return NotFound(ApiResponse<FloorballMatchDto>.ErrorResponse($"Match with ID {matchId} not found."));
        }

        FloorballMatchDto match = matchResult.Data;
        if (match.Status != FloorballMatchStatus.Scheduled)
        {
            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse(
                "The roster can only be announced while the match is still scheduled."));
        }

        if (match.HomeTeamId != teamId && match.AwayTeamId != teamId)
        {
            return BadRequest(ApiResponse<FloorballMatchDto>.ErrorResponse("Team is not participating in this match."));
        }

        _logger.LogInformation(
            "Team leader announcing roster for floorball match {MatchId}, team {TeamId} ({PlayerCount} players, goalie={GoalieId})",
            matchId, teamId, request.Players?.Count ?? 0, SanitizeForLog(request.GoalieId));

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

        return HandleResult(result, "Match roster announced successfully", "Failed to announce match roster");
    }

    /// <summary>
    /// Announces (saves) the lineup for one team in an upcoming football match.
    /// Writes into the same match lineup structure the admin UI uses, so the announced
    /// lineup is immediately visible in the match view. Only allowed while the match is
    /// still scheduled.
    /// </summary>
    [HttpPut("football/matches/{matchId:guid}/teams/{teamId:guid}/lineup")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> AnnounceFootballLineup(
        Guid matchId,
        Guid teamId,
        [FromBody] SetMatchLineupRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<FootballMatchDto>.ErrorResponse("Request body is required."));
        }

        ActionResult? accessError = await CheckFootballTeamAccessAsync(teamId);
        if (accessError != null)
        {
            return accessError;
        }

        Result<FootballMatchDto> matchResult = await _mediator.Send(new GetFootballMatchByIdQuery(matchId), cancellationToken);
        if (!matchResult.IsSuccess || matchResult.Data is null)
        {
            return NotFound(ApiResponse<FootballMatchDto>.ErrorResponse($"Match with ID {matchId} not found."));
        }

        FootballMatchDto match = matchResult.Data;
        if (match.Status != FootballMatchStatus.Scheduled)
        {
            return BadRequest(ApiResponse<FootballMatchDto>.ErrorResponse(
                "The lineup can only be announced while the match is still scheduled."));
        }

        if (match.HomeTeamId != teamId && match.AwayTeamId != teamId)
        {
            return BadRequest(ApiResponse<FootballMatchDto>.ErrorResponse("Team is not participating in this match."));
        }

        _logger.LogInformation(
            "Team leader announcing lineup for football match {MatchId}, team {TeamId} ({PlayerCount} players)",
            matchId, teamId, request.Players?.Count ?? 0);

        SetMatchLineupCommand command = new SetMatchLineupCommand
        {
            MatchId = matchId,
            TeamId = teamId,
            Players = request.Players?.Select(p => new LineupPlayerInput
            {
                PlayerId = p.PlayerId,
                Position = p.Position,
                IsOnField = p.IsOnField,
            }).ToList() ?? new List<LineupPlayerInput>(),
        };

        Result<FootballMatchDto> result = await _mediator.Send(command, cancellationToken);

        return HandleResult(result, "Match lineup announced successfully", "Failed to announce match lineup");
    }

    private bool IsAdmin =>
        User.IsInRole(AuthRoles.ClubAdmin) || User.IsInRole(AuthRoles.SystemAdmin);

    private bool TryGetPersonId(out Guid personId)
    {
        string? personIdClaim = User.FindFirst("personId")?.Value;
        return Guid.TryParse(personIdClaim, out personId);
    }

    /// <summary>
    /// Returns null when the caller may manage the floorball team, otherwise the error result.
    /// Admins always pass; team leaders must have an active team manager link.
    /// </summary>
    private async Task<ActionResult?> CheckFloorballTeamAccessAsync(Guid teamId)
    {
        if (IsAdmin)
        {
            return null;
        }

        if (!TryGetPersonId(out Guid personId))
        {
            return Unauthorized(ApiResponse.ErrorResponse("Invalid token."));
        }

        if (!await _accessService.CanManageFloorballTeamAsync(personId, teamId))
        {
            _logger.LogWarning("Person {PersonId} attempted to manage floorball team {TeamId} without access", personId, teamId);
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.ErrorResponse("You are not a team leader of this team."));
        }

        return null;
    }

    /// <summary>
    /// Returns null when the caller may manage the football team, otherwise the error result.
    /// Admins always pass; team leaders must have an active team manager link.
    /// </summary>
    private async Task<ActionResult?> CheckFootballTeamAccessAsync(Guid teamId)
    {
        if (IsAdmin)
        {
            return null;
        }

        if (!TryGetPersonId(out Guid personId))
        {
            return Unauthorized(ApiResponse.ErrorResponse("Invalid token."));
        }

        if (!await _accessService.CanManageFootballTeamAsync(personId, teamId))
        {
            _logger.LogWarning("Person {PersonId} attempted to manage football team {TeamId} without access", personId, teamId);
            return StatusCode(StatusCodes.Status403Forbidden, ApiResponse.ErrorResponse("You are not a team leader of this team."));
        }

        return null;
    }
}
