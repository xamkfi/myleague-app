using Application.Queries.Floorball;
using Application.DTOs.Floorball;
using Application.Mappings.Floorball;
using Application.Common;
using Domain.Entities.Floorball;
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Handlers.Floorball.Teams;

/// <summary>
/// Handler for retrieving all floorball teams
/// </summary>
public class GetAllFloorballTeamsHandler : IRequestHandler<GetAllFloorballTeamsQuery, Result<IEnumerable<FloorballTeamDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly ILogger<GetAllFloorballTeamsHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetAllFloorballTeamsHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="logger">The logger</param>
    public GetAllFloorballTeamsHandler(
        IFloorballTeamRepository teamRepository,
        ILogger<GetAllFloorballTeamsHandler> logger)
    {
        _teamRepository = teamRepository;
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
            
            IEnumerable<FloorballTeam> teams = await _teamRepository.GetAllAsync();
            IEnumerable<FloorballTeamDto> teamDtos = FloorballTeamMapper.ToDtos(teams);
            
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