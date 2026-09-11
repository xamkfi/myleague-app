using Application.Features.Football.Teams.Queries;
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
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Application.Features.Football.Teams.Handlers;

/// <summary>
/// Handler for retrieving football teams by club
/// </summary>
public class GetFootballTeamsByClubHandler : IRequestHandler<GetFootballTeamsByClubQuery, Result<IEnumerable<FootballTeamDto>>>
{
    private readonly IFootballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetFootballTeamsByClubHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFootballTeamsByClubHandler class
    /// </summary>
    /// <param name="teamRepository">The football team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetFootballTeamsByClubHandler(
        IFootballTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetFootballTeamsByClubHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFootballTeamsByClubQuery request
    /// </summary>
    /// <param name="request">The query containing club ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Football teams by club as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FootballTeamDto>>> Handle(GetFootballTeamsByClubQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving football teams for club: {ClubId}", request.ClubId);
            
            // Load the club first
            Club? club = await _clubRepository.GetByIdAsync(request.ClubId);
            if (club == null)
            {
                _logger.LogWarning("Club with ID {ClubId} not found", request.ClubId);
                return Result<IEnumerable<FootballTeamDto>>.Failure("Club not found");
            }
            
            // Get teams for the club
            IEnumerable<FootballTeam?> teamsNullable = await _teamRepository.GetByClubIdAsync(request.ClubId);
            IEnumerable<FootballTeam> teams = teamsNullable.Where(t => t != null)!;
            
            // Create club dictionary for mapping (all teams belong to the same club)
            Dictionary<Guid, Club> clubDictionary = new Dictionary<Guid, Club> { { club.Id, club } };
            IEnumerable<FootballTeamDto> teamDtos = FootballTeamMapper.ToDtos(teams, clubDictionary);
            
            _logger.LogInformation("Successfully retrieved {TeamCount} football teams for club: {ClubId}", teamDtos.Count(), request.ClubId);
            
            return Result<IEnumerable<FootballTeamDto>>.Success(teamDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving football teams for club: {ClubId}", request.ClubId);
            return Result<IEnumerable<FootballTeamDto>>.Failure("An error occurred while retrieving football teams.");
        }
    }
} 
