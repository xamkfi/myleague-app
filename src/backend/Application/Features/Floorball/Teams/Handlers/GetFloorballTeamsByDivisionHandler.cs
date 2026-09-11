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
/// Handler for retrieving floorball teams by division
/// </summary>
public class GetFloorballTeamsByDivisionHandler : IRequestHandler<GetFloorballTeamsByDivisionQuery, Result<IEnumerable<FloorballTeamDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetFloorballTeamsByDivisionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballTeamsByDivisionHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballTeamsByDivisionHandler(
        IFloorballTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetFloorballTeamsByDivisionHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballTeamsByDivisionQuery request
    /// </summary>
    /// <param name="request">The query containing division</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Floorball teams by division as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballTeamDto>>> Handle(GetFloorballTeamsByDivisionQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball teams for division: {Division}", request.DivisionId);
            
            // Get teams for the division
            IEnumerable<FloorballTeam> teams = await _teamRepository.GetByDivisionAsync(request.DivisionId);
            
            // Load all clubs for the teams
            IEnumerable<Club> clubs = await _clubRepository.GetAllAsync();
            Dictionary<Guid, Club> clubDictionary = clubs.ToDictionary(c => c.Id);
            
            // Map teams to DTOs with their corresponding clubs
            IEnumerable<FloorballTeamDto> teamDtos = FloorballTeamMapper.ToDtos(teams, clubDictionary);
            
            _logger.LogInformation("Successfully retrieved {TeamCount} floorball teams for division: {Division}", teamDtos.Count(), request.DivisionId);
            
            return Result<IEnumerable<FloorballTeamDto>>.Success(teamDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball teams for division: {Division}", request.DivisionId);
            return Result<IEnumerable<FloorballTeamDto>>.Failure("An error occurred while retrieving floorball teams.");
        }
    }
} 
