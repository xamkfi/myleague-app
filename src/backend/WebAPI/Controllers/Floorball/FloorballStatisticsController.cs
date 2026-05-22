using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Common;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Statistics.Queries;
using Application.Features.Floorball.Teams.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Models.Common;

namespace WebAPI.Controllers.Floorball
{
    /// <summary>
    /// Controller for managing floorball statistics
    /// </summary>
    [ApiController]
    [Route("api/floorball/statistics")]
    [Produces("application/json")]
    public class FloorballStatisticsController : ControllerBase
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

            GetTeamSeasonStatisticsQuery query = new GetTeamSeasonStatisticsQuery(competitionId, teamId);
            Result<FloorballTeamSeasonStatisticsDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTeamSeasonStatisticsDto>.SuccessResponse(result.Data, "Team statistics retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve team statistics";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTeamSeasonStatisticsDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballTeamSeasonStatisticsDto>.ErrorResponse(errorMessage));
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

            GetAggregatedTeamStatisticsQuery query = new GetAggregatedTeamStatisticsQuery(teamId);
            Result<FloorballTeamSeasonStatisticsDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballTeamSeasonStatisticsDto>.SuccessResponse(result.Data, "Aggregated team statistics retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve aggregated team statistics";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballTeamSeasonStatisticsDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballTeamSeasonStatisticsDto>.ErrorResponse(errorMessage));
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

            GetAggregatedTeamPlayerStatisticsQuery query = new GetAggregatedTeamPlayerStatisticsQuery(teamId);
            Result<List<FloorballPlayerSeasonStatisticsDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>.SuccessResponse(result.Data, "Aggregated team player statistics retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve aggregated team player statistics";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>.ErrorResponse(errorMessage));
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

            GetTeamPlayerStatisticsQuery query = new GetTeamPlayerStatisticsQuery(competitionId, teamId);
            Result<List<FloorballPlayerSeasonStatisticsDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>.SuccessResponse(result.Data, "Team player statistics retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve team player statistics";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>.ErrorResponse(errorMessage));
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

            GetPlayerSeasonStatisticsQuery query = new GetPlayerSeasonStatisticsQuery(competitionId, playerId);
            Result<FloorballPlayerSeasonStatisticsDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballPlayerSeasonStatisticsDto>.SuccessResponse(result.Data, "Player statistics retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve player statistics";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballPlayerSeasonStatisticsDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballPlayerSeasonStatisticsDto>.ErrorResponse(errorMessage));
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
            GetPlayerProfileQuery query = new GetPlayerProfileQuery(playerId);

            Result<FloorballPlayerProfileDto> result = await _mediator.Send(query);

            if(result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballPlayerProfileDto>.SuccessResponse(result.Data, "Player profile retrieved succesfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve player profile";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<List<FloorballPlayerProfileDto>>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<List<FloorballPlayerProfileDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets match statistics for a specific match
        /// </summary>
        /// <param name="matchId">The match ID</param>
        /// <returns>Match statistics for both teams</returns>
        [HttpGet("match/{matchId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballMatchTeamStatisticsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballMatchTeamStatisticsDto>>>> GetMatchStatistics(Guid matchId)
        {
            _logger.LogInformation("Getting match statistics for Match: {MatchId}", matchId);

            GetMatchStatisticsQuery query = new GetMatchStatisticsQuery(matchId);
            Result<List<FloorballMatchTeamStatisticsDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballMatchTeamStatisticsDto>>.SuccessResponse(result.Data, "Match statistics retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve match statistics";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<List<FloorballMatchTeamStatisticsDto>>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<List<FloorballMatchTeamStatisticsDto>>.ErrorResponse(errorMessage));
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

            GetTopScorersQuery query = new GetTopScorersQuery(competitionId, topN);
            Result<List<FloorballPlayerSeasonStatisticsDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>.SuccessResponse(result.Data, $"Top {topN} scorers retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve top scorers";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets season statistics summary
        /// </summary>
        /// <param name="competitionId">The season ID</param>
        /// <returns>Season statistics summary</returns>
        [HttpGet("season/{competitionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonStatisticsSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonStatisticsSummaryDto>>> GetSeasonStatistics(Guid competitionId)
        {
            _logger.LogInformation("Getting season statistics summary for Season: {CompetitionId}", competitionId);

            GetSeasonStatisticsSummaryQuery query = new GetSeasonStatisticsSummaryQuery(competitionId);
            Result<FloorballSeasonStatisticsSummaryDto> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<FloorballSeasonStatisticsSummaryDto>.SuccessResponse(result.Data, "Season statistics retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve season statistics";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<FloorballSeasonStatisticsSummaryDto>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<FloorballSeasonStatisticsSummaryDto>.ErrorResponse(errorMessage));
        }

        /// <summary>
        /// Gets team standings for a specific season
        /// </summary>
        /// <param name="competitionId">The season ID</param>
        /// <returns>Team standings ordered by points</returns>
        [HttpGet("standings/{competitionId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTeamSeasonStatisticsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballTeamSeasonStatisticsDto>>>> GetTeamStandings(Guid competitionId)
        {
            _logger.LogInformation("Getting team standings for Season: {CompetitionId}", competitionId);

            GetTeamStandingsQuery query = new GetTeamStandingsQuery(competitionId);
            Result<List<FloorballTeamSeasonStatisticsDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballTeamSeasonStatisticsDto>>.SuccessResponse(result.Data, "Team standings retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve team standings";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<List<FloorballTeamSeasonStatisticsDto>>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<List<FloorballTeamSeasonStatisticsDto>>.ErrorResponse(errorMessage));
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

            GetTournamentGroupStandingsQuery query = new GetTournamentGroupStandingsQuery(groupId);
            Result<List<FloorballTournamentGroupStandingDto>> result = await _mediator.Send(query);

            if (result.IsSuccess && result.Data != null)
            {
                return Ok(ApiResponse<List<FloorballTournamentGroupStandingDto>>.SuccessResponse(result.Data, "Tournament group standings retrieved successfully"));
            }

            string errorMessage = result.Error ?? "Failed to retrieve tournament group standings";
            if (errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(ApiResponse<List<FloorballTournamentGroupStandingDto>>.ErrorResponse(errorMessage));
            }

            return StatusCode(500, ApiResponse<List<FloorballTournamentGroupStandingDto>>.ErrorResponse(errorMessage));
        }

    }
}
