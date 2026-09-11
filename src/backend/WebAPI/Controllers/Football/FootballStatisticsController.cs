using Application.Common;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Statistics.DTOs;
using Application.Features.Football.Statistics.Queries;
using Application.Features.Football.Teams.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Football;

/// <summary>
/// Controller for managing football statistics
/// </summary>
[Route("api/football/statistics")]
public class FootballStatisticsController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly ILogger<FootballStatisticsController> _logger;

    /// <summary>
    /// Initializes a new instance of the FootballStatisticsController class
    /// </summary>
    public FootballStatisticsController(IMediator mediator, ILogger<FootballStatisticsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Gets team statistics for a specific season
    /// </summary>
    [HttpGet("team/{competitionId:guid}/{teamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballTeamSeasonStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTeamSeasonStatisticsDto>>> GetTeamStatistics(Guid competitionId, Guid teamId)
    {
        _logger.LogInformation("Getting team statistics for Team: {TeamId} in Season: {CompetitionId}", teamId, competitionId);

        GetTeamSeasonStatisticsQuery query = new GetTeamSeasonStatisticsQuery(competitionId, teamId);
        Result<FootballTeamSeasonStatisticsDto> result = await _mediator.Send(query);

        return HandleResult(result, "Team statistics retrieved successfully", "Failed to retrieve team statistics");
    }

    /// <summary>
    /// Gets a team's combined statistics aggregated across every competition.
    /// </summary>
    [HttpGet("team-aggregate/{teamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballTeamSeasonStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballTeamSeasonStatisticsDto>>> GetAggregatedTeamStatistics(Guid teamId)
    {
        _logger.LogInformation("Getting aggregated team statistics for Team: {TeamId}", teamId);

        GetAggregatedTeamStatisticsQuery query = new GetAggregatedTeamStatisticsQuery(teamId);
        Result<FootballTeamSeasonStatisticsDto> result = await _mediator.Send(query);

        return HandleResult(result, "Aggregated team statistics retrieved successfully", "Failed to retrieve aggregated team statistics");
    }

    /// <summary>
    /// Gets per-player statistics for a team aggregated across every competition.
    /// </summary>
    [HttpGet("team-players-aggregate/{teamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballPlayerSeasonStatisticsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballPlayerSeasonStatisticsDto>>>> GetAggregatedTeamPlayerStatistics(Guid teamId)
    {
        _logger.LogInformation("Getting aggregated player statistics for Team: {TeamId}", teamId);

        GetAggregatedTeamPlayerStatisticsQuery query = new GetAggregatedTeamPlayerStatisticsQuery(teamId);
        Result<List<FootballPlayerSeasonStatisticsDto>> result = await _mediator.Send(query);

        return HandleResult(result, "Aggregated team player statistics retrieved successfully", "Failed to retrieve aggregated team player statistics");
    }

    /// <summary>
    /// Gets all player statistics for a specific team in a season
    /// </summary>
    [HttpGet("team-players/{competitionId:guid}/{teamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballPlayerSeasonStatisticsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballPlayerSeasonStatisticsDto>>>> GetTeamPlayerStatistics(Guid competitionId, Guid teamId)
    {
        _logger.LogInformation("Getting player statistics for Team: {TeamId} in Season: {CompetitionId}", teamId, competitionId);

        GetTeamPlayerStatisticsQuery query = new GetTeamPlayerStatisticsQuery(competitionId, teamId);
        Result<List<FootballPlayerSeasonStatisticsDto>> result = await _mediator.Send(query);

        return HandleResult(result, "Team player statistics retrieved successfully", "Failed to retrieve team player statistics");
    }

    /// <summary>
    /// Gets player statistics for a specific season
    /// </summary>
    [HttpGet("player/{competitionId:guid}/{playerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballPlayerSeasonStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballPlayerSeasonStatisticsDto>>> GetPlayerStatistics(Guid competitionId, Guid playerId)
    {
        _logger.LogInformation("Getting player statistics for Player: {PlayerId} in Season: {CompetitionId}", playerId, competitionId);

        GetPlayerSeasonStatisticsQuery query = new GetPlayerSeasonStatisticsQuery(competitionId, playerId);
        Result<FootballPlayerSeasonStatisticsDto> result = await _mediator.Send(query);

        return HandleResult(result, "Player statistics retrieved successfully", "Failed to retrieve player statistics");
    }

    /// <summary>
    /// Gets a player profile with all season statistics
    /// </summary>
    [HttpGet("playerprofile/{playerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballPlayerProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballPlayerProfileDto>>> GetPlayerProfile(Guid playerId)
    {
        GetPlayerProfileQuery query = new GetPlayerProfileQuery(playerId);
        Result<FootballPlayerProfileDto> result = await _mediator.Send(query);

        return HandleResult(result, "Player profile retrieved succesfully", "Failed to retrieve player profile");
    }

    /// <summary>
    /// Gets match statistics for a specific match
    /// </summary>
    [HttpGet("match/{matchId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballMatchTeamStatisticsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballMatchTeamStatisticsDto>>>> GetMatchStatistics(Guid matchId)
    {
        _logger.LogInformation("Getting match statistics for Match: {MatchId}", matchId);

        GetMatchStatisticsQuery query = new GetMatchStatisticsQuery(matchId);
        Result<List<FootballMatchTeamStatisticsDto>> result = await _mediator.Send(query);

        return HandleResult(result, "Match statistics retrieved successfully", "Failed to retrieve match statistics");
    }

    /// <summary>
    /// Gets top scorers for a specific season
    /// </summary>
    [HttpGet("topscorers/{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballPlayerSeasonStatisticsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballPlayerSeasonStatisticsDto>>>> GetTopScorers(Guid competitionId, [FromQuery] int topN = 10)
    {
        _logger.LogInformation("Getting top {TopN} scorers for Season: {CompetitionId}", topN, competitionId);

        GetTopScorersQuery query = new GetTopScorersQuery(competitionId, topN);
        Result<List<FootballPlayerSeasonStatisticsDto>> result = await _mediator.Send(query);

        return HandleResult(result, $"Top {topN} scorers retrieved successfully", "Failed to retrieve top scorers");
    }

    /// <summary>
    /// Gets season statistics summary
    /// </summary>
    [HttpGet("season/{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FootballSeasonStatisticsSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<FootballSeasonStatisticsSummaryDto>>> GetSeasonStatistics(Guid competitionId)
    {
        _logger.LogInformation("Getting season statistics summary for Season: {CompetitionId}", competitionId);

        GetSeasonStatisticsSummaryQuery query = new GetSeasonStatisticsSummaryQuery(competitionId);
        Result<FootballSeasonStatisticsSummaryDto> result = await _mediator.Send(query);

        return HandleResult(result, "Season statistics retrieved successfully", "Failed to retrieve season statistics");
    }

    /// <summary>
    /// Gets team standings for a specific season
    /// </summary>
    [HttpGet("standings/{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballTeamSeasonStatisticsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballTeamSeasonStatisticsDto>>>> GetTeamStandings(Guid competitionId)
    {
        _logger.LogInformation("Getting team standings for Season: {CompetitionId}", competitionId);

        GetTeamStandingsQuery query = new GetTeamStandingsQuery(competitionId);
        Result<List<FootballTeamSeasonStatisticsDto>> result = await _mediator.Send(query);

        return HandleResult(result, "Team standings retrieved successfully", "Failed to retrieve team standings");
    }

    /// <summary>
    /// Gets standings for a single tournament group computed from completed group-stage matches.
    /// </summary>
    [HttpGet("standings/group/{groupId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<FootballTournamentGroupStandingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<FootballTournamentGroupStandingDto>>>> GetTournamentGroupStandings(Guid groupId)
    {
        _logger.LogInformation("Getting tournament group standings for Group: {GroupId}", groupId);

        GetTournamentGroupStandingsQuery query = new GetTournamentGroupStandingsQuery(groupId);
        Result<List<FootballTournamentGroupStandingDto>> result = await _mediator.Send(query);

        return HandleResult(result, "Tournament group standings retrieved successfully", "Failed to retrieve tournament group standings");
    }
}
