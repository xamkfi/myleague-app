using Application.Features.Football.Players.Queries;
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
using Domain.Repositories.Football;
using Domain.Repositories.Common;
using Domain.Entities.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Football.Teams.Queries;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for retrieving a football team by ID
/// </summary>
public class GetFootballTeamByIdHandler : IRequestHandler<GetFootballTeamByIdQuery, Result<FootballTeamDto>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IPersonRepository _personRepository;
    private readonly ILogger<GetFootballTeamByIdHandler> _logger;
    private readonly IFootballPlayerRepository _footballPlayerRepository;

    /// <summary>
    /// Initializes a new instance of the GetFootballTeamByIdHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetFootballTeamByIdHandler(
        IFootballTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetFootballTeamByIdHandler> logger,
        IPersonRepository personRepository,
        IFootballPlayerRepository footballPlayerRepository)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
        _personRepository = personRepository;
        _footballPlayerRepository = footballPlayerRepository;
    }

    /// <summary>
    /// Handles the GetFootballTeamByIdQuery request
    /// </summary>
    /// <param name="request">The query containing the team ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The team as a DTO wrapped in a Result, or a not found result</returns>
    public async Task<Result<FootballTeamDto>> Handle(GetFootballTeamByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving football team with ID: {TeamId}", request.Id);
            
            FootballTeam? team = await _teamRepository.GetByIdAsync(request.Id);
            if (team == null)
            {
                _logger.LogWarning("Football team with ID {TeamId} not found", request.Id);
                return Result<FootballTeamDto>.NotFound("FootballTeam", request.Id);
            }

            // Load the club for the team
            Club? club = await _clubRepository.GetByIdAsync(team.ClubId);
            if (club == null)
            {
                _logger.LogWarning("Club with ID {ClubId} not found for team {TeamId}", team.ClubId, team.Id);
                return Result<FootballTeamDto>.Failure("Associated club not found");
            }

            //Load Person using player ids.
            List<Guid> playerIds = team.Roster.Select(t => t.PlayerId).ToList();
            Dictionary<Guid, Person> playerPersons = new Dictionary<Guid, Person>();

            if (playerIds.Any())
            {
                foreach (Guid playerId in playerIds)
                {
                    FootballPlayer? footballPlayer = await _footballPlayerRepository.GetByIdAsync(playerId);
                    if (footballPlayer != null)
                    {
                        Person? person = await _personRepository.GetByIdAsync(footballPlayer.PersonId);
                        if (person != null)
                        {
                            playerPersons[playerId] = person;
                        }
                    }
                }
            }

            FootballTeamDto teamDto = FootballTeamMapper.ToDto(team, club, playerPersons);
            _logger.LogInformation("Successfully retrieved football team: {TeamId}", team.Id);

            return Result<FootballTeamDto>.Success(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football team: {TeamId}", request.Id);
            return Result<FootballTeamDto>.Failure("An error occurred while retrieving the football team.");
        }
    }
} 
