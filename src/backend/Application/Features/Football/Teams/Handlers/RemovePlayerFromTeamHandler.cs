using Application.Features.Football.Teams.Commands;
using Application.Features.Football.Teams.DTOs;
using Application.Features.Football.Players.DTOs;
using Application.Features.Football.Referees.DTOs;
using Application.Features.Football.TeamManagers.DTOs;
using Application.Features.Football.Teams.Mappings;
using Application.Features.Football.Players.Mappings;
using Application.Features.Football.Referees.Mappings;
using Application.Features.Football.TeamManagers.Mappings;
using Application.Common;
using Domain.Entities.Football.Teams;
using Domain.Entities.Common;
using Domain.Repositories.Football;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Repositories.Common;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for removing a player from a football team
/// </summary>
public class RemovePlayerFromTeamHandler : IRequestHandler<RemovePlayerFromTeamCommand, Result<FootballTeamDto>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IFootballUnitOfWork _footballUnitOfWork;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemovePlayerFromTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the RemovePlayerFromTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public RemovePlayerFromTeamHandler(
        IFootballTeamRepository teamRepository,
        IClubRepository clubRepository,
        IFootballUnitOfWork footballUnitOfWork,
        IUnitOfWork unitOfWork,
        ILogger<RemovePlayerFromTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _footballUnitOfWork = footballUnitOfWork;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the RemovePlayerFromTeamCommand request
    /// </summary>
    /// <param name="request">The command containing player and team information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballTeamDto>> Handle(RemovePlayerFromTeamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the team
            FootballTeam? team = await _teamRepository.GetByIdAsync(request.TeamId);
            if (team == null)
            {
                _logger.LogWarning("Team not found with ID: {TeamId}", request.TeamId);
                return Result<FootballTeamDto>.Failure($"Team with ID {request.TeamId} not found.");
            }

            _logger.LogInformation("Removing player {PlayerId} from team {TeamId}", request.PlayerId, request.TeamId);
            team.RemovePlayer(request.PlayerId);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _footballUnitOfWork.SaveChangesAsync(cancellationToken);

            // Load the club for the team
            Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
            if (club == null)
            {
                _logger.LogWarning("Club with ID {ClubId} not found for team {TeamId}", team.ClubId, team.Id);
                return Result<FootballTeamDto>.Failure("Associated club not found");
            }

            FootballTeamDto teamDto = FootballTeamMapper.ToDto(team, club);
            _logger.LogInformation("Successfully removed player {PlayerId} from team {TeamId}", request.PlayerId, request.TeamId);

            return Result<FootballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while removing player {PlayerId} from team {TeamId}", request.PlayerId, request.TeamId);
            return Result<FootballTeamDto>.Failure("An error occurred while removing the player from the team.");
        }
    }
} 
