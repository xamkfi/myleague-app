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
/// Handler for searching floorball teams by name
/// </summary>
public class SearchFloorballTeamsByNameHandler : IRequestHandler<SearchFloorballTeamsByNameQuery, Result<IEnumerable<FloorballTeamDto>>>
{
    private readonly IFloorballTeamRepository _teamRepository;
    private readonly ILogger<SearchFloorballTeamsByNameHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the SearchFloorballTeamsByNameHandler class
    /// </summary>
    /// <param name="teamRepository">The floorball team repository</param>
    /// <param name="logger">The logger</param>
    public SearchFloorballTeamsByNameHandler(
        IFloorballTeamRepository teamRepository,
        ILogger<SearchFloorballTeamsByNameHandler> logger)
    {
        _teamRepository = teamRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the SearchFloorballTeamsByNameQuery request
    /// </summary>
    /// <param name="request">The query containing the search term</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Matching floorball teams as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballTeamDto>>> Handle(SearchFloorballTeamsByNameQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Searching for floorball teams with name containing: {SearchTerm}", request.SearchTerm);
            
            IEnumerable<FloorballTeam> teams = await _teamRepository.SearchByNameAsync(request.SearchTerm);
            if (!teams.Any())
            {
                _logger.LogWarning("No floorball teams found matching search term: {SearchTerm}", request.SearchTerm);
                return Result<IEnumerable<FloorballTeamDto>>.NotFound("FloorballTeam", request.SearchTerm);
            }

            IEnumerable<FloorballTeamDto> teamDtos = FloorballTeamMapper.ToDtos(teams);
            _logger.LogInformation("Found {TeamCount} floorball teams matching search term: {SearchTerm}", 
                teamDtos.Count(), request.SearchTerm);

            return Result<IEnumerable<FloorballTeamDto>>.Success(teamDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching for floorball teams with term: {SearchTerm}", request.SearchTerm);
            return Result<IEnumerable<FloorballTeamDto>>.Failure("An error occurred while searching for floorball teams.");
        }
    }
} 