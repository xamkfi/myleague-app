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
/// Handler for retrieving floorball teams by season ID
/// </summary>
public class GetFloorballTeamsBySeasonIdHandler : IRequestHandler<GetFloorballTeamsBySeasonIdQuery, Result<IEnumerable<FloorballTeamDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly IFloorballSeasonRepository _seasonRepository;
    private readonly ILogger<GetFloorballTeamsBySeasonIdHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballTeamsBySeasonIdHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="seasonRepository">The floorball season repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballTeamsBySeasonIdHandler(
        IFloorballTeamRepository teamRepository,
        IFloorballSeasonRepository seasonRepository,
        ILogger<GetFloorballTeamsBySeasonIdHandler> logger)
    {
        _teamRepository = teamRepository;
        _seasonRepository = seasonRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballTeamsBySeasonIdQuery request
    /// </summary>
    /// <param name="request">The query containing the season ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Teams in the season as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballTeamDto>>> Handle(GetFloorballTeamsBySeasonIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify season exists
            bool seasonExists = await _seasonRepository.ExistsAsync(request.SeasonId);
            if (!seasonExists)
            {
                _logger.LogWarning("Attempt to get teams for non-existent season with ID: {SeasonId}", request.SeasonId);
                return Result<IEnumerable<FloorballTeamDto>>.NotFound("FloorballSeason", request.SeasonId);
            }

            _logger.LogInformation("Retrieving floorball teams for season: {SeasonId}", request.SeasonId);
            
            IEnumerable<FloorballTeam> teams = await _teamRepository.GetBySeasonIdAsync(request.SeasonId);
            IEnumerable<FloorballTeamDto> teamDtos = FloorballTeamMapper.ToDtos(teams);
            
            _logger.LogInformation("Successfully retrieved {TeamCount} teams for season {SeasonId}", 
                teamDtos.Count(), request.SeasonId);
            
            return Result<IEnumerable<FloorballTeamDto>>.Success(teamDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball teams for season: {SeasonId}", request.SeasonId);
            return Result<IEnumerable<FloorballTeamDto>>.Failure("An error occurred while retrieving the season's teams.");
        }
    }
} 