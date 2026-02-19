using Application.Features.Floorball.Matches.Commands;
using Application.Common;
using Application.Features.Floorball.Seasons.DTOs;
using Application.Features.Floorball.Matches.DTOs;
using Application.Features.Floorball.Teams.DTOs;
using Application.Features.Floorball.Players.DTOs;
using Application.Features.Floorball.Referees.DTOs;
using Application.Features.Floorball.TeamManagers.DTOs;
using Application.Features.Floorball.Statistics.DTOs;
using Application.Features.Floorball.Seasons.Mappings;
using Application.Features.Floorball.Matches.Mappings;
using Application.Features.Floorball.Teams.Mappings;
using Application.Features.Floorball.Players.Mappings;
using Application.Features.Floorball.Referees.Mappings;
using Application.Features.Floorball.TeamManagers.Mappings;
using Application.Features.Floorball.Statistics.Mappings;
using Domain.Entities.Floorball;
using Domain.Repositories.Common;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Matches.Handlers;

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

            // Change the active goalie
            match.SetActiveGoalie(request.TeamId, request.GoalieId);

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
