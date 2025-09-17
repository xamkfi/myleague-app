using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Application.Commands.Floorball.Match;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Services.Common;
using Domain.Entities.Floorball;
using Domain.Enums.Floorball;
using Domain.Repositories.Floorball;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for completing a floorball match
/// </summary>
public class CompleteFloorballMatchHandler : IRequestHandler<CompleteFloorballMatchCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballStatisticsRepository _statisticsRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly IMatchTimerService _timerService;
    private readonly ILogger<CompleteFloorballMatchHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the CompleteFloorballMatchHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="statisticsRepository">The statistics repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="timerService">The timer service</param>
    /// <param name="logger">The logger</param>
    public CompleteFloorballMatchHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballStatisticsRepository statisticsRepository,
        IFloorballUnitOfWork unitOfWork,
        IMatchTimerService timerService,
        ILogger<CompleteFloorballMatchHandler> logger)
    {
        _matchRepository = matchRepository;
        _statisticsRepository = statisticsRepository;
        _unitOfWork = unitOfWork;
        _timerService = timerService;
        _logger = logger;
    }

    /// <summary>
    /// Handles the CompleteFloorballMatchCommand request
    /// </summary>
    /// <param name="request">The command containing match ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The completed match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(CompleteFloorballMatchCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the match
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.Id);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.Id);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.Id} not found.");
            }

            _logger.LogInformation("Completing floorball match: {MatchId}", request.Id);
            match.Complete();

            // Update final team season statistics (wins/losses/ties)
            await UpdateFinalTeamSeasonStatistics(match, cancellationToken);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Destroy the timer for this match to prevent background service queries
            try
            {
                _logger.LogInformation("Destroying timer for completed match: {MatchId}", request.Id);
                await _timerService.DestroyTimerAsync(request.Id);
                _logger.LogInformation("Successfully destroyed timer for completed match: {MatchId}", request.Id);
            }
            catch (Exception timerEx)
            {
                _logger.LogWarning(timerEx, "Failed to destroy timer for completed match: {MatchId}. This is non-critical.", request.Id);
                // Don't fail the match completion if timer destruction fails
            }

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully completed floorball match: {MatchId}", request.Id);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing floorball match: {MatchId}", request.Id);
            return Result<FloorballMatchDto>.Failure("An error occurred while completing the match.");
        }
    }

    /// <summary>
    /// Updates final team season statistics with match results (wins/losses/ties)
    /// </summary>
    private async Task UpdateFinalTeamSeasonStatistics(FloorballMatch match, CancellationToken cancellationToken)
    {
        // Update home team statistics
        await UpdateTeamMatchResult(match.HomeTeamId, match.SeasonId, match.HomeScore, match.AwayScore, true, match, cancellationToken);
        
        // Update away team statistics  
        await UpdateTeamMatchResult(match.AwayTeamId, match.SeasonId, match.AwayScore, match.HomeScore, false, match, cancellationToken);
    }

    /// <summary>
    /// Updates team season statistics with match result
    /// </summary>
    private async Task UpdateTeamMatchResult(Guid teamId, Guid seasonId, int teamScore, int opponentScore, bool isHomeGame, FloorballMatch match, CancellationToken cancellationToken)
    {
        FloorballTeamSeasonStatistics? teamStats = await _statisticsRepository.GetTeamSeasonStatisticsAsync(teamId, seasonId, cancellationToken);
        if (teamStats == null)
        {
            teamStats = new FloorballTeamSeasonStatistics(teamId, seasonId);
        }

        // Determine match result using enum
        FloorballGameResult gameResult;
        if (teamScore > opponentScore) gameResult = FloorballGameResult.Win;
        else if (teamScore < opponentScore) gameResult = FloorballGameResult.Loss;
        else gameResult = FloorballGameResult.Tie;

        // Update team statistics with match result
        teamStats.UpdateAfterMatch(
            gameResult: gameResult,
            isHomeGame: isHomeGame,
            goalsFor: teamScore,
            goalsAgainst: opponentScore);

        await _statisticsRepository.SaveTeamSeasonStatisticsAsync(teamStats, cancellationToken);
    }
} 
