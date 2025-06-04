using Application.Queries.Floorball.Team;
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
/// Handler for retrieving floorball teams by division
/// </summary>
public class GetFloorballTeamsByDivisionHandler : IRequestHandler<GetFloorballTeamsByDivisionQuery, Result<IEnumerable<FloorballTeamDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly ILogger<GetFloorballTeamsByDivisionHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballTeamsByDivisionHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballTeamsByDivisionHandler(
        IFloorballTeamRepository teamRepository,
        ILogger<GetFloorballTeamsByDivisionHandler> logger)
    {
        _teamRepository = teamRepository;
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
            _logger.LogInformation("Retrieving floorball teams for division: {Division}", request.Division);
            
            IEnumerable<FloorballTeam> teams = await _teamRepository.GetByDivisionAsync(request.Division);
            IEnumerable<FloorballTeamDto> teamDtos = FloorballTeamMapper.ToDtos(teams);
            
            _logger.LogInformation("Successfully retrieved {TeamCount} floorball teams for division: {Division}", teamDtos.Count(), request.Division);
            
            return Result<IEnumerable<FloorballTeamDto>>.Success(teamDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball teams for division: {Division}", request.Division);
            return Result<IEnumerable<FloorballTeamDto>>.Failure("An error occurred while retrieving floorball teams.");
        }
    }
} 