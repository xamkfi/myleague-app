using Application.Commands.Floorball.Match;
using Application.Common;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Matches;

/// <summary>
/// Handler for changing the active goalie during a floorball match
/// </summary>
public class ChangeGoalieHandler : IRequestHandler<ChangeGoalieCommand, Result<FloorballMatchDto>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballPlayerRepository _playerRepository;
    private readonly IFloorballUnitOfWork _unitOfWork;
    private readonly ILogger<ChangeGoalieHandler> _logger;

    public ChangeGoalieHandler(
        IFloorballMatchRepository matchRepository,
        IFloorballTeamRepository teamRepository,
        IFloorballPlayerRepository playerRepository,
        IFloorballUnitOfWork unitOfWork,
        ILogger<ChangeGoalieHandler> logger)
    {
        _matchRepository = matchRepository;
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<FloorballMatchDto>> Handle(ChangeGoalieCommand request, CancellationToken cancellationToken)
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

            // Get the team
            FloorballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FloorballMatchDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            // Get the goalie
            FloorballPlayer? goalie = await _playerRepository.GetByIdAsync(request.GoalieId);
            if (goalie == null)
            {
                _logger.LogWarning("Goalie not found with ID: {GoalieId}", request.GoalieId);
                return Result<FloorballMatchDto>.Failure($"Goalie with ID {request.GoalieId} not found.");
            }

            _logger.LogInformation("Changing goalie in match {MatchId} for team {TeamId} to goalie {GoalieId}",
                request.MatchId, request.TeamId, request.GoalieId);

            // Change the active goalie based on which team it is
            if (request.TeamId == match.HomeTeamId)
            {
                match.SetHomeActiveGoalie(request.GoalieId);
            }
            else if (request.TeamId == match.AwayTeamId)
            {
                match.SetAwayActiveGoalie(request.GoalieId);
            }
            else
            {
                return Result<FloorballMatchDto>.Failure($"Team with ID {request.TeamId} is not participating in this match.");
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            FloorballMatchDto matchDto = FloorballMatchMapper.ToDto(match);
            _logger.LogInformation("Successfully changed goalie in match {MatchId} for team {TeamId}", request.MatchId, request.TeamId);

            return Result<FloorballMatchDto>.Success(matchDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing goalie in match {MatchId}", request.MatchId);
            return Result<FloorballMatchDto>.Failure("An error occurred while changing the goalie.");
        }
    }
}
