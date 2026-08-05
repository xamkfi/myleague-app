using Application.Common;
using Application.Features.Hockey.Statistics.Commands;
using Application.Features.Hockey.Statistics.DTOs;
using Application.Features.Hockey.Statistics.Queries;
using Domain.Enums.Hockey.Statistics;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Hockey;

namespace WebAPI.Controllers.Hockey;

/// <summary>
/// API endpoints for hockey statistics recalculation and reads.
/// </summary>
[Route("api/HockeyStatistics")]
public class HockeyStatisticsController : BaseApiController
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Creates a new <see cref="HockeyStatisticsController"/>.
    /// </summary>
    public HockeyStatisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Recalculates match-level statistics.
    /// </summary>
    [HttpPost("matches/{matchId:guid}/recalculate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> RecalculateMatch(Guid matchId)
    {
        Result result = await _mediator.Send(new RecalculateHockeyMatchStatisticsCommand(matchId));
        return HandleVoidResult(result, "Match statistics recalculated successfully", "Failed to recalculate match statistics");
    }

    /// <summary>
    /// Recalculates competition aggregate statistics for a scope.
    /// </summary>
    [HttpPost("competitions/{competitionId:guid}/recalculate")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> RecalculateCompetition(
        Guid competitionId,
        [FromBody] RecalculateHockeyCompetitionStatisticsRequest? request)
    {
        RecalculateHockeyCompetitionStatisticsRequest body = request ?? new RecalculateHockeyCompetitionStatisticsRequest();
        Result result = await _mediator.Send(new RecalculateHockeyCompetitionStatisticsCommand(
            competitionId,
            body.Scope,
            body.CompetitionDivisionId,
            body.TournamentGroupId,
            body.PlayoffSeriesId));
        return HandleVoidResult(result, "Competition statistics recalculated successfully", "Failed to recalculate competition statistics");
    }

    /// <summary>
    /// Resets competition aggregate statistics without rebuilding.
    /// </summary>
    [HttpPost("competitions/{competitionId:guid}/reset")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> ResetCompetition(
        Guid competitionId,
        [FromBody] ResetHockeyCompetitionStatisticsRequest? request)
    {
        ResetHockeyCompetitionStatisticsRequest body = request ?? new ResetHockeyCompetitionStatisticsRequest();
        Result result = await _mediator.Send(new ResetHockeyCompetitionStatisticsCommand(
            competitionId,
            body.Scope,
            body.CompetitionDivisionId,
            body.TournamentGroupId,
            body.PlayoffSeriesId));
        return HandleVoidResult(result, "Competition statistics reset successfully", "Failed to reset competition statistics");
    }

    /// <summary>
    /// Gets match box score statistics.
    /// </summary>
    [HttpGet("matches/{matchId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyMatchStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyMatchStatisticsDto>>> GetMatch(Guid matchId)
    {
        Result<HockeyMatchStatisticsDto> result = await _mediator.Send(new GetHockeyMatchStatisticsQuery(matchId));
        return HandleResult(result, "Match statistics retrieved successfully", "Failed to retrieve match statistics");
    }

    /// <summary>
    /// Gets competition standings.
    /// </summary>
    [HttpGet("standings/{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyTeamCompetitionStatisticsDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyTeamCompetitionStatisticsDto>>>> GetStandings(Guid competitionId)
    {
        Result<List<HockeyTeamCompetitionStatisticsDto>> result =
            await _mediator.Send(new GetHockeyCompetitionStandingsQuery(competitionId));
        return HandleResult(result, "Standings retrieved successfully", "Failed to retrieve standings");
    }

    /// <summary>
    /// Gets division standings.
    /// </summary>
    [HttpGet("standings/{competitionId:guid}/divisions/{divisionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyTeamCompetitionStatisticsDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyTeamCompetitionStatisticsDto>>>> GetDivisionStandings(
        Guid competitionId,
        Guid divisionId)
    {
        Result<List<HockeyTeamCompetitionStatisticsDto>> result =
            await _mediator.Send(new GetHockeyDivisionStandingsQuery(competitionId, divisionId));
        return HandleResult(result, "Division standings retrieved successfully", "Failed to retrieve division standings");
    }

    /// <summary>
    /// Gets tournament group standings.
    /// </summary>
    [HttpGet("standings/{competitionId:guid}/groups/{groupId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyTeamCompetitionStatisticsDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyTeamCompetitionStatisticsDto>>>> GetTournamentGroupStandings(
        Guid competitionId,
        Guid groupId)
    {
        Result<List<HockeyTeamCompetitionStatisticsDto>> result =
            await _mediator.Send(new GetHockeyTournamentGroupStandingsQuery(competitionId, groupId));
        return HandleResult(result, "Tournament group standings retrieved successfully", "Failed to retrieve group standings");
    }

    /// <summary>
    /// Gets playoff series statistics.
    /// </summary>
    [HttpGet("standings/{competitionId:guid}/playoffs/{playoffSeriesId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyPlayoffSeriesStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyPlayoffSeriesStatisticsDto>>> GetPlayoffSeriesStatistics(
        Guid competitionId,
        Guid playoffSeriesId)
    {
        Result<HockeyPlayoffSeriesStatisticsDto> result =
            await _mediator.Send(new GetHockeyPlayoffSeriesStatisticsQuery(competitionId, playoffSeriesId));
        return HandleResult(result, "Playoff series statistics retrieved successfully", "Failed to retrieve playoff series statistics");
    }

    /// <summary>
    /// Gets one team's competition statistics.
    /// </summary>
    [HttpGet("teams/{competitionId:guid}/{teamId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyTeamCompetitionStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HockeyTeamCompetitionStatisticsDto>>> GetTeamStatistics(
        Guid competitionId,
        Guid teamId,
        [FromQuery] HockeyStatisticsScope scope = HockeyStatisticsScope.Competition,
        [FromQuery] Guid? competitionDivisionId = null,
        [FromQuery] Guid? tournamentGroupId = null,
        [FromQuery] Guid? playoffSeriesId = null)
    {
        Result<HockeyTeamCompetitionStatisticsDto> result = await _mediator.Send(
            new GetHockeyTeamCompetitionStatisticsQuery(
                competitionId,
                teamId,
                scope,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId));
        return HandleResult(result, "Team statistics retrieved successfully", "Failed to retrieve team statistics");
    }

    /// <summary>
    /// Gets player competition statistics (list or single when playerId+teamId provided).
    /// </summary>
    [HttpGet("players/{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyPlayerCompetitionStatisticsDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyPlayerCompetitionStatisticsDto>>>> GetPlayerStatistics(
        Guid competitionId,
        [FromQuery] HockeyStatisticsScope scope = HockeyStatisticsScope.Competition,
        [FromQuery] Guid? playerId = null,
        [FromQuery] Guid? teamId = null,
        [FromQuery] Guid? competitionDivisionId = null,
        [FromQuery] Guid? tournamentGroupId = null,
        [FromQuery] Guid? playoffSeriesId = null)
    {
        Result<List<HockeyPlayerCompetitionStatisticsDto>> result = await _mediator.Send(
            new GetHockeyPlayerCompetitionStatisticsQuery(
                competitionId,
                scope,
                playerId,
                teamId,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId));
        return HandleResult(result, "Player statistics retrieved successfully", "Failed to retrieve player statistics");
    }

    /// <summary>
    /// Gets goalie competition statistics (list or single when playerId+teamId provided).
    /// </summary>
    [HttpGet("goalies/{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyGoalieCompetitionStatisticsDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyGoalieCompetitionStatisticsDto>>>> GetGoalieStatistics(
        Guid competitionId,
        [FromQuery] HockeyStatisticsScope scope = HockeyStatisticsScope.Competition,
        [FromQuery] Guid? playerId = null,
        [FromQuery] Guid? teamId = null,
        [FromQuery] Guid? competitionDivisionId = null,
        [FromQuery] Guid? tournamentGroupId = null,
        [FromQuery] Guid? playoffSeriesId = null)
    {
        Result<List<HockeyGoalieCompetitionStatisticsDto>> result = await _mediator.Send(
            new GetHockeyGoalieCompetitionStatisticsQuery(
                competitionId,
                scope,
                playerId,
                teamId,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId));
        return HandleResult(result, "Goalie statistics retrieved successfully", "Failed to retrieve goalie statistics");
    }

    /// <summary>
    /// Gets top scorers.
    /// </summary>
    [HttpGet("topscorers/{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyTopScorerDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyTopScorerDto>>>> GetTopScorers(
        Guid competitionId,
        [FromQuery] HockeyStatisticsScope scope = HockeyStatisticsScope.Competition,
        [FromQuery] int topN = 10,
        [FromQuery] Guid? competitionDivisionId = null,
        [FromQuery] Guid? tournamentGroupId = null,
        [FromQuery] Guid? playoffSeriesId = null)
    {
        Result<List<HockeyTopScorerDto>> result = await _mediator.Send(
            new GetHockeyTopScorersQuery(
                competitionId,
                scope,
                topN,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId));
        return HandleResult(result, "Top scorers retrieved successfully", "Failed to retrieve top scorers");
    }

    /// <summary>
    /// Gets top goalies.
    /// </summary>
    [HttpGet("topgoalies/{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<HockeyTopGoalieDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HockeyTopGoalieDto>>>> GetTopGoalies(
        Guid competitionId,
        [FromQuery] HockeyStatisticsScope scope = HockeyStatisticsScope.Competition,
        [FromQuery] int topN = 10,
        [FromQuery] int minimumGamesPlayed = 1,
        [FromQuery] Guid? competitionDivisionId = null,
        [FromQuery] Guid? tournamentGroupId = null,
        [FromQuery] Guid? playoffSeriesId = null)
    {
        Result<List<HockeyTopGoalieDto>> result = await _mediator.Send(
            new GetHockeyTopGoaliesQuery(
                competitionId,
                scope,
                topN,
                minimumGamesPlayed,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId));
        return HandleResult(result, "Top goalies retrieved successfully", "Failed to retrieve top goalies");
    }

    /// <summary>
    /// Gets competition statistics summary.
    /// </summary>
    [HttpGet("summary/{competitionId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HockeyCompetitionStatisticsSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<HockeyCompetitionStatisticsSummaryDto>>> GetSummary(
        Guid competitionId,
        [FromQuery] HockeyStatisticsScope scope = HockeyStatisticsScope.Competition,
        [FromQuery] Guid? competitionDivisionId = null,
        [FromQuery] Guid? tournamentGroupId = null,
        [FromQuery] Guid? playoffSeriesId = null,
        [FromQuery] int topN = 5)
    {
        Result<HockeyCompetitionStatisticsSummaryDto> result = await _mediator.Send(
            new GetHockeyCompetitionStatisticsSummaryQuery(
                competitionId,
                scope,
                competitionDivisionId,
                tournamentGroupId,
                playoffSeriesId,
                topN));
        return HandleResult(result, "Competition statistics summary retrieved successfully", "Failed to retrieve summary");
    }
}
