using Application.Commands.Floorball.Match;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for recording a penalty in a floorball match
/// </summary>
public class RecordPenaltyHandler : IRequestHandler<RecordPenaltyCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<RecordPenaltyHandler> _logger;

    public RecordPenaltyHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<RecordPenaltyHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(RecordPenaltyCommand request, CancellationToken cancellationToken)
    {
        try
        {
            FloorballMatch? match = await _matchRepository.GetByIdAsync(request.MatchId);
            if (match == null)
            {
                _logger.LogWarning("Match not found with ID: {MatchId}", request.MatchId);
                return Result<FloorballMatchDto>.Failure($"Match with ID {request.MatchId} not found.");
            }

            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FloorballMatchDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            FloorballPlayer? player = null;
            if (request.PlayerId.HasValue)
            {
                player = await _playerRepository.GetByIdAsync(request.PlayerId.Value);
                if (player == null)
                {
                    _logger.LogWarning("Player not found with ID: {PlayerId}", request.PlayerId);
                    return Result<FloorballMatchDto>.Failure($"Player with ID {request.PlayerId} not found.");
                }
            }

            _logger.LogInformation("Recording penalty in match {MatchId}", request.MatchId);

            match.RecordPenalty(
                team,
                player!,
                request.PenaltyType,
                request.Minutes,
                request.PeriodNumber,
                request.TimeInSeconds,
                request.Description);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while recording penalty in match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while recording the penalty.");
        }
    }
}


