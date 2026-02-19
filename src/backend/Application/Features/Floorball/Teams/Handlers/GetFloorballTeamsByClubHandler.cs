using Application.Features.Floorball.Teams.Queries;
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
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Entities.Common;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Application.Features.Floorball.Teams.Handlers;

/// <summary>
/// Handler for retrieving floorball teams by club
/// </summary>
public class GetFloorballTeamsByClubHandler : IRequestHandler<GetFloorballTeamsByClubQuery, Result<IEnumerable<FloorballTeamDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetFloorballTeamsByClubHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballTeamsByClubHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballTeamsByClubHandler(
        IFloorballTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetFloorballTeamsByClubHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballTeamsByClubQuery request
    /// </summary>
    /// <param name="request">The query containing club ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Floorball teams by club as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballTeamDto>>> Handle(GetFloorballTeamsByClubQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball teams for club: {ClubId}", request.ClubId);
            
            // Load the club first
            Club? club = await _clubRepository.GetByIdAsync(request.ClubId);
            if (club == null)
            {
                _logger.LogWarning("Club with ID {ClubId} not found", request.ClubId);
                return Result<IEnumerable<FloorballTeamDto>>.Failure("Club not found");
            }
            
            // Get teams for the club
            IEnumerable<FloorballTeam?> teamsNullable = await _teamRepository.GetByClubIdAsync(request.ClubId);
            IEnumerable<FloorballTeam> teams = teamsNullable.Where(t => t != null)!;
            
            // Create club dictionary for mapping (all teams belong to the same club)
            Dictionary<Guid, Club> clubDictionary = new Dictionary<Guid, Club> { { club.Id, club } };
            IEnumerable<FloorballTeamDto> teamDtos = FloorballTeamMapper.ToDtos(teams, clubDictionary);
            
            _logger.LogInformation("Successfully retrieved {TeamCount} floorball teams for club: {ClubId}", teamDtos.Count(), request.ClubId);
            
            return Result<IEnumerable<FloorballTeamDto>>.Success(teamDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball teams for club: {ClubId}", request.ClubId);
            return Result<IEnumerable<FloorballTeamDto>>.Failure("An error occurred while retrieving floorball teams.");
        }
    }
} 
