using Application.Commands.Floorball.Match;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for recording a goal in a floorball match
/// </summary>
public class RecordGoalHandler : IRequestHandler<RecordGoalCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<RecordGoalHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the RecordGoalHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="playerRepository">The floorball player repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public RecordGoalHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<RecordGoalHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the RecordGoalCommand request
    /// </summary>
    /// <param name="request">The command containing goal information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated match as a DTO wrapped in a Result</returns>
    public async Task<Result<FloorballMatchDto>> Handle(RecordGoalCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the match
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            // Get the scoring team
            FloorballTeam? scoringTeam = await _teamRepository.GetByIdAsync(request.ScoringTeamId);
            if (scoringTeam == null)
            {
                _logger.LogWarning("Scoring team not found with ID: {TeamId}", request.ScoringTeamId);
                return Result<FloorballMatchDto>.Failure($"Scoring team with ID {request.ScoringTeamId} not found.");
            }

            // Get the scoring player
            FloorballPlayer? scoringPlayer = await _playerRepository.GetByIdAsync(request.ScoringPlayerId);
            if (scoringPlayer == null)
            {
                _logger.LogWarning("Scoring player not found with ID: {PlayerId}", request.ScoringPlayerId);
                return Result<FloorballMatchDto>.Failure($"Scoring player with ID {request.ScoringPlayerId} not found.");
            }

            // Get the assisting player (optional)
            FloorballPlayer? assistingPlayer = null;
            if (request.AssistingPlayerId.HasValue)
            {
                assistingPlayer = await _playerRepository.GetByIdAsync(request.AssistingPlayerId.Value);
                if (assistingPlayer == null)
                {
                    _logger.LogWarning("Assisting player not found with ID: {PlayerId}", request.AssistingPlayerId.Value);
                    return Result<FloorballMatchDto>.Failure($"Assisting player with ID {request.AssistingPlayerId.Value} not found.");
                }
            }

            // Get the second assisting player (optional)
            FloorballPlayer? secondAssistingPlayer = null;
            if (request.SecondaryAssistingPlayerId.HasValue)
            {
                secondAssistingPlayer = await _playerRepository.GetByIdAsync(request.SecondaryAssistingPlayerId.Value);
                if (secondAssistingPlayer == null)
                {
                    _logger.LogWarning("Assisting player not found with ID: {PlayerId}", request.SecondaryAssistingPlayerId.Value);
                    return Result<FloorballMatchDto>.Failure($"Assisting player with ID {request.SecondaryAssistingPlayerId.Value} not found.");
                }
            }

            _logger.LogInformation("Recording goal in match {MatchId} by player {PlayerId}", request.MatchId, request.ScoringPlayerId);

            FloorballGoal goal = match.RecordGoal(scoringTeam, scoringPlayer,
                assistingPlayer, secondAssistingPlayer,
                request.PeriodNumber, request.TimeInSeconds,
                request.Description, request.GoalType);

            _matchRepository.MarkEventAsAdded(goal);

            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully recorded goal in match {MatchId} by player {PlayerId}", request.MatchId, request.ScoringPlayerId);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording goal in match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while recording the goal.");
        }
    }
} 
