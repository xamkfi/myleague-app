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
using Application.Features.Common.Users.Mappings;
using Application.Features.Common.Persons.Mappings;
using Application.Features.Common.Clubs.Mappings;
using Application.Features.Common.Divisions.Mappings;
using Application.Features.Common.News.Mappings;
using System.Collections.Generic;
using System.Linq;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for adding a player to a football team
/// </summary>
public class AddPlayerToTeamHandler : IRequestHandler<AddPlayerToTeamCommand, Result<FootballTeamDto>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IFootballPlayerRepository _playerRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IPersonRepository _personRepository;
    private readonly IFootballUnitOfWork _unitOfWork;
    private readonly ILogger<AddPlayerToTeamHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the AddPlayerToTeamHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="playerRepository">The football player repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="personRepository">The person repository</param>
    /// <param name="unitOfWork">The unit of work</param>
    /// <param name="logger">The logger</param>
    public AddPlayerToTeamHandler(
        IFootballTeamRepository teamRepository,
        IFootballPlayerRepository playerRepository,
        IClubRepository clubRepository,
        IPersonRepository personRepository,
        IFootballUnitOfWork unitOfWork,
        ILogger<AddPlayerToTeamHandler> logger)
    {
        _teamRepository = teamRepository;
        _playerRepository = playerRepository;
        _clubRepository = clubRepository;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Handles the AddPlayerToTeamCommand request
    /// </summary>
    /// <param name="request">The command containing player and team information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated team as a DTO wrapped in a Result</returns>
    public async Task<Result<FootballTeamDto>> Handle(AddPlayerToTeamCommand request, CancellationToken cancellationToken)
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

            // Get the player
            FootballPlayer? player = await _playerRepository.GetByIdAsync(request.PlayerId);
            if (player == null)
            {
                _logger.LogWarning("Player not found with ID: {PlayerId}", request.PlayerId);
                return Result<FootballTeamDto>.Failure($"Player with ID {request.PlayerId} not found.");
            }

            _logger.LogInformation("Adding player {PlayerId} to team {TeamId}", request.PlayerId, request.TeamId);
            team.AddPlayer(player, request.Position, request.JerseyNumber, request.RequestedJerseyNumber);
            
            // Save changes explicitly to trigger domain events
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Load the club for the team
            Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
            if (club == null)
            {
                _logger.LogWarning("Club with ID {ClubId} not found for team {TeamId}", team.ClubId, team.Id);
                return Result<FootballTeamDto>.Failure("Associated club not found");
            }

            // Load Person data for all players in the roster
            Dictionary<Guid, Person> playerPersons = new Dictionary<Guid, Person>();
            foreach (FootballTeamPlayer rosterPlayer in team.Roster)
            {
                // Get the FootballPlayer to find the PersonId
                FootballPlayer? footballPlayer = await _playerRepository.GetByIdAsync(rosterPlayer.PlayerId);
                if (footballPlayer != null)
                {
                    // Get the associated Person
                    Person? person = await _personRepository.GetByIdAsync(footballPlayer.PersonId);
                    if (person != null)
                    {
                        playerPersons[rosterPlayer.PlayerId] = person;
                    }
                }
            }

            FootballTeamDto teamDto = FootballTeamMapper.ToDto(team, club, playerPersons);
            _logger.LogInformation("Successfully added player {PlayerId} to team {TeamId}", request.PlayerId, request.TeamId);

            return Result<FootballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding player {PlayerId} to team {TeamId}", request.PlayerId, request.TeamId);

            return Result<FootballTeamDto>.Failure($"An error occurred while adding the player to the team.", new List<string>() { ex.Message });
        }
    }
} 
