using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Common;
using Domain.Common;
using Application.Features.Common.Shared.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Statistics.Queries;
using Application.Features.Floorball.Teams.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using WebAPI.DependencyInjections;
using WebAPI.Controllers.Common;
using WebAPI.Models.Common;
using WebAPI.Models.Common.Pagination;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball statistics
    /// </summary>
    [Route("api/floorball/statistics")]
    public class FloorballStatisticsController : BaseApiController
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FloorballStatisticsController> _logger;

        /// <summary>
        /// Initializes new instance of FloorballStatisticsController class
        /// </summary>
        /// <param name="mediator">Mediator instance for handling commands and queries</param>
        /// <param name="logger">Logger instance for logging</param>
        public FloorballStatisticsController(IMediator mediator, ILogger<FloorballStatisticsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Gets team statistics for a specific season
        /// </summary>
        /// <param name="competitionId">The season ID</param>
        /// <param name="teamId">The team ID</param>
        /// <returns>Team season statistics</returns>
        [HttpGet("team/{competitionId:guid}/{teamId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamSeasonStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamSeasonStatisticsDto>>> GetTeamStatistics(Guid competitionId, Guid teamId)
        {
            _logger.LogInformation("Getting team statistics for Team: {TeamId} in Season: {CompetitionId}", teamId, competitionId);

            GetFloorballTeamSeasonStatisticsQuery query = new GetFloorballTeamSeasonStatisticsQuery(competitionId, teamId);
            Result<FloorballTeamSeasonStatisticsDto> result = await _mediator.Send(query);

            return HandleResult(result, "Team statistics retrieved successfully", "Failed to retrieve team statistics");
        }

        /// <summary>
        /// Gets a team's combined statistics aggregated across every competition (regular seasons
        /// + tournaments) the team has played in. Used by the team page so the Statistics tab
        /// surfaces tournament games and points alongside the regular-season totals.
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <returns>Aggregated team statistics across all competitions</returns>
        [HttpGet("team-aggregate/{teamId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamSeasonStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamSeasonStatisticsDto>>> GetAggregatedTeamStatistics(Guid teamId)
        {
            _logger.LogInformation("Getting aggregated team statistics for Team: {TeamId}", teamId);

            GetAggregatedFloorballTeamStatisticsQuery query = new GetAggregatedFloorballTeamStatisticsQuery(teamId);
            Result<FloorballTeamSeasonStatisticsDto> result = await _mediator.Send(query);

            return HandleResult(result, "Aggregated team statistics retrieved successfully", "Failed to retrieve aggregated team statistics");
        }

        /// <summary>
        /// Gets per-player statistics for a team aggregated across every competition (regular
        /// seasons + tournaments) the team has played in. Each player appears once with their
        /// totals summed; used by the team page's player stats table.
        /// </summary>
        /// <param name="teamId">The team ID</param>
        /// <returns>Aggregated per-player statistics</returns>
        [HttpGet("team-players-aggregate/{teamId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>>> GetAggregatedTeamPlayerStatistics(Guid teamId)
        {
            _logger.LogInformation("Getting aggregated player statistics for Team: {TeamId}", teamId);

            GetAggregatedFloorballTeamPlayerStatisticsQuery query = new GetAggregatedFloorballTeamPlayerStatisticsQuery(teamId);
            Result<List<FloorballPlayerSeasonStatisticsDto>> result = await _mediator.Send(query);

            return HandleResult(result, "Aggregated team player statistics retrieved successfully", "Failed to retrieve aggregated team player statistics");
        }

        /// <summary>
        /// Gets all player statistics for a specific team in a season
        /// </summary>
        /// <param name="competitionId">The season ID</param>
        /// <param name="teamId">The team ID</param>
        /// <returns>List of player season statistics for the team</returns>
        [HttpGet("team-players/{competitionId:guid}/{teamId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>>> GetTeamPlayerStatistics(Guid competitionId, Guid teamId)
        {
            _logger.LogInformation("Getting player statistics for Team: {TeamId} in Season: {CompetitionId}", teamId, competitionId);

            GetFloorballTeamPlayerStatisticsQuery query = new GetFloorballTeamPlayerStatisticsQuery(competitionId, teamId);
            Result<List<FloorballPlayerSeasonStatisticsDto>> result = await _mediator.Send(query);

            return HandleResult(result, "Team player statistics retrieved successfully", "Failed to retrieve team player statistics");
        }

        /// <summary>
        /// Gets player statistics for a specific season
        /// </summary>
        /// <param name="competitionId">The season ID</param>
        /// <param name="playerId">The player ID</param>
        /// <returns>Player season statistics</returns>
        [HttpGet("player/{competitionId:guid}/{playerId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballPlayerSeasonStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPlayerSeasonStatisticsDto>>> GetPlayerStatistics(Guid competitionId, Guid playerId)
        {
            _logger.LogInformation("Getting player statistics for Player: {PlayerId} in Season: {CompetitionId}", playerId, competitionId);

            GetFloorballPlayerSeasonStatisticsQuery query = new GetFloorballPlayerSeasonStatisticsQuery(competitionId, playerId);
            Result<FloorballPlayerSeasonStatisticsDto> result = await _mediator.Send(query);

            return HandleResult(result, "Player statistics retrieved successfully", "Failed to retrieve player statistics");
        }

        /// <summary>
        /// Gets a player profile with all season statistics
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        [HttpGet("playerprofile/{playerId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballPlayerProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPlayerProfileDto>>> GetPlayerProfile(Guid playerId)
        {
            GetFloorballPlayerProfileQuery query = new GetFloorballPlayerProfileQuery(playerId);

            Result<FloorballPlayerProfileDto> result = await _mediator.Send(query);

            return HandleResult(result, "Player profile retrieved succesfully", "Failed to retrieve player profile");
        }

        /// <summary>
        /// Gets match statistics for a specific match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>Match statistics for both teams</returns>
        [HttpGet("match/{matchId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballMatchTeamStatisticsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballMatchTeamStatisticsDto>>>> GetMatchStatistics(Guid matchId)
        {
            _logger.LogInformation("Getting match statistics for Match: {MatchId}", matchId);

            GetFloorballMatchStatisticsQuery query = new GetFloorballMatchStatisticsQuery(matchId);
            Result<List<FloorballMatchTeamStatisticsDto>> result = await _mediator.Send(query);

            return HandleResult(result, "Match statistics retrieved successfully", "Failed to retrieve match statistics");
        }

        /// <summary>
        /// Gets top scorers for a specific season
        /// </summary>
        /// <param name="competitionId">The season ID</param>
        /// <param name="topN">Number of top scorers to retrieve (default: 10)</param>
        /// <returns>List of top scorers</returns>
        [HttpGet("topscorers/{competitionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>>> GetTopScorers(Guid competitionId, [FromQuery] int topN = 10)
        {
            _logger.LogInformation("Getting top {TopN} scorers for Season: {CompetitionId}", topN, competitionId);

            GetFloorballTopScorersQuery query = new GetFloorballTopScorersQuery(competitionId, topN);
            Result<List<FloorballPlayerSeasonStatisticsDto>> result = await _mediator.Send(query);

            return HandleResult(result, $"Top {topN} scorers retrieved successfully", "Failed to retrieve top scorers");
        }

        /// <summary>
        /// Gets season statistics summary
        /// </summary>
        /// <param name="competitionId">The season ID</param>
        /// <param name="topN">Maximum rows per player leaderboard (1-100, default 10)</param>
        /// <returns>Season statistics summary</returns>
        [HttpGet("season/{competitionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonStatisticsSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonStatisticsSummaryDto>>> GetSeasonStatistics(
            Guid competitionId,
            [FromQuery] int topN = 10)
        {
            _logger.LogInformation("Getting season statistics summary for Season: {CompetitionId}, TopN: {TopN}", competitionId, topN);

            GetFloorballSeasonStatisticsSummaryQuery query = new GetFloorballSeasonStatisticsSummaryQuery(competitionId, topN);
            Result<FloorballSeasonStatisticsSummaryDto> result = await _mediator.Send(query);

            return HandleResult(result, "Season statistics retrieved successfully", "Failed to retrieve season statistics");
        }

        /// <summary>
        /// Gets team standings for a specific season
        /// </summary>
        /// <param name="competitionId">The season ID</param>
        /// <returns>Team standings ordered by points. An empty list when the season has no standings yet.</returns>
        [HttpGet("standings/{competitionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTeamSeasonStatisticsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballTeamSeasonStatisticsDto>>>> GetTeamStandings(Guid competitionId)
        {
            _logger.LogInformation("Getting team standings for Season: {CompetitionId}", competitionId);

            GetFloorballTeamStandingsQuery query = new GetFloorballTeamStandingsQuery(competitionId);
            Result<List<FloorballTeamSeasonStatisticsDto>> result = await _mediator.Send(query);

            return HandleResult(result, "Team standings retrieved successfully", "Failed to retrieve team standings");
        }

        /// <summary>
        /// Gets standings for a single tournament group computed from completed group-stage matches.
        /// </summary>
        /// <param name="groupId">The tournament group ID</param>
        /// <returns>Per-team standings rows ordered by Points → GoalDifference → GoalsFor</returns>
        [HttpGet("standings/group/{groupId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTournamentGroupStandingDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballTournamentGroupStandingDto>>>> GetTournamentGroupStandings(Guid groupId)
        {
            _logger.LogInformation("Getting tournament group standings for Group: {GroupId}", groupId);

            GetFloorballTournamentGroupStandingsQuery query = new GetFloorballTournamentGroupStandingsQuery(groupId);
            Result<List<FloorballTournamentGroupStandingDto>> result = await _mediator.Send(query);

            return HandleResult(result, "Tournament group standings retrieved successfully", "Failed to retrieve tournament group standings");
        }

        /// <summary>
        /// Gets paged all-time player statistics summed across public competitions.
        /// </summary>
        /// <param name="request">Page, sort, team category, and competition type</param>
        /// <returns>Players ordered by the requested column</returns>
        [HttpGet("all-time")]
        [EnableRateLimiting(PublicTrafficProtectionExtensions.PublicStatsPolicy)]
        [OutputCache(PolicyName = PublicTrafficProtectionExtensions.PublicStatsCachePolicy)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballAllTimePlayerStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballAllTimePlayerStatisticsDto>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(PaginatedApiResponse<FloorballAllTimePlayerStatisticsDto>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaginatedApiResponse<FloorballAllTimePlayerStatisticsDto>>> GetAllTimePlayerStatistics(
            [FromQuery] GetAllTimePlayerStatisticsRequest request)
        {
            _logger.LogInformation(
                "Getting floorball all-time player statistics page {Page} sorted by {Sort}",
                request.Page,
                request.Sort);

            GetFloorballAllTimePlayerStatisticsQuery query = new(
                request.Page,
                request.PageSize,
                request.TeamCategory,
                request.CompetitionType,
                request.Sort,
                request.Direction,
                request.Search,
                request.TeamId);
            Result<PagedResult<FloorballAllTimePlayerStatisticsDto>> result = await _mediator.Send(query);

            return HandlePaginatedResult(result, "All-time player statistics retrieved successfully", "Failed to retrieve all-time player statistics");
        }

        /// <summary>
        /// Gets the teams that have public all-time player statistics.
        /// </summary>
        /// <param name="request">Team category and competition type</param>
        /// <returns>Teams ordered by name</returns>
        [HttpGet("all-time/teams")]
        [EnableRateLimiting(PublicTrafficProtectionExtensions.PublicStatsPolicy)]
        [OutputCache(PolicyName = PublicTrafficProtectionExtensions.PublicStatsCachePolicy)]
        [ProducesResponseType(typeof(ApiResponse<List<AllTimeTeamOptionDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<AllTimeTeamOptionDto>>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<List<AllTimeTeamOptionDto>>>> GetAllTimeTeams(
            [FromQuery] GetAllTimeTeamsRequest request)
        {
            GetFloorballAllTimeTeamsQuery query = new(request.TeamCategory, request.CompetitionType);
            Result<List<AllTimeTeamOptionDto>> result = await _mediator.Send(query);

            return HandleResult(result, "All-time teams retrieved successfully", "Failed to retrieve all-time teams");
        }

    }
}
