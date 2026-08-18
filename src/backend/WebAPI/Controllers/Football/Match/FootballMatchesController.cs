using Domain.Constants;
using Application.Common;
using Application.Features.Football.Matches.Commands;
using Application.Features.Football.Matches.DTOs;
using Application.Features.Football.Matches.Queries;
using Domain.Common;
using Domain.Enums.Football;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;
using WebAPI.Models.Football;

namespace WebAPI.Controllers.Football;

/// <summary>
/// Controller for managing football matches
/// </summary>
[Route("api/football-matches")]
public class FootballMatchesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FootballMatchesController> _logger;

    /// <summary>
    /// Initializes a new instance of the FootballMatchesController class
    /// </summary>
    public FootballMatchesController(IMediator mediator, ILogger<FootballMatchesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all matches
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedApiResponse<FootballMatchDto>>> GetAllMatches(
        [FromQuery] GetFootballMatchesRequest request,
        CancellationToken cancellationToken)
    {
        GetAllFootballMatchesQuery query = new(
            request.Page,
            request.PageSize,
            request.CompetitionId,
            request.TeamId,
            request.StartDate,
            request.EndDate,
            request.SortOrder,
            request.SearchQuery,
            request.Status,
            request.TournamentGroupId,
            request.CompetitionType,
            request.TeamCategory);

        Result<PagedResult<FootballMatchDto>> result = await _mediator.Send(query, cancellationToken);
        return HandlePaginatedResult(result, "Football matches retrieved successfully", "Failed to retrieve football matches");
    }

    /// <summary>
    /// Get match by id
    /// </summary>
    [HttpGet("by-id/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> GetMatchById(
        Guid id,
        CancellationToken cancellationToken)
    {
        Result<FootballMatchDto> result = await _mediator.Send(new GetFootballMatchByIdQuery(id), cancellationToken);
        return HandleResult(result, "Football match retrieved successfully", "Football match not found");
    }

    /// <summary>
    /// Get match by season
    /// </summary>
    [HttpGet("by-competitionId/{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballMatchDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<FootballMatchDto>>>> GetMatchBySeason(
        Guid competitionId,
        CancellationToken cancellationToken)
    {
        Result<IEnumerable<FootballMatchDto>> result =
            await _mediator.Send(new GetFootballMatchesBySeasonQuery(competitionId), cancellationToken);
        return HandleListResult(result, "Retrieved football matches with season ID successfully", "Failed to retrieve football matches");
    }

    /// <summary>
    /// Get match by team
    /// </summary>
    [HttpGet("by-team/{teamId:guid}")]
    [ProducesResponseType(typeof(PaginatedApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedApiResponse<FootballMatchDto>>> GetMatchByTeam(
        Guid teamId,
        [FromQuery] GetTeamMatchesRequest request,
        CancellationToken cancellationToken)
    {
        GetFootballMatchesByTeamQuery query = new(
            request.Page,
            request.PageSize,
            teamId,
            request.StartDate,
            request.EndDate);

        Result<PagedResult<FootballMatchDto>> result = await _mediator.Send(query, cancellationToken);
        return HandlePaginatedResult(result, "Retrieved football matches with team ID successfully", "Failed to retrieve football matches with team ID");
    }

    /// <summary>
    /// Get todays matches by team
    /// </summary>
    [HttpGet("by-team/{teamId:guid}/today")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballMatchDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<FootballMatchDto>>>> GetTodaysMatchesByTeam(
        Guid teamId,
        CancellationToken cancellationToken)
    {
        Result<IEnumerable<FootballMatchDto>> result =
            await _mediator.Send(new GetTodaysMatchesByTeamQuery(teamId), cancellationToken);
        return HandleListResult(result, "Retrieved today's football matches with team ID successfully", "Failed to retrieve today's football matches with team ID");
    }

    /// <summary>
    /// Create match
    /// </summary>
    [HttpPost]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> CreateMatch(
        [FromBody] CreateFootballMatchRequest request,
        CancellationToken cancellationToken)
    {
        if (!DateTime.TryParse(request.ScheduledDateTime, out DateTime scheduledDateTime))
        {
            return BadRequest(ApiResponse<FootballMatchDto>.ErrorResponse("Invalid scheduled date and time format"));
        }

        FootballTournamentStage? stage = null;
        if (!string.IsNullOrWhiteSpace(request.TournamentStage))
        {
            if (!Enum.TryParse(request.TournamentStage, true, out FootballTournamentStage parsedStage))
            {
                return BadRequest(ApiResponse<FootballMatchDto>.ErrorResponse(
                    $"Invalid tournament stage '{request.TournamentStage}'. Valid values: {string.Join(", ", Enum.GetNames<FootballTournamentStage>())}"));
            }

            stage = parsedStage;
        }

        CreateFootballMatchCommand command = new(
            request.CompetitionId,
            request.HomeTeamId,
            request.AwayTeamId,
            request.RefereeId,
            scheduledDateTime,
            request.Venue,
            request.TournamentGroupId,
            stage);

        Result<FootballMatchDto> result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess && result.Data != null)
        {
            return CreatedAtAction(
                nameof(GetMatchById),
                new { id = result.Data.Id },
                ApiResponse<FootballMatchDto>.SuccessResponse(result.Data, "Football match created successfully"));
        }

        return ToErrorResponse(result, "Failed to create football match");
    }

    /// <summary>
    /// Update match
    /// </summary>
    [HttpPut]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse<FootballMatchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<FootballMatchDto>>> UpdateMatch(
        [FromBody] UpdateFootballMatchRequest request,
        CancellationToken cancellationToken)
    {
        if (!DateTimeOffset.TryParse(
                request.ScheduledDateTime,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out DateTimeOffset dto))
        {
            return BadRequest(ApiResponse<FootballMatchDto>.ErrorResponse("Invalid scheduled date and time format"));
        }

        UpdateFootballMatchCommand command = new(request.Id, dto.UtcDateTime, request.Venue);
        Result<FootballMatchDto> result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result, "Football match updated successfully", "Failed to update football match");
    }

    /// <summary>
    /// Delete match
    /// </summary>
    [HttpDelete("{matchId:guid}")]
    [Authorize(Roles = AuthRoles.AdminOnly)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> DeleteMatch(Guid matchId, CancellationToken cancellationToken)
    {
        Result result = await _mediator.Send(new DeleteFootballMatchCommand(matchId), cancellationToken);
        return HandleVoidResult(result, "Match deleted successfully", "Failed to delete match");
    }
}
