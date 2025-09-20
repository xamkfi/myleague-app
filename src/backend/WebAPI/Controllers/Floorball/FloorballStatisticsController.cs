using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Queries.Floorball.Statistics;
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
        /// <param name="seasonId">The season ID</param>
        /// <param name="teamId">The team ID</param>
        /// <returns>Team season statistics</returns>
        [HttpGet("team/{seasonId:guid}/{teamId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballTeamSeasonStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballTeamSeasonStatisticsDto>>> GetTeamStatistics(Guid seasonId, Guid teamId)
        {
            _logger.LogInformation("Getting team statistics for Team: {TeamId} in Season: {SeasonId}", teamId, seasonId);

            GetTeamSeasonStatisticsQuery query = new GetTeamSeasonStatisticsQuery(seasonId, teamId);
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
        /// Gets player statistics for a specific season
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <param name="playerId">The player ID</param>
        /// <returns>Player season statistics</returns>
        [HttpGet("player/{seasonId:guid}/{playerId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballPlayerSeasonStatisticsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballPlayerSeasonStatisticsDto>>> GetPlayerStatistics(Guid seasonId, Guid playerId)
        {
            _logger.LogInformation("Getting player statistics for Player: {PlayerId} in Season: {SeasonId}", playerId, seasonId);

            GetPlayerSeasonStatisticsQuery query = new GetPlayerSeasonStatisticsQuery(seasonId, playerId);
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
        /// <param name="seasonId">The season ID</param>
        /// <param name="topN">Number of top scorers to retrieve (default: 10)</param>
        /// <returns>List of top scorers</returns>
        [HttpGet("topscorers/{seasonId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballPlayerSeasonStatisticsDto>>>> GetTopScorers(Guid seasonId, [FromQuery] int topN = 10)
        {
            _logger.LogInformation("Getting top {TopN} scorers for Season: {SeasonId}", topN, seasonId);

            GetTopScorersQuery query = new GetTopScorersQuery(seasonId, topN);
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
        /// <param name="seasonId">The season ID</param>
        /// <returns>Season statistics summary</returns>
        [HttpGet("season/{seasonId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<FloorballSeasonStatisticsSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<FloorballSeasonStatisticsSummaryDto>>> GetSeasonStatistics(Guid seasonId)
        {
            _logger.LogInformation("Getting season statistics summary for Season: {SeasonId}", seasonId);

            GetSeasonStatisticsSummaryQuery query = new GetSeasonStatisticsSummaryQuery(seasonId);
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
        /// <param name="seasonId">The season ID</param>
        /// <returns>Team standings ordered by points</returns>
        [HttpGet("standings/{seasonId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<FloorballTeamSeasonStatisticsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<List<FloorballTeamSeasonStatisticsDto>>>> GetTeamStandings(Guid seasonId)
        {
            _logger.LogInformation("Getting team standings for Season: {SeasonId}", seasonId);

            GetTeamStandingsQuery query = new GetTeamStandingsQuery(seasonId);
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

    }
}
