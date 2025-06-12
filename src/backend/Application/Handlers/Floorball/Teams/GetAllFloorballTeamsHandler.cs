using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Domain.Repositories.Common;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Queries.Floorball.Team;
using Domain.Entities.Common;

namespace Application.Handlers.Floorball.Teams;

/// <summary>
/// Handler for retrieving all floorball teams
/// </summary>
public class GetAllFloorballTeamsHandler : IRequestHandler<GetAllFloorballTeamsQuery, Result<IEnumerable<FloorballTeamDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IClubRepository _clubRepository;
    private readonly ILogger<GetAllFloorballTeamsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballTeamsHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="clubRepository">The club repository</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballTeamsHandler(
        IFloorballTeamRepository teamRepository,
        IClubRepository clubRepository,
        ILogger<GetAllFloorballTeamsHandler> logger)
    {
        _teamRepository = teamRepository;
        _clubRepository = clubRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetAllFloorballTeamsQuery request
    /// </summary>
    /// <param name="request">The query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All floorball teams as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballTeamDto>>> Handle(GetAllFloorballTeamsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving all floorball teams");
            
            // Get all teams
            IEnumerable<FloorballTeam> teams = await _teamRepository.GetAllAsync();
            
            // Load all clubs
            IEnumerable<Club> clubs = await _clubRepository.GetAllAsync();
            Dictionary<Guid, Club> clubDictionary = clubs.ToDictionary(c => c.Id);
            
            // Map teams to DTOs with their corresponding clubs
            IEnumerable<FloorballTeamDto> teamDtos = FloorballTeamMapper.ToDtos(teams, clubDictionary);
            
            _logger.LogInformation("Successfully retrieved {TeamCount} floorball teams", teamDtos.Count());
            
            return Result<IEnumerable<FloorballTeamDto>>.Success(teamDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all floorball teams");
            return Result<IEnumerable<FloorballTeamDto>>.Failure("An error occurred while retrieving floorball teams.");
        }
    }
} 
