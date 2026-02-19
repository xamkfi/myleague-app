using Application.Features.Floorball.Matches.Queries;
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
using Domain.Repositories.Floorball;
using Microsoft.Extensions.Logging;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Floorball.Matches.Handlers;

/// <summary>
/// Handler for retrieving floorball matches by season
/// </summary>
public class GetFloorballMatchesBySeasonHandler : IRequestHandler<GetFloorballMatchesBySeasonQuery, Result<IEnumerable<FloorballMatchDto>>>
{
    private readonly IFloorballMatchRepository _matchRepository;
    private readonly ILogger<GetFloorballMatchesBySeasonHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the GetFloorballMatchesBySeasonHandler class
    /// </summary>
    /// <param name="matchRepository">The floorball match repository</param>
    /// <param name="logger">The logger</param>
    public GetFloorballMatchesBySeasonHandler(
        IFloorballMatchRepository matchRepository,
        ILogger<GetFloorballMatchesBySeasonHandler> logger)
    {
        _matchRepository = matchRepository;
        _logger = logger;
    }

    /// <summary>
    /// Handles the GetFloorballMatchesBySeasonQuery request
    /// </summary>
    /// <param name="request">The query containing season ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Floorball matches by season as DTOs wrapped in a Result</returns>
    public async Task<Result<IEnumerable<FloorballMatchDto>>> Handle(GetFloorballMatchesBySeasonQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Retrieving floorball matches for season: {SeasonId}", request.SeasonId);
            
            IEnumerable<FloorballMatch> matches = await _matchRepository.GetBySeasonIdAsync(request.SeasonId);
            IEnumerable<FloorballMatchDto> matchDtos = FloorballMatchMapper.ToDtos(matches);
            
            _logger.LogInformation("Successfully retrieved {MatchCount} floorball matches for season: {SeasonId}", matchDtos.Count(), request.SeasonId);
            
            return Result<IEnumerable<FloorballMatchDto>>.Success(matchDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving floorball matches for season: {SeasonId}", request.SeasonId);
            return Result<IEnumerable<FloorballMatchDto>>.Failure("An error occurred while retrieving floorball matches.");
        }
    }
} 
